using System;
using System.IO;

public struct PropRef : IEquatable<PropRef>
{
	public static PropRef Read(BinaryReader br)
	{
		return new PropRef
		{
			ChunkPos = StreamUtils.ReadVector2i(br),
			PropId = br.ReadInt32()
		};
	}

	public readonly void Write(BinaryWriter bw)
	{
		StreamUtils.Write(bw, this.ChunkPos);
		bw.Write(this.PropId);
	}

	public override string ToString()
	{
		return string.Format("ChunkPos=({0}), PropId={1}", this.ChunkPos, this.PropId);
	}

	public bool Equals(PropRef other)
	{
		return this.ChunkPos.Equals(other.ChunkPos) && this.PropId == other.PropId;
	}

	public override bool Equals(object obj)
	{
		if (obj is PropRef)
		{
			PropRef propRef = (PropRef)obj;
			return this.Equals(propRef);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine<Vector2i, int>(this.ChunkPos, this.PropId);
	}

	public static bool operator ==(PropRef left, PropRef right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(PropRef left, PropRef right)
	{
		return !left.Equals(right);
	}

	public static readonly PropRef Default = new PropRef
	{
		ChunkPos = Vector2i.zero,
		PropId = 0
	};

	public Vector2i ChunkPos;

	public int PropId;
}
