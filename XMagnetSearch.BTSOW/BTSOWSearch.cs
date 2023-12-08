using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using System.ComponentModel.Composition;

namespace XMagnetSearch.BTSOW
{
    [Export(typeof(ISearch))]
    [SearchMetadata("https://btsow.motorcycles", "btsow.motorcycles","1.0.0")]
    public class BTSOWSearch : ISearch
    {
       public override async Task<IEnumerable<SearchBean>> SearchAsync(string search, int page)
        {
            var results = new List<SearchBean>();
            using var client = new HttpClient();
            client.Timeout = TimeSpan.FromMilliseconds(ISearch.Timeout);
            client.DefaultRequestHeaders.Add("User-Agent", ISearch.UserAgent);
            var url = $"https://btsow.motorcycles/search/{search}/page/{page}";
            var response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var parser = new HtmlParser();
                var document = parser.ParseDocument(content);
                var elemnets = document.All.Where(p => p.ClassName == "row");
                foreach (var elemnet in elemnets)
                {
                    if (elemnet.Children.Length == 3)
                    {
                        if (elemnet.Children[0] is IHtmlAnchorElement urlElement
                            && elemnet.Children[1] is IHtmlDivElement sizeElement
                            && elemnet.Children[2] is IHtmlDivElement dateElement)
                        {
                            var magnetHash = urlElement.Href.Substring(urlElement.Href.Length - 40, 40);
                            var title = urlElement.Title;
                            var size = sizeElement.TextContent;
                            var date = dateElement.TextContent;
                            if (!string.IsNullOrWhiteSpace(title) && !string.IsNullOrWhiteSpace(magnetHash))
                            {
                                results.Add(new(title, magnetHash, size, "btsow.motorcycles", date));
                            }
                        }
                    }
                }
            }
            return results;
        }
    }
}
