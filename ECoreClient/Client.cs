using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace ECoreClient
{
    internal class CUserClient
    {
        public Socket socket { get; set; }

        CoreClient client_ptr;

        public CUserClient(CoreClient p)
        {
            this.client_ptr = p;
        }

        internal bool DoSend(ECore.CPacket msg)
        {
            msg.record_size();

            // 동기 전송
            if (this.socket.Connected == false)
                return false;

            int ret;
            try
            {
                ret = this.socket.Send(msg.buffer, msg.position, 0);
            }
            catch (System.Net.Sockets.SocketException e)
            {
                if (client_ptr.message_handler != null)
                    client_ptr.message_handler(CoreClient.MsgType.Warning, string.Format("send error code {0}", e.ErrorCode));
                ret = -1;
            }
            if (ret < 0)
            {
                // 접속끊김,전송실패
                if (client_ptr.message_handler != null)
                    client_ptr.message_handler(CoreClient.MsgType.Warning, string.Format("Send fail ret {0}", ret));
                return false;
            }
            return true;
        }

        // 해제처리
        public void disconnect()
        {
            // close the socket associated with the client
            try
            {
                this.socket.Shutdown(SocketShutdown.Send);
            }
            // throws if client process has already closed
            catch (Exception) { }
            this.socket.Close();
        }
    }
}
