using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Collections;
using HtmlAgilityPack;
using System.Globalization;

namespace ConsoleApplication3
{
    class Program
    {
        static void Main(string[] args)
        {
            ArrayList htmls = new ArrayList();
            ArrayList fileNames = new ArrayList();

            //htmls.Add("http://yule.baidu.com/");
            //htmls.Add("http://mil.news.baidu.com/");
            //htmls.Add("http://finance.baidu.com/");
            //htmls.Add("http://sports.baidu.com/");
            //htmls.Add("http://guoji.news.baidu.com/");
            //htmls.Add("http://shehui.news.baidu.com/");
            //htmls.Add("http://tech.baidu.com/");
            //htmls.Add("http://lady.baidu.com/");
            //htmls.Add("http://auto.baidu.com/");
            htmls.Add("http://youxi.news.baidu.com/");
            htmls.Add("http://jiaoyu.news.baidu.com/");

            //fileNames.Add("entertainment");
            //fileNames.Add("military");
            //fileNames.Add("finance");
            //fileNames.Add("sports");
            //fileNames.Add("international");
            //fileNames.Add("social");
            //fileNames.Add("technology");
            //fileNames.Add("lady");
            //fileNames.Add("auto");
            fileNames.Add("game");
            fileNames.Add("education");

            for (int i=0;i<htmls.Count & i<fileNames.Count; i++)
            {
                ArrayList urls = new ArrayList();
                ArrayList webTitle = new ArrayList();
                HtmlDocument doc = new HtmlDocument();
                StreamWriter outFile = new StreamWriter(@"../../"+fileNames[i]+".txt");
                string html = GetWebHtml((string)htmls[i], null);
                doc.LoadHtml(html);
                try
                {
                    foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//a[@href]"))
                    {
                        string url = node.Attributes["href"].Value; //that's the text you are looking for
                        string text = node.InnerText;
                        if (text.Length < 6)
                            continue;
                        urls.Add(url);
                        webTitle.Add(text);
                        Console.WriteLine(text + "  " + text.Length);
                    }
                }
                catch
                {
                    Console.WriteLine("document is null!!!");
                }

                Console.WriteLine(urls.Count);

                int countIndex = 0;
                foreach (string url in urls)
                {
                    Console.WriteLine(webTitle[countIndex] + " " + urls[countIndex]);
                    Console.WriteLine(countIndex++);
                    string tempHtml = GetWebHtml(url, null);
                    if (tempHtml == null)
                    {
                        Console.WriteLine("html is null!!!");
                        continue;
                    }

                    string pageContent = "";
                    doc.LoadHtml(tempHtml);
                    try
                    {
                        foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//p"))
                            foreach (char c in node.InnerText)
                                if (char.GetUnicodeCategory(c) == UnicodeCategory.OtherLetter)
                                    pageContent += c;
                        if(pageContent.Length > 100)
                            outFile.WriteLine(pageContent);
                        Console.WriteLine(pageContent);
                        Console.WriteLine(pageContent.Length);
                    }
                    catch
                    {
                        Console.WriteLine("document is null!!!");
                    }
                }
                outFile.Close();
            }

            Console.WriteLine("Press any key to continue...");
            Console.ReadKey(true);
        }

        public static string GetWebHtml(string url, Encoding encoding)
        {
            try
            {
                HttpWebRequest hwr = (HttpWebRequest)HttpWebRequest.Create(url);
                HttpWebResponse res;

                try
                {
                    res = (HttpWebResponse)hwr.GetResponse();
                }
                catch
                {
                    return string.Empty;
                }

                if (res.StatusCode == HttpStatusCode.OK)
                {
                    using (Stream mystream = res.GetResponseStream())
                    {
                        if (encoding == null)
                        {
                            return DecodeData(mystream, res);
                        }
                        else
                        {
                            using (StreamReader reader = new StreamReader(mystream, encoding))
                            {
                                return reader.ReadToEnd();
                            }
                        }
                    }
                }

                return null;
            }
            catch
            {
                return null;
            }
        }


        private static string DecodeData(Stream responseStream, HttpWebResponse response)
        {
            string name = null;
            string text2 = response.Headers["content-type"];
            if (text2 != null)
            {
                int index = text2.IndexOf("charset=");
                if (index != -1)
                {
                    name = text2.Substring(index + 8);
                }
            }
            MemoryStream stream = new MemoryStream();
            byte[] buffer = new byte[0x400];
            for (int i = responseStream.Read(buffer, 0, buffer.Length); i > 0; i = responseStream.Read(buffer, 0, buffer.Length))
            {
                stream.Write(buffer, 0, i);
            }
            responseStream.Close();
            if (name == null)
            {
                MemoryStream stream3 = stream;
                stream3.Seek((long)0, SeekOrigin.Begin);
                string text3 = new StreamReader(stream3, Encoding.ASCII).ReadToEnd();
                if (text3 != null)
                {
                    int startIndex = text3.IndexOf("charset=");
                    int num4 = -1;
                    if (startIndex != -1)
                    {
                        num4 = text3.IndexOf("\"", startIndex);
                        if (num4 != -1)
                        {
                            int num5 = startIndex + 8;
                            name = text3.Substring(num5, (num4 - num5) + 1).TrimEnd(new char[] { '>', '"' });
                        }
                    }
                }
            }
            Encoding aSCII = null;
            if (name == null)
            {
                aSCII = Encoding.GetEncoding("gb2312");
            }
            else
            {
                try
                {
                    if (name == "GBK")
                    {
                        name = "GB2312";
                    }
                    aSCII = Encoding.GetEncoding(name);
                }
                catch
                {
                    aSCII = Encoding.GetEncoding("gb2312");
                }
            }
            stream.Seek((long)0, SeekOrigin.Begin);
            StreamReader reader2 = new StreamReader(stream, aSCII);
            return reader2.ReadToEnd();
        }
    }
}
