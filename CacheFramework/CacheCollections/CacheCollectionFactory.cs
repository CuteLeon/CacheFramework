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
        /// <summary>
        /// 缓存容器
        /// </summary>
        internal static readonly Dictionary<Type, Lazy<ICacheCollection>> cacheContainer = new Dictionary<Type, Lazy<ICacheCollection>>();

        /// <summary>
        /// 创建缓存容器
        /// </summary>
        /// <typeparam name="TCollection"></typeparam>
        /// <returns></returns>
        internal static TCollection CreateCacheCollection<TCollection>()
            => Activator.CreateInstance<TCollection>();

        /// <summary>
        /// 检查是否存在指定类型的懒加载缓存
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <returns></returns>
        internal static bool CheckCacheExists<TModel>()
            => cacheContainer.ContainsKey(typeof(TModel));

        /// <summary>
        /// 填充懒加载缓存到容器
        /// </summary>
        /// <typeparam name="TModel">缓存类型</typeparam>
        /// <param name="collection">缓存数据集合</param>
        /// <param name="collectionType">缓存数据集合</param>
        /// <param name="models">填充缓存数据</param>
        internal static void SeedChecheCollection<TModel>(ICacheCollection collection, CacheCollectionType collectionType, IEnumerable<TModel> models)
        {
            switch (collectionType)
            {
                case CacheCollectionType.List:
                    {
                        // 填充 List
                        if (!(collection is CacheCollectionList<TModel> cacheCollection))
                        {
                            throw new ArgumentException("缓存集合类型错误");
                        }

                        cacheCollection.AddRange(models);
                        break;
                    }
                case CacheCollectionType.HashSet:
                    {
                        // 使用迭代器填充 HashSet
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

        /// <summary>
        /// 注册缓存
        /// </summary>
        /// <typeparam name="TModel">缓存类型</typeparam>
        /// <typeparam name="TFunc">获取缓存数据委托类型</typeparam>
        /// <param name="collectionType">缓存数据集合类型</param>
        /// <param name="func">获取缓存数据委托</param>
        /// <param name="args">获取缓存数据委托实际参数</param>
        public static void RegistCache<TModel, TFunc>(CacheCollectionType collectionType, TFunc func, params object[] args)
            where TFunc : Delegate
        {
            Type modelType = typeof(TModel);
            bool exist = CheckCacheExists<TModel>();
            if (exist)
            {
                throw new InvalidOperationException($"已经存在类型 {modelType.FullName} 的缓存集合，无法重复注册");
            }

            // 验证委托和实参的参数数量是否匹配
            var methodParams = func.Method.GetParameters();
            int minParamCount = methodParams.Count(p => !p.IsOptional);
            int maxParamCount = methodParams.Length;
            if (args.Length < minParamCount || args.Length > maxParamCount)
            {
                throw new ArgumentException($"注册懒加载缓存类型 {modelType.Name} 使用的委托形式参数列表和实际参数列表数量不匹配");
            }

            // 验证委托返回类型
            // func.Method.ReturnType

            // 声明懒加载类，当首次获取缓存时将执行构造函数内的匿名方法以装载缓存数据
            var cache = new Lazy<ICacheCollection>(() =>
            {
                ICacheCollection cacheCollection = null;
                // 创建懒加载缓存数据集合
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

                // 装载缓存数据
                IEnumerable<TModel> models = null;
                try
                {
                    // 动态调用获取缓存数据委托
                    models = func.DynamicInvoke(args) as IEnumerable<TModel>;
                }
                catch
                {
                    throw;
                }

                // 填充缓存数据集合
                SeedChecheCollection(cacheCollection, collectionType, models);

                return cacheCollection;
            });

            // 注册懒加载缓存
            cacheContainer.Add(modelType, cache);
        }

        /// <summary>
        /// 获取指定类型的缓存数据
        /// </summary>
        /// <typeparam name="TModel">缓存数据类型</typeparam>
        /// <returns></returns>
        public static ICacheCollection<TModel> GetCache<TModel>()
        {
            Type modelType = typeof(TModel);
            bool exist = CheckCacheExists<TModel>();
            if (!exist)
            {
                throw new InvalidOperationException($"不存在类型 {modelType.FullName} 的缓存集合");
            }

            // 调用 Lazy<>.Value 将会自动装载缓存数据
            ICacheCollection<TModel> cacheCollection = cacheContainer[modelType].Value as ICacheCollection<TModel>;
            return cacheCollection ?? throw new InvalidOperationException();
        }
    }
}
