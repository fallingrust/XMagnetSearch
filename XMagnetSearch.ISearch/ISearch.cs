

namespace XMagnetSearch
{
    public interface ISearch
    {
        public const string UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/119.0.0.0 Safari/537.36 Edg/119.0.0.0";
        Task<IEnumerable<SearchBean>> SearchAsync(string search, int page);
    }
}
