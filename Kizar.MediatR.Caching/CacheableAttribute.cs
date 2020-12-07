namespace Kizar.MediatR.Caching {
    using System;

    [AttributeUsage(AttributeTargets.Class)]
    public class CacheableAttribute : Attribute {
        public const long ForMinute = 60;
        public const long ForHour = ForMinute * 60;
        public const long ForDay = ForHour * 24;
        public CacheableAttribute() { }

        public CacheableAttribute(ExpirationType type, long seconds) {
            switch (type) {
                case ExpirationType.AbsoluteExpiration:
                    this.AbsoluteExpiration = DateTimeOffset.FromUnixTimeSeconds(seconds);
                    break;
                case ExpirationType.SlidingExpiration:
                    this.SlidingExpiration = TimeSpan.FromSeconds(seconds);
                    break;
                case ExpirationType.AbsoluteExpirationRelativeToNow:
                    this.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(seconds);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        public string[] KeyProps { get; set; }

        /// <summary>
        /// Gets or sets an absolute expiration date for the cache entry.
        /// </summary>
        public DateTimeOffset? AbsoluteExpiration { get; }

        /// <summary>
        /// Gets or sets how long a cache entry can be inactive (e.g. not accessed) before it will be removed.
        /// This will not extend the entry lifetime beyond the absolute expiration (if set).
        /// </summary>
        public TimeSpan? SlidingExpiration { get; }

        public TimeSpan? AbsoluteExpirationRelativeToNow { get; }
    }
}
