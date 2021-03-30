namespace Kizar.MediatR.Caching {
    using System;
    using System.Collections;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using global::MediatR;
    using Microsoft.Extensions.Caching.Memory;

    public class CacheBehavior<T, TK> : IPipelineBehavior<T, TK> {
        private const string CacheKeyDelimiter = "-";
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

        private string CacheKey(T request, CacheableAttribute attr, Type type) {
            if (attr.KeyProps == null || !attr.KeyProps.Any()) {
                return type.FullName;
            }

            var propKeys = string.Join(CacheKeyDelimiter, attr.KeyProps.Select(propertyName => {
                var value = GetAccessor(propertyName).DynamicInvoke(request);
                return ConvertToString(value);
            }));
            return string.Concat(type.FullName, CacheKeyDelimiter, propKeys);
        }

        private static string ConvertToString(object value) {
            return value switch {
                string stringValue => stringValue,
                IEnumerable enumerable => string.Join(CacheKeyDelimiter, enumerable.Cast<object>()),
                _ => value?.ToString()
            };
        }

        private void EvictionCallback(object key, object value, EvictionReason reason, object state) {
            if (reason == EvictionReason.Replaced) {
                return;
            }

            var type = typeof(T);
            keyStore.RemoveKey(type.FullName, key);
        }

        private Delegate GetAccessor(string propertyName) {
            return memoryCache.GetOrCreate(string.Concat(typeof(T).FullName, propertyName), _ => {
                var type = typeof(T);
                var parameter = Expression.Parameter(type, "req");
                var condition = Expression.NotEqual(parameter, Expression.Constant(null, typeof(T)));
                var (propertyAccess, nullCheckAccess) = propertyName.Split('.')
                    .Aggregate<string, (Expression parameter, Expression condition )>((parameter, condition), (prev, key) => {
                        var next = Expression.PropertyOrField(prev.parameter, key);
                        var nextCondition = Expression.AndAlso(prev.condition, Expression.NotEqual(next, Expression.Default(next.Type)));
                        return (next, nextCondition);
                    });
                var access = Expression.Lambda(Expression.Condition(nullCheckAccess, propertyAccess, Expression.Default(propertyAccess.Type)), parameter);
                return access.Compile();
            });
        }
    }
}
