using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using MSOWebChatRoom.Data;
using MSOWebChatRoom.Hubs;
using MSOWebChatRoom.Models;
using MSOWebChatRoom.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace MSOWebChatRoom.Controllers
{
    [Authorize]  // only authorized users can access to chat page
    public class ChatController : Controller
    {
        //Dependency injections
        private readonly UserManager<ChatUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<ChatHub> _hubContext;

        public ChatController(ApplicationDbContext context, UserManager<ChatUser> userManager, IHubContext<ChatHub> hubContext)
        {
            _context = context;
            _userManager = userManager;
            _hubContext = hubContext;
        }

        /// <summary>
        /// Display Chat index 
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Index()
        {
            // Get the connected user
            var currentUser = await _userManager.GetUserAsync(User);
            ChatUser userdetails = (from chat in await ChatLookup.GetAllUsers(_context)
                                    where (chat.UserName == currentUser.UserName)
                                    select chat).FirstOrDefault();
                                    
            ChatViewModel model = new ChatViewModel
            {
                UsersList = new List<ChatUser>(),
                GroupsList = new List<ChatGroup>()
            };

            if (userdetails != null)
            {
                // Get all users except the current one
                model.UsersList = (from chat in await ChatLookup.GetAllUsers(_context)
                                   where (chat.UserName != currentUser.UserName)
                                   select chat).ToList();


                // Get all the groups which the connected user is on
                var groups = await ChatLookup.GetAllGroupsByUser(_context,currentUser.Id);
                model.GroupsList = groups;

                model.UsersGroup = (from ul in model.UsersList
                                   select new UserGroup
                                   {
                                       UserId = ul.Id,
                                       Username=ul.UserName
                                   }).ToList();

                return View(model);
            }
            else
                return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateGroup(ChatGroup group,List<UserGroup> usersgroup)
        {
            if (ModelState.IsValid)
            {
                var currentUser = await _userManager.GetUserAsync(User);
                ChatHub _connectedHub = new ChatHub(_context, _userManager);

                // create new group in DB & get its ID
                (bool result,int group_id) = await ChatLookup.AddNewChatGroup(_context, group, usersgroup);

                if (!result)
                    return BadRequest();

                // Get connections of the group's users 
                foreach (UserGroup ug in usersgroup)
                {
                    if(ug.Is_selected)
                    {
                        string name = ug.Username;
                        //get connectionsId of each user of the  group
                        UserConnection connections = _connectedHub.GetUsersConnections(name);
                        if (connections != null)
                        {
                            foreach (var con in connections.ConnectionIds)
                            {
                                // Add each user's connection to the new created group
                                await _hubContext.Groups.AddToGroupAsync(con, group.GroupName);
                            }
                        }
                    }
                }

                // send notif for the connected users to inform them by the new group
                await _hubContext.Clients.Group(group.GroupName).SendAsync("NotifCreateGroup",
                     new
                     {
                         createDate = DateTime.Now.ToString("hh:mm tt MMM dd", CultureInfo.InvariantCulture),
                         createBy = currentUser.UserName,
                         groupName = group.GroupName,
                         groupId=group_id
                     });

                

                return RedirectToAction("Index","Chat");
            }
            else
            return RedirectToAction("Index");
        }


        /// <summary>
        /// Get all messages with specifc user
        /// </summary>
        /// <param name="contact"> The contact's name</param>
        /// <returns>List </returns>
        [HttpGet]
        public JsonResult ConversationWithContact(string contact)
        {
            var currentUser = _userManager.GetUserId(User);
            var receiverUser = _context.Users.Where(u => u.UserName == contact).FirstOrDefault();

            if (receiverUser != null)
            {
                //// update status of received message from the other user
                //var last_status_msg = (from m in _context.Messages
                //                 where m.SenderId == receiverUser.Id && m.ReceiverId == currentUser
                //                 orderby m.MessageDate descending
                //                 select m.Status).FirstOrDefault();

                //// verify if the user had read the last message : if not, we update the status of the message(s)
                //if(last_status_msg == ChatMessage.MessageStatus.Unread)
                //{
                //    (from m in _context.Messages
                //     where m.SenderId == receiverUser.Id && m.ReceiverId == currentUser
                //     select m).ToList().ForEach(x => x.Status = ChatMessage.MessageStatus.read);
                //    _context.SaveChanges();
                //}

                // Get all messages between current user and the receiver
                List<UserMessage> conversations =  ChatLookup.GetMessagesOfPrivateConversation(_context, currentUser, receiverUser.Id);

                return Json(conversations) ;
            }
            else
                return Json("failed"); 
        }

        /// <summary>
        /// Get all messages with specific group
        /// </summary>
        /// <param name="contact"> The group's name</param>
        /// <returns></returns>
        [HttpGet]
        public JsonResult ConversationWithGroup(string contact)
        {
            if (!string.IsNullOrEmpty(contact))
            {
                var currentUser = _userManager.GetUserId(User);
                var groupdetails = _context.Groups.Where(g => g.GroupName == contact.Trim()).FirstOrDefault();
                if (groupdetails == null)
                    return Json("failed");

                if (groupdetails != null)
                {
                    // Get all messages between current user and the group
                    List<UserMessage> conversations = ChatLookup.GetMessagesOfGroupConversation(_context, groupdetails.GroupID);
                    return Json(conversations);
                }
                else
                    return Json("failed");
            }
            else
                return Json("failed");
        }


        [HttpGet]
        public JsonResult GroupDetailsUsers(string group)
        {
            if (!string.IsNullOrEmpty(group))
            {
                int group_id = _context.Groups.Where(g => g.GroupName == group.Trim()).FirstOrDefault().GroupID;
                List<string> groupdetails = ChatLookup.GetUsersGroupListByGroupId(_context, group_id);
                if (groupdetails == null)
                    return Json("failed");

                return Json(groupdetails);
            }
            else
                return Json("failed");
        }

    }
}
