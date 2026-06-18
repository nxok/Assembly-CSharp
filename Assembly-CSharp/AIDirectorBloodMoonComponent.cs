using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class AIDirectorBloodMoonComponent : AIDirectorComponent
{
	public override void InitNewGame()
	{
		base.InitNewGame();
		int num = GameUtils.WorldTimeToDays(this.Director.World.worldTime);
		this.bmDayLast = (num - 1) / 7 * 7;
		this.CalcNextDay(false);
		this.ComputeDawnAndDuskTimes();
	}

	public override void Tick(double _dt)
	{
		base.Tick(_dt);
		World world = this.Director.World;
		bool flag = this.isBloodMoon;
		this.isBloodMoon = this.IsBloodMoonTime(world.worldTime);
		if (this.isBloodMoon != flag)
		{
			if (this.isBloodMoon)
			{
				this.StartBloodMoon();
			}
			else
			{
				this.EndBloodMoon();
			}
		}
		if (!this.isBloodMoon)
		{
			int @int = GameStats.GetInt(EnumGameStats.BloodMoonDay);
			if (@int != this.bmDay)
			{
				this.bmDay = @int;
				this.bmDayLast = @int - 1;
				Log.Warning("Blood Moon day stat changed {0}", new object[] { @int });
			}
		}
		if (this.isBloodMoon && GameStats.GetBool(EnumGameStats.IsSpawnEnemies))
		{
			this.delay -= (float)_dt;
			for (int i = 0; i < this.players.Count; i++)
			{
				EntityPlayer entityPlayer = this.players[i];
				if (entityPlayer.bloodMoonParty == null && entityPlayer.IsSpawned())
				{
					this.AddPlayerToParty(entityPlayer);
				}
			}
			for (int j = 0; j < this.parties.Count; j++)
			{
				if (this.nextParty >= this.parties.Count)
				{
					this.nextParty = 0;
				}
				AIDirectorBloodMoonParty aidirectorBloodMoonParty = this.parties[j];
				bool flag2 = j == this.nextParty && this.delay <= 0f;
				if (aidirectorBloodMoonParty.IsEmpty)
				{
					aidirectorBloodMoonParty.KillPartyZombies();
					if (flag2)
					{
						this.nextParty++;
					}
				}
				else if (aidirectorBloodMoonParty.Tick(world, _dt, flag2) && flag2)
				{
					this.delay = 1f / (float)this.parties.Count;
					this.nextParty++;
				}
			}
		}
	}

	public bool BloodMoonActive
	{
		get
		{
			return this.isBloodMoon;
		}
	}

	public bool SetForToday(bool _keepNextDay)
	{
		int num = GameUtils.WorldTimeToDays(this.Director.World.worldTime);
		if (num == this.bmDay)
		{
			return false;
		}
		if (_keepNextDay)
		{
			this.bmDayNextOverride = this.bmDay;
		}
		this.SetDay(num);
		return true;
	}

	public override void Read(BinaryReader _stream, int _version)
	{
		base.Read(_stream, _version);
		if (_version >= 8)
		{
			this.bmDayLast = _stream.ReadInt32();
			int num = _stream.ReadInt32();
			int num2 = (int)_stream.ReadInt16();
			int num3 = (int)_stream.ReadInt16();
			int bloodMoonFrequency = AIDirectorBloodMoonComponent.BloodMoonFrequency;
			int bloodMoonRange = AIDirectorBloodMoonComponent.BloodMoonRange;
			if (bloodMoonFrequency != num2 || bloodMoonRange != num3)
			{
				this.CalcNextDay(false);
			}
			else
			{
				this.SetDay(num);
			}
		}
		this.ComputeDawnAndDuskTimes();
	}

	public override void Write(BinaryWriter _stream)
	{
		base.Write(_stream);
		_stream.Write(this.bmDayLast);
		_stream.Write(this.bmDay);
		_stream.Write((short)AIDirectorBloodMoonComponent.BloodMoonFrequency);
		_stream.Write((short)AIDirectorBloodMoonComponent.BloodMoonRange);
	}

	public void AddPlayer(EntityPlayer _player)
	{
		this.players.Add(_player);
	}

	public void RemovePlayer(EntityPlayer _player)
	{
		if (this.players.Remove(_player))
		{
			for (int i = 0; i < this.parties.Count; i++)
			{
				this.parties[i].PlayerLoggedOut(_player);
			}
		}
	}

	public void TimeChanged(bool isSeek = false)
	{
		if (this.isBloodMoon && !this.IsBloodMoonTime(this.Director.World.worldTime))
		{
			this.EndBloodMoon();
		}
		if (this.bmDay != GameUtils.WorldTimeToElements(this.Director.World.worldTime).Item1 && !this.isBloodMoon && !this.IsBloodMoonTime(this.Director.World.worldTime))
		{
			this.CalcNextDay(isSeek);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void StartBloodMoon()
	{
		Log.Out("BloodMoon starting for day " + GameUtils.WorldTimeToDays(this.Director.World.worldTime).ToString());
		this.ClearParties();
		for (int i = 0; i < this.players.Count; i++)
		{
			this.players[i].IsBloodMoonDead = false;
		}
		this.delay = 0f;
		DictionaryList<int, Entity> entities = this.Director.World.Entities;
		for (int j = 0; j < entities.Count; j++)
		{
			EntityEnemy entityEnemy = entities.list[j] as EntityEnemy;
			if (entityEnemy != null)
			{
				entityEnemy.IsBloodMoon = true;
				entityEnemy.timeStayAfterDeath /= 3;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void EndBloodMoon()
	{
		Log.Out("Blood moon is over!");
		this.isBloodMoon = false;
		if (this.bmDayNextOverride > 0)
		{
			this.bmDay = this.bmDayNextOverride;
			this.bmDayNextOverride = 0;
			this.SetDay(this.bmDay);
		}
		if (GameUtils.WorldTimeToDays(this.Director.World.worldTime) > this.bmDay)
		{
			this.bmDayLast = this.bmDay;
			this.CalcNextDay(false);
		}
		this.ClearParties();
		DictionaryList<int, Entity> entities = this.Director.World.Entities;
		for (int i = 0; i < entities.Count; i++)
		{
			EntityEnemy entityEnemy = entities.list[i] as EntityEnemy;
			if (entityEnemy != null)
			{
				entityEnemy.bIsChunkObserver = false;
				entityEnemy.IsHordeZombie = false;
				entityEnemy.IsBloodMoon = false;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ClearParties()
	{
		this.nextParty = 0;
		this.parties.Clear();
		for (int i = 0; i < this.players.Count; i++)
		{
			this.players[i].bloodMoonParty = null;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void AddPlayerToParty(EntityPlayer _player)
	{
		for (int i = 0; i < this.parties.Count; i++)
		{
			AIDirectorBloodMoonParty aidirectorBloodMoonParty = this.parties[i];
			if (aidirectorBloodMoonParty.IsMemberOfParty(_player.entityId))
			{
				aidirectorBloodMoonParty.AddPlayer(_player);
				break;
			}
		}
		if (_player.bloodMoonParty == null)
		{
			int num = 0;
			while (num < this.parties.Count && !this.parties[num].TryAddPlayer(_player))
			{
				num++;
			}
		}
		if (_player.bloodMoonParty == null)
		{
			this.CreateNewParty(_player);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CreateNewParty(EntityPlayer _player)
	{
		this.parties.Add(new AIDirectorBloodMoonParty(_player, this, AIDirectorBloodMoonComponent.BloodMoonEnemyCount));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ComputeDawnAndDuskTimes()
	{
		ValueTuple<int, int> valueTuple = GameUtils.CalcDuskDawnHours(GameStats.GetInt(EnumGameStats.DayLightLength));
		this.duskHour = valueTuple.Item1;
		this.dawnHour = valueTuple.Item2;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool IsBloodMoonTime(ulong worldTime)
	{
		return GameUtils.IsBloodMoonTime(worldTime, new ValueTuple<int, int>(this.duskHour, this.dawnHour), this.bmDay);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CalcNextDay(bool isSeek = false)
	{
		int bloodMoonFrequency = AIDirectorBloodMoonComponent.BloodMoonFrequency;
		int num;
		if (bloodMoonFrequency <= 0)
		{
			num = 0;
		}
		else
		{
			int bloodMoonRange = AIDirectorBloodMoonComponent.BloodMoonRange;
			int num2 = bloodMoonFrequency + base.Random.RandomRange(0, bloodMoonRange + 1);
			int i = GameUtils.WorldTimeToDays(this.Director.World.worldTime);
			while (i <= this.bmDayLast)
			{
				this.bmDayLast -= num2;
			}
			if (this.bmDayLast < 0)
			{
				this.bmDayLast = 0;
			}
			num = this.bmDayLast;
			do
			{
				num += num2;
			}
			while (num < i);
			this.bmDayLast = num - num2;
			if (isSeek && this.bmDay > this.bmDayLast && this.bmDay <= this.bmDayLast + bloodMoonFrequency + bloodMoonRange)
			{
				num = this.bmDay;
			}
		}
		this.SetDay(num);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetDay(int day)
	{
		if (GameManager.Instance != null && GameManager.Instance.gameStateManager != null)
		{
			GameManager.Instance.gameStateManager.SetBloodMoonDay(day);
		}
		if (this.bmDay != day)
		{
			this.bmDay = day;
			Log.Out("BloodMoon SetDay: day {0}, last day {1}, freq {2}, range {3}", new object[]
			{
				this.bmDay,
				this.bmDayLast,
				AIDirectorBloodMoonComponent.BloodMoonFrequency,
				AIDirectorBloodMoonComponent.BloodMoonRange
			});
		}
	}

	public void LogBM(string format, params object[] args)
	{
		format = string.Format("{0} BM {1}", Time.frameCount, format);
		Log.Warning(format, args);
	}

	public const int cPartyEnemyMax = 30;

	public const int cTimeStayAfterDeathScale = 3;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cSpawnDelay = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public int bmDay;

	[PublicizedFrom(EAccessModifier.Private)]
	public int bmDayLast;

	[PublicizedFrom(EAccessModifier.Private)]
	public int bmDayNextOverride;

	[PublicizedFrom(EAccessModifier.Private)]
	public int dawnHour;

	[PublicizedFrom(EAccessModifier.Private)]
	public int duskHour;

	[PublicizedFrom(EAccessModifier.Private)]
	public int nextParty;

	[PublicizedFrom(EAccessModifier.Private)]
	public float delay;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isBloodMoon;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<AIDirectorBloodMoonParty> parties = new List<AIDirectorBloodMoonParty>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<EntityPlayer> players = new List<EntityPlayer>();

	public static int BloodMoonFrequency;

	public static int BloodMoonRange;

	public static int BloodMoonEnemyCount;
}
