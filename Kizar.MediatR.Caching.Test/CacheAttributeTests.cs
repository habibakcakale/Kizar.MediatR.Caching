namespace Kizar.MediatR.Caching.Test {
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using global::MediatR;
    using Microsoft.Extensions.DependencyInjection;
    using Xunit;

    public class CacheAttributeTests : IClassFixture<TestProviderSource> {
        public CacheAttributeTests(TestProviderSource providerSource) {
            this.ServiceProvider = providerSource.ServiceProvider;
        }

        public IServiceProvider ServiceProvider { get; set; }

        [Fact]
        public async void SameData_Should_Return_Same_Result() {
            var mediator = ServiceProvider.GetRequiredService<IMediator>();
            var response = await mediator.Send(new CacheWithPropRequest {StartDate = DateTime.Now});
            var response2 = await mediator.Send(new CacheWithPropRequest {StartDate = DateTime.Now});
            Assert.Equal(response.Result, response2.Result);
        }

        [Fact]
        public async void DifferentData_Should_Return_Different_Result() {
            var mediator = ServiceProvider.GetRequiredService<IMediator>();
            var response = await mediator.Send(new CacheWithPropRequest {StartDate = DateTime.Now});
            var response2 = await mediator.Send(new CacheWithPropRequest {StartDate = DateTime.Now.AddDays(10)});
            Assert.NotEqual(response.Result, response2.Result);
        }

        [Fact]
        public async void DifferentArrayData_Should_Return_Different_Result() {
            var mediator = ServiceProvider.GetRequiredService<IMediator>();
            var response = await mediator.Send(new CacheWithArrayProp {Params = new[] {"1", "2"}});
            var response2 = await mediator.Send(new CacheWithArrayProp {Params = new[] {"1", "1"}});
            Assert.NotEqual(response.Result, response2.Result);
        }

        [Fact]
        public async void RequestWithoutKeyProps() {
            var mediator = ServiceProvider.GetRequiredService<IMediator>();
            var response = await mediator.Send(new CacheRequest());
            var response2 = await mediator.Send(new CacheRequest());
            Assert.Equal(response.Result, response2.Result);
        }

        [Fact]
        public async void NullPropertyReference_Should_Return_Response() {
            var mediator = ServiceProvider.GetRequiredService<IMediator>();
            var response = await mediator.Send(new CacheWithComplexGraph(null));
            var response2 = await mediator.Send(new CacheWithComplexGraph(null));
            Assert.Equal(response.Result, response2.Result);
        }

        [Fact]
        public async void NotNullPropertyReference_Should_Return_Same_Response() {
            var mediator = ServiceProvider.GetRequiredService<IMediator>();
            var response = await mediator.Send(new CacheWithComplexGraph(new User("Habib", Array.Empty<Role>())));
            var response2 = await mediator.Send(new CacheWithComplexGraph(new User("Habib", Array.Empty<Role>())));
            Assert.Equal(response.Result, response2.Result);
        }

        public record User (string Name, Role[] Roles);

        public record Role(int Id, string Name);

        [Cacheable(KeyProps = new[] {"User.Name"})]
        public record CacheWithComplexGraph(User User) : IRequest<CacheResponse>;

        [Cacheable]
        public record CacheRequest() : IRequest<CacheResponse>;

        [Cacheable(KeyProps = new[] {"StartDate.Date"})]
        public record CacheWithPropRequest : IRequest<CacheResponse> {
            public DateTime StartDate { get; init; }
        }

        [Cacheable(KeyProps = new[] {nameof(Params)})]
        public record CacheWithArrayProp : IRequest<CacheResponse> {
            public string[] Params { get; set; }
        }

        public record CacheResponse(DateTime Result);

        public class CacheHandler :
            IRequestHandler<CacheWithPropRequest, CacheResponse>,
            IRequestHandler<CacheRequest, CacheResponse>,
            IRequestHandler<CacheWithArrayProp, CacheResponse>,
            IRequestHandler<CacheWithComplexGraph, CacheResponse> {
            public Task<CacheResponse> Handle(CacheRequest request, CancellationToken cancellationToken) => Task.FromResult(new CacheResponse(DateTime.Now));
            public Task<CacheResponse> Handle(CacheWithPropRequest request, CancellationToken cancellationToken) => Task.FromResult(new CacheResponse(DateTime.Now));
            public Task<CacheResponse> Handle(CacheWithArrayProp request, CancellationToken cancellationToken) => Task.FromResult(new CacheResponse(DateTime.Now));
            public Task<CacheResponse> Handle(CacheWithComplexGraph request, CancellationToken cancellationToken) => Task.FromResult(new CacheResponse(DateTime.Now));
        }
    }
}
