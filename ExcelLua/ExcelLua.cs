
using System;
using System.IO;
using System.Linq;
using NPOI.OpenXml4Net.OPC;
using XLua;
using Workbook = NPOI.XSSF.UserModel.XSSFWorkbook;

namespace ExcelUtil
{
    public class LuaEnvSingleton  {
	
        static private LuaEnv instance = null;
        static public LuaEnv Instance
        {
            get
            {
                if(instance == null)
                {
                    instance = new LuaEnv();
                    #if XLUA_GENERAL
                    instance.DoString(@"package.path = package.path
					    ..';lua/?.lua'
                        ..';../lua/?.lua'"
                    );
                    #endif
                }

                return instance;
            }
        }
    }

    internal class ExcelLua
    {
        public static void Main(string[] args)
        {
            var path = "Master.xlsx";
            // var inStream = new FileStream(path, FileMode.Open);
            var wb = new Workbook(path);
            var sheet = wb.GetSheet("jp");
            Console.WriteLine($"wb={wb};sheet={sheet}");
            
                       
            // var db = new Mono.Data.Sqlite.SqliteConnection("URI=file:strings.sqlite3;version=3");
            // db.Open();
            // db.Close();
            // var cmd = db.CreateCommand();
            // cmd.CommandText = "select * from strings_no_trans;";
            // var reader  = cmd.ExecuteReader();
            // Console.WriteLine($"{reader.GetName(0)}\t{reader.GetName(2)}\t{reader.GetName(3)}\t{reader.GetName(4)}\t{reader.GetName(5)}\t");
            // Console.WriteLine($"{reader.GetDataTypeName(0)}\t{reader.GetDataTypeName(2)}\t{reader.GetDataTypeName(3)}\t{reader.GetDataTypeName(4)}\t{reader.GetDataTypeName(5)}\t");
            // while(reader.Read())
            // {
            //     Console.WriteLine($"{reader.GetInt32(0)},\t{reader.GetTextReader(1).ReadToEnd()},\t {reader.GetTextReader(3).ReadToEnd()}");
            // }

            
            LuaEnv luaenv = LuaEnvSingleton.Instance;
            luaenv.DoString("require 'main'");
        }
    }
}