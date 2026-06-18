using System;
using System.Runtime.CompilerServices;
using UnityEngine;

public struct Vector3i : IEquatable<Vector3i>
{
	public int this[int _index]
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			switch (_index)
			{
			case 0:
				return this.x;
			case 1:
				return this.y;
			case 2:
				return this.z;
			default:
				throw new IndexOutOfRangeException("Invalid Vector3i index!");
			}
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			switch (_index)
			{
			case 0:
				this.x = value;
				return;
			case 1:
				this.y = value;
				return;
			case 2:
				this.z = value;
				return;
			default:
				throw new IndexOutOfRangeException("Invalid Vector3i index!");
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector3i(int _x, int _y, int _z)
	{
		this.x = _x;
		this.y = _y;
		this.z = _z;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector3i(Vector3 _v)
	{
		this.x = (int)_v.x;
		this.y = (int)_v.y;
		this.z = (int)_v.z;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector3i(Vector3Int _v)
	{
		this.x = _v.x;
		this.y = _v.y;
		this.z = _v.z;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector3i(float _x, float _y, float _z)
	{
		this.x = (int)_x;
		this.y = (int)_y;
		this.z = (int)_z;
	}

	public void FloorToInt(Vector3 _v)
	{
		this.x = Utils.Fastfloor(_v.x);
		this.y = Utils.Fastfloor(_v.y);
		this.z = Utils.Fastfloor(_v.z);
	}

	public void RoundToInt(Vector3 _v)
	{
		this.x = Mathf.RoundToInt(_v.x);
		this.y = Mathf.RoundToInt(_v.y);
		this.z = Mathf.RoundToInt(_v.z);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Equals(int _x, int _y, int _z)
	{
		return this.x == _x && this.y == _y && this.z == _z;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public override bool Equals(object obj)
	{
		Vector3i vector3i = (Vector3i)obj;
		return this.Equals(vector3i);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Equals(Vector3i other)
	{
		return other.x == this.x && other.y == this.y && other.z == this.z;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator ==(Vector3i one, Vector3i other)
	{
		return one.x == other.x && one.y == other.y && one.z == other.z;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public override int GetHashCode()
	{
		return this.x * 8976890 + this.y * 981131 + this.z;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator !=(Vector3i one, Vector3i other)
	{
		return !(one == other);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector3i operator -(Vector3i one, Vector3i other)
	{
		return new Vector3i(one.x - other.x, one.y - other.y, one.z - other.z);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector3i operator +(Vector3i one, Vector3i other)
	{
		return new Vector3i(one.x + other.x, one.y + other.y, one.z + other.z);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector3i operator *(Vector3i a, float d)
	{
		return new Vector3i((float)a.x * d, (float)a.y * d, (float)a.z * d);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector3i operator *(float d, Vector3i a)
	{
		return new Vector3i((float)a.x * d, (float)a.y * d, (float)a.z * d);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector3i operator *(Vector3i a, int i)
	{
		return new Vector3i(a.x * i, a.y * i, a.z * i);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector3i operator *(int i, Vector3i a)
	{
		return new Vector3i(a.x * i, a.y * i, a.z * i);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector3i operator /(Vector3i a, int i)
	{
		return new Vector3i(a.x / i, a.y / i, a.z / i);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector3i operator &(Vector3i a, int i)
	{
		return new Vector3i(a.x & i, a.y & i, a.z & i);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector3i operator &(int i, Vector3i a)
	{
		return new Vector3i(a.x & i, a.y & i, a.z & i);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int Volume()
	{
		return this.x * this.y * this.z;
	}

	public override string ToString()
	{
		return string.Format("{0}, {1}, {2}", this.x, this.y, this.z);
	}

	public string ToStringNoBlanks()
	{
		return string.Format("{0},{1},{2}", this.x, this.y, this.z);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly Vector3 ToVector3()
	{
		return new Vector3((float)this.x, (float)this.y, (float)this.z);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly Vector3 ToVector3Center()
	{
		return new Vector3((float)this.x + 0.5f, (float)this.y + 0.5f, (float)this.z + 0.5f);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly Vector3 ToVector3CenterXZ()
	{
		return new Vector3((float)this.x + 0.5f, (float)this.y, (float)this.z + 0.5f);
	}

	public static Vector3i Parse(string _s)
	{
		string[] array = _s.Split(',', StringSplitOptions.None);
		if (array.Length != 3)
		{
			return Vector3i.zero;
		}
		return new Vector3i(int.Parse(array[0]), int.Parse(array[1]), int.Parse(array[2]));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector3i Cross(Vector3i _a, Vector3i _b)
	{
		return new Vector3i(_a.y * _b.z - _a.z * _b.y, _a.z * _b.x - _a.x * _b.z, _a.x * _b.y - _a.y * _b.x);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector3i FromVector3Rounded(Vector3 _v)
	{
		return new Vector3i(Mathf.RoundToInt(_v.x), Mathf.RoundToInt(_v.y), Mathf.RoundToInt(_v.z));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector3i Min(Vector3i v1, Vector3i v2)
	{
		return new Vector3i(Math.Min(v1.x, v2.x), Math.Min(v1.y, v2.y), Math.Min(v1.z, v2.z));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector3i Max(Vector3i v1, Vector3i v2)
	{
		return new Vector3i(Math.Max(v1.x, v2.x), Math.Max(v1.y, v2.y), Math.Max(v1.z, v2.z));
	}

	public static void SortBoundingBoxEdges(ref Vector3i _minEdge, ref Vector3i _maxEdge)
	{
		Vector3i vector3i = _minEdge;
		Vector3i vector3i2 = _maxEdge;
		_minEdge = new Vector3i(Math.Min(vector3i.x, vector3i2.x), Math.Min(vector3i.y, vector3i2.y), Math.Min(vector3i.z, vector3i2.z));
		_maxEdge = new Vector3i(Math.Max(vector3i.x, vector3i2.x), Math.Max(vector3i.y, vector3i2.y), Math.Max(vector3i.z, vector3i2.z));
	}

	public bool IsValid
	{
		get
		{
			return this != Vector3i.invalid;
		}
	}

	public static Vector3i Floor(Vector3 _v)
	{
		Vector3i vector3i;
		vector3i.x = Utils.Fastfloor(_v.x);
		vector3i.y = Utils.Fastfloor(_v.y);
		vector3i.z = Utils.Fastfloor(_v.z);
		return vector3i;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static implicit operator Vector3(Vector3i _v3i)
	{
		return _v3i.ToVector3();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static implicit operator Vector3Int(Vector3i _v3i)
	{
		return new Vector3Int(_v3i.x, _v3i.y, _v3i.z);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static implicit operator Vector3i(Vector3Int _v3i)
	{
		return new Vector3i(_v3i.x, _v3i.y, _v3i.z);
	}

	public static readonly Vector3i invalid = new Vector3i(int.MinValue, int.MinValue, int.MinValue);

	public static readonly Vector3i up = new Vector3i(0, 1, 0);

	public static readonly Vector3i down = new Vector3i(0, -1, 0);

	public static readonly Vector3i zero = new Vector3i(0, 0, 0);

	public static readonly Vector3i left = new Vector3i(-1, 0, 0);

	public static readonly Vector3i right = new Vector3i(1, 0, 0);

	public static readonly Vector3i forward = new Vector3i(0, 0, 1);

	public static readonly Vector3i back = new Vector3i(0, 0, -1);

	public static readonly Vector3i one = new Vector3i(1, 1, 1);

	public static readonly Vector3i min = new Vector3i(int.MinValue, int.MinValue, int.MinValue);

	public static readonly Vector3i max = new Vector3i(int.MaxValue, int.MaxValue, int.MaxValue);

	public static readonly Vector3i[] MIDDLE_AND_ADJACENT_DIRECTIONS = new Vector3i[]
	{
		Vector3i.zero,
		new Vector3i(1, 0, 0),
		new Vector3i(-1, 0, 0),
		new Vector3i(0, 1, 0),
		new Vector3i(0, -1, 0),
		new Vector3i(0, 0, 1),
		new Vector3i(0, 0, -1)
	};

	public static readonly Vector3i[] AllDirections = new Vector3i[]
	{
		Vector3i.right,
		Vector3i.left,
		Vector3i.up,
		Vector3i.down,
		Vector3i.forward,
		Vector3i.back
	};

	public static readonly Vector3i[] AllDirectionsShuffled = new Vector3i[]
	{
		Vector3i.right,
		Vector3i.left,
		Vector3i.up,
		Vector3i.down,
		Vector3i.forward,
		Vector3i.back
	};

	public static readonly Vector3i[] MIDDLE_AND_HORIZONTAL_DIRECTIONS = new Vector3i[]
	{
		Vector3i.zero,
		new Vector3i(1, 0, 0),
		new Vector3i(-1, 0, 0),
		new Vector3i(0, 0, 1),
		new Vector3i(0, 0, -1)
	};

	public static readonly Vector3i[] HORIZONTAL_DIRECTIONS = new Vector3i[]
	{
		new Vector3i(1, 0, 0),
		new Vector3i(-1, 0, 0),
		new Vector3i(0, 0, 1),
		new Vector3i(0, 0, -1)
	};

	public static readonly Vector3i[] MIDDLE_AND_HORIZONTAL_DIRECTIONS_DIAGONAL = new Vector3i[]
	{
		Vector3i.zero,
		new Vector3i(1, 0, 0),
		new Vector3i(1, 0, -1),
		new Vector3i(0, 0, -1),
		new Vector3i(-1, 0, -1),
		new Vector3i(-1, 0, 0),
		new Vector3i(-1, 0, 1),
		new Vector3i(0, 0, 1),
		new Vector3i(1, 0, 1)
	};

	public int x;

	public int y;

	public int z;
}
