using System;
using System.IO;

public class WorldBlockTickerEntry
{
	public WorldBlockTickerEntry(Vector3i _pos, int _id, ulong _scheduledTime)
	{
		long num = WorldBlockTickerEntry.nextTickEntryID;
		WorldBlockTickerEntry.nextTickEntryID = num + 1L;
		this.tickEntryID = num;
		this.worldPos = _pos;
		this.blockID = _id;
		this.scheduledTime = _scheduledTime;
	}

	public static WorldBlockTickerEntry Read(BinaryReader _br, int _chunkX, int _chunkZ, int _version)
	{
		Vector3i vector3i = new Vector3i((int)_br.ReadByte() + _chunkX * 16, (int)_br.ReadByte(), (int)_br.ReadByte() + _chunkZ * 16);
		int num = (int)_br.ReadUInt16();
		ulong num2 = _br.ReadUInt64();
		_br.ReadUInt16();
		return new WorldBlockTickerEntry(vector3i, num, num2);
	}

	public void Write(BinaryWriter _bw)
	{
		_bw.Write((byte)World.toBlockXZ(this.worldPos.x));
		_bw.Write((byte)this.worldPos.y);
		_bw.Write((byte)World.toBlockXZ(this.worldPos.z));
		_bw.Write((ushort)this.blockID);
		_bw.Write(this.scheduledTime);
		_bw.Write(0);
	}

	public override bool Equals(object _obj)
	{
		WorldBlockTickerEntry worldBlockTickerEntry = _obj as WorldBlockTickerEntry;
		return worldBlockTickerEntry != null && (this.worldPos.Equals(worldBlockTickerEntry.worldPos) && this.blockID == worldBlockTickerEntry.blockID) && worldBlockTickerEntry.tickEntryID == this.tickEntryID;
	}

	public override int GetHashCode()
	{
		return WorldBlockTickerEntry.ToHashCode(this.worldPos, this.blockID);
	}

	public static int ToHashCode(Vector3i _pos, int _blockID)
	{
		return (_pos.GetHashCode() * 397) ^ _blockID;
	}

	public long GetChunkKey()
	{
		return WorldChunkCache.MakeChunkKey(World.toChunkXZ(this.worldPos.x), World.toChunkXZ(this.worldPos.z));
	}

	public readonly Vector3i worldPos;

	public readonly int blockID;

	public readonly ulong scheduledTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public static long nextTickEntryID;

	public readonly long tickEntryID;
}
