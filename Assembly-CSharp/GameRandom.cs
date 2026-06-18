using System;
using System.Runtime.CompilerServices;
using UnityEngine;

public class GameRandom : IMemoryPoolableObject
{
	public void SetSeed(int _seed)
	{
		this.InternalSetSeed(_seed);
	}

	public void SetLock()
	{
	}

	public void Cleanup()
	{
	}

	public void Reset()
	{
	}

	public double RandomDouble
	{
		get
		{
			return this.NextDouble();
		}
	}

	public float RandomFloat
	{
		get
		{
			return (float)this.NextDouble();
		}
	}

	public int RandomInt
	{
		get
		{
			return this.Next();
		}
	}

	public Vector2 RandomInsideUnitCircle
	{
		get
		{
			float num = (float)this.NextDouble() * 6.2831855f;
			return new Vector2(Mathf.Cos(num), Mathf.Sin(num)) * (float)Math.Sqrt(this.NextDouble());
		}
	}

	public Vector2 RandomOnUnitCircle
	{
		get
		{
			float num = (float)this.NextDouble() * 6.2831855f;
			return new Vector2(Mathf.Cos(num), Mathf.Sin(num));
		}
	}

	public Vector3 RandomOnUnitCircleXZ
	{
		get
		{
			float num = (float)this.NextDouble() * 6.2831855f;
			return new Vector3(Mathf.Cos(num), 0f, Mathf.Sin(num));
		}
	}

	public Vector3 RandomInsideUnitSphere
	{
		get
		{
			return new Vector3((float)(this.NextDouble() - 0.5), (float)(this.NextDouble() - 0.5), (float)(this.NextDouble() - 0.5)).normalized * (float)Math.Sqrt(this.NextDouble());
		}
	}

	public Vector3 RandomOnUnitSphere
	{
		get
		{
			return new Vector3((float)(this.NextDouble() - 0.5), (float)(this.NextDouble() - 0.5), (float)(this.NextDouble() - 0.5)).normalized;
		}
	}

	public float RandomGaussian
	{
		get
		{
			float num;
			float num3;
			do
			{
				num = 2f * this.RandomRange(0f, 1f) - 1f;
				float num2 = 2f * this.RandomRange(0f, 1f) - 1f;
				num3 = num * num + num2 * num2;
			}
			while (num3 >= 1f || num3 == 0f);
			num3 = Mathf.Sqrt(-2f * Mathf.Log(num3) / num3);
			return num3 * num;
		}
	}

	public float RandomRange(float _maxExclusive)
	{
		return (float)(this.NextDouble() * (double)_maxExclusive);
	}

	public float RandomRange(float _min, float _maxExclusive)
	{
		return (float)(this.NextDouble() * (double)(_maxExclusive - _min) + (double)_min);
	}

	public int RandomRange(int _maxExclusive)
	{
		return this.Next(_maxExclusive);
	}

	public int RandomRange(int _min, int _maxExclusive)
	{
		return this.Next(_maxExclusive - _min) + _min;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void log(string _format, params object[] _values)
	{
		Log.Warning(string.Format("{0} GameRandom ", Time.time.ToCultureInvariantString()) + _format, _values);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void InternalSetSeed(int Seed)
	{
		int num = ((Seed == int.MinValue) ? int.MaxValue : Math.Abs(Seed));
		int num2 = 161803398 - num;
		this.SeedArray[55] = num2;
		int num3 = 1;
		for (int i = 1; i < 55; i++)
		{
			int num4 = 21 * i % 55;
			this.SeedArray[num4] = num3;
			num3 = num2 - num3;
			if (num3 < 0)
			{
				num3 += int.MaxValue;
			}
			num2 = this.SeedArray[num4];
		}
		for (int j = 1; j < 5; j++)
		{
			for (int k = 1; k < 56; k++)
			{
				this.SeedArray[k] -= this.SeedArray[1 + (k + 30) % 55];
				if (this.SeedArray[k] < 0)
				{
					this.SeedArray[k] += int.MaxValue;
				}
			}
		}
		this.inext = 0;
		this.inextp = 21;
		Seed = 1;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public double Sample()
	{
		return (double)this.InternalSample() * 4.656612875245797E-10;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int InternalSample()
	{
		int num = this.inext;
		int num2 = this.inextp;
		if (++num >= 56)
		{
			num = 1;
		}
		if (++num2 >= 56)
		{
			num2 = 1;
		}
		int num3 = this.SeedArray[num] - this.SeedArray[num2];
		if (num3 == 2147483647)
		{
			num3--;
		}
		if (num3 < 0)
		{
			num3 += int.MaxValue;
		}
		this.SeedArray[num] = num3;
		this.inext = num;
		this.inextp = num2;
		return num3;
	}

	public int PeekSample()
	{
		int num = this.inext;
		int num2 = this.inextp;
		if (++num >= 56)
		{
			num = 1;
		}
		if (++num2 >= 56)
		{
			num2 = 1;
		}
		int num3 = this.SeedArray[num] - this.SeedArray[num2];
		if (num3 == 2147483647)
		{
			num3--;
		}
		if (num3 < 0)
		{
			num3 += int.MaxValue;
		}
		return num3;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int Next()
	{
		return this.InternalSample();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public double GetSampleForLargeRange()
	{
		int num = this.InternalSample();
		if (this.InternalSample() % 2 == 0)
		{
			num = -num;
		}
		return ((double)num + 2147483646.0) / 4294967293.0;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int Next(int minValue, int maxValue)
	{
		if (minValue > maxValue)
		{
			throw new ArgumentOutOfRangeException("minValue", "Argument_MinMaxValue");
		}
		long num = (long)maxValue - (long)minValue;
		if (num <= 2147483647L)
		{
			return (int)(this.Sample() * (double)num) + minValue;
		}
		return (int)((long)(this.GetSampleForLargeRange() * (double)num) + (long)minValue);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int Next(int maxValue)
	{
		if (maxValue < 0)
		{
			throw new ArgumentOutOfRangeException("maxValue", "ArgumentOutOfRange_MustBePositive");
		}
		return (int)(this.Sample() * (double)maxValue);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public double NextDouble()
	{
		return this.Sample();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void NextBytes(byte[] buffer)
	{
		if (buffer == null)
		{
			throw new ArgumentNullException("buffer");
		}
		for (int i = 0; i < buffer.Length; i++)
		{
			buffer[i] = (byte)(this.InternalSample() % 256);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const int MBIG = 2147483647;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int MSEED = 161803398;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int MZ = 0;

	[PublicizedFrom(EAccessModifier.Private)]
	public int inext;

	[PublicizedFrom(EAccessModifier.Private)]
	public int inextp;

	[PublicizedFrom(EAccessModifier.Private)]
	public int[] SeedArray = new int[56];
}
