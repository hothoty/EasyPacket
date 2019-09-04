using System;
using System.Collections.Generic;
using System.Linq;

namespace ECore
{
    public enum RemoteID : int
    {
        Remote_None = -1,			// 유효하지 않음
        Remote_Server = -2,			// 서버전용ID
        Remote_Client = 0,			// Client 이보다 큰값 모두 해당   : if (remoteID > Remote_Client) --> 이렇게 ClientID판단가능
    };

    public enum PacketType : short
    {
        PacketType_None = -100,
        PacketType_Internal = -1,
        PacketType_Event = -2,

        PacketType_User = 0,
        PacketType_MaxCount = 32767
    }

    public enum PackInternal : short
    {
        ePID_Invalid = 0,               // 이값은 변경금지

        ePID_SC_Remote,                 // remoteID 전송

        ePID_CS_Test,					// test
        ePID_AliveSC,					// Alive패킷
    }

    public enum Protocol8 : byte
    {
        Protocol_Tcp = 0,
        //Protocol_Udp,
    }

    public enum PacketMode8 : byte
    {
        PM_None = 0,
        PM_Encrypt_Mode = 1,
    }

    public enum PackExtra : byte
    {
        Ex_None = 0,
    }

    public class NetAddress
    {
        public RemoteID m_remote;
        public string m_host;
        public int m_port = 0;

        public RemoteID remoteID
        {
            get { return m_remote; }
        }
        public string hostname
        {
            get { return m_host; }
        }
        public int port
        {
            get { return m_port; }
        }
        public override string ToString()
        {
            return string.Format("({0})/{1}/{2}", (int)m_remote, m_host, m_port);
        }

        public NetAddress() { }

        public NetAddress(RemoteID _remote, string _host, int _port)
        {
            m_remote = _remote;
            m_host = _host;
            m_port = _port;
        }
    }

    public class CPackOption
    {
        internal UInt32 m_Dummy;
        public RemoteID m_remote;
        public RemoteID m_from;
        public Protocol8 m_protocol;
        public PacketMode8 m_pack_mode;
        public PackExtra m_dmy_ex1;   // 확장옵션1 (일단 4바이트 맞추기 위해 ex2개를 추가)
        public PackExtra m_dmy_ex2;   // 확장옵션2
        public CPackOption()
        {
            m_Dummy = 0;
            m_remote = RemoteID.Remote_None;
            m_from = RemoteID.Remote_None;
            m_protocol = Protocol8.Protocol_Tcp;
            m_pack_mode = PacketMode8.PM_None;
            m_dmy_ex1 = PackExtra.Ex_None;
            m_dmy_ex2 = PackExtra.Ex_None;
        }
        public CPackOption(Protocol8 prot, PacketMode8 mode)
        {
            m_Dummy = 0;
            m_remote = RemoteID.Remote_None;
            m_from = RemoteID.Remote_None;
            m_protocol = prot;
            m_pack_mode = mode;
            m_dmy_ex1 = PackExtra.Ex_None;
            m_dmy_ex2 = PackExtra.Ex_None;
        }

        static public CPackOption Basic = new CPackOption();
        static public CPackOption Encrypt = new CPackOption(Protocol8.Protocol_Tcp, PacketMode8.PM_Encrypt_Mode);
    }
}
