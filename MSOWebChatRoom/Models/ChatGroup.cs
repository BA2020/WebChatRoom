/***
 *  ClassName: ChatGroup.cs -  represent  the ChatGroup Object : store all the chat groups created
 *  
 *  Author: Arbia Ben Azaiez - arbiaa.azaiez@gmail.com
 */
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MSOWebChatRoom.Models
{
    public class ChatGroup
    {

        public ChatGroup()
        {
            Cusers = new HashSet<ChatUserGroupLink>();
            Messages = new HashSet<ChatMessage>();
        }

        [Key]
        /* The group ID */
        public int GroupID { get; set;}
        [Required]
        /* The group name */
        public string GroupName { get; set; }
        /* The user created the group */
        public string CreatedBy { get; set; }
        /* The date creation of the groupe */
        public DateTime CreatedOn { get; set; }


        public virtual ICollection<ChatUserGroupLink> Cusers { get; set; }
        public virtual ICollection<ChatMessage> Messages { get; set; }
    }
}
