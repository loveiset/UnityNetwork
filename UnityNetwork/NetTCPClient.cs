using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace UnityNetwork
{
    class NetTCPClient
    {
        public int _sendTimeout = 3;
        public int _revTimeout = 3;

        private NetworkManager _netMgr = null;

        private Socket _socket = null;

        public NetTCPClient()
        {
            _netMgr = NetworkManager.Instance;
        }

        public bool Connect(string address, int remotePort)
        {
            if (_socket != null && _socket.Connected)
            {
                return true;
            }

            IPHostEntry hostEntry = Dns.GetHostEntry(address);
            foreach (IPAddress ip in hostEntry.AddressList)
            {
                try
                {
                    IPEndPoint ipe = new IPEndPoint(ip, remotePort);
                    _socket = new Socket(ipe.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                    _socket.BeginConnect(ipe, new System.AsyncCallback(ConnectionCallback), _socket);
                    break;
                }
                catch(System.Exception e)
                {
                    PushPacket((ushort)MessageIdentifiers.ID.CONNECTION_ATTEMPT_FAILED, e.Message);
                    return false;
                }
            }
            return true;
        }

        void PushPacket(ushort msgid, string exception)
        {
            NetPacket packet = new NetPacket();
            packet.SetIDOnly(msgid);
            packet._error = exception;
            packet._peer = _socket;

            _netMgr.AddPacket(packet);
        }

        void PushPacket2(NetBitStream stream)
        {
            NetPacket packet = new NetPacket();
            stream.BYTES.CopyTo(packet._bytes, 0);
            packet._peer = stream._socket;

            _netMgr.AddPacket(packet);
        }

        void ConnectionCallback(System.IAsyncResult ar)
        {
            NetBitStream stream = new NetBitStream();
            stream._socket = (Socket)ar.AsyncState;

            try
            {
                _socket.EndConnect(ar);
                _socket.SendTimeout = _sendTimeout;
                _socket.ReceiveTimeout = _revTimeout;

                PushPacket((ushort)MessageIdentifiers.ID.CONNECTION_REQUEST_ACCEPTED, "");
                _socket.BeginReceive(stream.BYTES, 0, NetBitStream.header_length, SocketFlags.None, new System.AsyncCallback(ReceiveHeader), stream);


            }
            catch (System.Exception e)
            {
                if (e.GetType() == typeof(SocketException))
                {
                    if (((SocketException)e).SocketErrorCode == SocketError.ConnectionRefused)
                    {
                        PushPacket((ushort)MessageIdentifiers.ID.CONNECTION_ATTEMPT_FAILED, e.Message);
                    }
                    else
                    {
                        PushPacket((ushort)MessageIdentifiers.ID.CONNECTION_LOST, e.Message);
                    }
                }
                Disconnect(0);
            }
        }

        public void Disconnect(int timeout)
        {
            if (_socket.Connected)
            {
                _socket.Shutdown(SocketShutdown.Receive);
                _socket.Close(timeout);
            }
            else
            {
                _socket.Close();
            }
        }

        void ReceiveHeader(System.IAsyncResult ar)
        {
            NetBitStream stream = (NetBitStream)ar.AsyncState;
            try
            {
                int read = _socket.EndReceive(ar);
                if (read < 1)
                {
                    Disconnect(0);
                    PushPacket((ushort)MessageIdentifiers.ID.CONNECTION_LOST, "");
                    return;
                }
                stream.DecodeHeader();
                _socket.BeginReceive(stream.BYTES, NetBitStream.header_length, stream.BodyLength, SocketFlags.None, new System.AsyncCallback(ReceiveBody), stream);

            }
            catch (System.Exception e)
            {
                PushPacket((ushort)MessageIdentifiers.ID.CONNECTION_LOST, e.Message);
                Disconnect(0);
            }
        }

        void ReceiveBody(System.IAsyncResult ar)
        {
            NetBitStream stream = (NetBitStream)ar.AsyncState;
            try
            {
                int read = _socket.EndReceive(ar);
                if (read < 1)
                {
                    Disconnect(0);
                    PushPacket((ushort)MessageIdentifiers.ID.CONNECTION_LOST, "");
                    return;
                }
                PushPacket2(stream);
                _socket.BeginReceive(stream.BYTES, 0, NetBitStream.header_length, SocketFlags.None, new System.AsyncCallback(ReceiveHeader), stream);

            }
            catch (System.Exception e)
            {
                PushPacket((ushort)MessageIdentifiers.ID.CONNECTION_LOST, e.Message);
                Disconnect(0);
            }
        }

        public void Send(NetBitStream bts)
        {
            if (!_socket.Connected)
                return;

            NetworkStream ns;

            lock (_socket)
            {
                ns = new NetworkStream(_socket);
            }

            if (ns.CanWrite)
            {
                try
                {
                    ns.BeginWrite(bts.BYTES, 0, bts.Length, new System.AsyncCallback(SenCallback), ns);

                }
                catch (System.Exception)
                {
                    PushPacket((ushort)MessageIdentifiers.ID.CONNECTION_LOST, "");
                    Disconnect(0);
                }
            }
        }

        private void SendCallback(System.IAsyncResult ar)
        {
            NetworkStream ns = (NetworkStream)ar.AsyncState;
            try
            {
                ns.EndWrite(ar);
                ns.Flush();
                ns.Close();
            }
            catch (System.Exception)
            {
                PushPacket((ushort)MessageIdentifiers.ID.CONNECTION_LOST, "");
                Disconnect(0);
            }
        }
    }
}
