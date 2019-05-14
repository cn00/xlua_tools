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

namespace replace
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("请输入翻译 Excel 文档:");
            var inputdir = Console.ReadLine().upath();

            var excelName = inputdir.TrimStart(new char[] {'"'}).TrimEnd(new char[] { '"' });// inputdir + "/" + inputdir.Substring(inputdir.LastIndexOf(Path.DirectorySeparatorChar) + 1) + ".xlsx";
            var excelStream = new FileStream(excelName, FileMode.Open);
            var workbook = new XSSFWorkbook(excelStream);//创建Workbook对象
            var sheet = workbook.GetSheet("jp");
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
                    var ostr = row.Cell(0).SValue();
                    var tstr = row.Cell(1).SValue();
                    var fname = row.Cell(3).SValue();
                    var sidx = row.Cell(5).SValue();
                    
                    // var lineno = (int)row.Cell(2).NumericCellValue;

                    if(string.IsNullOrEmpty(tstr))
                        continue;
                    LineReplace(fname, ostr, tstr, sidx);

                    // // 全文替换
                    // var rstream = new StreamReader(fname);
                    // var str = rstream.ReadToEnd();
                    // rstream.Close();
                    //
                    // str = str.Replace(ostr, tstr);
                    //
                    // var wstream = new StreamWriter(fname);
                    // wstream.Write(str);
                    // wstream.Flush();
                    // wstream.Close();

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

        // static void DocReplace(string fpath, string ostr, string nstr, string sidx)
        // {
        //     var s = File.ReadAllText(fpath.upath());
        //     s.Replace("\"" + ostr + "\"", "\"" + nstr + "\"");
        //     File.WriteAllText(fpath, s, Encoding.UTF8);
        // }

        static void LineReplace(string fpath, string ostr, string nstr, string sidx)
        {
            var ls = File.ReadAllLines(fpath);
            for (int i = 0; i < ls.Length; ++i)
            {
                var l = ls[i];
                if (l.Contains("const ") || l.Contains("static "))
                {
                    Console.WriteLine("const_static:{0}->{1}", ostr, nstr);
                    l = l.Replace("\"" + ostr + "\"", "\"" + nstr + "\"");
                    if (l != ls[i])
                    {
                        ls[i] = l + "//JP: " + ostr;
                    }
                }
                else
                {
                    l = l.Replace("\"" + ostr + "\"", "CN_LANG(\"" + sidx + "\")");
                    if (l != ls[i])
                    {
                        ls[i] = l + "//ZH: " + nstr;
                    }
                    l = l.Replace(ostr, nstr );
                }
            }
            File.WriteAllLines(fpath, ls, Encoding.UTF8);
        }
    }
}
