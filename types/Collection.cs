using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace battlesdk.types;
public class Collection<T> : IEnumerable<T> where T : INameable {
    private List<T> _elements = [];
    private Dictionary<string, int> _indices = [];

    /// <summary>
    /// All the elements in this collection.
    /// </summary>
    public IReadOnlyList<T> Elements => _elements;
    /// <summary>
    /// Maps each element name with its index in the <see cref="Elements"/>
    /// list.
    /// </summary>
    public IReadOnlyDictionary<string, int> Indices => _indices;

    /// <summary>
    /// Returns the element at the given index.
    /// </summary>
    /// <returns></returns>
    public T this[int index] => _elements[index];
    /// <summary>
    /// Returns the element with the given name.
    /// </summary>
    /// <returns></returns>
    public T this[string name] => _elements[_indices[name]];

    /// <summary>
    /// Registers a new element in this collection, and returns its index.
    /// </summary>
    /// <param name="el"></param>
    public int Register (T el) {
        _indices[el.Name] = _elements.Count;
        _elements.Add(el);

        return _elements.Count - 1;
    }

    public bool TryGetElementByName (
        string name, [NotNullWhen(true)] out T? element
    ) {
        if (_indices.TryGetValue(name, out int index) == false) {
            element = default;
            return false;
        }

        element = _elements[index];
        return true;
    }

    IEnumerator IEnumerable.GetEnumerator () {
        return GetEnumerator();
    }

    public IEnumerator<T> GetEnumerator () {
        return _elements.GetEnumerator();
    }
}

public interface INameable {
    string Name { get; }
}