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


namespace SubscriptionExample {
    
    public class SubscriptionHost {
        public string Host;
        public string Pathname;
    }
    
    public class SubscriptionTarget {
        public class FilterStruct {
            public Int32 MinWait;
            public Int32 MaxWait;
            // not sure how to get JSON.Net to set these
            // to undefined or null rather than zero
            //public Int32 Since;
            //public Int32 Until;
        }
        
        public SubscriptionTarget() {
            Params = new JObject();
            Filter = new FilterStruct();
            //Since = null;
            //Until = null;
        }
        
        public JObject Params;
        public string Protocol;
        public string Hostname;
        public string Pathname;
        public string Port;
        public FilterStruct Filter;
        
    }
    
    public class Subscription {
        // TODO
    }
    
    class SubscriptionMain {
        
        // The data that goes across the wire will look something like this
        /*
          POST /geolocation.json/subscriptions HTTP/1.1
          Host: 192.168.254.254:8088 
          Content-Type: application/json 
          Content-Length: 288 
          Connection: keep-alive 

          {
            "params": {
              "foo": "module",
              "bar": "specific",
              "baz": "properties"
            },
            "protocol": "http:",
            "hostname": "192.168.254.100",
            "pathname": "/geolocation.json?this_is_sent_to_your_listener",
            "port": "4444",
            "filter": {
              "minWait": 1000,
              "maxWait": 1000
            }
          }
        */
        static Subscription Subscribe(SubscriptionHost sensor, SubscriptionTarget target) {
            string json;
            string url;
            byte[] postBody;
            StringBuilder sb = new StringBuilder();
            byte[] buf = new byte[8192];
            Stream postBodyWriteStream;
            HttpWebRequest req;
            HttpWebResponse res;
            Stream resStream;
            int responseByteCount;
            
            /*
             * Format the Request POST Body, which contains information about the target
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

            /*
             * Create the Request
             */
            // Form URL and POST Body
            url = "http://" + sensor.Host + sensor.Pathname + "/subscriptions";
            postBody = System.Text.Encoding.UTF8.GetBytes(json);
            // Form Request and Headers
            Console.WriteLine(url);
            req = (HttpWebRequest)WebRequest.Create(url);
            req.Method = "POST";
            req.ContentType = "application/json";
            req.ContentLength = postBody.Length;
            // Send the Body (headers are sent automatically at start of write)
            Console.WriteLine(postBody);
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
          // TODO
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
                Console.WriteLine("Usage: subscription.exe 192.168.254.254:80 /geolocation.json http 192.168.254.100 4444");
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

            /*
             * Adjusting the user's commandline input for SubscriptionTarget
             */
            if (!target.Protocol.EndsWith(":")) {
                // normalize the protocol... because the standard is stupid and expects a trailing ":"
                target.Protocol = target.Protocol + ":";
            }

            /*
             * Adjusting the user's commandline input for SubscriptionHost
             */
            if (sensor.Host.StartsWith("http://")) {
                // any use of http is superfluous
                sensor.Host = sensor.Host.Substring(0, "http://".Length - 1);
            }
            if (sensor.Host.EndsWith("/")) {
                // remove trailing slash if present
                sensor.Host = sensor.Host.Substring(0, sensor.Host.Length - 1);
            }
            if (!sensor.Pathname.StartsWith("/")) {
                // add leading "/" if absent
                sensor.Pathname = "/" + sensor.Pathname;
            }
            if (!sensor.Pathname.Contains(".")) {
                // add ".json" if no extension is present
                sensor.Pathname += ".json";
            }

            /*
             * Now time for the heavy-lifting
             */
            Console.WriteLine("Subscribing...");
            subscription = Subscribe(sensor, target);
            
            Console.WriteLine("Waiting 15 seconds (so you can inspect the subscription)...");
            System.Threading.Thread.Sleep(15000);
            
            
            Console.WriteLine("Unsubscribing...");
            Unsubscribe(sensor, subscription);
        }
    }
}
