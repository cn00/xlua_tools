using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Android.OS;
using app.Models;
using NPOI;
using NPOI.XSSF.UserModel;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration;

namespace app.Services
{
    public class MockDataStore : IDataStore<Item>
    {
        
        readonly List<Item> items;

        public MockDataStore()
        {
            items = new List<Item>();
            Stream stream = null;
            var fpath = "a3-strings-305-202010151020.xlsx";
            if(File.Exists(fpath))
                stream = File.OpenRead(fpath);
            if(stream == null)switch (Device.RuntimePlatform)
            {
                case Device.Android:
                    stream = Android.App.Application.Context.Assets.Open(fpath);
                    break;
                case Device.iOS:
                case Device.UWP:
                case Device.macOS:
                default:
                    stream = File.OpenRead("Assets/a3-strings-305-202010151020.xlsx");
                    break;
            }

            var t0 = DateTime.Now.ToFileTimeUtc();
            Func<string, float> deltat = (tag) =>
            {
                var delta = (DateTime.Now.ToFileTimeUtc() - t0) / 10000000f;
                t0 = DateTime.Now.ToFileTimeUtc();
                Console.WriteLine($"{tag}: {delta}s");
                return delta;
            };

            var book = new XSSFWorkbook(stream);
            book.AllSheets().ForEach(i => {
                var start = i.FirstRowNum;
                for(var ri = start; ri < Math.Min(1000, i.LastRowNum); ++ri)
                {
                    items.Add(new Item(){
                        Id = i.Cell(ri, 0).SValue,//Guid.NewGuid().ToString(),
                        Us = i.Cell(ri, 1).SValue,//Guid.NewGuid().ToString(),
                        Ctg = i.Cell(ri, 2).SValue,//Guid.NewGuid().ToString(),
                        Text = i.Cell(ri, 3).SValue,
                        Description = i.Cell(ri, 4).SValue,
                    });
                }
            });
            deltat("ReadExcelSheets");
        }

        public void Add(Item item)
        {
            items.Add(item);
        }
        public async Task<bool> AddItemAsync(Item item)
        {
            items.Add(item);

            return await Task.FromResult(true);
        }

        public async Task<bool> UpdateItemAsync(Item item)
        {
            var oldItem = items.FirstOrDefault(arg => arg.Id == item.Id);
            items.Remove(oldItem);
            items.Add(item);

            return await Task.FromResult(true);
        }

        public async Task<bool> DeleteItemAsync(string id)
        {
            var oldItem = items.FirstOrDefault(arg => arg.Id == id);
            items.Remove(oldItem);

            return await Task.FromResult(true);
        }

        public async Task<Item> GetItemAsync(string id)
        {
            return await Task.FromResult(items.FirstOrDefault(s => s.Id == id));
        }

        public IEnumerable<Item> List(bool forceRefresh = false)
        {
            return items;
        }
        public async Task<IEnumerable<Item>> GetItemsAsync(bool forceRefresh = false)
        {
            return await Task.FromResult(items);
        }
    }
}