using System;
using System.Diagnostics;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using app.Services;
using app.Views;
using XLua;
using System.Text;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;

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

            if (UseMockDataStore)
                DependencyService.Register<MockDataStore>();
            else
                DependencyService.Register<AzureDataStore>();
            MainPage = new MainPage();
        }

        protected override void OnStart()
        {
            var td = new TimeDebug("App.OnStartTimeDebug");
            td.Step("InitializeComponent");

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
                var lua = LuaSys.Instance;
                LuaSys.Instance.Init();
                
                td.Step("lua.AddBuildin");
                lua.DoString(Resource.GetStr("lua/main.lua"));
                td.Step("lua/main");

            }
            catch(Exception e)
            {
                Debug.WriteLine($"lua error: {e.Message}\n {e.StackTrace}");
            }

            mysqlTest();
            td.Step("mysqlTest");

        }

        void mysqlTest()
        {
            if (DeviceInfo.Platform == DevicePlatform.Android)
            {
                Debug.WriteLine($"{DeviceInfo.Platform} not support.");
                return;
            }
            // not support android
            var db = new MySql.Data.MySqlClient.MySqlConnection("Database=a3_m_308;Data Source=10.23.22.233;User Id=a3;Password=654123");
            if (db != null)
            {
                string myInsertQuery = "select * from m_string_item limit 100;";
                MySqlCommand cmd = new MySqlCommand(myInsertQuery);
                cmd.Connection = db;
                db.Open();
                var reader = cmd.ExecuteReader();
                var fcount = reader.FieldCount;
                if (reader.HasRows)
                {
                    var head = "";
                    for (var i = 0; i < fcount; ++i)
                    {
                        head += reader.GetName(i) + ":" + reader.GetFieldType(i) + "\t";
                    }
                    Debug.WriteLine(head);

                    var sb = new StringBuilder(2048);
                    var rc = reader.RecordsAffected;
                    while (reader.Read())
                    {
                        for (var i = 0; i < fcount; ++i)
                        {
                            sb.AppendFormat("{0}\t", reader.GetString(i).Replace("\n", "\\n"));
                        }
                        Debug.WriteLine($"{sb.ToString()}");
                        sb.Clear();
                    }

                }
                cmd.Connection.Close();
            }
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
