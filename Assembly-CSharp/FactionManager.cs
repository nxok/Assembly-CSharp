using System;
using System.IO;
using UnityEngine;

public class FactionManager
{
	public void PrintData()
	{
		for (int i = 0; i < this.Factions.Length; i++)
		{
			if (this.Factions[i] != null)
			{
				Log.Out(this.Factions[i].ToString());
			}
		}
	}

	public static void Init()
	{
		FactionManager.Instance = new FactionManager();
	}

	public void Update()
	{
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer || GameManager.Instance.World == null || GameManager.Instance.World.Players == null || GameManager.Instance.World.Players.Count == 0 || GameManager.Instance.gameStateManager.IsGameStarted())
		{
			return;
		}
		this.saveTime -= Time.deltaTime;
		if (this.saveTime <= 0f && (this.dataSaveThreadInfo == null || this.dataSaveThreadInfo.HasTerminated()))
		{
			this.saveTime = 60f;
			this.Save();
		}
	}

	public FactionManager.Relationship GetRelationshipTier(EntityAlive checkingEntity, EntityAlive targetEntity)
	{
		if (checkingEntity == null || targetEntity == null)
		{
			return FactionManager.Relationship.Neutral;
		}
		this.rel = this.GetRelationshipValue(checkingEntity, targetEntity);
		if (this.rel < 200f)
		{
			return FactionManager.Relationship.Hate;
		}
		if (this.rel < 400f)
		{
			return FactionManager.Relationship.Dislike;
		}
		if (this.rel < 600f)
		{
			return FactionManager.Relationship.Neutral;
		}
		if (this.rel < 800f)
		{
			return FactionManager.Relationship.Like;
		}
		if (this.rel < 1001f)
		{
			return FactionManager.Relationship.Love;
		}
		return FactionManager.Relationship.Leader;
	}

	public Faction CreateFaction(string _name = "", bool _playerFaction = true, string _icon = "")
	{
		Faction faction = new Faction(_name, _playerFaction, _icon);
		this.AddFaction(faction);
		return faction;
	}

	public void AddFaction(Faction _faction)
	{
		for (int i = (_faction.IsPlayerFaction ? 8 : 0); i < this.Factions.Length; i++)
		{
			if (this.Factions[i] == null)
			{
				this.Factions[i] = _faction;
				_faction.ID = (byte)i;
				return;
			}
		}
	}

	public void RemoveFaction(byte _id)
	{
		this.Factions[(int)_id] = null;
	}

	public Faction GetFaction(byte _id)
	{
		return this.Factions[(int)_id];
	}

	public Faction GetFactionByName(string _name)
	{
		for (int i = 0; i < this.Factions.Length; i++)
		{
			if (this.Factions[i].Name == _name)
			{
				return this.Factions[i];
			}
		}
		return null;
	}

	public float GetRelationshipValue(EntityAlive checkingEntity, EntityAlive targetEntity)
	{
		if (checkingEntity == null || targetEntity == null)
		{
			return 400f;
		}
		if (checkingEntity.factionId == targetEntity.factionId)
		{
			return 800f;
		}
		if (this.Factions[(int)checkingEntity.factionId] != null && this.Factions[(int)targetEntity.factionId] != null)
		{
			return this.Factions[(int)checkingEntity.factionId].GetRelationship(targetEntity.factionId);
		}
		return 400f;
	}

	public void SetRelationship(byte _myFaction, byte _targetFaction, sbyte _modification)
	{
		if (this.Factions[(int)_myFaction] != null)
		{
			this.Factions[(int)_myFaction].ModifyRelationship(_targetFaction, (float)_modification);
		}
	}

	public void ModifyRelationship(byte _myFaction, byte _targetFaction, sbyte _modification)
	{
		if (this.Factions[(int)_myFaction] != null)
		{
			this.Factions[(int)_myFaction].ModifyRelationship(_targetFaction, (float)_modification);
		}
	}

	public void Write(BinaryWriter _bw)
	{
		_bw.Write(FactionManager.Version);
		for (int i = 0; i < this.Factions.Length; i++)
		{
			_bw.Write(this.Factions[i] != null);
			if (this.Factions[i] != null)
			{
				this.Factions[i].Write(_bw);
			}
		}
	}

	public void Read(BinaryReader _br)
	{
		_br.ReadByte();
		for (int i = 0; i < 255; i++)
		{
			if (_br.ReadBoolean())
			{
				this.Factions[i] = new Faction();
				this.Factions[i].Read(_br);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int saveFactionDataThreaded(ThreadManager.ThreadInfo _threadInfo)
	{
		PooledExpandableMemoryStream pooledExpandableMemoryStream = (PooledExpandableMemoryStream)_threadInfo.parameter;
		string text = string.Format("{0}/{1}", GameIO.GetSaveGameDir(), "factions.dat");
		if (!SdDirectory.Exists(GameIO.GetSaveGameDir()))
		{
			return -1;
		}
		if (SdFile.Exists(text))
		{
			SdFile.Copy(text, string.Format("{0}/{1}", GameIO.GetSaveGameDir(), "factions.dat.bak"), true);
		}
		pooledExpandableMemoryStream.Position = 0L;
		StreamUtils.WriteStreamToFile(pooledExpandableMemoryStream, text);
		MemoryPools.poolMemoryStream.FreeSync(pooledExpandableMemoryStream);
		return -1;
	}

	public void Save()
	{
		if (this.dataSaveThreadInfo == null || !ThreadManager.ActiveThreads.ContainsKey("factionDataSave"))
		{
			PooledExpandableMemoryStream pooledExpandableMemoryStream = MemoryPools.poolMemoryStream.AllocSync(true);
			using (PooledBinaryWriter pooledBinaryWriter = MemoryPools.poolBinaryWriter.AllocSync(false))
			{
				pooledBinaryWriter.SetBaseStream(pooledExpandableMemoryStream);
				this.Write(pooledBinaryWriter);
			}
			this.dataSaveThreadInfo = ThreadManager.StartThread("factionDataSave", null, new ThreadManager.ThreadFunctionLoopDelegate(this.saveFactionDataThreaded), null, pooledExpandableMemoryStream, null, false, true);
		}
	}

	public void Load()
	{
		string text = string.Format("{0}/{1}", GameIO.GetSaveGameDir(), "factions.dat");
		if (SdDirectory.Exists(GameIO.GetSaveGameDir()) && SdFile.Exists(text))
		{
			try
			{
				using (Stream stream = SdFile.OpenRead(text))
				{
					using (PooledBinaryReader pooledBinaryReader = MemoryPools.poolBinaryReader.AllocSync(false))
					{
						pooledBinaryReader.SetBaseStream(stream);
						this.Read(pooledBinaryReader);
					}
				}
			}
			catch (Exception)
			{
				text = string.Format("{0}/{1}", GameIO.GetSaveGameDir(), "factions.dat.bak");
				if (SdFile.Exists(text))
				{
					using (Stream stream2 = SdFile.OpenRead(text))
					{
						using (PooledBinaryReader pooledBinaryReader2 = MemoryPools.poolBinaryReader.AllocSync(false))
						{
							pooledBinaryReader2.SetBaseStream(stream2);
							this.Read(pooledBinaryReader2);
						}
					}
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const float SAVE_TIME_SEC = 60f;

	public static FactionManager Instance;

	[PublicizedFrom(EAccessModifier.Private)]
	public static byte Version = 1;

	[PublicizedFrom(EAccessModifier.Private)]
	public Faction[] Factions = new Faction[255];

	[PublicizedFrom(EAccessModifier.Private)]
	public float saveTime = 60f;

	[PublicizedFrom(EAccessModifier.Private)]
	public ThreadManager.ThreadInfo dataSaveThreadInfo;

	[PublicizedFrom(EAccessModifier.Private)]
	public float rel;

	public enum Relationship
	{
		Hate,
		Dislike = 200,
		Neutral = 400,
		Like = 600,
		Love = 800,
		Leader = 1001
	}
}
