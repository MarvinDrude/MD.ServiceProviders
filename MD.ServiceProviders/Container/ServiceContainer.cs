
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace MD.ServiceProviders.Container;

/// <inheritdoc cref="IServiceContainer" />
public abstract class ServiceContainer<TService, TDecideParams> : IServiceContainer<TService, TDecideParams>
    where TService : class, IServiceImplementation
    where TDecideParams : class {

    private LazyConcDictionary<Type, TService> Services { get; set; } = new ();

    public void SetService<T>(T service)
        where T : class, TService {

        ArgumentNullException.ThrowIfNull(service, nameof(service));
        SetService(() => service);

    }

    public void SetService<T>(Func<T> serviceFactory)
        where T : class, TService {

        ArgumentNullException.ThrowIfNull(serviceFactory, nameof(serviceFactory));
        var type = serviceFactory.Method.ReturnType;

        Services.AddOrUpdate(type,
            addFactory: (k) => serviceFactory(),
            updateFactory: (k, o) => serviceFactory());

    }

    public bool TryGetService<T>([MaybeNullWhen(false)] out T service)
        where T : class, TService {

        if(Services.TryGetValue(typeof(T), out var generic)) {

            service = Unsafe.As<T>(generic);
            return true;

        }

        service = default;
        return false;

    }

    public T GetService<T>()
        where T : class, TService {

        if(Services.TryGetValue(typeof(T), out var generic)) {

            return Unsafe.As<T>(generic);

        }

        throw new ArgumentException($"Service of type {typeof(T)} not found.");

    }

    public abstract Task<TService?> RetrieveService(TDecideParams parameters);

}
