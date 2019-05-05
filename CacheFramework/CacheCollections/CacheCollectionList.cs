using System.Collections.Generic;

namespace CacheFramework.CacheCollections
{
    public class CacheCollectionList<T> : List<T>, ICacheCollection<T>
    {
        public IEnumerable<T> GetCaches()
            => this;
    }
}
