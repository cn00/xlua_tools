using System;
using System.Diagnostics;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using app.Services;
using app.Views;
using XLua;
using System.Text;

namespace app
{

    public partial class App : Application
    {
        //TODO: Replace with *.azurewebsites.net url after deploying backend to Azure
        //To debug on Android emulators run the web backend against .NET Core not IIS
        //If using other emulators besides stock Google images you may need to adjust the IP address
        public static string AzureBackendUrl =
            DeviceInfo.Platform == DevicePlatform.Android ? "http://10.0.2.2:5000" : "http://localhost:5000";
        public static bool UseMockDataStore = true;

        public byte[] Require(ref string lpath)
        {
            string luapath = "lua/" + lpath;
            if (string.IsNullOrEmpty(luapath))
                return null;

            var LuaExtension = ".lua";

            byte[] bytes = null;
            var assetName = luapath.Replace(".", "/") + LuaExtension;
            {
                var data = Resource.GetStr(assetName);
                bytes = Encoding.Default.GetBytes(data);
            }

            return bytes;
        }

        public App()
        {
            InitializeComponent();

            for (var i = Environment.SpecialFolder.Desktop; i < Environment.SpecialFolder.CDBurning; ++i)
            {
                try
                {
                    Console.WriteLine($"Environment.GetFolderPath({i}):{Environment.GetFolderPath(i)}");
                }
                catch (Exception e)
                {
                    // Console.WriteLine(e);
                }
            }
            try
            {
                var lua = new LuaEnv();
                lua.AddBuildin("mime.core", XLua.LuaDLL.Lua.LoadSocketMime);
                lua.AddBuildin("lpeg", XLua.LuaDLL.Lua.LoadLpeg);
                lua.AddBuildin("ffi", XLua.LuaDLL.Lua.LoadFfi);
                lua.AddBuildin("lfb", XLua.LuaDLL.Lua.LoadLfb);
                lua.AddBuildin("nslua", XLua.LuaDLL.Lua.LoadNSLua);
                lua.AddBuildin("p7zip", XLua.LuaDLL.Lua.LoadP7zip);
                lua.AddBuildin("lsqlite3", XLua.LuaDLL.Lua.LoadLSQLite3);
                lua.AddBuildin("lxp", XLua.LuaDLL.Lua.LoadLxp);
                lua.AddBuildin("luasql.mysql", XLua.LuaDLL.Lua.LoadLuaSqlMysql);
                //lua.AddLoader(Require);
                lua.DoString(Resource.GetStr("lua/main.lua"));
            }
            catch(Exception e)
            {
                Debug.WriteLine($"lua error: {e.Message}\n {e.StackTrace}");
            }

            if (UseMockDataStore)
                DependencyService.Register<MockDataStore>();
            else
                DependencyService.Register<AzureDataStore>();
            MainPage = new MainPage();
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
