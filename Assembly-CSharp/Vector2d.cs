using System;
using System.Runtime.CompilerServices;
using UnityEngine;

public struct Vector2d : IEquatable<Vector2d>
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector2d(double _x, double _y)
	{
		this.x = _x;
		this.y = _y;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector2d(Vector2 _v)
	{
		this.x = (double)_v.x;
		this.y = (double)_v.y;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector2d(Vector2i _v)
	{
		this.x = (double)_v.x;
		this.y = (double)_v.y;
	}

	public bool Equals(double _x, double _y)
	{
		return this.x == _x && this.y == _y;
	}

	public override bool Equals(object obj)
	{
		Vector2d vector2d = (Vector2d)obj;
		return this.Equals(vector2d);
	}

	public bool Equals(Vector2d other)
	{
		return other.x == this.x && other.y == this.y;
	}

	public static bool operator ==(Vector2d one, Vector2d other)
	{
		return one.x == other.x && one.y == other.y;
	}

	public override int GetHashCode()
	{
		return this.x.GetHashCode() ^ (this.y.GetHashCode() << 2);
	}

	public static bool operator !=(Vector2d one, Vector2d other)
	{
		return !(one == other);
	}

	public static Vector2d operator -(Vector2d one, Vector2d other)
	{
		return new Vector2d(one.x - other.x, one.y - other.y);
	}

	public static Vector2d operator +(Vector2d one, Vector2d other)
	{
		return new Vector2d(one.x + other.x, one.y + other.y);
	}

	public static Vector2d operator *(Vector2d a, double d)
	{
		return new Vector2d(a.x * d, a.y * d);
	}

	public static Vector2d operator *(double d, Vector2d a)
	{
		return new Vector2d(a.x * d, a.y * d);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static double Dot(Vector2d lhs, Vector2d rhs)
	{
		return lhs.x * rhs.x + lhs.y * rhs.y;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public double Dot(Vector2 rhs)
	{
		return this.x * (double)rhs.x + this.y * (double)rhs.y;
	}

	public override string ToString()
	{
		return this.ToCultureInvariantString();
	}

	public string ToCultureInvariantString()
	{
		return string.Concat(new string[]
		{
			"(",
			this.x.ToCultureInvariantString("F1"),
			", ",
			this.y.ToCultureInvariantString("F1"),
			")"
		});
	}

	public double x;

	public double y;

	public static readonly Vector2d Zero = new Vector2d(0.0, 0.0);
}
