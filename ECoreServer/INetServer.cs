using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ECore;

namespace ECoreServer
{
    /// <summary>
    /// 
    /// 클라이언트 처리 함수
    /// 
    /// - OnConnect()
    /// 
    /// - OnLeave()
    /// 
    /// - OnRecv()
    /// 
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class INetServer<T> : ZNetCore
    {
        object sync_obj;
        Dictionary<RemoteID, IRemoteClient> RemoteClients;


        /// <summary>
        /// 
        /// 서버가 멀티 스레드일때 (싱글 스레드 서버는 해당사항 없음, 무시해도 됨)
        /// 
        /// 최초 클라이언트가 접속시 Connect와 Recv스레드가 달라서
        /// 
        /// Connect가 먼저 실행되야 하는데 Recv가 먼저 실행되는 문제를 방지하기 위해 Recv실행에 가져갈 딜레이 최대값.
        /// 
        ///   기본적으로 100밀리 세컨드 단위로 기다림.
        /// 
        /// --->  이 값이 20일경우 100 * 20 즉 2000밀리세컨드 -> 최대 2초까지 기다려준다
        /// 
        ///       필요시 값을 조정하면 즉시 반영됨
        /// 
        /// </summary>
        public int WaitDelayRecvThreadCount = 20;


        public INetServer(int maxConnection)
        {
            RemotePool.CreateRemotePool(maxConnection);

            sync_obj = new object();
            RemoteClients = new Dictionary<RemoteID, IRemoteClient>();
        }

        /// <summary>
        /// 클라이언트가 Connect됬을때 가장 먼저 호출해줘야 함
        /// </summary>
        /// <param name="session"></param>
        /// <returns></returns>
        public RemoteID OnConnect(ref T session)
        {
            RemoteID remote = RemotePool.GetRemotePool();
            if (remote == RemoteID.Remote_None)
            {
                if (message_handler != null)
                    message_handler(MsgType.Critical, "remotePool Empty!");
            }

            lock (sync_obj)
            {
                if (RemoteClients.ContainsKey(remote))
                {
                    if (message_handler != null)
                        message_handler(MsgType.Warning,
                            "remote key 중복되어 반복루프시작함... (멀티스레드 서버에서 close된상태에서 남은recv처리가 존재할경우 발생될수있음)");
                    while (true)
                    {
                        RemotePool.FreeRemotePool(remote);      // 기존 중복된것을 사용안했으니 바로 반환한다음..
                        remote = RemotePool.GetRemotePool();    // 다시 하나 가져오고..
                        if (RemoteClients.ContainsKey(remote) == false)
                            break;
                    }
                }
                RemoteClients.Add(remote, (IRemoteClient)session);
            }

            // 내부패킷 보내기
            CMessage sMsg = new CMessage();
            CPackOption sendOption = CPackOption.Basic;
            sMsg.WriteStart(PacketType.PacketType_Internal, sendOption);
            Marshaler.Write(sMsg, PackInternal.ePID_SC_Remote);
            Marshaler.Write(sMsg, remote);
            SendToClient(remote, (IRemoteClient)session, sMsg, sendOption);   // remote를 별도로 보내는 이유는 이함수가 리턴되야 session의 remote가 갱신되므로..

            return remote;
        }

        public void OnLeave(T remoteClient)
        {
            IRemoteClient client = (IRemoteClient)remoteClient;
            lock (sync_obj)
            {
                RemoteClients.Remove(client.GetRemoteID());
            }
            RemotePool.FreeRemotePool(client.GetRemoteID());
        }

        public void OnRecv(T remoteClient, byte[] buffer, int len)
        {
            IRemoteClient client = (IRemoteClient)remoteClient;
            if (client.GetRemoteID() == RemoteID.Remote_None)
            {
                // Connect스레드가 다를경우 조금 기다려주도록 하자
                for (int i = 0; i < WaitDelayRecvThreadCount; i++)    // 20일경우 100 * 20 즉 2000밀리세컨드 -> 최대 2초까지 기다려준다
                {
                    System.Threading.Thread.Sleep(100);
                    if (client.GetRemoteID() != RemoteID.Remote_None)
                        break;
                }

                // 그래도 할당안됬다면?
                if (client.GetRemoteID() == RemoteID.Remote_None)
                {
                    // 이경우는 연결이벤트 처리가 아직안된경우임
                    if (message_handler != null)
                        message_handler(MsgType.Error, "remoteID 할당이 안되었음.");
                }
            }

            CRecvedMsg recved_msg = new CRecvedMsg();
            try
            {
                CPackOption packOption = new CPackOption();
                CMessage MsgBuffer = new CMessage(buffer, 0, len);
                recved_msg.From(
                    client.GetRemoteID(),
                    MsgBuffer,
                    MsgBuffer.ReadStart(out packOption),
                    packOption
                );

                // 예외발생 test 
                //string a = "aa";
                //int.Parse(a);
            }
            catch (Exception e)
            {
                if (message_handler != null)
                    message_handler(MsgType.Warning, string.Format("{0} OnPackException: {1}",
                        client.Address().ToString(),
                        e.Message));
                client.Disconnect();
                return;
            }

            string msg = OnPacket(recved_msg);
            if (msg != string.Empty)
            {
                if (message_handler != null)
                    message_handler(MsgType.Warning, string.Format("{0} {1}", client.Address().ToString(), msg));
                client.Disconnect();
            }
        }

        protected override bool RecvInternalMessage(RemoteID remote, PackInternal pkID, CMessage msgData, CPackOption op)
        {
            switch (pkID)
            {
                case PackInternal.ePID_CS_Test:
                    {
                        int aa;
                        string ss;
                        Marshaler.Read(msgData, out aa);
                        Marshaler.Read(msgData, out ss);
                        if (message_handler != null)
                            message_handler(MsgType.Debug, op.m_pack_mode + ", InterPkt: " + pkID + ", " + aa + ", " + ss);
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

        public void SendToClient(RemoteID remote, IRemoteClient client, CMessage msg, CPackOption op)
        {
            PreparePacket(remote, RemoteID.Remote_Server, msg, op);
            client.SendToRemote(CPacket.create(msg));
        }

        public override bool Send(RemoteID remote, CMessage msg, CPackOption op)
        {
            //IRemoteClient client = null;
            //lock (sync_obj)
            //{
            //    if (RemoteClients.TryGetValue(remote, out client) == false)
            //    {
            //        return false;
            //    }
            //}
            IRemoteClient client = (IRemoteClient)GetRemoteClient(remote);
            if (client != null)
            {
                SendToClient(remote, client, msg, op);
            }
            return true;
        }

        public T GetRemoteClient(RemoteID remote)
        {
            IRemoteClient client = null;
            lock (sync_obj)
            {
                if (RemoteClients.TryGetValue(remote, out client) == false)
                {
                    return (T)client;
                }
            }
            return (T)client;
        }
    }
}
