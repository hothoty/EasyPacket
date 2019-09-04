using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace ECore
{
    public class PKProxy
    {
        protected ZNetCore owner;

        public PKProxy()
        {
            this.owner = null;
        }

        public bool PacketSend(RemoteID remote, CPackOption pko, CMessage sMsg)
        {
            if (this.owner == null)
                return false;

            return owner.Send(remote, sMsg, pko);
        }

        public void SetOwner(ZNetCore ow)
        {
            this.owner = ow;
        }
    }

    public class PKStub
    {
        ZNetCore owner;

        public PKStub()
        {
            this.owner = null;
        }

        public void SetOwner(ZNetCore ow)
        {
            this.owner = ow;
        }

        public void NeedImplement(string str)
        {
            if (this.owner == null)
                return;

            this.owner.OnMessage(str);
        }

        virtual public bool ProcessMsg(CRecvedMsg rm)
        {
            return false;
        }
    }
}
