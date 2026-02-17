namespace RD.AspNetCore.Mediation;

/// <summary>
/// Represents a command that can be executed. and return no value. (Task)
/// </summary>
public interface ICommand
{
}

/// <summary>
/// Represents a command that can be executed and returns a result of type <typeparamref name="TResult"/>.
/// </summary>
/// <typeparam name="TResult">The type of the result returned by the command.</typeparam
public interface ICommand<out TResult>
{
}
