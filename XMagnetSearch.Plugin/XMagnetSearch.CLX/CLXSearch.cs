using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using System.ComponentModel.Composition;

namespace XMagnetSearch.CLX
{
    [Export(typeof(SearchBase))]
    [SearchMetadata("cilixiong.com", "磁力熊", "1.0.0")]
    public class CLXSearch : SearchBase
    {
        public override async Task<IEnumerable<SearchBean>> SearchAsync(string search, int page)
        {
            var results = new List<SearchBean>();
            var client = GetClient();
            var formContent = new MultipartFormDataContent
            {
                { new StringContent("1,2"), "classid" },
                { new StringContent("title"), "show" },
                { new StringContent("1"), "tempid" },
                { new StringContent(search), "keyboard" },
                { new StringContent(page.ToString()), "page" }
            };
            var url = $"https://www.cilixiong.com/e/search/index.php";

            try
            {
                var response = await client.PostAsync(url, formContent);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var parser = new HtmlParser();
                    var document = parser.ParseDocument(content);

                    var elemnets = document.All.Where(p => p is IHtmlAnchorElement);
                    var tasks = new List<Task<List<(string, string, string)>>>();
                    foreach (var element in elemnets)
                    {
                        if (element is IHtmlAnchorElement anchorElement
                            && anchorElement.Href.EndsWith("html")
                            && (anchorElement.Href.StartsWith("about:///drama") || anchorElement.Href.StartsWith("about:///movie")))
                        {
                            var detailUrl = $"https://www.cilixiong.com{anchorElement.Href.Replace("about://", "")}";
                            tasks.Add(GetMagnetUrlAsync(detailUrl));
                        }
                    }
                    Task.WaitAll([.. tasks]);
                    foreach (var task in tasks)
                    {
                        var result = await task;
                        foreach (var item in result)
                        {
                            results.Add(new SearchBean(item.Item1, item.Item2, item.Item3, "磁力熊", ""));
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

        private async Task<List<(string, string, string)>> GetMagnetUrlAsync(string url)
        {
            var magnetUrls = new List<(string, string, string)>();
            try
            {
                var client = GetClient();
                var response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var parser = new HtmlParser();
                    var document = parser.ParseDocument(content);
                    var elemnets = document.All.Where(p => p.ClassName == "ms-3 text-muted small");
                    var tasks = new List<Task<(string, string, string)>>();
                    foreach (var element in elemnets)
                    {
                        if (element is IHtmlAnchorElement anchorElement)
                        {
                            var magnetDetailUrl = anchorElement.Href.Replace("about://", "https://www.cilixiong.com");
                            tasks.Add(GetMagnetDetailAsync(magnetDetailUrl));
                        }
                    }
                    Task.WaitAll([.. tasks]);
                    foreach (var task in tasks)
                    {
                        var result = await task;
                        if (!string.IsNullOrWhiteSpace(result.Item1))
                            magnetUrls.Add((result.Item1, result.Item2, result.Item3));
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
           
            return magnetUrls;
        }

        private async Task<(string, string,string)> GetMagnetDetailAsync(string url)
        {
            try
            {
                var client = GetClient();
                var response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var parser = new HtmlParser();
                    var document = parser.ParseDocument(content);
                    var elemnets = document.All.Where(p => p.ClassName == "mb-1").ToList();
                    if (elemnets.Count == 6 
                        && elemnets[0] is IHtmlParagraphElement magnetUrlElement
                        && elemnets[1] is IHtmlParagraphElement nameElement
                        && elemnets[4] is IHtmlParagraphElement sizeMagment)
                    {
                        var name = nameElement.TextContent.Replace("名称：", "");
                        var magnetUrl = magnetUrlElement.TextContent.Replace("磁力链接：", "");
                        var size = sizeMagment.TextContent.Replace("文件大小：", "");
                        return (name, magnetUrl, size);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
           
            return ("","", "");
        }
    }
}
