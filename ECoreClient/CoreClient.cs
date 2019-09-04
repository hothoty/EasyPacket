using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ECore;

namespace ECoreClient
{
    public class CoreClient : ZNetCore
    {
        // 서버에서 할당받은 나의 remoteID
        public RemoteID remoteID { get; private set; }


        protected override bool RecvInternalMessage(RemoteID remote, PackInternal pkID, CMessage msgData, CPackOption op)
        {
            switch (pkID)
            {
                case PackInternal.ePID_SC_Remote:
                    {
                        RemoteID _remote;
                        Marshaler.Read(msgData, out _remote);
                        remoteID = _remote;
                        OnReadyClient();
                    }
                    break;

                default:
                    {
                        // test
                    }
                    return false;
            }
            return true;
        }

        internal virtual void OnReadyClient() {}
    }
}
