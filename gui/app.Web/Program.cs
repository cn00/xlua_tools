using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Xamarin.Forms;
using app.Services;
using app.Views;

namespace app.Web
{
    // public class Program
    public partial class App : Application
    {
        public App()
        {
            DependencyService.Register<MockDataStore>();
            MainPage = new MainPage();
        }

        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}