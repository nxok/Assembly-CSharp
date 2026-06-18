using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class NetPackagePOIAround : NetPackage
{
	public override int Channel
	{
		get
		{
			return 1;
		}
	}

	public override bool Compress
	{
		get
		{
			return true;
		}
	}

	public NetPackagePOIAround Setup(Dictionary<int, PrefabInstance> _prefabsAroundFar, Dictionary<int, PrefabInstance> _prefabsAroundNear)
	{
		using (PooledBinaryWriter pooledBinaryWriter = MemoryPools.poolBinaryWriter.AllocSync(false))
		{
			pooledBinaryWriter.SetBaseStream(this.ms);
			pooledBinaryWriter.Write((ushort)_prefabsAroundFar.Count);
			foreach (KeyValuePair<int, PrefabInstance> keyValuePair in _prefabsAroundFar)
			{
				pooledBinaryWriter.Write(keyValuePair.Value.id);
				pooledBinaryWriter.Write(keyValuePair.Value.rotation);
				pooledBinaryWriter.Write((keyValuePair.Value.prefab.distantPOIOverride == null) ? keyValuePair.Value.prefab.PrefabName : keyValuePair.Value.prefab.distantPOIOverride);
				StreamUtils.Write(pooledBinaryWriter, keyValuePair.Value.boundingBoxPosition);
				StreamUtils.Write(pooledBinaryWriter, keyValuePair.Value.boundingBoxSize);
				pooledBinaryWriter.Write(keyValuePair.Value.prefab.distantPOIYOffset);
			}
			pooledBinaryWriter.Write((ushort)_prefabsAroundNear.Count);
			foreach (KeyValuePair<int, PrefabInstance> keyValuePair2 in _prefabsAroundNear)
			{
				pooledBinaryWriter.Write(keyValuePair2.Value.id);
				pooledBinaryWriter.Write(keyValuePair2.Value.rotation);
				pooledBinaryWriter.Write((keyValuePair2.Value.prefab.distantPOIOverride == null) ? keyValuePair2.Value.prefab.PrefabName : keyValuePair2.Value.prefab.distantPOIOverride);
				StreamUtils.Write(pooledBinaryWriter, keyValuePair2.Value.boundingBoxPosition);
				StreamUtils.Write(pooledBinaryWriter, keyValuePair2.Value.boundingBoxSize);
				pooledBinaryWriter.Write(keyValuePair2.Value.prefab.distantPOIYOffset);
			}
		}
		return this;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public ~NetPackagePOIAround()
	{
		MemoryPools.poolMemoryStream.FreeSync(this.ms);
	}

	public override void read(PooledBinaryReader _reader)
	{
		int num = _reader.ReadInt32();
		StreamUtils.StreamCopy(_reader.BaseStream, this.ms, num, null, true);
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write((int)this.ms.Length);
		this.ms.WriteTo(_writer.BaseStream);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		Dictionary<int, PrefabInstance> dictionary = new Dictionary<int, PrefabInstance>();
		Dictionary<int, PrefabInstance> dictionary2 = new Dictionary<int, PrefabInstance>();
		using (PooledBinaryReader pooledBinaryReader = MemoryPools.poolBinaryReader.AllocSync(false))
		{
			PooledExpandableMemoryStream pooledExpandableMemoryStream = this.ms;
			lock (pooledExpandableMemoryStream)
			{
				pooledBinaryReader.SetBaseStream(this.ms);
				this.ms.Position = 0L;
				int num = (int)pooledBinaryReader.ReadUInt16();
				for (int i = 0; i < num; i++)
				{
					int num2 = pooledBinaryReader.ReadInt32();
					byte b = pooledBinaryReader.ReadByte();
					string text = pooledBinaryReader.ReadString();
					PathAbstractions.AbstractedLocation location = PathAbstractions.PrefabsSearchPaths.GetLocation(text, null, null);
					Vector3i vector3i = StreamUtils.ReadVector3i(pooledBinaryReader);
					Vector3i vector3i2 = StreamUtils.ReadVector3i(pooledBinaryReader);
					dictionary.Add(num2, new PrefabInstance(num2, location, vector3i, b, null, 1)
					{
						boundingBoxSize = vector3i2,
						yOffsetOfPrefab = pooledBinaryReader.ReadSingle()
					});
				}
				num = (int)pooledBinaryReader.ReadUInt16();
				for (int j = 0; j < num; j++)
				{
					int num3 = pooledBinaryReader.ReadInt32();
					byte b2 = pooledBinaryReader.ReadByte();
					string text2 = pooledBinaryReader.ReadString();
					PathAbstractions.AbstractedLocation location2 = PathAbstractions.PrefabsSearchPaths.GetLocation(text2, null, null);
					Vector3i vector3i3 = StreamUtils.ReadVector3i(pooledBinaryReader);
					Vector3i vector3i4 = StreamUtils.ReadVector3i(pooledBinaryReader);
					dictionary2.Add(num3, new PrefabInstance(num3, location2, vector3i3, b2, null, 1)
					{
						boundingBoxSize = vector3i4,
						yOffsetOfPrefab = pooledBinaryReader.ReadSingle()
					});
				}
			}
		}
		GameManager.Instance.prefabLODManager.UpdatePrefabsAround(dictionary, dictionary2);
	}

	public override int GetLength()
	{
		return (int)this.ms.Length;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public PooledExpandableMemoryStream ms = MemoryPools.poolMemoryStream.AllocSync(true);
}
