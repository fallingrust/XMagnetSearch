using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using System.Collections.Generic;
using System.Data;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;

namespace XMagnetSearch
{
    internal class Program
    {
        
        static async Task Main(string[] args)
        {
          
            await SearchByZhongisouAsync(1, "特朗普");
            Console.WriteLine("Hello, World!");
            Console.ReadKey();
        }


        public static async Task SearchByBTSOWAsync(int page,string search)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/119.0.0.0 Safari/537.36 Edg/119.0.0.0");
            var url = $"https://btsow.motorcycles/search/{search}/page/{page}";
            var response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var parser = new HtmlParser();

                var content = await response.Content.ReadAsStringAsync();
                var document = parser.ParseDocument(content);
                var elemnets = document.All.Where(p => p.ClassName == "row");
                foreach(var elemnet in elemnets )
                {
                    if(elemnet.Children.Count() == 3)
                    {
                        if (elemnet.Children[0] is IHtmlAnchorElement urlElement
                            && elemnet.Children[1] is IHtmlDivElement sizeElement
                            && elemnet.Children[2] is IHtmlDivElement dateElement)
                        {
                            var magnetHash = urlElement.Href.Substring(urlElement.Href.Length - 40, 40);
                            var title = urlElement.Title;
                            var size = sizeElement.TextContent;
                            var date = dateElement.TextContent;
                        }
                    }
                }
            }
            Console.WriteLine("111111111111");
        }


        public static async Task SearchByEUSAsync(int page, string search)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/119.0.0.0 Safari/537.36 Edg/119.0.0.0");
            using var md5 = MD5.Create();
            var data = md5.ComputeHash(Encoding.UTF8.GetBytes($"{search}999"));
            var param = BitConverter.ToString(data).Replace("-", string.Empty).Substring(0, 7).ToLower();
            var url = $"https://{GetUrlTime()}.eusjdkws.lol/list.php?q={search}&m=&f=_all&s=&p={page}&tk={param}";
            var response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine(content);
            }
            Console.WriteLine("111111111111");
        }

        public static async Task SearchBySKKAsync(int page, string search)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/119.0.0.0 Safari/537.36 Edg/119.0.0.0");

            var url = $"https://11h.sokk24.buzz/search/{search}/page-{page}.html";
            var response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var parser = new HtmlParser();
                var content = await response.Content.ReadAsStringAsync();
                var document = parser.ParseDocument(content);
                var elemnets = document.All.Where(p => p.ClassName == "item");
                foreach ( var elemnet in elemnets )
                {
                  
                    if (!elemnet.TextContent.Contains("在线播放"))
                    {
                        var array = elemnet.TextContent.Split(" ");
                        if (array.Length > 5 && elemnet.FirstElementChild != null && elemnet.FirstElementChild.FirstElementChild is IHtmlAnchorElement urlElement)
                        {
                            var title = array[1].Substring(0, array[1].IndexOf("Hot"));
                            var size = array[3].Replace("Size：", "");
                            var date = array[5].Replace("Created：", "");
                            var children = elemnet.Children;
                            var magnetUrl = urlElement.Href.Replace("about:///hash/", "").Replace(".html", "").Trim();
                        }
                    }
                }
                //Console.WriteLine(content);
            }
            Console.WriteLine("111111111111");
        }

        public static async Task SearchByBTMOVAsync(int page, string search)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/119.0.0.0 Safari/537.36 Edg/119.0.0.0");

            var url = $"https://www.btmovi.fund/so/{search}_time_{page}.html";
            var response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine(content);
            }
            Console.WriteLine("111111111111");
        }

        public static async Task SearchByZhongisouAsync(int page, string search)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/119.0.0.0 Safari/537.36 Edg/119.0.0.0");

            var url = $"https://m.zhongziso365.xyz/list/{search}/{page}";
            var response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                
                var parser = new HtmlParser();
                var content = await response.Content.ReadAsStringAsync();
                var document = parser.ParseDocument(content);
                var elemnets = document.All.Where(p => p.ClassName == "list-group");
                foreach ( var elemnet in elemnets )
                {                  
                    var array = elemnet.TextContent.Replace("\t", "").Replace(" ","").Split("\n").Where(p => p.Length > 0).Select(p=>p.Trim()).ToList();
        
                    if (array.Count >= 6 && elemnet.FirstElementChild?.Children.FirstOrDefault(p => p is IHtmlAnchorElement) is IHtmlAnchorElement urlElement)
                    {
                        var title = array[0];
                        var size = array[3];
                        var date = array[5];
                        var magnetUrl = urlElement.Href.Replace("about:///info-", "");
                    }
                }
                //Console.WriteLine(content);
            }
            Console.WriteLine("111111111111");
        }



        private static string GetUrlTime()
        {
            var dataStr = DateTime.Now.ToString("yyyyMMddHH") + "123";
            using var md5 = MD5.Create();
            var data = md5.ComputeHash(Encoding.UTF8.GetBytes(dataStr));

            return BitConverter.ToString(data).Replace("-", string.Empty).Substring(0, 11).ToLower();
        }
    }
}