namespace Kizar.MediatR.Caching {
    using System.Collections.Concurrent;
    using System.Collections.Generic;

    public class CacheKeyTypeStore {
        private readonly ConcurrentDictionary<string, IList<object>> store = new();

        public void Add(string typeName, object key) {
            var list = store.GetOrAdd(typeName, value => new List<object>());
            list.Add(key);
        }

        public bool Remove(string typeName, out IList<object> keys) => store.TryRemove(typeName, out keys);

        public void RemoveKey(string typeName, object key) {
            if (store.TryGetValue(typeName, out var keys)) {
                keys.Remove(key);
            }
        }
    }
}
