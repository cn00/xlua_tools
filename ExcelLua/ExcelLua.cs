
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
            
            // var types = XLuaConfig.LuaCallCSharp.ToList();
            // for (var i =0; i < types.Count; ++i)
            // {
            //     Console.WriteLine($"{i}= {types[i].FullName}");
            // }
            
            LuaEnv luaenv = LuaEnvSingleton.Instance;
            luaenv.DoString("require 'main'");
        }
    }
}