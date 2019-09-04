using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SuperSocket.Common;
using SuperSocket.SocketBase.Protocol;
using SuperSocket.Facility.Protocol;


namespace TestNetServer
{
    public class EFBinaryRequestInfo : BinaryRequestInfo
    {
        //public Int16 PacketID { get; }
        //public EFBinaryRequestInfo(Int16 packetID, byte[] body)
        public EFBinaryRequestInfo(byte[] body)
            : base(null, body)
        {
            //this.PacketID = packetID;
        }
    }

    public class ReceiveFilter : FixedHeaderReceiveFilter<EFBinaryRequestInfo>
    {
        public ReceiveFilter() : base(ECore.CoreParam.HEADER_SIZE_LEN)
        {
        }

        protected override int GetBodyLengthFromHeader(byte[] header, int offset, int length)
        {
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(header, offset, ECore.CoreParam.HEADER_SIZE_LEN);

            // 패킷 사이즈 - 헤더 ===> 바디 사이즈
            return BitConverter.ToInt32(header, offset) - ECore.CoreParam.HEADER_SIZE_LEN;
        }

        protected override EFBinaryRequestInfo ResolveRequestInfo(ArraySegment<byte> header, byte[] bodyBuffer, int offset, int length)
        {
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(header.Array, 0, ECore.CoreParam.HEADER_SIZE_LEN);

            //return new EFBinaryRequestInfo(BitConverter.ToInt16(header.Array, 4), bodyBuffer.CloneRange(offset, length));
            return new EFBinaryRequestInfo(bodyBuffer.CloneRange(offset, length));
        }
    }
}
