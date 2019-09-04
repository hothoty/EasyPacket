using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace TestNetServer
{
    public partial class Server
    {
        public void PacketHandler()
        {
            stub.cs_test = (ECore.RemoteID remote, ECore.CPackOption pkOption, int num) =>
            {
                //Console.WriteLine(string.Format("fromRemote: {0}", (int)remote));

                // 1개의 remote에대해 딜레이를 줬을때 동일remote에 대한 순서꼬임과 다른remote들에게 미치는 영향 확인함. 이상없음
                //if (num == 1 && (int)remote == 0)
                //{
                //    Thread.Sleep(5000);
                //}

                NetworkSession session = net.GetRemoteClient(remote);
                if (session == null) return true;

                Console.WriteLine(DateTime.Now.ToLongTimeString() + string.Format(
                    "{0} param({1}) thrID({2})",
                    session.Address().ToString(), num, Thread.CurrentThread.ManagedThreadId));

                //NetworkSession session = netServer.GetRemoteClient(remote);
                //if (session == null) return true;
                //session.Close();
                //Thread.Sleep(5000);
                //Console.WriteLine(DateTime.Now.ToLongTimeString() + string.Format("{0} End param({1}) thrID({2})",
                //    session.Address().ToString(), num, Thread.CurrentThread.ManagedThreadId));

                //// 예외발생test
                //string a = "aa";
                //int.Parse(a);

                return true;
            };

            stubGame.cs_num = (ECore.RemoteID remote, ECore.CPackOption pkOption, int aa) =>
            {
                Console.WriteLine(DateTime.Now.ToLongTimeString() + string.Format(
                    "<{0}> remote({1}) param({2}) thrID({3})",
                    RmiGame.Common.cs_num,
                    (int)remote, aa, Thread.CurrentThread.ManagedThreadId));
                proxyGame.sc_num(remote, pkOption, aa);
                return true;
            };
        }
    }
}
