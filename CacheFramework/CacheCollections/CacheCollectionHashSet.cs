using System.Collections.Generic;

namespace CacheFramework.CacheCollections
{
    public class CacheCollectionHashSet<T> : HashSet<T>, ICacheCollection<T>
    {
        public IEnumerable<T> GetCaches()
            => this;
    }
}
