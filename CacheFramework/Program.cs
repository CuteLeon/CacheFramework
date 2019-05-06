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
            // 注册 CacheModel 类型的缓存，使用 TFunc 约束生产者的方法委托，传入生产者方法的实际参数
            CacheCollectionFactory.RegistCache<CacheModel, Func<int, CacheModel[]>>(
                CacheCollectionType.List,
                CacheModelFactory.CreateModels,
                6);

            // 获取 CacheModel 类型的缓存数据集合，消费者不用考虑缓存何时加载
            var cacheCollection = CacheCollectionFactory.GetCache<CacheModel>().GetCaches();

            Console.WriteLine(string.Join("、", cacheCollection.Select(model => model.Name)));
            Console.Read();
        }
    }
}
