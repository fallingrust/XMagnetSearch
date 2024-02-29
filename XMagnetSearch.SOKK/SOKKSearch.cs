using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using System.ComponentModel.Composition;

namespace XMagnetSearch.SOKK
{
    [Export(typeof(SearchBase))]
    [SearchMetadata("11h.sokk24.buzz", "吃力网", "1.0.0")]
    public class SOKKSearch : SearchBase
    {
        public override async Task<IEnumerable<SearchBean>> SearchAsync(string search, int page)
        {
            var results = new List<SearchBean>();
            var client = GetClient();
            var url = $"https://11h.sokk24.buzz/search/{search}/page-{page}.html";
            try
            {
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
                                var titleIndex = array[1].IndexOf("hot") > 0 ? array[1].IndexOf("hot") : array[1].IndexOf("热度");
                                var title = array[1][..titleIndex];
                                var size = array[3].Replace("文件大小：", "");
                                var date = array[5].Replace("创建时间：", "");
                                var magnetUrl = urlElement.Href.Replace("about:///hash/", "").Replace(".html", "").Trim();
                                results.Add(new(title, magnetUrl, size, "吃力网", date));
                            }
                        }
                    }
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
            }
           
            return results;
        }
    }
}
