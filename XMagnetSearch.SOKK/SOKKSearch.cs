using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using System.ComponentModel.Composition;

namespace XMagnetSearch.SOKK
{
    [Export(typeof(ISearch))]
    [SearchMetadata("11h.sokk24.buzz", "吃力网", "1.0.0")]
    public class SOKKSearch : ISearch
    {
        public async Task<IEnumerable<SearchBean>> SearchAsync(string search, int page)
        {
            var results = new List<SearchBean>();
            using var client = new HttpClient();
            client.Timeout = TimeSpan.FromMilliseconds(ISearch.Timeout);
            client.DefaultRequestHeaders.Add("User-Agent", ISearch.UserAgent);
            var url = $"https://11h.sokk24.buzz/search/{search}/page-{page}.html";
            var response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var parser = new HtmlParser();
                var content = await response.Content.ReadAsStringAsync();
                var document = parser.ParseDocument(content);
                var elemnets = document.All.Where(p => p.ClassName == "item");
                foreach (var elemnet in elemnets)
                {
                    if (!elemnet.TextContent.Contains("在线播放"))
                    {
                        var array = elemnet.TextContent.Split(" ");
                        if (array.Length > 5 && elemnet.FirstElementChild != null && elemnet.FirstElementChild.FirstElementChild is IHtmlAnchorElement urlElement)
                        {
                            var title = array[1][..array[1].IndexOf("Hot")];
                            var size = array[3].Replace("Size：", "");
                            var date = array[5].Replace("Created：", "");                            
                            var magnetUrl = urlElement.Href.Replace("about:///hash/", "").Replace(".html", "").Trim();
                            results.Add(new(title, magnetUrl, size, "吃力网", date));
                        }
                    }
                }
            }
            return results;
        }
    }
}
