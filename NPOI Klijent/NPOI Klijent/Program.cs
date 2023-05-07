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
        client.BaseAddress = new Uri(@"http://localhost:5208");
        var response = await client.GetAsync($"http://localhost:5208/Excel/GetFileMT/employees");
        Console.WriteLine(response);
        var bytes = await response.Content.ReadAsByteArrayAsync();
        using (var fs = new FileStream($"{path}\\httptest.xlsx", FileMode.OpenOrCreate, FileAccess.ReadWrite))
        {
            fs.Write(bytes);
        }
        Stopwatch sw = new Stopwatch();
        return 0;
    }
}