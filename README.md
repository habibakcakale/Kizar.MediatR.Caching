# Kizar.MediatR.Caching
MediatR pipeline caching, with attribute based convention, on the top of the dotnet In Memory Caching

# How To  
In your start up or place where you configure your dependencies register below services.
```c#
    public class Startup {
        public void ConfigureServices(IServiceCollection services) {
            services.AddSingleton<CacheKeyTypeStore>();
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(CacheBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(InvalidateCacheBehavior<,>));
        }
        .....
    }
```
And use to cache ```Account List``` and invalidates cache of ```Filtered Account``` List etc..
```c#
    [Cacheable(ExpirationType.SlidingExpiration, CacheableAttribute.ForHour, KeyProps = new[] {nameof(CustomerId)})]
    [Invalidate(typeof(FilteredAccountListRequest))]
    public class GetAccountListRequest : IRequest<List<Account>> {
        public string CustomerId { get; set; }
    }
```
