using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Xml.Linq;
using Sgml;
using System.Text;
using System.Windows;

namespace ShindanMaker
{
    public static class Method
    {
        public static void GetShindan(int id, Action<object> act)
        {
            var wc = new WebClient();
            wc.OpenReadAsync(new Uri("http://shindanmaker.com/"+id));
            wc.OpenReadCompleted += (sender, e) =>
            {
                if (e.Error != null)
                {
                    return;
                }

                using (var reader = new StreamReader(e.Result, System.Text.Encoding.UTF8))
                using (var sgmlReader = new SgmlReader { InputStream = reader })
                {
                    sgmlReader.DocType = "HTML";
                    sgmlReader.CaseFolding = CaseFolding.ToLower;

                    var doc = XElement.Load(sgmlReader);
                    var ns = doc.Name.Namespace;
                    var q = from ele in doc.Descendants(ns + "body")
                            .Elements(ns + "div")
                            .Where(ul => ul.Attribute("class") != null
                                && ul.Attribute("class").Value == "wrap1")
                            .Elements(ns + "div")
                            .Elements(ns + "div")
                            .Skip(1)
                            .Elements(ns + "div")
                            select new ShindanItem
                            {
                                Text = ele.Value//Element(ns + "div").Element(ns + "h1").Value,
                            };
                    var re = q.First<ShindanItem>();
                    char[] sep = { ' ', '\n', '\t' };
                    re.Title = re.Text.Split(sep, StringSplitOptions.RemoveEmptyEntries)[0];
                    re.Description = re.Text.Split(sep, StringSplitOptions.RemoveEmptyEntries)[1];
                    act(re);
                    
                }

            };
        }

        public static void ExecShindan(int id, string name, Action<string> act)
        {

            var url = "http://shindanmaker.com/"+id;

            // Create the web request object
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
            webRequest.Method = "POST";
            webRequest.ContentType = "application/x-www-form-urlencoded";

            // Start the request
            webRequest.BeginGetRequestStream(new AsyncCallback(e =>
            {

                HttpWebRequest webRequest1 = (HttpWebRequest)e.AsyncState;
                // End the stream request operation
                Stream postStream = webRequest1.EndGetRequestStream(e);

                // Create the post data
                // Demo POST data 
                string postData = "u="+HttpUtility.UrlEncode(name);

                byte[] byteArray = Encoding.UTF8.GetBytes(postData);

                // Add the post data to the web request
                postStream.Write(byteArray, 0, byteArray.Length);
                postStream.Close();

                // Start the web request
                webRequest1.BeginGetResponse(new AsyncCallback(e2 =>
                {
                    try
                    {
                        HttpWebRequest webRequest2 = (HttpWebRequest)e2.AsyncState;
                        HttpWebResponse response;

                        // End the get response operation
                        response = (HttpWebResponse)webRequest2.EndGetResponse(e2);
                        Stream streamResponse = response.GetResponseStream();
                        TextReader streamReader = new StreamReader(streamResponse, System.Text.Encoding.UTF8);
                        //var Response = streamReader.ReadToEnd();
                        //streamResponse.Close();
                        //streamReader.Close();
                        //response.Close();

                        //using (var reader = new StreamReader(Response, System.Text.Encoding.UTF8))
                        using (var sgmlReader = new SgmlReader { InputStream = streamReader })
                        {
                            sgmlReader.DocType = "HTML";
                            sgmlReader.CaseFolding = CaseFolding.ToLower;

                            var doc = XElement.Load(sgmlReader);
                            var ns = doc.Name.Namespace;
                            var q = from ele in doc.Descendants(ns + "div")
                                    .Where(ul => ul.Attribute("class") != null
                                        && ul.Attribute("class").Value == "result")
                                    .Elements(ns + "div")
                                    select new ShindanItem
                                    {
                                        Text = ele.Value//Element(ns + "div").Element(ns + "h1").Value,
                                    };
                            char[] sep = { ' ', '\n', '\t' };
                            //re.Title = q.First().Text.Split(sep, StringSplitOptions.RemoveEmptyEntries)[0];
                            act(q.First().Text.Split(sep, StringSplitOptions.RemoveEmptyEntries)[0]);
                        }

                    }
                    catch (WebException ex)
                    {
                        // Error treatment
                        // ...
                    }
                }), webRequest1);
            }), webRequest);    
        }
    }
}
