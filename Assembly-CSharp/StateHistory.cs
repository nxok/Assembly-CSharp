using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

public sealed class StateHistory<T> : IEnumerable<T>, IEnumerable
{
	public StateHistory(int max, bool preventConsecutiveDuplicates = false, bool preventDuplicates = false)
	{
		this.m_states = new T[max];
		this.m_statesSet = (preventDuplicates ? new HashSet<T>() : null);
		this.m_preventConsecutiveDuplicates = preventConsecutiveDuplicates;
		this.m_preventDuplicates = preventDuplicates;
		this.m_index = -1;
		this.m_size = 0;
	}

	public T Current
	{
		get
		{
			if (this.m_size <= 0)
			{
				return default(T);
			}
			return this.m_states[this.m_index];
		}
	}

	public int Length
	{
		get
		{
			return this.m_size;
		}
	}

	public void Add(T state)
	{
		if (this.m_preventDuplicates && this.m_statesSet.Contains(state))
		{
			return;
		}
		if (this.m_preventConsecutiveDuplicates && this.m_size > 0 && EqualityComparer<T>.Default.Equals(this.m_states[this.m_index], state))
		{
			return;
		}
		if (this.m_size < this.m_states.Length)
		{
			this.m_size++;
		}
		this.m_index++;
		if (this.m_index >= this.m_size)
		{
			this.m_index = 0;
		}
		HashSet<T> statesSet = this.m_statesSet;
		if (statesSet != null)
		{
			statesSet.Remove(this.m_states[this.m_index]);
		}
		this.m_states[this.m_index] = state;
		HashSet<T> statesSet2 = this.m_statesSet;
		if (statesSet2 == null)
		{
			return;
		}
		statesSet2.Add(state);
	}

	public void Clear()
	{
		for (int i = 0; i < this.m_states.Length; i++)
		{
			this.m_states[i] = default(T);
		}
		HashSet<T> statesSet = this.m_statesSet;
		if (statesSet != null)
		{
			statesSet.Clear();
		}
		this.m_index = -1;
		this.m_size = 0;
	}

	public IEnumerator<T> GetEnumerator()
	{
		int num2;
		for (int i = 0; i < this.m_size; i = num2 + 1)
		{
			int num = this.m_index - i;
			if (num < 0)
			{
				num += this.m_size;
			}
			yield return this.m_states[num];
			num2 = i;
		}
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator System.Collections.IEnumerable.GetEnumerator()
	{
		return this.GetEnumerator();
	}

	public override string ToString()
	{
		if (this.m_size <= 0)
		{
			return "No State History";
		}
		StringBuilder stringBuilder = new StringBuilder();
		foreach (T t in this)
		{
			if (stringBuilder.Length > 0)
			{
				stringBuilder.Append(" < ");
			}
			stringBuilder.Append(t);
		}
		return stringBuilder.ToString();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly T[] m_states;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly HashSet<T> m_statesSet;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly bool m_preventConsecutiveDuplicates;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly bool m_preventDuplicates;

	[PublicizedFrom(EAccessModifier.Private)]
	public int m_index;

	[PublicizedFrom(EAccessModifier.Private)]
	public int m_size;
}
