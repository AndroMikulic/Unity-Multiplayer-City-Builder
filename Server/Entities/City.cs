using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Entities
{
    class City
    {
        public int id;
        public int taxes;
        public int money;

        public City()
        {
            id = 0;
            taxes = 10;
            money = 42000;
        }
    }
}
