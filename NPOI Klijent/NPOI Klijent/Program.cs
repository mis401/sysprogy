using NPOI;
using NPOI.OpenXmlFormats.Dml;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System.Diagnostics;
using System.Security.Cryptography;

public class Program
{
    public static async Task<int> Main(string[] args)
    {
        string path = Path.GetFullPath($"{System.AppDomain.CurrentDomain.BaseDirectory}\\..\\..\\..\\..");
        HttpClient client = new HttpClient();
        Console.WriteLine("Unesite port");
        ushort port = ushort.Parse(Console.ReadLine());
        client.BaseAddress = new Uri($"http://localhost:{port}");
        string file = null;
        while (file == null)
        { 
            Console.WriteLine("Unesite ime fajla");
            file = Console.ReadLine();
        }
        var response = await client.GetAsync($"{client.BaseAddress}/?file={file}"); ;
        Console.WriteLine(response);
        var bytes = await response.Content.ReadAsByteArrayAsync();
        using (var fs = new FileStream($"{path}\\{response.Content.Headers.ContentDisposition.FileName}", FileMode.OpenOrCreate, FileAccess.ReadWrite))
        {
            fs.Write(bytes);
        }
        Stopwatch sw = new Stopwatch();
        return 0;
    }
}