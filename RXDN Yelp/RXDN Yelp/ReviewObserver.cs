using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VaderSharp;

namespace RXDN_Yelp
{
    public class ReviewObserver : IObserver<Review>
    {
        SentimentIntensityAnalyzer analyzer;
        List<ReviewBias> analyzedReviews;
        public ReviewObserver()
        {
            analyzer = new SentimentIntensityAnalyzer();
            analyzedReviews = new List<ReviewBias>();
        }

        public void OnCompleted()
        {
            Console.WriteLine("Done");
        }

        public void OnError(Exception error)
        {
            Console.WriteLine(error.Message);
        }

        public void OnNext(Review value)
        {
            var bias = analyzer.PolarityScores($"{value.Text} Rating: {value.Rating}");
            analyzedReviews.Add(new ReviewBias(value, bias.Compound, bias.Positive, bias.Negative, bias.Neutral));
            Console.WriteLine($"Restaurant: {value.Restaurant.Name} Review: {value.Text} Rating: {value.Rating} \n" +
                $"Positivity: {(bias.Positive*100).ToString("##.##")}%, Bias: {bias.Compound} \n\n\n");
        }
    }
}
