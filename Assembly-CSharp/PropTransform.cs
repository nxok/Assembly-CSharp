using System;
using System.IO;
using UnityEngine;

public struct PropTransform : IEquatable<PropTransform>
{
	public static PropTransform Read(BinaryReader br)
	{
		return new PropTransform
		{
			position = StreamUtils.ReadVector3(br),
			rotation = StreamUtils.ReadQuaterion(br),
			scale = StreamUtils.ReadVector3(br)
		};
	}

	public readonly void Write(BinaryWriter bw)
	{
		StreamUtils.Write(bw, this.position);
		StreamUtils.Write(bw, this.rotation);
		StreamUtils.Write(bw, this.scale);
	}

	public override string ToString()
	{
		return string.Format("position={0}, rotation={1}, scale={2}", this.position, this.rotation, this.scale);
	}

	public bool Equals(PropTransform other)
	{
		return this.position.Equals(other.position) && this.rotation.Equals(other.rotation) && this.scale.Equals(other.scale);
	}

	public override bool Equals(object obj)
	{
		if (obj is PropTransform)
		{
			PropTransform propTransform = (PropTransform)obj;
			return this.Equals(propTransform);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine<Vector3, Quaternion, Vector3>(this.position, this.rotation, this.scale);
	}

	public static bool operator ==(PropTransform left, PropTransform right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(PropTransform left, PropTransform right)
	{
		return !left.Equals(right);
	}

	public static readonly PropTransform identity = new PropTransform
	{
		position = Vector3.zero,
		rotation = Quaternion.identity,
		scale = Vector3.one
	};

	public Vector3 position;

	public Quaternion rotation;

	public Vector3 scale;
}
