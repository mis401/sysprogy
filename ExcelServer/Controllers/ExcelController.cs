using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using NPOI;
using NPOI.OpenXmlFormats.Dml;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;


namespace ExcelServer.Controllers;



[ApiController]
[Route("[controller]")]
public class ExcelController : ControllerBase
{
    string rootPath = Path.GetFullPath($"{System.AppDomain.CurrentDomain.BaseDirectory}\\..\\..\\..\\files");
    private readonly ILogger<ExcelController> _logger;

    public static object workbookLocker = new object();
    private IMemoryCache _cache;

    public IMemoryCache? Cache { get {return _cache;}}
    public ExcelController(ILogger<ExcelController> logger, IMemoryCache cache)
    {
        _logger = logger;
        _cache = cache;
    }


 [HttpGet("GetFileMT/{name}")]
    public async Task<ActionResult> GetFileMT(string name)
    {

        System.Diagnostics.Stopwatch sw = new();
        sw.Restart();
        var cacheResult = GetMemoryStream(name);
        if (cacheResult != null) {
            Console.WriteLine($"Proteklo je {sw.ElapsedMilliseconds}ms, {sw.ElapsedTicks} tikova, sa cache hitom");
            return File(cacheResult.ToArray(), "application/octet-stream", $"{name}.xlsx");
        }
        string filePath = Path.Combine(rootPath, name);

        if (!Path.Exists(filePath)){
            return BadRequest("No file");
        }

        var workbook = new XSSFWorkbook();
        var CSVFiles = Directory.GetFiles(filePath);
        Barrier b = new Barrier(CSVFiles.Length+1);
        foreach(var file in CSVFiles){
            if (!file.Contains(".csv"))
                continue;
            var sheet = workbook.CreateSheet();
            object[] args = new object[] {file, sheet, b};
            ThreadPool.QueueUserWorkItem(ParseCSVToSheet, args, true);
        }
        string fullPath = Path.Combine(filePath, $"{name}.xlsx");
        b.SignalAndWait();
        var ms = new MemoryStream();
        workbook.Write(ms);
        var res = GetOrAddIntoCache(name, ms);
        Console.WriteLine($"Multi thread funkcija je trajala: {sw.ElapsedMilliseconds}ms");
        return File(ms.ToArray(), "application/octet-stream", $"{name}.xlsx");
    }

    [HttpGet("GetFileST/{name}")]
    public async Task<ActionResult> GetFileST(string name)
    {
        System.Diagnostics.Stopwatch sw = new();
        sw.Restart();
        string filePath = Path.Combine(rootPath, name);

        if (!Path.Exists(filePath)){
            return BadRequest("No file");
        }

        var workbook = new XSSFWorkbook();
        var CSVFiles = Directory.GetFiles(filePath);
        foreach(var file in CSVFiles){
            if (!file.Contains(".csv"))
                continue;
            var sheet = workbook.CreateSheet();
            ParseCSVToSheet(file, sheet);
        }
        string fullPath = Path.Combine(filePath, $"{name}.xlsx");
        using (var ms = new MemoryStream()){
            workbook.Write(ms);
            Console.WriteLine($"Single thread funkcija je trajala: {sw.ElapsedMilliseconds}ms");
            sw.Stop();
            return File(ms.ToArray(), "application/octet-stream", $"{name}.xlsx");
        }

    }

    void ParseCSVToSheet(object? state){
        var args = state as object[];
        var file = args![0] as string;
        var sheet = args[1] as ISheet;
        var b = args[2] as Barrier;
        try{
        using (var cs = new StreamReader($"{file}"))
        {
            if (cs.EndOfStream)
                return;
            string? csvRow;
            string[] tokens;
            csvRow = cs.ReadLine();
            tokens = csvRow!.Split(',');
            var headerRow = sheet!.CreateRow(sheet.PhysicalNumberOfRows);
            int col = 0;
            CellType type;
            int ivalue;
            float fvalue;
            bool bvalue;
            ICell cell;
            List<int> columnWidths = new List<int>(capacity: tokens.Length);
            tokens = csvRow!.Split(',');
            foreach(var token in tokens)
            {
                cell = headerRow.CreateCell(col);
                lock(workbookLocker) 
                {
                cell.SetCellValue(token);
                }
                columnWidths.Add(0);
                ++col;
            }
            while (!cs.EndOfStream)
            {
                csvRow = cs.ReadLine();
                tokens = csvRow!.Split(',');
                var row = sheet.CreateRow(sheet.PhysicalNumberOfRows);
                col = 0;
                
                foreach(var token in tokens )
                {
                    if(int.TryParse(token, out ivalue)){
                        type = CellType.Numeric;
                        cell = row.CreateCell(col, type);
                        lock(workbookLocker) 
                        {
                            cell.SetCellValue(ivalue);
                        }
                        columnWidths.Add(0);
                    }
                    else if(float.TryParse(token, out fvalue))
                    {
                        type = CellType.Numeric;
                        cell = row.CreateCell(col, type);
                        lock(workbookLocker) 
                        {
                            cell.SetCellValue(fvalue);
                        }
                        columnWidths.Add(0);
                    }
                    else if(bool.TryParse(token, out bvalue))
                    {
                        type = CellType.Boolean;
                        cell = row.CreateCell(col, type);
                        lock(workbookLocker) 
                        {
                            cell.SetCellValue(bvalue);
                        }
                        columnWidths.Add(0);
                    }
                    else
                    {
                        type = CellType.String;
                        cell = row.CreateCell(col, type);
                        lock(workbookLocker) 
                        {
                            cell.SetCellValue(token);
                        }
                        columnWidths.Add(0);
                    }
                    if (token.Length > columnWidths[col])
                        columnWidths[col] = token.Length;
                    ++col;
                }
                
            }
            for (int i = 0; i < col; i++)
            {
                    sheet.SetColumnWidth(i, (columnWidths[i]+1)*256);
            }
        }
        }
        catch(Exception e){
            Console.WriteLine(e.InnerException);
        }
        b.SignalAndWait();
    }
    void ParseCSVToSheet(string file, ISheet sheet){
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
            foreach(var token in tokens)
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
                foreach(var token in tokens )
                {
                    if(int.TryParse(token, out ivalue)){
                        type = CellType.Numeric;
                        cell = row.CreateCell(col, type);
                        cell.SetCellValue(ivalue);
                    }
                    else if(float.TryParse(token, out fvalue))
                    {
                        type = CellType.Numeric;
                        cell = row.CreateCell(col, type);
                        cell.SetCellValue(fvalue);
                    }
                    else if(bool.TryParse(token, out bvalue))
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
                    sheet.SetColumnWidth(i, (columnWidths[i]+1)*256);
            }
        }
        
    }

    private MemoryStream? GetOrAddIntoCache(string name, MemoryStream ms){
        MemoryStream? value;
        if (!Cache!.TryGetValue(name, out value)){
            value = ms;
            var options = new Microsoft.Extensions.Caching.Memory.MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromDays(1))
            .SetSlidingExpiration(TimeSpan.FromSeconds(15))
            .RegisterPostEvictionCallback(PostEviction);
            Cache!.Set(name, value, options);
        }
        return value;
    }

    private MemoryStream? GetMemoryStream(string name)
    {
        MemoryStream? ms = null;
        Cache!.TryGetValue(name, out ms);
        return ms;
    }
    public static void PostEviction
    (object cacheKey, object? cacheValue, EvictionReason evictionReason, object? state)
    {
        var stream = cacheValue as MemoryStream;
        stream!.Dispose();
        Console.WriteLine($"Obrisan je fajl {cacheKey} iz kesa");
        return;
    }

}
