using System;
using System.IO;
using System.Text;

namespace app.Services
{
    public class Resource
    {
        public Resource()
        {
        }

        public static string GetStr(string subpath)
        {
            string res = null;
            Stream stream = null;
            string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), subpath);
            if (File.Exists(filePath))
            {
                Console.WriteLine($"use LocalApplicationData [{filePath}]");
                stream = File.OpenRead(filePath);
            }
            else
            {
                var assembly = typeof(Resource).Assembly;
                stream = assembly.GetManifestResourceStream($"app.Assets.{subpath.Replace("/", ".")}");
                if (stream != null)
                {
                    Console.WriteLine($"use ManifestResourceStream [{subpath}]");
                    var dir = Path.GetDirectoryName(filePath);
                    Directory.CreateDirectory(dir);
                    var fs = new FileStream(filePath, FileMode.CreateNew);
                    stream.CopyTo(fs);
                    fs.Close();
                    stream.Position = 0;
                }
            }
            byte[] buf = new byte[stream.Length];
            stream.Read(buf, 0, (int)stream.Length);
            res = Encoding.UTF8.GetString(buf);

            return res;
        }
    }
}
