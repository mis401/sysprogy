using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;

namespace ExcelServer_Console;

public class Server
{
    public ushort ServerPort { get; }
    public string ServerName { get; }

    public string BaseURL { get; }

    public HttpListener listener { get; }

    private MemoryCache Cache { get; }
    private const string rootPath = "..\\..\\..\\files";
    public Server(ushort serverPort, string serverName, string baseURL)
    {
        ServerPort = serverPort;
        ServerName = serverName;
        BaseURL = baseURL;
        listener = new HttpListener();
        listener.Prefixes.Add($"{BaseURL}:{ServerPort}/");
        Cache = new MemoryCache("cache");
    }


    public void GetFile(object? _context)
    {

        HttpListenerContext? context = _context as HttpListenerContext;
        HttpListenerRequest req = context!.Request;
        if (!req.HttpMethod.ToUpper().Equals("GET"))
        {
            SendBadRequest(context, "Not GET");
            return;
        }
        else
        {
            var fileName = req.QueryString["file"];
            if (fileName == null || fileName == String.Empty)
            {
                SendBadRequest(context, "Los query, koristi .../?file=<ime_fajla>");
                return;
            }
            string filePath = Path.Combine(rootPath, fileName);
            if (!Directory.Exists(filePath))
            {
                SendBadRequest(context, $"Ne postoji fajl {fileName}");
                return;
            }
            System.Diagnostics.Stopwatch sw = new();
            sw.Restart();
            var ms = GetMemoryStream(fileName);
            if (ms == null)
            {
                Console.WriteLine($"Cache promasaj za fajl {fileName}");
                var workbook = new XSSFWorkbook();
                var CSVFiles = Directory.GetFiles(filePath);
                foreach (var file in CSVFiles)
                {
                    if (!file.Contains(".csv"))
                        continue;
                    var sheet = workbook.CreateSheet();
                    ParseCSVToSheet(file, sheet);
                }
                string fullPath = Path.Combine(filePath, $"{fileName}.xlsx");
                ms = new MemoryStream();
                AddIntoCache(fileName, ms);
                workbook.Write(ms);
                Console.WriteLine($"Fajl pripremljen za {sw.ElapsedMilliseconds}ms");
            }
            else
            {  
                Console.WriteLine($"Fajl povucen iz kesa za {sw.Elapsed.TotalNanoseconds}ns");
            }
            context.Response.ContentLength64 = ms.ToArray().Length;
            context.Response.StatusCode = 201;
            context.Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            context.Response.AddHeader("Content-Disposition", $"attachment; filename={fileName}.xlsx");
            context.Response.OutputStream.Write(ms.ToArray());
            context.Response.Close();
        }

    }
    private void ParseCSVToSheet(string file, ISheet sheet)
    {
        using (var cs = new StreamReader($"{file}"))
        {
            if (cs.EndOfStream)
                return;
            string? csvRow;
            string[] tokens;
            csvRow = cs.ReadLine();
            tokens = csvRow!.Split(',');
            var headerRow = sheet.CreateRow(sheet.PhysicalNumberOfRows);
            int col = 0;
            CellType type;
            int ivalue;
            float fvalue;
            bool bvalue;
            ICell cell;
            List<int> columnWidths = new List<int>(capacity: tokens.Length);

            tokens = csvRow!.Split(',');
            foreach (var token in tokens)
            {
                cell = headerRow.CreateCell(col);
                cell.SetCellValue(token);
                columnWidths.Add(0);
                ++col;
            }
            while (!cs.EndOfStream)
            {
                csvRow = cs.ReadLine();
                tokens = csvRow!.Split(',');
                var row = sheet.CreateRow(sheet.PhysicalNumberOfRows);
                col = 0;
                foreach (var token in tokens)
                {
                    if (int.TryParse(token, out ivalue))
                    {
                        type = CellType.Numeric;
                        cell = row.CreateCell(col, type);
                        cell.SetCellValue(ivalue);
                    }
                    else if (float.TryParse(token, out fvalue))
                    {
                        type = CellType.Numeric;
                        cell = row.CreateCell(col, type);
                        cell.SetCellValue(fvalue);
                    }
                    else if (bool.TryParse(token, out bvalue))
                    {
                        type = CellType.Boolean;
                        cell = row.CreateCell(col, type);
                        cell.SetCellValue(bvalue);
                    }
                    else
                    {
                        type = CellType.String;
                        cell = row.CreateCell(col, type);
                        cell.SetCellValue(token);
                    }
                    if (token.Length > columnWidths[col])
                        columnWidths[col] = token.Length;
                    ++col;
                }

            }
            for (int i = 0; i < col; i++)
            {
                sheet.SetColumnWidth(i, (columnWidths[i] + 1) * 256);
            }
        }

    }

    private void SendBadRequest(HttpListenerContext context, string msg)
    {
        string badRequest = msg;
        Console.WriteLine(msg);
        byte[] BadRequest = Encoding.UTF8.GetBytes(badRequest);
        context.Response.ContentLength64 = BadRequest.Length;
        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
        context.Response.ContentType = "text/plain";
        context.Response.OutputStream.Write(BadRequest);
        context.Response.OutputStream.Close();
    }


    private MemoryStream? AddIntoCache(string name, MemoryStream ms)
    {
        MemoryStream? value;
        value = ms;
        var options = new CacheItemPolicy();
        options.AbsoluteExpiration = DateTimeOffset.Now.AddSeconds(10);
        options.RemovedCallback = PostEviction;
        Cache.Add(name, ms, options);
        return value;
    }

    private MemoryStream? GetMemoryStream(string name)
    {

        MemoryStream? ms = Cache!.Get(name) as MemoryStream;
        return ms;

    }
    private void PostEviction
    (object _state)
    {
        var state = _state as CacheEntryRemovedArguments;
        var item = state!.CacheItem;
        var stream = item.Value as MemoryStream;
        stream!.Dispose();
        Console.WriteLine($"Obrisan je fajl {item.Key} iz kesa");
        return;
    }

}
