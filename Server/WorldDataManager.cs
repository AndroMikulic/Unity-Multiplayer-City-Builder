using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Threading;
using Newtonsoft.Json.Linq;
using Server.Entities;
using Server.Misc;
using Server.Networking;

namespace Server
{
    class WorldDataManager
    {
        public Server server;

        public dynamic worldConfig;
        DatabaseManager databaseManager;

        public ConcurrentDictionary<Location, Building> buildings = new ConcurrentDictionary<Location, Building>();
        public ConcurrentDictionary<Location, RoadTile> roadTiles = new ConcurrentDictionary<Location, RoadTile>();
        public City city = new City();
        public long[,] tileTimestamp = new long[Constants.Gameplay.WORLD_SIZE, Constants.Gameplay.WORLD_SIZE];

        Thread entityUpdateThread;
        public BlockingCollection<KeyValuePair<string, Packet>> entityUpdateQueue = new BlockingCollection<KeyValuePair<string, Packet>>();

        public WorldDataManager(Server server)
        {
            this.server = server;
            LoadWorldConfig();

            //Used to manage the database
            databaseManager = new DatabaseManager(this);
            InitializeTileTimestamps();

            ShowWorldStatus();

            entityUpdateThread = new Thread(new ThreadStart(OperationQueueHandler));
            entityUpdateThread.Start();
        }

        void ShowWorldStatus()
        {
            Console.WriteLine("--------------------------------");
            Console.WriteLine("City name: " + worldConfig.name);
            Console.WriteLine("Treasury: " + city.money);
            Console.WriteLine("Tax rate: " + city.taxes + "%");
            Console.WriteLine("--------------------------------");
        }

        void LoadWorldConfig()
        {
            Console.WriteLine("Loading world configuration");

            string configFile = Path.Combine(Constants.CurrentDirectory(), Constants.WORLD_CONFIG_FILE);
            if (!File.Exists(configFile))
            {
                Console.WriteLine("A confguration file is needed to set up the server.");
                Environment.Exit(-1);
            }
            string configJSON = File.ReadAllText(configFile);
            worldConfig = JObject.Parse(configJSON);
        }

        void OperationQueueHandler()
        {
            while (true)
            {

                KeyValuePair<string, Packet> kv = entityUpdateQueue.Take();
                Packet p = kv.Value;
                if (p.type.Equals(Constants.Networking.PacketTypes.ENTITY_CREATE))
                {
                    CreateEntity(kv.Key, p.payload);
                }
                else if (p.type.Equals(Constants.Networking.PacketTypes.ENTITY_DELETE))
                {
                    DeleteEntity(kv.Key, p.payload);
                }
            }
        }

        void CreateEntity(string uid, dynamic obj)
        {
            obj.timestamp = DateTime.UtcNow.ToString();
            int price = -1;
            Location location = new Location(-1, -1);
            Building building = new Building();
            RoadTile roadTile = new RoadTile();
            Entity entity = Entity.ParseToEntity(obj);
            dynamic errorReason = new ExpandoObject();
            if (entity.entityType.Equals(EntityType.BUILDING))
            {
                building = Building.ParseToBuilding(obj);
                price = building.size * building.size * Constants.Gameplay.BASE_BUILDING_COST;
                location = building.location;
            }

            if (entity.entityType.Equals(EntityType.ROAD))
            {
                roadTile = RoadTile.ParseToRoadTile(obj);
                price = Constants.Gameplay.ROAD_TILE_COST;
                location = roadTile.location;

            }

            if (city.money - price > 0)
            {
                if (!ValidateLocation(location))
                {
                    if (entity.entityType.Equals(EntityType.BUILDING))
                    {
                        buildings.TryAdd(location, building);
                    }
                    else if (entity.entityType.Equals(EntityType.ROAD))
                    {
                        roadTiles.TryAdd(location, roadTile);
                    }
                    obj.tileTimestamp = UpdateTile(location);
                    Packet okPacket = new Packet(Constants.Networking.PacketTypes.ENTITY_CREATE, obj);
                    server.broadcastPackets.Add(okPacket);
                    return;
                }
                else
                {
                    errorReason.reason = Constants.Networking.PacketTypes.FailReason.ILLEGAL_LOCATION;
                }
            }
            else
            {
                errorReason.reason = Constants.Networking.PacketTypes.FailReason.NO_MONEY;
            }
            Packet errorPacket = new Packet(Constants.Networking.PacketTypes.OPERATION_FAILED, errorReason);
            server.GetClient(uid).outgoingPackets.Add(errorPacket);
        }

        void DeleteEntity(string uid, dynamic obj)
        {

            Location location = Location.ParseToLocation(obj);
            bool success = false;

            if (buildings.ContainsKey(location))
            {
                Building b = new Building();
                buildings.TryRemove(location, out b);
                success = true;
            }
            else if (roadTiles.ContainsKey(location))
            {
                RoadTile r = new RoadTile();
                roadTiles.TryRemove(location, out r);
                success = true;
            }

            if (success)
            {
                Entity e = new Entity();
                e.location = location;
                e.tileTimestamp = UpdateTile(location);
                Packet okPacket = new Packet(Constants.Networking.PacketTypes.ENTITY_DELETE, e);
                server.broadcastPackets.Add(okPacket);
            }
            else
            {
                dynamic errorReason = new ExpandoObject();
                errorReason.reason = Constants.Networking.PacketTypes.FailReason.ILLEGAL_LOCATION;
                Packet errorPacket = new Packet(Constants.Networking.PacketTypes.OPERATION_FAILED, errorReason);
                server.GetClient(uid).outgoingPackets.Add(errorPacket);
            }
        }

        bool ValidateLocation(Location location)
        {
            if (location.x < 0 || location.x > Constants.Gameplay.WORLD_SIZE - 1)
            {
                return false;
            }
            if (location.y < 0 || location.y > Constants.Gameplay.WORLD_SIZE - 1)
            {
                return false;
            }
            if (!buildings.ContainsKey(location))
            {
                if (!roadTiles.ContainsKey(location))
                {
                    return false;
                }
            }
            return true;
        }

        void InitializeTileTimestamps()
        {
            for (int i = 0; i < Constants.Gameplay.WORLD_SIZE; ++i)
            {
                for (int j = 0; j < Constants.Gameplay.WORLD_SIZE; ++j)
                {
                    ++tileTimestamp[i, j];
                }
            }
        }

        public long UpdateTile(Location location)
        {
            int x = location.x;
            int y = location.y;
            ++tileTimestamp[x, y];
            return tileTimestamp[x, y];
        }

        public long GetTileTimestamp(Location location)
        {
            try
            {
                return tileTimestamp[location.x, location.y];
            }
            catch (Exception e)
            {
                Console.WriteLine("Invalid location provided: " + location.x + " - " + location.y);
                return -1;
            }
        }
    }
}
