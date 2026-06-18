using System;
using System.Runtime.CompilerServices;

public struct Vector3b : IEquatable<Vector3b>
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector3b(byte _x, byte _y, byte _z)
	{
		this.x = _x;
		this.y = _y;
		this.z = _z;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector3b(int _x, int _y, int _z)
	{
		this.x = (byte)_x;
		this.y = (byte)_y;
		this.z = (byte)_z;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector3i ToVector3i()
	{
		return new Vector3i((int)this.x, (int)this.y, (int)this.z);
	}

	public override int GetHashCode()
	{
		return ((int)this.x << 16) | ((int)this.y << 8) | (int)this.z;
	}

	public override bool Equals(object obj)
	{
		return obj != null && obj is Vector3b && this.Equals((Vector3b)obj);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Equals(Vector3b other)
	{
		return this.x == other.x && this.y == other.y && this.z == other.z;
	}

	public byte x;

	public byte y;

	public byte z;
}
