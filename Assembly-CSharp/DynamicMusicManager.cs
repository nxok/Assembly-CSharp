using System;
using DynamicMusic.Legacy;
using DynamicMusic.Legacy.ObjectModel;

public class DynamicMusicManager : IGamePrefsChangedListener
{
	public bool MusicStarted
	{
		get
		{
			return this.IsMusicPlayingThisTick && !this.WasMusicPlayingLastTick;
		}
	}

	public bool MusicStopped
	{
		get
		{
			return !this.IsMusicPlayingThisTick && this.WasMusicPlayingLastTick;
		}
	}

	public bool IsDynamicMusicPlaying
	{
		get
		{
			return this.IsMusicPlayingThisTick;
		}
	}

	public bool IsBeforeDuskPlayBan
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return SkyManager.GetTimeOfDayAsMinutes() < SkyManager.GetDuskTimeAsMinutes() - DynamicMusicManager.PlayBanThreshold;
		}
	}

	public bool IsAfterDusk
	{
		get
		{
			return SkyManager.TimeOfDay() > SkyManager.GetDuskTime();
		}
	}

	public bool IsAfterDuskWindow
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return SkyManager.GetTimeOfDayAsMinutes() > SkyManager.GetDuskTimeAsMinutes() + DynamicMusicManager.deadWindow;
		}
	}

	public bool IsBeforeDawnPlayBan
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return SkyManager.GetTimeOfDayAsMinutes() < SkyManager.GetDawnTimeAsMinutes() - DynamicMusicManager.PlayBanThreshold;
		}
	}

	public bool IsAfterDawn
	{
		get
		{
			return SkyManager.TimeOfDay() > SkyManager.GetDawnTime();
		}
	}

	public bool IsAfterDawnWindow
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return SkyManager.GetTimeOfDayAsMinutes() > SkyManager.GetDawnTimeAsMinutes() + DynamicMusicManager.deadWindow;
		}
	}

	public bool IsInDeadWindow
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public bool IsPlayAllowed
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public float DistanceFromDeadWindow
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public bool IsPlayerInTraderStation
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public DynamicMusicManager()
	{
		this.UpdateConditions = default(DMSUpdateConditions);
		this.UpdateConditions.IsDMSEnabled = GamePrefs.GetBool(EnumGamePrefs.OptionsDynamicMusicEnabled);
		this.UpdateConditions.IsGameUnPaused = true;
	}

	public static void Init(EntityPlayerLocal _epLocal)
	{
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Initializing Dynamic Music System");
		_epLocal.DynamicMusicManager = new DynamicMusicManager();
		GamePrefs.AddChangeListener(_epLocal.DynamicMusicManager);
		_epLocal.DynamicMusicManager.PrimaryLocalPlayer = _epLocal;
		DynamicMusicManager.Random = GameRandomManager.Instance.CreateGameRandom();
		ThreatLevelTracker.Init(_epLocal.DynamicMusicManager);
		FrequencyManager.Init(_epLocal.DynamicMusicManager);
		StreamerMaster.Init(_epLocal.DynamicMusicManager);
		TransitionManager.Init(_epLocal.DynamicMusicManager);
		_epLocal.DynamicMusicManager.UpdateConditions.IsDMSInitialized = true;
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Finished initializing Dynamic Music System");
	}

	public void Tick()
	{
		if (this.UpdateConditions.CanUpdate)
		{
			if (StreamerMaster.currentStreamer != null)
			{
				this.IsMusicPlayingThisTick = StreamerMaster.currentStreamer.IsPlaying;
			}
			if (this.IsAfterDusk)
			{
				this.IsInDeadWindow = !(this.IsPlayAllowed = this.IsAfterDuskWindow);
				this.DistanceFromDeadWindow = (float)GamePrefs.GetInt(EnumGamePrefs.DayNightLength) - SkyManager.GetTimeOfDayAsMinutes() + SkyManager.GetDawnTimeAsMinutes();
			}
			else if (this.IsAfterDawn)
			{
				if (this.IsAfterDawnWindow)
				{
					this.DistanceFromDeadWindow = Utils.FastMax(SkyManager.GetDuskTimeAsMinutes() - DynamicMusicManager.deadWindow - SkyManager.GetTimeOfDayAsMinutes(), 0f);
					if (this.IsBeforeDuskPlayBan)
					{
						this.IsInDeadWindow = false;
						this.IsPlayAllowed = true;
					}
					else
					{
						this.IsPlayAllowed = false;
						this.IsInDeadWindow = this.DistanceFromDeadWindow == 0f;
					}
				}
			}
			else
			{
				this.DistanceFromDeadWindow = Utils.FastMax(SkyManager.GetDawnTimeAsMinutes() - DynamicMusicManager.deadWindow - SkyManager.GetTimeOfDayAsMinutes(), 0f);
				if (this.IsBeforeDawnPlayBan)
				{
					this.IsPlayAllowed = true;
					this.IsInDeadWindow = false;
				}
				else
				{
					this.IsPlayAllowed = false;
					this.IsInDeadWindow = this.DistanceFromDeadWindow == 0f;
				}
			}
			this.IsPlayerInTraderStation = this.IsPrimaryPlayerInTraderStation();
			this.ThreatLevelTracker.Tick();
			this.FrequencyManager.Tick();
			this.TransitionManager.Tick();
			this.StreamerMaster.Tick();
			this.WasMusicPlayingLastTick = this.IsMusicPlayingThisTick;
		}
	}

	public void CleanUpDynamicMembers()
	{
		if (this.StreamerMaster != null)
		{
			this.StreamerMaster.Cleanup();
		}
		this.UpdateConditions.IsDMSInitialized = false;
	}

	public static void Cleanup()
	{
		MusicGroup.Cleanup();
		ConfigSet.Cleanup();
	}

	public void Event(MinEventTypes _eventType, MinEventParams _eventParms)
	{
		if (_eventType <= MinEventTypes.onSelfDied)
		{
			switch (_eventType)
			{
			case MinEventTypes.onOtherDamagedSelf:
				this.ThreatLevelTracker.Event(_eventType, _eventParms);
				return;
			case MinEventTypes.onOtherAttackedSelf:
				this.ThreatLevelTracker.Event(_eventType, _eventParms);
				return;
			case MinEventTypes.onOtherHealedSelf:
				break;
			case MinEventTypes.onSelfDamagedOther:
				this.ThreatLevelTracker.Event(_eventType, _eventParms);
				return;
			case MinEventTypes.onSelfAttackedOther:
				this.ThreatLevelTracker.Event(_eventType, _eventParms);
				return;
			default:
				if (_eventType != MinEventTypes.onSelfDied)
				{
					return;
				}
				Log.Out("DMS Died!");
				this.UpdateConditions.DoesPlayerExist = false;
				return;
			}
		}
		else
		{
			switch (_eventType)
			{
			case MinEventTypes.onSelfRespawn:
				Log.Out("DMS Respawn!");
				this.UpdateConditions.DoesPlayerExist = true;
				return;
			case MinEventTypes.onSelfLeaveGame:
				Log.Out("DMS Left Game!");
				return;
			case MinEventTypes.onSelfEnteredGame:
				Log.Out("DMS Entered Game!");
				this.UpdateConditions.DoesPlayerExist = true;
				break;
			default:
				if (_eventType != MinEventTypes.onSelfEnteredBiome)
				{
					return;
				}
				break;
			}
		}
	}

	public void OnPlayerDeath()
	{
		this.StreamerMaster.Stop();
	}

	public void OnPlayerFirstSpawned()
	{
		this.FrequencyManager.OnPlayerFirstSpawned();
	}

	public void Pause()
	{
		this.UpdateConditions.IsGameUnPaused = false;
		this.StreamerMaster.Pause();
		this.FrequencyManager.OnPause();
	}

	public void UnPause()
	{
		this.UpdateConditions.IsGameUnPaused = true;
		this.StreamerMaster.UnPause();
		this.FrequencyManager.OnUnPause();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool IsPrimaryPlayerInTraderStation()
	{
		return GameManager.Instance.World.IsWithinTraderArea(this.PrimaryLocalPlayer.GetBlockPosition());
	}

	public bool IsInDawnOrDuskRange(float _dawnOrDuskTime, float _currentTime)
	{
		return this.DistanceFromDawnOrDusk(_dawnOrDuskTime, _currentTime) <= DynamicMusicManager.deadWindow;
	}

	public float DistanceFromDawnOrDusk(float _dawnOrDuskTime, float _currentTime)
	{
		return Math.Abs(_dawnOrDuskTime - _currentTime);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void IGamePrefsChangedListener.OnGamePrefChanged(EnumGamePrefs _enum)
	{
		if (_enum == EnumGamePrefs.OptionsDynamicMusicEnabled && !(this.UpdateConditions.IsDMSEnabled = GamePrefs.GetBool(EnumGamePrefs.OptionsDynamicMusicEnabled)))
		{
			this.StreamerMaster.Stop();
		}
	}

	public float TimeToNextDayEvent
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return (SkyManager.IsDark() ? SkyManager.GetDawnTimeAsMinutes() : SkyManager.GetDuskTimeAsMinutes()) - SkyManager.GetTimeOfDayAsMinutes();
		}
	}

	public EntityPlayerLocal PrimaryLocalPlayer;

	public ThreatLevelTracker ThreatLevelTracker;

	public FrequencyManager FrequencyManager;

	public TransitionManager TransitionManager;

	public StreamerMaster StreamerMaster;

	public bool IsMusicPlayingThisTick;

	public bool WasMusicPlayingLastTick;

	public static readonly float PlayBanThreshold = 1f;

	public static readonly float deadWindow = 0.16666667f;

	public static GameRandom Random;

	public DMSUpdateConditions UpdateConditions;
}
