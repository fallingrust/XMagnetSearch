using System.ComponentModel.Composition;
using System.Diagnostics;

namespace XMagnetSearch.EUSJ
{
    [Export(typeof(ISearch))]
    [SearchMetadata("eusjdkws.lol", "", "1.0.0")]
    public class EUSJSearch : ISearch
    {        
        Task<IEnumerable<SearchBean>> ISearch.SearchAsync(string search, int page)
        {
            Debug.WriteLine("eusjdkws.lol");
            return Task.FromResult(Enumerable.Empty<SearchBean>());
        }
    }
}
