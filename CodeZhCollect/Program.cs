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
using NPOI;

namespace CodeZhCollect
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("输入查找路径:");
            var inputdir = Console.ReadLine().Trim();
            var stringCount = 0;


            var excelName = inputdir + "/" + inputdir.Substring(inputdir.LastIndexOf(Path.DirectorySeparatorChar) + 1) +
                            ".xlsx";
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
            var sheet_jp_delta = workbook.Sheet("jp_delta"); //创建工作表

            int fidx = 1;
            foreach (var f in Directory.GetFiles(inputdir, "*.*", SearchOption.AllDirectories)
                                       .Where(f =>
                                           f.EndsWith(".cs")
                                           || f.EndsWith(".php")
                                           || f.EndsWith(".js")
                                           || f.EndsWith(".java")
                                           || f.EndsWith(".prefab")
                                           || f.EndsWith(".cpp")
                                           || f.EndsWith(".hpp")
                                           || f.EndsWith(".json")
                                       ).Where(f =>
                                           !f.Contains("/vitamin/Scene/debug/")
                                           && !f.Contains("/vitamin/images/")
                                           && !f.Contains("masterData")
                                       ))
            {
                var sheet = workbook.Sheet(string.Format("{0:00}-{1}", fidx, Path.GetFileName(f)));
                var info = sheet.Row(0);
                info.Cell(0).SetCellValue(f);
                var head = sheet.Row(1);
                head.Cell(0).SetCellValue("jp");
                head.Cell(2).SetCellValue("trans");
                CollectZhToSheet(f, sheet);
                fidx++;
            }

            excelStream = new FileStream(excelName, FileMode.OpenOrCreate);
            excelStream.Position = 0;
            workbook.Write(excelStream);
            excelStream.Close();

            Console.WriteLine("fineshed.");
        }

        // const string regular = "[\u3021-\u3126]";//jp //\u4e00-\u9fa5
        //const string regular = "[\u4e00-\u9fa5]"; //zh
        const string regular = "[\u3021-\u3126\u4e00-\u9fa5]"; //jp+zh

        public static void CollectZhToSheet(string inPath, ISheet sheet)
        {
            int rowidx = 2;
            var f = inPath;
            var lineCount = 0;
            //var stream = new StreamReader(f);
            var alllines = File.ReadAllLines(f);
            //while(stream.Peek() > 0)
            foreach (var line in alllines)
            {
                Console.WriteLine(">>>>>>{0}", f.upath());
                //var line = stream.ReadLine();
                ++lineCount;

                if (line.Contains("CCLOG") || line.Contains(":log("))
                {
                    continue;
                }

                var commLine = Regex.Matches(line, "^\\s*//*.*");
                if (commLine.Count > 0)
                {
                    //Console.WriteLine(line);
                    continue;
                }

                // "xxxxx"
                var matches = Regex.Matches(line, "\"[^\"]*" + regular + "+[^\"]*\"");
                foreach (var i in matches)
                {
                    var row = sheet.Row(rowidx);
                    var s0 = i.ToString().TrimStart('"').TrimEnd('"')
                              .Replace("\n", "\\n")
                              .Replace("\r", "\\r")
                        ;
                    row.Cell(2).SetCellValue(s0);
                    ++rowidx;
                }

                // // 'xxxx'
                // var matches2 = Regex.Matches(line, "'[^\"]*" + regular + "+[^']*'");
                // foreach (var i in matches2)
                // {
                //     
                // }
            }
        }
    }
}