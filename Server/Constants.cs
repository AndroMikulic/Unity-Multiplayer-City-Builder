using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace Server
{
    static class Constants
    {
        public static string WORLD_CONFIG_FILE = "world-config.json";
        public static int MAX_PACKET_SIZE = 1024; // in bytes

        // The directory of the server .exe file
        public static string CurrentDirectory()
        {
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }

        public static class SQLCommands
        {
            //Initialization of a new database
            public static string createBuildingsTable = @"CREATE TABLE buildings(id INTEGER PRIMARY KEY, x INTEGER, y INTEGER, type INTEGER, name STRING, size INTEGER, population INTEGER, resource INTEGER)";
            public static string createRoadsTable = @"CREATE TABLE roads(x INTEGER, y INTEGER)";
            public static string createCityTable = @"CREATE TABLE city(id INTEGER PRIMARY KEY, money INTEGER, taxes INTEGER)";

            //Loading from an existing database
            public static string loadBuildings = @"SELECT * FROM buildings";
            public static string loadRoads = @"SELECT * FROM roads";
            public static string loadCity = @"SELECT * FROM city";

            //World saving commands
            public static string saveBuilding = "INSERT INTO buildings(id, x, y, type, name, size, population, resource) VALUES(@id, @x, @y, @type, @name, @size, @population, @resource)";
            public static string saveRoad = "INSERT INTO roads(x, y) VALUES(@x, @y)";
            public static string saveCity = "INSERT INTO city(id, money, taxes) VALUES(@id, @money, @taxes)";

            //Empty table contents
            public static string emptyBuildingsTable = "DELETE FROM buildings";
            public static string emptyRoadsTable = "DELETE FROM roads";
            public static string emptyCityTable = "DELETE FROM city";
        }
    }
}
