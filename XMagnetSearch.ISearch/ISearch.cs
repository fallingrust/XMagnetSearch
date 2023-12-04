

namespace XMagnetSearch
{
    public interface ISearch
    {
        Task<IEnumerable<SearchBean>> SearchAsync(string search, int page);
    }
}
