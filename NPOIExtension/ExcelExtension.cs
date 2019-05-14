using System;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System.Collections.Generic;

namespace NPOI
{
    public static class ExcelExtension
    {
        public static string[] ColumnNames = new[]{
            "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", 
            "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", 
            "U", "V", "W", "X", "Y", "Z"
        };

        public const int MaxRowNum = 50000;
        public const int MaxColumn = 256;
        public static string upath(this string self)
        {

            var outp = self.Trim()
                .TrimEnd()
                .Replace("\\", "/")
                .Replace("//", "/");
            var os = System.Environment.OSVersion.ToString();
            if (os.Contains("Windows") && outp.StartsWith("/cygdrive/"))
            {
                //in cygwin convert "/cygdrive/d/path_to_some_where" => "d:/path_to_some_where"
                outp = outp.Replace("/cygdrive/", "");
                string tmp = outp.Substring(0, 1);
                tmp += ":" + outp.Substring(1);
                outp = tmp;
                // Console.WriteLine("cygwin_path_to_winpath:{0}", outp);
            }
            return outp;
        }

        public static string oneline(this string self)
        {

            return self.Trim()
                .Replace("\n", "\\n")
                .Replace("\r", "\\r");
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
                svalue = cell.StringCellValue
                             // .Replace("\n", "\\n")
                             // .Replace("\r", "\\r")
                             // .Replace("\t", "\\t")
                             // .Replace("\"", "\\\"")
                             ;
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
        public static string ColumnName(this ISheet sheet, int i)
        {
            string frefix = "";
            if (i > 26 && i < 26 * 26)
                frefix = ColumnNames[(i / 26)-1];
            return frefix + ColumnNames[i%26];
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
}
