namespace Kizar.MediatR.Caching {
    using System;
    using System.Collections;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using global::MediatR;
    using Microsoft.Extensions.Caching.Memory;

    public class CacheBehavior<T, TK> : IPipelineBehavior<T, TK> {
        private readonly CacheKeyTypeStore keyStore;
        private readonly IMemoryCache memoryCache;

        public CacheBehavior(IMemoryCache memoryCache, CacheKeyTypeStore keyStore) {
            this.keyStore = keyStore;
            this.memoryCache = memoryCache;
        }

        public Task<TK> Handle(T request, CancellationToken cancellationToken, RequestHandlerDelegate<TK> next) {
            if (next == null) throw new ArgumentNullException(nameof(next));
            var type = typeof(T);
            var attr = type.GetCustomAttribute<CacheableAttribute>();
            if (attr == null) return next();

            var cacheKey = CacheKey(request, attr, type);
            return memoryCache.GetOrCreateAsync(cacheKey, (entry) => {
                entry.AbsoluteExpiration = attr.AbsoluteExpiration;
                entry.AbsoluteExpirationRelativeToNow = attr.AbsoluteExpirationRelativeToNow;
                entry.SlidingExpiration = attr.SlidingExpiration;
                entry.PostEvictionCallbacks.Add(new PostEvictionCallbackRegistration {
                    State = request,
                    EvictionCallback = EvictionCallback
                });
                keyStore.Add(type.FullName, cacheKey);
                return next();
            });
        }

        private static string CacheKey(T request, CacheableAttribute attr, Type type) {
            var props = attr.KeyProps != null
                ? type.GetProperties().Where(item => attr.KeyProps.Contains(item.Name))
                : Array.Empty<PropertyInfo>();
            var cacheKey = string.Concat(type.FullName, string.Join("-", props.Select(prop => ConvertToString(prop.GetValue(request)))));
            return cacheKey;
        }

        private static string ConvertToString(object value) {
            if (value is IEnumerable enumerable) {
                return string.Join("-", enumerable.Cast<object>());
            }

            return value?.ToString();
        }

        private void EvictionCallback(object key, object value, EvictionReason reason, object state) {
            if (reason == EvictionReason.Replaced) {
                return;
            }

            var type = typeof(T);
            keyStore.RemoveKey(type.FullName, key);
        }
    }
}
