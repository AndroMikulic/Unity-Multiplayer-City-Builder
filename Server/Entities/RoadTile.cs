using Server.Misc;
using System;

namespace Server.Entities
{
    class RoadTile : Entity
    {
        public Location location;

        public static RoadTile ParseToRoadTile(dynamic obj)
        {
            RoadTile tile = new RoadTile();
            try
            {
                tile.entityType = obj.entityType;
                tile.location = new Location(obj.location.x, obj.location.y);
                return tile;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error parsing object to RoadTile");
                return null;
            }
        }
    }
}
