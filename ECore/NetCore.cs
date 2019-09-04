using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// 
/// 서버/클라이언트 공용 Core 클래스
/// 
/// </summary>
namespace ECore
{
    public class CoreParam
    {
        public const Int32 HEADER_SIZE_LEN = 4;     // 패킷크기 사이즈 (바디를 제외한 순수 헤더)
        public const Int32 PACKET_MAX_SIZE = 4096;
    }

    public class ZNetCore : IHandler
    {
        protected List<PKStub> stub_list = new List<PKStub>();


        public void Attach(PKProxy proxy, PKStub stub)
        {
            proxy.SetOwner(this);
            stub.SetOwner(this);
            stub_list.Add(stub);
        }

        public void AttachProxy(PKProxy proxy)
        {
            proxy.SetOwner(this);
        }

        public void AttachStub(PKStub stub)
        {
            stub.SetOwner(this);
            stub_list.Add(stub);
        }

        protected void PreparePacket(RemoteID remote, RemoteID from, CMessage msg, CPackOption op)
        {
            if (op.m_pack_mode == PacketMode8.PM_Encrypt_Mode)
            {
                msg.Encrypt();
            }

            // remote target처리
            Int32 remote_target = (Int32)remote;
            byte[] temp_remote = BitConverter.GetBytes(remote_target);
            temp_remote.CopyTo(msg.buffer, 4);      // CPackOption의 순서에 따라 정확한 위치에 덮어써준다

            // remote from
            Int32 remote_from = (Int32)from;
            temp_remote = BitConverter.GetBytes(remote_from);
            temp_remote.CopyTo(msg.buffer, 8);     // CPackOption의 순서에 따라 정확한 위치에 덮어써준다
        }

        // 내부 패킷 + 사용자 패킷을 구분하여 처리
        protected string OnPacket(CRecvedMsg recved_msg)
        {
            if (recved_msg.pkop.m_pack_mode == PacketMode8.PM_None)
            {
            }
            else if (recved_msg.pkop.m_pack_mode == PacketMode8.PM_Encrypt_Mode)
            {
                try
                {
                    recved_msg.msg.Decrypt();
                }
                catch (Exception e)
                {
                    return "MsgDecryptException: " + e.Message;
                }
            }
            else
            {
                if (this.message_handler != null)
                    this.message_handler(MsgType.Warning, string.Format("packmode warning {0}", recved_msg.pkop.m_pack_mode));
            }

            if (recved_msg.pkID == PacketType.PacketType_Internal)
            {
                try
                {
                    PackInternal internel_id;
                    recved_msg.msg.Read(out internel_id);
                    if (!RecvInternalMessage(recved_msg.remote, internel_id, recved_msg.msg, recved_msg.pkop))
                    {
                        if (this.message_handler != null)
                            this.message_handler(MsgType.Warning, string.Format("RecvInternal need implement : pkid : {0}", internel_id));
                    }
                }
                catch (Exception e)
                {
                    return string.Format("RecvInternal ErrorException : {0}", e.Message);
                }
                return string.Empty;
            }

            ECore.PacketType runningPkID = PacketType.PacketType_None;
            int nReceive = 0;
            try
            {
                foreach (var obj in this.stub_list)
                {
                    runningPkID = recved_msg.pkID;
                    if (obj.ProcessMsg(recved_msg))
                    {
                        nReceive++;
                    }
                    if (nReceive >= 2)
                    {
                        if (this.message_handler != null)
                            this.message_handler(MsgType.Warning, string.Format("ProcessMsg duplicate warning msgID {0}  callCnt {1}", recved_msg.pkID, nReceive));
                    }
                }
            }
            catch (Exception e)
            {
                return string.Format("StubException Rmi.Common.ID:{0}, {1}", (int)runningPkID, e.Message);
            }
            if (nReceive == 0)
            {
                if (this.message_handler != null)
                    this.message_handler(MsgType.Warning, string.Format("ProcessMsg warning msgID {0}  call zero", recved_msg.pkID));
            }
            return string.Empty;
        }

        public void OnMessage(string str)
        {
            if (this.message_handler != null)
                this.message_handler(MsgType.Warning, string.Format("rmi message: {0}", str));
        }


        protected virtual bool RecvInternalMessage(RemoteID remote, PackInternal pkID, CMessage msgData, CPackOption op) { return false; }

        public virtual bool Send(RemoteID remote, CMessage msg, CPackOption op) { return false; }
    }
}
