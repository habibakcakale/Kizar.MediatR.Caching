namespace Kizar.MediatR.Caching {
    using System;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using global::MediatR;
    using Microsoft.Extensions.Caching.Memory;

    public class InvalidateCacheBehavior<T, TK> : IPipelineBehavior<T, TK> {
        private readonly CacheKeyTypeStore store;
        private readonly IMemoryCache memoryCache;

        public InvalidateCacheBehavior(CacheKeyTypeStore store, IMemoryCache memoryCache) {
            this.store = store;
            this.memoryCache = memoryCache;
        }

        public Task<TK> Handle(T request, CancellationToken cancellationToken, RequestHandlerDelegate<TK> next) {
            if (next == null) throw new ArgumentNullException(nameof(next));
            var attribute = typeof(T).GetCustomAttribute<InvalidateAttribute>();
            if (attribute?.Types == null) return next();
            foreach (var type in attribute.Types) {
                if (store.Remove(type.FullName, out var keys)) {
                    foreach (var key in keys) {
                        memoryCache.Remove(key);
                    }
                }
            }
            return next();
        }
    }
}
