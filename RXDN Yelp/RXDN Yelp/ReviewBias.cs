using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RXDN_Yelp
{
    public class ReviewBias
    {
        public Review Review { get; set; }
        public double Bias { get; set; }
        public double Positive { get; set; }
        public double Negative { get; set; }
        public double Neutral { get; set; }
        public ReviewBias(Review review, double bias, double positive, double negative, double neutral)
        {
            Review = review;
            Bias = bias;
            Positive = positive;
            Negative = negative;
            Neutral = neutral;
        }
    }
}
