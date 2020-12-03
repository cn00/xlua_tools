
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using XLua;
using XLua.LuaDLL;

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
        
        [DllImport("lua", CallingConvention = CallingConvention.Cdecl)]
        public static extern void doREPL(System.IntPtr L);

        [MonoPInvokeCallback(typeof(lua_CSFunction))]
        public static int LuaDoREPL(System.IntPtr L)
        {
             doREPL(L);
             return 0;
        }

        public static string ExecutableDir;
        public static void Main(string[] args)
        {
            // args = new[] {"/Volumes/Data/a3/tools/cslua/lua/bf.lua"};
            
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
                            + string.Format("package.cpath = package.cpath .. ';{0}lib?.dylib;{0}lib/lib?.dylib;{0}lib/?.dylib;{0}../lib/lib?.dylib;{0}../lib/?.dylib'", ExecutableDir)
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
                Console.Write("xlua");

                
                // // XLua.LuaDLL.Lua.lua_pushcclosure(L, (IntPtr)(pmain), 0);
                // XLua.LuaDLL.Lua.xlua_pushinteger(L, args.Length - 1);
                // luaenv.Translator.PushAny(L, args);
                // var ok = pmain(L);
                // Console.WriteLine($"lua return: {ok}");


                // LuaDoREPL(L);
                //
                // return;
                //*
                var fhistory = "xlua.history.lua";
                System.ReadLine.HistoryEnabled = false;
                var historyList = ReadLine.GetHistory();
                if (File.Exists(fhistory))
                {
                    historyList.AddRange(File.ReadAllLines(fhistory)
                        .GroupBy(i => i)
                        .Select(i => i.First()));
                    // ReadLine.AddHistory(historyList.ToArray());
                }

                var history = File.AppendText(fhistory);
                int historyIdx = 0;
                var cmd = "";
                while (cmd != "quit" && cmd != "exit")
                {
                    cmd = cmd.Trim().Replace("\0", "");
                    if(!historyList.Contains(cmd)){
                        history.WriteLine(cmd);
                        history.Flush();
                    }
                    if(cmd .Length > 0)
                        historyList.Add(cmd);
                    if(cmd == "cls")
                    {
                        Console.Clear();
                        goto next;
                    }
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
                            try
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
                            catch (LuaException e)
                            {
                                Console.WriteLine(e.Message);
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

                    next:
                    cmd = ReadLine.Read("> ");
                }// while
                history.Close();
                Console.WriteLine("\nexit no error.");
                // */
            }
        }
    }
}