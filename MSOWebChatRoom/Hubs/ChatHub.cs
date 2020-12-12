using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Mvc;
using MSOWebChatRoom.Data;
using MSOWebChatRoom.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using MSOWebChatRoom.Utilities;

namespace MSOWebChatRoom.Hubs
{
    public class ChatHub : Hub
    {
        private readonly UserManager<ChatUser> _userManager;
        private readonly ApplicationDbContext _context;
        
        // to Store connectionId and username of the new connected Client (use Thread-safe) 
        private static readonly ConcurrentDictionary<string, UserConnection> Connections
        = new ConcurrentDictionary<string, UserConnection>(); 

        public ChatHub(ApplicationDbContext context, UserManager<ChatUser> userManager )
        {
            _context = context;
            _userManager = userManager;
        }


        /// <summary>
        /// Send message to a private user (Brodcast from the connected client)
        /// </summary>
        /// <param name="user"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task SendPrivateMessage(string user, string message)
        {
            if(!string.IsNullOrEmpty(user) && !string.IsNullOrEmpty(message))
            {
                try
                {
                    var currentUserConnectionId = Context.ConnectionId;

                    var receiverdetails = _context.Users.Where(u => u.UserName == user).FirstOrDefault();
                    if (receiverdetails == null)
                        await Clients.Caller.SendAsync("HubError", new { error = "Receiver not found!" });

                    var currentUser = await _userManager.GetUserAsync(Context.User); // get current connected user

                    // get connections of the receiver user
                    UserConnection receiverConnects = new UserConnection();
                    Connections.TryGetValue(user, out receiverConnects);

                    // add the new message to DB - receiver: user(receiverId)
                    ChatMessage newmessage = new ChatMessage()
                    {
                        MessageBody = message,
                        MessageDate = DateTime.Now,
                        SenderId = currentUser.Id,
                        ReceiverId = receiverdetails.Id
                    };

                    _context.Messages.Add(newmessage);
                    await _context.SaveChangesAsync();

                    
                    if (receiverConnects != null)
                    {
                        //newmessage.Status = ChatMessage.MessageStatus.read;
                        //_context.Messages.Update(newmessage);
                        //await _context.SaveChangesAsync();

                        // brodcast the message  to receiver user if online
                        await Clients.Clients(receiverConnects.ConnectionIds.ToArray()).SendAsync("ReceiveMessage",
                        new
                        {
                            messageBody = newmessage.MessageBody,
                            messageDate = newmessage.MessageDate.ToString("hh:mm tt MMM dd", CultureInfo.InvariantCulture),
                            status = newmessage.Status,
                            senderName = currentUser.UserName,
                            receiverName= receiverdetails.UserName
                        });
                    }
                    // brodcast the message to sender user 
                    await Clients.Client(currentUserConnectionId).SendAsync("ReceiveMessage",
                        new
                        {
                            messageId = newmessage.MessageId,
                            messageBody = newmessage.MessageBody,
                            status=newmessage.Status,
                            messageDate = newmessage.MessageDate.ToString("hh:mm tt MMM dd", CultureInfo.InvariantCulture),
                            senderName = currentUser.UserName,
                            receiverName = receiverdetails.UserName
                        });


                }
                catch (Exception ex)
                {
                    await Clients.Caller.SendAsync("HubError", new { error = ex.Message });
                }
            }
            else
                await Clients.Caller.SendAsync("HubError", new { error = "Could not find that user/message" });
        }

        /// <summary>
        /// Add connections users to a specific group
        /// </summary>
        /// <param name="groupName">The group's Name</param>
        /// <returns></returns>
        public  async Task AddToGroup(string groupName)
        {
            // verif group in DB
            var groupdetails = _context.Groups.Where(g => g.GroupName == groupName.Trim()).FirstOrDefault();
            if (groupdetails == null)
                await Clients.Caller.SendAsync("HubError", new { error = "Group not found!" });

            List<string> names = ChatLookup.GetUsersGroupListByGroupId(_context, groupdetails.GroupID);

            foreach(string Uname in names)
            {
                // get connections of the users group
                UserConnection usergrpConnects = new UserConnection();
                Connections.TryGetValue(Uname, out usergrpConnects);
                if (usergrpConnects != null)
                {
                    foreach (var con in usergrpConnects.ConnectionIds)
                    {
                        await Groups.AddToGroupAsync(con, groupName);
                    }
                }
            }
        }

        /// <summary>
        /// send message for a specific group
        /// </summary>
        /// <param name="groupName"> The group Name</param>
        /// <param name="message"> The content of the message </param>
        /// <returns></returns>
        public async Task SendMessageToGroup(string groupName, string message)
        {
            if (!string.IsNullOrEmpty(groupName) && !string.IsNullOrEmpty(message))
            {
                try
                {
                    var currentUserConnectionId = Context.UserIdentifier;

                    var groupdetails = _context.Groups.Where(g => g.GroupName == groupName.Trim()).FirstOrDefault();
                    if (groupdetails == null)
                        await Clients.Caller.SendAsync("HubError", new { error = "Group not found!" });

                    var currentUser = await _userManager.GetUserAsync(Context.User); // get current connected user

                    // Add new message(sent to group) to DB - receiver:group (groupId)
                    ChatMessage newmessage = new ChatMessage()
                    {
                        MessageBody = message,
                        MessageDate = DateTime.Now,
                        SenderId = currentUser.Id,
                        GroupeId = groupdetails.GroupID
                    };

                    // add the new message to DB
                    _context.Messages.Add(newmessage);
                    await _context.SaveChangesAsync();

                    await Clients.Group(groupName).SendAsync("ReceiveGroupMessage",
                     new
                     {
                         messageId=newmessage.MessageId,
                         messageBody = newmessage.MessageBody,
                         messageDate = newmessage.MessageDate.ToString("hh:mm tt MMM dd", CultureInfo.InvariantCulture),
                         senderName = currentUser.UserName,
                         status = newmessage.Status,
                         groupName,
                         receiverName = groupName
                     });
                }
                catch (Exception ex)
                {
                    await Clients.Caller.SendAsync("HubError", new { error = ex.Message });
                }
             
            }
            else

               await Clients.Caller.SendAsync("HubError", new { error = "Could not find that Group/Message" });
        }

        /// <summary>
        /// Detect the connected Users an send it to all Clients
        /// </summary>
        /// <returns></returns>
        public override async Task OnConnectedAsync()
        {
            try
            {
                string newconnected = Context.User.Identity.Name;
                string connectionId = Context.ConnectionId;

                var user = Connections.GetOrAdd(newconnected, _ => new UserConnection
                {
                    Name = newconnected,
                    ConnectionIds = new HashSet<string>()
                });

                lock (user.ConnectionIds)
                {
                    user.ConnectionIds.Add(connectionId);
                    Clients.All.SendAsync("UserConnected",
                  new
                  {
                      usersconnected = Connections.Values,
                      connectionName= newconnected,
                      connectionId = Context.ConnectionId,
                      connectionDate = DateTime.Now,
                      messageDate = DateTime.Now.ToString("hh:mm tt MMM dd", CultureInfo.InvariantCulture) // simple friendly message when connect on
                    });
                }

                await base.OnConnectedAsync();

            }
            catch(Exception ex)
            {
                await Clients.User(Context.ConnectionId).SendAsync("HubError", new { error = ex.Message });
            }
                
        }

        public override async Task OnDisconnectedAsync(Exception ex)
        {
            string userName = Context.User.Identity.Name;
            string connectionId = Context.ConnectionId;

            Connections.TryGetValue(userName, out UserConnection user);

            if (user != null)
            {
                lock (user.ConnectionIds)
                {
                    user.ConnectionIds.RemoveWhere(cid => cid.Equals(connectionId));
                    if (!user.ConnectionIds.Any())
                    {
                        Connections.TryRemove(userName, out UserConnection removedUser);
                        Clients.Clients(user.ConnectionIds.ToArray()).SendAsync("UserDeconnected", new
                        {
                            usersconnected = Connections.Values,
                        });
                    }
                }
            }

            await base.OnDisconnectedAsync(ex);
        }


        public UserConnection GetUsersConnections(String username)
        {
            // get connections of un user
            _ = new UserConnection();
            Connections.TryGetValue(username, out UserConnection receiverConnects);
            return (receiverConnects);
         }


    }
}
