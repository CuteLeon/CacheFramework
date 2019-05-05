using System.Linq;

namespace CacheFramework.Models
{
    public static class CacheModelFactory
    {
        public static CacheModel[] CreateModels()
            => Enumerable.Range(0, 10).Select(index => new CacheModel($"Model_{index}")).ToArray();

        public static CacheModel[] CreateModels(int count)
            => Enumerable.Range(0, count).Select(index => new CacheModel($"Model_{index}")).ToArray();

        public static CacheModel[] CreateModels(int start, int count)
            => Enumerable.Range(start, count).Select(index => new CacheModel($"Model_{index}")).ToArray();

        public static CacheModel[] CreateModels(int start, int count, string namePrefix)
            => Enumerable.Range(start, count).Select(index => new CacheModel($"{namePrefix}_{index}")).ToArray();
    }
}
