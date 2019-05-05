using System;
using System.Linq;

using CacheFramework.CacheCollections;
using CacheFramework.Models;

namespace CacheFramework
{
    class Program
    {
        static void Main(string[] args)
        {
            CacheCollectionFactory.RegistCache<CacheModel, Func<int, CacheModel[]>>(
                CacheCollectionType.List,
                CacheModelFactory.CreateModels,
                6);

            var cacheCollection = CacheCollectionFactory.GetCache<CacheModel>().GetCaches();

            Console.WriteLine(string.Join("、", cacheCollection.Select(model => model.Name)));
            Console.Read();
        }
    }
}
