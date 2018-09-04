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

namespace ExcelDiff
{
    class ExcelDiff
    {
        static void Main(string[] args)
        {
            string file1 = null;
            string file2 = null;
            if(args.Length == 2)
            {
                file1 = args[0];
                file2 = args[1];
                goto go;
            }
            begin:
            Console.WriteLine("\tusage: ExcelDiff file1 file2\n或按下面提示操作");
            Console.WriteLine("输入待比较 Excel1 路径: " + file1);
            Console.Write("输入或拖入: ");
            var tmp = Console.ReadLine();
            if(!string.IsNullOrWhiteSpace(tmp) && Directory.Exists(tmp))
                file1 = tmp;
            else
            {
                goto begin;
            }

            file1 = file1.upath();
            Console.WriteLine("待比较 Excel1 路径为: " + file1);

            input_old_str:
            Console.Write("输入待比较 Excel2 路径: ");
            file2 = Console.ReadLine();
            if(string.IsNullOrEmpty(file2))
                goto input_old_str;

            Console.WriteLine("待比较 Excel2 路径为: [{0}]", file2);
            Console.Write("按 Enter 确认: ");
            tmp = Console.ReadLine();
            if(!string.IsNullOrWhiteSpace(tmp))
                goto input_old_str;

            go:
            // redirect stdout
            var stdout = new StreamRedirect.StreamRedirect("./log.log");
            stdout.Add(Console.Out);
            var oldout = Console.Out;
            Console.SetOut(stdout);

            Diff(file1, file2);

            stdout.Flush();
            stdout.Close();

            Console.SetOut(oldout);
            Console.WriteLine("比较了 {0} 个单元格, {1} 个不同", TotalCellCount, DiffCellCount);
            var os = Environment.OSVersion.ToString();
            Console.WriteLine(os);
            if (!os.Contains("Unix"))
            {
                Console.WriteLine("按 Enter 退出");
                Console.ReadLine();
            }
        }//end main

        const string space = "                  ";
        static int TotalCellCount = 0;
        static int DiffCellCount = 0;
        public static void Diff(string filePath1, string filePath2)
        {
            var inStream = new FileStream(filePath1, FileMode.Open);

            IWorkbook book1 = null;
            if(filePath1.EndsWith(".xls"))
            {
                book1 = new HSSFWorkbook(inStream);
            }
            else if(filePath1.EndsWith(".xlsx"))
            {
                book1 = new XSSFWorkbook(inStream);
            }
            inStream.Close();

            IWorkbook book2 = null;
            inStream = new FileStream(filePath2, FileMode.Open);
            if (filePath2.EndsWith(".xls"))
            {
                book2 = new HSSFWorkbook(inStream);
            }
            else if (filePath2.EndsWith(".xlsx"))
            {
                book2 = new XSSFWorkbook(inStream);
            }
            inStream.Close();


            int count = 0;
            string outstring = filePath1 + space + "\n";
            foreach(var sheetL in book1.AllSheets())
            {
                var sheetR = book2.GetSheet(sheetL.SheetName);
                if (sheetR == null)
                {
                    Console.WriteLine("[{0}] not in {1}", sheetL.SheetName, filePath2);
                    continue;
                }

                for(int i = 0; i <= sheetL.LastRowNum; ++i)
                {
                    var rowL = sheetL.Row(i);
                    var rowR = sheetR.Row(i);
                    for(int j = 0; j < rowL.LastCellNum; ++j)
                    {
                        ++TotalCellCount;
                        var cL = rowL.Cell(j);
                        var cR = rowR.Cell(j);
                        var vL = cL.SValue();
                        var vR = cR.SValue();
                        if(vL != vR)
                        {
                            ++count;
                            ++DiffCellCount;
                            //c.SetCellValue(v.Replace(oldStr, newStr));
                            outstring += string.Format("\t{0}: [{1}{2}]: \n\t\t{3}\n\t\t{4}\n"
                                                       , sheetL.SheetName, sheetL.ColumnName(j), i+1
                                                       , vL.Replace("\n", "\\n").Replace("\r", "\\r")
                                                       , vR.Replace("\n", "\\n").Replace("\r", "\\r"));
                        }
                    }
                }
            }
            if(count > 0)
            {
                Console.Write(outstring + "                  \n");
            }
        }
    }
}
