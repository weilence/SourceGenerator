using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SourceGenerator.Library.Models;

public class ValueArray<T> : IEnumerable<T>, IEqualityComparer<ValueArray<T>>
{
    private readonly List<T> _array;

    public ValueArray()
    {
        _array = [];
    }

    public ValueArray(IEnumerable<T> array)
    {
        _array = array.ToList();
    }

    public T this[int index]
    {
        get => _array[index];
        set => _array[index] = value;
    }

    public IEnumerator<T> GetEnumerator()
    {
        return _array.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Add(T item)
    {
        _array.Add(item);
    }

    public int Count => _array.Count;

    public bool Equals(ValueArray<T> x, ValueArray<T> y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (ReferenceEquals(x, null)) return false;
        if (ReferenceEquals(y, null)) return false;
        if (x.GetType() != y.GetType()) return false;
        return x._array.SequenceEqual(y._array);
    }

    public int GetHashCode(ValueArray<T> obj)
    {
        return obj._array.GetHashCode();
    }

    public static implicit operator ValueArray<T>(T[] array)
    {
        return new ValueArray<T>(array);
    }

    public static implicit operator ValueArray<T>(List<T> list)
    {
        return new ValueArray<T>(list);
    }
}