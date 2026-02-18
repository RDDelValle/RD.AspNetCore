using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using RD.AspNetCore.Mediation;

namespace RD.AspNetCore.Mediation.Tests;

public sealed class EndpointRouteBuilderExtensionsTests
{
    [Fact]
    public void UseMediationFromAssembly_maps_endpoints_and_uses_di()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddSingleton<EndpointCounter>();

        using var app = builder.Build();

        app.UseMediationFromAssembly(typeof(EndpointRouteBuilderExtensionsTests).Assembly);

        var counter = app.Services.GetRequiredService<EndpointCounter>();
        Assert.Equal(1, counter.Count);
    }

    [Fact]
    public void UseMediationFromAssembly_throws_for_null_assembly()
    {
        var builder = WebApplication.CreateBuilder();
        using var app = builder.Build();

        var exception = Assert.Throws<ArgumentNullException>(() => app.UseMediationFromAssembly(null!));

        Assert.Equal("assembly", exception.ParamName);
    }

    private sealed class EndpointCounter
    {
        public int Count { get; set; }
    }

    private sealed class TestEndpoint : IEndpoint
    {
        private readonly EndpointCounter counter;

        public TestEndpoint(EndpointCounter counter)
        {
            this.counter = counter;
        }

        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            counter.Count++;
        }
    }
}
