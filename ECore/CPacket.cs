using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ECore
{
    /// <summary>
    /// 
    /// CMessage버퍼를 이용하므로 최소 기능만 남겨두고 단순화 시킨 클래스
    /// 
    /// </summary>
    public class CPacket
    {
        public byte[] buffer { get; private set; }
        public int position { get; private set; }

        public CPacket()
        {
            this.buffer = new byte[CoreParam.PACKET_MAX_SIZE];
        }

        public static CPacket create(CMessage msgBuffer)
        {
            CPacket packet = new CPacket();
            packet.Prepare(msgBuffer);
            return packet;
        }

        void Prepare(CMessage msgBuffer)
        {
            // 헤더는 나중에 넣을것이므로 데이터 부터 넣을 수 있도록 위치를 점프시켜놓는다.
            this.position = CoreParam.HEADER_SIZE_LEN;
            Array.Copy(msgBuffer.buffer, 0, this.buffer, position, msgBuffer.GetPosition());
            this.position += msgBuffer.GetPosition();
        }

        public void record_size()
        {
            byte[] header = BitConverter.GetBytes(this.position);
            header.CopyTo(this.buffer, 0);
        }
    }
}
