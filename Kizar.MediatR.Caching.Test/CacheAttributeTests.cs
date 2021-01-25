namespace Kizar.MediatR.Caching.Test {
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using global::MediatR;
    using Microsoft.Extensions.DependencyInjection;
    using Xunit;

    public class CacheAttributeTests {
        [Fact]
        public async void RequestWithKeyProps() {
            var provider = BuildProvider();
            var mediator = provider.GetRequiredService<IMediator>();
            var response = await mediator.Send(new CacheWithPropRequest {StartDate = DateTime.Now});
            var response2 = await mediator.Send(new CacheWithPropRequest {StartDate = DateTime.Now});
            Assert.Equal(response.Result, response2.Result);
        }

        [Fact]
        public async void RequestWithoutKeyProps() {
            var provider = BuildProvider();
            var mediator = provider.GetRequiredService<IMediator>();
            var response = await mediator.Send(new CacheRequest());
            var response2 = await mediator.Send(new CacheRequest());
            Assert.Equal(response.Result, response2.Result);
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

        [Cacheable]
        public record CacheRequest() : IRequest<CacheResponse>;

        [Cacheable(KeyProps = new[] {"StartDate.Date"})]
        public record CacheWithPropRequest : IRequest<CacheResponse> {
            public DateTime StartDate { get; init; }
        }

        public record CacheResponse(DateTime Result);

        public class CacheHandler : IRequestHandler<CacheWithPropRequest, CacheResponse>, IRequestHandler<CacheRequest, CacheResponse> {
            public Task<CacheResponse> Handle(CacheRequest request, CancellationToken cancellationToken) {
                return Task.FromResult(new CacheResponse(DateTime.Now));
            }

            public Task<CacheResponse> Handle(CacheWithPropRequest request, CancellationToken cancellationToken) {
                return Task.FromResult(new CacheResponse(DateTime.Now));
            }
        }
    }
}
