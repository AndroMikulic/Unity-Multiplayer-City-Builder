using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Server.Entities
{
    class Entity
    {
        public EntityType entityType;

        public static Entity ParseToEntity(dynamic obj)
        {
            Entity entiy = new Entity();
            try
            {
                entiy.entityType = obj.entityType;
                return entiy;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error parsing object to Entity");
                return null;
            }
        }
    }

    public enum EntityType
    {
        BUILDING = 1,
        ROAD = 2
    }
}
