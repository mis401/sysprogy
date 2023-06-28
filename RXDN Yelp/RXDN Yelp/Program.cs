

using RXDN_Yelp;

public class Program
{
    public static void Main()
    {
        var server = new Server();
        var observers = new List<ReviewObserver>();
        for (int i = 1; i < 3; i++)
        {
            observers.Add(new ReviewObserver(i));
            server.Subscribe(observers[i-1]);
        }
        string location;
        Console.WriteLine("Unesite lokaciju: ");
        location = Console.ReadLine();
        server.SearchByLocation(location);
        Console.ReadLine();
    }
}