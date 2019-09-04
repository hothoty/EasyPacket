using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ECore
{
    public class IHandler
    {
        public enum MsgType
        {
            Debug,
            Info,
            Warning,
            Error,
            Critical,
        }

        public delegate void MessageDelegate(MsgType t, string msg);
        public MessageDelegate message_handler { get; set; }
    }
}
