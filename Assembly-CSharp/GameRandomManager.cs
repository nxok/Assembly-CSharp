using System;
using System.Diagnostics;
using UnityEngine;

public class GameRandomManager
{
	public static GameRandomManager Instance
	{
		get
		{
			if (GameRandomManager.instance == null)
			{
				GameRandomManager.instance = new GameRandomManager();
				GameRandomManager.instance.SetBaseSeed((int)Stopwatch.GetTimestamp());
				GameRandomManager.instance.tempRandom = new GameRandom();
			}
			return GameRandomManager.instance;
		}
	}

	public void SetBaseSeed(int _baseSeed)
	{
		this.baseSeed = _baseSeed;
	}

	public int BaseSeed
	{
		get
		{
			return this.baseSeed;
		}
	}

	public GameRandom CreateGameRandom()
	{
		return this.CreateGameRandom(this.baseSeed);
	}

	public GameRandom CreateGameRandom(int _seed)
	{
		GameRandom gameRandom = this.pool.AllocSync(false);
		gameRandom.SetSeed(_seed);
		return gameRandom;
	}

	public void FreeGameRandom(GameRandom _gameRandom)
	{
		if (_gameRandom == null)
		{
			return;
		}
		this.pool.FreeSync(_gameRandom);
	}

	public GameRandom GetTempGameRandom(int _seed)
	{
		this.tempRandom.SetSeed(_seed);
		return this.tempRandom;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void log(string _format, params object[] _values)
	{
		float num = -1f;
		if (ThreadManager.IsMainThread())
		{
			num = Time.time;
			if (num != GameRandomManager.logTime)
			{
				if (GameRandomManager.logCount > 10)
				{
					Log.Warning("GameRandomManager {0} more...", new object[] { GameRandomManager.logCount - 10 });
				}
				GameRandomManager.logTime = num;
				GameRandomManager.logCount = 0;
			}
			if (++GameRandomManager.logCount > 10)
			{
				return;
			}
		}
		Log.Warning(string.Format("{0} GameRandomManager ", num.ToCultureInvariantString()) + _format, _values);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static GameRandomManager instance;

	[PublicizedFrom(EAccessModifier.Private)]
	public int baseSeed;

	[PublicizedFrom(EAccessModifier.Private)]
	public MemoryPooledObject<GameRandom> pool = new MemoryPooledObject<GameRandom>(30);

	[PublicizedFrom(EAccessModifier.Private)]
	public GameRandom tempRandom;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cLogMax = 10;

	[PublicizedFrom(EAccessModifier.Private)]
	public static float logTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public static int logCount;
}
