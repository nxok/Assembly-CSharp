using System;
using UnityEngine;

public class ChunkCoordinates
{
	public ChunkCoordinates(int _x, int _y, int _z)
	{
		this.position = new Vector3i(_x, _y, _z);
	}

	public ChunkCoordinates(ChunkCoordinates _cc)
	{
		this.position = _cc.position;
	}

	public override bool Equals(object _obj)
	{
		return _obj is ChunkCoordinates && this.position.Equals(((ChunkCoordinates)_obj).position);
	}

	public override int GetHashCode()
	{
		return this.position.x + this.position.z << 8 + this.position.y << 16;
	}

	public float getDistance(int _x, int _y, int _z)
	{
		int num = this.position.x - _x;
		int num2 = this.position.y - _y;
		int num3 = this.position.z - _z;
		return Mathf.Sqrt((float)(num * num + num2 * num2 + num3 * num3));
	}

	public float getDistanceSquared(int _x, int _y, int _z)
	{
		int num = this.position.x - _x;
		int num2 = this.position.y - _y;
		int num3 = this.position.z - _z;
		return (float)(num * num + num2 * num2 + num3 * num3);
	}

	public Vector3i position;
}
