using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RXDN_Yelp
{
    public class Review
    {
        public string ID { get; set; }
        public Restaurant Restaurant{ get; set; }
        public string Text { get; set; }
        public int Rating { get; set; }

        public Review(string iD, Restaurant restaurant, string text, int rating)
        {
            ID = iD;
            Restaurant = restaurant;
            Text = text;
            Rating = rating;
        }
    }
}
