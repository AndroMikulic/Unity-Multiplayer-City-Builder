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

        Thread entityUpdateThread;
        public BlockingCollection<KeyValuePair<string, Packet>> entityUpdateQueue = new BlockingCollection<KeyValuePair<string, Packet>>();

        public WorldDataManager(Server server)
        {
            this.server = server;
            LoadWorldConfig();

            //Used to manage the database
            databaseManager = new DatabaseManager(this);

            entityUpdateThread = new Thread(new ThreadStart(OperationQueueHandler));
            entityUpdateThread.Start();
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
            int price = -1;
            Location l = new Location(-1, -1);
            Building building = new Building();
            RoadTile roadTile = new RoadTile();
            Entity entity = Entity.ParseToEntity(obj);
            dynamic errorReason = new ExpandoObject();
            if (entity.entityType.Equals(EntityType.BUILDING))
            {
                building = Building.ParseToBuilding(obj);
                price = building.size * building.size * Constants.Gameplay.BASE_BUILDING_COST;
                l = building.location;
            }

            if (entity.entityType.Equals(EntityType.ROAD))
            {
                roadTile = RoadTile.ParseToRoadTile(obj);
                price = Constants.Gameplay.ROAD_TILE_COST;
                l = roadTile.location;

            }

            if (city.money - price > 0)
            {
                if (!LocationOccupied(l))
                {
                    if (entity.entityType.Equals(EntityType.BUILDING))
                    {
                        buildings.TryAdd(l, building);
                    }
                    else if (entity.entityType.Equals(EntityType.ROAD))
                    {
                        roadTiles.TryAdd(l, roadTile);
                    }
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
            Location location = new Location(obj.x, obj.y);
            bool success = false;
            
            if(buildings.ContainsKey(location))
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
                Packet okPacket = new Packet(Constants.Networking.PacketTypes.ENTITY_DELETE, location);
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

        bool LocationOccupied(Location l)
        {
            if (!buildings.ContainsKey(l))
            {
                if (!roadTiles.ContainsKey(l))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
