using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using System.ComponentModel.Composition;

namespace XMagnetSearch.SoFan
{
    [Export(typeof(SearchBase))]
    [SearchMetadata("ae.sefan.cc", "搜番", "1.0.0")]
    public class SoFanSearch : SearchBase
    {
        public override async Task<IEnumerable<SearchBean>> SearchAsync(string search, int page)
        {
            var results = new List<SearchBean>();
            var client = GetClient();
           
            var url = $"https://cdn.sofan.one:65533/s?word={search}&page={page}";
            client.DefaultRequestHeaders.Referrer = new Uri(url);
            try
            {
                var response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var parser = new HtmlParser();
                    var content = await response.Content.ReadAsStringAsync();
                    var document = parser.ParseDocument(content);
                    var elemnets = document.All.Where(p => p.ClassName == "list-unstyled");
                    foreach (var elemnet in elemnets)
                    {
                        if(elemnet.ChildElementCount >= 3)
                        {
                            if (elemnet.Children[0] is IHtmlListItemElement titleElement && elemnet.Children[1] is IHtmlListItemElement detailElement)
                            {
                                var title = titleElement.TextContent;
                                var hash = titleElement.InnerHtml.Substring((titleElement.InnerHtml.IndexOf("/hash/") + 6),40);
                                var details = detailElement.TextContent.Trim().Split(" ");
                                if (details.Length >= 5)
                                {
                                    var size = details[1].Replace("文件大小: ", "") + details[2];
                                    var time = details[3][(details[3].IndexOf("收录时间:") + 5)..];
                                    results.Add(new SearchBean(title, hash, size, "搜番", time));
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return results;
        }
    }
}
