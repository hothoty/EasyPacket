using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using ECore;

namespace ECoreClient
{
    public class INetClient : CoreClient
    {
        // 소켓 recv할때 쪼개져 오는걸 쉽게 조립하기위한 고정버퍼(한번만 할당하니깐 좀 크게잡아두자, 그래야 편하게 큰 패킷을 받을 수 있당)
        byte[] recv_buf = new byte[40960];


        public delegate void EventDelegate();
        public delegate void ConnectDelegate(bool isConnectSuccess);


        /// <summary>
        /// 서버와 연결시도 결과
        /// </summary>
        public ConnectDelegate server_connect_result_handler { get; set; }

        /// <summary>
        /// 서버와 연결된 이후 통신이 준비된 시점, 이때부터 remoteID를 통해 통신이 가능함
        /// </summary>
        public EventDelegate client_ready_handler { get; set; }

        /// <summary>
        /// 서버와의 연결이 종료됨
        /// </summary>
        public EventDelegate server_disconnect_handler { get; set; }

        /// <summary>
        /// 마지막 발생된 소켓 에러코드
        /// </summary>
        public SocketError last_connect_socket_err { get; private set; }


        EventManager event_manager;

        internal CUserClient m_client;

        Int32 m_headsize;

        string hostname;
        int portnum;


        // 스레드 관리를 위해
        Thread thr;


        bool bSocketThread = false;

        public bool isClientLive
        {
            get { return thr != null; }
        }


        public INetClient(int head_size = -1)
        {
            if (head_size != -1)
                m_headsize = head_size;
            else
                m_headsize = ECore.CoreParam.HEADER_SIZE_LEN;

            thr = null;

            event_manager = new EventManager();

            this.m_client = new CUserClient(this);
        }

        internal override void OnReadyClient()
        {
            if (client_ready_handler != null)
                client_ready_handler();
        }


        string SocketConnect(AddressFamily addnet)
        {
            try
            {
                m_client.socket = new Socket(addnet, SocketType.Stream, ProtocolType.Tcp);
                m_client.socket.Connect(hostname, portnum);
            }
            catch (SocketException se)
            {
                last_connect_socket_err = se.SocketErrorCode;
                return string.Format("sockException: {0}, {1}", addnet, se.Message);
            }
            catch (Exception ex)
            {
                return string.Format("Exception: {0}, {1}", addnet, ex.Message);
            }
            return string.Empty;
        }


        void SocketThread()
        {
            // ipv4로 먼저 접속 시도
            string err = SocketConnect(AddressFamily.InterNetwork);

            if (m_client.socket.Connected == false)
            {
                // ipv4로 접속 실패시 ipv6로 즉시 재시도한다 : ios경우 ipv6 only환경에서 작동되야 하므로..
                //  -> 즉시 시도해도 되는 이유는 addressfamily가 다르면 즉각 피드백오므로 문제없다
                string err2 = SocketConnect(AddressFamily.InterNetworkV6);

                if (m_client.socket.Connected == false)
                {
                    if (err != string.Empty && message_handler != null)
                        message_handler(MsgType.Info, err);

                    if (err2 != string.Empty && message_handler != null)
                        message_handler(MsgType.Info, err2);
                }
            }

            if (m_client.socket.Connected == false)
            {
                this.event_manager.enqueue_network_event(NETWORK_EVENT.connect_fail);
                thr = null;
                return;
            }

            this.event_manager.enqueue_network_event(NETWORK_EVENT.connected);

            // 여기서부터 연결이 끊길때까지 Recv무한루프가 시작된다
            bSocketThread = true;

            byte[] buf;
            int bufsize;
            while (bSocketThread)
            {
                if (RecvLong(out buf, out bufsize) == false)
                {
                    event_manager.enqueue_network_event(NETWORK_EVENT.disconnected);
                    thr = null;
                    return;
                }

                if (bufsize - m_headsize < 0)
                {
                    if (this.message_handler != null)
                        this.message_handler(MsgType.Warning, string.Format("pksize error : {0}", bufsize));

                    event_manager.enqueue_network_event(NETWORK_EVENT.disconnected);
                    thr = null;
                    return;
                }

                bufsize -= ECore.CoreParam.HEADER_SIZE_LEN;

                try
                {
                    CRecvedMsg recved_msg = new CRecvedMsg();
                    CPackOption packOption = new CPackOption();
                    CMessage MsgBuffer = new CMessage(buf, 4, bufsize);
                    recved_msg.From(
                        RemoteID.Remote_Server,
                        MsgBuffer,
                        MsgBuffer.ReadStart(out packOption),
                        packOption
                    );

                    event_manager.enqueue_network_message(recved_msg);
                }
                catch (Exception e)
                {
                    if (this.message_handler != null)
                        this.message_handler(MsgType.Warning, string.Format("recvMsgException : {0}", e.Message));

                    event_manager.enqueue_network_event(NETWORK_EVENT.disconnected);
                    thr = null;
                    return;
                }
            }
            thr = null;
        }

        bool RecvLong(out byte[] buf, out int ssize)
        {
            int ret;
            int temp = 0;
            ssize = 0;
            buf = new byte[1];

            byte[] bsize = new byte[m_headsize];

            while (true)
            {
                int before_recv = temp;
                try
                {
                    ret = m_client.socket.Receive(recv_buf, m_headsize - temp, SocketFlags.None);
                }
                catch
                {
                    return false;
                }

                if (ret == 0)
                    return false;   // 정상적인 연결해제

                if (ret <= 0)
                    return false;

                temp += ret;
                if (temp == m_headsize)
                {
                    Array.Copy(recv_buf, 0, bsize, before_recv, ret); // 패킷조립
                    break;
                }

                Array.Copy(recv_buf, 0, bsize, before_recv, ret); // 패킷조립
            }
            ssize = BitConverter.ToInt32(bsize, 0);
            if (ssize == 0)
                return true;

            buf = new byte[ssize];

            // 새로만든 buf에다가 사이즈를 넣어준다
            Array.Copy(bsize, 0, buf, 0, m_headsize);

            int loop = 0;

            while (true)
            {
                int before_recv = temp;
                try
                {
                    ret = m_client.socket.Receive(recv_buf, ssize - temp, SocketFlags.None);
                }
                catch
                {
                    return false;
                }

                if (ret == 0)
                    return false;

                if (ret <= 0)
                    return false;

                temp += ret;
                if (temp == ssize)
                {
                    Array.Copy(recv_buf, 0, buf, before_recv, ret); // 패킷조립
                    break;
                }

                Array.Copy(recv_buf, 0, buf, before_recv, ret); // 패킷조립

                loop++;
            }

            return ret == -1 ? false : true;
        }

        public bool Connect(string _hostname, int _portnum)
        {
            hostname = _hostname;
            portnum = _portnum;

            if (thr != null) return false;

            thr = new Thread(this.SocketThread);
            thr.Start();
            return true;
        }

        /// <summary>
        /// 서버와 연결해제 (Disconnect함수와 동일)
        /// </summary>
        public bool Destroy()
        {
            if (bSocketThread)
            {
                m_client.disconnect();
                bSocketThread = false;
                return true;
            }
            return false;
        }

        /// <summary>
        /// 서버와 연결해제 (Destroy함수와 동일)
        /// </summary>
        public void Disconnect()
        {
            Destroy();
        }

        // 더이상사용안함
        //public void Send(ZNet.CPacket pk)
        //{
        //    m_client.DoSend(pk);
        //}

        public override bool Send(RemoteID remote, CMessage msg, CPackOption op)
        {
            PreparePacket(remote, RemoteID.Remote_Client, msg, op);
            m_client.DoSend(CPacket.create(msg));
            return true;
        }

        public void Update()
        {
            while (this.event_manager.has_event())
            {
                CRecvedMsg recved_msg = this.event_manager.dequeue_network_event();

                if (recved_msg.evt == NETWORK_EVENT.disconnected)
                {
                    if (server_disconnect_handler != null)
                        server_disconnect_handler();
                }
                else if (recved_msg.evt == NETWORK_EVENT.connected)
                {
                    if (server_connect_result_handler != null)
                        server_connect_result_handler(true);

                    //// 내부패킷 보내기 test
                    //CMessage sMsg = new CMessage();
                    //CPackOption sendOption = CPackOption.Basic;
                    //sMsg.WriteStart(PacketType.PacketType_Internal, sendOption);
                    //Marshaler.Write(sMsg, PackInternal.ePID_CS_Test);
                    //Marshaler.Write(sMsg, 123);
                    //Marshaler.Write(sMsg, "가나다");
                    //Send(RemoteID.Remote_Server, sMsg, sendOption);
                }
                else if (recved_msg.evt == NETWORK_EVENT.connect_fail)
                {
                    if (server_connect_result_handler != null)
                        server_connect_result_handler(false);
                }
                else
                {
                    string msg = OnPacket(recved_msg);
                    if (msg != string.Empty)
                    {
                        if (this.message_handler != null)
                            this.message_handler(MsgType.Warning, msg);
                        Disconnect();
                    }
                }
            }
        }
    }
}
