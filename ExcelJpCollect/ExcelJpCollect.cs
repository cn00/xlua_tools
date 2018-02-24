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

namespace CollectExcelJp
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
    class ExcelJpCollect
    {
        static void Main(string[] args)
        {
            begin:
            var inputdir = "D:/a3/client/Unity/Assets/Application/Resource/ExcelData";
            Console.WriteLine("输入待翻译 Excel 根路径, 默认值: " + inputdir);
            Console.Write("输入或拖入: ");
            var tmp = Console.ReadLine();
            if(!string.IsNullOrWhiteSpace(tmp))
                inputdir = tmp;
            if(!Directory.Exists(inputdir))
            {
                goto begin;
            }

            inputdir = inputdir.upath();
            Console.WriteLine("待翻译 Excel 根路径为: " + inputdir);
            var outdir = inputdir + ".out";
            foreach(var f in Directory.GetFiles(inputdir, "*.*", SearchOption.AllDirectories)
                .Where(f => f.EndsWith(".xls") || f.EndsWith(".xlsx")))
            {
                var fin = f.upath();
                var b = Collect(fin, fin.Replace(inputdir, outdir));
                //Console.WriteLine(fin);
            }// foreach

            Console.WriteLine("按 Enter 退出");
            Console.ReadLine();
        }//end main

        const string regular = "[\u3021-\u3126]";
        static int colCount = 0;
        public static bool Collect(string inExcel, string outExcel)
        {
            bool b = false;
            var inStream = new FileStream(inExcel, FileMode.Open);

            IWorkbook inbook = null;
            if(inExcel.EndsWith("xls"))
            {
                inbook = new HSSFWorkbook(inStream);
                outExcel += "x";
            }
            else if(inExcel.EndsWith(".xlsx"))
            {
                inbook = new XSSFWorkbook(inStream);
            }

            int totalCount = 0;
            var outBook = new XSSFWorkbook();
            int count = 0;
            var outSheet = outBook.Sheet("jp");//new XSSFSheet();//
            int tmi = -1;
            outSheet.Cell(0, ++tmi).SetCellValue("jp");
            outSheet.Cell(0, ++tmi).SetCellValue("trans");
            outSheet.Cell(0, ++tmi).SetCellValue("trans_jd");
            outSheet.Cell(0, ++tmi).SetCellValue("i");
            outSheet.Cell(0, ++tmi).SetCellValue("j");
            outSheet.Cell(0, ++tmi).SetCellValue("SheetName");

            bool cellLock = true;
            foreach(var sheet in inbook.AllSheets())
            {
                //跳过表头
                for(int i = 1; i <= sheet.LastRowNum; ++i)
                {
                    var row = sheet.Row(i);
                    for(int j = 0; j < row.LastCellNum; ++j)
                    {
                        var v = row.Cell(j).SValue();
                        var matches = Regex.Matches(v, regular + "+.*");
                        if(matches.Count > 0)
                        {
                            ++count;
                            ++totalCount;

                            int tmi2 = -1;

                            var c = outSheet.Cell(count, ++tmi2);
                            c.SetCellValue(v);
                            c.CellStyle.IsLocked = cellLock;

                            c = outSheet.Cell(count, ++tmi2);
                            c.SetCellValue("译文: " + v);
                            c.CellStyle.IsLocked = false;

                            c = outSheet.Cell(count, ++tmi2);
                            c.SetCellValue("校对");
                            c.CellStyle.IsLocked = false;

                            c = outSheet.Cell(count, ++tmi2);
                            c.SetCellValue(i);
                            c.CellStyle.IsLocked = cellLock;

                            c = outSheet.Cell(count, ++tmi2);
                            c.SetCellValue(j);
                            c.CellStyle.IsLocked = cellLock;

                            c = outSheet.Cell(count, ++tmi2);
                            c.SetCellValue(sheet.SheetName);
                            c.CellStyle.IsLocked = cellLock;
                        }
                    }
                }

                //if(count == 0)
                //{
                //    var b = outBook.Remove(outSheet);
                //    outBook.FirstVisibleTab = 0;
                //    if(b == false)
                //    {
                //        Console.WriteLine("remove failed.");
                //    }
                //}
            }
            //var l = outSheet.GetColumnStyle(0);
            //l.IsLocked = true;
            //l.ShrinkToFit = true;
            //l = outSheet.GetColumnStyle(1);
            //l.IsLocked = false;
            //l.ShrinkToFit = true;
            //l = outSheet.GetColumnStyle(2);
            //l.IsLocked = true;
            //l.ShrinkToFit = true;
            //l = outSheet.GetColumnStyle(3);
            //l.IsLocked = true;
            //l.ShrinkToFit = true;
            //l = outSheet.GetColumnStyle(4);
            //l.IsLocked = true;
            //l.ShrinkToFit = true;

            //outSheet.ProtectSheet("654123");
            inStream.Close();

            if(totalCount > 0)
            {
                b = true;
                ++colCount;
                var infoSheet = outBook.Sheet("info");//
                infoSheet.Cell(0, 0).SetCellValue(inExcel);
                var outDir = Path.GetDirectoryName(outExcel);
                if(!Directory.Exists(outDir))
                {
                    Directory.CreateDirectory(outDir);
                }
                var outStream = new FileStream(outExcel, FileMode.Create);
                outStream.Position = 0;

                //outBook.LockRevision();
                //outBook.LockStructure();
                //outBook.LockWindows();

                outBook.Write(outStream);
                outStream.Close();
                Console.WriteLine(colCount + " >>> " + outExcel);
            }
            return b;
        }
    }
}
