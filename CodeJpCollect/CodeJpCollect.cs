using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NPOI.HSSF.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.SS.UserModel;
using System.IO;
using System.Text.RegularExpressions;

namespace Getjp
{
    public static class ExcelExtension
    {
        public static string upath(this string self)
        {

            return self.Trim()
                .TrimEnd()
                .Replace("\\", "/")
                .Replace("//", "/");
        }

        public static string SValue(this ICell cell, CellType? FormulaResultType = null)
        {
            string svalue = "";
            var cellType = FormulaResultType ?? cell.CellType;
            switch(cellType)
            {
            case CellType.Unknown:
                svalue = "nil";
                break;
            case CellType.Numeric:
                svalue = cell.NumericCellValue.ToString();
                break;
            case CellType.String:
                svalue = "\"" + cell.StringCellValue
                    .Replace("\n", "\\n")
                    .Replace("\t", "\\t")
                    .Replace("\"", "\\\"") + "\"";
                break;
            case CellType.Formula:
                svalue = cell.SValue(cell.CachedFormulaResultType);
                break;
            case CellType.Blank:
                svalue = "nil";
                break;
            case CellType.Boolean:
                svalue = cell.BooleanCellValue.ToString();
                break;
            case CellType.Error:
                svalue = "nil";
                break;
            default:
                break;
            }
            return svalue;
        }


        public static ISheet Sheet(this IWorkbook workbook, string name)
        {
            return workbook.GetSheet(name) ?? workbook.CreateSheet(name);
        }
        public static IRow Row(this ISheet sheet, int i)
        {
            return sheet.GetRow(i) ?? sheet.CreateRow(i);
        }
        public static ICell Cell(this IRow row, int i)
        {
            return row.GetCell(i) ?? row.CreateCell(i);
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("输入查找路径:");
            var inputdir = Console.ReadLine() ?? "D:/a3/client/Unity/Assets/Application/Editor";
            var stringCount = 0;

            var workbook = new XSSFWorkbook();//创建Workbook对象
            var sheet = workbook.Sheet("Sheet");//创建工作表
            var row = sheet.Row(0);
            row.Cell(0).SetCellValue("string");
            row.Cell(1).SetCellValue("line");
            row.Cell(2).SetCellValue("path");
            row.Cell(3).SetCellValue("type");

            var regular = "[^\x00-\xff「」（）【】■～…]";

            var textName = inputdir + "/" + inputdir.Substring(inputdir.LastIndexOf(Path.DirectorySeparatorChar) + 1) + ".txt";
            var textStream = new StreamWriter(textName, false, Encoding.UTF8);
            foreach(var f in Directory.GetFiles(inputdir, "*.*", SearchOption.AllDirectories)
                .Where(f => f.EndsWith(".cs") || f.EndsWith(".php") || f.EndsWith(".js") || f.EndsWith(".java") || f.EndsWith(".prefab")))
            {
                var lineCount = 0;
                var stream = new StreamReader(f);
                while(stream.Peek() > 0)
                {
                    var line = stream.ReadLine();
                    ++lineCount;

                    var commLine = Regex.Matches(line, "^\\s*//*.*");
                    if(commLine.Count > 0)
                    {
                        //Console.WriteLine(line);
                        continue;
                    }

                    var matches = Regex.Matches(line, "\""+ regular + "*" + regular + "+[^\"]*\"");
                    foreach(var i in matches)
                    {
                        ++stringCount;
                        row = sheet.Row(stringCount);
                        var s0 = i.ToString().TrimStart(new char[] { '\'' }).TrimEnd(new char[] { '\'' });
                        row.Cell(0).SetCellValue(s0);
                        row.Cell(1).SetCellValue(lineCount);
                        row.Cell(2).SetCellValue(f.upath());
                        row.Cell(3).SetCellValue(Path.GetExtension(f));
                        var s = string.Format("{0}#{1}#{2}\n", f.upath(), lineCount, s0);
                        textStream.Write(s);
                        //Console.WriteLine("{0}:{1}:{2}", stringCount, lineCount, i.ToString());
                    }
                    var matches2 = Regex.Matches(line, "'" + regular + "*" + regular + "+[^']*'");
                    foreach(var i in matches2)
                    {
                        ++stringCount;
                        row = sheet.Row(stringCount);
                        var s0 = i.ToString().TrimStart(new char[] { '\'' }).TrimEnd(new char[] { '\'' });
                        row.Cell(0).SetCellValue(s0);
                        row.Cell(1).SetCellValue(lineCount);
                        row.Cell(2).SetCellValue(f);
                        row.Cell(3).SetCellValue(Path.GetExtension(f));
                        var s = string.Format("{0}#{1}#{2}\n", f.upath(), lineCount, s0);
                        textStream.Write(s);
                        //Console.WriteLine("{0}:{1}:{2}", stringCount, lineCount, i.ToString());
                    }
                }//while
                textStream.Flush();
            }// foreach
            textStream.Close();

            var excelName = inputdir + "/" + inputdir.Substring(inputdir.LastIndexOf(Path.DirectorySeparatorChar) + 1) + ".xlsx";
            var excelStream = new FileStream(excelName, FileMode.OpenOrCreate);
            excelStream.Position = 0;
            workbook.Write(excelStream);
            excelStream.Close();

            Console.WriteLine("按 Enter 退出");
            Console.ReadLine();
        }
    }
}
