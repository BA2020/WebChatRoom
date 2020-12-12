/**
 * Class UserGroup.cs : used as utility to store the selected users added to a new group
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
    public class UserGroup
    {
        public string UserId { get; set; }
        public string Username { get; set; }
        public bool Is_selected { get; set; } = false;
    }
}
