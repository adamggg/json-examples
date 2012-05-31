using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text;

using Newtonsoft.Json.Linq;

namespace example {
    class Example {
        private const string RESOURCE = "/geolocation.json";

        static string GetSettings(string url) {
            // add the resource we're looking for
            url += RESOURCE;

            StringBuilder sb = new StringBuilder();
            byte[] buf = new byte[8192];

            // initialize the request
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url + "?minWait=1000");

            HttpWebResponse res = (HttpWebResponse)req.GetResponse();

            Stream resStream = res.GetResponseStream();

            int count = 0;
            while ((count = resStream.Read(buf, 0, buf.Length)) > 0) {
                sb.Append(Encoding.UTF8.GetString(buf, 0, count));
            }

            return sb.ToString();
        }

        static string GetSettingsGzip(string url) {
            // add the resource we're looking for
            url += RESOURCE;

            StringBuilder sb = new StringBuilder();
            byte[] buf = new byte[8192];

            // initialize the request
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url + "?minWait=1000");
            req.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip;q=1.0");

            HttpWebResponse res = (HttpWebResponse)req.GetResponse();

            Stream resStream = res.GetResponseStream();
            if (res.ContentEncoding.Equals("gzip", StringComparison.CurrentCultureIgnoreCase)) {
                resStream = new GZipStream(resStream, CompressionMode.Decompress);
            }

            int count = 0;
            while ((count = resStream.Read(buf, 0, buf.Length)) > 0) {
                sb.Append(Encoding.UTF8.GetString(buf, 0, count));
            }
            return sb.ToString();
        }

        static string SetSettings(string url, string settings) {
            // we know it's UTF-8
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(settings);
            StringBuilder sb = new StringBuilder();
            byte[] buf = new byte[8192];

            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url + RESOURCE + "/settings");
            req.Method = "POST";
            req.ContentType = "application/json";
            req.ContentLength = settings.Length;

            // get the request stream and write our data back to it
            Stream writeStream = req.GetRequestStream();
            writeStream.Write(bytes, 0, bytes.Length);

            HttpWebResponse res = (HttpWebResponse)req.GetResponse();

            Stream resStream = res.GetResponseStream();

            int count = 0;
            while ((count = resStream.Read(buf, 0, buf.Length)) > 0) {
                sb.Append(Encoding.UTF8.GetString(buf, 0, count));
            }

            return sb.ToString();
        }

        static string ParseSettingsJSONNet(string settings) {
            JObject obj = JObject.Parse(settings);
            // get the result object
            JToken result = obj["result"];

            float alt = result["altitude"].Value<float>();

            if (alt > 2000) {
                alt = 747;
            } else {
                alt += 1000;
            }

            // modify the altitude with our new value
            result["altitude"] = alt;

            return result.ToString();
        }

        static void Main(string[] args) {
            if (args.Length != 1) {
                Console.WriteLine("Usage: example.exe <url>");
                return;
            }

            string url = args[0];

            // add the http if we forgot it
            if (!url.StartsWith("http://")) {
                url = "http://" + url;
            }
            if (url.EndsWith("/")) {
                url = url.Substring(0, url.Length - 1);
            }

            string res = GetSettings(url);

            Console.WriteLine("Before:");
            Console.WriteLine(res);
            Console.WriteLine();

            string newSettings = ParseSettingsJSONNet(res);

            Console.WriteLine("After:");
            Console.WriteLine(newSettings);
            Console.WriteLine();

            string response = SetSettings(url, newSettings);

            Console.WriteLine("POST Response:");
            Console.WriteLine(response);
            Console.WriteLine();

            response = GetSettingsGzip(url);

            Console.WriteLine("New Settings:");
            Console.WriteLine(response);
        }
    }
}
