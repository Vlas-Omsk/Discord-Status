using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.IO;
using System.Reflection;

namespace WEBLib
{
    class WEB
    {
        public const string DefaultUserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/79.0.3945.118 Safari/537.36";

        public static Response Get(string url, string userAgent = DefaultUserAgent)
        {
            return Post(url, new string[] { "User-Agent: " + userAgent }, "", "GET");
        }

        public static Response Post(string url, string[] headers = null, string stringData = "", string method = "POST")
        {
            return Post(url, headers, Encoding.UTF8.GetBytes(stringData), method);
        }

        public static Response Post(string url, string[] headers = null, byte[] data = null, string method = "POST")
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

            if (headers != null)
            {
                foreach (var line in headers)
                {
                    request.SetRawHeader(line.Split(':')[0], line.Substring(line.IndexOf(':') + 1));
                }
            }

            request.Method = method;
            if (data.Length != 0)
                request.ContentLength = data.Length;

            try
            {
                using (var stream = request.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }
            }
            catch { }

            var content = "";
            WebHeaderCollection headers2 = new WebHeaderCollection();

            try
            {
                using (WebResponse response = (HttpWebResponse)request.GetResponse())
                using (Stream s = response.GetResponseStream())
                {
                    headers2 = response.Headers;
                    using (StreamReader sr = new StreamReader(s))
                    {
                        content = sr.ReadToEnd();
                    }
                }
            }
            catch (WebException ex)
            {
                try
                {
                    using (WebResponse response = ex.Response)
                    {
                        headers2 = response.Headers;
                        using (Stream xdata = response.GetResponseStream())
                        using (var reader = new StreamReader(xdata))
                        {
                            content = reader.ReadToEnd();
                        }
                    }
                }
                catch (Exception exe)
                {
                    content = exe.ToString();
                }
            }

            return new Response
            {
                Content = content,
                Headers = headers2
            };
        }

        public static string CreateParams(Dictionary<string, string> parameters)
        {
            var result = "";
            foreach (var param in parameters)
            {
                result += param.Key + "=" + Uri.EscapeDataString(param.Value);
            }
            return result;
        }
    }

    public class Response
    {
        public string Content;
        public WebHeaderCollection Headers;
    }

    public static class WebHeaderCollectionExtension
    {
        public static new string ToString(this WebHeaderCollection headers)
        {
            string res = "";

            foreach (string head in headers)
            {
                res += $"{head}: {headers[head]} ";
            }

            return res;
        }
    }

    public static class HttpWebRequestExtensions
    {
        static string[] RestrictedHeaders = new string[] {
            "Accept",
            "Accept-Charset",
            "Connection",
            "Content-Length",
            "Content-Type",
            "Date",
            "Expect",
            "Host",
            "If-Modified-Since",
            "Keep-Alive",
            "Proxy-Connection",
            "Range",
            "Referer",
            "Transfer-Encoding",
            "User-Agent"
        };

        static Dictionary<string, PropertyInfo> HeaderProperties = new Dictionary<string, PropertyInfo>(StringComparer.OrdinalIgnoreCase);

        static HttpWebRequestExtensions()
        {
            Type type = typeof(HttpWebRequest);
            foreach (string header in RestrictedHeaders)
            {
                PropertyInfo[] arr = type.GetProperties();

                string propertyName = header.Replace("-", "");
                PropertyInfo headerProperty = type.GetProperty(propertyName);
                HeaderProperties[header] = headerProperty;
            }
        }

        public static void SetRawHeader(this HttpWebRequest request, string name, string value)
        {
            if (HeaderProperties.ContainsKey(name))
            {
                PropertyInfo property = HeaderProperties[name];
                if (property.PropertyType == typeof(DateTime))
                    property.SetValue(request, DateTime.Parse(value), null);
                else if (property.PropertyType == typeof(bool))
                    property.SetValue(request, Boolean.Parse(value), null);
                else if (property.PropertyType == typeof(long))
                    property.SetValue(request, Int64.Parse(value), null);
                else
                    property.SetValue(request, value, null);
            }
            else
            {
                request.Headers[name] = value;
            }
        }
    }
}
