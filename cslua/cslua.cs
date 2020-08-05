
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
            // args = new[] {"/Volumes/Data/a3/c2/client/Unity/Tools/excel/lua/sqlutil.lua"};
            
            // for (int i = 0; i < args.Length; i++)
            // {
            //     Console.WriteLine("args{0}: {1}", i, args[i]);
            // }
            
            var l = LuaCallCSharpTypes.L;
            LuaEnv luaenv = LuaEnvSingleton.Instance;
            
            ExecutableDir = Application.ExecutablePath.Replace(Path.GetFileName(Application.ExecutablePath), "");
            var mainlua = ExecutableDir + "lua/main.lua";
            if (args.Length > 0)
            {
                mainlua = args[0];
            
                var initlua = ExecutableDir + "init.lua";
            
                LuaTable env =luaenv.NewTable();
                env.Set("__index",    luaenv.Global);
                env.Set("__newindex", luaenv.Global);

                LuaTable argv =luaenv.NewTable();
                for(int i = 0; i < args.Length; ++i)
                {
                    // Console.WriteLine($"cs-argv[{i}] = {args[i]}");
                    argv.Set(i, args[i]);
                }
                env.Set("argv", argv);
                env.SetMetaTable(env);

                if(File.Exists(initlua))
                    luaenv.DoFile(initlua);
                if (File.Exists(mainlua))
                    luaenv.DoFile(mainlua, env);
            }
            else
            {
                // Console.WriteLine("run default entry lua/main.lua");
                Console.WriteLine("osx usage: mono cslua.exe path/to/entry.lua");
                Console.WriteLine("windows usage: cslua.exe path/to/entry.lua");
                Console.Write("Console Module.\ncslua$ ");
                var cmd = Console.ReadLine();
                while (cmd != "quit" && cmd != "exit")
                {
                    if (!cmd.Contains(" ") && !cmd.Contains("(") && !cmd.Contains("="))
                        cmd = "print(" + cmd + ")";
                    luaenv.DoString(cmd);
                    Console.Write("$ ");
                    cmd = Console.ReadLine();
                }
            }
        }
    }
}