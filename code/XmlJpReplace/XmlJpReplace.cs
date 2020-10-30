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

namespace XmlJpReplace
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("输入查找路径:");
            // var inputdir = Console.ReadLine() ?? "Resources";


            var excelName = "ui.spritebuilder/ui.spritebuilder-ccb.xlsx";//inputdir + "/" + inputdir.Substring(inputdir.LastIndexOf(Path.DirectorySeparatorChar) + 1) + ".xlsx";
            XSSFWorkbook workbook = null;
            FileStream excelStream = null;
            if (File.Exists(excelName))
            {
                excelStream = new FileStream(excelName, FileMode.Open);
                excelStream.Position = 0;
                workbook = new XSSFWorkbook(excelStream);
                excelStream.Close();
            }
            else
            {
                workbook = new XSSFWorkbook();
            }

            var sheet_jp = workbook.Sheet("jp"); //创建工作表
            for (int i = 1; i < sheet_jp.LastRowNum && i < 20000; ++i)
            {
                var row = sheet_jp.Row(i);
                var srcPath = row.Cell(3).StringCellValue;
                var jp = row.Cell(0).SValue().Replace("\\r", "\r").Replace("\\n", "\n");
                var tr = row.Cell(1).SValue().Replace("\\r", "\r").Replace("\\n", "\n");
                if (File.Exists(srcPath) && jp.Length > 0 && tr.Length > 0 && tr != "0")
                {
                    ReplaceOne(srcPath, jp, tr);
                }
            }
            
        }

        public static void ReplaceOne(string srcPath, string jp, string tr)
        {
            string srcc = File.ReadAllText(srcPath);
            var nsrcc = srcc.Replace(">" + jp + "<", ">" + tr + "<");
            if (nsrcc == srcc)
            {
                Console.WriteLine("{0}:{1}:{2} NOT Replaced", jp, tr, srcPath);
            }
            else
            {
                File.WriteAllText(srcPath, nsrcc);
            }
        }
    }
}