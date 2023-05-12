using NPOI;
using NPOI.OpenXmlFormats.Dml;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System.Net;
using System.Text;
using System.Xml.Linq;
using System.Runtime.Caching;

namespace ExcelServer_Console;

public class Program
{

    public static void Main(string[] args)
    {
        Console.WriteLine("Startup");
        Server server = new Server(8000, "HTTP Server", "http://localhost");
        string rootPath = "..\\..\\..\\files";
        server.listener.Start();
        while (true)
        {
            try
            {
                HttpListenerContext _context = server.listener.GetContext();
                ThreadPool.QueueUserWorkItem(server.GetFile, _context as object);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                if (ex.InnerException != null)
                {
                    Console.WriteLine(ex.InnerException.Message);
                }
            }
        }
    }

}