namespace RD.AspNetCore.Mediation;

/// <summary>
/// Represents a handler for a specific command.
/// </summary>
public interface ICommandHandler<in TCommand>
    where TCommand : ICommand
{
    /// <summary>
    /// Handles the specified command asynchronously.
    /// </summary>
    /// <param name="command">The command to handle.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task HandleAsync(TCommand command, CancellationToken cancellationToken);
}

/// <summary>
/// Represents a handler for a specific command that returns a result.
/// </summary>
public interface ICommandHandler<in TCommand, TResult>
    where TCommand : ICommand<TResult>
{
    /// <summary>
    /// Handles the specified command asynchronously and returns a result.
    /// </summary>
    /// <param name="command">The command to handle.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation, containing the result.</returns>
    Task<TResult> HandleAsync(TCommand command, CancellationToken cancellationToken);
}