using System;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;

[Serializable]
public struct PropValue : IEquatable<PropValue>
{
	public Vector3 position
	{
		get
		{
			return this.transform.position;
		}
		set
		{
			this.transform.position = value;
		}
	}

	public Quaternion rotation
	{
		get
		{
			return this.transform.rotation;
		}
		set
		{
			this.transform.rotation = value;
		}
	}

	public Vector3 scale
	{
		get
		{
			return this.transform.scale;
		}
		set
		{
			this.transform.scale = value;
		}
	}

	public bool IsAir
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return this.blockValue.isair;
		}
	}

	public static PropValue Read(BinaryReader br)
	{
		return new PropValue
		{
			transform = PropTransform.Read(br),
			blockValue = BlockValue.Read(br)
		};
	}

	public readonly void Write(BinaryWriter bw)
	{
		this.transform.Write(bw);
		this.blockValue.Write(bw);
	}

	public override string ToString()
	{
		return string.Format("transform=[{0}], blockValue=[{1}]", this.transform, this.blockValue);
	}

	public bool Equals(PropValue other)
	{
		return this.transform.Equals(other.transform) && this.blockValue.Equals(other.blockValue);
	}

	public override bool Equals(object obj)
	{
		if (obj is PropValue)
		{
			PropValue propValue = (PropValue)obj;
			return this.Equals(propValue);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine<PropTransform, BlockValue>(this.transform, this.blockValue);
	}

	public static bool operator ==(PropValue left, PropValue right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(PropValue left, PropValue right)
	{
		return !left.Equals(right);
	}

	public static readonly PropValue AIR = new PropValue
	{
		transform = PropTransform.identity,
		blockValue = BlockValue.Air
	};

	public PropTransform transform;

	public BlockValue blockValue;
}
