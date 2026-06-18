using System;
using System.Collections.Generic;
using UnityEngine;

public class GameStateManager
{
	public GameStateManager(GameManager _gameManager)
	{
		this.bGameStarted = false;
		this.gameManager = _gameManager;
	}

	public void InitGame(bool _bServer)
	{
		this.bServer = _bServer;
		GameStats.Set(EnumGameStats.GameState, 1);
		Type type = Type.GetType(GamePrefs.GetString(EnumGamePrefs.GameMode));
		if (type == null)
		{
			type = Type.GetType((string)GamePrefs.GetDefault(EnumGamePrefs.GameMode));
		}
		this.currentGameMode = (GameMode)Activator.CreateInstance(type);
		GameStats.Set(EnumGameStats.GameModeId, this.currentGameMode.GetID());
		if (this.bServer)
		{
			GameStats.Set(EnumGameStats.CurrentRoundIx, 0);
			this.timeRoundStarted = Time.time;
			this.currentGameMode.Init();
			this.currentGameMode.StartRound(GameStats.GetInt(EnumGameStats.CurrentRoundIx));
			this.bDirty = true;
		}
	}

	public void StartGame()
	{
		this.bGameStarted = true;
		BacktraceUtils.StartStatisticsUpdate();
	}

	public bool IsGameStarted()
	{
		return this.bGameStarted;
	}

	public void EndGame()
	{
		GameStats.Set(EnumGameStats.GameState, 0);
		this.bDirty = true;
		this.bGameStarted = false;
		this.bServer = false;
	}

	public void SetBloodMoonDay(int day)
	{
		int @int = GameStats.GetInt(EnumGameStats.BloodMoonDay);
		if (day != @int)
		{
			GameStats.Set(EnumGameStats.BloodMoonDay, day);
			this.bDirty = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int nextRound()
	{
		int num = GameStats.GetInt(EnumGameStats.CurrentRoundIx);
		this.currentGameMode.EndRound(num);
		if (++num >= this.currentGameMode.GetRoundCount())
		{
			num = 0;
		}
		GameStats.Set(EnumGameStats.CurrentRoundIx, num);
		this.bDirty = true;
		this.timeRoundStarted = Time.time;
		return num;
	}

	public bool OnUpdateTick()
	{
		if (this.bServer)
		{
			if (GameStats.GetBool(EnumGameStats.TimeLimitActive))
			{
				int num = GameStats.GetInt(EnumGameStats.TimeLimitThisRound);
				if (num >= 0 && Time.time - this.timeRoundStarted >= 1f)
				{
					int num2 = (int)(Time.time - this.timeRoundStarted);
					this.timeRoundStarted = Time.time;
					num -= num2;
					GameStats.Set(EnumGameStats.TimeLimitThisRound, num);
					if (num < 0)
					{
						this.currentGameMode.StartRound(this.nextRound());
					}
					this.bDirty = true;
				}
			}
			if (GameStats.GetBool(EnumGameStats.DayLimitActive) && GameUtils.WorldTimeToDays(this.gameManager.World.worldTime) > GameStats.GetInt(EnumGameStats.DayLimitThisRound))
			{
				this.currentGameMode.StartRound(this.nextRound());
			}
			if (GameStats.GetBool(EnumGameStats.FragLimitActive))
			{
				int num3 = this.fragLimitCounter + 1;
				this.fragLimitCounter = num3;
				if (num3 > 40)
				{
					this.fragLimitCounter = 0;
					int @int = GameStats.GetInt(EnumGameStats.FragLimitThisRound);
					for (int i = 0; i < this.gameManager.World.Players.list.Count; i++)
					{
						if (this.gameManager.World.Players.list[i].KilledPlayers >= @int)
						{
							this.currentGameMode.StartRound(this.nextRound());
							break;
						}
					}
				}
			}
			if (GameTimer.Instance.ticks % 20UL == 0UL)
			{
				int num4 = 0;
				int num5 = 0;
				List<Entity> list = this.gameManager.World.Entities.list;
				for (int j = list.Count - 1; j >= 0; j--)
				{
					Entity entity = list[j];
					if (!entity.IsDead())
					{
						EntityClass entityClass = EntityClass.list[entity.entityClass];
						if (entityClass.bIsEnemyEntity)
						{
							num4++;
						}
						else if (entityClass.bIsAnimalEntity)
						{
							num5++;
						}
					}
				}
				GameStats.Set(EnumGameStats.EnemyCount, num4);
				GameStats.Set(EnumGameStats.AnimalCount, num5);
			}
			if (this.bDirty)
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageGameStats>().Setup(GameStats.Instance), true, -1, -1, -1, null, 192, false);
				this.bDirty = false;
			}
		}
		return true;
	}

	public string GetModeName()
	{
		if (this.currentGameMode == null)
		{
			return string.Empty;
		}
		if (this.currentGameMode.GetID() != this.lastGameModeID)
		{
			this.lastGameModeString = Localization.Get(this.currentGameMode.GetName(), false, null);
			this.lastGameModeID = this.currentGameMode.GetID();
		}
		return this.lastGameModeString;
	}

	public GameMode GetGameMode()
	{
		return this.currentGameMode;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public float timeRoundStarted;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bDirty;

	[PublicizedFrom(EAccessModifier.Private)]
	public GameMode currentGameMode;

	[PublicizedFrom(EAccessModifier.Private)]
	public GameManager gameManager;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bServer;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bGameStarted;

	[PublicizedFrom(EAccessModifier.Private)]
	public int fragLimitCounter;

	[PublicizedFrom(EAccessModifier.Private)]
	public int lastGameModeID = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lastGameModeString = string.Empty;
}
