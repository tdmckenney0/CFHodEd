using System.Collections;

namespace HW2MAD;

/// <summary>
/// Event-based generic list that fires events on modifications.
/// </summary>
internal sealed class EventList<T> : IList<T>
{
    private readonly List<T> _list = new();

    public event Action? AddItem;
    public event Action<int>? InsertItem;
    public event Action<int>? ModifiedItem;
    public event Action<int>? PreRemoveItem;
    public event Action<int>? RemoveItem;
    public event Action? PreClearList;
    public event Action? ClearList;

    public void Add(T item)
    {
        _list.Add(item);
        AddItem?.Invoke();
    }

    public void Clear()
    {
        PreClearList?.Invoke();
        _list.Clear();
        ClearList?.Invoke();
    }

    public bool Contains(T item) => _list.Contains(item);

    public void CopyTo(T[] array, int arrayIndex) => _list.CopyTo(array, arrayIndex);

    public int Count => _list.Count;

    public bool IsReadOnly => false;

    public bool Remove(T item)
    {
        int index = _list.IndexOf(item);
        if (index >= 0)
        {
            PreRemoveItem?.Invoke(index);
            bool result = _list.Remove(item);
            if (result)
                RemoveItem?.Invoke(index);
            return result;
        }
        return false;
    }

    public IEnumerator<T> GetEnumerator() => _list.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => _list.GetEnumerator();

    public int IndexOf(T item) => _list.IndexOf(item);

    public void Insert(int index, T item)
    {
        _list.Insert(index, item);
        if (index >= 0 && index < _list.Count)
            InsertItem?.Invoke(index);
    }

    public T this[int index]
    {
        get => _list[index];
        set
        {
            _list[index] = value;
            if (index >= 0 && index < _list.Count)
                ModifiedItem?.Invoke(index);
        }
    }

    public void RemoveAt(int index)
    {
        bool valid = index >= 0 && index < _list.Count;
        if (valid)
            PreRemoveItem?.Invoke(index);
        _list.RemoveAt(index);
        if (valid)
            RemoveItem?.Invoke(index);
    }

    public void Sort()
    {
        _list.Sort();
    }

    public T[] ToArray() => _list.ToArray();
}
