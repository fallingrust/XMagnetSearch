using System.Net.NetworkInformation;
using System.Net;

namespace XMagnetSearch
{
    public abstract class SearchBase
    {
        public const string UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/119.0.0.0 Safari/537.36 Edg/119.0.0.0";
        public const int Timeout = 10000;
        private HttpClient? _client;

        protected HttpClient GetClient()
        {
            if (_client == null)
            {
                _client = new HttpClient()
                {
                    Timeout = TimeSpan.FromMilliseconds(Timeout),
                };
                _client.DefaultRequestHeaders.Add("User-Agent", UserAgent);
            }

            return _client;
        }
        public abstract Task<IEnumerable<SearchBean>> SearchAsync(string search, int page);

        public static async Task<long> CheckEnableAsync(string host)
        {
            try
            {
                var ips = Dns.GetHostAddresses(host);
                var ip = ips?.FirstOrDefault(p => p.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
                if (ip == null) return long.MaxValue;
                using var ping = new Ping();
                var reply = await ping.SendPingAsync(ip, TimeSpan.FromMilliseconds(5000));
                Console.WriteLine($"ping host:{host} {reply.Status} {reply.RoundtripTime}");
                return reply.Status == IPStatus.Success ? reply.RoundtripTime : long.MaxValue;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return long.MaxValue;
        }
    }
}
