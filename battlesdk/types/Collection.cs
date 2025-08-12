using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace battlesdk.types;
public class Collection<T> : IEnumerable<T> where T : IIdentifiable {
    private List<T> _elements = [];
    private Dictionary<string, int> _ids = [];

    /// <summary>
    /// The number of elements in the collection.
    /// </summary>
    public int Count => _elements.Count;


    /// <summary>
    /// Returns the element at the given index.
    /// </summary>
    /// <param name="index">The index of the element</param>
    /// <returns></returns>
    /// <exception cref="RegistryException" />
    public T this[int index] {
        get {
            if (index >= _elements.Count) {
                throw Exceptions.InvalidRegistryIndex(this, index);
            }

            return _elements[index];
        }
    }
    /// <summary>
    /// Returns the element with the given name.
    /// </summary>
    /// <returns></returns>
    public T this[string name] => _elements[_ids[name]];

    /// <summary>
    /// Registers a new element in this collection, and returns its index.
    /// </summary>
    /// <param name="el"></param>
    public int Register (T el) {
        _ids[el.Name] = _elements.Count;
        _elements.Add(el);
        el.SetId(_elements.Count - 1);

        return _elements.Count - 1;
    }

    /// <summary>
    /// Gets the element with the name given.
    /// </summary>
    /// <param name="name">The element's name.</param>
    /// <param name="element">The element.</param>
    public bool TryGetElementByName (
        string name, [NotNullWhen(true)] out T? element
    ) {
        if (_ids.TryGetValue(name, out int index) == false) {
            element = default;
            return false;
        }

        element = _elements[index];
        return true;
    }

    /// <summary>
    /// Gets the element with the id given.
    /// </summary>
    /// <param name="id">The element's id.</param>
    /// <param name="element">The element.</param>
    public bool TryGetElement (int id, [NotNullWhen(true)] out T? element) {
        if (id >= _elements.Count) {
            element = default;
            return false;
        }

        element = _elements[id];
        return true;
    }

    /// <summary>
    /// Gets the id associated to the name given, if it's registered.
    /// </summary>
    /// <param name="name">The name of the element.</param>
    /// <param name="id">The element's id.</param>
    public bool TryGetId (string name, out int id) {
        return _ids.TryGetValue(name, out id);
    }

    /// <summary>
    /// Returns the id associated to the name given. Throws if no such name
    /// exists.
    /// </summary>
    /// <param name="name">The name of the element.</param>
    /// <returns>The element's id.</returns>
    public int GetId (string name) {
        return _ids[name];
    }

    IEnumerator IEnumerable.GetEnumerator () {
        return GetEnumerator();
    }

    public IEnumerator<T> GetEnumerator () {
        return _elements.GetEnumerator();
    }
}

/// <summary>
/// Represents an object that can be identified either by its name, or by its
/// id inside a collection.
/// </summary>
public interface IIdentifiable {
    /// <summary>
    /// The name of this resource.
    /// </summary>
    string Name { get; }
    /// <summary>
    /// The id of this resource on the registry, or -1 if it's not in a registry.
    /// </summary>
    int Id { get; }

    void SetId (int id);
}