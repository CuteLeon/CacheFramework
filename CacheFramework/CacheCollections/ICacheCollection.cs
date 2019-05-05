using System.Collections;
using System.Collections.Generic;

namespace CacheFramework.CacheCollections
{
    public interface ICacheCollection
    {
    }

    public interface ICacheCollection<T> : ICacheCollection
    {
        IEnumerable<T> GetCaches();
    }
}
