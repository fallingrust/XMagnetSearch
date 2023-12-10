using System.Net.NetworkInformation;
using System.Net;

namespace XMagnetSearch
{
    public interface ISearch
    {
        public const string UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/119.0.0.0 Safari/537.36 Edg/119.0.0.0";
        public const int Timeout = 10000;        
        Task<IEnumerable<SearchBean>> SearchAsync(string search, int page);

        public static async Task<bool> CheckEnableAsync(string host)
        {
            try
            {
                var ips = Dns.GetHostAddresses(host);
                if (ips == null || ips.Length <= 0) return false;
                using var ping = new Ping();
                var reply = await ping.SendPingAsync(ips[0], TimeSpan.FromMilliseconds(3000));
                return reply.Status == IPStatus.Success;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return false;
        }
    }
}
