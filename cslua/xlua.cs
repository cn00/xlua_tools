
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Policy;
using NPOI.XSSF.UserModel;
using XLua;
using XLua.LuaDLL;
// using PinYinConverter;
using LuaAPI = XLua.LuaDLL.Lua;

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
                        
            // args = new[] {"/Users/cn/a3/c308/client/Unity/Tools/excel/lua/app/CollectImg2Excel.lua"};

            // // no use
            // AppDomain.CurrentDomain.AssemblyResolve += delegate(object sender, ResolveEventArgs eventArgs)
            // {
            //     string assemblyFile = (eventArgs.Name.Contains(','))
            //         ? eventArgs.Name.Substring(0, eventArgs.Name.IndexOf(','))
            //         : eventArgs.Name;
            //
            //     assemblyFile += ".dll";
            //
            //     // // Forbid non handled dll's
            //     // if (!LOAD_ASSEMBLIES.Contains(assemblyFile))
            //     // {
            //     //     return null;
            //     // }
            //
            //     string absoluteFolder = new FileInfo((new System.Uri(Assembly.GetExecutingAssembly().CodeBase)).LocalPath).Directory.FullName;
            //     string targetPath = Path.Combine(absoluteFolder, assemblyFile);
            //
            //     try
            //     {
            //         Console.WriteLine($"try load:{targetPath}");
            //         return Assembly.LoadFile(targetPath);
            //     }
            //     catch (Exception)
            //     {
            //         return null;
            //     }
            //     Console.WriteLine($"load:{targetPath}");
            // };
           
            
            // // no use 2
            // var PrivateBinPath = AppDomain.CurrentDomain.SetupInformation.PrivateBinPath + (System.IO.Path.PathSeparator+AppDomain.CurrentDomain.BaseDirectory+"lib");
            // AppDomain.CurrentDomain.SetupInformation.PrivateBinPath = PrivateBinPath;
            // Console.WriteLine($"PathSeparator:{System.IO.Path.PathSeparator}\nPrivateBinPath:{PrivateBinPath}=>{AppDomain.CurrentDomain.SetupInformation.PrivateBinPath}\nBaseDirectory:{AppDomain.CurrentDomain.BaseDirectory} ");
            
            
            // for (int i = 0; i < args.Length; i++)
            // {
            //     Debug.WriteLine("args{0}: {1}", i, args[i]);
            // }

            // var w = AppDomain.CurrentDomain.GetAssemblies();
            // var asm = Assembly.LoadFrom("");
            // asm.GetExportedTypes();

            // Activator.CreateInstance(typeof(String));
            
            // var l = LuaCallCSharpTypes.L;

            // var size = NPOI.SS.Util.ImageUtils.GetImageDimension(null);
            // var wb = new XSSFWorkbook();
            // var sheet = wb.GetSheet("");
            // var picInd = wb.AddPicture(new FileStream("", FileMode.Open), 6);
            // var helper = wb.GetCreationHelper();
            // var drawing = sheet.CreateDrawingPatriarch();
            // var anchor = helper.CreateClientAnchor();
            // anchor.Col1 = 0;
            // anchor.Col2 = 0;
            // anchor.Row1 = 5;
            // var pict = drawing.CreatePicture(anchor, picInd);
            // pict.Resize();

            
            LuaEnv luaenv = LuaEnvSingleton.Instance;
            var L = luaenv.L;
            if (0 == LuaAPI.xlua_getglobal(L, "_VERSION"))
            {
                Console.WriteLine($"{LuaAPI.lua_tostring(L, -1)}");
            }

            ExecutableDir = AppDomain.CurrentDomain.BaseDirectory.Replace("\\", "/");// Application.ExecutablePath.Replace(Path.GetFileName(Application.ExecutablePath), "");
        
            luaenv.DoString("package.cpath = package.cpath .. ';./lib?.dylib;./?.dylib';"
                            + string.Format("package.cpath = package.cpath .. ';{0}lib?.dylib;{0}lib/lib?.dylib;{0}lib/?.dylib;{0}../lib/lib?.dylib;{0}../lib/?.dylib'", ExecutableDir)
            );
            
            var initlua = ExecutableDir + "init.lua";
            if(File.Exists(initlua))
                luaenv.DoFile(initlua);
            
            var mainlua = ExecutableDir + "lua/main.lua";
            if (args.Length > 0)
            {
                mainlua = args[0];
                var maindir = mainlua.Substring(0, mainlua.LastIndexOf("/"));
                luaenv.DoString(string.Format("package.path = package.path .. ';{0}/../?.lua;{0}/?.lua;{0}/lua/?.lua;'", maindir)
                                    + @"package.path = package.path .. ';lua/?.lua' .. ';../lua/?.lua';"
                                    + string.Format("package.path = package.path .. ';{0}/?.lua;;{0}/lua/?.lua;'", ExecutableDir));
            
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
                luaenv.DoString(@"package.path = package.path .. ';lua/?.lua' .. ';../lua/?.lua';"
                                    + string.Format("package.path = package.path .. ';{0}/?.lua;;{0}/lua/?.lua;'", ExecutableDir));
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