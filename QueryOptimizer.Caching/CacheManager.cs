using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using QueryOptimizer.Shared.Common.Models.Caching;
using System;
using System.Collections.Generic;
using System.Text;

namespace QueryOptimizer.Caching
{
    public class CacheManager : IDisposable
    {
        private readonly MemoryCache _cache = new MemoryCache(Options.Create(new MemoryCacheOptions()));

        public void Add(MetricsCash cacheItem)
        { 
            _cache.Set(cacheItem.SessionId, cacheItem, new DateTimeOffset(DateTime.Now.AddMinutes(15)));
        }

        public T Get<T>(Guid sessionId) where T : class
        {
            T data = _cache.Get(sessionId) as T;
            return data;
        }

        public void ClearByKey(Guid sessionId)
        {
            _cache.Remove(sessionId);
        }

        #region IDisposable
        private bool _disposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                _cache.Dispose();
            }

            _disposed = true;
        }

        ~CacheManager()
        {
            Dispose(false);
        }
        #endregion
    }
}
