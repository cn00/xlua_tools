using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Net;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Timers;
using Mono.Web;
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

            // Debug.WriteLine(retString);
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

        public static string DoImg(string fpath)
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
            string retString = DoImg(fpath, from, to, appId, secretKey);
            return retString;
        }

        static String a(byte[] bArr)
        {
            char[] a = {'0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'c', 'd', 'e', 'f'};
            char[] cArr = new char[(bArr.Length << 1)];
            int length = bArr.Length;
            int i = 0;
            int i2 = 0;
            while (i < length)
            {
                byte b = bArr[i];
                int i3 = i2 + 1;
                cArr[i2] = a[(b >> 4) & 15];
                cArr[i3] = a[b & 15];
                i++;
                i2 = i3 + 1;
            }

            return new String(cArr);
        }

        public static string DoImg(string fpath, string from, string to, string appId, string secretKey)
        {
            Random rd = new Random();
            string salt = rd.Next(100000).ToString();
            var fmd5 = xlua.Util.Md5(fpath).ToLower();
            string sign = EncryptString(appId + fmd5 + salt + "APICUIDmac" + secretKey);
            string url = "https://fanyi-api.baidu.com/api/trans/sdk/picture?";
            // url += "q=" + HttpUtility.UrlEncode(fpath);
            url += "from=" + from;
            url += "&to=" + to;
            url += "&appid=" + appId;
            url += "&salt=" + salt;
            url += "&sign=" + sign;
            url += "&cuid=APICUID&mac=mac";
            url += $"&fmd5={fmd5}";
            Console.WriteLine($"url:{url}");

            // var retString = HttpPostData(url, 6000, "image.png", fpath, new NameValueCollection());
            var retString = Upload(url, fpath);
            
            // HttpWebRequest request = (HttpWebRequest) WebRequest.Create(url);

            // // request.Method = "GET";
            // // request.ContentType = "text/html;charset=UTF-8";
            // request.Method = "POST";
            // request.KeepAlive = true;
            // // request.Connection = "Keep-Alive";
            // request.ContentType = "multipart/form-data; boundary=WFxd9eMHN7IjLC2KgBBYWXZ0hXvG9J4w";
            // request.UserAgent = null;
            // request.Timeout = 6000;
            // var rs = request.GetRequestStream();
            // var fbytes = File.ReadAllBytes(fpath);
            // rs.Write(fbytes, 0, fbytes.Length);
            // rs.Close();
            // HttpWebResponse response = (HttpWebResponse) request.GetResponse();
            //
            // Stream myResponseStream = response.GetResponseStream();
            // StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.ASCII);
            // string retString = myStreamReader.ReadToEnd();
            // myStreamReader.Close();
            // myResponseStream.Close();

            // Debug.WriteLine(retString);
            return UnicodeToString2(retString);
        }

        private static string HttpPostData(string url, int timeOut, string fileKeyName,
            string filePath, NameValueCollection stringDict)
        {

            const string filePartHeader =
                "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\n" +
                "Content-Type: application/octet-stream\r\n\r\n";
            var header = string.Format(filePartHeader, fileKeyName, filePath);
            var headerbytes = Encoding.UTF8.GetBytes(header);

            var boundary = "---------------" + DateTime.Now.Ticks.ToString("x");
            var beginBoundary = Encoding.ASCII.GetBytes("--" + boundary + "\r\n");
            
            var memStream = new MemoryStream();
            memStream.Write(beginBoundary, 0, beginBoundary.Length);
            memStream.Write(headerbytes, 0, headerbytes.Length);
            Console.WriteLine($"beginBoundary:{boundary}");
            Console.WriteLine($"header:{header}");
            
            var buffer = new byte[1024];
            int bytesRead; // =0
            var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
            {
                memStream.Write(buffer, 0, bytesRead);
            }

            // 写入字符串的Key
            var stringKeyHeader = "\r\n--" + boundary +
                                  "\r\nContent-Disposition: form-data; name=\"{0}\"" +
                                  "\r\n\r\n{1}\r\n";

            foreach (byte[] formitembytes in from string key in stringDict.Keys
                select string.Format(stringKeyHeader, key, stringDict[key])
                into formitem
                select Encoding.UTF8.GetBytes(formitem))
            {
                memStream.Write(formitembytes, 0, formitembytes.Length);
            }

            // 写入最后的结束边界符
            var endBoundary = Encoding.ASCII.GetBytes("--" + boundary + "--\r\n");
            memStream.Write(endBoundary, 0, endBoundary.Length);
            Console.WriteLine($"endBoundary:{boundary}");

            var webRequest = (HttpWebRequest) WebRequest.Create(url);
            webRequest.Method = "POST";
            webRequest.Timeout = timeOut;
            webRequest.ContentType = "multipart/form-data; boundary=" + boundary;
            webRequest.ContentLength = memStream.Length;

            var requestStream = webRequest.GetRequestStream();
            
            Console.WriteLine($"webRequest.Headers: {string.Join("|", webRequest.Headers)} endHEADERs");

            memStream.Position = 0;
            var tempBuffer = new byte[memStream.Length];
            memStream.Read(tempBuffer, 0, tempBuffer.Length);
            memStream.Close();

            requestStream.Write(tempBuffer, 0, tempBuffer.Length);
            requestStream.Close();

            var httpWebResponse = (HttpWebResponse) webRequest.GetResponse();

            string responseContent;
            using (var httpStreamReader = new StreamReader(httpWebResponse.GetResponseStream(), Encoding.GetEncoding("utf-8")))
            {
                responseContent = httpStreamReader.ReadToEnd();
            }

            fileStream.Close();
            httpWebResponse.Close();
            webRequest.Abort();

            return responseContent;
        }

        public static string Upload(string uri, string filePath)
        {   
            string formdataTemplate = "Content-Disposition: form-data; filename=\"{0}\";\r\nContent-Type: image/jpeg\r\n\r\n";
            string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
            byte[] boundarybytes = Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.ServicePoint.Expect100Continue = false;
            request.Method = "POST";
            request.ContentType = "image/png; multipart/form-data; boundary=" + boundary;

            using(FileStream fileStream    = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (Stream requestStream = request.GetRequestStream())
                {
                    requestStream.Write(boundarybytes, 0, boundarybytes.Length);
                    string formitem = string.Format(formdataTemplate, Path.GetFileName(filePath));   
                    byte[] formbytes = Encoding.UTF8.GetBytes(formitem);
                    requestStream.Write(formbytes, 0, formbytes.Length);
                    byte[] buffer = new byte[1024 * 4];
                    int bytesLeft = 0;

                    while ((bytesLeft = fileStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        requestStream.Write(buffer, 0, bytesLeft);
                    }

                }
            }

            try
            {
                using (HttpWebResponse response = (HttpWebResponse) request.GetResponse())
                {
                    using (var httpStreamReader = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding("utf-8")))
                    {
                        return httpStreamReader.ReadToEnd();
                    }
                }

                Console.WriteLine ("Success");
            }
            catch (Exception ex)
            {
                throw ex;
            }
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
                , x => Convert.ToChar(Convert.ToUInt16(x.Result("$1"), 16)).ToString()
                , RegexOptions.IgnoreCase | RegexOptions.Compiled);

            return s;
        }

        public static string UnicodeToString2(string src)
        {
            var cp = src;
            var matchs = (IList<Match>) Regex.Matches(cp, @"\\u([0-9A-F]{4})",
                RegexOptions.IgnoreCase | RegexOptions.Compiled);
            var sb = new StringBuilder(1024);
            int indexa = 0;
            matchs.ToList().ForEach(i =>
            {
                if (i.Index > indexa)
                {
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
            // Debug.WriteLine("UnicodeToString2=" + sb.ToString());
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