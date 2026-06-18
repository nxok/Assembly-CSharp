using System;
using System.IO;
using UnityEngine;

public struct BlockValueRef : IEquatable<BlockValueRef>
{
	[PublicizedFrom(EAccessModifier.Private)]
	public BlockValueRef(BlockValueRefType type, Vector3i blockPosition, PropRef propRef)
	{
		this.Type = type;
		this.BlockPosition = blockPosition;
		this.PropReference = propRef;
	}

	public BlockValueRef(int x, int y, int z)
	{
		this = new BlockValueRef(new Vector3i(x, y, z));
	}

	public BlockValueRef(Vector3i pos)
	{
		this = new BlockValueRef(BlockValueRefType.Block, pos, default(PropRef));
	}

	public BlockValueRef(PropRef propRef)
	{
		this = new BlockValueRef(BlockValueRefType.Prop, Vector3i.zero, propRef);
	}

	public bool TryGetBlockPos(out Vector3i pos)
	{
		if (this.Type != BlockValueRefType.Block)
		{
			pos = Vector3i.zero;
			return false;
		}
		pos = this.BlockPosition;
		return true;
	}

	public bool TryGetPropRef(out PropRef propRef)
	{
		if (this.Type != BlockValueRefType.Prop)
		{
			propRef = PropRef.Default;
			return false;
		}
		propRef = this.PropReference;
		return true;
	}

	public Vector3 ToVector3(WorldBase world)
	{
		Vector3 vector;
		switch (this.Type)
		{
		case BlockValueRefType.None:
			vector = Vector3.zero;
			break;
		case BlockValueRefType.Block:
			vector = this.BlockPosition.ToVector3();
			break;
		case BlockValueRefType.Prop:
		{
			IChunk chunkSync = world.GetChunkSync(this.PropReference);
			vector = ((chunkSync != null) ? chunkSync.GetWorldPos() : Vector3i.zero) + world.GetProp(this.PropReference).position;
			break;
		}
		default:
			throw new ArgumentOutOfRangeException();
		}
		return vector;
	}

	public Vector3 ToVector3Center(WorldBase world)
	{
		Vector3 vector;
		switch (this.Type)
		{
		case BlockValueRefType.None:
			vector = Vector3.zero;
			break;
		case BlockValueRefType.Block:
			vector = this.BlockPosition.ToVector3Center();
			break;
		case BlockValueRefType.Prop:
		{
			IChunk chunkSync = world.GetChunkSync(this.PropReference);
			vector = ((chunkSync != null) ? chunkSync.GetWorldPos() : Vector3i.zero) + world.GetProp(this.PropReference).position + new Vector3(0.5f, 0.5f, 0.5f);
			break;
		}
		default:
			throw new ArgumentOutOfRangeException();
		}
		return vector;
	}

	public Vector3 ToVector3CenterXZ(WorldBase world)
	{
		Vector3 vector;
		switch (this.Type)
		{
		case BlockValueRefType.None:
			vector = Vector3.zero;
			break;
		case BlockValueRefType.Block:
			vector = this.BlockPosition.ToVector3CenterXZ();
			break;
		case BlockValueRefType.Prop:
		{
			IChunk chunkSync = world.GetChunkSync(this.PropReference);
			vector = ((chunkSync != null) ? chunkSync.GetWorldPos() : Vector3i.zero) + world.GetProp(this.PropReference).position + new Vector3(0.5f, 0f, 0.5f);
			break;
		}
		default:
			throw new ArgumentOutOfRangeException();
		}
		return vector;
	}

	public Vector3i ToBlockPos(WorldBase world)
	{
		return World.worldToBlockPos(this.ToVector3Center(world));
	}

	public static BlockValueRef Create(WorldRayHitInfo hitInfo)
	{
		if (!hitInfo.hit.propValue.IsAir)
		{
			return new BlockValueRef(hitInfo.hit.propData.PropRef);
		}
		if (!hitInfo.hit.blockValue.isair)
		{
			return new BlockValueRef(hitInfo.hit.blockPos);
		}
		return BlockValueRef.None;
	}

	public static BlockValueRef Read(BinaryReader br)
	{
		BlockValueRef blockValueRef;
		switch (br.ReadByte())
		{
		case 0:
			blockValueRef = BlockValueRef.None;
			break;
		case 1:
			blockValueRef = new BlockValueRef(StreamUtils.ReadVector3i(br));
			break;
		case 2:
			blockValueRef = new BlockValueRef(PropRef.Read(br));
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
		return blockValueRef;
	}

	public readonly void Write(BinaryWriter bw)
	{
		bw.Write((byte)this.Type);
		switch (this.Type)
		{
		case BlockValueRefType.None:
			return;
		case BlockValueRefType.Block:
			StreamUtils.Write(bw, this.BlockPosition);
			return;
		case BlockValueRefType.Prop:
			this.PropReference.Write(bw);
			return;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	public override string ToString()
	{
		object obj = this.Type;
		string text;
		switch (this.Type)
		{
		case BlockValueRefType.None:
			text = "";
			break;
		case BlockValueRefType.Block:
			text = string.Format(", BlockPos=({0})", this.BlockPosition);
			break;
		case BlockValueRefType.Prop:
			text = string.Format(", PropRef=[{0}]", this.PropReference);
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
		return string.Format("Type={0}{1}", obj, text);
	}

	public static implicit operator Vector3i(BlockValueRef bvRef)
	{
		Vector3i vector3i;
		if (bvRef.TryGetBlockPos(out vector3i))
		{
			return vector3i;
		}
		Log.Warning(string.Format("[PROPS] BlockValueRef implicitly converted to Vector3i but type was {0}.", bvRef.Type));
		return Vector3i.zero;
	}

	public static implicit operator PropRef(BlockValueRef bvRef)
	{
		PropRef propRef;
		if (bvRef.TryGetPropRef(out propRef))
		{
			return propRef;
		}
		Log.Warning(string.Format("[PROPS] BlockValueRef implicitly converted to PropRef but type was {0}.", bvRef.Type));
		return PropRef.Default;
	}

	public static implicit operator BlockValueRef(Vector3i pos)
	{
		return new BlockValueRef(pos);
	}

	public static implicit operator BlockValueRef(PropRef propRef)
	{
		return new BlockValueRef(propRef);
	}

	public bool Equals(BlockValueRef other)
	{
		return this.Type == other.Type && this.BlockPosition.Equals(other.BlockPosition) && this.PropReference.Equals(other.PropReference);
	}

	public override bool Equals(object obj)
	{
		if (obj is BlockValueRef)
		{
			BlockValueRef blockValueRef = (BlockValueRef)obj;
			return this.Equals(blockValueRef);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine<int, Vector3i, PropRef>((int)this.Type, this.BlockPosition, this.PropReference);
	}

	public static bool operator ==(BlockValueRef left, BlockValueRef right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(BlockValueRef left, BlockValueRef right)
	{
		return !left.Equals(right);
	}

	public static readonly BlockValueRef None = new BlockValueRef(BlockValueRefType.None, Vector3i.zero, default(PropRef));

	public readonly BlockValueRefType Type;

	public readonly Vector3i BlockPosition;

	public readonly PropRef PropReference;
}
