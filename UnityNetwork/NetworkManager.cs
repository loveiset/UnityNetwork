using System.Collections.Generic;
using System.Text;

namespace UnityNetwork
{
    class NetworkManager
    {
        protected static NetworkManager _instance = null;

        public static NetworkManager Instance
        {
            get
            {
                return _instance;
            }
        }

        public NetworkManager()
        {
            _instance = this;
        }

        private static System.Collections.Queue Packets = new System.Collections.Queue();
        public int PacketSize
        {
            get
            {
                return Packets.Count;
            }
        }

        public NetPacket AddPacket(NetPacket packet)
        {
            if (Packets.Count == 0)
                return null;

            return (NetPacket)Packets.Dequeue();
        }

        public virtual void Update()
        {

        }
    }
}
