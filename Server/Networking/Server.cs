using Server.Entities;
using Server.Misc;
using Server.Networking;
using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using static Server.Constants;

namespace Server
{
    class Server
    {
        string localAddr = String.Empty;
        Int32 port = -1;

        Thread mainServerThread;
        TcpListener server;
        ConcurrentDictionary<string, ClientHandler> clients = new ConcurrentDictionary<string, ClientHandler>();

        Thread publisherThread;
        public BlockingCollection<Packet> broadcastPackets = new BlockingCollection<Packet>();

        public WorldDataManager worldDataManager;
        public Server(string localAddr, Int32 port)
        {
            this.localAddr = localAddr;
            this.port = port;

            //Used to store information about the game world as well as save the game
            worldDataManager = new WorldDataManager(this);

            //Main server thread, listens for connections
            mainServerThread = new Thread(new ThreadStart(ListenForConnections));
            mainServerThread.Start();

            publisherThread = new Thread(new ThreadStart(PublishPackets));
            publisherThread.Start();
        }

        void ListenForConnections()
        {
            Console.WriteLine("Listening for TCP connections on port:" + port);

            IPAddress addr = IPAddress.Parse(localAddr);
            server = new TcpListener(addr, port);
            server.Start();

            while (true)
            {
                TcpClient client = server.AcceptTcpClient();
                string uid = Guid.NewGuid().ToString();
                ClientHandler handler = new ClientHandler(this, client, uid);
                clients.TryAdd(uid, handler);
                Console.WriteLine("New connection with uid: " + uid);
            }
        }

        public void ClientDisconnected(string uid)
        {
            Console.WriteLine(uid + " disconnected");
            ClientHandler handler;
            clients.TryRemove(uid, out handler);
        }

        public ClientHandler GetClient(string uid)
        {
            ClientHandler handler;
            clients.TryGetValue(uid, out handler);
            return handler;
        }

        void PublishPackets()
        {
            while (true)
            {
                Packet p = broadcastPackets.Take();
                foreach(var client in clients)
                {
                    client.Value.outgoingPackets.Add(p);
                }
            }
        }
    }
}
