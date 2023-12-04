using System.ComponentModel.Composition;
using System.Diagnostics;

namespace XMagnetSearch.BTSOW
{
    [Export(typeof(ISearch))]
    [SearchMetadata("btsow.motorcycles","","1.0.0")]
    public class BTSOWSearch : ISearch
    {
        Task<IEnumerable<SearchBean>> ISearch.SearchAsync(string search, int page)
        {
            Debug.WriteLine("btsow.motorcycles");
            return Task.FromResult(Enumerable.Empty<SearchBean>());
        }
    }
}
