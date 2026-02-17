using Microsoft.Extensions.DependencyInjection;
namespace RD.AspNetCore.Mediation;

/// <summary>
/// Default implementation of the <see cref="IMediator"/> interface.
/// </summary>
public class Mediator(IServiceProvider serviceProvider) : IMediator
{
    private readonly IServiceProvider serviceProvider =
        serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

    /// <summary>
    /// Executes the specified query to its corresponding handler and returns the result.
    /// </summary>
    /// <typeparam name="TResult">The type of the result returned by the query handler.</typeparam>
    /// <param name="query">The query to be handled.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The result of the query.</returns>
    public async Task<TResult> ExecuteQueryAsync<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default)
    {
        if (query is null)
        {
            throw new ArgumentNullException(nameof(query));
        }

        var handlerType = typeof(IQueryHandler<,>).MakeGenericType(query.GetType(), typeof(TResult));
        var handler = serviceProvider.GetService(handlerType)
                      ?? throw new MediationException($"No handler found for query '{query.GetType().Name}' with response '{typeof(TResult).Name}'.");
        var method = handlerType.GetMethod("HandleAsync")!;
        return await (Task<TResult>)method.Invoke(handler, [query, cancellationToken])!;
    }

    /// <summary>
    /// Executes the specified command to its corresponding handler.
    /// </summary>
    /// <typeparam name="TCommand">The type of the command to be handled.</typeparam>
    /// <param name="command">The command to be handled.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task ExecuteAsync<TCommand>(TCommand command, CancellationToken cancellationToken = default) where TCommand : ICommand
    {
        if (command is null)
        {
            throw new ArgumentNullException(nameof(command));
        }

        var handler = serviceProvider.GetService<ICommandHandler<TCommand>>()
                      ?? throw new MediationException($"No handler found for command '{typeof(TCommand).Name}'.");
        await handler.HandleAsync(command, cancellationToken);
    }

    /// <summary>
    /// Executes the specified command to its corresponding handler and returns the result.
    /// </summary>
    /// <typeparam name="TResult">The type of the result returned by the command handler.</typeparam>
    /// <param name="command">The command to be handled.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The result of the command.</returns>
    public async Task<TResult> ExecuteAsync<TResult>(ICommand<TResult> command, CancellationToken cancellationToken = default)
    {
        if (command is null)
        {
            throw new ArgumentNullException(nameof(command));
        }

        var handlerType = typeof(ICommandHandler<,>).MakeGenericType(command.GetType(), typeof(TResult));
        var handler = serviceProvider.GetService(handlerType) 
                      ?? throw new MediationException($"No handler found for command '{command.GetType().Name}' with response '{typeof(TResult).Name}'.");

        // Invoke HandleAsync via reflection because we don't have the concrete generic type TCommand at compile time here
        var method = handlerType.GetMethod("HandleAsync")!;
        return await (Task<TResult>)method.Invoke(handler, [command, cancellationToken])!;
    }
}
