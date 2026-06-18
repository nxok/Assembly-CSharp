using System;
using System.Collections.Generic;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_SpawnNearFriendsList : XUiC_List<XUiC_SpawnNearFriendsList.ListEntry>
{
	public static AllowSpawnNearFriend SpawnNearFriendMode
	{
		get
		{
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.CurrentMode == ProtocolManager.NetworkType.None)
			{
				return (AllowSpawnNearFriend)GamePrefs.GetInt(EnumGamePrefs.AllowSpawnNearFriend);
			}
			GameServerInfo currentGameServerInfoServerOrClient = SingletonMonoBehaviour<ConnectionManager>.Instance.CurrentGameServerInfoServerOrClient;
			if (currentGameServerInfoServerOrClient == null)
			{
				return AllowSpawnNearFriend.Always;
			}
			return (AllowSpawnNearFriend)currentGameServerInfoServerOrClient.GetValue(GameInfoInt.AllowSpawnNearFriend);
		}
	}

	public event Action<PersistentPlayerData> SpawnClicked;

	public override void Init()
	{
		base.Init();
		XUiC_SpawnNearFriendsList.Instance = this;
		XUiC_SimpleButton xuiC_SimpleButton = base.GetChildById("btnSpawnNearFriend") as XUiC_SimpleButton;
		if (xuiC_SimpleButton != null)
		{
			xuiC_SimpleButton.OnPressed += delegate(XUiController _, int _)
			{
				XUiC_SpawnNearFriendsList.ListEntry selectedEntryData = base.SelectedEntryData;
				if (selectedEntryData == null)
				{
					return;
				}
				Action<PersistentPlayerData> spawnClicked = this.SpawnClicked;
				if (spawnClicked == null)
				{
					return;
				}
				spawnClicked(selectedEntryData.PersistentPlayerData);
			};
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.RebuildList(false);
		PersistentPlayerList persistentPlayers = GameManager.Instance.persistentPlayers;
		ObservableDictionary<PlatformUserIdentifierAbs, PersistentPlayerData> observableDictionary = ((persistentPlayers != null) ? persistentPlayers.Players : null);
		if (observableDictionary != null)
		{
			observableDictionary.EntryAdded += this.playersDictChanged;
			observableDictionary.EntryModified += this.playersDictChanged;
			observableDictionary.EntryRemoved += this.playersDictChanged;
			observableDictionary.EntryUpdatedValue += this.playersDictChanged;
		}
	}

	public override void OnClose()
	{
		base.OnClose();
		PersistentPlayerList persistentPlayers = GameManager.Instance.persistentPlayers;
		ObservableDictionary<PlatformUserIdentifierAbs, PersistentPlayerData> observableDictionary = ((persistentPlayers != null) ? persistentPlayers.Players : null);
		if (observableDictionary != null)
		{
			observableDictionary.EntryAdded -= this.playersDictChanged;
			observableDictionary.EntryModified -= this.playersDictChanged;
			observableDictionary.EntryRemoved -= this.playersDictChanged;
			observableDictionary.EntryUpdatedValue -= this.playersDictChanged;
		}
	}

	public override void Cleanup()
	{
		base.Cleanup();
		XUiC_SpawnNearFriendsList.Instance = null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void playersDictChanged(object _sender, DictionaryChangedEventArgs<PlatformUserIdentifierAbs, PersistentPlayerData> _e)
	{
		this.UpdatePlayers();
	}

	public void UpdatePlayers()
	{
		XUiC_SpawnNearFriendsList.<>c__DisplayClass13_0 CS$<>8__locals1 = new XUiC_SpawnNearFriendsList.<>c__DisplayClass13_0();
		XUiC_SpawnNearFriendsList.<>c__DisplayClass13_0 CS$<>8__locals2 = CS$<>8__locals1;
		XUiC_SpawnNearFriendsList.ListEntry currentSelectedEntry = this.currentSelectedEntry;
		CS$<>8__locals2.previouslySelectedEntityId = ((currentSelectedEntry != null) ? currentSelectedEntry.PersistentPlayerData.EntityId : (-1));
		this.RebuildList(false);
		if (CS$<>8__locals1.previouslySelectedEntityId != -1)
		{
			base.SelectedEntryIndex = this.filteredEntries.FindIndex((XUiC_SpawnNearFriendsList.ListEntry _entry) => _entry.PersistentPlayerData.EntityId == CS$<>8__locals1.previouslySelectedEntityId);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string _value, string _bindingName)
	{
		if (_bindingName == "selectionValid")
		{
			XUiC_SpawnNearFriendsList.ListEntry currentSelectedEntry = this.currentSelectedEntry;
			_value = (currentSelectedEntry != null && currentSelectedEntry.ValidSpawn).ToString();
			return true;
		}
		if (!(_bindingName == "invalidSpawnReason"))
		{
			return base.GetBindingValueInternal(ref _value, _bindingName);
		}
		XUiC_SpawnNearFriendsList.ListEntry currentSelectedEntry2 = this.currentSelectedEntry;
		_value = ((currentSelectedEntry2 != null) ? currentSelectedEntry2.InvalidSpawnReason : null) ?? "";
		return true;
	}

	public override void RebuildList(bool _resetFilter = false)
	{
		this.allEntries.Clear();
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.CurrentMode == ProtocolManager.NetworkType.None)
		{
			for (int i = 0; i < 10; i++)
			{
				this.allEntries.Add(new XUiC_SpawnNearFriendsList.ListEntry(i + (int)(Time.unscaledTime * 1000f)));
			}
			this.allEntries.Sort();
		}
		if (GameManager.Instance.World == null)
		{
			base.RebuildList(_resetFilter);
			return;
		}
		PersistentPlayerList persistentPlayers = GameManager.Instance.persistentPlayers;
		if (persistentPlayers == null)
		{
			return;
		}
		bool value = SingletonMonoBehaviour<ConnectionManager>.Instance.CurrentGameServerInfoServerOrClient.GetValue(GameInfoBool.AllowCrossplay);
		foreach (KeyValuePair<PlatformUserIdentifierAbs, PersistentPlayerData> keyValuePair in persistentPlayers.Players)
		{
			PlatformUserIdentifierAbs platformUserIdentifierAbs;
			PersistentPlayerData persistentPlayerData;
			keyValuePair.Deconstruct(out platformUserIdentifierAbs, out persistentPlayerData);
			PersistentPlayerData persistentPlayerData2 = persistentPlayerData;
			IPlatformUserBlockedData platformUserBlockedData;
			if (persistentPlayerData2.EntityId != -1 && (!PlatformUserManager.GetOrCreate(persistentPlayerData2.PrimaryId).Blocked.TryGetValue(EBlockType.Play, out platformUserBlockedData) || !platformUserBlockedData.IsBlocked()))
			{
				this.allEntries.Add(new XUiC_SpawnNearFriendsList.ListEntry(persistentPlayerData2, value));
			}
		}
		this.allEntries.Sort();
		base.RebuildList(_resetFilter);
	}

	public static XUiC_SpawnNearFriendsList Instance;

	[Preserve]
	public class ListEntry : XUiListEntry<XUiC_SpawnNearFriendsList.ListEntry>
	{
		public bool IsFriend
		{
			get
			{
				return !string.IsNullOrEmpty(this.friendPlatform);
			}
		}

		public bool ValidSpawn
		{
			get
			{
				return this.IsFriend && this.locationAllowed;
			}
		}

		public string InvalidSpawnReason
		{
			get
			{
				if (!this.IsFriend)
				{
					return Localization.Get("xuiSpawnNearFriendNotAFriend", false, null);
				}
				if (this.locationAllowed)
				{
					return null;
				}
				return this.invalidLocationReason;
			}
		}

		public ListEntry(int _randomSeed)
		{
			GameRandom tempGameRandom = GameRandomManager.Instance.GetTempGameRandom(_randomSeed);
			int randomInt = tempGameRandom.RandomInt;
			this.name = randomInt.ToString();
			this.displayName = this.name;
			this.position = new Vector3i(tempGameRandom.RandomRange(-100, 101), 0, tempGameRandom.RandomRange(-100, 101));
			this.locationAllowed = randomInt / 2 % 2 == 0;
			this.friendPlatform = ((randomInt % 2 == 0) ? "X" : null);
			if (!this.locationAllowed)
			{
				this.invalidLocationReason = Localization.Get("xuiSpawnNearFriendNotInForest", false, null);
			}
			this.showIconCrossplay = randomInt % 2 == 0;
			this.iconCrossplaySprite = "ui_platform_pc";
		}

		public ListEntry(PersistentPlayerData _persistentPlayerData, bool _showPlatformIcons)
		{
			this.name = _persistentPlayerData.PlayerName.AuthoredName.Text;
			this.displayName = _persistentPlayerData.PlayerName.SafeDisplayName;
			this.PersistentPlayerData = _persistentPlayerData;
			PlayerData playerData = _persistentPlayerData.PlayerData;
			this.iconCrossplaySprite = PlatformManager.NativePlatform.Utils.GetCrossplayPlayerIcon(playerData.PlayGroup, true, playerData.NativeId.PlatformIdentifier);
			this.showIconCrossplay = _showPlatformIcons;
			DiscordManager.DiscordUser discordUser;
			if (DiscordManager.Instance.TryGetUserFromEntityId(_persistentPlayerData.EntityId, out discordUser) && discordUser.IsFriend)
			{
				this.friendPlatform = "Discord";
			}
			if (!PlatformUserIdentifierAbs.Equals(playerData.NativeId, playerData.PrimaryId) && playerData.PrimaryId != null && PlatformManager.MultiPlatform.User.IsFriend(playerData.PrimaryId) && this.friendPlatform == null)
			{
				this.friendPlatform = PlatformManager.GetPlatformDisplayName(playerData.PrimaryId.PlatformIdentifier);
			}
			if (PlatformManager.MultiPlatform.User.IsFriend(playerData.NativeId) && this.friendPlatform == null)
			{
				this.friendPlatform = PlatformManager.GetPlatformDisplayName(playerData.NativeId.PlatformIdentifier);
			}
			this.UpdatePosition();
		}

		public void UpdatePosition()
		{
			if (this.PersistentPlayerData == null)
			{
				return;
			}
			this.position = this.PersistentPlayerData.Position;
			World world = GameManager.Instance.World;
			this.biome = ((world != null) ? world.GetBiomeInWorld(this.position.x, this.position.z) : null);
			AllowSpawnNearFriend spawnNearFriendMode = XUiC_SpawnNearFriendsList.SpawnNearFriendMode;
			if (spawnNearFriendMode != AllowSpawnNearFriend.Always)
			{
				if (spawnNearFriendMode != AllowSpawnNearFriend.InForest)
				{
					this.locationAllowed = false;
					this.invalidLocationReason = Localization.Get("xuiSpawnNearFriendDisabled", false, null);
				}
				else
				{
					BiomeDefinition biomeDefinition = this.biome;
					BiomeDefinition.BiomeType? biomeType = ((biomeDefinition != null) ? new BiomeDefinition.BiomeType?(biomeDefinition.m_BiomeType) : null);
					bool flag;
					if (biomeType != null)
					{
						BiomeDefinition.BiomeType valueOrDefault = biomeType.GetValueOrDefault();
						if (valueOrDefault - BiomeDefinition.BiomeType.Forest <= 1)
						{
							flag = true;
							goto IL_00AD;
						}
					}
					flag = false;
					IL_00AD:
					this.locationAllowed = flag;
					if (!this.locationAllowed)
					{
						this.invalidLocationReason = Localization.Get("xuiSpawnNearFriendNotInForest", false, null);
						return;
					}
				}
				return;
			}
			this.locationAllowed = true;
			this.invalidLocationReason = string.Empty;
		}

		public override int CompareTo(XUiC_SpawnNearFriendsList.ListEntry _otherEntry)
		{
			if (_otherEntry == null)
			{
				return 1;
			}
			int num = this.IsFriend.CompareTo(_otherEntry.IsFriend);
			if (num != 0)
			{
				return -num;
			}
			num = this.ValidSpawn.CompareTo(_otherEntry.ValidSpawn);
			if (num != 0)
			{
				return -num;
			}
			return string.Compare(this.name, _otherEntry.name, StringComparison.OrdinalIgnoreCase);
		}

		public override bool MatchesSearch(string _searchString)
		{
			return this.name.IndexOf(_searchString, StringComparison.OrdinalIgnoreCase) >= 0;
		}

		public readonly PersistentPlayerData PersistentPlayerData;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly string name;

		public readonly string displayName;

		public readonly bool showIconCrossplay;

		public readonly string iconCrossplaySprite;

		public Vector3i position;

		public BiomeDefinition biome;

		public bool locationAllowed;

		[PublicizedFrom(EAccessModifier.Private)]
		public string invalidLocationReason;

		public readonly string friendPlatform;
	}

	[Preserve]
	[PublicizedFrom(EAccessModifier.Protected)]
	public class SpawnNearFriendsListEntryController : XUiC_List<XUiC_SpawnNearFriendsList.ListEntry>.XUiC_ListEntry
	{
		public override void Init()
		{
			base.Init();
			this.btnProfile = base.GetChildById("btnViewProfile");
			this.btnProfile.OnPress += delegate(XUiController _, int _)
			{
				XUiC_SpawnNearFriendsList.ListEntry entry = base.GetEntry();
				if (entry == null || entry.PersistentPlayerData == null)
				{
					return;
				}
				if (entry.PersistentPlayerData.NativeId != null && PlatformManager.MultiPlatform.User.CanShowProfile(entry.PersistentPlayerData.NativeId))
				{
					PlatformManager.MultiPlatform.User.ShowProfile(entry.PersistentPlayerData.NativeId);
					return;
				}
			};
		}

		[XuiXmlBinding("name")]
		[PublicizedFrom(EAccessModifier.Private)]
		public string bindingName()
		{
			XUiC_SpawnNearFriendsList.ListEntry entryData = this.entryData;
			return ((entryData != null) ? entryData.displayName : null) ?? "";
		}

		[XuiXmlBinding("position")]
		[PublicizedFrom(EAccessModifier.Private)]
		public string bindingPosition()
		{
			if (this.entryData != null)
			{
				return ValueDisplayFormatters.WorldPos(this.entryData.position, " ", false);
			}
			return "";
		}

		[XuiXmlBinding("biomeName")]
		[PublicizedFrom(EAccessModifier.Private)]
		public string bindingBiomeName()
		{
			XUiC_SpawnNearFriendsList.ListEntry entryData = this.entryData;
			string text;
			if (entryData == null)
			{
				text = null;
			}
			else
			{
				BiomeDefinition biome = entryData.biome;
				text = ((biome != null) ? biome.LocalizedName : null);
			}
			return text ?? "";
		}

		[XuiXmlBinding("locationAllowed")]
		[PublicizedFrom(EAccessModifier.Private)]
		public bool bindingLocationAllowed()
		{
			XUiC_SpawnNearFriendsList.ListEntry entryData = this.entryData;
			return entryData != null && entryData.locationAllowed;
		}

		[XuiXmlBinding("isFriend")]
		[PublicizedFrom(EAccessModifier.Private)]
		public bool bindingIsFriend()
		{
			XUiC_SpawnNearFriendsList.ListEntry entryData = this.entryData;
			return entryData != null && entryData.IsFriend;
		}

		[XuiXmlBinding("friendPlatform")]
		[PublicizedFrom(EAccessModifier.Private)]
		public string bindingFriendPlatform()
		{
			XUiC_SpawnNearFriendsList.ListEntry entryData = this.entryData;
			return ((entryData != null) ? entryData.friendPlatform : null) ?? "-";
		}

		[XuiXmlBinding("validSpawn")]
		[PublicizedFrom(EAccessModifier.Private)]
		public bool bindingValidSpawn()
		{
			XUiC_SpawnNearFriendsList.ListEntry entryData = this.entryData;
			return entryData != null && entryData.ValidSpawn;
		}

		[XuiXmlBinding("invalidSpawnReason")]
		[PublicizedFrom(EAccessModifier.Private)]
		public string bindingInvalidSpawnReason()
		{
			XUiC_SpawnNearFriendsList.ListEntry entryData = this.entryData;
			return ((entryData != null) ? entryData.InvalidSpawnReason : null) ?? "";
		}

		[XuiXmlBinding("showIconCrossplay")]
		[PublicizedFrom(EAccessModifier.Private)]
		public bool bindingShowIconCrossplay()
		{
			XUiC_SpawnNearFriendsList.ListEntry entryData = this.entryData;
			return entryData != null && entryData.showIconCrossplay;
		}

		[XuiXmlBinding("iconCrossplaySprite")]
		[PublicizedFrom(EAccessModifier.Private)]
		public string bindingIconCrossplaySprite()
		{
			XUiC_SpawnNearFriendsList.ListEntry entryData = this.entryData;
			return ((entryData != null) ? entryData.iconCrossplaySprite : null) ?? "";
		}

		[XuiXmlBinding("canShowProfile")]
		[PublicizedFrom(EAccessModifier.Private)]
		public bool bindingCanShowProfile()
		{
			XUiC_SpawnNearFriendsList.ListEntry entryData = this.entryData;
			return ((entryData != null) ? entryData.PersistentPlayerData : null) != null && PlatformManager.NativePlatform.User.CanShowProfile(this.entryData.PersistentPlayerData.PlayerData.NativeId);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public XUiController btnProfile;
	}
}
