using AngleSharp.Html.Parser;
using System.ComponentModel.Composition;
using System.Security.Cryptography;
using System.Text;

namespace XMagnetSearch.EUSJ
{
    [Export(typeof(SearchBase))]
    [SearchMetadata("eusjdkws.lol", "czechsearch", "1.0.0")]
    public class EUSJSearch : SearchBase
    {
        public override async Task<IEnumerable<SearchBean>> SearchAsync(string search, int page)
        {
            var results = new List<SearchBean>();
            var client = GetClient();
            var data = MD5.HashData(Encoding.UTF8.GetBytes($"{search}999"));
            var param = BitConverter.ToString(data).Replace("-", string.Empty)[..7].ToLower();
            var url = $"https://{GetUrlTime()}.eusjdkws.lol/list.php?q={search}&m=&f=_all&s=&p={page}&tk={param}";
            var response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var parser = new HtmlParser();
                var content = await response.Content.ReadAsStringAsync();
                var document = parser.ParseDocument(content);
                var elemnets = document.All.Where(p => p.ClassName == "list-group-item");
                foreach (var elemnet in elemnets)
                {
                    if (elemnet.TextContent != "result")
                    {
                        var array = elemnet.TextContent.Split("\n").Select(p => p.Trim()).ToArray();
                        if (array.Length >= 2 && array[1].Length > 40)
                        {
                            var title = array[0];
                            var magnetUrl = array[1].Substring(array[1].Length - 40, 40);
                            results.Add(new(title, magnetUrl, "", "czechsearch", ""));
                        }
                    }
                }                
            }
            return results;
        }
        private static string GetUrlTime()
        {
            var dataStr = DateTime.Now.ToString("yyyyMMddHH") + "123";
            var data = MD5.HashData(Encoding.UTF8.GetBytes(dataStr));
            return BitConverter.ToString(data).Replace("-", string.Empty).Substring(0, 11).ToLower();
        }
    }
}
