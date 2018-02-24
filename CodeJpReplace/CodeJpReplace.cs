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

namespace replace
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
                svalue = "";
                break;
            case CellType.Numeric:
                svalue = cell.NumericCellValue.ToString();
                break;
            case CellType.String:
                svalue = cell.StringCellValue;
                break;
            case CellType.Formula:
                svalue = cell.SValue(cell.CachedFormulaResultType);
                break;
            case CellType.Blank:
                svalue = "";
                break;
            case CellType.Boolean:
                svalue = cell.BooleanCellValue.ToString();
                break;
            case CellType.Error:
                svalue = "";
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
            Console.WriteLine("请输入翻译 Excel 文档:");
            var inputdir = Console.ReadLine();

            var excelName = inputdir.TrimStart(new char[] {'"'}).TrimEnd(new char[] { '"' });// inputdir + "/" + inputdir.Substring(inputdir.LastIndexOf(Path.DirectorySeparatorChar) + 1) + ".xlsx";
            var excelStream = new FileStream(excelName, FileMode.Open);
            var workbook = new XSSFWorkbook(excelStream);//创建Workbook对象
            var sheet = workbook.GetSheet("Sheet");
            if(sheet == null)
            {
                Console.WriteLine("翻译内容工作表名必须是 [Sheet], 原文在第 1 列, 译文在第 2 列, 文件名在第 4 列");
                return;
            }
            var row = sheet.Row(0);

            IRow headerRow = sheet.GetRow(0);
            int columnCount = headerRow.LastCellNum;
            int rowCount = sheet.LastRowNum;
            //row.Cell(0).SetCellValue("string");
            //row.Cell(1).SetCellValue("translate");
            //row.Cell(2).SetCellValue("line");
            //row.Cell(3).SetCellValue("path");
            //row.Cell(4).SetCellValue("type");

            try
            {
                for(var i = 1; i < rowCount; ++i)
                {
                    row = sheet.Row(i);
                    var fname = row.Cell(3).SValue();
                    var ostr = row.Cell(0).SValue();
                    var tstr = row.Cell(1).SValue();
                    var lineno = (int)row.Cell(2).NumericCellValue;

                    if(string.IsNullOrEmpty(tstr))
                        continue;

                    // 全文替换
                    var rstream = new StreamReader(fname);
                    var str = rstream.ReadToEnd();
                    rstream.Close();

                    str = str.Replace(ostr, tstr);

                    var wstream = new StreamWriter(fname);
                    wstream.Write(str);
                    wstream.Flush();
                    wstream.Close();

                    //// 用行号指定替换
                    //var lines = File.ReadAllLines(fname);
                    //lines[lineno - 1] = lines[lineno - 1].Replace(ostr, tstr);
                    //File.WriteAllLines(fname, lines);

                    Console.WriteLine("{0}:{1}:{2}:{3}", i, ostr, tstr, fname);

                }// foreach
            }
            catch(Exception e)
            {
                Console.WriteLine("\n\n翻译内容工作表名必须是 [Sheet], 原文在第 1 列, 译文在第 2 列, 文件名在第 4 列\n\n\n{0}", e);
            }

            Console.WriteLine("按 Enter 退出");
            Console.ReadLine();
        }
    }
}
