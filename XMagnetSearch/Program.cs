using System.Data;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;

namespace XMagnetSearch
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
           
           await SearchByZhongisouAsync(1, "测试");
            Console.WriteLine("Hello, World!");
            Console.ReadKey();
        }


        public static async Task SearchByBTSOWAsync(int page,string search)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/119.0.0.0 Safari/537.36 Edg/119.0.0.0");
            var url = $"https://btsow.motorcycles/search/{search}/page/{page}";
            var response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine(content);
            }
            Console.WriteLine("111111111111");
        }


        public static async Task SearchByEUSAsync(int page, string search)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/119.0.0.0 Safari/537.36 Edg/119.0.0.0");
            using var md5 = MD5.Create();
            var data = md5.ComputeHash(Encoding.UTF8.GetBytes($"{search}999"));
            var param = BitConverter.ToString(data).Replace("-", string.Empty).Substring(0, 7).ToLower();
            var url = $"https://{GetUrlTime()}.eusjdkws.lol/list.php?q={search}&m=&f=_all&s=&p={page}&tk={param}";
            var response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine(content);
            }
            Console.WriteLine("111111111111");
        }

        public static async Task SearchBySKKAsync(int page, string search)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/119.0.0.0 Safari/537.36 Edg/119.0.0.0");

            var url = $"https://11h.sokk24.buzz/search/{search}/page-{page}.html";
            var response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine(content);
            }
            Console.WriteLine("111111111111");
        }

        public static async Task SearchByBTMOVAsync(int page, string search)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/119.0.0.0 Safari/537.36 Edg/119.0.0.0");

            var url = $"https://www.btmovi.fund/so/{search}_time_{page}.html";
            var response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine(content);
            }
            Console.WriteLine("111111111111");
        }

        public static async Task SearchByZhongisouAsync(int page, string search)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/119.0.0.0 Safari/537.36 Edg/119.0.0.0");

            var url = $"https://m.zhongziso365.xyz/list/{search}/{page}";
            var response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine(content);
            }
            Console.WriteLine("111111111111");
        }



        private static string GetUrlTime()
        {
            var dataStr = DateTime.Now.ToString("yyyyMMddHH") + "123";
            using var md5 = MD5.Create();
            var data = md5.ComputeHash(Encoding.UTF8.GetBytes(dataStr));

            return BitConverter.ToString(data).Replace("-", string.Empty).Substring(0, 11).ToLower();
        }
    }
}