using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ECoreClient;
using ECore;


namespace TestNetClient
{
    class Program
    {
        public class NetClient : INetClient
        {
        }

        static void Main(string[] args)
        {
            NetClient client = new NetClient();

            Rmi.Proxy proxy = new Rmi.Proxy();
            Rmi.Stub stub = new Rmi.Stub();
            RmiGame.Proxy proxyGame = new RmiGame.Proxy();
            RmiGame.Stub stubGame = new RmiGame.Stub();

            client.Attach(proxy, stub);
            client.Attach(proxyGame, stubGame);


            client.message_handler = (t, str) =>
            {
                Console.WriteLine(string.Format("{0}: {1}", t, str));
            };

            client.server_connect_result_handler = (ok) =>
            {
                Console.WriteLine("서버연결결과: " + ok);

                proxy.cs_test(RemoteID.Remote_Server, CPackOption.Basic, 1);
                proxy.cs_test(RemoteID.Remote_Server, CPackOption.Encrypt, 2);

                proxyGame.cs_num(RemoteID.Remote_Server, CPackOption.Basic, 1122);
            };

            // 서버 연결후 rmi를 이용한 서버와의 통신이 준비된 시점, 이때부터 idl파일에 정의한 패킷으로 통신해야함
            client.client_ready_handler = () =>
            {
                Console.WriteLine("클라이언트 Ready. my remoteID: " + (int)client.remoteID);
            };

            client.server_disconnect_handler = () =>
            {
                Console.WriteLine("서버와 연결종료됨");
            };

            stub.sc_test = (ECore.RemoteID remote, ECore.CPackOption pkOption, int a, bool b, string zz) =>
            {
                Console.WriteLine(string.Format("받은패킷: {0}  {1}  {2}  ({3})", a, b, zz, pkOption.m_pack_mode));
                return true;
            };

            stubGame.sc_num = (ECore.RemoteID remote, ECore.CPackOption pkOption, int bb) =>
            {
                Console.WriteLine(string.Format("stubGame.sc_num: {0}  ({1})", bb, pkOption.m_pack_mode));

                //// 예외발생test
                //string a = "aa";
                //int.Parse(a);

                return true;
            };

            string hostname = "localhost";
            int portnum = 22000;

            Console.WriteLine("서버와 연결대기중.\n연결cmd '/c': " + hostname + "/" + portnum);


            Console.Write("\n> ");

            var ret = ReadLineAsync();
            bool run_program = true;
            while (run_program)
            {
                if (ret.IsCompleted)
                {
                    switch (ret.Result)
                    {
                        case "/c":
                            Console.WriteLine("서버와 연결중...");
                            if (client.Connect(hostname, portnum) == false)
                            {
                                Console.WriteLine("기존연결이 아직종료되지 않았습니다. 기존연결을 먼저 종료해야 새로운연결이 가능합니다.");
                            }
                            break;

                        case "/q":
                            if (client.Destroy() == false)
                            {
                                Console.WriteLine("서버와 연결중이 아닙니다");
                            }
                            //if (client.isClientLive)
                            //    client.Destroy();    // 소켓파괴
                            //else
                            //    Console.WriteLine("서버와 통신중이 아닙니다");

                            //run_program = false;    // 종료하자
                            break;

                        default:
                            break;
                    }

                    if (run_program)
                        ret = ReadLineAsync();
                }

                client.Update();
                System.Threading.Thread.Sleep(1);
            }
        }

        static async Task<string> ReadLineAsync()
        {
            var line = await Task.Run(() => Console.ReadLine());
            return line;
        }
    }
}
