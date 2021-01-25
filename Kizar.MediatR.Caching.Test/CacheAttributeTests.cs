namespace Kizar.MediatR.Caching.Test {
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using global::MediatR;
    using Microsoft.Extensions.DependencyInjection;
    using Xunit;

    public class CacheAttributeTests {
        [Fact]
        public async void Test1() {
            IServiceCollection services = new ServiceCollection();
            services.AddMediatR(typeof(CacheAttributeTests).Assembly);
            services.AddMemoryCache();
            services.AddSingleton<CacheKeyTypeStore>();
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(CacheBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(InvalidateCacheBehavior<,>));
            var provider = services.BuildServiceProvider();
            var mediator = provider.GetRequiredService<IMediator>();
            var response = await mediator.Send(new CacheRequest {StartDate = DateTime.Now});
            var response2 = await mediator.Send(new CacheRequest {StartDate = DateTime.Now});
            Assert.Equal(response.Result, response2.Result);
        }

        [Cacheable(KeyProps = new[] {"StartDate.Date"})]
        public record CacheRequest : IRequest<CacheResponse> {
            public DateTime StartDate { get; init; }
        }

        public record CacheResponse(DateTime Result);

        public class CacheHandler : IRequestHandler<CacheRequest, CacheResponse> {
            public Task<CacheResponse> Handle(CacheRequest request, CancellationToken cancellationToken) {
                return Task.FromResult(new CacheResponse(DateTime.Now));
            }
        }
    }
}
