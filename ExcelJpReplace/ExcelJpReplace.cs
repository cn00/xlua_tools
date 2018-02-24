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


namespace ExcelJpReplace
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
                svalue = cell.StringCellValue
                    //.Replace("\n", "\\n")
                    //.Replace("\t", "\\t")
                    //.Replace("\"", "\\\"")
                    ;
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

        public static List<ISheet> AllSheets(this IWorkbook workbook)
        {
            List<ISheet> sheets = new List<ISheet>();
            if(workbook is HSSFWorkbook)
            {
                HSSFWorkbook book = workbook as HSSFWorkbook;
                for(int i = 0; i < book.NumberOfSheets; ++i)
                {
                    sheets.Add(book.GetSheetAt(i));
                }
            }
            else if(workbook is XSSFWorkbook)
            {
                XSSFWorkbook book = workbook as XSSFWorkbook;
                for(int i = 0; i < book.NumberOfSheets; ++i)
                {
                    sheets.Add(book.GetSheetAt(i));
                }
            }
            return sheets;
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
        public static ICell Cell(this ISheet sheet, int i, int j)
        {
            return sheet.Row(i).Cell(j);
        }

    }
    class ExcelJpReplace
    {
        static void Main(string[] args)
        {
            begin:
            var inputdir = "D:/a3/client/Unity/Assets/Application/Resource/ExcelData.out";
            Console.WriteLine("输入已翻译 Excel 根路径, 默认值: " + inputdir);
            Console.Write("输入或拖入: ");
            var tmp = Console.ReadLine();
            if(!string.IsNullOrWhiteSpace(tmp))
                inputdir = tmp;
            if(!Directory.Exists(inputdir))
            {
                goto begin;
            }

            inputdir = inputdir.upath();
            Console.WriteLine("已翻译 Excel 根路径为: " + inputdir);
            var outdir = inputdir + ".out";
            foreach(var f in Directory.GetFiles(inputdir, "*.*", SearchOption.AllDirectories)
                .Where(f => f.EndsWith(".xls") || f.EndsWith(".xlsx")))
            {
                var fin = f.upath();
                Replace(fin);
                //Console.WriteLine(fin);
            }// foreach

            Console.WriteLine("按 Enter 退出");
            Console.ReadLine();
        }//end main

        static int colCount = 0;
        public static void Replace(string inExcel)
        {
            var inStream = new FileStream(inExcel, FileMode.Open);

            IWorkbook inbook = null;
            if(inExcel.EndsWith("xls"))
            {
                inbook = new HSSFWorkbook(inStream);
            }
            else if(inExcel.EndsWith(".xlsx"))
            {
                inbook = new XSSFWorkbook(inStream);
            }

            var infoSheet = inbook.Sheet("info");
            string originExcel = infoSheet.Cell(0, 0).StringCellValue;
            var outStream = new FileStream(originExcel, FileMode.Open);
            IWorkbook outbook = null;
            if(originExcel.EndsWith(".xls"))
            {
                outbook = new HSSFWorkbook(outStream);
            }
            else if(originExcel.EndsWith(".xlsx"))
            {
                outbook = new XSSFWorkbook(outStream);
            }
            outStream.Close();

            var jpSheet = inbook.Sheet("jp");
            for(var i = 1; i <= jpSheet.LastRowNum; ++i)
            {
                int tmi2 = -1;
                var row = jpSheet.Row(i);
                var os = row.Cell(++tmi2).StringCellValue;
                var ts = row.Cell(++tmi2).StringCellValue;
                ++tmi2;//trans_jd
                var x = (int)row.Cell(++tmi2).NumericCellValue;
                var y = (int)row.Cell(++tmi2).NumericCellValue;
                var sname = row.Cell(++tmi2).StringCellValue;
                var osheet = outbook.Sheet(sname);
                var ocell = osheet.Cell(x, y);
                if(ocell.StringCellValue == os)
                    ocell.SetCellValue(ts);
                else
                    Console.WriteLine(string.Format("{0},[{1}] not match", i, os));
            }

            //File.Delete(originExcel);
            outStream = new FileStream(originExcel, FileMode.Create);
            outbook.Write(outStream);
            outStream.Close();
            inStream.Close();
            ++colCount;
            Console.WriteLine(colCount + " <<< " + originExcel);
        }

    }
}
