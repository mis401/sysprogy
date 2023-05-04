using Microsoft.AspNetCore.Mvc;
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

    public ExcelController(ILogger<ExcelController> logger)
    {
        _logger = logger;
    }

    [HttpGet("GetFile/{name}")]
    public XSSFWorkbook? GetFile(string name)
    {
        string filePath = Path.Combine(rootPath, name);
        if (!Path.Exists(filePath)){
            return null;
        }
        var workbook = new XSSFWorkbook();
        var CSVFiles = Directory.EnumerateFiles(filePath);
        foreach(var file in CSVFiles){
            var sheet = workbook.CreateSheet();
            ParseCSVToSheet(file, sheet);
        }
        
        return null;//kako da vratimo xlsx?
        
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
            List<int> columnWidths = new List<int>(tokens.Length);
            

            csvRow = cs.ReadLine();
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
                if(sheet.GetRow(2).GetCell(i).CellType != CellType.Numeric)
                    sheet.SetColumnWidth(i, columnWidths[i]*256);
            }
        }
    }
}
