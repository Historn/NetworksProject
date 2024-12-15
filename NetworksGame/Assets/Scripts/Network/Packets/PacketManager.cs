using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace HyperStrike
{
    public class PacketManager
    {
        private Dictionary<int, Func<Packet>> packetTypes = new Dictionary<int, Func<Packet>>();
        private Dictionary<int, Action<Packet, EndPoint>> packetHandlers = new Dictionary<int, Action<Packet, EndPoint>>();

        public PacketManager(Socket socket)
        {
            //this.socket = socket;
        }

        public void RegisterPacket<T>(int packetType, Func<T> createPacket) where T : Packet, new()
        {
            packetTypes[packetType] = createPacket;
        }

        public void RegisterHandler(int packetType, Action<Packet, EndPoint> handler)
        {
            packetHandlers[packetType] = handler;
        }

        //public void SendPacket(Packet packet, EndPoint endpoint)
        //{
        //    byte[] data = packet.Serialize();
        //    socket.SendTo(data, endpoint);
        //}

        //public void Listen()
        //{
        //    byte[] buffer = new byte[1024];
        //    EndPoint sender = new IPEndPoint(IPAddress.Any, 0);

        //    while (true)
        //    {
        //        int receivedBytes = socket.ReceiveFrom(buffer, ref sender);
        //        byte[] packetData = new byte[receivedBytes];
        //        Array.Copy(buffer, packetData, receivedBytes);

        //        HandlePacket(packetData, sender);
        //    }
        //}

        //private void HandlePacket(byte[] data, EndPoint sender)
        //{
        //    int packetType = BitConverter.ToInt32(data, 0);
        //    if (packetTypes.TryGetValue(packetType, out var createPacket))
        //    {
        //        Packet packet = createPacket();
        //        packet.Deserialize(data);

        //        if (packetHandlers.TryGetValue(packetType, out var handler))
        //        {
        //            handler(packet, sender);
        //        }
        //    }
        //}
    }
}
