using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageQuestGotoPoint : NetPackage
{
	public NetPackageQuestGotoPoint Setup(int _traderId, int _playerId, FastTags<TagGroup.Global> _questTags, int _questCode, NetPackageQuestGotoPoint.QuestGotoTypes _gotoType, byte _difficulty, int posX = 0, int posZ = -1, float sizeX = 0f, float sizeY = 0f, float sizeZ = 0f, float offset = -1f, BiomeFilterTypes _biomeFilterType = BiomeFilterTypes.SameBiome, string _biomeFilter = "")
	{
		this.traderId = _traderId;
		this.playerId = _playerId;
		this.questCode = _questCode;
		this.GotoType = _gotoType;
		this.questTags = _questTags;
		this.position = new Vector2((float)posX, (float)posZ);
		this.size = new Vector3(sizeX, sizeY, sizeZ);
		this.difficulty = _difficulty;
		this.biomeFilterType = _biomeFilterType;
		this.biomeFilter = _biomeFilter;
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		this.playerId = _br.ReadInt32();
		this.questCode = _br.ReadInt32();
		this.GotoType = (NetPackageQuestGotoPoint.QuestGotoTypes)_br.ReadByte();
		this.questTags = FastTags<TagGroup.Global>.Parse(_br.ReadString());
		this.position = new Vector2((float)_br.ReadInt32(), (float)_br.ReadInt32());
		this.size = StreamUtils.ReadVector3(_br);
		this.difficulty = _br.ReadByte();
		this.biomeFilterType = (BiomeFilterTypes)_br.ReadByte();
		this.biomeFilter = _br.ReadString();
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write(this.playerId);
		_bw.Write(this.questCode);
		_bw.Write((byte)this.GotoType);
		_bw.Write(this.questTags.ToString());
		_bw.Write((int)this.position.x);
		_bw.Write((int)this.position.y);
		StreamUtils.Write(_bw, this.size);
		_bw.Write(this.difficulty);
		_bw.Write((byte)this.biomeFilterType);
		_bw.Write(this.biomeFilter);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null)
		{
			return;
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			for (int i = 0; i < 5; i++)
			{
				EntityAlive entityAlive = GameManager.Instance.World.GetEntity(this.playerId) as EntityAlive;
				PrefabInstance prefabInstance = null;
				if (this.GotoType == NetPackageQuestGotoPoint.QuestGotoTypes.Trader)
				{
					prefabInstance = GameManager.Instance.World.ChunkCache.ChunkProvider.GetDynamicPrefabDecorator().GetClosestPOIToWorldPos(this.questTags, new Vector2(entityAlive.position.x, entityAlive.position.z), null, -1, false, this.biomeFilterType, this.biomeFilter, "");
					if (prefabInstance == null)
					{
						prefabInstance = GameManager.Instance.World.ChunkCache.ChunkProvider.GetDynamicPrefabDecorator().GetClosestPOIToWorldPos(this.questTags, new Vector2(entityAlive.position.x, entityAlive.position.z), null, -1, false, BiomeFilterTypes.SameBiome, "", "");
					}
				}
				else if (this.GotoType == NetPackageQuestGotoPoint.QuestGotoTypes.Closest)
				{
					prefabInstance = GameManager.Instance.World.ChunkCache.ChunkProvider.GetDynamicPrefabDecorator().GetClosestPOIToWorldPos(this.questTags, new Vector2(entityAlive.position.x, entityAlive.position.z), null, -1, false, BiomeFilterTypes.SameBiome, "", "");
				}
				else if (this.GotoType == NetPackageQuestGotoPoint.QuestGotoTypes.RandomPOI)
				{
					prefabInstance = DynamicPrefabDecorator.GetRandomPOINearWorldPos(new Vector2(entityAlive.position.x, entityAlive.position.z), 100, 50000000, this.questTags, this.difficulty, null, -1, BiomeFilterTypes.SameBiome, "");
				}
				new Vector2((float)prefabInstance.boundingBoxPosition.x + (float)prefabInstance.boundingBoxSize.x / 2f, (float)prefabInstance.boundingBoxPosition.z + (float)prefabInstance.boundingBoxSize.z / 2f);
				if (prefabInstance != null)
				{
					SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageQuestGotoPoint>().Setup(this.traderId, this.playerId, this.questTags, this.questCode, this.GotoType, this.difficulty, prefabInstance.boundingBoxPosition.x, prefabInstance.boundingBoxPosition.z, (float)prefabInstance.boundingBoxSize.x, (float)prefabInstance.boundingBoxSize.y, (float)prefabInstance.boundingBoxSize.z, -1f, BiomeFilterTypes.SameBiome, ""), false, this.playerId, -1, -1, null, 192, false);
					return;
				}
			}
			return;
		}
		EntityPlayer primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
		Quest quest = primaryPlayer.QuestJournal.FindActiveQuest(this.questCode);
		if (quest != null)
		{
			for (int j = 0; j < quest.Objectives.Count; j++)
			{
				if (quest.Objectives[j] is ObjectiveGoto && this.GotoType == NetPackageQuestGotoPoint.QuestGotoTypes.Trader)
				{
					((ObjectiveGoto)quest.Objectives[j]).FinalizePoint(new Vector3(this.position.x, primaryPlayer.position.y, this.position.y), this.size);
				}
				else if (quest.Objectives[j] is ObjectiveClosestPOIGoto && this.GotoType == NetPackageQuestGotoPoint.QuestGotoTypes.Closest)
				{
					((ObjectiveClosestPOIGoto)quest.Objectives[j]).FinalizePoint(new Vector3(this.position.x, primaryPlayer.position.y, this.position.y), this.size);
				}
				else if (quest.Objectives[j] is ObjectiveRandomPOIGoto && this.GotoType == NetPackageQuestGotoPoint.QuestGotoTypes.RandomPOI)
				{
					((ObjectiveRandomPOIGoto)quest.Objectives[j]).FinalizePoint(new Vector3(this.position.x, primaryPlayer.position.y, this.position.y), this.size);
				}
			}
		}
	}

	public override int GetLength()
	{
		return 8;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int traderId;

	[PublicizedFrom(EAccessModifier.Private)]
	public int playerId;

	[PublicizedFrom(EAccessModifier.Private)]
	public int questCode;

	[PublicizedFrom(EAccessModifier.Private)]
	public FastTags<TagGroup.Global> questTags;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2 position;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 size;

	[PublicizedFrom(EAccessModifier.Private)]
	public byte difficulty;

	public NetPackageQuestGotoPoint.QuestGotoTypes GotoType;

	[PublicizedFrom(EAccessModifier.Private)]
	public BiomeFilterTypes biomeFilterType = BiomeFilterTypes.SameBiome;

	[PublicizedFrom(EAccessModifier.Private)]
	public string biomeFilter;

	public enum QuestGotoTypes
	{
		Trader,
		Closest,
		RandomPOI
	}
}
