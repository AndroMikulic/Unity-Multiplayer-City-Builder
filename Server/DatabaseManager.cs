using Server.Entities;
using Server.Misc;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Text;

namespace Server
{
    class DatabaseManager
    {
        public WorldDataManager wdm;
        public string databaseFile;
        public string databaseLocation;

        public DatabaseManager(WorldDataManager wdm)
        {
            this.wdm = wdm;
            LoadDatabase();
        }

        void LoadDatabase()
        {
            databaseFile = wdm.worldConfig.name + ".db";
            databaseLocation = "URI=file:" + databaseFile;

            //Check if the databse file exsists, if not, create a new one
            if (!File.Exists(databaseFile))
            {
                InitialDatabaseSetup();
                return;
            }
            Console.WriteLine("Found existing world, loading database...");

            using var connection = new SQLiteConnection(databaseLocation);
            connection.Open();
            using var cmd = new SQLiteCommand(connection);

            //Load buildings from the database

            cmd.CommandText = Constants.SQLCommands.loadBuildings;
            SQLiteDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                Building building = new Building();
                building.entityType = EntityType.BUILDING;
                building.location = new Location(rdr.GetInt32(1), rdr.GetInt32(2));
                building.type = rdr.GetInt32(3);
                building.name = rdr.GetString(4);
                building.population = rdr.GetInt32(5);
                building.resource = rdr.GetInt32(6);

                //add the building to the list
                wdm.buildings.TryAdd(building.location, building);
            }
            rdr.Close();

            //Load roads from the database
            cmd.CommandText = Constants.SQLCommands.loadCity;
            rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                RoadTile roadTile = new RoadTile();
                roadTile.entityType = EntityType.ROAD;
                roadTile.location = new Location(rdr.GetInt32(0), rdr.GetInt32(1));

                //add the road tile to the list
                wdm.roadTiles.TryAdd(roadTile.location, roadTile);
            }
            rdr.Close();

            //Load the city from the database
            cmd.CommandText = Constants.SQLCommands.loadRoads;
            rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                City city = new City();
                city.money = rdr.GetInt32(1);
                city.taxes = rdr.GetInt32(2);

                //set the city in the world data manager
                wdm.city = city;
                break;
            }
            rdr.Close();
            connection.Close();
            Console.WriteLine("Loaded world from database");
        }

        void InitialDatabaseSetup()
        {
            Console.WriteLine("No world database found, initializing NEW...");
            using var connection = new SQLiteConnection(databaseLocation);
            connection.Open();
            using var cmd = new SQLiteCommand(connection);
            cmd.CommandText = Constants.SQLCommands.createBuildingsTable;
            cmd.ExecuteNonQuery();
            cmd.CommandText = Constants.SQLCommands.createRoadsTable;
            cmd.ExecuteNonQuery();
            cmd.CommandText = Constants.SQLCommands.createCityTable;
            cmd.ExecuteNonQuery();
            connection.Close();
            Console.WriteLine("Created new database file: " + databaseLocation);
        }

        public void SaveWorld()
        {
            Console.WriteLine("Saving world...");
            using var connection = new SQLiteConnection(databaseLocation);
            connection.Open();
            using var cmd = new SQLiteCommand(connection);

            //Empty the table to resave, not the smartest idea but works for now
            cmd.CommandText = Constants.SQLCommands.emptyBuildingsTable;
            cmd.ExecuteNonQuery();
            cmd.CommandText = Constants.SQLCommands.emptyRoadsTable;
            cmd.ExecuteNonQuery();
            cmd.CommandText = Constants.SQLCommands.emptyCityTable;
            cmd.ExecuteNonQuery();

            foreach (var building in wdm.buildings)
            {
                cmd.CommandText = Constants.SQLCommands.saveBuilding;
                cmd.Parameters.AddWithValue("@x", building.Value.location.x);
                cmd.Parameters.AddWithValue("@y", building.Value.location.y);
                cmd.Parameters.AddWithValue("@type", building.Value.type);
                cmd.Parameters.AddWithValue("@name", building.Value.name);
                cmd.Parameters.AddWithValue("@size", building.Value.size);
                cmd.Parameters.AddWithValue("@population", building.Value.population);
                cmd.Parameters.AddWithValue("@resource", building.Value.resource);
                cmd.Prepare();
                cmd.ExecuteNonQuery();
            }

            foreach (var road in wdm.roadTiles)
            {
                cmd.CommandText = Constants.SQLCommands.saveRoad;
                cmd.Parameters.AddWithValue("@x", road.Value.location.x);
                cmd.Parameters.AddWithValue("@y", road.Value.location.y);
                cmd.Prepare();
                cmd.ExecuteNonQuery();
            }

            cmd.CommandText = Constants.SQLCommands.saveBuilding;
            cmd.Parameters.AddWithValue("@money", wdm.city.money);
            cmd.Parameters.AddWithValue("@taxes", wdm.city.taxes);
            cmd.Prepare();
            cmd.ExecuteNonQuery();

            connection.Close();
            Console.WriteLine("Save complete");
        }
    }
}
