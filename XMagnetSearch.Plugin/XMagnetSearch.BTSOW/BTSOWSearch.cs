using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using System.ComponentModel.Composition;

namespace XMagnetSearch.BTSOW
{
    [Export(typeof(SearchBase))]
    [SearchMetadata("btsow.motorcycles", "btsow","1.0.0")]
    public class BTSOWSearch : SearchBase
    {
        public override async Task<IEnumerable<SearchBean>> SearchAsync(string search, int page)
        {
            var results = new List<SearchBean>();
            var client = GetClient();
            var url = $"https://btsow.motorcycles/search/{search}/page/{page}";
            try
            {
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
                                    results.Add(new(title, magnetHash, size, "btsow", date));
                                }
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
