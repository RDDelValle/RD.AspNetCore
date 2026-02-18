using Microsoft.Extensions.DependencyInjection;
using RD.AspNetCore.Mediation;

namespace RD.AspNetCore.Mediation.Tests;

public sealed class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddMediationFromAssembly_registers_mediator_and_handlers()
    {
        var services = new ServiceCollection();

        services.AddMediationFromAssembly(typeof(ServiceCollectionExtensionsTests).Assembly);

        using var provider = services.BuildServiceProvider();

        var mediator = provider.GetService<IMediator>();
        var handler = provider.GetService<ICommandHandler<Ping>>();

        Assert.NotNull(mediator);
        Assert.IsType<Mediator>(mediator);
        Assert.IsType<PingHandler>(handler);
    }

    private sealed record Ping : ICommand;

    private sealed class PingHandler : ICommandHandler<Ping>
    {
        public Task HandleAsync(Ping command, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
