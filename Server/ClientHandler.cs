using System;
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
        string uid;

        Thread clientThread;
        NetworkStream stream;
        Byte[] rawData;

        public ClientHandler(Server server, TcpClient client, string uid)
        {
            this.server = server;
            this.client = client;
            this.uid = uid;
            clientThread = new Thread(new ThreadStart(Listen));
            clientThread.Start();
        }

        void Listen()
        {
            stream = client.GetStream();
            while(true)
            {
                rawData = new Byte[Constants.MAX_PACKET_SIZE];
                String data = String.Empty;
                Int32 bytes = stream.Read(rawData, 0, rawData.Length);

                //Check if bytes == 0, if so, connection was dropped
                if(bytes == 0)
                {
                    server.ClientDisconnected(uid);
                    return;
                }
                data = Encoding.ASCII.GetString(rawData, 0, bytes);

                //Debug
                Console.WriteLine(data);
            }
        }
    }
}
