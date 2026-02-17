using System.Reflection;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace RD.AspNetCore.Mediation;

public static class EndpointRouteBuilderExtensions
{
    extension(IEndpointRouteBuilder app)
    {
        public IEndpointRouteBuilder UseMediationFromAssembly(Assembly assembly)
        {
            if (assembly is null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }

            var types = GetLoadableTypes(assembly)
                .Where(IsEndpointType);

            foreach (var type in types)
            {
                if (CreateEndpoint(app, type) is { } endpoint)
                {
                    endpoint.MapEndpoint(app);
                }
            }
            return app;
        }
    }

    private static bool IsEndpointType(Type type)
        => typeof(IEndpoint).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract;

    private static IEndpoint? CreateEndpoint(IEndpointRouteBuilder app, Type type)
    {
        return ActivatorUtilities.CreateInstance(app.ServiceProvider, type) as IEndpoint;
    }

    private static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            return ex.Types.Where(t => t is not null)!;
        }
    }
}
