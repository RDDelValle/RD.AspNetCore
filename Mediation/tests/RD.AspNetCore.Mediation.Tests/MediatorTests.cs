using Microsoft.Extensions.DependencyInjection;
using RD.AspNetCore.Mediation;

namespace RD.AspNetCore.Mediation.Tests;

public sealed class MediatorTests
{
    [Fact]
    public async Task ExecuteAsync_for_command_invokes_handler()
    {
        var services = new ServiceCollection();
        services.AddSingleton<PingHandler>();
        services.AddSingleton<ICommandHandler<Ping>>(sp => sp.GetRequiredService<PingHandler>());
        services.AddTransient<IMediator, Mediator>();

        await using var provider = services.BuildServiceProvider();
        var mediator = provider.GetRequiredService<IMediator>();

        await mediator.ExecuteAsync(new Ping());

        var handler = provider.GetRequiredService<ICommandHandler<Ping>>() as PingHandler;
        Assert.NotNull(handler);
        Assert.True(handler!.WasCalled);
    }

    [Fact]
    public async Task ExecuteAsync_for_command_with_result_returns_value()
    {
        var services = new ServiceCollection();
        services.AddTransient<ICommandHandler<Add, int>, AddHandler>();
        services.AddTransient<IMediator, Mediator>();

        await using var provider = services.BuildServiceProvider();
        var mediator = provider.GetRequiredService<IMediator>();

        var result = await mediator.ExecuteAsync<int>(new Add(2, 3));

        Assert.Equal(5, result);
    }

    [Fact]
    public async Task ExecuteQueryAsync_returns_value()
    {
        var services = new ServiceCollection();
        services.AddTransient<IQueryHandler<GetValue, int>, GetValueHandler>();
        services.AddTransient<IMediator, Mediator>();

        await using var provider = services.BuildServiceProvider();
        var mediator = provider.GetRequiredService<IMediator>();

        var result = await mediator.ExecuteQueryAsync(new GetValue(7));

        Assert.Equal(7, result);
    }

    [Fact]
    public async Task ExecuteAsync_for_command_throws_when_handler_missing()
    {
        var services = new ServiceCollection();
        services.AddTransient<IMediator, Mediator>();

        await using var provider = services.BuildServiceProvider();
        var mediator = provider.GetRequiredService<IMediator>();

        var exception = await Assert.ThrowsAsync<MediationException>(() => mediator.ExecuteAsync(new Ping()));

        Assert.Contains("No handler found for command", exception.Message);
    }

    [Fact]
    public async Task ExecuteAsync_for_command_with_result_throws_when_handler_missing()
    {
        var services = new ServiceCollection();
        services.AddTransient<IMediator, Mediator>();

        await using var provider = services.BuildServiceProvider();
        var mediator = provider.GetRequiredService<IMediator>();

        var exception = await Assert.ThrowsAsync<MediationException>(() => mediator.ExecuteAsync<int>(new Add(1, 1)));

        Assert.Contains("No handler found for command", exception.Message);
    }

    [Fact]
    public async Task ExecuteQueryAsync_throws_when_handler_missing()
    {
        var services = new ServiceCollection();
        services.AddTransient<IMediator, Mediator>();

        await using var provider = services.BuildServiceProvider();
        var mediator = provider.GetRequiredService<IMediator>();

        var exception = await Assert.ThrowsAsync<MediationException>(() => mediator.ExecuteQueryAsync(new GetValue(1)));

        Assert.Contains("No handler found for query", exception.Message);
    }

    [Fact]
    public async Task ExecuteAsync_for_command_throws_for_null_command()
    {
        var services = new ServiceCollection();
        services.AddTransient<IMediator, Mediator>();

        await using var provider = services.BuildServiceProvider();
        var mediator = provider.GetRequiredService<IMediator>();

        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => mediator.ExecuteAsync((Ping)null!));

        Assert.Equal("command", exception.ParamName);
    }

    [Fact]
    public async Task ExecuteAsync_for_command_with_result_throws_for_null_command()
    {
        var services = new ServiceCollection();
        services.AddTransient<IMediator, Mediator>();

        await using var provider = services.BuildServiceProvider();
        var mediator = provider.GetRequiredService<IMediator>();

        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => mediator.ExecuteAsync<int>((ICommand<int>)null!));

        Assert.Equal("command", exception.ParamName);
    }

    [Fact]
    public async Task ExecuteQueryAsync_throws_for_null_query()
    {
        var services = new ServiceCollection();
        services.AddTransient<IMediator, Mediator>();

        await using var provider = services.BuildServiceProvider();
        var mediator = provider.GetRequiredService<IMediator>();

        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => mediator.ExecuteQueryAsync<int>((IQuery<int>)null!));

        Assert.Equal("query", exception.ParamName);
    }

    private sealed record Ping : ICommand;

    private sealed class PingHandler : ICommandHandler<Ping>
    {
        public bool WasCalled { get; private set; }

        public Task HandleAsync(Ping command, CancellationToken cancellationToken)
        {
            WasCalled = true;
            return Task.CompletedTask;
        }
    }

    private sealed record Add(int Left, int Right) : ICommand<int>;

    private sealed class AddHandler : ICommandHandler<Add, int>
    {
        public Task<int> HandleAsync(Add command, CancellationToken cancellationToken)
        {
            return Task.FromResult(command.Left + command.Right);
        }
    }

    private sealed record GetValue(int Value) : IQuery<int>;

    private sealed class GetValueHandler : IQueryHandler<GetValue, int>
    {
        public Task<int> HandleAsync(GetValue query, CancellationToken cancellationToken)
        {
            return Task.FromResult(query.Value);
        }
    }
}
