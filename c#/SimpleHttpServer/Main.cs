// Taken from
// http://stackoverflow.com/questions/5485951/httplistener-problem
// http://msdn.microsoft.com/en-us/library/system.net.httplistener.aspx
// http://msdn.microsoft.com/en-us/library/system.net.httplistener.getcontext
// http://msdn.microsoft.com/en-us/library/system.net.httplistenerrequest.headers.aspx
// http://msdn.microsoft.com/en-us/library/system.net.httplistenerrequest.inputstream.aspx

// Tested with:
// curl -v http://localhost:8088/somethingawesome -X GET
// curl -v http://localhost:8088/foo/bar -X POST -d 'baz=corge'

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Threading;

namespace SimpleHttpServer
{
    class HttpListenerClass
    {
        bool keepAlive = true;

        private HttpListener listener;

        public HttpListenerClass()
        {
            ThreadPool.SetMaxThreads(50, 100);
            ThreadPool.SetMinThreads(50, 50);
            listener = new HttpListener();
            listener.Prefixes.Add("http://*:8088/");
        }

        public void Start()
        {
            Console.WriteLine("Called Start");
            listener.Start();

            while (keepAlive == true) {
            {
                try
                {
                    HttpListenerContext ctx = listener.GetContext();
                    ThreadPool.QueueUserWorkItem(new WaitCallback(ProcessRequest), ctx);
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);

                }
              }
            }
        }

        public void Stop()
        {
            Console.WriteLine("Called Stop");
            listener.Stop();
            keepAlive = false;
        }

        public static void DisplayHttpRequest(HttpListenerRequest request)
        {
            Console.Write(request.HttpMethod + " ");
            Console.Write(request.RawUrl + " ");
            Console.WriteLine("HTTP/" + request.ProtocolVersion);
        }


        public static void DisplayHttpRequestHeaders(HttpListenerRequest request)
        {
            System.Collections.Specialized.NameValueCollection headers = request.Headers;
            // Get each header and display each value.
            foreach (string key in headers.AllKeys)
            {
                string[] values = headers.GetValues(key);

                foreach (string value in values)
                {
                    Console.Write("{0}: {1} \r\n", key, value);
                }

                /*
                if(values.Length > 0)
                {
                    Console.WriteLine("Weird... no value associated with {0}.", key);
                }
                */
            }
        }

        public static void DisplayHttpRequestBody(HttpListenerRequest request)
        {
            if (!request.HasEntityBody)
            {
                //Console.WriteLine("No client data was sent with the request.");
                return;
            }

            System.IO.Stream body = request.InputStream;
            System.Text.Encoding encoding = request.ContentEncoding;
            System.IO.StreamReader reader = new System.IO.StreamReader(body, encoding);

            /*
            // this should have already been printed with the headers above
            if (request.ContentType != null)
            {
                Console.WriteLine("Client data content type {0}", request.ContentType);
            }
            */
            //Console.WriteLine("Client data content length {0}", request.ContentLength64);

            // Convert the data to a string and display it on the console.
            string s = reader.ReadToEnd();
            Console.Write(s);
            body.Close();
            reader.Close();
            // If you are finished with the request, it should be closed also.
        }

        public static void LogHttpRequest(HttpListenerRequest request) {
             // non-HTTP, just for pretty-printing
            Console.WriteLine("");
            Console.WriteLine("------------");
            Console.WriteLine("NEW HTTP REQUEST");
            Console.WriteLine("------------");


            // All part of HTTP
            DisplayHttpRequest(request);
            DisplayHttpRequestHeaders(request);
            Console.Write("\r\n\r\n"); // HTTP Header / Body separater
            DisplayHttpRequestBody(request);

             // non-HTTP, just for pretty-printing
            Console.Write("\r\n");
        }

        public void ProcessRequest(object listenerContext)
        {
            try
            {
                var context = (HttpListenerContext)listenerContext;
                HttpListenerRequest request = context.Request;
                HttpListenerResponse response = context.Response;

                LogHttpRequest(request);

                //string QS = request.QueryString["ID"];
                string responseString = "<HTML><BODY> Hello world!</BODY></HTML>";
                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
                // Get a response stream and write the response to it.
                response.ContentLength64 = buffer.Length;
                System.IO.Stream output = response.OutputStream;
                output.Write(buffer,0,buffer.Length);
                // You must close the output stream.
                output.Close();
            }

            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }

    class MainClass
    {
        public static void Main (string[] args)
        {
            Console.WriteLine("Hello World!");
            HttpListenerClass HTTP = new HttpListenerClass();
            HTTP.Start();
            Console.WriteLine("Start has been started");
        }
    }
}