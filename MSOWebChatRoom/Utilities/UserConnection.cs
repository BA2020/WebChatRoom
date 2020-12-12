/**
 * Class Name: UserConnection.cs : used as utility to get the connected users and their differents connections ID to the chat room
 * 
 * Author: Arbia Ben Azaiez - arbia.azaiez@gmail.com
 *
 **/
using System.Collections.Generic;

namespace MSOWebChatRoom.Utilities
{
    public class UserConnection
    {
        /* The user's name */
        public string Name { get; set; }
        /* The user's connection(s) to the chat */
        public HashSet<string> ConnectionIds { get; set; }
    }
}
