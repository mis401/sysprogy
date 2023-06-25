using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Headers;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace RXDN_Yelp
{
    public class Server: IObservable<Review>
    {
        const string API_KEY = "4qzKOZiyCokzCe6uKH0xWPTNWV4soBuEIeTcqrTgTVYcYZlImfk6QBr9JutRfKod5VGnZV_1_BvTMZswsjGhLRhqc0hVSP5efB8wOhEMZ28Q84xiZKHAI1kj3SeXZHYx";

        HttpClient client;

        public Subject<Review> reviewStream;

        const string BASE_URL = "https://api.yelp.com/v3";
        const string locationSearchURL = $"{BASE_URL}/businesses/search?location=";

        public Server()
        {
            client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", API_KEY);
            reviewStream = new Subject<Review>();
        }

        public void SearchByLocation(string location)
        {
            client.GetAsync($"{locationSearchURL}{location}").ContinueWith(async (task) =>
            {
                try
                {
                    var response = task.Result;
                    response.EnsureSuccessStatusCode();
                    var unparsedContent = await response.Content.ReadAsStringAsync();
                    var content = JObject.Parse(unparsedContent);
                    var restaurants = JsonConvert.DeserializeObject<List<Restaurant>>(content["businesses"].ToString());
                    foreach (var restaurant in restaurants)
                    {
                        await client.GetAsync($"{BASE_URL}/businesses/{restaurant.ID}/reviews").ContinueWith(async (reviewTask) =>
                        {
                            try
                            {
                                HttpResponseMessage reviewResponse = reviewTask.Result;
                                reviewResponse.EnsureSuccessStatusCode();
                                var unparsedreviewContent = await reviewResponse.Content.ReadAsStringAsync();
                                var reviewContent = JObject.Parse(unparsedreviewContent);
                                var reviews = JsonConvert.DeserializeObject<List<Review>>(reviewContent["reviews"].ToString());
                                foreach (var review in reviews)
                                {
                                    var reviewObj = new Review(review.ID, 
                                        new Restaurant(restaurant.ID, restaurant.Name, restaurant.ReviewCount), 
                                        review.Text, 
                                        review.Rating);
                                    reviewStream.OnNext(reviewObj);
                                }
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("Review " + e.Message);
                                reviewStream.OnError(e);
                            }
                        });
                    }
                    reviewStream.OnCompleted();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    reviewStream.OnError(e);
                }
            });
        }

        public IDisposable Subscribe(IObserver<Review> observer)
        {
            return reviewStream.Subscribe(observer); 
        }
    }
}
