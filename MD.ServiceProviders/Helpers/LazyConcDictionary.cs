
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace MD.ServiceProviders.Helpers;


/// <summary>
/// Usage of lazy and concurrent dictionary for having true thread safe dictionary
/// GetOrAdd is guaranteed to only instantiate one instance by this implementation.
/// This is important if the value your trying to add involves instantiation of IDisposable's for example.
/// As the default GetOrAdd of ConccurentDictionary could run the factory func multiple times and discarding
/// a IDisposable object without ever calling its Dispose method leading to memory leaks and other hurtful stuff.
/// </summary>
public sealed class LazyConcDictionary<TKey, TValue>
    where TKey : notnull {

    /// <summary>
    /// Internal concurrent dictionary to be used for key value storage with Lazy
    /// </summary>
    private readonly ConcurrentDictionary<TKey, Lazy<TValue>> Cache = new();

    /// <summary>
    /// Generally not recommended to be used with this dictionary, as it has not
    /// the same safety mechanisms as GetOrAdd/TryGetValue have. 
    /// </summary>
    /// <param name="key">The dictionary key</param>
    /// <returns>The given value for the key</returns>
    public TValue? this[TKey key] {
        get {
            var val = Cache[key];
            return val == default ? default : val.Value;
        }
        set {
            if (value == null) return;
            Cache[key] = new Lazy<TValue>(() => value, LazyThreadSafetyMode.ExecutionAndPublication);
        }
    }

    /// <summary>
    /// Main method to be used to add new key value pairs to the dictionary.
    /// </summary>
    /// <param name="key">The key of the value.</param>
    /// <param name="factory">Factory is not run multiple times.</param>
    /// <returns>Either the added or existing value.</returns>
    public TValue GetOrAdd(TKey key, Func<TKey, TValue> factory) {

        // factory method is delayed to be run until Lazy.Value is accessed, meaning any discarded creations of lazy
        // will never run the actual factory method making this perfect for IDisposable creation in factory
        return Cache.GetOrAdd(key, (k) => new Lazy<TValue>(() => factory(k), LazyThreadSafetyMode.ExecutionAndPublication)).Value;

    }

    /// <summary>
    /// Main method to be used to update a value in the dictionary
    /// </summary>
    /// <param name="key">The key of the to add/change value.</param>
    /// <param name="addFactory">Factory is not run multiple times.</param>
    /// <param name="updateFactory">Factory is not run multiple times.</param>
    /// <returns>Either the added or updated value of the corresponding key.</returns>
    public TValue AddOrUpdate(TKey key, Func<TKey, TValue> addFactory, Func<TKey, TValue, TValue> updateFactory) {

        return Cache.AddOrUpdate(key,
            (k) => new Lazy<TValue>(() => addFactory(k), LazyThreadSafetyMode.ExecutionAndPublication),
            (k, currVal) => new Lazy<TValue>(() => updateFactory(k, currVal.Value), LazyThreadSafetyMode.ExecutionAndPublication)).Value;

    }

    /// <summary>
    /// Attemps to get the value associated with the specified key from <see cref="LazyConcDictionary{TKey, TValue}"/>
    /// </summary>
    /// <param name="key">The key of the value to get</param>
    /// <param name="value">Contains the value from the dictionary that has the specific key, if <see langword="false"/> is returned contains default value.</param>
    /// <returns><see langword="true"/> if key was found, otherwise <see langword="false"/></returns>
    public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value) {

        if (Cache.TryGetValue(key, out var lazy)) {

            value = lazy.Value;
            return true;

        }

        value = default;
        return false;

    }

    public bool TryGetLazyValue(TKey key, [MaybeNullWhen(false)] out Lazy<TValue> value) {

        return Cache.TryGetValue(key, out value);

    }

    /// <summary>
    /// Attempts to remove and return the value associated with the given key in <see cref="LazyConcDictionary{TKey, TValue}"/>
    /// </summary>
    /// <param name="key">The key to remove</param>
    /// <param name="value">The value that was removed</param>
    /// <returns><see langword="true"/> if key was found and removed, otherwise <see langword="false"/></returns>
    public bool TryRemove(TKey key, [MaybeNullWhen(false)] out TValue value) {

        if (Cache.TryRemove(key, out var lazy)) {

            value = lazy.Value;
            return true;

        }

        value = default;
        return false;

    }

    public bool TryRemove(KeyValuePair<TKey, Lazy<TValue>> kv) {

        return Cache.TryRemove(kv);

    }

    /// <summary>
    /// Gets all the keys present in <see cref="LazyConcDictionary{TKey, TValue}"/>.
    /// While iterating the keys, newly added keys may or may not be included in concurrent context.
    /// </summary>
    /// <returns>A collection of keys</returns>
    public IEnumerable<TKey> GetKeys() {

        foreach (var key in Cache.Keys) {

            yield return key;

        }

    }

    /// <summary>
    /// Gets all the key value pairs present in <see cref="LazyConcDictionary{TKey, TValue}"/>.
    /// While iterating the key value pairs, newly added pairs may or may not be included in concurrent context.
    /// </summary>
    /// <returns>A collection of key value pairs</returns>
    public IEnumerable<KeyValuePair<TKey, TValue>> GetKeyValues() {

        foreach (var (key, val) in Cache) {

            yield return new KeyValuePair<TKey, TValue>(key, val.Value);

        }

    }

}
