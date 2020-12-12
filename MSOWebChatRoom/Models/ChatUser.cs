/***
 *  ClassName: ChatUser.cs -  inherit from Identity user 
 *  
 *  Author: Arbia Ben Azaiez - arbiaa.azaiez@gmail.com
 */
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MSOWebChatRoom.Models
{
    public class ChatUser : IdentityUser
    {

        public ChatUser()
        {
            MessagesS = new HashSet<ChatMessage>();
            Cgroups = new HashSet<ChatUserGroupLink>();
        }
        // 1-* ChatUser(receiver) || ChatMessage
        public virtual ICollection<ChatMessage> MessagesR { get; set; }
        // 1-* ChatUser(sender) || ChatMessage
        public virtual ICollection<ChatMessage> MessagesS { get; set; }
        // 1-* ChatUser || ChatGroup
        public virtual ICollection<ChatUserGroupLink> Cgroups { get; set; }
    }
}
