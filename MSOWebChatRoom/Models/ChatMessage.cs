/***
 *  ClassName: ChatMessage.cs -  represent  the ChatMessage Object : store all the messages of the users
 *  
 *  Author: Arbia Ben Azaiez - arbiaa.azaiez@gmail.com
 */
using System;
using System.ComponentModel.DataAnnotations;

namespace MSOWebChatRoom.Models
{
    public class ChatMessage
    {

        public ChatMessage()
        {
            Status = MessageStatus.Unread;
        }

        public enum MessageStatus
        {
            
            Unread,
            read
        }

        [Key]
        /* The message ID */
        public int MessageId { get; set; }
        /* The content of a message */
        public string MessageBody { get; set; }
        /* The timestamp of a message: when the message was sent */
        public DateTime MessageDate { get; set; }
        /* The status of the message*/
        public MessageStatus Status { get; set; }
        /* The date when the message's is read*/
        public DateTime StatusDate { get; set; }
        public string ReceiverId { get; set; }
        public int? GroupeId { get; set; }

        public string SenderId { get; set; }
        public virtual ChatUser UserSender { get; set; }
        public virtual ChatUser UserReceiver { get; set; }
        public virtual ChatGroup Group { get; set;}

    }
}
