using System.IO;
using System.Reflection;

namespace Server
{
    static class Constants
    {
        public static string WORLD_CONFIG_FILE = "world-config.json";

        public static class Networking
        {
            public static int MAX_PACKET_SIZE = 256; // in bytes

            public static class PacketTypes
            {
                public static string WORLD_INIT = "WORLD_INIT";
                public static string WORLD_INIT_DONE = "WORLD_INIT_DONE";
                public static string ENTITY_DELETE = "ENTITY_DELETE";
                public static string ENTITY_CREATE = "ENTITY_CREATE";
                public static string OPERATION_FAILED = "OPERATION_FAILED";

                public static class FailReason
                {
                    public static string NO_MONEY = "NO_MONEY";
                    public static string ILLEGAL_LOCATION = "ILLEGAL_LOCATION";
                }
            }
        }
        public static class Gameplay
        {
            public enum BuildingType
            {
                RESIDENTIAL = 1,
                COMMERCIAL = 2,
                INDUSTRIAL = 3,
                MISC = 4
            }
            public static int WORLD_SIZE = 512;
            public static int BASE_BUILDING_COST = 100;
            public static int ROAD_TILE_COST = 10;
        }


        // The directory of the server .exe file
        public static string CurrentDirectory()
        {
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }

        public static class SQLCommands
        {
            //Initialization of a new database
            public static string createBuildingsTable = @"CREATE TABLE buildings(x INTEGER, y INTEGER, type INTEGER, name STRING, size INTEGER, population INTEGER, resource INTEGER)";
            public static string createRoadsTable = @"CREATE TABLE roads(x INTEGER, y INTEGER)";
            public static string createCityTable = @"CREATE TABLE city(money INTEGER, taxes INTEGER)";

            //Loading from an existing database
            public static string loadBuildings = @"SELECT * FROM buildings";
            public static string loadRoads = @"SELECT * FROM roads";
            public static string loadCity = @"SELECT * FROM city";

            //World saving commands
            public static string saveBuilding = "INSERT INTO buildings(x, y, type, name, size, population, resource) VALUES(@x, @y, @type, @name, @size, @population, @resource)";
            public static string saveRoad = "INSERT INTO roads(x, y) VALUES(@x, @y)";
            public static string saveCity = "INSERT INTO city(money, taxes) VALUES(@money, @taxes)";

            //Empty table contents
            public static string emptyBuildingsTable = "DELETE FROM buildings";
            public static string emptyRoadsTable = "DELETE FROM roads";
            public static string emptyCityTable = "DELETE FROM city";
        }
    }
}
