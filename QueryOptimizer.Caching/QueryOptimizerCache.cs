using QueryOptimizer.Shared.Common.Models.Caching;
using System;
using System.Collections.Generic;
using System.Text;

namespace QueryOptimizer.Caching
{
    public static class QueryOptimizerCache
    {
        private static readonly CacheManager CacheManager = new CacheManager();

        public static MetricsCash Get(Guid sessionId)
        {
            var data = CacheManager.Get<MetricsCash>(sessionId);
            return data;
        }

        public static void AddToCache(MetricsCash cacheItem)
        {
            CacheManager.Add(cacheItem);
        }
    }
}
