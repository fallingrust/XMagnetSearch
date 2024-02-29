using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using System.ComponentModel.Composition;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace XMagnetSearch.FITACG
{
    [Export(typeof(SearchBase))]
    [SearchMetadata("fitacg.com", "菲特动漫", "1.0.0")]
    public class FitAcgSearch : SearchBase
    {
        public override async Task<IEnumerable<SearchBean>> SearchAsync(string search, int page)
        {
            var results = new List<SearchBean>();
            var client = GetClient();
            var url = $"https://fitacg.com/search/{search}?page={page}";
            try
            {
                var response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var parser = new HtmlParser();
                    var document = parser.ParseDocument(content);

                    var elemnets = document.All.Where(p => p is IHtmlScriptElement);
                    foreach (var element in elemnets)
                    {
                        var elementContent = element.TextContent.Trim();
                        if (elementContent.StartsWith("var frontendContext"))
                        {
                            try
                            {
                                elementContent = elementContent.Replace("var frontendContext = ", "")[..^1];
                                var responseBean = JsonSerializer.Deserialize<Response>(elementContent);
                                if (responseBean != null && responseBean.Search != null && responseBean.Search.TopicList != null)
                                {
                                    var tasks = new List<Task<string>>();
                                    foreach (var topic in responseBean.Search.TopicList)
                                    {
                                        tasks.Add(GetMagnetUrl(topic.Id));
                                    }
                                    Task.WaitAll(tasks.ToArray());
                                    for (int i = 0; i < tasks.Count; i++)
                                    {
                                        var magnetUrl = await tasks[i];
                                        var topic = responseBean.Search.TopicList[i];
                                        if (!string.IsNullOrWhiteSpace(magnetUrl))
                                            results.Add(new SearchBean(topic.Title ?? string.Empty, magnetUrl, topic.Size ?? string.Empty, "菲特动漫", topic.DateTime ?? string.Empty));
                                    }
                                }

                                break;
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e);
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
        public  async Task<string> GetMagnetUrl(int topic)
        {
            var client = GetClient();
            var url = $"https://fitacg.com/topic/{topic}";
            try
            {
                var response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var parser = new HtmlParser();
                    var document = parser.ParseDocument(content);

                    var elemnets = document.All.Where(p => !string.IsNullOrWhiteSpace(p.ClassName) && p.ClassName.StartsWith("btn-download-link-2"));
                    foreach (var element in elemnets)
                    {
                        if (element is IHtmlAnchorElement anchorElement && !string.IsNullOrWhiteSpace(anchorElement.Href))
                        {
                            return anchorElement.Href;
                        }
                    }

                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
            }
            
            return string.Empty;
        }
    }


   
    public class TopicItem
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("published_on")]
        public string? DateTime { get; set; }

        [JsonPropertyName("size")]
        public string? Size { get; set; }
    }

    public class Search
    {
        [JsonPropertyName("topic_list")]
        public List<TopicItem>? TopicList { get; set; }
    }

    public class Response
    {
        [JsonPropertyName("search")]
        public Search? Search { get; set; }
    }

}
