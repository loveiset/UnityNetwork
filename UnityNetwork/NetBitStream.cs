using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;

namespace UnityNetwork
{
    public class NetBitStream
    {
        //定义消息头和消息体长度
        //消息头int32
        //消息体最大512字节

        public const int header_length = 4;

        public const int max_body_length = 512;

        public const int BYTE_LEN = 1;
        public const int SHORT16_LEN = 2;
        public const int INT32_LEN = 4;
        public const int FLOAT_LEN = 4;

        private byte[] _bytes = null;
        public byte[] BYTES
        {
            get
            {
                return _bytes;
            }
            set
            {
                _bytes = value;
            }
        }

        private int _bodyLength = 0;
        public int BodyLength
        {
            get
            {
                return _bodyLength;
            }
        }

        public int Length
        {
            get
            {
                return header_length + _bodyLength;
            }
        }

        public Socket _socket = null;



        public NetBitStream()
        {
            _bodyLength = 0;
            _bytes = new byte[header_length + max_body_length];
        }

        public void BeginWrite(ushort msdid)
        {
            _bodyLength = 0;
            this.WriteUShort(msdid);
        }

        public void WriteByte(byte bt)
        {
            if (_bodyLength + BYTE_LEN > max_body_length)
            {
                return;
            }

            _bytes[header_length + _bodyLength] = bt;
            _bodyLength += BYTE_LEN;
        }

        public void WriteBool(bool flag)
        {
            if (_bodyLength + BYTE_LEN > max_body_length)
                return;

            byte b = (byte)'1';
            if (!flag)
                b = (byte)'0';

            _bytes[header_length + _bodyLength] = b;
            _bodyLength += BYTE_LEN;
        }

        public void WriteInt(int number)
        {
            if (_bodyLength + INT32_LEN > max_body_length)
                return;

            byte[] bs = System.BitConverter.GetBytes(number);
            bs.CopyTo(_bytes, header_length + _bodyLength);
            _bodyLength += INT32_LEN;
        }

        public void WriteUInt(uint number)
        {
            if (_bodyLength + INT32_LEN > max_body_length)
                return;

            byte[] bs = System.BitConverter.GetBytes(number);
            bs.CopyTo(_bytes, header_length + _bodyLength);
            _bodyLength += INT32_LEN;
        }

        public void WriteShort(short number)
        {
            if (_bodyLength + SHORT16_LEN > max_body_length)
                return;

            byte[] bs = System.BitConverter.GetBytes(number);
            bs.CopyTo(_bytes, header_length + _bodyLength);
            _bodyLength += SHORT16_LEN;
        }

        public void WriteUShort(ushort number)
        {
            if (_bodyLength + SHORT16_LEN > max_body_length)
                return;

            byte[] bs = System.BitConverter.GetBytes(number);
            bs.CopyTo(_bytes, header_length + _bodyLength);
            _bodyLength += SHORT16_LEN;
        }

        public void WriteFloat(float number)
        {
            if (_bodyLength + FLOAT_LEN > max_body_length)
                return;

            byte[] bs = System.BitConverter.GetBytes(number);
            bs.CopyTo(_bytes, header_length + _bodyLength);
            _bodyLength += FLOAT_LEN;
        }

        public void WriteString(string str)
        {
            ushort len = (ushort)System.Text.Encoding.UTF8.GetByteCount(str);
            this.WriteUShort(len);

            if (_bodyLength + len > max_body_length)
                return;

            System.Text.Encoding.UTF8.GetBytes(str, 0, str.Length, _bytes, header_length + _bodyLength);
            _bodyLength += len;
        }

        public void BeginRead(NetPacket packet, out ushort msgid)
        {
            packet._bytes.CopyTo(this.BYTES, 0);
            this._socket = packet._peer;
            _bodyLength = 0;
            this.ReadUShort(out msgid);
        }

        public void BeginRead2(NetPacket packet)
        {
            packet._bytes.CopyTo(this.BYTES, 0);
            this._socket = packet._peer;
            _bodyLength = 0;
            _bodyLength += SHORT16_LEN;
        }

        public void ReadByte(out byte bt)
        {
            bt = 0;
            if (_bodyLength + BYTE_LEN > max_body_length)
                return;

            bt = _bytes[header_length + _bodyLength];
            _bodyLength += BYTE_LEN;
        }

        public void ReadBool(out bool flag)
        {
            flag = false;
            if (_bodyLength + BYTE_LEN > max_body_length)
                return;

            byte bt = _bytes[header_length + _bodyLength];

            if (bt == (byte)'1')
                flag = true;
            else
                flag = false;

            _bodyLength += BYTE_LEN;
        }

        public void ReadInt(out int number)
        {
            number = 0;
            if (_bodyLength + INT32_LEN > max_body_length)
                return;

            number = System.BitConverter.ToInt32(_bytes, header_length + _bodyLength);
            _bodyLength += INT32_LEN;
        }

        public void ReadUInt(out uint number)
        {
            number = 0;
            if (_bodyLength + INT32_LEN > max_body_length)
                return;

            number = System.BitConverter.ToUInt32(_bytes, header_length + _bodyLength);
            _bodyLength += INT32_LEN;
        }

        public void ReadUShort(out ushort number)
        {
            number = 0;
            if (_bodyLength + SHORT16_LEN > max_body_length)
                return;

            number = System.BitConverter.ToUInt16(_bytes, header_length + _bodyLength);
            _bodyLength += SHORT16_LEN;
        }

        public void ReadShort(out short number)
        {
            number = 0;
            if (_bodyLength + SHORT16_LEN > max_body_length)
                return;

            number = System.BitConverter.ToInt16(_bytes, header_length + _bodyLength);
            _bodyLength += SHORT16_LEN;
        }

        public void ReadFloat(out float number)
        {
            number = 0;
            if (_bodyLength + FLOAT_LEN > max_body_length)
                return;

            number = System.BitConverter.ToSingle(_bytes, header_length + _bodyLength);
            _bodyLength += FLOAT_LEN;
        }

        public void ReadString(out string str)
        {
            str = "";
            ushort len = 0;
            ReadUShort(out len);

            if (_bodyLength + len > max_body_length)
                return;

            str = Encoding.UTF8.GetString(_bytes, header_length + _bodyLength, (int)len);
            _bodyLength += len;

        }

        public bool CopyBytes(byte[] bs)
        {
            if (bs.Length > _bytes.Length)
                return false;

            bs.CopyTo(_bytes, 0);
            _bodyLength = System.BitConverter.ToInt32(_bytes, 0);

            return true;
        }

        public void EncodeHeader()
        {
            byte[] bs = System.BitConverter.GetBytes(_bodyLength);
            bs.CopyTo(_bytes, 0);
        }

        public void DecodeHeader()
        {
            _bodyLength = System.BitConverter.ToInt32(_bytes, 0);
        }
    }
}
