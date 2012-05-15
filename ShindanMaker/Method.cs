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
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create("http://shindanmaker.com/"+id);
            webRequest.Method = "POST";
            webRequest.ContentType = "multipart/form-data";

            // Start the request
            webRequest.BeginGetRequestStream(r =>
            {
                try
                {
                    HttpWebRequest request1 = (HttpWebRequest)r.AsyncState;
                    Stream postStream = request1.EndGetRequestStream(r);


                    string postData = "&u=" + HttpUtility.UrlEncode(name)+"&from=";
                    byte[] byteArray = Encoding.UTF8.GetBytes(postData);

                    postStream.Write(byteArray, 0, byteArray.Length);
                    postStream.Close();

                    request1.BeginGetResponse(
                        s =>
                        {
                            try
                            {
                                HttpWebRequest request2 = (HttpWebRequest)s.AsyncState;
                                HttpWebResponse response = (HttpWebResponse)request2.EndGetResponse(s);

                                Stream streamResponse = response.GetResponseStream();
                                StreamReader streamReader = new StreamReader(streamResponse);
                                string response2 = streamReader.ReadToEnd();
                                streamResponse.Close();
                                streamReader.Close();
                                response.Close();
                                //act("hoge");
                                act(response2);
                                return;
                            }
                            catch
                            {
                            }
                        },
                    request1);
                }
                catch
                {
                }
            }, webRequest);


        }
    }
}
