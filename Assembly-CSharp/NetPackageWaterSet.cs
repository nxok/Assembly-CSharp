using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageWaterSet : NetPackage
{
	public bool HasChanges
	{
		get
		{
			return this.changes.Count > 0;
		}
	}

	public void SetSenderId(int _entityId)
	{
		this.senderEntityId = _entityId;
	}

	public void Reset()
	{
		this.senderEntityId = -1;
		this.changes.Clear();
	}

	public void AddChange(int _worldX, int _worldY, int _worldZ, WaterValue _data)
	{
		this.AddChange(new Vector3i(_worldX, _worldY, _worldZ), _data);
	}

	public void AddChange(Vector3i _worldPos, WaterValue _data)
	{
		NetPackageWaterSet.WaterSetInfo waterSetInfo = new NetPackageWaterSet.WaterSetInfo
		{
			worldPos = _worldPos,
			waterData = _data
		};
		this.changes.Add(waterSetInfo);
	}

	public override void read(PooledBinaryReader _br)
	{
		this.senderEntityId = _br.ReadInt32();
		this.changes.Clear();
		int num = (int)_br.ReadUInt16();
		for (int i = 0; i < num; i++)
		{
			NetPackageWaterSet.WaterSetInfo waterSetInfo = default(NetPackageWaterSet.WaterSetInfo);
			waterSetInfo.Read(_br);
			this.changes.Add(waterSetInfo);
		}
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write(this.senderEntityId);
		int count = this.changes.Count;
		_bw.Write((ushort)count);
		for (int i = 0; i < this.changes.Count; i++)
		{
			this.changes[i].Write(_bw);
		}
	}

	public void ApplyChanges(ChunkCluster _cc)
	{
		_cc.ChunkPosNeedsRegeneration_DelayedStart();
		for (int i = 0; i < this.changes.Count; i++)
		{
			NetPackageWaterSet.WaterSetInfo waterSetInfo = this.changes[i];
			_cc.SetWater(waterSetInfo.worldPos, waterSetInfo.waterData);
			GameManager.Instance.World.HandleWaterLevelChanged(waterSetInfo.worldPos, waterSetInfo.waterData.GetMassPercent());
		}
		_cc.ChunkPosNeedsRegeneration_DelayedStop();
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(this, false, -1, this.senderEntityId, -1, null, 192, false);
		}
		if (_world == null)
		{
			return;
		}
		ChunkCluster chunkCache = _world.ChunkCache;
		if (chunkCache == null)
		{
			return;
		}
		this.ApplyChanges(chunkCache);
	}

	public override int GetLength()
	{
		return 2 + this.changes.Count * NetPackageWaterSet.WaterSetInfo.GetLength();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int senderEntityId = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<NetPackageWaterSet.WaterSetInfo> changes = new List<NetPackageWaterSet.WaterSetInfo>();

	[PublicizedFrom(EAccessModifier.Private)]
	public struct WaterSetInfo
	{
		public void Read(BinaryReader _br)
		{
			this.worldPos = StreamUtils.ReadVector3i(_br);
			this.waterData.Read(_br);
		}

		public void Write(BinaryWriter _bw)
		{
			StreamUtils.Write(_bw, this.worldPos);
			this.waterData.Write(_bw);
		}

		public static int GetLength()
		{
			return 12 + WaterValue.SerializedLength();
		}

		public Vector3i worldPos;

		public WaterValue waterData;
	}
}
