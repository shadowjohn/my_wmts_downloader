using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Web;
using System.Data.SqlClient;
using System.Data;
using System.Net;
using System.Configuration;
using System.Runtime.Serialization;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Diagnostics;
using System.Collections.Concurrent;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO.Compression;
using System.Net.Http;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.Zip;
using System.Linq;
using System.Net.Cache;

namespace utility
{
    public class PointF
    {
        public double X { get; set; }
        public double Y { get; set; }
    }
    public class myinclude
    {
        private Random rnd = new Random(DateTime.Now.Millisecond);
        public string _userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/134.0.0.0 Safari/537.36";
        public string pwd()
        {
            return Directory.GetCurrentDirectory();
        }
        public bool is_dir(string path)
        {
            return Directory.Exists(path);
        }
        public bool is_file(string filepath)
        {
            return File.Exists(filepath);
        }
        public void unlink(string filepath)
        {
            if (is_file(filepath))
            {
                File.Delete(filepath);
            }
        }
        public int rand(int min, int max)
        {
            return rnd.Next(min, max);
        }
        public string b2s(byte[] input)
        {
            return System.Text.Encoding.UTF8.GetString(input);
        }
        public byte[] s2b(string input)
        {
            return System.Text.Encoding.UTF8.GetBytes(input);
        }
        public bool is_string_like(string data, string find_string)
        {
            return (data.IndexOf(find_string) == -1) ? false : true;
        }
        public bool is_istring_like(string data, string find_string)
        {
            return (data.ToUpper().IndexOf(find_string.ToUpper()) == -1) ? false : true;
        }

        public string getSystemKey(string keyindex)
        {
            return ConfigurationManager.AppSettings[keyindex];
        }

        //大小寫
        public string strtoupper(string input)
        {
            return input.ToUpper();
        }
        public string strtolower(string input)
        {
            return input.ToLower();
        }
        public ConcurrentDictionary<string, object> curl_getPost_INIT(string URL, string postData, ConcurrentDictionary<string, object> options = null)
        {
            //From : https://stackoverflow.com/questions/2972643/how-to-use-cookies-with-httpwebrequest
            ConcurrentDictionary<string, object> output = new ConcurrentDictionary<string, object>();
            output["cookies"] = new CookieContainer();
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(URL);
            request.CookieContainer = (CookieContainer)output["cookies"];
            request.UserAgent = _userAgent;
            try
            {


                if (options != null)
                {
                    if (options.ContainsKey("login_id") && options.ContainsKey("login_pd"))
                    {
                        CredentialCache mycache = new CredentialCache();
                        Uri uri = new Uri(URL);
                        mycache.Add(uri, "Basic", new NetworkCredential(options["login_id"].ToString(), options["login_pd"].ToString()));
                        request.Credentials = mycache;
                    }
                    if (options.ContainsKey("timeout"))
                    {
                        request.Timeout = Convert.ToInt32(options["timeout"]);
                    }
                    if (options.ContainsKey("cookie"))
                    {
                        //request.Headers.Add("Cookie", options["cookie"].ToString());
                        //request.CookieContainer.Add( = options["cookie"].ToString();      
                        request.CookieContainer = new CookieContainer();
                        Uri uri = new Uri(URL);
                        request.CookieContainer.SetCookies(uri, options["cookie"].ToString());
                    }
                    if (options.ContainsKey("user_agent"))
                    {
                        request.UserAgent = options["user_agent"].ToString();
                    }
                }
                request.Proxy = null;


                HttpWebResponse response = (HttpWebResponse)request.GetResponse();


                if (postData == "")
                {
                    //GET
                    Stream stream = response.GetResponseStream();
                    output["data"] = ReadStream(stream, 32765);
                    stream.Close();
                }
                else
                {
                    //Post
                    byte[] data = Encoding.UTF8.GetBytes(postData);
                    request.Method = "POST";
                    request.ContentType = "application/x-www-form-urlencoded";
                    request.ContentLength = data.Length;

                    using (Stream stream = request.GetRequestStream())
                    {
                        stream.Write(data, 0, data.Length);
                        stream.Close();
                    }
                }
                output["realCookie"] = response.Headers[HttpResponseHeader.SetCookie];
                response.Close();
                output["reason"] = "";
                return output;
            }
            catch (Exception ex)
            {
                output["data"] = new byte[0];
                output["reason"] = ex.Message + "\n\r" + ex.StackTrace;
                return output;
            }
        }
        public ConcurrentDictionary<string, object> curl_getPost_continue(ConcurrentDictionary<string, object> C, string URL, string postData, ConcurrentDictionary<string, object> options = null)
        {
            ConcurrentDictionary<string, object> output = new ConcurrentDictionary<string, object>();
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(URL);
            request.Proxy = null;
            request.UserAgent = _userAgent;
            request.CookieContainer = (CookieContainer)C["cookies"];
            C["cookies"] = request.CookieContainer;
            try
            {
                if (options != null)
                {
                    if (options.ContainsKey("login_id") && options.ContainsKey("login_pd"))
                    {
                        CredentialCache mycache = new CredentialCache();
                        Uri uri = new Uri(URL);
                        mycache.Add(uri, "Basic", new NetworkCredential(options["login_id"].ToString(), options["login_pd"].ToString()));
                        request.Credentials = mycache;
                    }
                    if (options.ContainsKey("timeout"))
                    {
                        request.Timeout = Convert.ToInt32(options["timeout"]);
                    }
                    if (options.ContainsKey("user_agent"))
                    {
                        request.UserAgent = options["user_agent"].ToString();
                    }

                    //file_put_contents(pwd() + "\\log\\cookie.txt", json_encode(C["cookies"]));
                    if (options.ContainsKey("cookie"))
                    {
                        //request.Headers.Add("Cookie", options["cookie"].ToString());
                        //request.CookieContainer.Add( = options["cookie"].ToString();      
                        //request.CookieContainer = new CookieContainer();
                        /*
                        List<Cookie> LC = cookieStrToCookie(options["cookie"].ToString());
                        for (int i = 0, max_i = LC.Count; i < max_i; i++)
                        {
                            request.CookieContainer.Add(LC[i]);
                        }
                        */
                        Uri uri = new Uri(URL);
                        request.CookieContainer.SetCookies(uri, options["cookie"].ToString());
                    }
                }
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                if (postData == "")
                {
                    //GET
                    Stream stream = response.GetResponseStream();
                    output["data"] = ReadStream(stream, 32765);
                    stream.Close();
                }
                else
                {
                    //Post
                    byte[] data = Encoding.UTF8.GetBytes(postData);
                    request.Method = "POST";
                    request.ContentType = "application/x-www-form-urlencoded";
                    request.ContentLength = data.Length;

                    using (Stream stream = request.GetRequestStream())
                    {
                        stream.Write(data, 0, data.Length);
                        stream.Close();
                    }
                }
                output["realCookie"] = response.Headers[HttpResponseHeader.SetCookie];
                response.Close();
                output["reason"] = "";
                return output;
            }
            catch (Exception ex)
            {
                output["data"] = new byte[0];
                output["reason"] = ex.Message + "\n\r" + ex.StackTrace;
                return output;
            }
        }
        public byte[] ReadStream(Stream stream, int initialLength)
        {
            if (initialLength < 1)
            {
                initialLength = 32768;
            }
            byte[] buffer = new byte[initialLength];
            int read = 0;
            int chunk;
            while ((chunk = stream.Read(buffer, read, buffer.Length - read)) > 0)
            {
                read += chunk;
                if (read == buffer.Length)
                {
                    int nextByte = stream.ReadByte();
                    if (nextByte == -1)
                    {
                        return buffer;
                    }
                    byte[] newBuffer = new byte[buffer.Length * 2];
                    Array.Copy(buffer, newBuffer, buffer.Length);
                    newBuffer[read] = (byte)nextByte;
                    buffer = newBuffer;
                    read++;
                }
            }
            byte[] bytes = new byte[read];
            Array.Copy(buffer, bytes, read);
            return bytes;
        }

        public DateTime UnixTimeToDateTime(string text)
        {
            System.DateTime dateTime = new System.DateTime(1970, 1, 1, 0, 0, 0, 0);
            // Add the number of seconds in UNIX timestamp to be converted.            
            dateTime = dateTime.AddSeconds(Convert.ToDouble(text));
            return dateTime;
        }
        //仿php的date
        public string time()
        {
            return strtotime(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        }
        public string date()
        {
            return date("Y-m-d H:i:s", strtotime(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffffff")));
        }
        public string date(string format)
        {
            return date(format, strtotime(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffffff")));
        }
        public string date(string format, string unixtimestamp)
        {
            DateTime tmp = UnixTimeToDateTime(unixtimestamp);
            tmp = tmp.AddHours(+8);
            switch (format)
            {
                case "Y-m-d H:i:s":
                    return tmp.ToString("yyyy-MM-dd HH:mm:ss");
                case "Y/m/d":
                    return tmp.ToString("yyyy/MM/dd");
                case "Y/m/d H:i:s":
                    return tmp.ToString("yyyy/MM/dd HH:mm:ss");
                case "Y/m/d H:i:s.fff":
                    return tmp.ToString("yyyy/MM/dd HH:mm:ss.fff");
                case "Y-m-d_H_i_s":
                    return tmp.ToString("yyyy-MM-dd_HH_mm_ss");
                case "Y-m-d":
                    return tmp.ToString("yyyy-MM-dd");
                case "H:i:s":
                    return tmp.ToString("HH:mm:ss");
                case "Y-m-d H:i":
                    return tmp.ToString("yyyy-MM-dd HH:mm");
                case "Y_m_d_H_i_s":
                    return tmp.ToString("yyyy_MM_dd_HH_mm_ss");
                case "Y_m_d_H_i_s_fff":
                    return tmp.ToString("yyyy_MM_dd_HH_mm_ss_fff");
                case "w":
                    //回傳week, sun =0 , sat = 6, mon=1.....
                    return Convert.ToInt16(tmp.DayOfWeek).ToString();
                case "Y":
                    return tmp.ToString("yyyy");
                case "m":
                    return tmp.ToString("MM");
                case "d":
                    return tmp.ToString("dd");
                case "H":
                    return tmp.ToString("HH");
                case "i":
                    return tmp.ToString("mm");
                case "s":
                    return tmp.ToString("ss");
                case "Y-m-d H:i:s.fff":
                    return tmp.ToString("yyyy-MM-dd HH:mm:ss.fff");
                case "Y-m-d H:i:s.ffffff":
                    return tmp.ToString("yyyy-MM-dd HH:mm:ss.ffffff");
                case "H:i:s.fff":
                    return tmp.ToString("HH:mm:ss.fff");
                case "H:i:s.ffffff":
                    return tmp.ToString("HH:mm:ss.ffffff");
            }
            return "";
        }
        //strtotime 轉換成 Unix time
        public string strtotime(string value)
        {
            //create Timespan by subtracting the value provided from
            //the Unix Epoch
            TimeSpan span = (Convert.ToDateTime(value) - new DateTime(1970, 1, 1, 0, 0, 0, 0).ToLocalTime());

            //return the total seconds (which is a UNIX timestamp)
            if (is_string_like(value, "."))
            {
                //有小數點               
                double sec = span.Ticks / (TimeSpan.TicksPerMillisecond / 1000.0) / 1000000.0;
                return sec.ToString();
            }
            else
            {
                return span.TotalSeconds.ToString();
            }
        }
        public string strtotime(DateTime value)
        {
            //create Timespan by subtracting the value provided from
            //the Unix Epoch
            TimeSpan span = (value - new DateTime(1970, 1, 1, 0, 0, 0, 0).ToLocalTime());

            //return the total seconds (which is a UNIX timestamp)
            return span.TotalSeconds.ToString();
        }
        //javascript用的吐js資料
        public string jsAddSlashes(string value)
        {
            value = value.Replace("\\", "\\\\");
            value = value.Replace("\n", "\\n");
            value = value.Replace("\r", "\\r");
            value = value.Replace("\"", "\\\"");
            value = value.Replace("&", "\\x26");
            value = value.Replace("<", "\\x3C");
            value = value.Replace(">", "\\x3E");
            return value;
        }

        public string basename(string path)
        {
            return Path.GetFileName(path);
        }
        public string mainname(string path)
        {
            return Path.GetFileNameWithoutExtension(path);
        }
        public string subname(string path)
        {
            return Path.GetExtension(path).TrimStart('.');
        }
        public long getfilesize(string path)
        {
            FileInfo f = new FileInfo(path);
            return f.Length;
        }
        public string size_hum_read(long bytes)
        {
            string[] Suffix = { "B", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };
            int i = 0;
            double dblSByte = Convert.ToDouble(bytes);
            if (bytes > 1024)
                for (i = 0; (bytes / 1024) > 0; i++, bytes /= 1024)
                    dblSByte = bytes / 1024.0;
            return String.Format("{0:0.##} {1}", dblSByte, Suffix[i]);
        }

        public void mkdir(string path)
        {
            Directory.CreateDirectory(path);
        }
        public void copy(string sourceFile, string destFile)
        {
            System.IO.File.Copy(sourceFile, destFile, true);
        }
        public string dirname(string path)
        {
            return Directory.GetParent(path).FullName;
        }
        public string basedir()
        {
            //取得專案的起始位置
            return pwd();
        }

        public string system(string command)
        {
            StringBuilder sb = new StringBuilder();
            string version = System.Environment.OSVersion.VersionString;//读取操作系统版本  
            if (version.Contains("Windows"))
            {
                using (Process p = new Process())
                {
                    p.StartInfo.FileName = "cmd.exe";

                    p.StartInfo.UseShellExecute = false;//是否指定操作系统外壳进程启动程序  
                    p.StartInfo.RedirectStandardInput = true;
                    p.StartInfo.RedirectStandardOutput = true;
                    p.StartInfo.CreateNoWindow = true;//不显示dos命令行窗口  

                    p.Start();//启动cmd.exe  
                    p.StandardInput.WriteLine(command);//输入命令  
                    p.StandardInput.WriteLine("exit");//退出cmd.exe  
                    p.WaitForExit();//等待执行完了，退出cmd.exe  

                    using (StreamReader reader = p.StandardOutput)//截取输出流  
                    {
                        string line = reader.ReadLine();//每次读取一行  
                        while (!reader.EndOfStream)
                        {
                            sb.Append(line).Append("<br />");//在Web中使用<br />换行  
                            line = reader.ReadLine();
                        }
                        p.WaitForExit();//等待程序执行完退出进程  
                        p.Close();//关闭进程  
                        reader.Close();//关闭流  
                    }
                }
            }
            return sb.ToString();
        }
        public string EscapeUnicode(string input)
        {
            StringBuilder sb = new StringBuilder(input.Length);
            foreach (char ch in input)
            {
                if (ch <= 0x7f)
                    sb.Append(ch);
                else
                    sb.AppendFormat(CultureInfo.InvariantCulture, "\\u{0:x4}", (int)ch);
            }
            return sb.ToString();
        }
        public string unEscapeUnicode(string input)
        {
            return Regex.Unescape(input);
        }
        public string json_encode(object input)
        {
            return EscapeUnicode(JsonConvert.SerializeObject(input, Formatting.None));
        }
        public string trim(string input)
        {
            return input.Trim();
        }
        public string md5(string str)
        {
            using (var cryptoMD5 = System.Security.Cryptography.MD5.Create())
            {
                //將字串編碼成 UTF8 位元組陣列
                var bytes = Encoding.UTF8.GetBytes(str);
                //取得雜湊值位元組陣列
                var hash = cryptoMD5.ComputeHash(bytes);
                //取得 MD5
                var md5 = BitConverter.ToString(hash)
                  .Replace("-", String.Empty)
                  .ToUpper();
                return md5;
            }
        }
        public JArray json_decode(string input)
        {
            input = trim(input);
            if (input.Length != 0)
            {
                if (input.Substring(1, 1) != "[")
                {
                    input = "[" + input + "]";
                    return (JArray)JsonConvert.DeserializeObject<JArray>(input);
                }
                else
                {
                    return (JArray)JsonConvert.DeserializeObject<JArray>(input);
                }
            }
            else
            {
                return null;
            }
        }
        public string microtime()
        {
            System.DateTime dt = DateTime.Now;
            System.DateTime UnixEpoch = new System.DateTime(1970, 1, 1, 0, 0, 0, 0);
            TimeSpan span = dt - UnixEpoch;
            long microseconds = span.Ticks / (TimeSpan.TicksPerMillisecond / 1000);
            return microseconds.ToString();
        }
        public void file_put_contents(string filepath, string input)
        {
            file_put_contents(filepath, s2b(input), false);
        }
        public void file_put_contents(string filepath, byte[] input)
        {
            file_put_contents(filepath, input, false);
        }
        public void file_put_contents(string filepath, string input, bool isFileAppend)
        {
            file_put_contents(filepath, s2b(input), isFileAppend);
        }
        public void file_put_contents(string filepath, byte[] input, bool isFileAppend)
        {

            switch (isFileAppend)
            {
                case true:
                    {
                        FileStream myFile = null;
                        if (!is_file(filepath))
                        {
                            myFile = File.Open(@filepath, FileMode.Create);
                        }
                        else
                        {
                            myFile = File.Open(@filepath, FileMode.Append);
                        }
                        myFile.Seek(myFile.Length, SeekOrigin.Begin);
                        myFile.Write(input, 0, input.Length);
                        myFile.Dispose();
                    }
                    break;
                case false:
                    {
                        FileStream myFile = File.Open(@filepath, FileMode.Create);
                        myFile.Write(input, 0, input.Length);
                        myFile.Dispose();
                    }
                    break;
            }
        }
        /// Will return the string contents of a
        /// regular file or the contents of a
        /// response from a URL
        /// </summary>
        /// <param name="fileName">The filename or URL</param>
        /// <returns></returns>
        public byte[] file_get_contents_retry(string url, int maxRetries = 3)
        {
            if (url.ToLower().IndexOf("http:") == 0 || url.ToLower().IndexOf("https:") == 0)
            {
                // URL                 
                //http://social.msdn.microsoft.com/Forums/en-US/8050d80a-ca45-4b0c-82dc-81dd1eac496f/retry-catch
                bool redo = false;

                int retries = 0;
                HttpWebRequest request = null;
                HttpWebResponse response = null;
                byte[] byteData = null;
                do
                {
                    try
                    {
                        request = (HttpWebRequest)WebRequest.Create(url);
                        request.Timeout = 30000;
                        request.Proxy = null;
                        request.UserAgent = _userAgent;
                        //request.Referer = getSystemKey("HTTP_REFERER");
                        response = (HttpWebResponse)request.GetResponse();
                        Stream stream = response.GetResponseStream();
                        byteData = ReadStream(stream, 5000);
                        stream.Close();
                    }
                    catch (Exception e)
                    {
                        Console.Write(e.Message);
                        //Console.WriteLine(e.Message);
                        redo = true;
                        Thread.Sleep(5);
                        ++retries;
                        //myLog("retry..." + retries);
                    }

                } while (redo && retries < maxRetries);
                response.Close();
                return byteData;
            }
            else
            {
                System.IO.StreamReader sr = new System.IO.StreamReader(url);
                string sContents = sr.ReadToEnd();
                sr.Close();
                return s2b(sContents);
            }
        }
        public byte[] file_get_contents(string url)
        {
            if (url.ToLower().IndexOf("http:") > -1 || url.ToLower().IndexOf("https:") > -1)
            {
                // URL                 
                ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
                ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;
                HttpWebRequest request = null;
                HttpWebResponse response = null;
                byte[] byteData = null;

                request = (HttpWebRequest)WebRequest.Create(url);
                request.Timeout = 60000;
                request.Proxy = null;
                request.UserAgent = _userAgent;
                request.Referer = "http://localhost/gglonglongder";
                response = (HttpWebResponse)request.GetResponse();
                Stream stream = response.GetResponseStream();
                byteData = ReadStream(stream, 32765);
                response.Close();
                stream.Close();
                return byteData;
            }
            else
            {
                /*System.IO.StreamReader sr = new System.IO.StreamReader(url);
                
                string sContents = sr.ReadToEnd();
                sr.Close();
                return s2b(sContents);
                */
                /*FileStream fs = new FileStream(url, FileMode.Open);
                byte[] buffer = new byte[fs.Length];
                fs.Read(buffer, 0, buffer.Length);
                fs.Close();
                return buffer;
                */
                byte[] data;
                using (StreamReader sr = new StreamReader(url))
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        sr.BaseStream.CopyTo(ms);
                        data = ms.ToArray();
                        ms.Close();
                        ms.Dispose();
                    }
                    sr.Close();
                    sr.Dispose();
                };
                return data;
            }
        }
        public string get_between(string data, string s_begin, string s_end)
        {
            //http://stackoverflow.com/questions/378415/how-do-i-extract-a-string-of-text-that-lies-between-two-parenthesis-using-net
            //string a = "abcdefg";
            //MessageBox.Show(my.get_between(a, "cde", "g"));
            //return f;
            string s = data;
            int start = s.IndexOf(s_begin);
            if (start < 0)
            {
                return "";
            }
            string new_s = data.Substring(start + s_begin.Length);
            int end = new_s.IndexOf(s_end);
            if (end < 0)
            {
                return "";
            }
            return s.Substring(start + s_begin.Length, end);
        }
        public string implode(string keyword, string[] arrays)
        {
            return string.Join(keyword, arrays);
        }

        public string implode(string keyword, Dictionary<int, string> arrays)
        {
            string[] tmp = new String[arrays.Keys.Count];
            int i = 0;
            foreach (int k in arrays.Keys)
            {
                tmp[i++] = arrays[k];
            }
            return string.Join(keyword, tmp);
        }
        public string implode(string keyword, Dictionary<string, string> arrays)
        {
            string[] tmp = new String[arrays.Keys.Count];
            int i = 0;
            foreach (string k in arrays.Keys)
            {
                tmp[i++] = arrays[k];
            }
            return string.Join(keyword, tmp);
        }
        public string implode(string keyword, ArrayList arrays)
        {
            string[] tmp = new String[arrays.Count];
            for (int i = 0; i < arrays.Count; i++)
            {
                tmp[i] = arrays[i].ToString();
            }
            return string.Join(keyword, tmp);
        }
        public string[] explode(string keyword, string data)
        {
            return data.Split(new string[] { keyword }, StringSplitOptions.None);
        }
        public string[] explode(string keyword, object data)
        {
            return data.ToString().Split(new string[] { keyword }, StringSplitOptions.None);
        }
        public string[] explode(string[] keyword, string data)
        {
            return data.Split(keyword, StringSplitOptions.None);
        }
        static public double min(double a, double b)
        {
            return (a < b) ? a : b;
        }
        public double max(double a, double b)
        {
            return (a > b) ? a : b;
        }
        public void echo(string data)
        {
            Console.WriteLine(data);
        }
        public void echo(double data)
        {
            Console.WriteLine(data);
        }
        public Dictionary<string, double> fix_LT_RB(Dictionary<string, double> data)
        {
            Dictionary<string, double> output = new Dictionary<string, double>();
            output["LT_X"] = min(data["LT_X"], data["RB_X"]);
            output["RB_X"] = max(data["LT_X"], data["RB_X"]);
            output["LT_Y"] = max(data["LT_Y"], data["RB_Y"]);
            output["RB_Y"] = min(data["LT_Y"], data["RB_Y"]);
            return output;
        }
        public Dictionary<string, double> p3826_to_p4326(Dictionary<string, double> data)
        {
            Dictionary<string, double> output = new Dictionary<string, double>();
            ProjNet.CoordinateSystems.Transformations.ICoordinateTransformation pct = CoordinateTransformation.CoordinateTransformation.TransformCoordinate("3826", "4326");
            double[] maxResult = pct.MathTransform.Transform(new double[] { data["LT_X"], data["LT_Y"] });
            output["LT_X"] = maxResult[0];
            output["LT_Y"] = maxResult[1];
            double[] minResult = pct.MathTransform.Transform(new double[] { data["RB_X"], data["RB_Y"] });
            output["RB_X"] = minResult[0];
            output["RB_Y"] = minResult[1];
            return output;
        }
        public Dictionary<string, double> p4326_to_p3826(Dictionary<string, double> data)
        {
            Dictionary<string, double> output = new Dictionary<string, double>();
            ProjNet.CoordinateSystems.Transformations.ICoordinateTransformation pct = CoordinateTransformation.CoordinateTransformation.TransformCoordinate("4326", "3826");
            double[] maxResult = pct.MathTransform.Transform(new double[] { data["LT_X"], data["LT_Y"] });
            output["LT_X"] = maxResult[0];
            output["LT_Y"] = maxResult[1];
            double[] minResult = pct.MathTransform.Transform(new double[] { data["RB_X"], data["RB_Y"] });
            output["RB_X"] = minResult[0];
            output["RB_Y"] = minResult[1];
            return output;
        }
        public PointF[] p3826_to_pointf(Dictionary<string, double> data)
        {
            PointF[] p = new PointF[4];
            p[0] = new PointF();
            p[0].X = data["LT_X"];
            p[0].Y = data["LT_Y"];

            p[1] = new PointF();
            p[1].X = data["RB_X"];
            p[1].Y = data["LT_Y"];

            p[2] = new PointF();
            p[2].X = data["RB_X"];
            p[2].Y = data["RB_Y"];

            p[3] = new PointF();
            p[3].X = data["LT_X"];
            p[3].Y = data["RB_Y"];
            return p;
        }
        public double area_of_polygon(PointF[] point)
        {
            //平面計算面積
            int i;
            double s;

            int vcount = point.Length;

            if (vcount < 3) return 0;
            s = point[0].Y * (point[vcount - 1].X - point[1].X);
            for (i = 1; i < vcount; i++)
                s += point[i].Y * (point[(i - 1)].X - point[(i + 1) % vcount].X);
            if (s < 0)
            {
                return Math.Abs(s / 2);
            }
            else
            {
                return s / 2;
            }
        }
        public void exit()
        {
            Environment.Exit(1);
        }
        public Dictionary<string, int> getTileFromLongLat(int Zoom, double lon, double lat)
        {
            //某個經緯度在哪個圖磚            
            double pi = Math.PI;
            Dictionary<string, int> Tile = new Dictionary<string, int>();
            Tile["Zoom"] = Zoom;
            double ZoomLevelTiles = 1 << Zoom;
            Tile["X"] = Convert.ToInt32((Math.Floor((lon + 180.0) / 360.0 * ZoomLevelTiles)));
            Tile["Y"] = Convert.ToInt32((Math.Floor((1.0 - Math.Log(Math.Tan(lat * pi / 180.0) + 1.0 / Math.Cos(lat * pi / 180.0)) / pi) / 2.0 * ZoomLevelTiles)));
            return Tile;
        }
        public Dictionary<string, int> p4326_to_ptile(int Zoom, Dictionary<string, double> p4326)
        {
            Dictionary<string, int> output = new Dictionary<string, int>();
            output["ZOOM"] = Zoom;
            var LT = getTileFromLongLat(Zoom, p4326["LT_X"], p4326["LT_Y"]);
            output["LT_X"] = LT["X"];
            output["LT_Y"] = LT["Y"];
            var RB = getTileFromLongLat(Zoom, p4326["RB_X"], p4326["RB_Y"]);
            output["RB_X"] = RB["X"];
            output["RB_Y"] = RB["Y"];
            return output;
        }
        public Dictionary<string, string> QueryParse(string url)
        {
            Dictionary<string, string> qDict = new Dictionary<string, string>();
            foreach (string qPair in url.Substring(url.IndexOf('?') + 1).Split('&'))
            {
                string[] qVal = qPair.Split('=');
                qDict.Add(qVal[0], Uri.UnescapeDataString(qVal[1]));
            }
            return qDict;
        }
        public System.Drawing.Image resizeImage(System.Drawing.Image imgToResize, Size size)
        {
            //Get the image current width  
            int sourceWidth = imgToResize.Width;
            //Get the image current height  
            int sourceHeight = imgToResize.Height;
            float nPercent = 0;
            float nPercentW = 0;
            float nPercentH = 0;
            //Calulate  width with new desired size  
            nPercentW = ((float)size.Width / (float)sourceWidth);
            //Calculate height with new desired size  
            nPercentH = ((float)size.Height / (float)sourceHeight);
            if (nPercentH < nPercentW)
                nPercent = nPercentH;
            else
                nPercent = nPercentW;
            //New Width  
            int destWidth = (int)(sourceWidth * nPercent);
            //New Height  
            int destHeight = (int)(sourceHeight * nPercent);
            Bitmap b = new Bitmap(destWidth, destHeight);
            Graphics g = Graphics.FromImage((System.Drawing.Image)b);
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            // Draw image with new width and height  
            g.DrawImage(imgToResize, 0, 0, destWidth, destHeight);
            g.Dispose();
            return (System.Drawing.Image)b;
        }
        public bool is_zip(string filepath)
        {
            // 確實用 zip 打開看看是不是
            // https://stackoverflow.com/questions/7355252/how-to-check-if-a-file-is-a-valid-zip-file
            try
            {
                using (var zip = System.IO.Compression.ZipFile.OpenRead(filepath))
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
        public bool is_sqlite(string filepath)
        {
            // 用 Microsoft.Data.Sqlite 去開啟看看是不是
            // https://stackoverflow.com/questions/394316/using-sqlite-in-c-sharp
            try
            {
                using (var connection = new Microsoft.Data.Sqlite.SqliteConnection("Data Source=" + filepath))
                {
                    connection.Open();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
        public DataTable sqliteFile_selectSQL_SAFE(string filepath, string SQL)
        {
            Dictionary<string, string> pa = new Dictionary<string, string>();
            return sqliteFile_selectSQL_SAFE(filepath, SQL, pa);
        }
        public DataTable sqliteFile_selectSQL_SAFE(string filepath, string SQL, Dictionary<string, string> pa)
        {
            // https://stackoverflow.com/questions/394316/using-sqlite-in-c-sharp
            DataTable dt = new DataTable();
            using (var connection = new Microsoft.Data.Sqlite.SqliteConnection("Data Source=" + filepath))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = SQL;
                    foreach (string key in pa.Keys)
                    {
                        command.Parameters.AddWithValue("@" + key, pa[key]);
                    }
                    using (var reader = command.ExecuteReader())
                    {
                        dt.Load(reader);
                    }
                }
            }
            return dt;
        }
        public bool sqliteFile_execSQL_SAFE(string filepath, string SQL)
        {
            Dictionary<string, string> pa = new Dictionary<string, string>();
            return sqliteFile_execSQL_SAFE(filepath, SQL, pa);
        }
        public bool sqliteFile_execSQL_SAFE(string filepath, string SQL, Dictionary<string, string> pa)
        {
            // https://stackoverflow.com/questions/394316/using-sqlite-in-c-sharp
            using (var connection = new Microsoft.Data.Sqlite.SqliteConnection("Data Source=" + filepath))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = SQL;
                    foreach (string key in pa.Keys)
                    {
                        command.Parameters.AddWithValue("@" + key, pa[key]);
                    }
                    command.ExecuteNonQuery();
                }
            }
            return true;
        }
        public DataTable sqlitePDO_selectSQL_SAFE(Microsoft.Data.Sqlite.SqliteConnection pdo, string SQL)
        {
            Dictionary<string, string> pa = new Dictionary<string, string>();
            return sqlitePDO_selectSQL_SAFE(pdo, SQL, pa);
        }
        public DataTable sqlitePDO_selectSQL_SAFE(Microsoft.Data.Sqlite.SqliteConnection pdo, string SQL, Dictionary<string, string> pa)
        {
            DataTable dt = new DataTable();
            using (var command = pdo.CreateCommand())
            {
                command.CommandText = SQL;
                foreach (string key in pa.Keys)
                {
                    command.Parameters.AddWithValue("@" + key, pa[key]);
                }
                using (var reader = command.ExecuteReader())
                {
                    dt.Load(reader);
                }
            }
            return dt;
        }
        public int sqlitePDO_insertSQL(Microsoft.Data.Sqlite.SqliteConnection pdo, string table, Dictionary<string, object> data)
        {
            string SQL = "INSERT INTO `" + table + "`(`";
            string SQL2 = " VALUES (";
            foreach (string key in data.Keys)
            {
                SQL += key + "`,`";
                SQL2 += "@" + key + ",";
            }
            SQL = SQL.Substring(0, SQL.Length - 2) + ")";
            SQL2 = SQL2.Substring(0, SQL2.Length - 1) + ")";
            SQL += SQL2;
            using (var command = pdo.CreateCommand())
            {
                command.CommandText = SQL;
                foreach (string key in data.Keys)
                {
                    command.Parameters.AddWithValue("@" + key, data[key]);
                }
                return command.ExecuteNonQuery();
            }
        }
        public ConcurrentDictionary<string, object> curl_getPost_INIT(string URL, Dictionary<string, string> posts, ConcurrentDictionary<string, object> options = null)
        {
            ConcurrentDictionary<string, string> p = new ConcurrentDictionary<string, string>();
            foreach (string k in posts.Keys)
            {
                p[k] = posts[k];
            }
            return curl_getPost_INIT(URL, p, options);
        }
        public ConcurrentDictionary<string, object> curl_getPost_INIT(string URL, ConcurrentDictionary<string, string> posts, ConcurrentDictionary<string, object> options = null)
        {
            //NameValueCollection postParameters = new NameValueCollection();
            List<string> mPostData = new List<string>();
            int step = 0;
            List<Dictionary<string, string>> upfiles = new List<Dictionary<string, string>>();
            if (posts != null)
            {
                foreach (string k in posts.Keys)
                {
                    //postParameters.Add(k, posts[k]);                
                    if (posts[k].Length > 0 && posts[k].Substring(0, 1) == "@" && is_file(posts[k].Substring(1, posts[k].Length - 1)))
                    {
                        //file_put_contents("C:\\temp\\a.txt", posts[k].Substring(1, posts[k].Length - 1));
                        //這是檔案
                        var d = new Dictionary<string, string>();
                        d[post_encode_string(k)] = posts[k].Substring(1, posts[k].Length - 1);
                        upfiles.Add(d);
                    }
                    else
                    {
                        mPostData.Add(post_encode_string(k) + "=" + post_encode_string(posts[k]));
                    }
                    step++;

                }
            }
            upfiles = (upfiles.Count == 0) ? null : upfiles;
            return curl_getPost_INIT(URL, implode("&", mPostData), upfiles, options);
        }
        public string implode(string keyword, List<string> arrays)
        {
            return string.Join<string>(keyword, arrays);
        }
        private string post_encode_string(string value)
        {
            /*int limit = 2000;

            StringBuilder sb = new StringBuilder();
            int loops = value.Length / limit;

            for (int i = 0; i <= loops; i++)
            {
                if (i < loops)
                {
                    sb.Append(Uri.EscapeDataString(value.Substring(limit * i, limit)));
                }
                else
                {
                    sb.Append(Uri.EscapeDataString(value.Substring(limit * i)));
                }
            }
            //Uri.EscapeDataString()
            return sb.ToString();
            */
            return Uri.EscapeDataString(value);
        }
        public string post_decode_string(string value)
        {
            return Uri.UnescapeDataString(value);
        }
        public ConcurrentDictionary<string, object> curl_getPost_INIT(string URL, string postData, List<Dictionary<string, string>> upfiles, ConcurrentDictionary<string, object> options = null)
        {
            // ConcurrentDictionary<string, object> options = new ConcurrentDictionary<string, object>();
            // ConcurrentDictionary<string, string> headers = new ConcurrentDictionary<string, string>();
            // options["headers"] = headers;
            //file_put_contents("C:\\temp\\a.txt", postData);
            //From : https://stackoverflow.com/questions/2972643/how-to-use-cookies-with-httpwebrequest
            ConcurrentDictionary<string, object> output = new ConcurrentDictionary<string, object>();
            output["cookies"] = new CookieContainer();

            //Uri U = null;
            try
            {
                //U = new Uri(URL);
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(URL);
                HttpRequestCachePolicy noCachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.NoCacheNoStore);
                request.CachePolicy = noCachePolicy;
                request.CookieContainer = (CookieContainer)output["cookies"];
                
                request.UserAgent = _userAgent;



                if (options != null)
                {
                    if (options.ContainsKey("login_id") && options.ContainsKey("login_pd"))
                    {
                        if (options["login_id"].ToString() != "")
                        {
                            CredentialCache mycache = new CredentialCache();
                            Uri uri = new Uri(URL);
                            mycache.Add(uri, "Basic", new NetworkCredential(options["login_id"].ToString(), options["login_pd"].ToString()));
                            //加入另一種 Digest 驗證
                            mycache.Add(
                              new Uri(uri.GetLeftPart(UriPartial.Authority)), // request url's host
                              "Digest",  // authentication type 
                              new NetworkCredential(options["login_id"].ToString(), options["login_pd"].ToString()) // credentials 
                            );
                            request.Credentials = mycache;
                        }
                    }
                    if (options.ContainsKey("timeout"))
                    {
                        request.Timeout = Convert.ToInt32(options["timeout"]);
                    }
                    if (options.ContainsKey("cookie"))
                    {
                        //request.Headers.Add("Cookie", options["cookie"].ToString());
                        //request.CookieContainer.Add( = options["cookie"].ToString();      
                        request.CookieContainer = new CookieContainer();
                        Uri uri = new Uri(URL);
                        request.CookieContainer.SetCookies(uri, options["cookie"].ToString());
                    }
                    if (options.ContainsKey("user_agent"))
                    {
                        request.UserAgent = options["user_agent"].ToString();
                    }
                    if (options.ContainsKey("headers") && options["headers"] != null)
                    {
                        foreach (string k in ((ConcurrentDictionary<string, string>)options["headers"]).Keys)
                        {
                            if (k.ToUpper().Replace("-", "") == "CONTENTTYPE")
                            {
                                request.ContentType = ((ConcurrentDictionary<string, string>)options["headers"])[k];
                                continue;
                            }
                            request.Headers[k] = ((ConcurrentDictionary<string, string>)options["headers"])[k];
                        }
                    }
                }
                request.Proxy = null;




                HttpWebResponse response = null;
                if (postData == "" && (upfiles == null || upfiles.Count == 0))
                {
                    //GET         
                    request.Method = "GET";
                    response = (HttpWebResponse)request.GetResponse();
                    Stream stream = response.GetResponseStream();
                    output["data"] = ReadStream(stream, 32765);
                    stream.Close();
                }
                else if (upfiles == null || upfiles.Count == 0) //post only
                {
                    request.Method = "POST";
                    //Post
                    byte[] data = Encoding.UTF8.GetBytes(postData);
                    if (request.ContentType == null)
                    {
                        request.ContentType = "application/x-www-form-urlencoded";
                    }
                    request.ContentLength = data.Length;
                    using (Stream stream = request.GetRequestStream())
                    {
                        stream.Write(data, 0, data.Length);
                        stream.Close();
                    }
                    Array.Clear(data, 0, data.Length);
                    data = null;
                    response = (HttpWebResponse)request.GetResponse();
                    Stream streamD = response.GetResponseStream();
                    output["data"] = ReadStream(streamD, 32767);
                    streamD.Close();
                }
                else if (upfiles.Count() != 0)
                {
                    //Post


                    string boundary = "----------------------------" + DateTime.Now.Ticks.ToString("x");
                    // The first boundary
                    byte[] firstBoundaryBytes = System.Text.Encoding.UTF8.GetBytes("--" + boundary + "\r\n");
                    byte[] boundaryBytes = System.Text.Encoding.UTF8.GetBytes("\r\n--" + boundary + "\r\n");
                    // The last boundary
                    byte[] trailer = System.Text.Encoding.UTF8.GetBytes("\r\n--" + boundary + "--\r\n");

                    request.Method = "POST";
                    request.ContentType = "multipart/form-data; boundary=" + boundary + "";



                    // Get request stream
                    Stream requestStream = request.GetRequestStream();




                    /*
                     Content-Disposition: form-data; name="gg"\r\n\r\n
        GG
                    */

                    if (postData != "")
                    {
                        var m = explode("&", postData);
                        for (int i = 0, max_i = m.Count(); i < max_i; i++)
                        {
                            var d = explode("=", m[i]);
                            if (d.Length < 2) continue;
                            // Write item to stream
                            string key = post_decode_string(d[0]);
                            string keyvalue = post_decode_string(d[1]);
                            byte[] formItemBytes = s2b(string.Format("Content-Disposition: form-data; name={0};\r\n\r\n{1}", key, keyvalue));
                            if (firstBoundaryBytes != null)
                            {
                                requestStream.Write(firstBoundaryBytes, 0, firstBoundaryBytes.Length);
                                Array.Clear(firstBoundaryBytes, 0, firstBoundaryBytes.Length);
                                firstBoundaryBytes = null;
                            }
                            else
                            {
                                requestStream.Write(boundaryBytes, 0, boundaryBytes.Length);
                            }
                            requestStream.Write(formItemBytes, 0, formItemBytes.Length);
                            Array.Clear(formItemBytes, 0, formItemBytes.Length);
                            formItemBytes = null;
                        }
                    }

                    if (upfiles != null && upfiles.Count > 0)
                    {
                        foreach (Dictionary<string, string> d in upfiles)
                        {
                            foreach (string keyname in d.Keys)
                            {
                                //keyname = string.Join("", d.Keys);
                                string filename = post_decode_string(d[keyname]);
                                string bn = basename(filename);
                                string dekeyname = post_decode_string(keyname);
                                if (File.Exists(filename))
                                {
                                    if (firstBoundaryBytes != null)
                                    {
                                        requestStream.Write(firstBoundaryBytes, 0, firstBoundaryBytes.Length);
                                        Array.Clear(firstBoundaryBytes, 0, firstBoundaryBytes.Length);
                                        firstBoundaryBytes = null;
                                    }
                                    else
                                    {
                                        requestStream.Write(boundaryBytes, 0, boundaryBytes.Length);
                                    }


                                    byte[] formItemBytes = s2b(string.Format("Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: application/octet-stream\r\n\r\n", dekeyname, filename));

                                    requestStream.Write(formItemBytes, 0, formItemBytes.Length);
                                    Array.Clear(formItemBytes, 0, formItemBytes.Length);
                                    byte[] buffer = file_get_contents(filename);
                                    requestStream.Write(buffer, 0, buffer.Length);
                                    Array.Clear(buffer, 0, buffer.Length);
                                    buffer = null;
                                }
                            }
                        }
                    }

                    // Write trailer and close stream
                    requestStream.Write(trailer, 0, trailer.Length);
                    requestStream.Close();

                    Array.Clear(boundaryBytes, 0, boundaryBytes.Length);
                    Array.Clear(trailer, 0, trailer.Length);
                    boundaryBytes = null;
                    trailer = null;

                    response = (HttpWebResponse)request.GetResponse();
                    Stream streamD = response.GetResponseStream();
                    output["data"] = ReadStream(streamD, 32767);
                    streamD.Close();
                }
                output["headers"] = new Dictionary<string, string>();
                foreach (var k in response.Headers)
                {
                    ((Dictionary<string, string>)output["headers"])[k.ToString()] = response.Headers[k.ToString()].ToString();
                }
                output["realCookie"] = response.Headers[HttpResponseHeader.SetCookie];
                response.Close();
                output["reason"] = "";
                output["status"] = "OK";
                return output;
            }
            catch (Exception ex)
            {
                output["status"] = "NO";
                output["data"] = new byte[0];
                output["reason"] = ex.Message + "\n\r" + ex.StackTrace;
                return output;
            }
        }

    }
    public class myZip : IDisposable
    {
        private FileStream zipToOpen;
        private ZipArchive archive;
        private string _zipFile = "";
        private List<Dictionary<string, object>> zipQueue = new List<Dictionary<string, object>>();
        private Timer addFileTimer;
        private bool isFlushing = false;
        public myZip(string zipFile)
        {
            zipQueue = new List<Dictionary<string, object>>();
            _zipFile = zipFile;
            zipToOpen = new FileStream(_zipFile, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
            archive = new ZipArchive(zipToOpen, ZipArchiveMode.Update, true);

            // 定期每 5 秒強制刷新一次
            addFileTimer = new Timer(addCallback, null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5));

            // 設置 Console 取消處理，捕獲中斷信號（Ctrl+C）
            Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true; // 取消退出
                Dispose(); // 清理資源
                Console.WriteLine("中斷操作，資源已釋放");
            };
        }
        private void addCallback(object state)
        {
            // 從 zipQueue 取出資料，並加入到 ZIP 檔案中
            if (isFlushing)
            {
                return;
            }
            int count = zipQueue.Count;
            for (int i = 0; i < count; i++)
            {
                var item = zipQueue[i];
                if (isFlushing)
                {
                    return;
                }
                AddFile((byte[])item["fromByte"], item["toInSideZipfile"].ToString());
            }
            // 移除 0 ~ count
            zipQueue.RemoveRange(0, count);
            // Flush            
            Flush();
        }
        public void AddByteToQueue(byte[] fromByte, string toInSideZipfile)
        {
            Dictionary<string, object> zipItem = new Dictionary<string, object>();
            zipItem["fromByte"] = fromByte;
            zipItem["toInSideZipfile"] = toInSideZipfile;
            zipQueue.Add(zipItem);
        }

        public bool IsFile(string toInSideZipfile)
        {
            return archive.GetEntry(toInSideZipfile) != null;
        }
        public bool AddFile(byte[] frombytearray, string toInSideZipfile, CompressionLevel compressionLevel = CompressionLevel.NoCompression)
        {
            try
            {
                // 檢查 ZIP 內是否已有相同檔案，若有則刪除
                var existingEntry = archive.GetEntry(toInSideZipfile);
                existingEntry?.Delete();

                // 新增檔案
                var entry = archive.CreateEntry(toInSideZipfile, compressionLevel);
                using (var stream = entry.Open())
                {
                    stream.Write(frombytearray, 0, frombytearray.Length);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
        public bool AddFile(string fromFile, string toInSideZipfile, CompressionLevel compressionLevel = CompressionLevel.NoCompression)
        {
            try
            {
                // 檢查 ZIP 內是否已有相同檔案，若有則刪除
                var existingEntry = archive.GetEntry(toInSideZipfile);
                existingEntry?.Delete();

                // 新增檔案
                archive.CreateEntryFromFile(fromFile, toInSideZipfile, compressionLevel);
                return true;
            }
            catch
            {
                return false;
            }
        }
        public bool DelFile(string toInSideZipfile)
        {
            try
            {
                // 檢查 ZIP 內是否已有相同檔案，若有則刪除
                var existingEntry = archive.GetEntry(toInSideZipfile);
                existingEntry?.Delete();
                return true;
            }
            catch
            {
                return false;
            }
        }
        public void Flush()
        {
            isFlushing = true;
            zipToOpen.Flush();
            archive?.Dispose();
            zipToOpen?.Dispose();
            zipToOpen = new FileStream(_zipFile, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
            archive = new ZipArchive(zipToOpen, ZipArchiveMode.Update, true);
            isFlushing = false;
        }
        public void Dispose()
        {
            // 釋放資源
            addFileTimer?.Dispose();
            archive?.Dispose();
            zipToOpen?.Dispose();
        }
    }
}