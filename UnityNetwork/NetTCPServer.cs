using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace UnityNetwork
{
    class NetTCPServer
    {
        public int _maxConnections = 5000;

        public int _sendTimeout = 3;
        public int _revTimeout = 3;

        Socket _listener;

        int _port = 0;

        private NetworkManager _netMgr = null;

        public NetTCPServer()
        {
            _netMgr = NetworkManager.Instance;
        }

        public bool CreateTcpServer(string ip, int listenPort)
        {
            _port = listenPort;
            _listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            foreach (IPAddress address in Dns.GetHostEntry(ip).AddressList)
            {
                try
                {
                    IPAddress hostIP = address;
                    IPEndPoint ipe = new IPEndPoint(address, _port);
                    _listener.Bind(ipe);
                    _listener.Listen(_maxConnections);
                    _listener.BeginAccept(new System.AsyncCallback(ListenTcpClient), _listener);
                    break;
                }
                catch (System.Exception)
                {
                    return false;
                }
            }
            return true;
        }

        void ListenTcpClient(System.IAsyncResult ar)
        {
            NetBitStream stream = new NetBitStream();
            try
            {
                Socket client = _listener.EndAccept(ar);
                stream._socket = client;

                client.SendTimeout = _sendTimeout;
                client.ReceiveTimeout = _revTimeout;

                client.BeginReceive(stream.BYTES, 0, NetBitStream.header_length, SocketFlags.None, new System.AsyncCallback(ReceiveHeader), stream);

                PushPacket((ushort)MessageIdentifiers.ID.NEW_INCOMING_CONNECTION, "", client);
            }
            catch (System.Exception)
            {
                
            }

            _listener.BeginAccept(new System.AsyncCallback(ListenTcpClient), _listener);
        }

        void ReceiveHeader(System.IAsyncResult ar)
        {
            NetBitStream stream = (NetBitStream)ar.AsyncState;
            try
            {
                int read = stream._socket.EndReceive(ar);
                if (read < 1)
                {
                    PushPacket((ushort)MessageIdentifiers.ID.CONNECTION_LOST, "", stream._socket);
                    return;
                }

                stream.DecodeHeader();

                stream._socket.BeginReceive(stream.BYTES, NetBitStream.header_length, stream.BodyLength, SocketFlags.None, new System.AsyncCallback(ReceiveBody), stream);
            }
            catch (System.Exception e)
            {
                PushPacket((ushort)MessageIdentifiers.ID.CONNECTION_LOST, e.Message, stream._socket);
            }
        }

        void ReceiveBody(System.IAsyncResult ar)
        {
            NetBitStream stream = (NetBitStream)ar.AsyncState;
            try
            {
                int read = stream._socket.EndReceive(ar);
                if (read < 1)
                {
                    PushPacket((ushort)MessageIdentifiers.ID.CONNECTION_LOST, "", stream._socket);
                    return;
                }
                PushPacket2(stream);
                stream._socket.BeginReceive(stream.BYTES, 0, NetBitStream.header_length, SocketFlags.None, new System.AsyncCallback(ReceiveHeader), stream);
            }
            catch (System.Exception e)
            {
                PushPacket((ushort)MessageIdentifiers.ID.CONNECTION_LOST, e.Message, stream._socket);
            }
        }

        public void Send(NetBitStream bts, Socket peer)
        {
            NetworkStream ns;
            lock (peer)
            {
                ns = new NetworkStream(peer);
            }
            if (ns.CanWrite)
            {
                try
                {
                    ns.BeginWrite(bts.BYTES, 0, bts.Length, new System.AsyncCallback(SendCallback), ns);
                }
                catch (System.Exception)
                {
                    PushPacket((ushort)MessageIdentifiers.ID.CONNECTION_LOST, "", peer);
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

            }
        }

        void PushPacket(ushort msgid, string exception, Socket peer)
        {
            NetPacket packet = new NetPacket();
            packet.SetIDOnly(msgid);
            packet._error = exception;
            packet._peer = peer;

            _netMgr.AddPacket(packet);
        }

        void PushPacket2(NetBitStream stream)
        {
            NetPacket packet = new NetPacket();
            stream.BYTES.CopyTo(packet._bytes, 0);
            packet._peer = stream._socket;
            _netMgr.AddPacket(packet);
        }
    }
}
