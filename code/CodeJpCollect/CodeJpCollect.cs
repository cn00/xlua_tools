using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    
namespace Getjp
{
    class Program
    {
        const string regular = "[\u3021-\u3126]";//jp //\u4e00-\u9fa5
        //const string regular = "[\u4e00-\u9fa5]"; //zh
        // const string regular = "[\u3021-\u3126\u4e00-\u9fa5]"; //jp+zh

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
                    rowl.Cell(1).SetCellValue(rec.tr);
                }
                else
                {
                    var row = deltaSheet.Row(deltaIdx);
                    row.Cell(0).SetCellValue(rowl.Cell(0).SValue);
                    row.Cell(1).SetCellValue(rowl.Cell(1).SValue);
                    row.Cell(2).SetCellValue(rowl.Cell(2).SValue);
                    row.Cell(3).SetCellValue(rowl.Cell(3).SValue);
                    row.Cell(4).SetCellValue(rowl.Cell(4).SValue);
                    row.Cell(5).SetCellValue(rowl.Cell(5).SValue);
                    ++deltaIdx;
                }
            }
        }

        static void Main(string[] args)
        {
            Debug.WriteLine("输入查找路径:");
            var inputdir = Console.ReadLine().Trim();
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
            var sheet3 = workbook.Sheet("ini");
            //CollectTranslateJsonToExcel("old-json.txt", sheet_old,
                //"/Users/men/ws/c2/Resources/vitamin/data/v2040/language_cn.ini", sheet3);

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
                    f.EndsWith(".cs")
                    || f.EndsWith(".php")
                    || f.EndsWith(".js")
                    || f.EndsWith(".java")
                    || f.EndsWith(".prefab")
                    || f.EndsWith(".cpp")
                    || f.EndsWith(".hpp")
                    || f.EndsWith(".json")
                    || f.EndsWith(".sql")
                )
                // .Where(f => 
                //        !f.Contains("/vitamin/Scene/debug/")
                //     && !f.Contains("/vitamin/images/")
                //     && !f.Contains("masterData")
                //     && !f.Contains("stable.json")
                //     )
            )
            {
                Debug.WriteLine("{0}", f);

                var lineCount = 0;
                //var stream = new StreamReader(f);
                var alllines = File.ReadAllLines(f);
                //while(stream.Peek() > 0)
                foreach (var l in alllines)
                {
//                    Debug.WriteLine(">>>>>>{0}", f.upath());
                    //var line = stream.ReadLine();
                    ++lineCount;
                    var line = l.Trim('\t', ' ');

                    if (line.StartsWith("CCLOG") || line.Contains(":log(")||line.StartsWith("echo"))
                    {
                        continue;
                    }

                    var commLine = Regex.Matches(line, "^\\s*//*.*");
                    if (commLine.Count > 0)
                    {
                        //Debug.WriteLine(line);
                        continue;
                    }

                    // // "xxxxx"
                    // var matches = Regex.Matches(line, "\"[^\"']*" + regular + "+[^\"]*\"");
                    // foreach (var i in matches)
                    // {
                    //     ++delta_lang_idx;
                    //     ++stringCount;
                    //     row = sheet_jp.Row(stringCount);
                    //     var s0 = i.ToString().TrimStart('"').TrimEnd('"').Replace("\n", "\\n");
                    //     row.Cell(0).SetCellValue(s0);
                    //     row.Cell(1).SetCellValue("译文");
                    //     row.Cell(2).SetCellValue(lineCount);
                    //     row.Cell(3).SetCellValue(f.upath());
                    //     row.Cell(4).SetCellValue(line.Trim());
                    //     row.Cell(5).SetCellValue(string.Format("{0:000000}", (delta_lang_idx)));
                    //     var s = string.Format("{0:000000}#{1}#{2}#{3}\n", delta_lang_idx, f.upath(), lineCount, s0);
                    //     textStream.Write(s);
                    //     fini.WriteLine(string.Format("{0:000000}={1}", delta_lang_idx, i.ToString().TrimStart('"').TrimEnd('"')));
                    //     Debug.WriteLine("{0}:{1}:{2}", stringCount, lineCount, i, f.upath());
                    // }

                    // 'xxxx'
                    var matches2 = Regex.Matches(line, "'[^'\"]*" + regular + "+[^']*'");
                    foreach (var i in matches2)
                    {
                        ++delta_lang_idx;
                        ++stringCount;
                        row = sheet_jp.Row(stringCount);
                        var s0 = i.ToString().TrimStart('\'').TrimEnd('\'').Replace("\n", "\\n");
                        if (s0.Length > 32766)
                        {
                            Debug.WriteLine("too long match [{0} ...] skip", s0.Substring(0, 20));
                            continue;
                        }
                        row.Cell(0).SetCellValue(s0);
                        row.Cell(1).SetCellValue("译文");
                        row.Cell(2).SetCellValue(lineCount);
                        row.Cell(3).SetCellValue(f);
                        // row.Cell(4).SetCellValue(line);
                        row.Cell(5).SetCellValue(string.Format("{0:000000}", (delta_lang_idx)));
                        var s = string.Format("{0:000000}#{1}#{2}#{3}\n", delta_lang_idx, f.upath(), lineCount, s0);
                        textStream.Write(s);
                        fini.WriteLine(string.Format("{0:000000}={1}", delta_lang_idx, i.ToString().TrimStart('"').TrimEnd('"')));
                        Debug.WriteLine("{0}:{1}:{2}", stringCount, lineCount, i, f.upath());
                    }
                    
                    //php >xxxx<
                    var matches3 = Regex.Matches(line, ">[^>\"']*" + regular + "+[^<\"']*<");
                    foreach (var i in matches3)
                    {
                        ++delta_lang_idx;
                        ++stringCount;
                        row = sheet_jp.Row(stringCount);
                        var s0 = i.ToString().TrimStart('>').TrimEnd('<').Replace("\n", "\\n");
                        row.Cell(0).SetCellValue(s0);
                        row.Cell(1).SetCellValue("译文");
                        row.Cell(2).SetCellValue(lineCount);
                        row.Cell(3).SetCellValue(f);
                        // row.Cell(4).SetCellValue(line);
                        row.Cell(5).SetCellValue(string.Format("{0:000000}", (delta_lang_idx)));
                        var s = string.Format("{0:000000}#{1}#{2}#{3}\n", delta_lang_idx, f.upath(), lineCount, s0);
                        textStream.Write(s);
                        fini.WriteLine(string.Format("{0:000000}={1}", delta_lang_idx, i.ToString().TrimStart('"').TrimEnd('"')));
                        Debug.WriteLine("{0}:{1}:{2}", stringCount, lineCount, i, f.upath());
                    }

                } //while

                textStream.Flush();
            } // foreach
            
            
            Delta(sheet_jp, sheet_old, sheet_jp_delta);
            
            
            textStream.Close();


            excelStream = new FileStream(excelName, FileMode.OpenOrCreate);
            excelStream.Position = 0;
            workbook.Write(excelStream);
            excelStream.Close();
            fini.Close();

            // Debug.WriteLine("按 Enter 退出");
            // Console.ReadLine();
        }
    }
}