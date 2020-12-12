/**
 * View model class: ChatViewModel.cs : to send the approriate data to the correspondant view
 * 
 * Author: Arbia Ben Azaiez - arbia.azaiez@gmail.com
 *
 **/
using MSOWebChatRoom.Utilities;
using System.Collections.Generic;

namespace MSOWebChatRoom.Models
{
    public class ChatViewModel
    {
        public List<ChatUser> UsersList { get; set; }
        public List <ChatGroup> GroupsList { get; set; }
        public ChatGroup NewGroup { get; set; }
        public List<UserGroup> UsersGroup { get; set; }
    }
}
