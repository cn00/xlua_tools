using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Timers;
using System.Web;
using System.Windows.Forms.VisualStyles;
using Mono.Web;
using UnityEngine;
using Random = System.Random;

namespace Baidu
{
    class Fanyi
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="src">原文</param>
        /// <param name="from">源语言</param>
        /// <param name="to">目标语言</param>
        /// <param name="appId"></param>
        /// <param name="secretKey"></param>
        /// <returns></returns>
        public static string Do(string src, string from, string to, string appId, string secretKey)
        {
            Random rd = new Random();
            string salt = rd.Next(100000).ToString();
            string sign = EncryptString(appId + src + salt + secretKey);
            string url = "http://api.fanyi.baidu.com/api/trans/vip/translate?";
            url += "q=" + HttpUtility.UrlEncode(src);
            url += "&from=" + from;
            url += "&to=" + to;
            url += "&appid=" + appId;
            url += "&salt=" + salt;
            url += "&sign=" + sign;
            HttpWebRequest request = (HttpWebRequest) WebRequest.Create(url);
            // request.Method = "GET";
            // request.ContentType = "text/html;charset=UTF-8";
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.UserAgent = null;
            request.Timeout = 6000;
            HttpWebResponse response = (HttpWebResponse) request.GetResponse();
            Stream myResponseStream = response.GetResponseStream();
            StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.ASCII);
            string retString = myStreamReader.ReadToEnd();
            myStreamReader.Close();
            myResponseStream.Close();

            // Console.WriteLine(retString);
            return UnicodeToString2(retString);
        }

        public static string Do(string src)
        {
            // 源语言
            string from = "jp";
            // 目标语言
            string to = "zh";
            // 改成您的APP ID
            string appId = "20200828000553756"; //
            Random rd = new Random();
            string salt = rd.Next(100000).ToString();
            // 改成您的密钥
            string secretKey = "f1933JDjZTZX87ECMoUO"; //
            string retString = Do(src, from, to, appId, secretKey);
            return retString;
        }

        public class MatchComparer : IEqualityComparer<Match>
        {
            public bool Equals(Match x, Match y)
            {
                if (x == null)
                    return y == null;
                return x.Value == y.Value;
            }


            public int GetHashCode(Match obj)
            {
                if (obj == null)
                    return 0;
                return obj.Value.GetHashCode();
            }
        }
        public static string UnicodeToString(string src)
        {
            var s = src;
            s = Regex.Replace(s, @"\\u([0-9A-F]{4})"
                , x =>Convert.ToChar(Convert.ToUInt16(x.Result("$1"), 16)).ToString()
                , RegexOptions.IgnoreCase | RegexOptions.Compiled);
            
            return s;
        }
        public static string UnicodeToString2(string src)
        {
            var cp = src;
            var matchs = (IList<Match>) Regex.Matches(cp, @"\\u([0-9A-F]{4})", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            var sb = new StringBuilder(1024);
            int indexa = 0;
            matchs.ToList().ForEach(i =>
            {
                if(i.Index > indexa) {
                    var pre = src.Substring(indexa, i.Index - indexa);
                    sb.Append(pre);
                    // Console.Write(pre);
                }

                var c = Convert.ToChar(Convert.ToUInt16(i.Result("$1"), 16));
                // Console.Write(i.Value + c);
                sb.Append(c);
                indexa = i.Index + i.Length;
            });
            sb.Append(src.Substring(indexa));
            // var unique = matchs.GroupBy(i => i.Value).Select(g => g.First()); //.Select(i=>new{i.Value, S = Convert.ToChar(Convert.ToUInt16(i.Result("$1"), 16)).ToString()});//.OrderBy(i => i.Length).ThenBy(i => i.Value);
            // unique.ToList().ForEach(i => cp = cp.Replace(i.Value, Convert.ToChar(Convert.ToUInt16(i.Result("$1"), 16)).ToString()));
            // Console.WriteLine("UnicodeToString2=" + sb.ToString());
            return sb.ToString();
        }

        // 计算MD5值
        public static string EncryptString(string str)
        {
            MD5 md5 = MD5.Create();
            // 将字符串转换成字节数组
            byte[] byteOld = Encoding.UTF8.GetBytes(str);
            // 调用加密方法
            byte[] byteNew = md5.ComputeHash(byteOld);
            // 将加密结果转换为字符串
            StringBuilder sb = new StringBuilder();
            foreach (byte b in byteNew)
            {
                // 将字节转换成16进制表示的字符串，
                sb.Append(b.ToString("x2"));
            }

            // 返回加密的字符串
            return sb.ToString();
        }
    }
}