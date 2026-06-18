using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;

[MemoryPackable(0)]
public class DictionarySave<T1, T2> : IMemoryPackable<DictionarySave<T1, T2>>, IMemoryPackFormatterRegister where T2 : class
{
	[MemoryPackConstructor]
	public DictionarySave()
	{
	}

	public virtual T2 this[T1 _v]
	{
		get
		{
			if (!DictionarySave<T1, T2>.KeyIsValuetype && _v == null)
			{
				return default(T2);
			}
			T2 t;
			if (this.dic.TryGetValue(_v, out t))
			{
				return t;
			}
			return default(T2);
		}
		set
		{
			this.dic[_v] = value;
		}
	}

	public Dictionary<T1, T2> Dict
	{
		get
		{
			return this.dic;
		}
	}

	public bool ContainsKey(T1 _key)
	{
		return this.dic.ContainsKey(_key);
	}

	public bool TryGetValue(T1 _key, out T2 _value)
	{
		return this.dic.TryGetValue(_key, out _value);
	}

	public void Add(T1 _key, T2 _value)
	{
		this.dic.Add(_key, _value);
	}

	public void Remove(T1 _key)
	{
		this.dic.Remove(_key);
	}

	public void Clear()
	{
		this.dic.Clear();
	}

	public void MarkToRemove(T1 _v)
	{
		this.toRemove.Add(_v);
	}

	public void RemoveAllMarked(DictionarySave<T1, T2>.DictionaryRemoveCallback _callback)
	{
		foreach (T1 t in this.toRemove)
		{
			_callback(t);
		}
		this.toRemove.Clear();
	}

	public int Count
	{
		get
		{
			return this.dic.Count;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	static DictionarySave()
	{
		DictionarySave<T1, T2>.RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<DictionarySave<T1, T2>>())
		{
			MemoryPackFormatterProvider.Register<DictionarySave<T1, T2>>(new DictionarySave<T1, T2>.DictionarySaveFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<DictionarySave<T1, T2>[]>())
		{
			MemoryPackFormatterProvider.Register<DictionarySave<T1, T2>[]>(new ArrayFormatter<DictionarySave<T1, T2>>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<Dictionary<T1, T2>>())
		{
			MemoryPackFormatterProvider.Register<Dictionary<T1, T2>>(new DictionaryFormatter<T1, T2>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, [Nullable(new byte[] { 2, 1, 1 })] ref DictionarySave<T1, T2> value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(3);
		writer.WriteValue<Dictionary<T1, T2>>(ref value.dic);
		Dictionary<T1, T2> dict = value.Dict;
		writer.WriteValue<Dictionary<T1, T2>>(ref dict);
		int count = value.Count;
		writer.WriteUnmanaged<int>(ref count);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, [Nullable(new byte[] { 2, 1, 1 })] ref DictionarySave<T1, T2> value)
	{
		byte b;
		if (!reader.TryReadObjectHeader(ref b))
		{
			value = null;
			return;
		}
		Dictionary<T1, T2> dictionary;
		if (b == 3)
		{
			Dictionary<T1, T2> dictionary2;
			int num;
			if (value == null)
			{
				dictionary = reader.ReadValue<Dictionary<T1, T2>>();
				dictionary2 = reader.ReadValue<Dictionary<T1, T2>>();
				reader.ReadUnmanaged<int>(ref num);
				goto IL_00D1;
			}
			dictionary = value.dic;
			dictionary2 = value.Dict;
			num = value.Count;
			reader.ReadValue<Dictionary<T1, T2>>(ref dictionary);
			reader.ReadValue<Dictionary<T1, T2>>(ref dictionary2);
			reader.ReadUnmanaged<int>(ref num);
		}
		else
		{
			if (b > 3)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(DictionarySave<T1, T2>), 3, b);
				return;
			}
			Dictionary<T1, T2> dictionary2;
			int num;
			if (value == null)
			{
				dictionary = null;
				dictionary2 = null;
				num = 0;
			}
			else
			{
				dictionary = value.dic;
				dictionary2 = value.Dict;
				num = value.Count;
			}
			if (b != 0)
			{
				reader.ReadValue<Dictionary<T1, T2>>(ref dictionary);
				if (b != 1)
				{
					reader.ReadValue<Dictionary<T1, T2>>(ref dictionary2);
					if (b != 2)
					{
						reader.ReadUnmanaged<int>(ref num);
					}
				}
			}
			if (value == null)
			{
				goto IL_00D1;
			}
		}
		value.dic = dictionary;
		return;
		IL_00D1:
		value = new DictionarySave<T1, T2>
		{
			dic = dictionary
		};
	}

	[MemoryPackInclude]
	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<T1, T2> dic = new Dictionary<T1, T2>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<T1> toRemove = new List<T1>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly bool KeyIsValuetype = typeof(T1).IsValueType;

	public delegate void DictionaryRemoveCallback(T1 _o);

	[NullableContext(1)]
	[Nullable(new byte[] { 0, 1, 1, 1 })]
	[Preserve]
	[PublicizedFrom(EAccessModifier.Private)]
	public sealed class DictionarySaveFormatter : MemoryPackFormatter<DictionarySave<T1, T2>>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref DictionarySave<T1, T2> value)
		{
			DictionarySave<T1, T2>.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref DictionarySave<T1, T2> value)
		{
			DictionarySave<T1, T2>.Deserialize(ref reader, ref value);
		}
	}
}
