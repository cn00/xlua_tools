
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using XLua;

namespace xlua
{
    public class LuaEnvSingleton  {
	
        static private LuaEnv instance = null;
        static public LuaEnv Instance
        {
            get
            {
                if(instance == null)
                {
                    try
                    {
                        instance = new LuaEnv();
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e);
                    }
                }

                return instance;
            }
        }
    }

    internal class xlua
    {
        public static string ExecutableDir;
        public static void Main(string[] args)
        {
            // args = new[] {"/Volumes/Data/a3/c3/client/Unity/Tools/excel/lua/ks3.lua"};
            
            // for (int i = 0; i < args.Length; i++)
            // {
            //     Debug.WriteLine("args{0}: {1}", i, args[i]);
            // }

            // var w = AppDomain.CurrentDomain.GetAssemblies();
            // var asm = Assembly.LoadFrom("");
            // asm.GetExportedTypes();

            // Activator.CreateInstance(typeof(String));
            
            // var l = LuaCallCSharpTypes.L;

            ExecutableDir = AppDomain.CurrentDomain.BaseDirectory;// Application.ExecutablePath.Replace(Path.GetFileName(Application.ExecutablePath), "");
        
            LuaEnv luaenv = LuaEnvSingleton.Instance;
            var L = luaenv.L;
            luaenv.DoString(@"package.path = package.path .. ';lua/?.lua' .. ';../lua/?.lua';"
                            + string.Format("package.path = package.path .. ';{0}/?.lua;'", ExecutableDir)
                            +       "package.cpath = package.cpath .. ';./lib?.dylib;./?.dylib';"
                            + string.Format("package.cpath = package.cpath .. ';{0}/lib?.dylib;{0}/?.dylib'", ExecutableDir)
            );
            
            var initlua = ExecutableDir + "init.lua";
            if(File.Exists(initlua))
                luaenv.DoFile(initlua);
            
            var mainlua = ExecutableDir + "lua/main.lua";
            if (args.Length > 0)
            {
                mainlua = args[0];
            
                LuaTable env =luaenv.NewTable();
                env.Set("__index",    luaenv.Global);
                env.Set("__newindex", luaenv.Global);

                LuaTable argv =luaenv.NewTable();
                for(int i = 0; i < args.Length; ++i)
                {
                    // Debug.WriteLine($"cs-argv[{i}] = {args[i]}");
                    argv.Set(i, args[i]);
                }
                env.Set("argv", argv);
                env.SetMetaTable(env);

                if (File.Exists(mainlua))
                    luaenv.DoFile(mainlua, env);
            }
            else
            {
                // Debug.WriteLine("run default entry lua/main.lua");
                Debug.WriteLine(" usage:\n\tosx/unix: mono xlua.exe path/to/entry.lua");
                Debug.WriteLine("\twindows: xlua.exe path/to/entry.lua");
                Debug.WriteLine("Or type lua code in Interaction Mode\nGood luck.");
                Console.Write("xlua> ");
                var history = File.AppendText("xlua.history.lua");
                var cmd = Console.ReadLine();
                while (cmd != "quit" && cmd != "exit")
                {
                    // var c = Console.ReadKey();
                    // Debug.WriteLine(c.Key);
                    
                    cmd = cmd.Trim().Replace("\0", "");
                    if (cmd != "" 
                        // && !cmd.Contains(" ") 
                        && !cmd.Contains("=") 
                        && !cmd.Contains("print")
                     )
                    {
                        if(!cmd.Contains(",")
                            && !cmd.Contains(" ") 
                        )
                            cmd = "return  tostring(" + cmd + ")";
                        else
                            cmd = "return " + cmd;
                    }
                    
                    try
                    {
                        if (cmd.Length > 0)
                        {
                            var ret = luaenv.DoString(cmd);
                            if (ret != null && ret.Length > 0)
                            {
                                foreach (var o in ret)
                                {
                                    // ObjectTranslatorPool.Instance.Find(L).PushAny(L, o);
                                    // var v = o is null ? XLua.LuaDLL.Lua.lua_tostring(L, -1) : o.ToString();
                                    var v = o is null ? "nil or native_ptr try tostring(obj) again" : o.ToString();
                                    Console.Write("{0}\t", v);
                                }
                                Console.Write("\n");
                            }
                        }
                    }
                    catch (LuaException e)
                    {
                        Debug.WriteLine(e.Message);
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e.Message + "\n#trace: \n" + e.StackTrace);
                    }

                    history.WriteLine(cmd);
                    history.Flush();
                    Console.Write("> ");
                    cmd = Console.ReadLine();
                }
                history.Close();
            }
        }
    }
}