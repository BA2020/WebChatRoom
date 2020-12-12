/**
 * Class ChatLookup.cs : used as utility to manage all queries
 * 
 * Author: Arbia Ben Azaiez - arbia.azaiez@gmail.com
 *
 **/
using Microsoft.EntityFrameworkCore;
using MSOWebChatRoom.Data;
using MSOWebChatRoom.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace MSOWebChatRoom.Utilities
{
    public static class ChatLookup
    {

        /// <summary>
        /// Get All the users in Chat DB
        /// </summary>
        /// <param name="_context"> the DB context</param>
        /// <returns>List of users </returns>
        public async static Task<List<ChatUser>> GetAllUsers (ApplicationDbContext _context)
        {
            return await _context.Users.AsNoTracking().ToListAsync();

        }
        /// <summary>
        /// Get all groups in DB
        /// </summary>
        /// <param name="_context"> the DB context</param>
        /// <param name="_context"></param>
        /// <returns></returns>
        public async static Task<List<ChatGroup>> GetAllGroups(ApplicationDbContext _context)
        {
            return await _context.Groups.AsNoTracking().ToListAsync();
        }

        /// <summary>
        /// Get group list of a specific user (connected user)
        /// </summary>
        /// <param name="_context"> the DB context</param>
        /// <param name="user_id"> The ID of the current user</param>
        /// <returns>List of groups</returns>
        public async static Task<List<ChatGroup>> GetAllGroupsByUser(ApplicationDbContext _context, string user_id)
        {
            return await _context.UsersGroups
                           .Where(gp => gp.userId == user_id)
                           .Join(_context.Groups, ug => ug.groupeId, g => g.GroupID, (ug, g) =>
                                   new ChatGroup
                                   {
                                       GroupID = g.GroupID,
                                       GroupName = g.GroupName
                                   }).AsNoTracking().ToListAsync();

        }
        /// <summary>
        /// Create new chat group
        /// </summary>
        /// <param name="_context"> the DB context</param>
        /// <param name="grp"> the group object to add </param>
        /// <param name="users">the group's users list </param>
        /// <returns> True if group created Or False in error case</returns>
        public async static Task<(bool,int)> AddNewChatGroup(ApplicationDbContext _context,ChatGroup grp, List<UserGroup> users)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            ChatUser current_user = (from u in await GetAllUsers(_context)
                                  where (u.UserName == grp.CreatedBy)
                                  select u).FirstOrDefault();
            if (current_user != null)
            {
                try
                {
                    if (string.IsNullOrEmpty(grp.GroupName))
                        return (false,0);

                    ChatGroup newgroup = new ChatGroup()
                    {
                        GroupName = grp.GroupName,
                        CreatedBy = grp.CreatedBy,
                        CreatedOn = DateTime.UtcNow
                    };

                    _context.Groups.Add(newgroup);
                   await  _context.SaveChangesAsync();

                    // Ad the current user to the new Group
                    ChatUserGroupLink usergrouplink = new ChatUserGroupLink()
                    {
                        groupeId = newgroup.GroupID,
                        userId = current_user.Id
                    };

                    _context.UsersGroups.Add(usergrouplink);

                    foreach (UserGroup ug in users)
                    {
                        ChatUser userToadd = (from u in await GetAllUsers(_context)
                                              where (u.UserName == ug.Username)
                                              select u).FirstOrDefault();
                        // verify if the user to add exist in DB
                        if (userToadd != null)
                        {
                            // Added the selected users to add
                            if (ug.Is_selected)
                            {
                                usergrouplink = new ChatUserGroupLink()
                                {
                                    groupeId = newgroup.GroupID,
                                    userId = userToadd.Id
                                };

                                _context.UsersGroups.Add(usergrouplink);
                            }
                        }
                        else
                            return (false, 0); // user not found
                    }

                    await _context.SaveChangesAsync();

                    // Commit transaction if all commands succeed, transaction will auto-rollback
                    // when disposed if either commands fails
                    await transaction.CommitAsync();

                    return (true, newgroup.GroupID);
                }
                catch (Exception)
                {
                    // in failure, transaction rollback
                    await transaction.RollbackAsync();
                    return (false, 0);
                }
            }
            else
                return (false, 0);
        }


        /// <summary>
        /// Get list of users for a given group
        /// </summary>
        /// <param name="_context">the DB context</param>
        /// <param name="group_id">The group Id </param>
        /// <returns> List of string of username's </returns>
        public  static List<string> GetUsersGroupListByGroupId(ApplicationDbContext _context, int group_id)
        {
            return  _context.UsersGroups
                           .Where(gp => gp.groupeId == group_id)
                           .Join(_context.Users, g => g.userId, u=> u.Id, (g, u) =>
                                   u.UserName).AsNoTracking().ToList();
        }
        /// <summary>
        /// Get messages of a private conversation which receiver is a specific user
        /// </summary>
        /// <param name="_context">The DB context</param>
        /// <param name="sender_id"> The sender ID</param>
        /// <param name="receiver_id"> The receiver ID</param>
        /// <returns>List of UserMessage</returns>
        public  static List<UserMessage> GetMessagesOfPrivateConversation (ApplicationDbContext _context,string sender_id, string receiver_id)
        {
            return  (from m in _context.Messages
                    join us in _context.Users on m.SenderId equals us.Id
                    join ur in _context.Users on m.ReceiverId equals ur.Id
                    where ((m.SenderId == sender_id && m.ReceiverId == receiver_id) ||
                          (m.ReceiverId == sender_id && m.SenderId == receiver_id))
                    orderby m.MessageDate
                    select new UserMessage
                    {
                        MessageId = m.MessageId,
                        MessageBody = m.MessageBody,
                        MessageDate = m.MessageDate.ToString("hh:mm tt MMM dd", CultureInfo.InvariantCulture),
                        Status = m.Status.ToString(),
                        SenderName = us.UserName,
                        ReceiverName = ur.UserName
                    }).AsNoTracking().ToList();
        }

        /// <summary>
        /// Get the messages of a group conversation
        /// </summary>
        /// <param name="_context">The DB context </param>
        /// <param name="group_id"> The group id which is the receiver </param>
        /// <returns>List of UserMessage</returns>
        public static List<UserMessage> GetMessagesOfGroupConversation(ApplicationDbContext _context, int group_id)
        {
            return  (from m in _context.Messages
                          join us in _context.Users on m.SenderId equals us.Id
                          join g in _context.Groups on m.GroupeId equals g.GroupID
                          where (m.GroupeId == group_id ) 
                          orderby m.MessageDate
                          select new UserMessage
                          {
                              MessageId = m.MessageId,
                              MessageBody = m.MessageBody,
                              MessageDate = m.MessageDate.ToString("hh:mm tt MMM dd", CultureInfo.InvariantCulture),
                              Status = m.Status.ToString(),
                              SenderName = us.UserName,
                              ReceiverName = g.GroupName 
                          }).AsNoTracking().ToList();
        }
    }
}
