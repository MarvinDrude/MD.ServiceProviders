
namespace MD.ServiceProviders.Container;

/// <summary>
/// Represents a thread safe collection of different implementations of the same kind of service.<br/>
/// <b>Example:</b>
/// Image preview generators, you will need different implementations for video / document / image
/// </summary>
public interface IServiceContainer<TService, TDecideParams>
    where TService : class, IServiceImplementation
    where TDecideParams : class {

    public Task<TService?> RetrieveService(TDecideParams parameters);

}
