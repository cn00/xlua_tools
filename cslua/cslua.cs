
using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using NPOI.OpenXml4Net.OPC;
using XLua;
using Workbook = NPOI.XSSF.UserModel.XSSFWorkbook;

namespace CSLua
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

    internal class cslua
    {
        public static string ExecutableDir;
        public static void Main(string[] args)
        {
            // for (int i = 0; i < args.Length; i++)
            // {
            //     Console.WriteLine("args{0}: {1}", i, args[i]);
            // }
            
            
            // var path = "Master.xlsx";
            // // var inStream = new FileStream(path, FileMode.Open);
            // var wb = new Workbook(path);
            // var sheet = wb.GetSheet("jp");
            // Console.WriteLine($"wb={wb};sheet={sheet}");
            
                       
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

            ExecutableDir = Application.ExecutablePath.Replace(Path.GetFileName(Application.ExecutablePath), "");
            var mainlua = ExecutableDir + "lua/main.lua";
            if (args.Length > 0)
            {
                mainlua = args[0];
            }
            else
            {
                Console.WriteLine("run default entry lua/main.lua\n");
                Console.WriteLine("osx usage: mono cslua.exe path/to/entry.lua");
                Console.WriteLine("windows usage: cslua.exe path/to/entry.lua\n");
            }

            var initlua = ExecutableDir + "/init.lua";
            var l = LuaCallCSharpTypes.L;
            
            LuaEnv luaenv = LuaEnvSingleton.Instance;
            
            if(File.Exists(initlua))
                luaenv.DoFile(initlua);
            if (File.Exists(mainlua))
                luaenv.DoFile(mainlua);
        }
    }
}