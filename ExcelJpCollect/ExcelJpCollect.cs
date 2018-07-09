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
using NPOI.OpenXmlFormats.Spreadsheet;
using NPOI;

public static class StringExtension
{
    public static string upath(this string self)
    {
        return self.Replace("\\", "/");
    }
}

namespace CollectExcelJp
{
    class ExcelJpCollect
    {
        IWorkbook AllInOnebook = null;
        static void Main(string[] args)
        {
            begin:
            var inputdir = "D:/a3/client/Unity/Assets/Application/Resource/ExcelData";
            Console.WriteLine("输入待翻译 Excel 根路径, 默认值: " + inputdir);
            Console.Write("输入或拖入: ");
            var tmp = Console.ReadLine();
            if(!string.IsNullOrWhiteSpace(tmp))
                inputdir = tmp;
            if(!Directory.Exists(inputdir))
            {
                goto begin;
            }

            inputdir = inputdir.upath();
            Console.WriteLine("待翻译 Excel 根路径为: " + inputdir);
            var outdir = inputdir + ".jp";

            var AllInOnebook = new XSSFWorkbook();
            int count = 0;
            var outSheet = AllInOnebook.Sheet("jp");//new XSSFSheet();//
            int tmi = -1;
            outSheet.Cell(0, ++tmi).SetCellValue("jp");
            outSheet.Cell(0, ++tmi).SetCellValue("trans");
            outSheet.Cell(0, ++tmi).SetCellValue("trans_jd");
            outSheet.Cell(0, ++tmi).SetCellValue("i");
            outSheet.Cell(0, ++tmi).SetCellValue("j");
            outSheet.Cell(0, ++tmi).SetCellValue("SheetName");
            outSheet.Cell(0, ++tmi).SetCellValue("FilePath");


            foreach(var f in Directory.GetFiles(inputdir, "*.*", SearchOption.AllDirectories)
                .Where(f => f.EndsWith(".xls") || f.EndsWith(".xlsx")))
            {
                var fin = f.upath();
                var b = Collect(fin, fin.Replace(inputdir, outdir));
                //Console.WriteLine(fin);
            }// foreach

            Console.WriteLine("按 Enter 退出");
            Console.ReadLine();
        }//end main

        const string regular = "[\u3021-\u3126]";
        static int colCount = 0;
        public static bool Collect(string inExcel, string outExcel)
        {
            bool b = false;
            var inStream = new FileStream(inExcel, FileMode.Open);

            IWorkbook inbook = null;
            if(inExcel.EndsWith("xls"))
            {
                inbook = new HSSFWorkbook(inStream);
                outExcel += "x";
            }
            else if(inExcel.EndsWith(".xlsx"))
            {
                inbook = new XSSFWorkbook(inStream);
            }

            //template
            var templateStream = new FileStream("d:/a3/client/docs/protected.xlsx", FileMode.Open);
            var outBook = new XSSFWorkbook(templateStream);
            XSSFSheet outSheet = outBook.GetSheet("jp") as XSSFSheet;//new XSSFSheet();//
            templateStream.Close();


            int totalCount = 0;
            //var outSheet = outBook.Sheet("jp");//new XSSFSheet();//
            //outBook.Add(outSheet as XSSFSheet);
            var hrow = outSheet.GetRow(0);
            var head = new ExcelHead(hrow);

            var locked = outBook.CreateCellStyle();
            locked.IsLocked = true;
            locked.WrapText = true;
            locked.ShrinkToFit = true;

            var nolocked = outBook.CreateCellStyle();
            nolocked.IsLocked = false;
            nolocked.WrapText = true;
            nolocked.ShrinkToFit = true;

            outSheet.SetDefaultColumnStyle(head[HeadIdx.jp], locked);
            outSheet.SetDefaultColumnStyle(head[HeadIdx.trans], nolocked);
            outSheet.SetDefaultColumnStyle(head[HeadIdx.trans_jd], nolocked);
            outSheet.SetDefaultColumnStyle(head[HeadIdx.i], locked);
            outSheet.SetDefaultColumnStyle(head[HeadIdx.j], locked);
            outSheet.SetDefaultColumnStyle(head[HeadIdx.SheetName], locked);

            int count = 0;
            bool cellLock = true;
            foreach(var sheet in inbook.AllSheets())
            {
                for(int i = 0; i <= sheet.LastRowNum; ++i)
                {
                    var row = sheet.Row(i);
                    for(int j = 0; j < row.LastCellNum; ++j)
                    {
                        var v = row.Cell(j).SValue();
                        var matches = Regex.Matches(v, regular + "+.*");
                        if(matches.Count > 0)
                        {
                            ++count;
                            ++totalCount;

                            var c = outSheet.Cell(count, head[HeadIdx.jp]);
                            c.SetCellValue(v);
                            //c.CellStyle.IsLocked = cellLock;

                            c = outSheet.Cell(count, head[HeadIdx.trans]);
                            c.SetCellValue("译文");
                            //c.CellStyle.IsLocked = false;

                            c = outSheet.Cell(count, head[HeadIdx.trans_jd]);
                            c.SetCellValue("校对");
                            //c.CellStyle.IsLocked = false;

                            c = outSheet.Cell(count, head[HeadIdx.i]);
                            c.SetCellValue(i);
                            //c.CellStyle.IsLocked = cellLock;

                            c = outSheet.Cell(count, head[HeadIdx.j]);
                            c.SetCellValue(j);
                            //c.CellStyle.IsLocked = cellLock;

                            c = outSheet.Cell(count, head[HeadIdx.SheetName]);
                            c.SetCellValue(sheet.SheetName);
                            //c.CellStyle.IsLocked = cellLock;
                        }
                    }
                }
                outSheet.AutoSizeColumn(0, false, 64);

                //if(count == 0)
                //{
                //    var b = outBook.Remove(outSheet);
                //    outBook.FirstVisibleTab = 0;
                //    if(b == false)
                //    {
                //        Console.WriteLine("remove failed.");
                //    }
                //}
            }

            //var pro = outSheet.AddProtection("654123");

            inStream.Close();

            if(totalCount > 0)
            {
                ++colCount;
                b = true;

                var infoSheet = outBook.Sheet("info");//
                infoSheet.Cell(0, 0).SetCellValue(inExcel);

                var outDir = Path.GetDirectoryName(outExcel);
                if(!Directory.Exists(outDir))
                {
                    Directory.CreateDirectory(outDir);
                }
                var outStream = new FileStream(outExcel, FileMode.Create);
                outStream.Position = 0;

                // no use
                //outBook.LockRevision();
                //outBook.LockStructure();
                //outBook.LockWindows();

                outBook.Write(outStream);
                outStream.Close();
                Console.WriteLine(colCount + " >>> " + outExcel);
            }
            return b;
        }
    }
}
