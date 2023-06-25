

using RXDN_Yelp;

public class Program
{
    public static void Main()
    {
        var server = new Server();
        var observer = new ReviewObserver();
        string location;
        var sub = server.Subscribe(observer);
        Console.WriteLine("Unesite lokaciju: ");
        location = Console.ReadLine();
        server.SearchByLocation(location);
        Console.ReadLine();
        Console.ReadKey();
        sub.Dispose();
    }
}