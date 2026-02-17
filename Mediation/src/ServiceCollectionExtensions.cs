using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace RD.AspNetCore.Mediation;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public void AddMediationFromAssembly(Assembly assembly)
        {
            if (assembly is null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }

            services.TryAddTransient<IMediator, Mediator>();
            
            var types = GetLoadableTypes(assembly)
                .Where(t => !t.IsAbstract && !t.IsInterface)
                .Select(type => new TypeInfo(type, type.GetInterfaces()))
                .Where(item => IsMediationServiceType(item.Interfaces));

            services.AddTypes(types);
        }
        
        private void AddTypes(IEnumerable<TypeInfo> types)
        {
            foreach (var item in types)
            {
                foreach (var interfaceType in item.Interfaces)
                {
                    if (IsHandlerType(interfaceType))
                    {
                        services.AddTransient(interfaceType, item.Type);
                    } 
                    else if (IsType<IScopedService>(interfaceType))
                    {
                        services.AddScoped(interfaceType, item.Type);
                    }
                    else if (IsType<ITransientService>(interfaceType))
                    {
                        services.AddTransient(interfaceType, item.Type);
                    }
                    else if (IsType<ISingletonService>(interfaceType))
                    {
                        services.AddSingleton(interfaceType, item.Type);
                    }  
                }
            }
        }
    }

    private static bool IsType<T>(Type type)
    {
        var target = typeof(T);
        if (type == target)
        {
            return true;
        }

        return type.IsGenericType && type.GetGenericTypeDefinition() == target;
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
    
    private static bool IsHandlerType(Type type)
    {
        if (!type.IsGenericType)
            return false;

        var typeDefinition = type.GetGenericTypeDefinition();

        return typeDefinition == typeof(ICommandHandler<>) ||
               typeDefinition == typeof(ICommandHandler<,>) ||
               typeDefinition == typeof(IQueryHandler<,>);
    }

    private static bool IsMediationServiceType(IEnumerable<Type> interfaces)
    {
        foreach (var interfaceType in interfaces)
        {
            if (IsHandlerType(interfaceType) ||
                IsType<IScopedService>(interfaceType) ||
                IsType<ITransientService>(interfaceType) ||
                IsType<ISingletonService>(interfaceType))
            {
                return true;
            }
        }

        return false;
    }

    private sealed record TypeInfo(Type Type, Type[] Interfaces);
}
