using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Text;
using System.Threading.Tasks;
using VaderSharp;

namespace RXDN_Yelp
{
    public class ReviewObserver : IObserver<Review>
    {
        SentimentIntensityAnalyzer analyzer;
        List<ReviewBias> analyzedReviews;
        public int ID { get; set; }
        public ReviewObserver()
        {
            analyzer = new SentimentIntensityAnalyzer();
            analyzedReviews = new List<ReviewBias>();
        }

       public ReviewObserver(int id)
        {
            ID = id;
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
            Thread.Sleep(ID * 1000);
            var bias = analyzer.PolarityScores($"{value.Text} Rating: {value.Rating}");
            analyzedReviews.Add(new ReviewBias(value, bias.Compound, bias.Positive, bias.Negative, bias.Neutral));
            Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId}, observer {ID}");
            
            //Console.WriteLine($"Restaurant: {value.Restaurant.Name} Review: {value.Text} Rating: {value.Rating} \n" +
                //$"Positivity: {(bias.Positive*100).ToString("##.##")}%, Bias: {bias.Compound} \n\n\n");
        }
    }
}
