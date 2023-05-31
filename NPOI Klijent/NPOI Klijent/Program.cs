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
        for (int i = 0; i < 1000; i++)
        {
            string path = Path.GetFullPath($"{System.AppDomain.CurrentDomain.BaseDirectory}\\..\\..\\..\\..");
            HttpClient client = new HttpClient();
            Console.WriteLine("Unesite port");
            ushort port = 8000;
            while (port == 0)
            {
                try
                {
                    port = ushort.Parse(Console.ReadLine()!);
                }
                catch (FormatException e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            client.BaseAddress = new Uri($"http://localhost:{port}");
            string file = "employees";
            while (file == String.Empty)
            {
                Console.WriteLine("Unesite ime fajla");
                file = Console.ReadLine()!;
            }

            var response = await client.GetAsync($"{client.BaseAddress}/?file={file}");
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine(await response.Content.ReadAsStringAsync());
                return 1;
            }
            Console.WriteLine(response);
            var bytes = await response.Content.ReadAsByteArrayAsync();
            using (var fs = new FileStream($"{path}\\{response.Content.Headers.ContentDisposition!.FileName}", FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                fs.Write(bytes);
            }
        }
        return 0;
    }
}