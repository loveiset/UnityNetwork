using System.Collections.Generic;
using System.Net.Sockets;

namespace UnityNetwork
{
    public class NetPacket
    {
        public byte[] _bytes;
        public Socket _peer = null;

        protected int _length = 0;
        public string _error = "";
        public NetPacket()
        {
            _bytes = new byte[NetBitStream.header_length + NetBitStream.max_body_length];
        }

        public void CopyBytes(NetBitStream stream)
        {
            stream.BYTES.CopyTo(_bytes, 0);
            _length = stream.Length;
        }

        public void SetIDOnly(ushort msgid)
        {
            byte[] bs = System.BitConverter.GetBytes(msgid);

            bs.CopyTo(_bytes, NetBitStream.header_length);
            _length = NetBitStream.header_length + NetBitStream.SHORT16_LEN;
        }

        public void TOID(out ushort msg_id)
        {
            msg_id = System.BitConverter.ToUInt16(_bytes, NetBitStream.header_length);
        }
    }
}
