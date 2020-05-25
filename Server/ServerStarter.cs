using System;

namespace Server
{
    class ServerStarter
    {
        //These should be read from a config file in the future
        static string localAddr = "127.0.0.1";
        static Int32 port = 1337;
        static void Main(string[] args)
        {
            Server server = new Server(localAddr, port);
        }
    }
}
