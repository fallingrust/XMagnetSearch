using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using System.ComponentModel.Composition;

namespace XMagnetSearch.Garden
{
    [Export(typeof(SearchBase))]
    [SearchMetadata("garden.onekuma.cn", "动漫花园", "1.0.0")]
    public class GardenSearch : SearchBase
    {
        public override async Task<IEnumerable<SearchBean>> SearchAsync(string search, int page)
        {
            var results = new List<SearchBean>();
            var client = GetClient();
            var url = $"https://garden.onekuma.cn/resources/{page}?search={search}";
            try
            {
                var response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var parser = new HtmlParser();
                    var content = await response.Content.ReadAsStringAsync();
                    var document = parser.ParseDocument(content);
                    var targetElement = document.All.FirstOrDefault(p => p.ClassName == "resources-table-body divide-y border-b text-sm lt-lg:text-xs");
                    if (targetElement != null && targetElement.ChildElementCount > 0)
                    {
                        foreach (var elemnet in targetElement.Children)
                        {
                            if (elemnet.ChildElementCount == 4)
                            {
                                var date = elemnet.Children[0].TextContent.Trim();
                                var title = elemnet.Children[1].TextContent.Trim();
                                var size = elemnet.Children[3].TextContent.Trim();
                                if (elemnet.Children[3].Children[0].Children[1] is IHtmlAnchorElement anchorElement)
                                {
                                    var magnet = anchorElement.Href;
                                    results.Add(new SearchBean(title, magnet, size, "动漫花园", date));
                                }
                            }
                           
                        }
                    }
                   
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
            }
          
            return results;
        }
    }
}
