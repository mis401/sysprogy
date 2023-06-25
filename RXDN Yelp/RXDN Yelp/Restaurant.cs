using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RXDN_Yelp
{
    public class Restaurant
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public int ReviewCount { get; set; }

        public Restaurant(string id, string name, int reviewCount)
        {
            ID = id;
            Name = name;
            ReviewCount = reviewCount;
        }
    }
}
