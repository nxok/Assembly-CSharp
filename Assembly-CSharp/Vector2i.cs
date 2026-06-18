using System;
using System.Runtime.CompilerServices;
using UnityEngine;

public struct Vector2i : IEquatable<Vector2i>
{
	public int this[int _index]
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			if (_index == 0)
			{
				return this.x;
			}
			if (_index != 1)
			{
				throw new IndexOutOfRangeException("Invalid Vector2i index!");
			}
			return this.y;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			if (_index == 0)
			{
				this.x = value;
				return;
			}
			if (_index != 1)
			{
				throw new IndexOutOfRangeException("Invalid Vector2i index!");
			}
			this.y = value;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector2i(int _x, int _y)
	{
		this.x = _x;
		this.y = _y;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector2i(Vector2 vector2)
	{
		this = default(Vector2i);
		this.x = Mathf.FloorToInt(vector2.x);
		this.y = Mathf.FloorToInt(vector2.y);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector2i(Vector2Int vector2)
	{
		this = default(Vector2i);
		this.x = vector2.x;
		this.y = vector2.y;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Set(int _x, int _y)
	{
		this.x = _x;
		this.y = _y;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector2Int AsVector2Int()
	{
		return new Vector2Int(this.x, this.y);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector2 AsVector2()
	{
		return new Vector2((float)this.x, (float)this.y);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public override bool Equals(object obj)
	{
		Vector2i vector2i = (Vector2i)obj;
		return this.Equals(vector2i);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Equals(Vector2i other)
	{
		return other.x == this.x && other.y == this.y;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float Distance(Vector2i a, Vector2i b)
	{
		double num = (double)(a.x - b.x);
		double num2 = (double)(a.y - b.y);
		return (float)Math.Sqrt(num * num + num2 * num2);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float DistanceSqr(Vector2i a, Vector2i b)
	{
		float num = (float)(a.x - b.x);
		float num2 = (float)(a.y - b.y);
		return num * num + num2 * num2;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int DistanceSqrInt(Vector2i a, Vector2i b)
	{
		int num = a.x - b.x;
		int num2 = a.y - b.y;
		return num * num + num2 * num2;
	}

	public void Normalize()
	{
		if (this.x < 0)
		{
			this.x = -1;
		}
		else if (this.x > 0)
		{
			this.x = 1;
		}
		if (this.y < 0)
		{
			this.y = -1;
			return;
		}
		if (this.y > 0)
		{
			this.y = 1;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator ==(Vector2i one, Vector2i other)
	{
		return one.x == other.x && one.y == other.y;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public override int GetHashCode()
	{
		return this.x * 8976890 + this.y * 981131;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator !=(Vector2i one, Vector2i other)
	{
		return !(one == other);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector2i operator +(Vector2i one, Vector2i other)
	{
		return new Vector2i(one.x + other.x, one.y + other.y);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector2i operator -(Vector2i one, Vector2i other)
	{
		return new Vector2i(one.x - other.x, one.y - other.y);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector2i operator /(Vector2i one, int div)
	{
		return new Vector2i(one.x / div, one.y / div);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector2i operator *(Vector2i a, int i)
	{
		return new Vector2i(a.x * i, a.y * i);
	}

	public override string ToString()
	{
		return string.Format("{0}, {1}", this.x, this.y);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static implicit operator Vector2Int(Vector2i _v2i)
	{
		return new Vector2Int(_v2i.x, _v2i.y);
	}

	public static readonly Vector2i zero = new Vector2i(0, 0);

	public static readonly Vector2i one = new Vector2i(1, 1);

	public static readonly Vector2i min = new Vector2i(int.MinValue, int.MinValue);

	public static readonly Vector2i max = new Vector2i(int.MaxValue, int.MaxValue);

	public static readonly Vector2i up = new Vector2i(0, 1);

	public static readonly Vector2i down = new Vector2i(0, -1);

	public static readonly Vector2i right = new Vector2i(1, 0);

	public static readonly Vector2i left = new Vector2i(-1, 0);

	public int x;

	public int y;
}
