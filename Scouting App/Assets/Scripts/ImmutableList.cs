using System;
using System.Collections.Generic;

public class ImmutableList<T> : List<T>
{
	private List<T> _List;
	public new int Count => _List.Count;

	public bool IsReadOnly => true;

	public ImmutableList(List<T> list)
	{
		_List = list;
	}

	public new T this[int index]
	{
		get
		{
			return _List[index];
		}
	}

	public new void Add(T item)
	{
		throw new NotImplementedException();
	}

	public new void Clear()
	{
		throw new NotImplementedException();
	}

	public new bool Contains(T item)
	{
		return _List.Contains(item);
	}

	public new void CopyTo(T[] array, int arrayIndex)
	{
		_List.CopyTo(array, arrayIndex);
	}

	public new IEnumerator<T> GetEnumerator()
	{
		return _List.GetEnumerator();
	}

	public new bool Remove(T item)
	{
		throw new NotImplementedException();
	}
}
