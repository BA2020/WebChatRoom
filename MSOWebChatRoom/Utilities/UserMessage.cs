/**
 * Class Name: UserMessage.cs : used as utility to display the user's messages 
 * 
 * Author: Arbia Ben Azaiez - arbia.azaiez@gmail.com
 *
 **/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MSOWebChatRoom.Utilities
{
    public class UserMessage
    {
        /* The message ID */
        public int MessageId { get; set; }
        /* The message Content */
        public string MessageBody { get; set; }
        /* The timestamp of a message: when the message was sent */
        public string MessageDate { get; set; }
        /* The status of the message: read - unread by the receiver */
        public string Status { get; set; }
        /* The message's sender user*/
        public string SenderName { get; set; }
        /* The message's receiver user */
        public string ReceiverName { get; set; }
        /* The message's receiver group */
        public string GroupeName { get; set; }
    }
}
