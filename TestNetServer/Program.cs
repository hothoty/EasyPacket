using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using SuperSocket.SocketBase.Logging;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Protocol;
using SuperSocket.SocketBase.Config;
using ECoreServer;
using ECore;


namespace TestNetServer
{
    public class NetworkSession : AppSession<NetworkSession, EFBinaryRequestInfo>, IRemoteClient
    {
        public RemoteID remote = RemoteID.Remote_None;

        RemoteID IRemoteClient.GetRemoteID()
        {
            return remote;
        }

        void IRemoteClient.SendToRemote(CPacket pk)
        {
            pk.record_size();
            Send(pk.buffer, 0, pk.position);
        }

        void IRemoteClient.Disconnect()
        {
            Close(CloseReason.ServerClosing);
        }

        public NetAddress Address()
        {
            return new NetAddress(remote, RemoteEndPoint.Address.ToString(), RemoteEndPoint.Port);
        }
    }

    public class NetServer : INetServer<NetworkSession>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="max"></param> 최대 동시접속자 설정
        public NetServer(int max) : base(max)
        {
        }
    }

    partial class Server : AppServer<NetworkSession, EFBinaryRequestInfo>
    {
        const int Max_Connection = 1000;


        // 서버클래스
        NetServer net;

        // 기본 rmi클래스
        public Rmi.Proxy proxy;
        Rmi.Stub stub;

        // 부가 rmi클래스
        public RmiGame.Proxy proxyGame;
        RmiGame.Stub stubGame;


        public IServerConfig m_Config { get; private set; }

        public Server() : base(new DefaultReceiveFilterFactory<ReceiveFilter, EFBinaryRequestInfo>())
        {
            net = new NetServer(Max_Connection);

            // 내부 메세지 핸들러 처리
            net.message_handler = (t, str) =>
            {
                Console.WriteLine(t + ", " + str);
            };

            NewSessionConnected += new SessionHandler<NetworkSession>(OnConnected);
            SessionClosed += new SessionHandler<NetworkSession, CloseReason>(OnClosed);
            NewRequestReceived += new RequestHandler<NetworkSession, EFBinaryRequestInfo>(OnRecved);
        }

        void OnConnected(NetworkSession session)
        {
            //Thread.Sleep(1000);

            // NetServer 내부적으로 관리를 위해 클라이언트 연결시점에 필요한 작업
            session.remote = net.OnConnect(ref session);

            Console.WriteLine(DateTime.Now.ToLongTimeString() + "{0} SessionID {1} Connected.  thrID({2})",
                session.Address().ToString(),
                session.SessionID, Thread.CurrentThread.ManagedThreadId);

            proxy.sc_test(session.remote, ECore.CPackOption.Encrypt, 123, true, "메세지~테스트abc---123");
        }

        void OnClosed(NetworkSession session, CloseReason reason)
        {
            Console.WriteLine("{0} SessionID {1} DisconnectReason : {2}  thdID({3})",
                session.Address().ToString(),
                session.SessionID, reason.ToString(), Thread.CurrentThread.ManagedThreadId);

            // NetServer 내부적으로 관리를 위해 클라이언트 연결종료시점에 필요한 작업
            net.OnLeave(session);
        }

        void OnRecved(NetworkSession session, EFBinaryRequestInfo reqInfo)
        {
            //Console.WriteLine(DateTime.Now.ToLongTimeString() + string.Format("remote({0}) OnRecved. thrID({1})", (int)session.remote, Thread.CurrentThread.ManagedThreadId));

            // NetServer 내부적으로 패킷받은 시점에 rmi 이벤트를 발생시키기 위해 필요한 작업 --> PacketHandler.cs의 stub으로 이벤트가 자동 발생됨
            net.OnRecv(session, reqInfo.Body, reqInfo.Body.Length);
        }

        public void InitConfig()
        {
            m_Config = new ServerConfig
            {
                Port = 22000,
                Ip = "Any",

                // 리스너 여러개 설정시
                //Listeners = new List<ListenerConfig>
                //{
                //    new ListenerConfig()
                //    {
                //        Ip = "Any", Port = 30001
                //    },
                //    new ListenerConfig()
                //    {
                //        Ip = "Any", Port = 30002
                //    },
                //    new ListenerConfig()
                //    {
                //        Ip = "Any", Port = 30003
                //    }
                //},

                MaxConnectionNumber = Max_Connection,
                Mode = SocketMode.Tcp,
                //MaxRequestLength = option.MaxRequestLength,
                ReceiveBufferSize = 4096,
                SendBufferSize = 4096,
                Name = "TestServerNet"
            };
        }

        public bool CreateServer()
        {
            try
            {
                var cfg = new RootConfig();
                bool bResult = Setup(cfg, m_Config, logFactory: new Log4NetLogFactory());
                if (bResult == false)
                {
                    Console.WriteLine("server config error");
                    return false;
                }

                Console.WriteLine("MaxWork : " + cfg.MaxWorkingThreads + ", MinWork : " + cfg.MinWorkingThreads);
                Console.WriteLine("MaxIoThr: " + cfg.MaxCompletionPortThreads + ", MinIoThr: " + cfg.MinCompletionPortThreads);

                proxy = new Rmi.Proxy();
                stub = new Rmi.Stub();

                proxyGame = new RmiGame.Proxy();
                stubGame = new RmiGame.Stub();

                net.Attach(proxy, stub);
                net.Attach(proxyGame, stubGame);
                PacketHandler();
            }
            catch (Exception e)
            {
                Console.WriteLine("CreateServer Exception: " + e);
                return false;
            }
            return true;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("서버초기화...");
            Server svr = new Server();
            svr.InitConfig();
            if (svr.CreateServer())
            {
                Console.WriteLine("성공, Listen:" + svr.m_Config.Ip + "/" + svr.m_Config.Port);
            }
            else
            {
                Console.WriteLine("서버초기화-실패");
                System.Threading.Thread.Sleep(1000 * 100);
                return;
            }

            Console.WriteLine("서비스 시작....");

            var IsResult = svr.Start();

            if (IsResult)
            {
                Console.WriteLine("서비스 시작 성공~");
            }
            else
            {
                Console.WriteLine("서비스 시작 실패!!!");
                System.Threading.Thread.Sleep(1000 * 100);
                return;
            }

            var ret = ReadLineAsync();
            bool run_program = true;
            while (run_program)
            {
                if (ret.IsCompleted)
                {
                    switch (ret.Result)
                    {
                        case "/d":
                            break;

                        case "/h":
                            break;

                        case "/stat":
                            Console.WriteLine("test...");
                            break;

                        case "/q":
                            Console.WriteLine("quit Server...");
                            run_program = false;    // 종료하자
                            break;
                    }
                    if (run_program)
                        ret = ReadLineAsync();
                }

                System.Threading.Thread.Sleep(10);
            }

            Console.WriteLine("Start Closing...  ");
            Console.WriteLine("Server close complete.");
            System.Threading.Thread.Sleep(1000 * 2);
        }

        static async Task<string> ReadLineAsync()
        {
            var line = await Task.Run(() => Console.ReadLine());
            return line;
        }
    }
}
