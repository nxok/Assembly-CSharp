using System;
using UnityEngine;

public class ChunkVertexLayer : IMemoryPoolableObject
{
	public ChunkVertexLayer()
	{
		this.wPow = 4;
		this.hPow = 4;
		int num = 1 << this.wPow;
		int num2 = 1 << this.hPow;
		this.m_Vertices = new Vector3[num * num2];
		this.yPos = new float[num * num2];
		this.valid = new bool[num * num2];
	}

	public void Reset()
	{
		for (int i = 0; i < this.valid.Length; i++)
		{
			this.m_Vertices[i] = Vector3.zero;
			this.yPos[i] = 0f;
			this.valid[i] = false;
		}
	}

	public void Cleanup()
	{
	}

	public bool getAt(int _x, int _y, out Vector3 _vec)
	{
		int num = _x + (_y << this.wPow);
		return this.getAt(num, out _vec);
	}

	public bool getAt(int _offs, out Vector3 _vec)
	{
		_vec = this.m_Vertices[_offs];
		return this.valid[_offs];
	}

	public void setAt(int _x, int _y, Vector3 _v)
	{
		int num = _x + (_y << this.wPow);
		this.m_Vertices[num] = _v;
		this.valid[num] = true;
	}

	public bool getYPosAt(int _x, int _y, out float _ypos)
	{
		int num = _x + (_y << this.wPow);
		return this.getYPosAt(num, out _ypos);
	}

	public bool getYPosAt(int _offs, out float _ypos)
	{
		_ypos = this.yPos[_offs];
		return this.valid[_offs];
	}

	public void setYPosAt(int _x, int _y, float _ypos)
	{
		int num = _x + (_y << this.wPow);
		this.yPos[num] = _ypos;
		this.valid[num] = true;
	}

	public void setInvalid(int _offs)
	{
		this.valid[_offs] = false;
	}

	public int GetUsedMem()
	{
		return this.m_Vertices.Length * 13 + 8;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3[] m_Vertices;

	[PublicizedFrom(EAccessModifier.Private)]
	public float[] yPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool[] valid;

	[PublicizedFrom(EAccessModifier.Private)]
	public int wPow;

	[PublicizedFrom(EAccessModifier.Private)]
	public int hPow;

	public static int InstanceCount;
}
