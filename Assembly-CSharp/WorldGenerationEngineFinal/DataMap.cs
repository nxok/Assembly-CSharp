using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace WorldGenerationEngineFinal
{
	public class DataMap<T> where T : struct
	{
		public DataMap(int tileWidth, T defaultValue)
		{
			this.width = tileWidth;
			int num = this.width * this.width;
			this.data = new T[num];
			for (int i = 0; i < num; i++)
			{
				this.data[i] = defaultValue;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T Get(int x, int y)
		{
			return this.data[x + y * this.width];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Set(int x, int y, T _value)
		{
			this.data[x + y * this.width] = _value;
		}

		public void Replace(T _old, T _new)
		{
			EqualityComparer<T> @default = EqualityComparer<T>.Default;
			int num = this.width * this.width;
			for (int i = 0; i < num; i++)
			{
				if (@default.Equals(this.data[i], _old))
				{
					this.data[i] = _new;
				}
			}
		}

		public int width;

		[PublicizedFrom(EAccessModifier.Private)]
		public T[] data;
	}
}
