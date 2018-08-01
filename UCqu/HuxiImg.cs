using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;

namespace UCqu
{
    class HuxiImg
    {
        public static async Task<List<HuxiImgEntry>> GetEntries()
        {
            List<HuxiImgEntry> entries = new List<HuxiImgEntry>(24);

            HttpWebRequest request = HttpWebRequest.CreateHttp("http://huxi.cqu.edu.cn/tsnewjson/1");
            //request.Referer = "http://huxi.cqu.edu.cn/newsclass/7f0e208b91d235fd";
            request.Headers[HttpRequestHeader.Referer] = "http://huxi.cqu.edu.cn/newsclass/7f0e208b91d235fd";
            request.Accept = "application/json, text/javascript, */*; q=0.01";
            //request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/64.0.3282.140 Safari/537.36 Edge/17.17134";
            request.Headers[HttpRequestHeader.UserAgent] = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/64.0.3282.140 Safari/537.36 Edge/17.17134";

            HttpWebResponse response = null;
            try
            {
                response = await request.GetResponseAsync() as HttpWebResponse;
            }
            catch(WebException)
            {
                return new List<HuxiImgEntry>();
            }
            StreamReader reader = new StreamReader(response.GetResponseStream());
            string responseStr = reader.ReadToEnd();

            Regex entryRegex = new Regex("\\{\"pichtml\".*?\\}");
            MatchCollection entryMatches = entryRegex.Matches(responseStr);

            foreach(Match m in entryMatches)
            {
                string matchValue = m.Value;

                Regex localRegex = new Regex(@"src=\\"".*?\\""");
                //Regex localRegex = new Regex(@"data-pic=\\"".*?\\""");

                Match localMatch = localRegex.Match(matchValue);
                string imgUri = localMatch.Value.Split('"')[1];
                imgUri = "http://huxi.cqu.edu.cn" + imgUri.Remove(imgUri.Length - 1);

                localRegex = new Regex(@"<p>.*?</p>");
                MatchCollection localMatches = localRegex.Matches(matchValue);
                string title = localMatches[0].Value;
                title = title.Replace("<p>", "");
                title = title.Replace("</p>", "");
                title.Trim();

                string content = localMatches[1].Value;
                content = content.Replace("<p>", "");
                content = content.Replace("</p>", "");
                content.Trim();

                localRegex = new Regex(@"<span class=\\""name\\"">.*?</span>");
                localMatch = localRegex.Match(matchValue);
                string author = localMatch.Value.Split('>')[1];
                author = author.Remove(author.IndexOf("（"));
                //author = author.Replace("（图片版权归作者所有）</span", "");
                author = author.Replace("摄影:", "");
                entries.Add(new HuxiImgEntry(title, imgUri, content, author));
            }

            return entries;
        }
    }

    class HuxiImgEntry
    {
        public HuxiImgEntry(string title, string uri, string content, string author)
        {
            Title = title;
            Uri = uri;
            Content = content;
            Author = author;
        }

        public string Title { get; private set; }
        public string Uri { get; private set; }
        public string Content { get; private set; }
        public string Author { get; private set; }

        public override string ToString()
        {
            return $"{Title} - {Author}";
        }
    }
}
