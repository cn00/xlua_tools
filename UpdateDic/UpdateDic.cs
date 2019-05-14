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

namespace UpdateDic
{
    public class Item
    {
        public string jp;
        public string tr;
        public string note;
    }
    public class Program
    {
        static void Main(string[] args)
        {
            string base_dic_xlsx_path, new_dic_xlsx_dir;
            if (args.Length == 2)
            {
                base_dic_xlsx_path = args[0].upath();
                new_dic_xlsx_dir = args[1].upath();
            }
            else
            {

                Console.WriteLine("输入字典 base Excel 文件路径:");
                base_dic_xlsx_path = Console.ReadLine().TrimEnd(' ', '\t', '\r').upath();
                Console.WriteLine("输入新增字典Excel目录:");
                new_dic_xlsx_dir = Console.ReadLine().TrimEnd(' ', '\t', '\r').upath();
            }
            Console.WriteLine("{0}, {1}", base_dic_xlsx_path, new_dic_xlsx_dir);

            XSSFWorkbook baseworkbook = null;
            FileStream excelStream = null;
            if (File.Exists(base_dic_xlsx_path))
            {
                excelStream = new FileStream(base_dic_xlsx_path, FileMode.Open);
                excelStream.Position = 0;
                baseworkbook = new XSSFWorkbook(excelStream);
                excelStream.Close();
            }
            else
            {
                baseworkbook = new XSSFWorkbook();
            }
            
            var dic = new Dictionary<string, Item>();
            var base_sheet = baseworkbook.Sheet("dic");
            var oldcount = BuildDic(ref dic, base_sheet);


            int addcount = 0;
            foreach (var f in Directory.GetFiles(new_dic_xlsx_dir, "*.xlsx", SearchOption.AllDirectories))
            {
                // new
                XSSFWorkbook newworkbook = null;
                if (File.Exists(f))
                {
                    excelStream = new FileStream(f, FileMode.Open);
                    excelStream.Position = 0;
                    newworkbook = new XSSFWorkbook(excelStream);
                    excelStream.Close();
                }

                foreach (ISheet sheet in newworkbook)
                {
                    addcount += BuildDic(ref dic, sheet);
                }
            }
            Console.WriteLine("原有:{0}, 新增:{1}", oldcount, addcount);

            // update to base excel
            int idx = 1;
            foreach (var it in dic)
            {
                var row = base_sheet.Row(idx);
                row.Cell(0).SetCellValue(it.Key);
                row.Cell(1).SetCellValue(it.Value.tr);
                row.Cell(2).SetCellValue(it.Value.note);
                ++idx;
            }
            
            if (File.Exists(base_dic_xlsx_path))
                File.Delete(base_dic_xlsx_path);
            excelStream = new FileStream(base_dic_xlsx_path, FileMode.OpenOrCreate);
            excelStream.Position = 0;
            baseworkbook.Write(excelStream);
            excelStream.Close();
            
            var os = Environment.OSVersion.ToString();
            if (!os.Contains("Unix"))
            {
                Console.WriteLine("按 Enter 退出");
                Console.ReadLine();
            }

        }//main

        static int BuildDic(ref Dictionary<string, Item> dic, ISheet sheet)
        {
            int addcount = 0;
            for (int i = sheet.FirstRowNum + 1; i < sheet.LastRowNum && i < 50000; i++)
            {
                var row = sheet.Row(i);
                var k = row.Cell(0).SValue()
                           .Replace("\r", "\\r")
                           .Replace("\n", "\\n")
                           .TrimStart('"').TrimEnd('"');
                var v = row.Cell(1).SValue()
                           .Replace("\r", "\\r")
                           .Replace("\n", "\\n")
                           .TrimStart('"').TrimEnd('"');
                var note = row.Cell(2).SValue()
                           .Replace("\r", "\\r")
                           .Replace("\n", "\\n")
                           .TrimStart('"').TrimEnd('"');
                Item vv = null;
                if (dic.TryGetValue(k, out vv))
                {
                    if (vv.tr != v)
                    {
                        Console.WriteLine("[{0}]:[{1}][{2}]", k, v, vv.tr);
                    }
                }
                else if(v.Length > 0)
                {
                    dic[k] = new Item()
                    {
                        jp = k,
                        tr = v,
                        note = note.Length > 0 ? note : sheet.SheetName,
                    };
                    ++addcount;
                }
            }

            return addcount;
        }
    }
}