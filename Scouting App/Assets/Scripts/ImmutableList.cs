using System;
using System.Collections.Generic;

public class ImmutableList<T> : List<T>
{
	public bool IsReadOnly => true;

	public ImmutableList(List<T> list)
	{
		foreach (T o in list)
			base.Add(o);
	}

	public new T this[int index]
	{
		get
		{
			return base[index];
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

	public new bool Remove(T item)
	{
		throw new NotImplementedException();
	}
}
