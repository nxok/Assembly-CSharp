using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;

public static class ModEvents
{
	public static readonly ModEvents.ModEvent<ModEvents.SGameFocusData> GameFocus = new ModEvents.ModEvent<ModEvents.SGameFocusData>("GameFocus");

	public static readonly ModEvents.ModEvent<ModEvents.SGameAwakeData> GameAwake = new ModEvents.ModEvent<ModEvents.SGameAwakeData>("GameAwake");

	public static readonly ModEvents.ModEvent<ModEvents.SGameStartingData> GameStarting = new ModEvents.ModEvent<ModEvents.SGameStartingData>("GameStarting");

	public static readonly ModEvents.ModEventInterruptible<ModEvents.SMainMenuOpeningData> MainMenuOpening = new ModEvents.ModEventInterruptible<ModEvents.SMainMenuOpeningData>("MainMenuOpening");

	public static readonly ModEvents.ModEvent<ModEvents.SMainMenuOpenedData> MainMenuOpened = new ModEvents.ModEvent<ModEvents.SMainMenuOpenedData>("MainMenuOpened");

	public static readonly ModEvents.ModEvent<ModEvents.SGameStartDoneData> GameStartDone = new ModEvents.ModEvent<ModEvents.SGameStartDoneData>("GameStartDone");

	public static readonly ModEvents.ModEvent<ModEvents.SCreateWorldDoneData> CreateWorldDone = new ModEvents.ModEvent<ModEvents.SCreateWorldDoneData>("CreateWorldDone");

	public static readonly ModEvents.ModEvent<ModEvents.SGameUpdateData> GameUpdate = new ModEvents.ModEvent<ModEvents.SGameUpdateData>("GameUpdate");

	public static readonly ModEvents.ModEvent<ModEvents.SWorldShuttingDownData> WorldShuttingDown = new ModEvents.ModEvent<ModEvents.SWorldShuttingDownData>("WorldShuttingDown");

	public static readonly ModEvents.ModEvent<ModEvents.SGameShutdownData> GameShutdown = new ModEvents.ModEvent<ModEvents.SGameShutdownData>("GameShutdown");

	public static readonly ModEvents.ModEvent<ModEvents.SServerRegisteredData> ServerRegistered = new ModEvents.ModEvent<ModEvents.SServerRegisteredData>("ServerRegistered");

	public static readonly ModEvents.ModEvent<ModEvents.SUnityUpdateData> UnityUpdate = new ModEvents.ModEvent<ModEvents.SUnityUpdateData>("UnityUpdate");

	public static readonly ModEvents.ModEventInterruptible<ModEvents.SPlayerLoginData> PlayerLogin = new ModEvents.ModEventInterruptible<ModEvents.SPlayerLoginData>("PlayerLogin");

	public static readonly ModEvents.ModEvent<ModEvents.SPlayerJoinedGameData> PlayerJoinedGame = new ModEvents.ModEvent<ModEvents.SPlayerJoinedGameData>("PlayerJoinedGame");

	public static readonly ModEvents.ModEvent<ModEvents.SPlayerSpawningData> PlayerSpawning = new ModEvents.ModEvent<ModEvents.SPlayerSpawningData>("PlayerSpawning");

	public static readonly ModEvents.ModEvent<ModEvents.SPlayerSpawnedInWorldData> PlayerSpawnedInWorld = new ModEvents.ModEvent<ModEvents.SPlayerSpawnedInWorldData>("PlayerSpawnedInWorld");

	public static readonly ModEvents.ModEvent<ModEvents.SPlayerDisconnectedData> PlayerDisconnected = new ModEvents.ModEvent<ModEvents.SPlayerDisconnectedData>("PlayerDisconnected");

	public static readonly ModEvents.ModEvent<ModEvents.SSavePlayerDataData> SavePlayerData = new ModEvents.ModEvent<ModEvents.SSavePlayerDataData>("SavePlayerData");

	public static readonly ModEvents.ModEventInterruptible<ModEvents.SGameMessageData> GameMessage = new ModEvents.ModEventInterruptible<ModEvents.SGameMessageData>("GameMessage");

	public static readonly ModEvents.ModEventInterruptible<ModEvents.SChatMessageData> ChatMessage = new ModEvents.ModEventInterruptible<ModEvents.SChatMessageData>("ChatMessage");

	public static readonly ModEvents.ModEvent<ModEvents.SCalcChunkColorsDoneData> CalcChunkColorsDone = new ModEvents.ModEvent<ModEvents.SCalcChunkColorsDoneData>("CalcChunkColorsDone");

	public static readonly ModEvents.ModEvent<ModEvents.SEntityKilledData> EntityKilled = new ModEvents.ModEvent<ModEvents.SEntityKilledData>("EntityKilled");

	public struct SGameFocusData
	{
		public SGameFocusData(bool _isFocused)
		{
			this.IsFocused = _isFocused;
		}

		public readonly bool IsFocused;
	}

	public struct SGameAwakeData
	{
	}

	public struct SGameStartingData
	{
		public SGameStartingData(bool _asServer)
		{
			this.AsServer = _asServer;
		}

		public readonly bool AsServer;
	}

	public struct SMainMenuOpeningData
	{
		public SMainMenuOpeningData(bool _openedBefore)
		{
			this.OpenedBefore = _openedBefore;
		}

		public readonly bool OpenedBefore;
	}

	public struct SMainMenuOpenedData
	{
		public SMainMenuOpenedData(bool _firstTimeOpen)
		{
			this.FirstTimeOpen = _firstTimeOpen;
		}

		public readonly bool FirstTimeOpen;
	}

	public struct SGameStartDoneData
	{
	}

	public struct SCreateWorldDoneData
	{
	}

	public struct SGameUpdateData
	{
	}

	public struct SWorldShuttingDownData
	{
	}

	public struct SGameShutdownData
	{
	}

	public struct SServerRegisteredData
	{
	}

	public struct SUnityUpdateData
	{
	}

	public struct SPlayerLoginData
	{
		public SPlayerLoginData(ClientInfo _clientInfo, string _compatibilityVersion)
		{
			this.ClientInfo = _clientInfo;
			this.CompatibilityVersion = _compatibilityVersion;
			this.CustomMessage = null;
		}

		public readonly ClientInfo ClientInfo;

		public readonly string CompatibilityVersion;

		public string CustomMessage;
	}

	public struct SPlayerJoinedGameData
	{
		public SPlayerJoinedGameData(ClientInfo _clientInfo)
		{
			this.ClientInfo = _clientInfo;
		}

		public readonly ClientInfo ClientInfo;
	}

	public readonly struct SPlayerSpawningData
	{
		public SPlayerSpawningData(ClientInfo _clientInfo, int _chunkViewDim, PlayerProfile _playerProfile)
		{
			this.ClientInfo = _clientInfo;
			this.ChunkViewDim = _chunkViewDim;
			this.PlayerProfile = _playerProfile;
		}

		public readonly ClientInfo ClientInfo;

		public readonly int ChunkViewDim;

		public readonly PlayerProfile PlayerProfile;
	}

	public struct SPlayerSpawnedInWorldData
	{
		public SPlayerSpawnedInWorldData(ClientInfo _clientInfo, bool _isLocalPlayer, int _entityId, RespawnType _respawnType, Vector3i _position)
		{
			this.ClientInfo = _clientInfo;
			this.IsLocalPlayer = _isLocalPlayer;
			this.EntityId = _entityId;
			this.RespawnType = _respawnType;
			this.Position = _position;
		}

		public readonly ClientInfo ClientInfo;

		public readonly bool IsLocalPlayer;

		public readonly int EntityId;

		public readonly RespawnType RespawnType;

		public readonly Vector3i Position;
	}

	public struct SPlayerDisconnectedData
	{
		public SPlayerDisconnectedData(ClientInfo _clientInfo, bool _gameShuttingDown)
		{
			this.ClientInfo = _clientInfo;
			this.GameShuttingDown = _gameShuttingDown;
		}

		public readonly ClientInfo ClientInfo;

		public readonly bool GameShuttingDown;
	}

	public struct SSavePlayerDataData
	{
		public SSavePlayerDataData(ClientInfo _clientInfo, PlayerDataFile _playerDataFile)
		{
			this.ClientInfo = _clientInfo;
			this.PlayerDataFile = _playerDataFile;
		}

		public readonly ClientInfo ClientInfo;

		public readonly PlayerDataFile PlayerDataFile;
	}

	public struct SGameMessageData
	{
		public SGameMessageData(ClientInfo _clientInfo, EnumGameMessages _messageType, string _mainName, string _secondaryName)
		{
			this.ClientInfo = _clientInfo;
			this.MessageType = _messageType;
			this.MainName = _mainName;
			this.SecondaryName = _secondaryName;
		}

		public readonly ClientInfo ClientInfo;

		public readonly EnumGameMessages MessageType;

		public readonly string MainName;

		public readonly string SecondaryName;
	}

	public struct SChatMessageData
	{
		public SChatMessageData(ClientInfo _clientInfo, EChatType _chatType, int _senderEntityId, string _message, string _mainName, List<int> _recipientEntityIds)
		{
			this.ClientInfo = _clientInfo;
			this.ChatType = _chatType;
			this.SenderEntityId = _senderEntityId;
			this.Message = _message;
			this.MainName = _mainName;
			this.RecipientEntityIds = _recipientEntityIds;
		}

		public readonly ClientInfo ClientInfo;

		public readonly EChatType ChatType;

		public readonly int SenderEntityId;

		public readonly string Message;

		public readonly string MainName;

		public readonly List<int> RecipientEntityIds;
	}

	public struct SCalcChunkColorsDoneData
	{
		public SCalcChunkColorsDoneData(Chunk _chunk)
		{
			this.Chunk = _chunk;
		}

		public readonly Chunk Chunk;
	}

	public struct SEntityKilledData
	{
		public SEntityKilledData(Entity _killedEntitiy, Entity _killingEntity)
		{
			this.KilledEntitiy = _killedEntitiy;
			this.KillingEntity = _killingEntity;
		}

		public readonly Entity KilledEntitiy;

		public readonly Entity KillingEntity;
	}

	public enum EModEventResult
	{
		Continue,
		StopHandlersRunVanilla,
		StopHandlersAndVanilla
	}

	public delegate void ModEventHandlerDelegate<TData>(ref TData _data) where TData : struct;

	public delegate ModEvents.EModEventResult ModEventInterruptibleHandlerDelegate<TData>(ref TData _data) where TData : struct;

	public abstract class ModEventAbs<TDelegate> where TDelegate : Delegate
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		public ModEventAbs(string _eventName)
		{
			this.eventName = _eventName;
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public void RegisterHandler(TDelegate _handlerFunc)
		{
			Assembly callingAssembly = Assembly.GetCallingAssembly();
			Assembly assembly = typeof(ModEvents).Assembly;
			bool flag = false;
			Mod mod = null;
			if (callingAssembly.Equals(assembly))
			{
				flag = true;
			}
			else
			{
				mod = ModManager.GetModForAssembly(callingAssembly);
				if (mod == null)
				{
					Log.Warning("[MODS] Could not find mod that tries to register a handler for event " + this.eventName);
				}
			}
			this.receivers.Add(new ModEvents.ModEventAbs<TDelegate>.Receiver(mod, _handlerFunc, flag));
		}

		public void UnregisterHandler(TDelegate _handlerFunc)
		{
			for (int i = this.receivers.Count - 1; i >= 0; i--)
			{
				if (this.receivers[i].DelegateFunc.Equals(_handlerFunc))
				{
					this.receivers.RemoveAt(i);
				}
			}
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public void LogError(Exception _e, ModEvents.ModEventAbs<TDelegate>.Receiver _currentMod)
		{
			Log.Error(string.Concat(new string[] { "[MODS] Error while executing ModEvent \"", this.eventName, "\" on mod \"", _currentMod.ModName, "\"" }));
			Log.Exception(_e);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public readonly string eventName;

		[PublicizedFrom(EAccessModifier.Protected)]
		public readonly List<ModEvents.ModEventAbs<TDelegate>.Receiver> receivers = new List<ModEvents.ModEventAbs<TDelegate>.Receiver>();

		[PublicizedFrom(EAccessModifier.Protected)]
		public class Receiver
		{
			public string ModName
			{
				get
				{
					if (this.Mod != null)
					{
						return this.Mod.Name;
					}
					if (this.coreGame)
					{
						return "-GameCore-";
					}
					return "-UnknownMod-";
				}
			}

			public Receiver(Mod _mod, TDelegate _handler, bool _coreGame = false)
			{
				this.Mod = _mod;
				this.DelegateFunc = _handler;
				this.coreGame = _coreGame;
			}

			public readonly Mod Mod;

			public readonly TDelegate DelegateFunc;

			[PublicizedFrom(EAccessModifier.Private)]
			public readonly bool coreGame;
		}
	}

	public class ModEvent<TData> : ModEvents.ModEventAbs<ModEvents.ModEventHandlerDelegate<TData>> where TData : struct
	{
		public ModEvent([CallerMemberName] string _eventName = null)
			: base(_eventName)
		{
		}

		public void Invoke(ref TData _data)
		{
			foreach (ModEvents.ModEventAbs<ModEvents.ModEventHandlerDelegate<TData>>.Receiver receiver in this.receivers)
			{
				try
				{
					receiver.DelegateFunc(ref _data);
				}
				catch (Exception ex)
				{
					base.LogError(ex, receiver);
				}
			}
		}
	}

	public class ModEventInterruptible<TData> : ModEvents.ModEventAbs<ModEvents.ModEventInterruptibleHandlerDelegate<TData>> where TData : struct
	{
		public ModEventInterruptible([CallerMemberName] string _eventName = null)
			: base(_eventName)
		{
		}

		public ValueTuple<ModEvents.EModEventResult, Mod> Invoke(ref TData _data)
		{
			foreach (ModEvents.ModEventAbs<ModEvents.ModEventInterruptibleHandlerDelegate<TData>>.Receiver receiver in this.receivers)
			{
				try
				{
					ModEvents.EModEventResult emodEventResult = receiver.DelegateFunc(ref _data);
					if (emodEventResult != ModEvents.EModEventResult.Continue)
					{
						return new ValueTuple<ModEvents.EModEventResult, Mod>(emodEventResult, receiver.Mod);
					}
				}
				catch (Exception ex)
				{
					base.LogError(ex, receiver);
				}
			}
			return new ValueTuple<ModEvents.EModEventResult, Mod>(ModEvents.EModEventResult.Continue, null);
		}
	}
}
