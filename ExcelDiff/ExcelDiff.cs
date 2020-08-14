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
            // args = new[]
            // {
            //     "/Volumes/Data/a3/c2/excels/strings/strings-zh-202-m110.xlsx",
            //     "/Volumes/Data/a3/c2/excels/strings/strings-zh-202.xlsx"
            // };
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
            //// redirect stdout
            //var stdout = new StreamRedirect.StreamRedirect("./log.log");
            //stdout.Add(Console.Out);
            //var oldout = Console.Out;
            //Console.SetOut(stdout);

            Diff(file1, file2);

            //stdout.Flush();
            //stdout.Close();

            //Console.SetOut(oldout);

            var os = Environment.OSVersion.ToString();
            //Console.WriteLine(os);
            if (!os.Contains("Unix"))
            {
                Console.WriteLine("按 Enter 退出");
                Console.ReadLine();
            }
        }//end main

        const string space = "                  ";
        static int TotalCellCount = 0;
        static int DiffCellCount = 0;
        const int MaxRowNum = 100000;
        const int MaxColuNum = 300;
        public static void Diff(string filePath1, string filePath2)
        {
            StringBuilder outstring = new StringBuilder(2048);
            outstring.Append(string.Format("]]--\n{{\n\ta=\"{0}\",\n\tb=\"{1}\",\n\tsheets={{", filePath1, filePath2));
            //Console.WriteLine("diff {0} {1}", filePath1, filePath2);
            if (!File.Exists(filePath1) || !File.Exists(filePath2)
               || filePath1 == "/dev/null" || filePath2 == "/dev/null")
            {
                outstring.Append("},\n\tmsg = \"warning: diff with null file, skip compare\",\n},");
                Console.WriteLine(outstring);
                return;
            }
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

            //var regular = "[^\x00-\xff「」（）【】■～…]";

            int bookcount = 0;
            foreach (var sheetL in book1.AllSheets())
            {
                var sheetout = new StringBuilder(1024);
                var sheetR = book2.GetSheet(sheetL.SheetName);

                var headL = sheetL.Row(sheetL.FirstRowNum);
                sheetout.Append(string.Format("\n\t{{\n\t\tname=\"{0}\",\n\t\thead={{", sheetL.SheetName));
                for (int j = headL.FirstCellNum; j < headL.LastCellNum && j < MaxColuNum; ++j)
                {
                    sheetout.Append(string.Format("\n\t\t\t[{1}]=\"{0}\",", headL.Cell(j).SValue().Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r"), j+1));
                }
                sheetout.Append("\n\t\t},");
                sheetout.Append(string.Format("\n\t\tL_LastRowNum={0},\n\t\tR_LastRowNum={1},", sheetL.LastRowNum, sheetR.LastRowNum));

                var sheetcount = 0;
                if (sheetR == null)
                {
                    sheetout.Append(string.Format("\n\t\t\tmsg=\"[{0}] not in {1}\"}},}},", sheetL.SheetName, filePath2));
                }
                else
                {

                    sheetout.Append(string.Format("\n\t\tcells={{"));
                   
                    var headR = sheetL.Row(sheetR.FirstRowNum);

                    for (int i = sheetL.FirstRowNum; i <= Math.Max(sheetL.LastRowNum, sheetR.LastRowNum) && i < MaxRowNum; ++i)
                    {
                        var rowL = sheetL.Row(i);
                        var rowR = sheetR.Row(i);
                        for (int j = headL.FirstCellNum; j < headL.LastCellNum && j < MaxColuNum; ++j)
                        {
                            ++TotalCellCount;
                            var cL = rowL.Cell(j);
                            var cR = rowR.Cell(j);
                            var vL = cL.SValue();
                            var vR = cR.SValue();
                            //var matches = Regex.Matches(vL, regular + "+"); 
                            if (
                                //(matches.Count == 0) 
                                //&& (vL != vR 
                                // || (vL != "nil" && vR == "nil")
                                //)
                                vL != vR
                            )
                            {
                                ++sheetcount;

                                ++DiffCellCount;
                                //c.SetCellValue(v.Replace(oldStr, newStr));

                                //Scenario:[I34]
                                //    不過，我們還得繼續成長哦！　目標是得到金花獎！
                                //    不過，我們還得繼續成長哦！　一定要得到金花獎！
                                /*
                                {
                                    a = "excel-a.xls",
                                    b = "excel-b.xls",
                                    sheets = {
                                        ["sheetname"] = {
                                            msg = "",
                                            cells = {
                                                {x = 2, y = 23,
                                                    a = "aaaaaaa",
                                                    b = "bbbbbbb",
                                                },
                                                {x = 2, y = 23,
                                                    a = "aaaaaaa",
                                                    b = "bbbbbbb",
                                                },
                                                ...
                                            },
                                        },
                                        ...
                                    }
                                    msg = "",
                                },
                                */
                                var s = string.Format("\n\t\t\t{{\n\t\t\t\tx={0},y={1},\n\t\t\t\ta=\"{2}\",\n\t\t\t\tb=\"{3}\",\n\t\t\t}},"
                                                           , j+1, i + 1
                                                           , vL.Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r")
                                                           , vR.Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r")
                                                     );
                                sheetout.Append(s);
                            }// for rows

                        }// for sheets

                    }
                }
                bookcount += sheetcount;
                sheetout.Append("\n\t\t},\n\t},-- " + sheetL.SheetName);
                if (sheetcount > 0)
                    outstring.Append(sheetout);

            }//foreach sheet
            outstring.Append(string.Format("\n\t}},--sheets\n\tcompared={0},different={1}\n}},\n--[[\n", TotalCellCount, DiffCellCount));
            if (bookcount > 0)
            {
                Console.Write(outstring);
            }
        }
    }
}
