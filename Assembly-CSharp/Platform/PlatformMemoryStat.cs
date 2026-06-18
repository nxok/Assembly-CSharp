using System;
using System.Collections.Generic;
using System.Text;

namespace Platform
{
	public sealed class PlatformMemoryStat<T> : IPlatformMemoryStat<T>, IPlatformMemoryStat
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public PlatformMemoryStat(string name)
		{
			this.m_columnValues = new EnumDictionary<MemoryStatColumn, T>();
			this.m_columnLastValues = new EnumDictionary<MemoryStatColumn, T>();
			this.Name = name;
		}

		public string Name { get; }

		public void UpdateLast()
		{
			foreach (KeyValuePair<MemoryStatColumn, T> keyValuePair in this.m_columnValues)
			{
				MemoryStatColumn memoryStatColumn;
				T t;
				keyValuePair.Deconstruct(out memoryStatColumn, out t);
				MemoryStatColumn memoryStatColumn2 = memoryStatColumn;
				T t2 = t;
				this.m_columnLastValues[memoryStatColumn2] = t2;
			}
		}

		public void RenderColumn(StringBuilder builder, MemoryStatColumn column, bool delta)
		{
			T t;
			if (!this.m_columnValues.TryGetValue(column, out t))
			{
				return;
			}
			if (!delta)
			{
				PlatformMemoryRenderValue<T> renderValue = this.RenderValue;
				if (renderValue == null)
				{
					return;
				}
				renderValue(builder, t);
				return;
			}
			else
			{
				T t2 = this.m_columnLastValues[column];
				PlatformMemoryRenderDelta<T> renderDelta = this.RenderDelta;
				if (renderDelta == null)
				{
					return;
				}
				renderDelta(builder, t, t2);
				return;
			}
		}

		public event PlatformMemoryColumnChangedHandler<T> ColumnSetAfter;

		public PlatformMemoryRenderValue<T> RenderValue { get; set; }

		public PlatformMemoryRenderDelta<T> RenderDelta { get; set; }

		public void Set(MemoryStatColumn column, T value)
		{
			this.m_columnValues[column] = value;
			if (!this.m_columnLastValues.ContainsKey(column))
			{
				this.m_columnLastValues[column] = value;
			}
			PlatformMemoryColumnChangedHandler<T> columnSetAfter = this.ColumnSetAfter;
			if (columnSetAfter == null)
			{
				return;
			}
			columnSetAfter(column, value);
		}

		public bool TryGet(MemoryStatColumn column, out T value)
		{
			return this.m_columnValues.TryGetValue(column, out value);
		}

		public bool TryGetLast(MemoryStatColumn column, out T value)
		{
			return this.m_columnLastValues.TryGetValue(column, out value);
		}

		public static IPlatformMemoryStat<T> Create(string name)
		{
			return new PlatformMemoryStat<T>(name);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly EnumDictionary<MemoryStatColumn, T> m_columnValues;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly EnumDictionary<MemoryStatColumn, T> m_columnLastValues;
	}
}
