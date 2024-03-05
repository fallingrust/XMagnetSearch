using AngleSharp.Html.Parser;
using System.ComponentModel.Composition;

namespace XMagnetSearch.BTLM
{
    [Export(typeof(SearchBase))]
    [SearchMetadata("ro.btlm.pro", "BT联盟", "1.0.0")]
    public class BTLMSearch : SearchBase
    {
        public override async Task<IEnumerable<SearchBean>> SearchAsync(string search, int page)
        {
            var results = new List<SearchBean>();
            var client = GetClient();
            client.DefaultRequestHeaders.Referrer = new Uri($"https://cdn.btlm.top:65533/s?word={search}");
            var url = $"https://cdn.btlm.top:65533/s?word={search}&page={page}";
            try
            {
                var response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var parser = new HtmlParser();
                    var document = parser.ParseDocument(content);
                    var elemnets = document.All.Where(p => p.ClassName == "list-unstyled");
                    foreach (var elemnet in elemnets)
                    {
                        var hash = elemnet.Id;
                        if (elemnet.Children != null && elemnet.Children.Length >= 2 && !string.IsNullOrWhiteSpace(hash))
                        {
                            var title = elemnet.Children[0].TextContent;
                            var detail = elemnet.Children[1].TextContent.Trim();
                            var size = detail.Substring(detail.IndexOf("文件大小: ") + 6, detail.IndexOf(" 文件数量:") - detail.IndexOf("文件大小: ") - 6);
                            var time = detail.Substring(detail.IndexOf("收录时间: ") + 6, detail.IndexOf(" 热度:") - detail.IndexOf("收录时间: ") - 6);
                            results.Add(new SearchBean(title, hash, size, "BT联盟", time));
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
