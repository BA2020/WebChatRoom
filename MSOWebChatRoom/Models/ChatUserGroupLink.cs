/***
 *  ClassName: ChatUserGroupLink.cs -  represent  the ChatUserGroupLink Object : store the users and their associated groups
 *  
 *  Author: Arbia Ben Azaiez - arbiaa.azaiez@gmail.com
 */


using System.ComponentModel.DataAnnotations;

namespace MSOWebChatRoom.Models
{
    public class ChatUserGroupLink
    {
        [Key]
        /* The user ID */
        public string userId { get; set; }
        [Key]
        /* The group ID */
        public int groupeId { get; set; }

        public virtual ChatUser User { get; set; }
        public virtual ChatGroup Group { get; set; }
    }
}
