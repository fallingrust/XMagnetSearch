using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using System.ComponentModel.Composition;

namespace XMagnetSearch.ZhongZiSo
{
    [Export(typeof(SearchBase))]
    [SearchMetadata("m.zhongziso365.xyz", "种子搜", "1.0.0")]
    public class ZhongzisoSearch : SearchBase
    {
        public override async Task<IEnumerable<SearchBean>> SearchAsync(string search, int page)
        {
            var results = new List<SearchBean>();
            var client = GetClient();
            var url = $"https://m.zhongziso365.xyz/list/{search}/{page}";
            try
            {
                var response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var parser = new HtmlParser();
                    var content = await response.Content.ReadAsStringAsync();
                    var document = parser.ParseDocument(content);
                    var elemnets = document.All.Where(p => p.ClassName == "list-group");
                    foreach (var elemnet in elemnets)
                    {
                        var array = elemnet.TextContent.Replace("\t", "").Replace(" ", "").Split("\n").Where(p => p.Length > 0).Select(p => p.Trim()).ToList();

                        if (array.Count >= 6 && elemnet.FirstElementChild?.Children.FirstOrDefault(p => p is IHtmlAnchorElement) is IHtmlAnchorElement urlElement)
                        {
                            var title = array[0];
                            var size = array[3];
                            var date = array[5];
                            var magnetUrl = urlElement.Href.Replace("about:///info-", "");
                            results.Add(new(title, magnetUrl, size, "种子搜", date));
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
