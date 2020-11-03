using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;


public static class StringUtil
{
    public static byte[] Utf8Bytes(this string self)
    {
        return Encoding.UTF8.GetBytes(self);
    }
    public static byte[] DefaultBytes(this string self)
    {
        return Encoding.Default.GetBytes(self);
    }

    public static string RReplace(this string self, string pattern, string replacement)
    {
        return Regex.Replace(self, pattern, replacement);
    }

}