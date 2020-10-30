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
using NPOI;

namespace ExcelJpReplace
{
    class ExcelJpReplace
    {
        static string ProjRoot = ".";
        static void Main(string[] args)
        {
            begin:
            var inputdir = "D:/a3/client/Unity/Assets/Application/Resource/ExcelData.out";
            Console.WriteLine("输入已翻译 Excel 根路径, 默认值: " + inputdir);
            Console.Write("输入或拖入: ");

            var tmp = Console.ReadLine();
            if(!string.IsNullOrWhiteSpace(tmp) && Directory.Exists(tmp))
                inputdir = tmp;
            else
            {
                goto begin;
            }

            Console.WriteLine("工程路径: " + ProjRoot);
            Console.Write("输入或拖入: ");
            tmp = Console.ReadLine();
            if(!string.IsNullOrWhiteSpace(tmp) && Directory.Exists(tmp))
                ProjRoot = tmp;
            else
            {
                goto begin;
            }

            try
            {
                inputdir = inputdir.upath();
                Console.WriteLine("已翻译 Excel 根路径为: " + inputdir);
                foreach(var f in Directory.GetFiles(inputdir, "*.*", SearchOption.AllDirectories)
                    .Where(f => f.EndsWith(".xls") || f.EndsWith(".xlsx")))
                {
                    var fin = f.upath();
                    Replace(fin);
                    //Console.WriteLine(fin);
                }// foreach
            }
            catch(Exception e)
            {
                Console.WriteLine("error: " + e.ToString());
            }
            Console.WriteLine("按 Enter 退出");
            Console.ReadLine();
        }//end main

        static int colCount = 0;
        public static void Replace(string inExcel)
        {
            try
            {
                Console.WriteLine(inExcel);
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
                inStream.Close();
                inStream.Dispose();

                var infoSheet = inbook.Sheet("info");
                string originExcel = infoSheet.Cell(0, 0).StringCellValue;
                originExcel = originExcel.Replace("D:/a3", ProjRoot);
                var outStream = new FileStream(originExcel, FileMode.Open);
                if(outStream == null)
                {
                    Console.WriteLine("{0} open failed.", originExcel);
                    throw new Exception(originExcel);
                }
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
                var hrow = jpSheet.GetRow(0);
                var head = new ExcelHead(hrow);

                for(var i = 1; i <= jpSheet.LastRowNum; ++i)
                {
                    var row = jpSheet.Row(i);
                    var os = row.Cell(head[HeadIdx.jp]).StringCellValue;
                    var ts = row.Cell(head[HeadIdx.trans]).StringCellValue;

                    var x = (int)row.Cell(head[HeadIdx.i]).NumericCellValue;
                    var y = (int)row.Cell(head[HeadIdx.j]).NumericCellValue;
                    var sname = row.Cell(head[HeadIdx.SheetName]).StringCellValue;
                    var osheet = outbook.Sheet(sname);
                    var ocell = osheet.Cell(x, y);
                    if(ocell.StringCellValue == os)
                    {
                        ocell.SetCellValue(ts);
                        Debug.WriteLine(string.Format("\t[{4},{2},{3}]:[{0}] => [{1}]", os.Replace("\n", "\\n"), ts.Replace("\n", "\\n"), x, y, sname));
                    }
                    else
                        Debug.WriteLine(string.Format("\t{0}[{1}] <=> [{2}] not match", i, os.Replace("\n", "\\n"), ocell.StringCellValue.Replace("\n", "\\n")));
                }

                //File.Delete(originExcel);
                outStream = new FileStream(originExcel, FileMode.Create);
                outbook.Write(outStream);
                outStream.Flush();
                outStream.Close();
                outStream.Dispose();
                ++colCount;
                Console.WriteLine(colCount + " done: " + originExcel);
            }
            catch(Exception e)
            {
                Console.WriteLine(inExcel + ": " + e.ToString());
            }
        }

    }
}
