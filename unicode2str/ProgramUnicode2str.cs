using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace unicode2str
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("输入文件路径:");
            var fname = Console.ReadLine();
            if(string.IsNullOrWhiteSpace(fname))
                fname = "D:/a3/client/Unity/Assets/Application/Editor";

            var regular = "\\\\u.*";
            var lines = File.ReadAllLines(fname);
            for(var i = 0; i < lines.Length; ++i)
            {
                var line = lines[i];
                var matches = Regex.Matches(line, regular);
                foreach(var match in matches)
                {
                    var ustr = match.ToString().TrimEnd(new char[] { '"', '\''});
                    var str = "";
                    int idx = 0;
                    var Length = ustr.Length;
                    while(idx < Length)
                    {
                        if(ustr[idx] == '\\' && ustr[idx + 1] == 'u')
                        {
                            var uchar = ustr.Substring(idx + 2, 4);
                            var schar = (char)short.Parse(uchar, global::System.Globalization.NumberStyles.HexNumber);
                            //Console.WriteLine("{0}:{1}=>{2}", idx, uchar, schar);

                            str += schar;
                            idx += 6;
                        }
                        else
                        {
                            var b = (byte)ustr[idx];
                            var c = (char)b;
                            str += ustr[idx];
                            ++idx;
                        }
                    }

                    // str2unicode
                    byte[] uc = Encoding.Unicode.GetBytes(str);
                    string s2 = "";
                    foreach(byte b in uc)
                    {
                        s2 += string.Format("\\u{0:X2}", b);
                    }

                    Console.WriteLine("{0} ... => {1}", ustr.Substring(0, Math.Min(ustr.Length, 12)), str);
                    lines[i] = line.Replace(ustr, str);
                }
            }
            File.WriteAllLines(fname, lines);

            Console.WriteLine("press Enter to exit:");
            Console.ReadLine();
        }
    }
}
