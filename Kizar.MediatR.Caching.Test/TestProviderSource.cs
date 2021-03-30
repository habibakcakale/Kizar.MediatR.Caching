namespace Kizar.MediatR.Caching.Test
{
    using System;
    using global::MediatR;
    using Microsoft.Extensions.DependencyInjection;

    public class TestProviderSource {
        public IServiceProvider ServiceProvider { get; set; }

        public TestProviderSource() {
            this.ServiceProvider = BuildProvider();
        }

        private static IServiceProvider BuildProvider() {
            IServiceCollection services = new ServiceCollection();
            services.AddMediatR(typeof(CacheAttributeTests).Assembly);
            services.AddMemoryCache();
            services.AddSingleton<CacheKeyTypeStore>();
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(CacheBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(InvalidateCacheBehavior<,>));
            var provider = services.BuildServiceProvider();
            return provider;
        }
    }
}
