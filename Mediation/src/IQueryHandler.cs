namespace RD.AspNetCore.Mediation;

/// <summary>
/// Represents a handler for a specific query that returns a result.
/// </summary>
/// <typeparam name="TQuery"></typeparam>
/// <typeparam name="TResult"></typeparam>
public interface IQueryHandler<in TQuery, TResult>
    where TQuery : IQuery<TResult>
{
    /// <summary>
    /// Handles the specified query asynchronously and returns a result.
    /// </summary>
    /// <param name="query">The query to handle.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation, containing the result.</returns>
    Task<TResult> HandleAsync(TQuery query, CancellationToken cancellationToken);
}