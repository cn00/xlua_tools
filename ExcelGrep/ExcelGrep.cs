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
using LibShared;

namespace ExcelGrep
{
    class ExcelGrep
    {
        static void Main(string[] args)
        {
            string inputdir = ".";
            string oldStr = null;
            if(args.Length == 2)
            {
                inputdir = args[0];
                oldStr = args[1];
                goto go;
            }
            begin:
            Console.WriteLine("usage: ExcelGrep Str\n或按下面提示操作");
            Console.WriteLine("输入待查找 Excel 根路径, 默认值: " + inputdir);
            Console.Write("输入或拖入: ");
            var tmp = Console.ReadLine();
            if(!string.IsNullOrWhiteSpace(tmp) && Directory.Exists(tmp))
                inputdir = tmp;
            else
            {
                goto begin;
            }

            inputdir = inputdir.upath();
            Console.WriteLine("待查找 Excel 根路径为: " + inputdir);

            input_old_str:
            Console.Write("输入查找字符串: ");
            oldStr = Console.ReadLine();
            if(string.IsNullOrEmpty(oldStr))
                goto input_old_str;

            Console.WriteLine("将查找全部 [{0}]", oldStr);
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

            foreach(var f in Directory.GetFiles(inputdir, "*.*", SearchOption.AllDirectories)
                .Where(f => f.EndsWith(".xls") || f.EndsWith(".xlsx")))
            {
                var fin = f.upath();
                Grep(fin, oldStr);
                stdout.Flush();
                //Console.WriteLine(fin);
            }// foreach

            stdout.Flush();
            stdout.Close();

            Console.SetOut(oldout);
            Console.Write("\n搜索了 {0} 个文件, {1} 个匹配\n", FileColCount, MatchColCount);
            var os = Environment.OSVersion.ToString();
            Console.WriteLine(os);
            if (!os.Contains("Unix"))
            {
                Console.WriteLine("按 Enter 退出");
                Console.ReadLine();
            }

        }//end main

        const string space = "                  ";
        static int FileColCount = 0;
        static int MatchColCount = 0;
        public static void Grep(string inExcel, string oldStr)
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
            inStream.Close();

            ++FileColCount;
            Console.Write(FileColCount + " <<< " + inExcel + space +  "\r");

            int count = 0;
            string outstring = FileColCount + ": " + inExcel + space + "\n";
            foreach(var sheet in inbook.AllSheets())
            {
                for(int i = 1; i <= sheet.LastRowNum; ++i)
                {
                    var row = sheet.Row(i);
                    for(int j = 0; j < row.LastCellNum; ++j)
                    {
                        var c = row.Cell(j);
                        var v = c.SValue();
                        var match = Regex.Matches(v, oldStr);
                        if(match.Count > 0)
                        {
                            ++count;
                            ++MatchColCount;
                            //c.SetCellValue(v.Replace(oldStr, newStr));
                            outstring += string.Format("\t{0}: [{1}, {2}]: {3}\n", sheet.SheetName, i, j, v.Replace("\n", "\\n").Replace("\r", "\\r"));
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


    public class test
    {
        public static string Mixture(string instr)
        {
            int len = instr.Length;
            int plus = (len % 8) + 1;
            var sbase64 = new byte[len];

            for(int i = 0; i < len; i++)
            {
                sbase64[i] = (byte)((int)instr[i] + plus);
            }

            return System.Text.Encoding.Default.GetString(sbase64);
            //return sbase64;
        }

        public static string Decode(string instr)
        {
            string sout = "";
            int len = instr.Length;
            var sbase64 = Mixture(instr);

            if(len > 0)
            {
                try
                {
                    var tmp = System.Convert.FromBase64String(sbase64);
                    sout = Encoding.Default.GetString(tmp);
                }
                catch(Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            return sout;
        }

        public static string Encode(string instr)
        {
            string sout = "";
            int len = instr.Length;
            if(len > 0)
            {
                try
                {
                    sout = System.Convert.ToBase64String(Encoding.Default.GetBytes(instr));
                    sout = Mixture(sout);
                }
                catch(Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return sout;
        }

        public static void main()
        {
            foreach(var l in File.ReadAllLines(""))
            {
                //var id = Regex.Replace(l, "|.*", "");
                //var s = Regex.Replace(l, ".*|", "");
                var r = l.Split('|');
                var id = r[0];
                var s = r[1];
                Console.WriteLine("{0},{1}",id, Encode(s));
            }

        }
    }
}
