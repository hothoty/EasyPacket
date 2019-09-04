using System;
using System.Collections.Generic;
//using System.Diagnostics;
//using System.Reflection;
//using System.Runtime.InteropServices;
using System.Linq;
using System.Text;
using System.Net;

namespace ECore
{
    class Clave
    {
        internal static readonly Int32 msg_clave = 9367;
        internal static readonly Int32 enc_header = 18;   // <pack_head>
    }

    public class CRecvedMsg
    {
        public RemoteID remote;
        public CMessage msg;
        public PacketType pkID;
        public CPackOption pkop;
        public NETWORK_EVENT evt;

        internal CRecvedMsg(NETWORK_EVENT ev)
        {
            evt = ev;
        }
        public CRecvedMsg()
        {
            evt = NETWORK_EVENT.pkt;
        }

        public void From(RemoteID in_remote, CMessage in_msg, PacketType in_pkID, CPackOption in_op)
        {
            this.remote = in_remote;
            this.msg = in_msg;
            this.pkID = in_pkID;
            this.pkop = in_op;
        }
    }

    public class CMessage
    {
        public byte[] buffer;
        int msglength = 0;
        int position = 0;

        internal int GetPosition() { return position; }


        // 8비트
        public void Write(byte p)
        {
            this.buffer[position] = p;
            position += 1;
        }
        public void Read(out byte p)
        {
            p = this.buffer[position];
            position += 1;
        }
        public void Write(sbyte p)
        {
            this.buffer[position] = (byte)p;
            position += 1;
        }
        public void Read(out sbyte p)
        {
            p = (sbyte)this.buffer[position];
            position += 1;
        }
        // bool
        public void Write(bool p)
        {
            byte data = 0;
            if (p) data = 1;
            Write(data);
        }
        public void Read(out bool p)
        {
            byte data = 0;
            Read(out data);

            if (data == 0)
                p = false;
            else
                p = true;
        }

        // 16비트
        public void Write(short p)
        {
            byte[] tempByte = BitConverter.GetBytes(p);
            Array.Copy(tempByte, 0, this.buffer, position, 2);
            position += 2;
        }
        public void Read(out short p)
        {
            p = BitConverter.ToInt16(this.buffer, position);
            position += 2;
        }
        // 16비트 unsinged
        public void Write(UInt16 p)
        {
            short data = (short)p;
            Write(data);
        }
        public void Read(out UInt16 p)
        {
            short data;
            Read(out data);
            p = (UInt16)data;
        }

        // 32비트
        public void Write(Int32 p)
        {
            byte[] tempByte = BitConverter.GetBytes(p);
            Array.Copy(tempByte, 0, this.buffer, position, 4);
            position += 4;
        }
        public void Read(out Int32 p)
        {
            p = BitConverter.ToInt32(this.buffer, position);
            position += 4;
        }
        // 32비트 unsigned
        public void Write(UInt32 p)
        {
            Int32 data = (Int32)p;
            Write(data);
        }
        public void Read(out UInt32 p)
        {
            Int32 data;
            Read(out data);
            p = (UInt32)data;
        }

        // 64비트
        public void Write(Int64 p)
        {
            byte[] tempByte = BitConverter.GetBytes(p);
            Array.Copy(tempByte, 0, this.buffer, position, 8);
            position += 8;
        }
        public void Read(out Int64 p)
        {
            p = BitConverter.ToInt64(this.buffer, position);
            position += 8;
        }
        // 64비트 unsigned
        public void Write(UInt64 p)
        {
            Int64 data = (Int64)p;
            Write(data);
        }
        public void Read(out UInt64 p)
        {
            Int64 data;
            Read(out data);
            p = (UInt64)data;
        }

        // float
        public void Write(float p)
        {
            byte[] tempByte = BitConverter.GetBytes(p);
            Array.Copy(tempByte, 0, this.buffer, position, 4);
            position += 4;
        }
        public void Read(out float p)
        {
            p = BitConverter.ToSingle(this.buffer, position);
            position += 4;
        }
        // double
        public void Write(double p)
        {
            byte[] tempByte = BitConverter.GetBytes(p);
            Array.Copy(tempByte, 0, this.buffer, position, 8);
            position += 8;
        }
        public void Read(out double p)
        {
            p = BitConverter.ToDouble(this.buffer, position);
            position += 8;
        }
        

        // 기타
        public void Write(PackInternal p)
        {
            short data = (short)p;
            Write(data);
        }
        public void Read(out PackInternal p)
        {
            short data;
            Read(out data);
            p = (PackInternal)data;
        }
        public void Write(RemoteID p)
        {
            Int32 data = (Int32)p;
            Write(data);
        }
        public void Read(out RemoteID p)
        {
            Int32 data;
            Read(out data);
            p = (RemoteID)data;
        }

        // netaddress
        public void Read(out NetAddress m_Addr)
        {
            NetAddress data = new NetAddress();
            Read(out data.m_remote);
            Read(out data.m_host);
            Read(out data.m_port);
            m_Addr = data;
        }
        public void Write(NetAddress m_Addr)
        {
            Write(m_Addr.m_remote);
            Write(m_Addr.m_host);
            Write(m_Addr.m_port);
        }

        // string
        public void Write(string str)
        {
            byte[] tempByte = System.Text.Encoding.Unicode.GetBytes(str);
            Int16 strLength = (Int16)(tempByte.Length / 2 + 1);
            Write(strLength);
            strLength *= 2;

            byte[] copyByte = new byte[strLength];
            Array.Copy(tempByte, 0, copyByte, 0, tempByte.Length);

            Array.Copy(copyByte, 0, this.buffer, position, strLength);
            position += strLength;
        }
        public void Read(out string str)
        {
            Int16 strLength;
            Read(out strLength);

            Int16 strBuffer = (Int16)(strLength * 2);
            strLength = (Int16)(strBuffer - 2);

            byte[] byteString = new byte[strLength];
            Array.Copy(this.buffer, position, byteString, 0, strLength);

            str = System.Text.Encoding.Unicode.GetString(byteString);
            position += strBuffer;
        }

        // Guid
        public void Write(Guid str)
        {
            byte[] tempByte = str.ToByteArray();
            Array.Copy(tempByte, 0, this.buffer, position, 16);
            position += 16;
        }
        public void Read(out Guid str)
        {
            byte[] byteGuid = new byte[16];
            Array.Copy(this.buffer, position, byteGuid, 0, 16);
            str = new Guid(byteGuid);
            position += 16;
        }

        // enum base
        public void Write(Protocol8 p)
        {
            byte data = (byte)p;
            Write(data);
        }
        public void Read(out Protocol8 p)
        {
            byte data;
            Read(out data);
            p = (Protocol8)data;
        }
        public void Write(PacketMode8 p)
        {
            byte data = (byte)p;
            Write(data);
        }
        public void Read(out PacketMode8 p)
        {
            byte data;
            Read(out data);
            p = (PacketMode8)data;
        }
        public void Write(PackExtra p)
        {
            byte data = (byte)p;
            Write(data);
        }
        public void Read(out PackExtra p)
        {
            byte data;
            Read(out data);
            p = (PackExtra)data;
        }

        public void WriteStart(PacketType pkType, CPackOption pkOption, Int16 PkInternalValue=0, bool isIDL = false)
        {
            if (isIDL)
            {
                if (pkType < PacketType.PacketType_User || pkType >= PacketType.PacketType_MaxCount)
                {
                    // error
                    return;
                }
            }
            else
            {
                if (pkType == PacketType.PacketType_Internal)
                {
                }
                else if (pkType == PacketType.PacketType_User)
                {
                    // 사용자 패킷인경우 일단 관리되게끔 하자
                }
            }

            this.ResetPosition();
            Marshaler.Write(this, pkOption);
            Marshaler.Write(this, pkType);
        }

        public PacketType ReadStart(out CPackOption op)
        {
            position = 0;

            Marshaler.Read(this, out op);

            PacketType pkType = (PacketType)BitConverter.ToInt16(this.buffer, position);
            position += 2;
            return pkType;
        }

        public void ResetPosition()
        {
            position = 0;
        }

        internal void Encrypt()
        {
            int work_pos = Clave.enc_header;
            int work_len = position - Clave.enc_header;

            int[] c = new int[2];
            bool bSwap = false;
            for (int i = work_pos; i < work_pos + work_len; i++)
            {
                buffer[i] += (byte)Clave.msg_clave;

                // 최소 4자리 숫자임 -> 2자리씩 2회 추가 암호화 작업
                if (bSwap)
                {
                    c[0] = Clave.msg_clave << 2;
                    c[1] = Clave.msg_clave << 3;
                    buffer[i] += (byte)c[0];
                    buffer[i] += (byte)c[1];
                }
                else
                {
                    c[0] = Clave.msg_clave << 3;
                    c[1] = Clave.msg_clave << 2;
                    buffer[i] += (byte)c[0];
                    buffer[i] += (byte)c[1];
                }
                bSwap = !bSwap;
            }
        }

        internal void Decrypt()
        {
            int work_pos = Clave.enc_header;
            int work_len = msglength - Clave.enc_header;

            int[] c = new int[2];
            bool bSwap = false;
            for (int i = work_pos; i < work_pos + work_len; i++)
            {
                buffer[i] -= (byte)Clave.msg_clave;

                // 최소 4자리 숫자임 -> 2자리씩 2회 추가 암호화 작업
                if (bSwap)
                {
                    c[0] = Clave.msg_clave << 2;
                    c[1] = Clave.msg_clave << 3;
                    buffer[i] -= (byte)c[0];
                    buffer[i] -= (byte)c[1];
                }
                else
                {
                    c[0] = Clave.msg_clave << 3;
                    c[1] = Clave.msg_clave << 2;
                    buffer[i] -= (byte)c[0];
                    buffer[i] -= (byte)c[1];
                }
                bSwap = !bSwap;
            }
        }

        public CMessage()
        {
            // send할때 기본 사이즈
            buffer = new byte[CoreParam.PACKET_MAX_SIZE];
        }
        public CMessage(byte[] buf, int startlength, int copy_size)
        {
            buffer = new byte[copy_size];
            Array.Copy(buf, startlength, buffer, 0, copy_size);
            msglength = copy_size;
        }
    }
}
