using System;
using System.Collections.Generic;
using System.Linq;

namespace CacheFramework.CacheCollections
{
    public enum CacheCollectionType
    {
        List = 0,
        HashSet = 1,
    }

    public static class CacheCollectionFactory
    {
        internal static readonly Dictionary<Type, Lazy<ICacheCollection>> cacheContainer = new Dictionary<Type, Lazy<ICacheCollection>>();

        internal static TCollection CreateCacheCollection<TCollection>()
            => Activator.CreateInstance<TCollection>();

        internal static bool CheckCacheExists<TModel>()
            => cacheContainer.ContainsKey(typeof(TModel));

        internal static void SeedChecheCollection<TModel>(ICacheCollection collection, CacheCollectionType collectionType, IEnumerable<TModel> models)
        {
            switch (collectionType)
            {
                case CacheCollectionType.List:
                    {
                        if (!(collection is CacheCollectionList<TModel> cacheCollection))
                        {
                            throw new ArgumentException("缓存集合类型错误");
                        }

                        cacheCollection.AddRange(models);
                        break;
                    }
                case CacheCollectionType.HashSet:
                    {
                        if (!(collection is CacheCollectionHashSet<TModel> cacheCollection))
                        {
                            throw new ArgumentException("缓存集合类型错误");
                        }

                        using (IEnumerator<TModel> enumerator = models.GetEnumerator())
                        {
                            while (enumerator.MoveNext())
                            {
                                cacheCollection.Add(enumerator.Current);
                            }
                        }
                        break;
                    }
                default:
                    throw new ArgumentException($"无效的缓存集合类型：{collectionType.ToString()}");
            }
        }

        public static void RegistCache<TModel, TFunc>(CacheCollectionType collectionType, TFunc func, params object[] args)
            where TFunc : Delegate
        {
            Type modelType = typeof(TModel);
            bool exist = CheckCacheExists<TModel>();
            if (exist)
            {
                throw new InvalidOperationException($"已经存在类型 {modelType.FullName} 的缓存集合，无法重复注册");
            }

            var methodParams = func.Method.GetParameters();
            int minParamCount = methodParams.Count(p => !p.IsOptional);
            int maxParamCount = methodParams.Length;
            if (args.Length < minParamCount || args.Length > maxParamCount)
            {
                throw new ArgumentException($"注册懒加载缓存类型 {modelType.Name} 使用的委托形式参数列表和实际参数列表数量不匹配");
            }

            // func.Method.ReturnType

            var cache = new Lazy<ICacheCollection>(() =>
            {
                ICacheCollection cacheCollection = null;
                try
                {
                    switch (collectionType)
                    {
                        case CacheCollectionType.List:
                            {
                                cacheCollection = CreateCacheCollection<CacheCollectionList<TModel>>();
                                break;
                            }
                        case CacheCollectionType.HashSet:
                            {
                                cacheCollection = CreateCacheCollection<CacheCollectionHashSet<TModel>>();
                                break;
                            }
                        default:
                            throw new ArgumentException($"无效的缓存集合类型：{collectionType.ToString()}");
                    }
                }
                catch
                {
                    throw;
                }

                if (cacheCollection == null)
                {
                    throw new InvalidOperationException($"无法创建类型 {modelType.FullName} 的缓存集合");
                }

                IEnumerable<TModel> models = null;
                try
                {
                    models = func.DynamicInvoke(args) as IEnumerable<TModel>;
                }
                catch
                {
                    throw;
                }

                SeedChecheCollection(cacheCollection, collectionType, models);

                return cacheCollection;
            });

            cacheContainer.Add(modelType, cache);
        }

        public static ICacheCollection<TModel> GetCache<TModel>()
        {
            Type modelType = typeof(TModel);
            bool exist = CheckCacheExists<TModel>();
            if (!exist)
            {
                throw new InvalidOperationException($"不存在类型 {modelType.FullName} 的缓存集合");
            }

            ICacheCollection<TModel> cacheCollection = cacheContainer[modelType].Value as ICacheCollection<TModel>;
            return cacheCollection ?? throw new InvalidOperationException();
        }
    }
}
