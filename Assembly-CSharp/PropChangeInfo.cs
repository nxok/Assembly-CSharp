using System;
using System.IO;
using UnityEngine;

public class PropChangeInfo
{
	public PropChangeInfo(Vector2i chunkPos, int? propId, Vector3? position, Quaternion? rotation, Vector3? scale, BlockValue? blockValue)
	{
		this.ChunkPos = chunkPos;
		this.PropId = propId;
		this.Position = position;
		this.Rotation = rotation;
		this.Scale = scale;
		this.BlockValue = blockValue;
	}

	public static PropChangeInfo Read(BinaryReader br)
	{
		PropChangeInfo.Builder builder = new PropChangeInfo.Builder(br.ReadInt32(), br.ReadInt32());
		byte b = br.ReadByte();
		if ((b & 1) == 1)
		{
			builder.SetPropId(br.ReadInt32());
		}
		if ((b & 2) == 2)
		{
			builder.SetPosition(StreamUtils.ReadVector3(br));
		}
		if ((b & 4) == 4)
		{
			builder.SetRotation(StreamUtils.ReadQuaterion(br));
		}
		if ((b & 8) == 8)
		{
			builder.SetScale(StreamUtils.ReadVector3(br));
		}
		if ((b & 16) == 16)
		{
			builder.SetBlockValue(new BlockValue
			{
				rawData = br.ReadUInt32(),
				damage = (int)br.ReadUInt16()
			});
		}
		return builder.Build();
	}

	public void Write(BinaryWriter bw)
	{
		bw.Write(this.ChunkPos.x);
		bw.Write(this.ChunkPos.y);
		PropChangeInfo.Flags flags = ((this.PropId != null) ? PropChangeInfo.Flags.HasPropId : ((PropChangeInfo.Flags)0)) | ((this.Position != null) ? PropChangeInfo.Flags.HasPosition : ((PropChangeInfo.Flags)0)) | ((this.Rotation != null) ? PropChangeInfo.Flags.HasRotation : ((PropChangeInfo.Flags)0)) | ((this.Scale != null) ? PropChangeInfo.Flags.HasScale : ((PropChangeInfo.Flags)0)) | ((this.BlockValue != null) ? PropChangeInfo.Flags.HasBlockValue : ((PropChangeInfo.Flags)0));
		bw.Write((byte)flags);
		if (this.PropId != null)
		{
			bw.Write(this.PropId.Value);
		}
		if (this.Position != null)
		{
			StreamUtils.Write(bw, this.Position.Value);
		}
		if (this.Rotation != null)
		{
			StreamUtils.Write(bw, this.Rotation.Value);
		}
		if (this.Scale != null)
		{
			StreamUtils.Write(bw, this.Scale.Value);
		}
		if (this.BlockValue != null)
		{
			bw.Write(this.BlockValue.Value.rawData);
			bw.Write((ushort)this.BlockValue.Value.damage);
		}
	}

	public override string ToString()
	{
		return string.Format("ChunkId={0}, PropId={1}, Position={2}, Rotation={3}, Scale={4}, BlockValue={5}", new object[] { this.ChunkPos, this.PropId, this.Position, this.Rotation, this.Scale, this.BlockValue });
	}

	public static readonly PropChangeInfo Empty = new PropChangeInfo.Builder(Vector2i.zero).Build();

	public readonly Vector2i ChunkPos;

	public readonly int? PropId;

	public readonly Vector3? Position;

	public readonly Quaternion? Rotation;

	public readonly Vector3? Scale;

	public readonly BlockValue? BlockValue;

	[Flags]
	[PublicizedFrom(EAccessModifier.Private)]
	public enum Flags : byte
	{
		HasPropId = 1,
		HasPosition = 2,
		HasRotation = 4,
		HasScale = 8,
		HasBlockValue = 16
	}

	public class Builder
	{
		public Builder(int chunkX, int chunkZ)
			: this(new Vector2i(chunkX, chunkZ))
		{
		}

		public Builder(Vector2i chunkPos)
		{
			this.ChunkPos = chunkPos;
		}

		public Builder(PropRef propRef)
		{
			this.ChunkPos = propRef.ChunkPos;
			this.PropId = new int?(propRef.PropId);
		}

		public PropChangeInfo Build()
		{
			return new PropChangeInfo(this.ChunkPos, this.PropId, this.Position, this.Rotation, this.Scale, this.BlockValue);
		}

		public PropChangeInfo.Builder SetPropId(int propId)
		{
			this.PropId = new int?(propId);
			return this;
		}

		public PropChangeInfo.Builder SetPosition(Vector3 position)
		{
			this.Position = new Vector3?(position);
			return this;
		}

		public PropChangeInfo.Builder SetRotation(Quaternion rotation)
		{
			this.Rotation = new Quaternion?(rotation);
			return this;
		}

		public PropChangeInfo.Builder SetScale(Vector3 scale)
		{
			this.Scale = new Vector3?(scale);
			return this;
		}

		public PropChangeInfo.Builder SetBlockValue(BlockValue blockValue)
		{
			this.BlockValue = new BlockValue?(blockValue);
			return this;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly Vector2i ChunkPos;

		[PublicizedFrom(EAccessModifier.Private)]
		public int? PropId;

		[PublicizedFrom(EAccessModifier.Private)]
		public Vector3? Position;

		[PublicizedFrom(EAccessModifier.Private)]
		public Quaternion? Rotation;

		[PublicizedFrom(EAccessModifier.Private)]
		public Vector3? Scale;

		[PublicizedFrom(EAccessModifier.Private)]
		public BlockValue? BlockValue;
	}
}
