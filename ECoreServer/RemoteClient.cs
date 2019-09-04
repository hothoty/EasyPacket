using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using ECore;


namespace ECoreServer
{
    public interface IRemoteClient
    {
        void SendToRemote(CPacket pk);

        RemoteID GetRemoteID();

        void Disconnect();

        NetAddress Address();
    }

    internal class RemotePool
    {
        static public int MaxConnectionCount { get; private set; }
        static ConcurrentBag<int> remotePool = new ConcurrentBag<int>();


        public static void CreateRemotePool(int maxCount)
        {
            MaxConnectionCount = maxCount;
            for (int i = 0; i < MaxConnectionCount; i++)
            {
                remotePool.Add(i);
            }
        }

        public static RemoteID GetRemotePool()
        {
            if (remotePool.TryTake(out var result))
                return (RemoteID)result;
            return RemoteID.Remote_None;
        }

        public static void FreeRemotePool(RemoteID remote)
        {
            if ((int)remote >= 0)
            {
                remotePool.Add((int)remote);
            }
        }
    }
}
