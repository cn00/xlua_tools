using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using app.Models;
using NPOI;
using NPOI.XSSF.UserModel;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Essentials;

namespace app.Services
{
    public class MockDataStore : IDataStore<Item>
    {
        
        readonly List<Item> items;

        public MockDataStore()
        {
            items = new List<Item>();

            Stream stream = null;
            var fname = "a3-strings-305-202010151020.xlsx";

            string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), fname);
            if (File.Exists(filePath))
            {
                Debug.WriteLine($"use LocalApplicationData [{filePath}]");
                stream = File.OpenRead(filePath);
            }
            else
            {
                var assembly = this.GetType().Assembly;
                stream = assembly.GetManifestResourceStream($"app.Assets.{fname}");
                if (stream != null)
                {
                    Debug.WriteLine($"use ManifestResourceStream [{fname}]");
                    var fs = new FileStream(filePath, FileMode.CreateNew);
                    stream.CopyTo(fs);
                    fs.Close();
                    stream.Position = 0;
                }
            }

            if (stream == null)
            {
                Debug.WriteLine($"GetManifestResourceStream [{fname}] not found");
                return;
            }

            var t0 = DateTime.Now.ToFileTimeUtc();
            Func<string, float> deltat = (tag) =>
            {
                var delta = (DateTime.Now.ToFileTimeUtc() - t0) / 10000000f;
                t0 = DateTime.Now.ToFileTimeUtc();
                Debug.WriteLine($"{tag}: {delta:F}s");
                return delta;
            };

            var book = new XSSFWorkbook(stream);
            book.AllSheets().ForEach(i => {
                var start = i.FirstRowNum;
                for(var ri = start; ri < Math.Min(1000, i.LastRowNum); ++ri)
                {
                    items.Add(new Item(){
                        Id = i.Cell(ri, 0).SValue, //Guid.NewGuid().ToString(),
                        Us = i.Cell(ri, 1).SValue, //Guid.NewGuid().ToString(),
                        Ctg = i.Cell(ri, 2).SValue,//Guid.NewGuid().ToString(),
                        Text = i.Cell(ri, 3).SValue,
                        Description = i.Cell(ri, 4).SValue,
                    });
                }
            });
            deltat("ReadExcelSheets 1000 rows");
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