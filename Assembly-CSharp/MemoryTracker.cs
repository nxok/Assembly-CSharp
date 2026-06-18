using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using UniLinq;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

public class MemoryTracker
{
	public static MemoryTracker Instance
	{
		get
		{
			if (MemoryTracker.m_Instance == null)
			{
				MemoryTracker.m_Instance = new MemoryTracker();
			}
			return MemoryTracker.m_Instance;
		}
	}

	public void New(object _o)
	{
		Dictionary<object, int> dictionary = this.refs;
		lock (dictionary)
		{
			Type type = _o.GetType();
			this.refs[type] = (this.refs.ContainsKey(type) ? (this.refs[type] + 1) : 1);
		}
	}

	public void Delete(object _o)
	{
		Dictionary<object, int> dictionary = this.refs;
		lock (dictionary)
		{
			Type type = _o.GetType();
			this.refs[type] = this.refs[type] - 1;
		}
	}

	public void SetEstimationFunction(object _o, MemoryTracker.EstimateOwnedBytes _func)
	{
		if (_o == null)
		{
			return;
		}
		Dictionary<Type, MemoryTracker.AllocationsForType> dictionary = this.allocTypeDict;
		lock (dictionary)
		{
			Type type = _o.GetType();
			MemoryTracker.AllocationsForType allocationsForType;
			if (!this.allocTypeDict.TryGetValue(type, out allocationsForType))
			{
				allocationsForType = new MemoryTracker.AllocationsForType();
				this.allocTypeDict.Add(type, allocationsForType);
			}
			allocationsForType.allocations.AddLast(new MemoryTracker.Allocation(_o, _func));
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int EstimateSelfBytes()
	{
		int num = 0;
		Dictionary<object, int> dictionary = this.refs;
		lock (dictionary)
		{
			num += MemoryTracker.GetUsedSize<object, int>(this.refs);
			num += MemoryTracker.GetUsedSize<object, int>(this.last);
		}
		Dictionary<Type, MemoryTracker.AllocationsForType> dictionary2 = this.allocTypeDict;
		lock (dictionary2)
		{
			num += MemoryTracker.GetUsedSize<Type, MemoryTracker.AllocationsForType>(this.allocTypeDict);
			foreach (MemoryTracker.AllocationsForType allocationsForType in this.allocTypeDict.Values)
			{
				num += allocationsForType.allocations.Count * MemoryTracker.GetSize<MemoryTracker.Allocation>();
			}
		}
		return num;
	}

	public void Dump()
	{
		Dictionary<string, MemoryTracker.AllocationsForType.Summary> dictionary = new Dictionary<string, MemoryTracker.AllocationsForType.Summary>();
		long num = 0L;
		Dictionary<object, int> dictionary2 = this.refs;
		lock (dictionary2)
		{
			Log.Out("---Classes----------------------------------------");
			foreach (KeyValuePair<object, int> keyValuePair in this.refs)
			{
				Log.Out(string.Concat(new string[]
				{
					keyValuePair.Key.ToString(),
					" = ",
					keyValuePair.Value.ToString(),
					" last = ",
					(this.last.ContainsKey(keyValuePair.Key) ? this.last[keyValuePair.Key] : 0).ToString()
				}));
				this.last[keyValuePair.Key] = keyValuePair.Value;
			}
		}
		long totalMemory = GC.GetTotalMemory(false);
		Dictionary<Type, MemoryTracker.AllocationsForType> dictionary3 = this.allocTypeDict;
		lock (dictionary3)
		{
			foreach (KeyValuePair<Type, MemoryTracker.AllocationsForType> keyValuePair2 in this.allocTypeDict)
			{
				Type key = keyValuePair2.Key;
				MemoryTracker.AllocationsForType value = keyValuePair2.Value;
				MemoryTracker.AllocationsForType.Summary summary = default(MemoryTracker.AllocationsForType.Summary);
				summary.numGC = value.ClearDeadAllocations();
				foreach (MemoryTracker.Allocation allocation in value.allocations)
				{
					summary.totalBytes += (long)allocation.GetOwnedBytes();
					summary.numInstances++;
				}
				dictionary.Add(key.ToString(), summary);
				num += summary.totalBytes;
			}
			MemoryTracker.AllocationsForType allocationsForType;
			if (this.allocTypeDict.TryGetValue(typeof(DynamicProperties), out allocationsForType))
			{
				int num2 = 0;
				MemoryTracker.EstimatedMemoryContext estimatedMemoryContext = new MemoryTracker.EstimatedMemoryContext(0);
				using (LinkedList<MemoryTracker.Allocation>.Enumerator enumerator3 = allocationsForType.allocations.GetEnumerator())
				{
					while (enumerator3.MoveNext())
					{
						object obj;
						if (enumerator3.Current.obj.TryGetTarget(out obj))
						{
							num2++;
							MemoryTracker.EstimateMemoryUsage(obj, ref estimatedMemoryContext);
						}
					}
				}
				double num3 = (double)estimatedMemoryContext.ActualBytes * 9.5367431640625E-07;
				double num4 = (double)estimatedMemoryContext.BytesInterned * 9.5367431640625E-07;
				long num5 = 0L;
				long num6 = 0L;
				foreach (KeyValuePair<string, int> keyValuePair3 in estimatedMemoryContext.InteredStrings)
				{
					num5 += (long)MemoryTracker.GetSize(keyValuePair3.Key);
				}
				foreach (KeyValuePair<string, int> keyValuePair4 in estimatedMemoryContext.NonInteredStrings)
				{
					num6 += (long)(MemoryTracker.GetSize(keyValuePair4.Key) * keyValuePair4.Value);
				}
				double num7 = (double)num5 * 9.5367431640625E-07;
				double num8 = (double)num6 * 9.5367431640625E-07;
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.AppendLine("---CSV DynamicProperties---------------------");
				stringBuilder.AppendLine("Type,Count,ActualBytes(MB),BytesInterned(MB),InternedStrings,InternedStrings(MB),NonInternedStrings,NonInternedStrings(MB)");
				stringBuilder.AppendLine(string.Format("DynamicProperties,{0},{1:F2},{2:F2},{3},{4:F2},{5},{6:F2}", new object[]
				{
					num2,
					num3,
					num4,
					estimatedMemoryContext.InteredStrings.Count,
					num7,
					estimatedMemoryContext.NonInteredStrings.Count,
					num8
				}));
				Log.Out(stringBuilder.ToString());
				StringBuilder stringBuilder2 = new StringBuilder();
				stringBuilder2.AppendLine("---CSV InteredStrings---------------------");
				stringBuilder2.AppendLine("InteredStrings,Count");
				foreach (KeyValuePair<string, int> keyValuePair5 in estimatedMemoryContext.InteredStrings.OrderByDescending((KeyValuePair<string, int> x) => x.Value))
				{
					stringBuilder2.AppendLine(string.Format("{0},{1}", keyValuePair5.Key, keyValuePair5.Value));
				}
				Log.Out(stringBuilder2.ToString());
				StringBuilder stringBuilder3 = new StringBuilder();
				stringBuilder3.AppendLine("---CSV NonInteredStrings---------------------");
				stringBuilder3.AppendLine("NonInteredStrings,Count");
				foreach (KeyValuePair<string, int> keyValuePair6 in estimatedMemoryContext.NonInteredStrings.OrderByDescending((KeyValuePair<string, int> x) => x.Value))
				{
					stringBuilder3.AppendLine(string.Format("{0},{1}", keyValuePair6.Key, keyValuePair6.Value));
				}
				Log.Out(stringBuilder3.ToString());
			}
		}
		int num9 = this.EstimateSelfBytes();
		dictionary.Add(typeof(MemoryTracker).ToString(), new MemoryTracker.AllocationsForType.Summary((long)num9));
		num += (long)num9;
		double num10 = (double)(totalMemory - num) * 9.5367431640625E-07;
		Log.Out("GC.GetTotalMemory (MB): {0:F2}", new object[] { (double)totalMemory * 9.5367431640625E-07 });
		Log.Out("Total Tracked (MB): {0:F2}", new object[] { (double)num * 9.5367431640625E-07 });
		Log.Out("Untracked (MB): {0:F2}", new object[] { num10 });
		if (num > 0L)
		{
			StringBuilder stringBuilder4 = new StringBuilder();
			stringBuilder4.AppendLine("---CSV----------------------------------------");
			stringBuilder4.AppendLine("Type,Bytes(MB),Count,GC Count,");
			stringBuilder4.AppendLine(string.Format("Untracked,{0:F2},0,0", num10));
			Log.Out("---Tracked----------------------------------------");
			foreach (KeyValuePair<string, MemoryTracker.AllocationsForType.Summary> keyValuePair7 in dictionary)
			{
				string key2 = keyValuePair7.Key;
				MemoryTracker.AllocationsForType.Summary value2 = keyValuePair7.Value;
				double num11 = (double)value2.totalBytes * 9.5367431640625E-07;
				stringBuilder4.AppendLine(string.Format("{0},{1:F2},{2},{3}", new object[] { key2, num11, value2.numInstances, value2.numGC }));
			}
			Log.Out(stringBuilder4.ToString());
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DrawLabel(float _left, float _top, string _text)
	{
		Rect rect;
		rect..ctor(_left, _top, (float)(_text.Length * 20), 30f);
		Rect rect2;
		rect2..ctor(rect);
		rect2.x += 1f;
		rect2.y += 1f;
		GUI.color = Color.black;
		GUI.Label(rect2, _text);
		GUI.color = Color.white;
		GUI.Label(rect, _text);
	}

	public void DebugOnGui()
	{
		this.DrawLabel(800f, 30f, "Type");
		this.DrawLabel(1000f, 30f, "Count");
		int num = 0;
		Dictionary<object, int> dictionary = this.refs;
		lock (dictionary)
		{
			foreach (KeyValuePair<object, int> keyValuePair in this.refs)
			{
				this.DrawLabel(800f, (float)(80 + num * 35), keyValuePair.Key.ToString());
				this.DrawLabel(1000f, (float)(80 + num * 35), keyValuePair.Value.ToString());
				num++;
			}
		}
	}

	public static int GetSize<T>()
	{
		return MemoryTracker.GetSize(typeof(T));
	}

	public static int GetSize(Type _type)
	{
		if (_type.IsEnum)
		{
			return Marshal.SizeOf(Enum.GetUnderlyingType(_type));
		}
		if (_type.IsValueType)
		{
			return UnsafeUtility.SizeOf(_type);
		}
		return IntPtr.Size;
	}

	public static int GetSize<[IsUnmanaged] T>(IBackedArray<T> _array) where T : struct, ValueType
	{
		int num = IntPtr.Size;
		if (_array != null)
		{
			num += MemoryTracker.GetSize<T>() * _array.Length;
		}
		return num;
	}

	public static int GetSize<T>(T[] _array)
	{
		int num = IntPtr.Size;
		if (_array != null)
		{
			num += MemoryTracker.GetSize<T>() * _array.Length;
		}
		return num;
	}

	public static int GetSize<T>(T[][] _doubleArray)
	{
		int num = IntPtr.Size;
		if (_doubleArray != null)
		{
			foreach (T[] array in _doubleArray)
			{
				num += MemoryTracker.GetSize<T>(array);
			}
		}
		return num;
	}

	public static int GetSize<T>(T[,] _array)
	{
		int num = IntPtr.Size;
		if (_array != null)
		{
			num += MemoryTracker.GetSize<T>() * _array.GetLength(0) * _array.GetLength(1);
		}
		return num;
	}

	public static int GetSize<T>(List<T> _list)
	{
		int num = IntPtr.Size;
		if (_list != null)
		{
			num += _list.Capacity * MemoryTracker.GetSize<T>();
		}
		return num;
	}

	public static int GetUsedSize<TKey, TValue>(IDictionary<TKey, TValue> _dictionary)
	{
		int num = IntPtr.Size;
		if (_dictionary != null)
		{
			num += (MemoryTracker.GetSize<TKey>() + MemoryTracker.GetSize<TValue>()) * _dictionary.Count;
		}
		return num;
	}

	public static int GetSize(string stringVal)
	{
		if (stringVal == null)
		{
			return IntPtr.Size;
		}
		return stringVal.Length * 2 + IntPtr.Size;
	}

	public static int GetSize(Dictionary<string, string> stringDict)
	{
		int num = 0;
		foreach (KeyValuePair<string, string> keyValuePair in stringDict)
		{
			num += MemoryTracker.GetSize(keyValuePair.Key) + MemoryTracker.GetSize(keyValuePair.Value);
		}
		return num;
	}

	public static void EstimateMemoryUsage(object _obj, ref MemoryTracker.EstimatedMemoryContext estimatedMemoryContext)
	{
		if (_obj == null)
		{
			estimatedMemoryContext.ActualBytes += (long)IntPtr.Size;
			return;
		}
		Type type = _obj.GetType();
		if (type.IsEnum)
		{
			estimatedMemoryContext.ActualBytes += (long)Marshal.SizeOf(Enum.GetUnderlyingType(type));
			return;
		}
		if (type.IsValueType)
		{
			estimatedMemoryContext.ActualBytes += (long)UnsafeUtility.SizeOf(type);
			return;
		}
		if (type.IsArray)
		{
			Type elementType = type.GetElementType();
			Array array = _obj as Array;
			int size = IntPtr.Size;
			if (array != null)
			{
				for (int i = 0; i < array.Rank; i++)
				{
					estimatedMemoryContext.ActualBytes += (long)(array.GetLength(i) * MemoryTracker.GetSize(elementType));
				}
			}
			return;
		}
		if (typeof(string).IsAssignableFrom(type))
		{
			string text = (string)_obj;
			estimatedMemoryContext.ActualBytes += (long)IntPtr.Size;
			string text2 = string.IsInterned(text);
			if (text2 != null)
			{
				int num;
				bool flag = estimatedMemoryContext.InteredStrings.TryGetValue(text2, out num);
				estimatedMemoryContext.InteredStrings[text2] = num + 1;
				if (!flag)
				{
					estimatedMemoryContext.ActualBytes += (long)MemoryTracker.GetSize(text);
					estimatedMemoryContext.BytesInterned += (long)MemoryTracker.GetSize(text);
					return;
				}
			}
			else
			{
				int num2;
				bool flag2 = estimatedMemoryContext.NonInteredStrings.TryGetValue(text, out num2);
				estimatedMemoryContext.NonInteredStrings[text] = num2 + 1;
				estimatedMemoryContext.ActualBytes += (long)MemoryTracker.GetSize(text);
				if (!flag2)
				{
					estimatedMemoryContext.BytesInterned += (long)MemoryTracker.GetSize(text);
					return;
				}
			}
		}
		else if (!(_obj is DynamicProperties))
		{
			estimatedMemoryContext.ActualBytes += (long)IntPtr.Size;
			return;
		}
	}

	public static int GetSizeAuto(object _obj)
	{
		if (_obj == null)
		{
			return IntPtr.Size;
		}
		Type type = _obj.GetType();
		if (type.IsEnum)
		{
			return Marshal.SizeOf(Enum.GetUnderlyingType(type));
		}
		if (type.IsValueType)
		{
			return UnsafeUtility.SizeOf(type);
		}
		if (type.IsArray)
		{
			Type elementType = type.GetElementType();
			Array array = _obj as Array;
			int num = IntPtr.Size;
			if (array != null)
			{
				for (int i = 0; i < array.Rank; i++)
				{
					num += array.GetLength(i) * MemoryTracker.GetSize(elementType);
				}
			}
			return num;
		}
		if (typeof(string).IsAssignableFrom(type))
		{
			string text = (string)_obj;
			int num2 = IntPtr.Size;
			if (text != null)
			{
				num2 += MemoryTracker.GetSize(text);
			}
			return num2;
		}
		return IntPtr.Size;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static MemoryTracker m_Instance;

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<object, int> refs = new Dictionary<object, int>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<object, int> last = new Dictionary<object, int>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<Type, MemoryTracker.AllocationsForType> allocTypeDict = new Dictionary<Type, MemoryTracker.AllocationsForType>();

	public struct EstimatedMemoryContext
	{
		public EstimatedMemoryContext(int lame)
		{
			this.InteredStrings = new Dictionary<string, int>();
			this.NonInteredStrings = new Dictionary<string, int>();
			this.ActualBytes = 0L;
			this.BytesInterned = 0L;
		}

		public Dictionary<string, int> InteredStrings;

		public Dictionary<string, int> NonInteredStrings;

		public long ActualBytes;

		public long BytesInterned;
	}

	public delegate int EstimateOwnedBytes(object obj);

	public struct Allocation
	{
		public Allocation(object _obj, MemoryTracker.EstimateOwnedBytes _func)
		{
			this.obj = new WeakReference<object>(_obj);
			this.estimateBytesFunc = _func;
		}

		public int GetOwnedBytes()
		{
			object obj;
			if (this.obj.TryGetTarget(out obj))
			{
				return this.estimateBytesFunc(obj);
			}
			return 0;
		}

		public WeakReference<object> obj;

		public MemoryTracker.EstimateOwnedBytes estimateBytesFunc;
	}

	public class AllocationsForType
	{
		public int ClearDeadAllocations()
		{
			int num = 0;
			LinkedListNode<MemoryTracker.Allocation> next;
			for (LinkedListNode<MemoryTracker.Allocation> linkedListNode = this.allocations.First; linkedListNode != null; linkedListNode = next)
			{
				next = linkedListNode.Next;
				object obj;
				if (!linkedListNode.Value.obj.TryGetTarget(out obj) || obj == null)
				{
					this.allocations.Remove(linkedListNode);
					num++;
				}
			}
			return num;
		}

		public LinkedList<MemoryTracker.Allocation> allocations = new LinkedList<MemoryTracker.Allocation>();

		public struct Summary
		{
			public Summary(long _totalBytes)
			{
				this.totalBytes = _totalBytes;
				this.numInstances = 1;
				this.numGC = 0;
			}

			public long totalBytes;

			public int numInstances;

			public int numGC;
		}
	}
}
