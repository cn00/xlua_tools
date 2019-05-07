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
using LitJson;

namespace XmlJp
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
            switch (cellType)
            {
                case CellType.Unknown:
                    svalue = "";
                    break;
                case CellType.Numeric:
                    svalue = cell.NumericCellValue.ToString();
                    break;
                case CellType.String:
                    svalue = cell.StringCellValue
                                 .Replace("\n", "\\n")
                                 .Replace("\t", "\\t")
                                 .Replace("\"", "\\\"");
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
        // const string regular = "[^\x00-\xff「」（）【】■～…]";
        const string regular = "[\u3021-\u3126]";
        private static int delta_lang_idx = 45000;

        public class Record
        {
            public string id = "";
            public string jp = "";
            public string tr = "";
        }

        public enum ColumIdx : int
        {
            jp,
            trans,
            line,
            path,
            src,
            idx,
        }

        static void CollectTranslateJsonToExcel(string jsonPath, ISheet sheet, string inipath, ISheet inisheet)
        {
            var inis = File.ReadAllLines(inipath);
            var inidic = new Dictionary<string, string>();
            var iniidx = 1;
            foreach (var ii in inis)
            {
                var k = ii.Substring(0, 6);
                var v = ii.Substring(7);
                inidic[v] = k;


                var row = inisheet.Row(iniidx);
                row.Cell(0).SetCellValue(k); // id
                row.Cell(1).SetCellValue(v); // jp

                ++iniidx;
            }


            var datas = JsonMapper.ToObject(File.ReadAllText(jsonPath));
            var idx = 1;
            foreach (var i in datas.Keys)
            {
                var row = sheet.Row(idx);
                row.Cell(1).SetCellValue(i.Replace("\n", "\\n")); // jp
                row.Cell(2).SetCellValue(datas[i].ToString().Replace("\n", "\\n")); // zh
                string cnidx = null;
                if (inidic.TryGetValue(datas[i].ToString(), out cnidx))
                {
                    row.Cell(0).SetCellValue(cnidx); // id
                }

                ++idx;
            }
        }

        const int MaxRowNum = 20000;
        const int MaxColuNum = 300;
        static void Delta(ISheet allSheet, ISheet transSheet, ISheet deltaSheet)
        {
            var transDic = new Dictionary<string, Record>();
            for (int i = 0; i < transSheet.LastRowNum && i < MaxRowNum; i++)
            {
                var row = transSheet.Row(i);
                var cell = row.Cell(1);
                var k = cell.StringCellValue;
                var v = new Record()
                {
                    id = row.Cell(0).SValue(),
                    jp = row.Cell(1).SValue(),
                    tr = row.Cell(2).SValue(),
                };
                transDic[k] = v;
            }

            var deltaIdx = 1;
            for (int i = allSheet.FirstRowNum+1; i <= allSheet.LastRowNum && i < MaxRowNum; ++i)
            {
                var rowl = allSheet.Row(i);
                Record rec = null;
                if (transDic.TryGetValue(rowl.Cell(0).StringCellValue, out rec))
                {
                    
                }
                else
                {
                    var row = deltaSheet.Row(deltaIdx);
                    row.Cell(0).SetCellValue(rowl.Cell(0).SValue());
                    row.Cell(1).SetCellValue(rowl.Cell(1).SValue());
                    row.Cell(2).SetCellValue(rowl.Cell(2).SValue());
                    row.Cell(3).SetCellValue(rowl.Cell(3).SValue());
                    row.Cell(4).SetCellValue(rowl.Cell(4).SValue());
                    row.Cell(5).SetCellValue(rowl.Cell(5).SValue());
                    ++deltaIdx;
                }
            }
        }

        static void Main(string[] args)
        {
            Console.WriteLine("输入查找路径:");
            var inputdir = Console.ReadLine() ?? "Classes";
            var stringCount = 0;


            var excelName = inputdir + "/" + inputdir.Substring(inputdir.LastIndexOf(Path.DirectorySeparatorChar) + 1) + ".xlsx";
            XSSFWorkbook workbook = null;
            FileStream excelStream = null;
            if (File.Exists(excelName))
            {
                excelStream = new FileStream(excelName, FileMode.OpenOrCreate);
                excelStream.Position = 0;
                workbook = new XSSFWorkbook(excelStream);
                excelStream.Close();
            }
            else
            {
                workbook = new XSSFWorkbook();
            }

            var sheet_jp = workbook.Sheet("jp"); //创建工作表
            var sheet_jp_delta = workbook.Sheet("jp_delta"); //创建工作表

            var sheet_old = workbook.Sheet("old");
            // var sheet3 = workbook.Sheet("ini");
            // CollectTranslateJsonToExcel("old-json.txt", sheet2,
            //     "/Users/men/ws/c2/Resources/vitamin/data/v2040/language_cn.ini", sheet3);

            var textName = inputdir + "/" + inputdir.Substring(inputdir.LastIndexOf(Path.DirectorySeparatorChar) + 1) +
                           ".txt";
            var textStream = new StreamWriter(textName, false, Encoding.UTF8);
            var fini = new StreamWriter(textName + ".ini", false, Encoding.UTF8);

            
            var row = sheet_jp.Row(0);
            row.Cell((int) ColumIdx.jp).SetCellValue("jp");
            row.Cell(1).SetCellValue("trans");
            row.Cell(2).SetCellValue("line");
            row.Cell(3).SetCellValue("path");
            row.Cell(4).SetCellValue("src");
            row.Cell(5).SetCellValue("idx");
            row = sheet_jp_delta.Row(0);
            row.Cell((int) ColumIdx.jp).SetCellValue("jp");
            row.Cell(1).SetCellValue("trans");
            row.Cell(2).SetCellValue("line");
            row.Cell(3).SetCellValue("path");
            row.Cell(4).SetCellValue("src");
            row.Cell(5).SetCellValue("idx");


            foreach (var f in Directory.GetFiles(inputdir, "*.*", SearchOption.AllDirectories)
                .Where(f =>
                    f.EndsWith(".xml")
                    || f.EndsWith(".ccb")
                    || f.EndsWith(".html")
                    || f.EndsWith(".plist")
                )
            )
            {
                //var stream = new StreamReader(f);
                var txt = File.ReadAllText(f);
                // <string>xxx</string>
                var matches3 = Regex.Matches(txt, ">[^>]*" + regular + "+[^<]*<", RegexOptions.Multiline);
                foreach (var i in matches3)
                {
                    ++delta_lang_idx;
                    ++stringCount;
                    row = sheet_jp.Row(stringCount);
                    var s0 = i.ToString()
                        .Replace(">", "")
                        .Replace("<", "")
                        .Replace("\r", "\\r")
                        .Replace("\n", "\\n")
                        ;
                    row.Cell(0).SetCellValue(s0);
                    row.Cell(1).SetCellValue("译文");
                    row.Cell(2).SetCellValue("");
                    row.Cell(3).SetCellValue(f.upath());
                    row.Cell(5).SetCellValue(string.Format("{0:000000}", (delta_lang_idx)));
                    var s = string.Format("{0:000000}#{1}#{2}#{3}\n", delta_lang_idx, f.upath(), 0, s0);
                    textStream.Write(s);
                    fini.WriteLine(string.Format("{0:000000}={1}", delta_lang_idx, s0));
                    Console.WriteLine("{0}:{1}:{2}:{3}", stringCount, 0, i, f.upath());
                }
                

                textStream.Flush();
            } // foreach
            
            
            Delta(sheet_jp, sheet_old, sheet_jp_delta);
            
            
            textStream.Close();


            excelStream = new FileStream(excelName, FileMode.OpenOrCreate);
            excelStream.Position = 0;
            workbook.Write(excelStream);
            excelStream.Close();
            fini.Close();

            Console.WriteLine("按 Enter 退出");
            Console.ReadLine();
        }
    }
}