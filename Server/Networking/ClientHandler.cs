using Server.Networking;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Server
{
    class ClientHandler
    {
        Server server;
        TcpClient client;
        public string uid;
        public string clientName;
        public bool initialized = false;

        Thread clientListen;
        Thread clientSend;
        NetworkStream stream;

        public BlockingCollection<Packet> outgoingPackets = new BlockingCollection<Packet>();

        public ClientHandler(Server server, TcpClient client, string uid)
        {
            this.server = server;
            this.client = client;
            this.uid = uid;

            stream = client.GetStream();

            clientSend = new Thread(new ThreadStart(Send));
            clientSend.Start();
            clientListen = new Thread(new ThreadStart(Listen));
            clientListen.Start();
        }

        void Send()
        {
            while (true)
            {
                Packet packet = outgoingPackets.Take();
                byte[] rawData = Encoding.UTF8.GetBytes(packet.json);
                stream.Write(rawData, 0, rawData.Length);
            }
        }

        void Listen()
        {
            while (true)
            {
                Byte[] rawData = new Byte[Constants.Networking.MAX_PACKET_SIZE];
                Int32 bytes = 0;
                try
                {
                    bytes = stream.Read(rawData, 0, rawData.Length);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.StackTrace);
                }

                //Check if bytes == 0, if so, connection was dropped
                if (bytes == 0)
                {
                    server.ClientDisconnected(uid);
                    return;
                }

                String data = String.Empty;
                data = Encoding.UTF8.GetString(rawData, 0, bytes);
                HandlePacket(Packet.Parse(data));
            }
        }

        void HandlePacket(Packet packet)
        {
            if (packet == null)
            {
                return;
            }
            if (initialized)
            {
                if (packet.type.Equals(Constants.Networking.PacketTypes.NEW_CONNECTION))
                {

                }
                return;
            }
            
            KeyValuePair<string, Packet> kv = new KeyValuePair<string, Packet>(this.uid, packet);
            if (packet.type.Equals(Constants.Networking.PacketTypes.ENTITY_CREATE))
            {
                server.worldDataManager.entityUpdateQueue.Add(kv);
            }

            if (packet.type.Equals(Constants.Networking.PacketTypes.ENTITY_DELETE))
            {
                server.worldDataManager.entityUpdateQueue.Add(kv);
            }
        }
    }
}
