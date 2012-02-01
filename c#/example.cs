using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

//using Newtonsoft.Json.Linq;
using fastJSON;

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

        static string ParseSettingsFastJSON(string settings) {
            Dictionary<string, object> jsonData = JSON.Instance.Parse(settings) as Dictionary<string, object>;
            Dictionary<string, object> result = jsonData["result"] as Dictionary<string, object>;

            float alt = float.Parse(result["altitude"] as string);

            if (alt > 2000) {
                alt = 747;
            } else {
                alt += 1000;
            }

            result["altitude"] = alt;
            return JSON.Instance.ToJSON(result);
        }

/*
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
*/

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
            string newSettings = ParseSettingsFastJSON(res);
            //string newSettings = ParseSettingsJSONNet(res);
            string response = SetSettings(url, newSettings);

            Console.WriteLine(response);
        }
    }
}
