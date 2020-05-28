using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;
using Server.Entities;

namespace Server
{
    class WorldDataManager
    {
        public Server server;
        
        public dynamic worldConfig;
        DatabaseManager databaseManager;

        public ConcurrentBag<Building> buildings = new ConcurrentBag<Building>();
        public ConcurrentBag<RoadTile> roadTiles = new ConcurrentBag<RoadTile>();
        public City city = new City();

        public WorldDataManager(Server server)
        {
            this.server = server;
            LoadWorldConfig();

            //Used to manage the database
            databaseManager = new DatabaseManager(this);
        }

        void LoadWorldConfig()
        {
            Console.WriteLine("Loading world configuration");

            string configFile = Path.Combine(Constants.CurrentDirectory(), Constants.WORLD_CONFIG_FILE);
            if(!File.Exists(configFile))
            {
                Console.WriteLine("A confguration file is needed to set up the server.");
                Environment.Exit(-1);
            }
            string configJSON = File.ReadAllText(configFile);
            worldConfig = JObject.Parse(configJSON);
        }
    }
}
