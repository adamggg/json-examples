using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text;

using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;


namespace example {
    
    public class SubscriptionHost {
        public string Host;
        public string Pathname;
    }
    
    public class SubscriptionTarget {
        public class FilterStruct {
            public Int32 MinWait;
            public Int32 MaxWait;
            public Int32 Since;
            public Int32 Until;
        }
        
        public SubscriptionTarget() {
            Params = new JObject();
            Filter = new FilterStruct();
        }
        
        public JObject Params;
        public string Protocol;
        public string Hostname;
        public string Pathname;
        public string Port;
        public FilterStruct Filter;
        
    }
    
    public class Subscription {
        
    }
    
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
        
        static Subscription Subscribe(SubscriptionHost sensor, SubscriptionTarget target) {
            string json;
            byte[] postBody;
            StringBuilder sb = new StringBuilder();
            byte[] buf = new byte[8192];
            Stream postBodyWriteStream;
            HttpWebRequest req;
            HttpWebResponse res;
            Stream resStream;
            int responseByteCount;
            
            /*
             * Format the Request POST Body
             */
            target.Params = new JObject(
                    new JProperty("foo", "module")
                ,    new JProperty("bar", "specific")
                ,    new JProperty("baz", "properties")
            );
            target.Filter.MinWait = 1000;
            target.Filter.MaxWait = 1000;

            if ("http:" == target.Protocol.ToLower()) {
              target.Pathname = sensor.Pathname + "?this_is_sent_to_your_listener";
            }

            json = JsonConvert.SerializeObject(
                    target
                ,    Formatting.Indented
                    // use normal JSON lowerCamelCase names instead of C# UpperCamelCase
                ,    new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() }
            );
            
            postBody = System.Text.Encoding.UTF8.GetBytes(json);
            
            /*
             * Create the Request
             */
            req = (HttpWebRequest)WebRequest.Create("http://" + sensor.Host + sensor.Pathname + "/subscriptions");
            req.Method = "POST";
            req.ContentType = "application/json";
            req.ContentLength = postBody.Length;
            postBodyWriteStream = req.GetRequestStream();
            postBodyWriteStream.Write(postBody, 0, postBody.Length);
            
            /*
             * Handle the Response
             */
            res = (HttpWebResponse)req.GetResponse();
            resStream = res.GetResponseStream();

            responseByteCount = 0;
            while ((responseByteCount = resStream.Read(buf, 0, buf.Length)) > 0) {
                sb.Append(Encoding.UTF8.GetString(buf, 0, responseByteCount));
            }

            // just viewing the response for now...
            Console.WriteLine(sb);
            return new Subscription();
        }
        
        static void Unsubscribe(SubscriptionHost sensor, Subscription subscription) {
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
            if (args.Length != 5) {
                Console.WriteLine("Usage: subscription.exe <sensor-host> <sensor-resource> <target-proto> <target-hostname> <target-port>");
                Console.WriteLine("Usage: subscription.exe 192.168.254.254:80 datetime http 192.168.254.100 4444");
                return;
            }
            
            SubscriptionHost sensor = new SubscriptionHost();
            SubscriptionTarget target = new SubscriptionTarget();
            Subscription subscription;

            sensor.Host = args[0];
            sensor.Pathname = args[1];
            
            target.Protocol = args[2];
            target.Hostname = args[3];
            target.Port = args[4];

            // normalize the protocol... because the standard is stupid and expects a trailing ":"
            if (!target.Protocol.EndsWith(":")) {
                target.Protocol = target.Protocol + ":";
            }

            // any use of http is superfluous
            if (sensor.Host.StartsWith("http://")) {
                sensor.Host = sensor.Host.Substring(0, "http://".Length - 1);
            }
            
            // remove trailing slash if present
            if (sensor.Host.EndsWith("/")) {
                sensor.Host = sensor.Host.Substring(0, sensor.Host.Length - 1);
            }


            // add leading "/" if absent
            if (!sensor.Pathname.StartsWith("/")) {
                sensor.Pathname = "/" + sensor.Pathname;
            }
            // add ".json" if no extension is present
            if (!sensor.Pathname.Contains(".")) {
                sensor.Pathname += ".json";
            }
            
            Console.WriteLine("Subscribing...");
            subscription = Subscribe(sensor, target);
            
            Console.WriteLine("Waiting 15 seconds (so you can inspect the subscription)...");
            System.Threading.Thread.Sleep(15000);
            
            
            Console.WriteLine("Unsubscribing...");
            Unsubscribe(sensor, subscription);
        }
    }
}
