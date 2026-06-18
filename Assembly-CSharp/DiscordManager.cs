using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using Audio;
using Discord.Sdk;
using Newtonsoft.Json;
using Platform;
using Twitch;
using UnityEngine;
using UnityEngine.Networking;

public class DiscordManager
{
	public static DiscordManager Instance
	{
		get
		{
			DiscordManager discordManager;
			if ((discordManager = DiscordManager.instance) == null)
			{
				discordManager = (DiscordManager.instance = new DiscordManager());
			}
			return discordManager;
		}
	}

	public event DiscordManager.UserAuthorizationResultCallback UserAuthorizationResult;

	public event Action<DiscordManager.EDiscordStatus> StatusChanged;

	public event DiscordManager.LocalUserChangedCallback LocalUserChanged;

	public event DiscordManager.LobbyStateChangedCallback LobbyStateChanged;

	public event DiscordManager.LobbyMembersChangedCallback LobbyMembersChanged;

	public event DiscordManager.CallChangedCallback CallChanged;

	public event DiscordManager.CallStatusChangedCallback CallStatusChanged;

	public event DiscordManager.CallMembersChangedCallback CallMembersChanged;

	public event DiscordManager.VoiceStateChangedCallback VoiceStateChanged;

	public event DiscordManager.SelfMuteStateChangedCallback SelfMuteStateChanged;

	public event DiscordManager.FriendsListChangedCallback FriendsListChanged;

	public event DiscordManager.RelationshipChangedCallback RelationshipChanged;

	public event DiscordManager.ActivityInviteReceivedCallback ActivityInviteReceived;

	public event DiscordManager.ActivityJoiningCallback ActivityJoining;

	public event DiscordManager.PendingActionsUpdateCallback PendingActionsUpdate;

	public event DiscordManager.AudioDevicesChangedCallback AudioDevicesChanged;

	public DiscordManager.EDiscordStatus Status
	{
		get
		{
			if (this.client == null)
			{
				return DiscordManager.EDiscordStatus.NotInitialized;
			}
			DiscordManager.EDiscordStatus ediscordStatus;
			switch (this.client.GetStatus())
			{
			case 0:
				ediscordStatus = DiscordManager.EDiscordStatus.Disconnected;
				break;
			case 1:
				ediscordStatus = DiscordManager.EDiscordStatus.Connecting;
				break;
			case 2:
				ediscordStatus = DiscordManager.EDiscordStatus.Connecting;
				break;
			case 3:
				ediscordStatus = DiscordManager.EDiscordStatus.Ready;
				break;
			case 4:
				ediscordStatus = DiscordManager.EDiscordStatus.Connecting;
				break;
			case 5:
				ediscordStatus = DiscordManager.EDiscordStatus.Disconnecting;
				break;
			default:
				ediscordStatus = DiscordManager.EDiscordStatus.Disconnected;
				break;
			}
			return ediscordStatus;
		}
	}

	public bool IsReady
	{
		get
		{
			return this.Status == DiscordManager.EDiscordStatus.Ready;
		}
	}

	public bool IsInitialized
	{
		get
		{
			return this.Status > DiscordManager.EDiscordStatus.NotInitialized;
		}
	}

	public DiscordManager.DiscordUser LocalUser
	{
		get
		{
			return this.localUser;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			if (this.localUser == value)
			{
				return;
			}
			this.localUser = value;
			this.localDiscordOrEntityIdChanged();
			DiscordManager.LocalUserChangedCallback localUserChanged = this.LocalUserChanged;
			if (localUserChanged != null)
			{
				localUserChanged(this.localUser != null);
			}
			this.refreshCachedUserHandlesAndRelationships();
		}
	}

	public static LoggingSeverity LogLevel
	{
		get
		{
			return DiscordManager.logLevel;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public DiscordManager()
	{
		this.setLogLevelsFromCmdLine();
		if (!DiscordManager.SupportsProvisionalAccounts)
		{
			Log.Warning("[Discord] Full Discord integration only available when running with EOS cross platform provider!");
		}
		this.Settings = DiscordManager.DiscordSettings.Load();
		this.userMappings = new DiscordManager.DiscordUserMappingManager(this);
		this.AuthManager = new DiscordManager.AuthAndLoginManager(this);
		this.Presence = new DiscordManager.PresenceManager(this);
		this.globalLobby = new DiscordManager.LobbyInfo(this, DiscordManager.ELobbyType.Global);
		this.partyLobby = new DiscordManager.LobbyInfo(this, DiscordManager.ELobbyType.Party);
		this.AudioOutput = new DiscordManager.AudioDeviceConfig(this, true);
		this.AudioInput = new DiscordManager.AudioDeviceConfig(this, false);
		this.UserSettings = DiscordManager.DiscordUserSettingsManager.Load();
		this.registerGameEventHandlers();
		this.ActivityInviteReceived += delegate(DiscordManager.DiscordUser _, bool _, ActivityActionTypes _)
		{
			this.updatePendingActionsEvent();
		};
		this.RelationshipChanged += delegate(DiscordManager.DiscordUser _)
		{
			this.updatePendingActionsEvent();
		};
		this.FriendsListChanged += this.updatePendingActionsEvent;
		if (GameManager.IsDedicatedServer || !this.Settings.DiscordFirstTimeInfoShown || this.Settings.DiscordDisabled)
		{
			return;
		}
		this.Init(false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setLogLevelsFromCmdLine()
	{
		string text = GameUtils.GetLaunchArgument("discordloglevel");
		LoggingSeverity loggingSeverity;
		if (text != null && EnumUtils.TryParse<LoggingSeverity>(text, out loggingSeverity, true))
		{
			DiscordManager.logLevel = loggingSeverity;
		}
		text = GameUtils.GetLaunchArgument("discordloglevelrtc");
		LoggingSeverity loggingSeverity2;
		if (text != null && EnumUtils.TryParse<LoggingSeverity>(text, out loggingSeverity2, true))
		{
			DiscordManager.logLevelRtc = loggingSeverity2;
		}
	}

	public void Init(bool _forceReinit = false)
	{
		if (!VoiceHelpers.VoiceAllowed || !PermissionsManager.IsMultiplayerAllowed())
		{
			return;
		}
		if (this.client != null)
		{
			if (!_forceReinit)
			{
				return;
			}
			this.client.Disconnect();
		}
		else
		{
			NativeMethods.UnhandledException += DiscordManager.nativeMethodException;
			Log.Out(string.Format("[Discord] Initializing, version {0}.{1}.{2}, # {3}", new object[]
			{
				Client.GetVersionMajor(),
				Client.GetVersionMinor(),
				Client.GetVersionPatch(),
				Client.GetVersionHash()
			}));
			this.client = new Client();
			this.client.SetApplicationId(1296840202995896363UL);
			this.client.SetLogDir("", 5);
			this.client.SetVoiceLogDir("", 5);
			this.client.AddLogCallback(new Client.LogCallback(this.OnDiscordLogMessageReceived), DiscordManager.logLevel);
			this.client.AddVoiceLogCallback(new Client.LogCallback(this.OnDiscordRtcLogMessageReceived), DiscordManager.logLevelRtc);
			this.registerGameStartupForInvites();
		}
		this.client.UpdateToken(1, "", delegate(ClientResult _)
		{
		});
		this.registerGlobalDiscordCallbacks();
		this.client.SetAutomaticGainControl(true);
		this.client.SetEchoCancellation(true);
		this.client.SetNoiseSuppression(true);
		this.client.SetOutputVolume((float)this.Settings.OutputVolume);
		this.client.SetInputVolume((float)this.Settings.InputVolume);
		this.updateAudioDeviceList();
		this.Settings.OutputDeviceChanged += delegate(string _)
		{
			this.AudioOutput.UpdateAudioDeviceList();
		};
		this.Settings.InputDeviceChanged += delegate(string _)
		{
			this.AudioInput.UpdateAudioDeviceList();
		};
		this.Settings.OutputVolumeChanged += delegate(int _v)
		{
			Client client = this.client;
			if (client == null)
			{
				return;
			}
			client.SetOutputVolume((float)_v);
		};
		this.Settings.InputVolumeChanged += delegate(int _v)
		{
			Client client2 = this.client;
			if (client2 == null)
			{
				return;
			}
			client2.SetInputVolume((float)_v);
		};
		this.Settings.VoiceModePttChanged += delegate(bool _)
		{
			DiscordManager.LobbyInfo activeVoiceLobby = this.ActiveVoiceLobby;
			if (activeVoiceLobby == null)
			{
				return;
			}
			activeVoiceLobby.VoiceCall.SetPushToTalkMode();
		};
		this.friendsServerList.Init(null);
		ServerListManager.Instance.RegisterAdditionalServerList(this.friendsServerList);
		Log.Out("[Discord] Initialized");
		Action<DiscordManager.EDiscordStatus> statusChanged = this.StatusChanged;
		if (statusChanged != null)
		{
			statusChanged(this.Status);
		}
		this.registerGameChatHandling();
		DiscordManager.CallInfo.LoadSounds();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void registerGlobalDiscordCallbacks()
	{
		this.client.SetDeviceChangeCallback(new Client.DeviceChangeCallback(this.OnDeviceChanged));
		this.client.SetLobbyCreatedCallback(new Client.LobbyCreatedCallback(this.OnLobbyCreated));
		this.client.SetLobbyDeletedCallback(new Client.LobbyDeletedCallback(this.OnLobbyDeleted));
		this.client.SetLobbyUpdatedCallback(new Client.LobbyUpdatedCallback(this.OnLobbyUpdated));
		this.client.SetLobbyMemberAddedCallback(new Client.LobbyMemberAddedCallback(this.OnLobbyMemberAdded));
		this.client.SetLobbyMemberRemovedCallback(new Client.LobbyMemberRemovedCallback(this.OnLobbyMemberRemoved));
		this.client.SetLobbyMemberUpdatedCallback(new Client.LobbyMemberUpdatedCallback(this.OnLobbyMemberUpdated));
		this.client.SetMessageCreatedCallback(new Client.MessageCreatedCallback(this.OnMessageCreated));
		this.client.SetMessageDeletedCallback(new Client.MessageDeletedCallback(this.OnMessageDeleted));
		this.client.SetMessageUpdatedCallback(new Client.MessageUpdatedCallback(this.OnMessageUpdated));
		this.client.SetNoAudioInputCallback(new Client.NoAudioInputCallback(this.OnNoAudioInput));
		this.client.SetRelationshipCreatedCallback(new Client.RelationshipCreatedCallback(this.OnRelationshipCreated));
		this.client.SetRelationshipDeletedCallback(new Client.RelationshipDeletedCallback(this.OnRelationshipDeleted));
		this.client.SetUserUpdatedCallback(new Client.UserUpdatedCallback(this.OnUserUpdated));
		this.client.SetVoiceParticipantChangedCallback(new Client.VoiceParticipantChangedCallback(this.OnVoiceParticipantChanged));
		this.AuthManager.RegisterDiscordCallbacks();
		this.Presence.RegisterDiscordCallbacks();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void registerGameStartupForInvites()
	{
		if ((DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5).IsCurrent())
		{
			return;
		}
		bool flag;
		switch (PlatformManager.NativePlatform.PlatformIdentifier)
		{
		case EPlatformIdentifier.Local:
			flag = this.<registerGameStartupForInvites>g__RegisterNativePcLauncher|100_1();
			goto IL_0083;
		case EPlatformIdentifier.EOS:
			flag = false;
			goto IL_0083;
		case EPlatformIdentifier.Steam:
			flag = this.<registerGameStartupForInvites>g__RegisterSteamLauncher|100_0();
			goto IL_0083;
		case EPlatformIdentifier.XBL:
			flag = this.<registerGameStartupForInvites>g__RegisterNativePcLauncher|100_1();
			goto IL_0083;
		case EPlatformIdentifier.EGS:
			flag = false;
			goto IL_0083;
		}
		throw new ArgumentOutOfRangeException("PlatformIdentifier", "Invalid native platform " + PlatformManager.NativePlatform.PlatformIdentifier.ToStringCached<EPlatformIdentifier>() + " for registering Discord launch command");
		IL_0083:
		bool flag2 = flag;
		Log.Out("[Discord] Registering game for Discord invites: " + (flag2 ? "Success" : "Failed"));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void cleanup(ref ModEvents.SGameShutdownData _data)
	{
		this.Settings.Save();
		if (this.client == null)
		{
			return;
		}
		Log.Out("[Discord] Cleanup");
		this.leaveLobbies(true);
		this.client.Disconnect();
		this.client.Dispose();
		this.client = null;
		Action<DiscordManager.EDiscordStatus> statusChanged = this.StatusChanged;
		if (statusChanged == null)
		{
			return;
		}
		statusChanged(this.Status);
	}

	public static bool SupportsProvisionalAccounts
	{
		get
		{
			return PlatformManager.CrossplatformPlatform != null && PlatformManager.CrossplatformPlatform.PlatformIdentifier == EPlatformIdentifier.EOS;
		}
	}

	public static bool SupportsFullAccounts
	{
		get
		{
			return !DeviceFlag.PS5.IsCurrent();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnRelationshipCreated(ulong _userId, bool _isDiscordRelationship)
	{
		DiscordManager.logCallbackInfo(string.Format("OnRelationshipCreated: {0}, {1}", _userId, _isDiscordRelationship), 3);
		DiscordManager.DiscordUser user = this.GetUser(_userId);
		user.UpdateRelationship();
		user.UpdatePresenceInfo();
		DiscordManager.RelationshipChangedCallback relationshipChanged = this.RelationshipChanged;
		if (relationshipChanged != null)
		{
			relationshipChanged(user);
		}
		DiscordManager.FriendsListChangedCallback friendsListChanged = this.FriendsListChanged;
		if (friendsListChanged == null)
		{
			return;
		}
		friendsListChanged();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnRelationshipDeleted(ulong _userId, bool _isDiscordRelationship)
	{
		DiscordManager.logCallbackInfo(string.Format("OnRelationshipDeleted: {0}, {1}", _userId, _isDiscordRelationship), 3);
		DiscordManager.DiscordUser user = this.GetUser(_userId);
		user.UpdateRelationship();
		user.UpdatePresenceInfo();
		DiscordManager.RelationshipChangedCallback relationshipChanged = this.RelationshipChanged;
		if (relationshipChanged != null)
		{
			relationshipChanged(user);
		}
		DiscordManager.FriendsListChangedCallback friendsListChanged = this.FriendsListChanged;
		if (friendsListChanged == null)
		{
			return;
		}
		friendsListChanged();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void clearFriends()
	{
		foreach (KeyValuePair<ulong, DiscordManager.DiscordUser> keyValuePair in this.knownUsers)
		{
			ulong num;
			DiscordManager.DiscordUser discordUser;
			keyValuePair.Deconstruct(out num, out discordUser);
			discordUser.UpdateRelationship();
		}
		DiscordManager.FriendsListChangedCallback friendsListChanged = this.FriendsListChanged;
		if (friendsListChanged == null)
		{
			return;
		}
		friendsListChanged();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void getFriends()
	{
		if (!this.IsReady)
		{
			DiscordManager.FriendsListChangedCallback friendsListChanged = this.FriendsListChanged;
			if (friendsListChanged == null)
			{
				return;
			}
			friendsListChanged();
			return;
		}
		else
		{
			foreach (RelationshipHandle relationshipHandle in this.client.GetRelationships())
			{
				this.GetUser(relationshipHandle.Id()).UpdatePresenceInfo();
			}
			DiscordManager.FriendsListChangedCallback friendsListChanged2 = this.FriendsListChanged;
			if (friendsListChanged2 == null)
			{
				return;
			}
			friendsListChanged2();
			return;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnUserUpdated(ulong _userId)
	{
		DiscordManager.DiscordUser user = this.GetUser(_userId);
		user.UpdatePresenceInfo();
		user.UpdateRelationship();
		DiscordManager.FriendsListChangedCallback friendsListChanged = this.FriendsListChanged;
		if (friendsListChanged != null)
		{
			friendsListChanged();
		}
		DiscordManager.logCallbackInfo(string.Format("OnUserUpdated: user={0}", user), 3);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void onLogMessageReceivedGeneric(string _message, LoggingSeverity _severity, string _prefix)
	{
		Match match = DiscordManager.logMessageMatcher.Match(_message);
		if (match.Success)
		{
			if (match.Groups[4].Value.IndexOf('\n') >= 0)
			{
				_message = match.Groups[4].Value + " (" + match.Groups[3].Value + ")\n";
			}
			else
			{
				_message = match.Groups[4].Value + " (" + match.Groups[3].Value + ")";
			}
		}
		else if (_message.IndexOf('\n') == _message.Length - 1)
		{
			_message = _message.Substring(0, _message.Length - 1);
		}
		switch (_severity)
		{
		case 1:
			Log.Out("[Discord]" + _prefix + "[Log](Verb) " + _message);
			return;
		case 2:
			Log.Out("[Discord]" + _prefix + "[Log](Info) " + _message);
			return;
		case 3:
			Log.Warning("[Discord]" + _prefix + "[Log] " + _message);
			return;
		case 4:
			Log.Error("[Discord]" + _prefix + "[Log] " + _message);
			return;
		case 5:
			Log.Out("[Discord]" + _prefix + "[Log](None) " + _message);
			return;
		default:
			Log.Error(string.Format("[Discord]{0}[Log] Unknown log severity ({1}): {2}", _prefix, _severity, _message));
			return;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void nativeMethodException(Exception _e)
	{
		Log.Error("[Discord] Exception: " + _e.Message);
		Log.Exception(_e);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDiscordLogMessageReceived(string _message, LoggingSeverity _severity)
	{
		this.onLogMessageReceivedGeneric(_message, _severity, "");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDiscordRtcLogMessageReceived(string _message, LoggingSeverity _severity)
	{
		this.onLogMessageReceivedGeneric(_message, _severity, "[RTC]");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnMessageCreated(ulong _messageId)
	{
		this.handleMessage(true, _messageId);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnMessageDeleted(ulong _messageId, ulong _channelId)
	{
		DiscordManager.logCallbackInfo(string.Format("OnMessageDeleted: msg={0}, channel={1}", _messageId, _channelId), 3);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnMessageUpdated(ulong _messageId)
	{
		this.handleMessage(false, _messageId);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void handleMessage(bool _created, ulong _messageId)
	{
		if (this.client == null)
		{
			return;
		}
		using (MessageHandle messageHandle = this.client.GetMessageHandle(_messageId))
		{
			if (messageHandle == null)
			{
				DiscordManager.logCallbackInfo(string.Format("{0}: msg={1}, No message handle", _created ? "OnMessageCreated" : "OnMessageUpdated", _messageId), 2);
			}
			else
			{
				ulong num = messageHandle.ChannelId();
				ulong num2 = messageHandle.Id();
				string text = messageHandle.Content();
				messageHandle.RawContent();
				using (AdditionalContent additionalContent = messageHandle.AdditionalContent())
				{
					AdditionalContentType? additionalContentType = ((additionalContent != null) ? new AdditionalContentType?(additionalContent.Type()) : null);
					DiscordManager.DiscordUser user = this.GetUser(messageHandle.AuthorId());
					bool isLocalAccount = user.IsLocalAccount;
					DiscordManager.DiscordUser discordUser = (isLocalAccount ? this.GetUser(messageHandle.RecipientId()) : this.LocalUser);
					DiscordManager.DiscordUser discordUser2 = (isLocalAccount ? discordUser : user);
					DiscordManager.logCallbackInfo(string.Format("{0}: msg={1} channel={2} sender='{3}' recipient='{4}' outbound='{5}' text='<redacted>' rawContent='<redacted>'", new object[]
					{
						_created ? "OnMessageCreated" : "OnMessageUpdated",
						num2,
						num,
						user.DisplayName,
						discordUser.DisplayName,
						isLocalAccount
					}), 3);
					if (additionalContent != null)
					{
						DiscordManager.logCallbackInfo(string.Format("{0}: Additional content: Count={1}, type={2} title='{3}'", new object[]
						{
							_created ? "OnMessageCreated" : "OnMessageUpdated",
							additionalContent.Count(),
							additionalContentType.Value.ToStringCached<AdditionalContentType>(),
							additionalContent.Title()
						}), 3);
					}
					if (_created)
					{
						if (this.Settings.DmPrivacyMode && !discordUser2.MessageSentFromGame)
						{
							if (DiscordManager.logLevel == 1)
							{
								Log.Out(string.Format("[Discord] Not showing received DM: Privacy mode active and no message sent to the user yet ({0})", discordUser2));
							}
						}
						else
						{
							LocalPlayerUI uiforPrimaryPlayer = LocalPlayerUI.GetUIForPrimaryPlayer();
							if (!(uiforPrimaryPlayer == null) && !(uiforPrimaryPlayer.entityPlayer == null))
							{
								EMessageSender emessageSender = EMessageSender.SenderIdAsPlayer;
								int num3;
								if (!this.userMappings.TryGetEntityId(discordUser2.ID, out num3))
								{
									emessageSender = EMessageSender.None;
									num3 = -1;
								}
								XUiC_Chat.EnforceTargetExists(uiforPrimaryPlayer.xui, EChatType.Discord, discordUser2.ID.ToString());
								if (!string.IsNullOrEmpty(text))
								{
									XUiC_ChatOutput.AddMessage(uiforPrimaryPlayer.xui, EnumGameMessages.Chat, text, EChatType.Discord, isLocalAccount ? EChatDirection.Outbound : EChatDirection.Inbound, num3, discordUser2.DisplayName, discordUser2.ID.ToString(), emessageSender, GeneratedTextManager.TextFilteringMode.Filter, GeneratedTextManager.BbCodeSupportMode.SupportedAndAddEscapes);
								}
								if (additionalContent != null)
								{
									if (additionalContentType != null)
									{
										string text2;
										switch (additionalContentType.GetValueOrDefault())
										{
										case 0:
											text2 = Localization.Get("discordMessageAdditionalContentTypeOther", false, null);
											break;
										case 1:
											text2 = ((additionalContent.Count() <= 1) ? Localization.Get("discordMessageAdditionalContentTypeAttachmentSingle", false, null) : string.Format(Localization.Get("discordMessageAdditionalContentTypeAttachmentMultiple", false, null), additionalContent.Count()));
											break;
										case 2:
											text2 = Localization.Get("discordMessageAdditionalContentTypePoll", false, null);
											break;
										case 3:
											text2 = Localization.Get("discordMessageAdditionalContentTypeVoiceMessage", false, null);
											break;
										case 4:
											text2 = Localization.Get("discordMessageAdditionalContentTypeThread", false, null);
											break;
										case 5:
											text2 = Localization.Get("discordMessageAdditionalContentTypeEmbed", false, null);
											break;
										case 6:
											text2 = Localization.Get("discordMessageAdditionalContentTypeSticker", false, null);
											break;
										default:
											goto IL_0322;
										}
										string text3 = text2;
										string text4 = ((this.client.CanOpenMessageInDiscord(num2) && !this.LocalUser.IsProvisionalAccount && !(DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5).IsCurrent()) ? (LabelUrlUtils.BuildUrlFunctionString("DiscordMessageButton", new ValueTuple<string, string>("MessageId", num2.ToString())) + text3 + " [sp=ui_game_symbol_external_link][/url]") : text3);
										XUiC_ChatOutput.AddMessage(uiforPrimaryPlayer.xui, EnumGameMessages.Chat, text4, EChatType.Discord, isLocalAccount ? EChatDirection.Outbound : EChatDirection.Inbound, num3, discordUser2.DisplayName, discordUser2.ID.ToString(), emessageSender, GeneratedTextManager.TextFilteringMode.None, GeneratedTextManager.BbCodeSupportMode.Supported);
										goto IL_03C8;
									}
									IL_0322:
									throw new ArgumentOutOfRangeException("additionalContentType", additionalContentType.Value, "Invalid AdditionalContentType");
								}
							}
						}
					}
					IL_03C8:;
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void registerGameChatHandling()
	{
		LabelUrlUtils.RegisterLabelUrlHandler("DiscordMessageButton", new LabelUrlUtils.HandleTextClickUrlDelegate(this.<registerGameChatHandling>g__DiscordButtonHandler|123_1), new LabelUrlUtils.HandleTextHoverUrlDelegate(DiscordManager.<registerGameChatHandling>g__DiscordButtonHoverHandler|123_0));
		XUiC_Chat.RegisterCustomMessagingHandler(EChatType.Discord, new XUiC_Chat.IsValidTarget(this.<registerGameChatHandling>g__IsValidTarget|123_4), new XUiC_Chat.GetTargetDisplayName(this.<registerGameChatHandling>g__GetTargetDisplayName|123_3), new XUiC_Chat.SendMessage(this.<registerGameChatHandling>g__SendMessage|123_2));
	}

	public DiscordManager.LobbyInfo GetLobby(DiscordManager.ELobbyType _type)
	{
		if (_type != DiscordManager.ELobbyType.Global)
		{
			return this.partyLobby;
		}
		return this.globalLobby;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void leaveLobbies(bool _manual = true)
	{
		this.LeaveLobbyVoice(_manual);
		this.globalLobby.Leave(_manual);
		this.partyLobby.Leave(_manual);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnLobbyCreated(ulong _lobbyId)
	{
		DiscordManager.logCallbackInfo(string.Format("OnLobbyCreated: lobby={0}", _lobbyId), 3);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnLobbyDeleted(ulong _lobbyId)
	{
		DiscordManager.logCallbackInfo(string.Format("OnLobbyDeleted: lobby={0}", _lobbyId), 3);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnLobbyUpdated(ulong _lobbyId)
	{
		DiscordManager.logCallbackInfo(string.Format("OnLobbyUpdated: lobby={0}", _lobbyId), 3);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnLobbyMemberAdded(ulong _lobbyId, ulong _memberId)
	{
		this.OnLobbyMemberChanged(_lobbyId, _memberId, DiscordManager.LobbyMemberActionType.Add);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnLobbyMemberRemoved(ulong _lobbyId, ulong _memberId)
	{
		this.OnLobbyMemberChanged(_lobbyId, _memberId, DiscordManager.LobbyMemberActionType.Remove);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnLobbyMemberUpdated(ulong _lobbyId, ulong _memberId)
	{
		this.OnLobbyMemberChanged(_lobbyId, _memberId, DiscordManager.LobbyMemberActionType.Update);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnLobbyMemberChanged(ulong _lobbyId, ulong _memberId, DiscordManager.LobbyMemberActionType _actionType)
	{
		DiscordManager.DiscordUser user = this.GetUser(_memberId);
		DiscordManager.logCallbackInfo(string.Format("OnLobbyMember{0}: lobby={1} user={2}", _actionType.ToStringCached<DiscordManager.LobbyMemberActionType>(), _lobbyId, user), 3);
		this.globalLobby.UpdateMembers();
		this.partyLobby.UpdateMembers();
		DiscordManager.LobbyInfo activeVoiceLobby = this.ActiveVoiceLobby;
		if (activeVoiceLobby == null)
		{
			return;
		}
		activeVoiceLobby.VoiceCall.UpdateMembers(false);
	}

	public bool Mute
	{
		get
		{
			Client client = this.client;
			return client != null && client.GetSelfMuteAll();
		}
		set
		{
			Client client = this.client;
			if (client != null)
			{
				client.SetSelfMuteAll(value);
			}
			DiscordManager.SelfMuteStateChangedCallback selfMuteStateChanged = this.SelfMuteStateChanged;
			if (selfMuteStateChanged == null)
			{
				return;
			}
			selfMuteStateChanged(value, this.Deaf);
		}
	}

	public bool Deaf
	{
		get
		{
			Client client = this.client;
			return client != null && client.GetSelfDeafAll();
		}
		set
		{
			Client client = this.client;
			if (client != null)
			{
				client.SetSelfDeafAll(value);
			}
			DiscordManager.SelfMuteStateChangedCallback selfMuteStateChanged = this.SelfMuteStateChanged;
			if (selfMuteStateChanged == null)
			{
				return;
			}
			selfMuteStateChanged(this.Mute, value);
		}
	}

	public DiscordManager.LobbyInfo ActiveVoiceLobby
	{
		get
		{
			if (this.partyLobby.IsInVoice)
			{
				return this.partyLobby;
			}
			if (this.globalLobby.IsInVoice)
			{
				return this.globalLobby;
			}
			return null;
		}
	}

	public bool AnyLobbyInUnstableVoiceState
	{
		get
		{
			Call.Status status = this.globalLobby.VoiceCall.Status;
			if (status != 4 && status != null)
			{
				return true;
			}
			status = this.partyLobby.VoiceCall.Status;
			return status != 4 && status != null;
		}
	}

	public void JoinLobbyVoice(DiscordManager.ELobbyType _type)
	{
		DiscordManager.LobbyInfo lobby = this.GetLobby(_type);
		if (this.ActiveVoiceLobby == lobby)
		{
			return;
		}
		this.LeaveLobbyVoice(true);
		lobby.VoiceCall.Join();
	}

	public void LeaveLobbyVoice(bool _manual = true)
	{
		DiscordManager.LobbyInfo activeVoiceLobby = this.ActiveVoiceLobby;
		if (activeVoiceLobby == null)
		{
			return;
		}
		activeVoiceLobby.VoiceCall.Leave(_manual);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnNoAudioInput(bool _inputDetected)
	{
		DiscordManager.logCallbackInfo(string.Format("OnNoAudioInput: inputDetected={0}", _inputDetected), 3);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnVoiceParticipantChanged(ulong _lobbyId, ulong _userId, bool _added)
	{
		DiscordManager.DiscordUser user = this.GetUser(_userId);
		DiscordManager.logCallbackInfo(string.Format("OnVoiceParticipantChanged: lobby={0} user={1} added={2}", _lobbyId, user, _added), 3);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDeviceChanged(AudioDevice[] _inputDevices, AudioDevice[] _outputDevices)
	{
		DiscordManager.logCallbackInfo(string.Format("OnDeviceChanged: input devices: {0}, output devices: {1}", _inputDevices.Length, _outputDevices.Length), 3);
		this.AudioOutput.ApplyDevicesFound(_outputDevices);
		this.AudioInput.ApplyDevicesFound(_inputDevices);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void fireAudioDevicesChanged(DiscordManager.AudioDeviceConfig _config)
	{
		DiscordManager.AudioDevicesChangedCallback audioDevicesChanged = this.AudioDevicesChanged;
		if (audioDevicesChanged == null)
		{
			return;
		}
		audioDevicesChanged(_config);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateAudioDeviceList()
	{
		this.AudioOutput.UpdateAudioDeviceList();
		this.AudioInput.UpdateAudioDeviceList();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void registerGameEventHandlers()
	{
		ModEvents.GameFocus.RegisterHandler(new ModEvents.ModEventHandlerDelegate<ModEvents.SGameFocusData>(this.gameFocusChanged));
		ModEvents.MainMenuOpening.RegisterHandler(new ModEvents.ModEventInterruptibleHandlerDelegate<ModEvents.SMainMenuOpeningData>(this.mainMenuOpening));
		ModEvents.UnityUpdate.RegisterHandler(new ModEvents.ModEventHandlerDelegate<ModEvents.SUnityUpdateData>(this.update));
		ModEvents.ServerRegistered.RegisterHandler(new ModEvents.ModEventHandlerDelegate<ModEvents.SServerRegisteredData>(this.serverStarted));
		ModEvents.PlayerJoinedGame.RegisterHandler(new ModEvents.ModEventHandlerDelegate<ModEvents.SPlayerJoinedGameData>(this.playerJoined));
		ModEvents.PlayerSpawning.RegisterHandler(new ModEvents.ModEventHandlerDelegate<ModEvents.SPlayerSpawningData>(this.playerSpawning));
		ModEvents.PlayerSpawnedInWorld.RegisterHandler(new ModEvents.ModEventHandlerDelegate<ModEvents.SPlayerSpawnedInWorldData>(this.playerSpawned));
		ModEvents.PlayerDisconnected.RegisterHandler(new ModEvents.ModEventHandlerDelegate<ModEvents.SPlayerDisconnectedData>(this.playerDisconnected));
		ModEvents.GameStarting.RegisterHandler(new ModEvents.ModEventHandlerDelegate<ModEvents.SGameStartingData>(this.gameStarting));
		ModEvents.GameUpdate.RegisterHandler(new ModEvents.ModEventHandlerDelegate<ModEvents.SGameUpdateData>(this.inGameUpdate));
		ModEvents.WorldShuttingDown.RegisterHandler(new ModEvents.ModEventHandlerDelegate<ModEvents.SWorldShuttingDownData>(this.gameEnded));
		ModEvents.GameShutdown.RegisterHandler(new ModEvents.ModEventHandlerDelegate<ModEvents.SGameShutdownData>(this.cleanup));
		this.AuthManager.RegisterGameEventHandlers();
		GameManager.Instance.OnLocalPlayerChanged += this.localPlayerChangedEvent;
		PlatformUserManager.BlockedStateChanged += this.playerBlockStateChanged;
		World world = GameManager.Instance.World;
		EntityPlayerLocal entityPlayerLocal = ((world != null) ? world.GetPrimaryPlayer() : null);
		if (entityPlayerLocal != null)
		{
			this.playerCreated(entityPlayerLocal);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void gameFocusChanged(ref ModEvents.SGameFocusData _data)
	{
		if (GameManager.IsDedicatedServer || !this.IsInitialized)
		{
			return;
		}
		this.client.SetShowingChat(_data.IsFocused);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public ModEvents.EModEventResult mainMenuOpening(ref ModEvents.SMainMenuOpeningData _data)
	{
		if (_data.OpenedBefore)
		{
			return ModEvents.EModEventResult.Continue;
		}
		if (PlatformManager.MultiPlatform.User.UserStatus != EUserStatus.LoggedIn)
		{
			return ModEvents.EModEventResult.Continue;
		}
		if (GameManager.IsDedicatedServer || !VoiceHelpers.VoiceAllowed || !PermissionsManager.IsMultiplayerAllowed())
		{
			return ModEvents.EModEventResult.Continue;
		}
		if (!this.Settings.DiscordFirstTimeInfoShown && !this.Settings.DiscordDisabled)
		{
			LocalPlayerUI.primaryUI.windowManager.Open(XUiC_DiscordInfo.ID, true);
			return ModEvents.EModEventResult.StopHandlersAndVanilla;
		}
		if (this.Settings.DiscordDisabled)
		{
			return ModEvents.EModEventResult.Continue;
		}
		this.Init(false);
		if (this.AuthManager.IsLoggingIn || this.Status != DiscordManager.EDiscordStatus.Disconnected)
		{
			return ModEvents.EModEventResult.Continue;
		}
		XUiC_DiscordLogin.Open(null, true, true, true, true, false);
		this.AuthManager.AutoLogin();
		return ModEvents.EModEventResult.StopHandlersAndVanilla;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void update(ref ModEvents.SUnityUpdateData _data)
	{
		if (!this.IsReady)
		{
			return;
		}
		this.handlePushToTalkButton();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void handlePushToTalkButton()
	{
		DiscordManager.LobbyInfo activeVoiceLobby = this.ActiveVoiceLobby;
		if (activeVoiceLobby == null || !activeVoiceLobby.IsInVoice)
		{
			return;
		}
		if (this.Settings.VoiceModePtt)
		{
			this.ActiveVoiceLobby.VoiceCall.SetPushToTalkActive(VoiceHelpers.PushToTalkPressed());
			return;
		}
		if (VoiceHelpers.PushToTalkWasPressed())
		{
			this.Mute = !this.Mute;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void serverStarted(ref ModEvents.SServerRegisteredData _data)
	{
		if (this.globalLobby.Secret != null)
		{
			return;
		}
		this.ReceivedLobbySecret(DiscordManager.ELobbyType.Global, SingletonMonoBehaviour<ConnectionManager>.Instance.LocalServerInfo.GetValue(GameInfoString.IP) + "_" + Utils.GenerateGuid());
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void playerJoined(ref ModEvents.SPlayerJoinedGameData _data)
	{
		this.userMappings.SendMappingsToClient(_data.ClientInfo);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void playerSpawning(ref ModEvents.SPlayerSpawningData _data)
	{
		_data.ClientInfo.SendPackage(NetPackageManager.GetPackage<NetPackageDiscordLobbySecret>().Setup(DiscordManager.ELobbyType.Global, this.globalLobby.Secret));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void playerSpawned(ref ModEvents.SPlayerSpawnedInWorldData _data)
	{
		this.updateUserNames();
		if (GameManager.IsDedicatedServer || !this.IsInitialized)
		{
			return;
		}
		if (!_data.IsLocalPlayer)
		{
			return;
		}
		this.client.SetShowingChat(true);
		if (this.Settings.AutoJoinVoiceMode == DiscordManager.EAutoJoinVoiceMode.Global && this.globalLobby.Secret != null)
		{
			this.JoinLobbyVoice(DiscordManager.ELobbyType.Global);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void playerDisconnected(ref ModEvents.SPlayerDisconnectedData _data)
	{
		int entityId = _data.ClientInfo.entityId;
		if (entityId == -1)
		{
			return;
		}
		this.userMappings.UpdateMapping(entityId, true, 0UL);
		DiscordManager.FriendsListChangedCallback friendsListChanged = this.FriendsListChanged;
		if (friendsListChanged != null)
		{
			friendsListChanged();
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageDiscordIdMappings>().Setup(entityId, true, 0UL), false, -1, -1, -1, null, 192, false);
		}
	}

	public void ReceivedLobbySecret(DiscordManager.ELobbyType _lobbyType, string _secret)
	{
		DiscordManager.LobbyInfo lobby = this.GetLobby(_lobbyType);
		Log.Out("[Discord] " + _lobbyType.ToStringCached<DiscordManager.ELobbyType>() + " lobby: " + _secret);
		lobby.Secret = _secret;
		if (GameManager.IsDedicatedServer || !this.IsReady)
		{
			return;
		}
		lobby.Join(true);
	}

	public void LeftParty()
	{
		this.partyLobby.Secret = null;
		if (GameManager.IsDedicatedServer || !this.IsInitialized)
		{
			return;
		}
		this.partyLobby.Leave(false);
	}

	public void UserMappingReceived(int _entityId, bool _remove, ulong _discordId, bool _batch = false)
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageDiscordIdMappings>().Setup(_entityId, _remove, _discordId), false, -1, -1, -1, null, 192, false);
		}
		this.userMappings.UpdateMapping(_entityId, _remove, _discordId);
		if (_discordId > 0UL)
		{
			this.GetUser(_discordId).UpdatePlayerName(null);
		}
		if (!_batch)
		{
			DiscordManager.FriendsListChangedCallback friendsListChanged = this.FriendsListChanged;
			if (friendsListChanged == null)
			{
				return;
			}
			friendsListChanged();
		}
	}

	public void UserMappingsReceived(List<int> _entityIds, List<ulong> _discordIds)
	{
		for (int i = 0; i < _entityIds.Count; i++)
		{
			this.UserMappingReceived(_entityIds[i], false, _discordIds[i], true);
		}
		DiscordManager.FriendsListChangedCallback friendsListChanged = this.FriendsListChanged;
		if (friendsListChanged == null)
		{
			return;
		}
		friendsListChanged();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void gameStarting(ref ModEvents.SGameStartingData _data)
	{
		this.Presence.SetRichPresenceState(new IRichPresence.PresenceStates?(_data.AsServer ? IRichPresence.PresenceStates.Loading : IRichPresence.PresenceStates.Connecting));
		this.resetPendingOutgoingJoinRequests();
		if (PlatformManager.MultiPlatform.User.UserStatus != EUserStatus.LoggedIn)
		{
			return;
		}
		if (GameManager.IsDedicatedServer || !VoiceHelpers.VoiceAllowed || !PermissionsManager.IsMultiplayerAllowed())
		{
			return;
		}
		if (this.Settings.DiscordDisabled)
		{
			return;
		}
		this.Init(false);
		if (this.AuthManager.IsLoggingIn || this.Status != DiscordManager.EDiscordStatus.Disconnected)
		{
			return;
		}
		this.AuthManager.AutoLogin();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void inGameUpdate(ref ModEvents.SGameUpdateData _data)
	{
		float unscaledTime = Time.unscaledTime;
		if (unscaledTime < this.nextActivityUpdate)
		{
			return;
		}
		this.nextActivityUpdate = unscaledTime + 10f;
		this.Presence.SetRichPresenceState(new IRichPresence.PresenceStates?(IRichPresence.PresenceStates.InGame));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void gameEnded(ref ModEvents.SWorldShuttingDownData _data)
	{
		this.globalLobby.Secret = null;
		this.partyLobby.Secret = null;
		this.userMappings.Clear();
		this.Presence.SetRichPresenceState(new IRichPresence.PresenceStates?(IRichPresence.PresenceStates.Menu));
		if (GameManager.IsDedicatedServer || !this.IsInitialized)
		{
			return;
		}
		this.leaveLobbies(true);
		DiscordManager.FriendsListChangedCallback friendsListChanged = this.FriendsListChanged;
		if (friendsListChanged != null)
		{
			friendsListChanged();
		}
		this.client.SetShowingChat(false);
		this.Settings.Save();
		this.UserSettings.Save();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void localPlayerChangedEvent(EntityPlayerLocal _newLocalPlayer)
	{
		if (!(_newLocalPlayer != null))
		{
			this.playerDestroyed();
			return;
		}
		this.playerCreated(_newLocalPlayer);
		DiscordManager.DiscordUser discordUser = this.LocalUser;
		if (discordUser != null)
		{
			discordUser.UpdatePlayerName(_newLocalPlayer);
		}
		this.localDiscordOrEntityIdChanged();
		DiscordManager.FriendsListChangedCallback friendsListChanged = this.FriendsListChanged;
		if (friendsListChanged == null)
		{
			return;
		}
		friendsListChanged();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void playerCreated(EntityPlayerLocal _newLocalPlayer)
	{
		if (PlatformManager.MultiPlatform.User.UserStatus == EUserStatus.OfflineMode)
		{
			return;
		}
		this.localPlayer = _newLocalPlayer;
		this.localPlayer.PartyJoined += this.playerJoinedParty;
		this.localPlayer.PartyLeave += this.playerLeftParty;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void playerDestroyed()
	{
		if (this.localPlayer != null)
		{
			this.localPlayer.PartyJoined -= this.playerJoinedParty;
			this.localPlayer.PartyLeave -= this.playerLeftParty;
			this.localPlayer = null;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void playerJoinedParty(Party _affectedParty, EntityPlayer _player)
	{
		this.ReceivedLobbySecret(DiscordManager.ELobbyType.Party, string.Format("{0}_{1}", this.globalLobby.Secret, _affectedParty.PartyID));
		if (this.Settings.AutoJoinVoiceMode == DiscordManager.EAutoJoinVoiceMode.Party && this.ActiveVoiceLobby == null)
		{
			this.JoinLobbyVoice(DiscordManager.ELobbyType.Party);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void playerLeftParty(Party _affectedParty, EntityPlayer _player)
	{
		this.LeftParty();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void playerBlockStateChanged(IPlatformUserData _ppd, EBlockType _blockType, EUserBlockState _blockState)
	{
		if (_blockType != EBlockType.VoiceChat)
		{
			return;
		}
		PersistentPlayerList persistentPlayers = GameManager.Instance.persistentPlayers;
		PersistentPlayerData persistentPlayerData = ((persistentPlayers != null) ? persistentPlayers.GetPlayerData(_ppd.PrimaryId) : null);
		if (persistentPlayerData == null || persistentPlayerData.EntityId == -1)
		{
			return;
		}
		DiscordManager.DiscordUser discordUser;
		if (!this.TryGetUserFromEntityId(persistentPlayerData.EntityId, out discordUser))
		{
			return;
		}
		DiscordManager.LobbyInfo activeVoiceLobby = this.ActiveVoiceLobby;
		if (activeVoiceLobby == null)
		{
			return;
		}
		activeVoiceLobby.VoiceCall.UpdateBlockState(discordUser, _blockState.IsBlocked());
	}

	public void OpenDiscordSocialSettings()
	{
		this.client.OpenConnectedGamesSettingsInDiscord(delegate(ClientResult _result)
		{
			DiscordManager.logCallbackInfoWithClientResult("OpenConnectedGamesSettingsInDiscord", null, _result, true);
		});
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void localDiscordOrEntityIdChanged()
	{
		if (this.localPlayer == null)
		{
			return;
		}
		DiscordManager.DiscordUserMappingManager discordUserMappingManager = this.userMappings;
		int entityId = this.localPlayer.entityId;
		bool flag = false;
		DiscordManager.DiscordUser discordUser = this.LocalUser;
		discordUserMappingManager.UpdateMapping(entityId, flag, (discordUser != null) ? discordUser.ID : 0UL);
		DiscordManager.DiscordUser discordUser2 = this.LocalUser;
		if (discordUser2 != null)
		{
			discordUser2.UpdatePlayerName(null);
		}
		ConnectionManager connectionManager = SingletonMonoBehaviour<ConnectionManager>.Instance;
		NetPackageDiscordIdMappings package = NetPackageManager.GetPackage<NetPackageDiscordIdMappings>();
		int entityId2 = this.localPlayer.entityId;
		bool flag2 = false;
		DiscordManager.DiscordUser discordUser3 = this.LocalUser;
		connectionManager.SendToClientsOrServer(package.Setup(entityId2, flag2, (discordUser3 != null) ? discordUser3.ID : 0UL));
		if (!GameManager.IsDedicatedServer && this.IsReady)
		{
			this.globalLobby.Join(false);
			this.partyLobby.Join(false);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateUserNames()
	{
		foreach (KeyValuePair<ulong, DiscordManager.DiscordUser> keyValuePair in this.knownUsers)
		{
			ulong num;
			DiscordManager.DiscordUser discordUser;
			keyValuePair.Deconstruct(out num, out discordUser);
			discordUser.UpdatePlayerName(null);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void resetPendingOutgoingJoinRequests()
	{
		foreach (KeyValuePair<ulong, DiscordManager.DiscordUser> keyValuePair in this.knownUsers)
		{
			ulong num;
			DiscordManager.DiscordUser discordUser;
			keyValuePair.Deconstruct(out num, out discordUser);
			discordUser.PendingOutgoingJoinRequest = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void refreshCachedUserHandlesAndRelationships()
	{
		foreach (KeyValuePair<ulong, DiscordManager.DiscordUser> keyValuePair in this.knownUsers)
		{
			ulong num;
			DiscordManager.DiscordUser discordUser;
			keyValuePair.Deconstruct(out num, out discordUser);
			discordUser.TryUpdateDiscordHandle();
		}
	}

	public int GetPendingActionsCount()
	{
		int num = 0;
		foreach (KeyValuePair<ulong, DiscordManager.DiscordUser> keyValuePair in this.knownUsers)
		{
			ulong num2;
			DiscordManager.DiscordUser discordUser;
			keyValuePair.Deconstruct(out num2, out discordUser);
			if (discordUser.PendingAction)
			{
				num++;
			}
		}
		return num;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updatePendingActionsEvent()
	{
		DiscordManager.PendingActionsUpdateCallback pendingActionsUpdate = this.PendingActionsUpdate;
		if (pendingActionsUpdate == null)
		{
			return;
		}
		pendingActionsUpdate(this.GetPendingActionsCount());
	}

	public DiscordManager.DiscordUser GetUser(ulong _userId)
	{
		DiscordManager.DiscordUser discordUser;
		if (!this.knownUsers.TryGetValue(_userId, out discordUser))
		{
			discordUser = new DiscordManager.DiscordUser(this, _userId, false);
			this.knownUsers[_userId] = discordUser;
		}
		discordUser.TryUpdateDiscordHandle();
		return discordUser;
	}

	public bool TryGetUserFromEntityId(int _entityId, out DiscordManager.DiscordUser _user)
	{
		ulong num;
		if (!this.userMappings.TryGetDiscordId(_entityId, out num))
		{
			_user = null;
			return false;
		}
		return this.knownUsers.TryGetValue(num, out _user);
	}

	public bool TryGetUserFromEntity(EntityPlayer _entity, out DiscordManager.DiscordUser _user)
	{
		if (_entity == null)
		{
			_user = null;
			return false;
		}
		return this.TryGetUserFromEntityId(_entity.entityId, out _user);
	}

	public void GetAllUsers(IList<DiscordManager.DiscordUser> _target)
	{
		this.knownUsers.CopyValuesTo(_target);
	}

	public void GetFriends(HashSet<DiscordManager.DiscordUser> _target)
	{
		foreach (KeyValuePair<ulong, DiscordManager.DiscordUser> keyValuePair in this.knownUsers)
		{
			ulong num;
			DiscordManager.DiscordUser discordUser;
			keyValuePair.Deconstruct(out num, out discordUser);
			DiscordManager.DiscordUser discordUser2 = discordUser;
			if (discordUser2.IsFriend)
			{
				_target.Add(discordUser2);
			}
		}
	}

	public void GetBlockedUsers(HashSet<DiscordManager.DiscordUser> _target)
	{
		foreach (KeyValuePair<ulong, DiscordManager.DiscordUser> keyValuePair in this.knownUsers)
		{
			ulong num;
			DiscordManager.DiscordUser discordUser;
			keyValuePair.Deconstruct(out num, out discordUser);
			DiscordManager.DiscordUser discordUser2 = discordUser;
			if (discordUser2.IsBlocked)
			{
				_target.Add(discordUser2);
			}
		}
	}

	public void GetUsersWithPendingAction(HashSet<DiscordManager.DiscordUser> _target)
	{
		foreach (KeyValuePair<ulong, DiscordManager.DiscordUser> keyValuePair in this.knownUsers)
		{
			ulong num;
			DiscordManager.DiscordUser discordUser;
			keyValuePair.Deconstruct(out num, out discordUser);
			DiscordManager.DiscordUser discordUser2 = discordUser;
			if (discordUser2.PendingAction)
			{
				_target.Add(discordUser2);
			}
		}
	}

	public void GetInServer(HashSet<DiscordManager.DiscordUser> _target)
	{
		this.userMappings.GetAll(delegate(int _, ulong _discordId)
		{
			if (_discordId <= 0UL)
			{
				return;
			}
			DiscordManager.DiscordUser user = this.GetUser(_discordId);
			if (user.IsLocalAccount)
			{
				return;
			}
			user.RequestAvatar();
			_target.Add(user);
		});
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void logCallbackInfo(string _message, LogType _logType = 3)
	{
		switch (_logType)
		{
		case 0:
		case 4:
			Log.Error("[Discord][CB] " + _message);
			return;
		case 2:
			if (DiscordManager.logLevel <= 3)
			{
				Log.Warning("[Discord][CB] " + _message);
			}
			return;
		}
		if (DiscordManager.logLevel <= 2)
		{
			Log.Out("[Discord][CB] " + _message);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void logCallbackInfoWithClientResult(string _callbackName, string _message, ClientResult _result, bool _disposeClientResult = false)
	{
		bool flag = _result.Type() != null;
		LogType logType = 0;
		if (!flag)
		{
			logType = 3;
			if (DiscordManager.logLevel > 2)
			{
				if (_disposeClientResult)
				{
					_result.Dispose();
				}
				return;
			}
		}
		string text = DiscordManager.clientResultToString(_result, _disposeClientResult);
		DiscordManager.logCallbackInfo(string.IsNullOrEmpty(_message) ? (_callbackName + " (" + text + ")") : string.Concat(new string[] { _callbackName, " (", text, "): ", _message }), logType);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string clientResultToString(ClientResult _result, bool _dispose = false)
	{
		string text = string.Format("{0}/{1}/{2}/'{3}'", new object[]
		{
			_result.Type().ToStringCached<ErrorType>(),
			_result.ErrorCode(),
			_result.Status().ToStringCached<HttpStatusCode>(),
			_result.Error()
		});
		if (_dispose)
		{
			_result.Dispose();
		}
		return text;
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool <registerGameStartupForInvites>g__RegisterSteamLauncher|100_0()
	{
		return this.client.RegisterLaunchSteamApplication(1296840202995896363UL, 251570U);
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool <registerGameStartupForInvites>g__RegisterNativePcLauncher|100_1()
	{
		RuntimePlatform platform = Application.platform;
		if (platform == null || platform == 1)
		{
			return this.client.RegisterLaunchCommand(1296840202995896363UL, "com.company.7dLauncher");
		}
		return this.client.RegisterLaunchCommand(1296840202995896363UL, GameIO.GetLauncherExecutablePath());
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Internal)]
	public static void <registerGameChatHandling>g__DiscordButtonHoverHandler|123_0(XUiView _sender, string _sourceUrl, Dictionary<string, string> _urlElements, out string _tooltipText)
	{
		_tooltipText = null;
		string text;
		if (!_urlElements.TryGetValue("MessageId", out text))
		{
			Log.Warning("DiscordButton URL (" + _sourceUrl + "): No MessageId defined");
			return;
		}
		ulong num;
		if (!StringParsers.TryParseUInt64(text, out num))
		{
			Log.Warning("DiscordButton URL (" + _sourceUrl + "): Invalid MessageId");
			return;
		}
		_tooltipText = Localization.Get("xuiChatOpenMessageInDiscord", false, null);
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Private)]
	public void <registerGameChatHandling>g__DiscordButtonHandler|123_1(XUiView _sender, string _sourceUrl, Dictionary<string, string> _urlElements)
	{
		string text;
		if (!_urlElements.TryGetValue("MessageId", out text))
		{
			Log.Warning("DiscordButton URL (" + _sourceUrl + "): No MessageId defined");
			return;
		}
		ulong messageId;
		if (!StringParsers.TryParseUInt64(text, out messageId))
		{
			Log.Warning("DiscordButton URL (" + _sourceUrl + "): Invalid MessageId");
			return;
		}
		this.client.OpenMessageInDiscord(messageId, new Client.ProvisionalUserMergeRequiredCallback(DiscordManager.<registerGameChatHandling>g__ProvisionalUserMergeRequiredCallback|123_6), delegate(ClientResult _result)
		{
			DiscordManager.logCallbackInfoWithClientResult("OpenMessageInDiscord", messageId.ToString(), _result, true);
		});
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Internal)]
	public static void <registerGameChatHandling>g__ProvisionalUserMergeRequiredCallback|123_6()
	{
		Log.Warning("[Discord] ProvisionalUserMergeRequiredCallback fired!");
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Private)]
	public void <registerGameChatHandling>g__SendMessage|123_2(EChatType _chatType, string _targetId, string _message)
	{
		ulong num;
		if (!ulong.TryParse(_targetId, out num))
		{
			throw new ArgumentException("Could not parse chat Discord id '" + _targetId + "'");
		}
		this.GetUser(num).MessageSentFromGame = true;
		this.client.SendUserMessage(num, _message, delegate(ClientResult _result, ulong _messageId)
		{
			DiscordManager.logCallbackInfoWithClientResult("SendUserMessage", _messageId.ToString(), _result, true);
		});
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Private)]
	public string <registerGameChatHandling>g__GetTargetDisplayName|123_3(EChatType _chatType, string _targetId)
	{
		ulong num;
		if (!ulong.TryParse(_targetId, out num))
		{
			throw new ArgumentException("Could not parse chat Discord id '" + _targetId + "'");
		}
		DiscordManager.DiscordUser user = this.GetUser(num);
		return string.Format(Localization.Get("xuiChatTargetWhisper", false, null), user.DisplayName + " [discord] ");
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool <registerGameChatHandling>g__IsValidTarget|123_4(EChatType _chatType, string _targetId)
	{
		if (_targetId == null)
		{
			return false;
		}
		ulong num;
		if (!ulong.TryParse(_targetId, out num))
		{
			throw new ArgumentException("Could not parse chat Discord id '" + _targetId + "' for target validation");
		}
		ulong num2 = num;
		DiscordManager.DiscordUser discordUser = this.LocalUser;
		return num2 != ((discordUser != null) ? discordUser.ID : 0UL);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const ulong DiscordApplicationId = 1296840202995896363UL;

	[PublicizedFrom(EAccessModifier.Private)]
	public const ulong DiscordClientId = 1296840202995896363UL;

	[PublicizedFrom(EAccessModifier.Private)]
	public static DiscordManager instance;

	public readonly DiscordManager.DiscordSettings Settings;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly DiscordManager.DiscordUserSettingsManager UserSettings;

	[PublicizedFrom(EAccessModifier.Private)]
	public Client client;

	[PublicizedFrom(EAccessModifier.Private)]
	public DiscordManager.DiscordUser localUser;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly DiscordManager.DiscordUserMappingManager userMappings;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<ulong, DiscordManager.DiscordUser> knownUsers = new Dictionary<ulong, DiscordManager.DiscordUser>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly DiscordManager.FriendsServerList friendsServerList = new DiscordManager.FriendsServerList();

	[PublicizedFrom(EAccessModifier.Private)]
	public static LoggingSeverity logLevel = 3;

	[PublicizedFrom(EAccessModifier.Private)]
	public static LoggingSeverity logLevelRtc = 3;

	public readonly DiscordManager.AuthAndLoginManager AuthManager;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Regex logMessageMatcher = new Regex("^\\[([^\\]]+)\\] \\[(\\d+)\\] \\(([^)]+)\\): (.*)\\n$", RegexOptions.Compiled | RegexOptions.Singleline);

	[PublicizedFrom(EAccessModifier.Private)]
	public const string UrlTypeDiscordMessageButton = "DiscordMessageButton";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string UrlFieldMessageId = "MessageId";

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly DiscordManager.LobbyInfo globalLobby;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly DiscordManager.LobbyInfo partyLobby;

	public readonly DiscordManager.PresenceManager Presence;

	public readonly DiscordManager.AudioDeviceConfig AudioOutput;

	public readonly DiscordManager.AudioDeviceConfig AudioInput;

	[PublicizedFrom(EAccessModifier.Private)]
	public float nextActivityUpdate;

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityPlayerLocal localPlayer;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int CurrentFileVersion = 1;

	public class AuthAndLoginManager
	{
		public DiscordManager.EDiscordAccountType IsLoggingInWith
		{
			get; [PublicizedFrom(EAccessModifier.Private)]
			set;
		}

		public bool IsLoggingIn
		{
			get
			{
				return this.IsLoggingInWith > DiscordManager.EDiscordAccountType.None;
			}
		}

		public AuthAndLoginManager(DiscordManager _owner)
		{
			this.owner = _owner;
		}

		public void RegisterGameEventHandlers()
		{
			ModEvents.MainMenuOpened.RegisterHandler(new ModEvents.ModEventHandlerDelegate<ModEvents.SMainMenuOpenedData>(this.OnMainMenuOpened));
			ModEvents.GameStarting.RegisterHandler(new ModEvents.ModEventHandlerDelegate<ModEvents.SGameStartingData>(this.OnGameStarting));
			ModEvents.PlayerSpawnedInWorld.RegisterHandler(new ModEvents.ModEventHandlerDelegate<ModEvents.SPlayerSpawnedInWorldData>(this.OnPlayerSpawned));
			ModEvents.WorldShuttingDown.RegisterHandler(new ModEvents.ModEventHandlerDelegate<ModEvents.SWorldShuttingDownData>(this.OnWorldShuttingDown));
		}

		public void RegisterDiscordCallbacks()
		{
			this.owner.client.SetStatusChangedCallback(new Client.OnStatusChanged(this.OnStatusChanged));
			this.owner.client.SetTokenExpirationCallback(new Client.TokenExpirationCallback(this.OnTokenExpiration));
			this.owner.StatusChanged += this.OnDiscordStatusChanged;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void OnMainMenuOpened(ref ModEvents.SMainMenuOpenedData _data)
		{
			this.updateExternalAuthRequestHandler();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void OnDiscordStatusChanged(DiscordManager.EDiscordStatus _status)
		{
			this.updateExternalAuthRequestHandler();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void OnGameStarting(ref ModEvents.SGameStartingData _data)
		{
			this.playerSpawned = false;
			this.updateExternalAuthRequestHandler();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void OnPlayerSpawned(ref ModEvents.SPlayerSpawnedInWorldData _data)
		{
			this.playerSpawned = true;
			this.updateExternalAuthRequestHandler();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void OnWorldShuttingDown(ref ModEvents.SWorldShuttingDownData _data)
		{
			this.playerSpawned = false;
			this.updateExternalAuthRequestHandler();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void updateExternalAuthRequestHandler()
		{
			if (GameManager.IsDedicatedServer || !this.owner.IsInitialized)
			{
				return;
			}
			bool flag = this.owner.Status == DiscordManager.EDiscordStatus.Ready;
			bool isStartingGame = GameManager.Instance.IsStartingGame;
			bool flag2 = GameManager.Instance.GetGameStateManager().IsGameStarted();
			if (DiscordManager.LogLevel < 3)
			{
				Log.Out(string.Format("[Discord] UpdateExternalAuthRequestHandler: Valid={0}, StartingGame={1}, InGame={2}, Spawned={3}", new object[] { flag, isStartingGame, flag2, this.playerSpawned }));
			}
			if (flag && ((!flag2 && !isStartingGame) || this.playerSpawned))
			{
				DiscordManager discordManager = this.owner;
				if (discordManager == null)
				{
					return;
				}
				Client client = discordManager.client;
				if (client == null)
				{
					return;
				}
				client.RegisterAuthorizeRequestCallback(new Client.AuthorizeRequestCallback(this.OnExternalAuthorizeRequest));
				return;
			}
			else
			{
				DiscordManager discordManager2 = this.owner;
				if (discordManager2 == null)
				{
					return;
				}
				Client client2 = discordManager2.client;
				if (client2 == null)
				{
					return;
				}
				client2.RemoveAuthorizeRequestCallback();
				return;
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void OnExternalAuthorizeRequest()
		{
			Log.Out("[Discord] AuthorizeRequest from Discord client");
			string returnWindow = null;
			if (!GameManager.Instance.GetGameStateManager().IsGameStarted())
			{
				returnWindow = LocalPlayerUI.primaryUI.windowManager.GetModalWindow().Id;
			}
			XUiC_DiscordLogin.Open(delegate
			{
				if (string.IsNullOrEmpty(returnWindow))
				{
					return;
				}
				LocalPlayerUI.primaryUI.windowManager.Open(returnWindow, true);
			}, false, false, false, true, (DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5).IsCurrent());
			this.LoginDiscordUser();
		}

		public void LoginWithPlatformDefaultAccountType()
		{
			if (DiscordManager.SupportsProvisionalAccounts)
			{
				this.LoginProvisionalAccount();
				return;
			}
			this.LoginDiscordUser();
		}

		public void AutoLogin()
		{
			switch (this.owner.Settings.LastAccountType)
			{
			case DiscordManager.EDiscordAccountType.None:
				this.LoginWithPlatformDefaultAccountType();
				return;
			case DiscordManager.EDiscordAccountType.Regular:
				this.LoginDiscordUser();
				return;
			case DiscordManager.EDiscordAccountType.Provisional:
				this.LoginProvisionalAccount();
				return;
			default:
				throw new ArgumentOutOfRangeException("LastAccountType");
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void prepareLoginStart(DiscordManager.EDiscordAccountType _accountType)
		{
			this.IsLoggingInWith = _accountType;
			this.fullAccountLoginResult = DiscordManager.EFullAccountLoginResult.None;
			this.provisionalAccountLoginResult = DiscordManager.EProvisionalAccountLoginResult.None;
		}

		public void AbortAuth()
		{
			if (!this.inAuthProcess)
			{
				return;
			}
			if ((DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5).IsCurrent())
			{
				this.owner.client.AbortGetTokenFromDevice();
			}
			else
			{
				this.owner.client.AbortAuthorize();
			}
			this.inAuthProcess = false;
		}

		public void LoginDiscordUser()
		{
			this.owner.Init(false);
			if (this.owner.IsReady && !this.owner.LocalUser.IsProvisionalAccount)
			{
				Log.Out("[Discord] Already logged in");
				return;
			}
			this.prepareLoginStart(DiscordManager.EDiscordAccountType.Regular);
			if (!string.IsNullOrEmpty(this.owner.Settings.AccessToken))
			{
				this.loginWithStoredTokens();
				return;
			}
			if (!string.IsNullOrEmpty(this.owner.Settings.RefreshToken))
			{
				this.refreshToken();
				return;
			}
			Log.Out("[Discord] Logging in with Discord user");
			if ((DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5).IsCurrent())
			{
				this.loginDiscordUserConsole();
				return;
			}
			this.loginDiscordUserPC();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void loginWithStoredTokens()
		{
			Log.Out("[Discord] Logging in with existing access token");
			this.owner.client.UpdateToken(1, this.owner.Settings.AccessToken, new Client.UpdateTokenCallback(this.updateTokenCallback));
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void loginDiscordUserPC()
		{
			AuthorizationCodeVerifier codeVerifier = this.owner.client.CreateAuthorizationCodeVerifier();
			AuthorizationArgs authorizationArgs = new AuthorizationArgs();
			authorizationArgs.SetClientId(1296840202995896363UL);
			authorizationArgs.SetScopes(Client.GetDefaultCommunicationScopes());
			authorizationArgs.SetCodeChallenge(codeVerifier.Challenge());
			DiscordManager.UserAuthorizationResultCallback userAuthorizationResult = this.owner.UserAuthorizationResult;
			if (userAuthorizationResult != null)
			{
				userAuthorizationResult(false, DiscordManager.EFullAccountLoginResult.RequestingAuth, DiscordManager.EProvisionalAccountLoginResult.None, false);
			}
			this.inAuthProcess = true;
			this.owner.client.Authorize(authorizationArgs, delegate(ClientResult _result, string _code, string _uri)
			{
				this.inAuthProcess = false;
				ErrorType errorType = _result.Type();
				int num = _result.ErrorCode();
				DiscordManager.logCallbackInfoWithClientResult("Authorize (PC)", string.Concat(new string[] { "code='", _code, "' uri='", _uri, "'" }), _result, true);
				if (this.owner.client == null)
				{
					return;
				}
				if (errorType != null)
				{
					if (errorType == 7)
					{
						this.fullAccountLoginResult = DiscordManager.EFullAccountLoginResult.AuthCancelled;
						Log.Out("[Discord] Auth aborted");
					}
					else if (num == 5000)
					{
						this.fullAccountLoginResult = DiscordManager.EFullAccountLoginResult.AuthCancelled;
						Log.Out("[Discord] Auth cancelled");
					}
					else
					{
						this.fullAccountLoginResult = DiscordManager.EFullAccountLoginResult.AuthFailed;
						Log.Out("[Discord] Auth failed");
					}
					DiscordManager.UserAuthorizationResultCallback userAuthorizationResult2 = this.owner.UserAuthorizationResult;
					if (userAuthorizationResult2 != null)
					{
						userAuthorizationResult2(false, this.fullAccountLoginResult, DiscordManager.EProvisionalAccountLoginResult.None, false);
					}
					this.loginProvisionalAccountInternal(false);
					return;
				}
				DiscordManager.UserAuthorizationResultCallback userAuthorizationResult3 = this.owner.UserAuthorizationResult;
				if (userAuthorizationResult3 != null)
				{
					userAuthorizationResult3(false, DiscordManager.EFullAccountLoginResult.AuthAccepted, DiscordManager.EProvisionalAccountLoginResult.None, false);
				}
				if (!DiscordManager.SupportsProvisionalAccounts)
				{
					this.owner.client.GetToken(1296840202995896363UL, _code, codeVerifier.Verifier(), _uri, new Client.TokenExchangeCallback(this.tokenExchangeCallbackFullAccount));
					return;
				}
				string authTicket = PlatformManager.CrossplatformPlatform.AuthenticationClient.GetAuthTicket();
				if (string.IsNullOrEmpty(authTicket))
				{
					Log.Error("[Discord] Logging in with merged account failed, could not fetch EOS IdToken");
					this.fullAccountLoginResult = DiscordManager.EFullAccountLoginResult.PlatformError;
					this.invokeAuthResultCallback(false);
					return;
				}
				this.owner.client.GetTokenFromProvisionalMerge(1296840202995896363UL, _code, codeVerifier.Verifier(), _uri, 2, authTicket, new Client.TokenExchangeCallback(this.tokenExchangeCallbackFullAccount));
			});
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void loginDiscordUserConsole()
		{
			DeviceAuthorizationArgs deviceAuthorizationArgs = new DeviceAuthorizationArgs();
			deviceAuthorizationArgs.SetClientId(1296840202995896363UL);
			deviceAuthorizationArgs.SetScopes(Client.GetDefaultCommunicationScopes());
			string authTicket = PlatformManager.CrossplatformPlatform.AuthenticationClient.GetAuthTicket();
			if (string.IsNullOrEmpty(authTicket))
			{
				Log.Error("[Discord] Logging in with merged account failed, could not fetch EOS IdToken");
				this.fullAccountLoginResult = DiscordManager.EFullAccountLoginResult.PlatformError;
				this.invokeAuthResultCallback(false);
				return;
			}
			this.inAuthProcess = true;
			this.owner.client.GetTokenFromDeviceProvisionalMerge(deviceAuthorizationArgs, 2, authTicket, new Client.TokenExchangeCallback(this.tokenExchangeCallbackFullAccount));
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void tokenExchangeCallbackFullAccount(ClientResult _result, string _accessToken, string _refreshToken, AuthorizationTokenType _tokenType, int _expiresIn, string _scope)
		{
			this.inAuthProcess = false;
			DiscordManager.logCallbackInfoWithClientResult("OnTokenExchange (Full)", string.Format("tokenType={0}, expires={1}, scope='{2}'", _tokenType.ToStringCached<AuthorizationTokenType>(), _expiresIn, _scope), _result, true);
			if (_accessToken != "")
			{
				this.owner.Settings.AccessToken = _accessToken;
				this.owner.Settings.RefreshToken = _refreshToken;
				this.owner.client.UpdateToken(1, _accessToken, new Client.UpdateTokenCallback(this.updateTokenCallback));
				return;
			}
			Log.Warning("[Discord] Failed retrieving account token!");
			this.owner.Settings.LastAccountType = DiscordManager.EDiscordAccountType.None;
			this.owner.Settings.AccessToken = null;
			this.fullAccountLoginResult = DiscordManager.EFullAccountLoginResult.TokenExchangeFailed;
			this.loginProvisionalAccountInternal(false);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void refreshToken()
		{
			string refreshToken = this.owner.Settings.RefreshToken;
			if (string.IsNullOrEmpty(refreshToken))
			{
				Log.Warning("[Discord] No refresh token");
				return;
			}
			Log.Out("[Discord] Trying to refresh access token");
			this.owner.Settings.RefreshToken = null;
			this.owner.client.RefreshToken(1296840202995896363UL, refreshToken, new Client.TokenExchangeCallback(this.<refreshToken>g__RefreshTokenExchangeCallback|29_0));
		}

		public void UnmergeAccount()
		{
			this.fullAccountLoginResult = DiscordManager.EFullAccountLoginResult.None;
			this.provisionalAccountLoginResult = DiscordManager.EProvisionalAccountLoginResult.None;
			this.IsLoggingInWith = DiscordManager.EDiscordAccountType.Provisional;
			if (!DiscordManager.SupportsProvisionalAccounts)
			{
				Log.Error("[Discord] Unmerging account only available when running with EOS");
				return;
			}
			if (!this.owner.IsReady || this.owner.LocalUser.IsProvisionalAccount)
			{
				Log.Out("[Discord] Not logged in with full account");
				return;
			}
			string authTicket = PlatformManager.CrossplatformPlatform.AuthenticationClient.GetAuthTicket();
			if (string.IsNullOrEmpty(authTicket))
			{
				Log.Error("[Discord] Unmerging account failed, could not fetch EOS IdToken");
				return;
			}
			this.owner.client.UnmergeIntoProvisionalAccount(1296840202995896363UL, 2, authTicket, delegate(ClientResult _result)
			{
				bool flag = _result.Type() != null;
				DiscordManager.logCallbackInfoWithClientResult("UnmergeIntoProvisionalAccount", null, _result, true);
				if (flag)
				{
					this.invokeAuthResultCallback(false);
					return;
				}
			});
		}

		public void LoginProvisionalAccount()
		{
			this.owner.Init(false);
			this.prepareLoginStart(DiscordManager.EDiscordAccountType.Provisional);
			this.loginProvisionalAccountInternal(false);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void loginProvisionalAccountInternal(bool _isRefresh)
		{
			if (!DiscordManager.SupportsProvisionalAccounts)
			{
				Log.Error("[Discord] Provisional account login only available when running with EOS");
				this.provisionalAccountLoginResult = DiscordManager.EProvisionalAccountLoginResult.NotSupported;
				this.invokeAuthResultCallback(false);
				return;
			}
			if (this.owner.IsReady && !_isRefresh)
			{
				Log.Out("[Discord] Already logged in");
				this.provisionalAccountLoginResult = DiscordManager.EProvisionalAccountLoginResult.Success;
				this.IsLoggingInWith = DiscordManager.EDiscordAccountType.Provisional;
				this.invokeAuthResultCallback(true);
				return;
			}
			if (PlatformManager.CrossplatformPlatform.User.UserStatus != EUserStatus.LoggedIn)
			{
				Log.Out("[Discord] Can not log in with provisional account, not logged in to cross platform provider");
				this.provisionalAccountLoginResult = DiscordManager.EProvisionalAccountLoginResult.PlatformError;
				this.invokeAuthResultCallback(false);
				return;
			}
			Log.Out(_isRefresh ? "[Discord] Refreshing provisional account token" : "[Discord] Logging in with provisional account");
			string authTicket = PlatformManager.CrossplatformPlatform.AuthenticationClient.GetAuthTicket();
			if (string.IsNullOrEmpty(authTicket))
			{
				Log.Error("[Discord] Logging in with provisional account failed, could not fetch EOS IdToken");
				this.provisionalAccountLoginResult = DiscordManager.EProvisionalAccountLoginResult.PlatformError;
				this.invokeAuthResultCallback(false);
				return;
			}
			this.owner.client.GetProvisionalToken(1296840202995896363UL, 2, authTicket, new Client.TokenExchangeCallback(this.<loginProvisionalAccountInternal>g__TokenExchangeCallbackProvisional|33_0));
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void updateTokenCallback(ClientResult _result)
		{
			bool flag = _result.Type() != null;
			DiscordManager.logCallbackInfoWithClientResult("UpdateToken", null, _result, true);
			if (flag)
			{
				Log.Error("[Discord] UpdateToken failed!");
				return;
			}
			this.owner.client.Connect();
		}

		public void Disconnect()
		{
			this.IsLoggingInWith = DiscordManager.EDiscordAccountType.None;
			if (this.owner.client == null)
			{
				return;
			}
			this.owner.leaveLobbies(true);
			if (this.owner.Status != DiscordManager.EDiscordStatus.Disconnected)
			{
				this.owner.client.Disconnect();
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void OnStatusChanged(Client.Status _status, Client.Error _error, int _errorDetail)
		{
			DiscordManager.logCallbackInfo(string.Format("OnStatusChanged status={0} error={1} errorDetail={2}", _status, _error, _errorDetail), (_error == null) ? 3 : 0);
			if (_status == null && _error == 1)
			{
				if (this.IsLoggingInWith == DiscordManager.EDiscordAccountType.Regular)
				{
					this.fullAccountLoginResult = DiscordManager.EFullAccountLoginResult.ConnectionFailed;
				}
				else
				{
					this.provisionalAccountLoginResult = DiscordManager.EProvisionalAccountLoginResult.ConnectionFailed;
				}
				this.invokeAuthResultCallback(false);
			}
			if (_errorDetail == 4004)
			{
				if (!string.IsNullOrEmpty(this.owner.Settings.RefreshToken))
				{
					this.refreshToken();
					return;
				}
				this.loginProvisionalAccountInternal(false);
				return;
			}
			else
			{
				if (_errorDetail == 4003)
				{
					DiscordManager.DiscordUser localUser = this.owner.LocalUser;
					if (localUser == null || !localUser.IsProvisionalAccount)
					{
						this.fullAccountLoginResult = DiscordManager.EFullAccountLoginResult.TokenRevoked;
						this.owner.Settings.AccessToken = null;
						this.owner.Settings.RefreshToken = null;
						this.loginProvisionalAccountInternal(false);
						return;
					}
				}
				switch (_status)
				{
				case 0:
				case 5:
					this.IsLoggingInWith = DiscordManager.EDiscordAccountType.None;
					this.owner.leaveLobbies(false);
					if (this.owner.LocalUser != null)
					{
						this.owner.LocalUser.Dispose();
						this.owner.LocalUser = null;
					}
					this.owner.clearFriends();
					break;
				case 1:
				case 2:
				case 4:
				case 6:
					break;
				case 3:
				{
					UserHandle currentUserV = this.owner.client.GetCurrentUserV2();
					if (currentUserV == null)
					{
						Log.Error("[Discord] StatusChanged=Ready, but could not fetch CurrentUser");
						if (this.IsLoggingInWith == DiscordManager.EDiscordAccountType.Regular)
						{
							this.fullAccountLoginResult = DiscordManager.EFullAccountLoginResult.PlatformError;
						}
						else
						{
							this.provisionalAccountLoginResult = DiscordManager.EProvisionalAccountLoginResult.PlatformError;
						}
						this.invokeAuthResultCallback(false);
						return;
					}
					bool flag = currentUserV.IsProvisional();
					this.owner.Settings.LastAccountType = (flag ? DiscordManager.EDiscordAccountType.Provisional : DiscordManager.EDiscordAccountType.Regular);
					ulong num = currentUserV.Id();
					bool flag2 = this.owner.LocalUser == null || this.owner.LocalUser.ID != num;
					if (flag2)
					{
						DiscordManager.DiscordUser localUser2 = this.owner.LocalUser;
						if (localUser2 != null)
						{
							localUser2.Dispose();
						}
						this.owner.LocalUser = new DiscordManager.DiscordUser(this.owner, num, true);
						this.owner.knownUsers[num] = this.owner.LocalUser;
					}
					this.owner.knownUsers[this.owner.LocalUser.ID] = this.owner.LocalUser;
					if (flag)
					{
						this.owner.client.UpdateProvisionalAccountDisplayName(GamePrefs.GetString(EnumGamePrefs.PlayerName), delegate(ClientResult _result)
						{
							DiscordManager.logCallbackInfoWithClientResult("UpdateProvisionalAccountDisplayName", null, _result, true);
						});
					}
					this.owner.getFriends();
					this.owner.Presence.SetRichPresenceState(null);
					this.owner.Settings.Save();
					if (!flag)
					{
						this.fullAccountLoginResult = DiscordManager.EFullAccountLoginResult.Success;
					}
					else
					{
						this.provisionalAccountLoginResult = DiscordManager.EProvisionalAccountLoginResult.Success;
					}
					if (flag2)
					{
						Log.Out("[Discord] Logged in");
						this.invokeAuthResultCallback(true);
					}
					break;
				}
				default:
					throw new ArgumentOutOfRangeException("_status", _status, null);
				}
				Action<DiscordManager.EDiscordStatus> statusChanged = this.owner.StatusChanged;
				if (statusChanged == null)
				{
					return;
				}
				statusChanged(this.owner.Status);
				return;
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void OnTokenExpiration()
		{
			DiscordManager.logCallbackInfo("OnTokenExpiration", 3);
			if (this.owner.LocalUser == null)
			{
				Log.Out("[Discord] Received token expiration without a logged in user.");
				return;
			}
			if (this.owner.LocalUser.IsProvisionalAccount)
			{
				this.loginProvisionalAccountInternal(true);
				return;
			}
			this.refreshToken();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void invokeAuthResultCallback(bool _success)
		{
			bool flag = this.fullAccountLoginResult == DiscordManager.EFullAccountLoginResult.Success || (this.fullAccountLoginResult == DiscordManager.EFullAccountLoginResult.None && this.provisionalAccountLoginResult == DiscordManager.EProvisionalAccountLoginResult.Success);
			DiscordManager.UserAuthorizationResultCallback userAuthorizationResult = this.owner.UserAuthorizationResult;
			if (userAuthorizationResult != null)
			{
				userAuthorizationResult(true, this.fullAccountLoginResult, this.provisionalAccountLoginResult, flag);
			}
			if (!_success)
			{
				this.IsLoggingInWith = DiscordManager.EDiscordAccountType.None;
			}
		}

		[CompilerGenerated]
		[PublicizedFrom(EAccessModifier.Private)]
		public void <refreshToken>g__RefreshTokenExchangeCallback|29_0(ClientResult _result, string _accessToken, string _refreshToken, AuthorizationTokenType _tokenType, int _expiresIn, string _scope)
		{
			DiscordManager.logCallbackInfoWithClientResult("OnTokenExchange (Refresh)", string.Format("tokenType={0}, expires={1}, scope='{2}'", _tokenType.ToStringCached<AuthorizationTokenType>(), _expiresIn, _scope), _result, true);
			if (_accessToken != "")
			{
				this.owner.Settings.AccessToken = _accessToken;
				this.owner.Settings.RefreshToken = _refreshToken;
				this.owner.client.UpdateToken(1, _accessToken, new Client.UpdateTokenCallback(this.updateTokenCallback));
				return;
			}
			Log.Warning("[Discord] Failed refreshing token!");
			this.owner.Settings.LastAccountType = DiscordManager.EDiscordAccountType.None;
			this.owner.Settings.AccessToken = null;
			this.fullAccountLoginResult = DiscordManager.EFullAccountLoginResult.TokenRefreshFailed;
			this.loginProvisionalAccountInternal(false);
		}

		[CompilerGenerated]
		[PublicizedFrom(EAccessModifier.Private)]
		public void <loginProvisionalAccountInternal>g__TokenExchangeCallbackProvisional|33_0(ClientResult _result, string _accessToken, string _refreshToken, AuthorizationTokenType _tokenType, int _expiresIn, string _scope)
		{
			if (_result.ErrorCode() == 530010)
			{
				this.provisionalAccountLoginResult = DiscordManager.EProvisionalAccountLoginResult.PlatformIdLinkedToDiscordAccount;
				Log.Out("[Discord] Can not login with provisional account, platform ID already linked to a Discord account");
				this.invokeAuthResultCallback(false);
				_result.Dispose();
				return;
			}
			DiscordManager.logCallbackInfoWithClientResult("OnTokenExchange (Provisional)", string.Format("tokenType={0}, expires={1}, scope='{2}'", _tokenType.ToStringCached<AuthorizationTokenType>(), _expiresIn, _scope), _result, true);
			if (_accessToken != "")
			{
				this.owner.client.UpdateToken(1, _accessToken, new Client.UpdateTokenCallback(this.updateTokenCallback));
				return;
			}
			Log.Warning("[Discord] Failed retrieving provisional token!");
			this.provisionalAccountLoginResult = DiscordManager.EProvisionalAccountLoginResult.TokenExchangeFailed;
			this.invokeAuthResultCallback(false);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly DiscordManager owner;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool playerSpawned;

		[PublicizedFrom(EAccessModifier.Private)]
		public DiscordManager.EFullAccountLoginResult fullAccountLoginResult;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool inAuthProcess;

		[PublicizedFrom(EAccessModifier.Private)]
		public DiscordManager.EProvisionalAccountLoginResult provisionalAccountLoginResult;
	}

	public class CallInfo
	{
		public static void LoadSounds()
		{
			LoadManager.LoadAsset<AudioClip>("@:Sounds/UI/ui_discord_join.wav", delegate(AudioClip _o)
			{
				DiscordManager.CallInfo.soundOtherJoin = _o;
			}, null, false, false, false);
			LoadManager.LoadAsset<AudioClip>("@:Sounds/UI/ui_discord_leave.wav", delegate(AudioClip _o)
			{
				DiscordManager.CallInfo.soundOtherLeave = _o;
			}, null, false, false, false);
			LoadManager.LoadAsset<AudioClip>("@:Sounds/UI/ui_discord_kicked.wav", delegate(AudioClip _o)
			{
				DiscordManager.CallInfo.soundSelfNoManualLeave = _o;
			}, null, false, false, false);
		}

		public Call.Status Status
		{
			get; [PublicizedFrom(EAccessModifier.Private)]
			set;
		}

		public bool IsJoined
		{
			get
			{
				return this.call != null && this.Status == 4;
			}
		}

		public Call Call
		{
			get
			{
				return this.call;
			}
		}

		public bool IsSpeaking
		{
			get; [PublicizedFrom(EAccessModifier.Private)]
			set;
		}

		public CallInfo(DiscordManager.LobbyInfo _ownerLobby, DiscordManager _ownerManager)
		{
			this.ownerLobby = _ownerLobby;
			this.ownerManager = _ownerManager;
			this.ownerManager.Settings.VoiceVadModeChanged += this.OnVadModeChanged;
			this.ownerManager.Settings.VoiceVadThresholdChanged += this.OnVadThresholdChanged;
		}

		public void Join()
		{
			if (!this.ownerLobby.IsJoined)
			{
				Log.Error("[Discord] Failed to start call for lobby, lobby not entered yet");
				return;
			}
			this.startedJoining = true;
			this.manualLeave = false;
			this.Status = 1;
			DiscordManager.CallStatusChangedCallback callStatusChanged = this.ownerManager.CallStatusChanged;
			if (callStatusChanged != null)
			{
				callStatusChanged(this, this.Status);
			}
			this.call = this.ownerManager.client.StartCall(this.ownerLobby.Id);
			if (this.call == null)
			{
				Log.Error(string.Format("[Discord] Failed to start call for lobby {0}", this.ownerLobby.Id));
				this.Leave(false);
				return;
			}
			this.SetPushToTalkMode();
			this.call.SetPTTReleaseDelay(200U);
			this.call.SetVADThreshold(this.ownerManager.Settings.VoiceVadModeAuto, (float)this.ownerManager.Settings.VoiceVadThreshold);
			this.call.SetStatusChangedCallback(new Call.OnStatusChanged(this.OnCallStatusChanged));
			this.call.SetParticipantChangedCallback(new Call.OnParticipantChanged(this.OnParticipantChanged));
			this.call.SetOnVoiceStateChangedCallback(new Call.OnVoiceStateChanged(this.OnVoiceStateChanged));
			this.call.SetSpeakingStatusChangedCallback(new Call.OnSpeakingStatusChanged(this.OnSpeakingStatusChanged));
		}

		public void Leave(bool _manual)
		{
			this.startedJoining = false;
			this.manualLeave = _manual;
			this.Status = 0;
			this.IsSpeaking = false;
			this.ownerManager.client.EndCall(this.ownerLobby.Id, delegate
			{
			});
			Call call = this.call;
			if (call != null)
			{
				call.Dispose();
			}
			this.call = null;
			this.UpdateMembers(false);
			DiscordManager.CallChangedCallback callChanged = this.ownerManager.CallChanged;
			if (callChanged == null)
			{
				return;
			}
			callChanged(null);
		}

		public void SetPushToTalkMode()
		{
			this.call.SetAudioMode(this.ownerManager.Settings.VoiceModePtt ? 2 : 1);
		}

		public void SetPushToTalkActive(bool _pushToTalkPressed)
		{
			this.call.SetPTTActive(_pushToTalkPressed);
		}

		public float GetParticipantVolume(DiscordManager.DiscordUser _user)
		{
			Call call = this.call;
			if (call == null)
			{
				return 0f;
			}
			return call.GetParticipantVolume(_user.ID);
		}

		public void SetParticipantVolume(DiscordManager.DiscordUser _user, float _volume)
		{
			Call call = this.call;
			if (call == null)
			{
				return;
			}
			call.SetParticipantVolume(_user.ID, _volume);
		}

		public void UpdateMembers(bool _applySavedVolume = false)
		{
			if (!this.IsJoined)
			{
				this.callMembersCurrent.Clear();
				this.callMembersOld.Clear();
			}
			else
			{
				Dictionary<ulong, DiscordManager.CallInfo.MemberState> dictionary = this.callMembersCurrent;
				Dictionary<ulong, DiscordManager.CallInfo.MemberState> dictionary2 = this.callMembersOld;
				this.callMembersOld = dictionary;
				this.callMembersCurrent = dictionary2;
				this.callMembersCurrent.Clear();
				foreach (ulong num in this.call.GetParticipants())
				{
					DiscordManager.DiscordUser user = this.ownerManager.GetUser(num);
					using (VoiceStateHandle voiceStateHandle = this.call.GetVoiceStateHandle(num))
					{
						DiscordManager.CallInfo.MemberState memberState;
						if (!this.callMembersOld.TryGetValue(num, out memberState))
						{
							memberState = default(DiscordManager.CallInfo.MemberState);
						}
						memberState.Speaking = false;
						memberState.Muted = voiceStateHandle.SelfMute();
						memberState.Deafened = voiceStateHandle.SelfDeaf();
						this.callMembersCurrent.Add(num, memberState);
						if (_applySavedVolume)
						{
							user.Volume = this.ownerManager.UserSettings.GetUserVolume(user.ID);
						}
					}
				}
				this.callMembersOld.Clear();
			}
			DiscordManager.CallMembersChangedCallback callMembersChanged = this.ownerManager.CallMembersChanged;
			if (callMembersChanged == null)
			{
				return;
			}
			callMembersChanged(this);
		}

		public bool TryGetMember(ulong _userId, out DiscordManager.CallInfo.MemberState _state)
		{
			return this.callMembersCurrent.TryGetValue(_userId, out _state);
		}

		public void GetMembers(List<ulong> _target)
		{
			this.callMembersCurrent.CopyKeysTo(_target);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void OnCallStatusChanged(Call.Status _status, Call.Error _error, int _errorDetail)
		{
			DiscordManager.logCallbackInfo(string.Format("Call: Status changed: status={0} error={1} errorDetail={2}", _status.ToStringCached<Call.Status>(), _error.ToStringCached<Call.Error>(), _errorDetail), (_error == null) ? 3 : 0);
			if (this.startedJoining)
			{
				this.Status = _status;
			}
			switch (_status)
			{
			case 0:
			case 6:
				if (!this.manualLeave)
				{
					Manager.PlayXUiSound(DiscordManager.CallInfo.soundSelfNoManualLeave, 1f);
				}
				this.IsSpeaking = false;
				if (this.startedJoining)
				{
					this.Leave(false);
				}
				break;
			case 1:
			case 2:
			case 3:
			case 5:
				break;
			case 4:
			{
				if (DiscordManager.logLevel == 1)
				{
					float inputVolume = this.ownerManager.client.GetInputVolume();
					AudioModeType audioMode = this.call.GetAudioMode();
					VADThresholdSettings vadthreshold = this.call.GetVADThreshold();
					Log.Out(string.Format("[Discord] AUDIO SETTINGS: InputVolume={0}, AudioMode={1}, VAD auto={2}, threshold={3}", new object[]
					{
						inputVolume,
						audioMode.ToStringCached<AudioModeType>(),
						vadthreshold.Automatic(),
						vadthreshold.VadThreshold()
					}));
				}
				this.UpdateMembers(true);
				DiscordManager.CallChangedCallback callChanged = this.ownerManager.CallChanged;
				if (callChanged != null)
				{
					callChanged(this);
				}
				break;
			}
			default:
				throw new ArgumentOutOfRangeException("_status", _status, null);
			}
			DiscordManager.CallStatusChangedCallback callStatusChanged = this.ownerManager.CallStatusChanged;
			if (callStatusChanged == null)
			{
				return;
			}
			callStatusChanged(this, this.Status);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void OnSpeakingStatusChanged(ulong _userId, bool _isPlayingSound)
		{
			DiscordManager.DiscordUser localUser = this.ownerManager.LocalUser;
			ulong? num = ((localUser != null) ? new ulong?(localUser.ID) : null);
			bool flag = (_userId == num.GetValueOrDefault()) & (num != null);
			DiscordManager.logCallbackInfo(string.Format("Call: Speaking state changed: user={0}({1}) isPlaying={2}", _userId, flag ? "SELF" : "other", _isPlayingSound), 3);
			if (flag)
			{
				this.IsSpeaking = _isPlayingSound;
			}
			DiscordManager.CallInfo.MemberState memberState;
			if (!this.callMembersCurrent.TryGetValue(_userId, out memberState))
			{
				Log.Warning(string.Format("[Discord] Speaking status changed for user not in call member list ({0})", _userId));
				return;
			}
			memberState.Speaking = _isPlayingSound;
			this.callMembersCurrent[_userId] = memberState;
			DiscordManager.VoiceStateChangedCallback voiceStateChanged = this.ownerManager.VoiceStateChanged;
			if (voiceStateChanged == null)
			{
				return;
			}
			voiceStateChanged(flag, _userId);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void OnParticipantChanged(ulong _userId, bool _added)
		{
			DiscordManager.DiscordUser localUser = this.ownerManager.LocalUser;
			ulong? num = ((localUser != null) ? new ulong?(localUser.ID) : null);
			bool flag = (_userId == num.GetValueOrDefault()) & (num != null);
			DiscordManager.logCallbackInfo(string.Format("Call: Participant changed: user={0}({1}) added={2}", _userId, flag ? "SELF" : "other", _added), 3);
			this.UpdateMembers(false);
			if (_added)
			{
				DiscordManager.DiscordUser user = this.ownerManager.GetUser(_userId);
				int num2;
				if (!this.ownerManager.userMappings.TryGetEntityId(_userId, out num2))
				{
					return;
				}
				PersistentPlayerList persistentPlayers = GameManager.Instance.persistentPlayers;
				PersistentPlayerData persistentPlayerData = ((persistentPlayers != null) ? persistentPlayers.GetPlayerDataFromEntityID(num2) : null);
				if (persistentPlayerData == null)
				{
					return;
				}
				if (!flag)
				{
					IPlatformUserData orCreate = PlatformUserManager.GetOrCreate(persistentPlayerData.PrimaryId);
					this.UpdateBlockState(user, orCreate.Blocked[EBlockType.VoiceChat].IsBlocked());
					user.Volume = this.ownerManager.UserSettings.GetUserVolume(user.ID);
				}
			}
			if (!flag)
			{
				Manager.PlayXUiSound(_added ? DiscordManager.CallInfo.soundOtherJoin : DiscordManager.CallInfo.soundOtherLeave, 1f);
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void OnVoiceStateChanged(ulong _userId)
		{
			DiscordManager.DiscordUser localUser = this.ownerManager.LocalUser;
			ulong? num = ((localUser != null) ? new ulong?(localUser.ID) : null);
			bool flag = (_userId == num.GetValueOrDefault()) & (num != null);
			DiscordManager.logCallbackInfo(string.Format("Call: Voice state changed: user={0}({1})", _userId, flag ? "SELF" : "other"), 3);
			DiscordManager.CallInfo.MemberState memberState;
			if (!this.callMembersCurrent.TryGetValue(_userId, out memberState))
			{
				Log.Warning(string.Format("[Discord] VoiceState changed for user not in call member list ({0})", _userId));
				return;
			}
			using (VoiceStateHandle voiceStateHandle = this.call.GetVoiceStateHandle(_userId))
			{
				if (voiceStateHandle == null)
				{
					Log.Warning(string.Format("[Discord] VoiceState changed for user {0} but can not get current state", _userId));
				}
				else
				{
					memberState.Muted = voiceStateHandle.SelfMute();
					memberState.Deafened = voiceStateHandle.SelfDeaf();
					this.callMembersCurrent[_userId] = memberState;
					DiscordManager.VoiceStateChangedCallback voiceStateChanged = this.ownerManager.VoiceStateChanged;
					if (voiceStateChanged != null)
					{
						voiceStateChanged(flag, _userId);
					}
				}
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void OnVadModeChanged(bool _auto)
		{
			if (!this.IsJoined)
			{
				return;
			}
			VADThresholdSettings vadthreshold = this.call.GetVADThreshold();
			this.call.SetVADThreshold(_auto, vadthreshold.VadThreshold());
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void OnVadThresholdChanged(int _threshold)
		{
			if (!this.IsJoined)
			{
				return;
			}
			VADThresholdSettings vadthreshold = this.call.GetVADThreshold();
			this.call.SetVADThreshold(vadthreshold.Automatic(), (float)_threshold);
		}

		public void UpdateBlockState(DiscordManager.DiscordUser _user, bool _isBlocked)
		{
			DiscordManager.CallInfo.MemberState memberState;
			if (!this.callMembersCurrent.TryGetValue(_user.ID, out memberState))
			{
				return;
			}
			_user.LocalMuted = _isBlocked;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static AudioClip soundOtherJoin;

		[PublicizedFrom(EAccessModifier.Private)]
		public static AudioClip soundOtherLeave;

		[PublicizedFrom(EAccessModifier.Private)]
		public static AudioClip soundSelfNoManualLeave;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly DiscordManager.LobbyInfo ownerLobby;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly DiscordManager ownerManager;

		[PublicizedFrom(EAccessModifier.Private)]
		public Call call;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool startedJoining;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool manualLeave;

		[PublicizedFrom(EAccessModifier.Private)]
		public Dictionary<ulong, DiscordManager.CallInfo.MemberState> callMembersCurrent = new Dictionary<ulong, DiscordManager.CallInfo.MemberState>();

		[PublicizedFrom(EAccessModifier.Private)]
		public Dictionary<ulong, DiscordManager.CallInfo.MemberState> callMembersOld = new Dictionary<ulong, DiscordManager.CallInfo.MemberState>();

		public struct MemberState
		{
			public bool Speaking;

			public bool Muted;

			public bool Deafened;
		}
	}

	public enum EDiscordStatus
	{
		NotInitialized,
		Disconnected,
		Ready,
		Connecting,
		Disconnecting
	}

	public enum ELobbyType : byte
	{
		Global,
		Party
	}

	public enum EDiscordAccountType
	{
		None,
		Regular,
		Provisional
	}

	public enum EFullAccountLoginResult
	{
		None,
		Success,
		RequestingAuth,
		AuthAccepted,
		AuthCancelled,
		AuthFailed,
		TokenExchangeFailed,
		TokenRefreshFailed,
		TokenRevoked,
		ConnectionFailed,
		PlatformError
	}

	public enum EProvisionalAccountLoginResult
	{
		None,
		Success,
		NotSupported,
		PlatformIdLinkedToDiscordAccount,
		TokenExchangeFailed,
		ConnectionFailed,
		PlatformError
	}

	public enum EAutoJoinVoiceMode
	{
		None,
		Global,
		Party
	}

	public delegate void UserAuthorizationResultCallback(bool _isDone, DiscordManager.EFullAccountLoginResult _fullAccResult, DiscordManager.EProvisionalAccountLoginResult _provisionalAccResult, bool _isExpectedSuccess);

	public delegate void LocalUserChangedCallback(bool _loggedIn);

	public delegate void LobbyStateChangedCallback(DiscordManager.LobbyInfo _lobby, bool _isReady, bool _isJoined);

	public delegate void LobbyMembersChangedCallback(DiscordManager.LobbyInfo _lobby);

	public delegate void CallChangedCallback(DiscordManager.CallInfo _newCall);

	public delegate void CallStatusChangedCallback(DiscordManager.CallInfo _call, Call.Status _callStatus);

	public delegate void CallMembersChangedCallback(DiscordManager.CallInfo _call);

	public delegate void VoiceStateChangedCallback(bool _self, ulong _userId);

	public delegate void SelfMuteStateChangedCallback(bool _selfMute, bool _selfDeaf);

	public delegate void FriendsListChangedCallback();

	public delegate void RelationshipChangedCallback(DiscordManager.DiscordUser _user);

	public delegate void ActivityInviteReceivedCallback(DiscordManager.DiscordUser _user, bool _cleared, ActivityActionTypes _type);

	public delegate void ActivityJoiningCallback();

	public delegate void PendingActionsUpdateCallback(int _pendingActionsCount);

	public delegate void AudioDevicesChangedCallback(DiscordManager.AudioDeviceConfig _inOutConfig);

	[PublicizedFrom(EAccessModifier.Private)]
	public enum LobbyMemberActionType
	{
		Add,
		Remove,
		Update
	}

	public class AudioDeviceConfig
	{
		public Dictionary<string, DiscordManager.DiscordAudioDevice> CurrentAudioDevices
		{
			get; [PublicizedFrom(EAccessModifier.Private)]
			set;
		} = new Dictionary<string, DiscordManager.DiscordAudioDevice>();

		public string ActiveAudioDevice
		{
			get; [PublicizedFrom(EAccessModifier.Private)]
			set;
		}

		public string ConfigAudioDevice
		{
			get
			{
				if (!this.IsOutput)
				{
					return this.owner.Settings.SelectedInputDevice;
				}
				return this.owner.Settings.SelectedOutputDevice;
			}
		}

		public AudioDeviceConfig(DiscordManager _owner, bool _isOutput)
		{
			this.owner = _owner;
			this.IsOutput = _isOutput;
		}

		public void UpdateAudioDeviceList()
		{
			if (this.owner.client == null)
			{
				this.CurrentAudioDevices.Clear();
				this.owner.fireAudioDevicesChanged(this);
				return;
			}
			this.getDevices();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void getDevices()
		{
			if (this.IsOutput)
			{
				this.owner.client.GetOutputDevices(new Client.GetOutputDevicesCallback(this.ApplyDevicesFound));
				return;
			}
			this.owner.client.GetInputDevices(new Client.GetInputDevicesCallback(this.ApplyDevicesFound));
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void swapAndClearAudioDeviceList()
		{
			Dictionary<string, DiscordManager.DiscordAudioDevice> currentAudioDevices = this.CurrentAudioDevices;
			Dictionary<string, DiscordManager.DiscordAudioDevice> dictionary = this.oldAudioDevices;
			this.oldAudioDevices = currentAudioDevices;
			this.CurrentAudioDevices = dictionary;
			this.CurrentAudioDevices.Clear();
		}

		public void ApplyDevicesFound(AudioDevice[] _devices)
		{
			this.swapAndClearAudioDeviceList();
			for (int i = 0; i < _devices.Length; i++)
			{
				DiscordManager.DiscordAudioDevice discordAudioDevice = new DiscordManager.DiscordAudioDevice(_devices[i], this.IsOutput);
				this.CurrentAudioDevices[discordAudioDevice.Identifier] = discordAudioDevice;
			}
			if (this.IsOutput)
			{
				this.owner.client.GetCurrentOutputDevice(new Client.GetCurrentOutputDeviceCallback(this.getCurrentDeviceCallbackFn));
				return;
			}
			this.owner.client.GetCurrentInputDevice(new Client.GetCurrentInputDeviceCallback(this.getCurrentDeviceCallbackFn));
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void getCurrentDeviceCallbackFn(AudioDevice _device)
		{
			bool flag = this.CurrentAudioDevices.Count == this.oldAudioDevices.Count && this.CurrentAudioDevices.Keys.All(new Func<string, bool>(this.oldAudioDevices.ContainsKey));
			this.oldAudioDevices.Clear();
			string text = _device.Id();
			bool flag2 = text == this.ActiveAudioDevice;
			this.ActiveAudioDevice = text;
			Log.Out(string.Format("[Discord] Current {0} device: {1} // {2} // {3}", new object[]
			{
				this.IsOutput ? "output" : "input",
				_device.Id(),
				_device.Name(),
				_device.IsDefault()
			}));
			DiscordManager.DiscordAudioDevice discordAudioDevice;
			if (this.CurrentAudioDevices.TryGetValue(this.ConfigAudioDevice, out discordAudioDevice) && this.ConfigAudioDevice != text)
			{
				Log.Out(string.Format("[Discord] Setting {0} device from config: {1} // {2} // {3}", new object[]
				{
					this.IsOutput ? "output" : "input",
					discordAudioDevice.Identifier,
					discordAudioDevice,
					discordAudioDevice.IsDefault
				}));
				if (this.IsOutput)
				{
					this.owner.client.SetOutputDevice(this.ConfigAudioDevice, delegate(ClientResult _result)
					{
						DiscordManager.logCallbackInfoWithClientResult("SetOutputDevice", null, _result, true);
					});
				}
				else
				{
					this.owner.client.SetInputDevice(this.ConfigAudioDevice, delegate(ClientResult _result)
					{
						DiscordManager.logCallbackInfoWithClientResult("SetInputDevice", null, _result, true);
					});
				}
				this.ActiveAudioDevice = this.ConfigAudioDevice;
			}
			if (!flag || !flag2)
			{
				this.owner.fireAudioDevicesChanged(this);
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly DiscordManager owner;

		public readonly bool IsOutput;

		[PublicizedFrom(EAccessModifier.Private)]
		public Dictionary<string, DiscordManager.DiscordAudioDevice> oldAudioDevices = new Dictionary<string, DiscordManager.DiscordAudioDevice>();
	}

	public class DiscordAudioDevice : IPartyVoice.VoiceAudioDevice
	{
		public DiscordAudioDevice(AudioDevice _device, bool _isOutput)
			: base(_isOutput, _device.IsDefault())
		{
			this.id = _device.Id();
			this.name = _device.Name();
		}

		public override string ToString()
		{
			if (!this.IsDefault)
			{
				return this.name;
			}
			return "(Default) " + this.name;
		}

		public override string Identifier
		{
			get
			{
				return this.id;
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly string id;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly string name;
	}

	public class DiscordSettings
	{
		public event Action<string> OutputDeviceChanged;

		public event Action<string> InputDeviceChanged;

		public event Action<int> OutputVolumeChanged;

		public event Action<int> InputVolumeChanged;

		public event Action<bool> VoiceModePttChanged;

		public event Action<bool> VoiceVadModeChanged;

		public event Action<int> VoiceVadThresholdChanged;

		public bool DiscordFirstTimeInfoShown
		{
			get
			{
				return GamePrefs.GetBool(EnumGamePrefs.DiscordFirstTimeInfoShown);
			}
			set
			{
				GamePrefs.Set(EnumGamePrefs.DiscordFirstTimeInfoShown, value);
			}
		}

		public bool DiscordDisabled
		{
			get
			{
				return GamePrefs.GetBool(EnumGamePrefs.DiscordDisabled);
			}
			set
			{
				GamePrefs.Set(EnumGamePrefs.DiscordDisabled, value);
			}
		}

		public DiscordManager.EDiscordAccountType LastAccountType
		{
			get
			{
				return (DiscordManager.EDiscordAccountType)GamePrefs.GetInt(EnumGamePrefs.DiscordLastAccountType);
			}
			set
			{
				GamePrefs.Set(EnumGamePrefs.DiscordLastAccountType, (int)value);
			}
		}

		public string AccessToken
		{
			get
			{
				return GamePrefs.GetString(EnumGamePrefs.DiscordAccessToken);
			}
			set
			{
				GamePrefs.Set(EnumGamePrefs.DiscordAccessToken, value);
			}
		}

		public string RefreshToken
		{
			get
			{
				return GamePrefs.GetString(EnumGamePrefs.DiscordRefreshToken);
			}
			set
			{
				GamePrefs.Set(EnumGamePrefs.DiscordRefreshToken, value);
			}
		}

		public string SelectedOutputDevice
		{
			get
			{
				return GamePrefs.GetString(EnumGamePrefs.DiscordSelectedOutputDevice);
			}
			set
			{
				GamePrefs.Set(EnumGamePrefs.DiscordSelectedOutputDevice, value);
			}
		}

		public string SelectedInputDevice
		{
			get
			{
				return GamePrefs.GetString(EnumGamePrefs.DiscordSelectedInputDevice);
			}
			set
			{
				GamePrefs.Set(EnumGamePrefs.DiscordSelectedInputDevice, value);
			}
		}

		public int OutputVolume
		{
			get
			{
				return Mathf.RoundToInt(GamePrefs.GetFloat(EnumGamePrefs.DiscordOutputVolume) * 100f);
			}
			set
			{
				GamePrefs.Set(EnumGamePrefs.DiscordOutputVolume, (float)value / 100f);
			}
		}

		public int InputVolume
		{
			get
			{
				return Mathf.RoundToInt(GamePrefs.GetFloat(EnumGamePrefs.DiscordInputVolume) * 100f);
			}
			set
			{
				GamePrefs.Set(EnumGamePrefs.DiscordInputVolume, (float)value / 100f);
			}
		}

		public bool VoiceModePtt
		{
			get
			{
				return GamePrefs.GetBool(EnumGamePrefs.DiscordVoiceModePtt);
			}
			set
			{
				GamePrefs.Set(EnumGamePrefs.DiscordVoiceModePtt, value);
			}
		}

		public bool VoiceVadModeAuto
		{
			get
			{
				return GamePrefs.GetBool(EnumGamePrefs.DiscordVoiceVadModeAuto);
			}
			set
			{
				GamePrefs.Set(EnumGamePrefs.DiscordVoiceVadModeAuto, value);
			}
		}

		public int VoiceVadThreshold
		{
			get
			{
				return GamePrefs.GetInt(EnumGamePrefs.DiscordVoiceVadThreshold);
			}
			set
			{
				GamePrefs.Set(EnumGamePrefs.DiscordVoiceVadThreshold, value);
			}
		}

		public bool DmPrivacyMode
		{
			get
			{
				return GamePrefs.GetBool(EnumGamePrefs.DiscordDmPrivacyMode);
			}
			set
			{
				GamePrefs.Set(EnumGamePrefs.DiscordDmPrivacyMode, value);
			}
		}

		public DiscordManager.EAutoJoinVoiceMode AutoJoinVoiceMode
		{
			get
			{
				return (DiscordManager.EAutoJoinVoiceMode)GamePrefs.GetInt(EnumGamePrefs.DiscordAutoJoinVoiceMode);
			}
			set
			{
				GamePrefs.Set(EnumGamePrefs.DiscordAutoJoinVoiceMode, (int)value);
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public DiscordSettings()
		{
			GamePrefs.OnGamePrefChanged += this.OnGamePrefChanged;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void OnGamePrefChanged(EnumGamePrefs _pref)
		{
			switch (_pref)
			{
			case EnumGamePrefs.DiscordSelectedOutputDevice:
			{
				Action<string> outputDeviceChanged = this.OutputDeviceChanged;
				if (outputDeviceChanged == null)
				{
					return;
				}
				outputDeviceChanged(this.SelectedOutputDevice);
				return;
			}
			case EnumGamePrefs.DiscordSelectedInputDevice:
			{
				Action<string> inputDeviceChanged = this.InputDeviceChanged;
				if (inputDeviceChanged == null)
				{
					return;
				}
				inputDeviceChanged(this.SelectedInputDevice);
				return;
			}
			case EnumGamePrefs.DiscordOutputVolume:
			{
				Action<int> outputVolumeChanged = this.OutputVolumeChanged;
				if (outputVolumeChanged == null)
				{
					return;
				}
				outputVolumeChanged(this.OutputVolume);
				return;
			}
			case EnumGamePrefs.DiscordInputVolume:
			{
				Action<int> inputVolumeChanged = this.InputVolumeChanged;
				if (inputVolumeChanged == null)
				{
					return;
				}
				inputVolumeChanged(this.InputVolume);
				return;
			}
			case EnumGamePrefs.DiscordVoiceModePtt:
			{
				Action<bool> voiceModePttChanged = this.VoiceModePttChanged;
				if (voiceModePttChanged == null)
				{
					return;
				}
				voiceModePttChanged(this.VoiceModePtt);
				return;
			}
			case EnumGamePrefs.DiscordVoiceVadModeAuto:
			{
				Action<bool> voiceVadModeChanged = this.VoiceVadModeChanged;
				if (voiceVadModeChanged == null)
				{
					return;
				}
				voiceVadModeChanged(this.VoiceVadModeAuto);
				return;
			}
			case EnumGamePrefs.DiscordVoiceVadThreshold:
			{
				Action<int> voiceVadThresholdChanged = this.VoiceVadThresholdChanged;
				if (voiceVadThresholdChanged == null)
				{
					return;
				}
				voiceVadThresholdChanged(this.VoiceVadThreshold);
				return;
			}
			default:
				return;
			}
		}

		public static DiscordManager.DiscordSettings Load()
		{
			if (!SdPlayerPrefs.HasKey("DiscordSettings"))
			{
				return new DiscordManager.DiscordSettings();
			}
			DiscordManager.DiscordSettings discordSettings;
			try
			{
				JsonConvert.DeserializeObject<DiscordManager.DiscordSettings.LegacySettings>(SdPlayerPrefs.GetString("DiscordSettings"));
				discordSettings = new DiscordManager.DiscordSettings();
				Log.Out(string.Format("[Discord] Loaded legacy settings with DiscordDisabled={0}", discordSettings.DiscordDisabled));
				GamePrefs.Instance.Save();
			}
			catch (JsonException ex)
			{
				Log.Error("[Discord] Failed loading legacy settings:");
				Log.Exception(ex);
				discordSettings = new DiscordManager.DiscordSettings();
			}
			SdPlayerPrefs.DeleteKey("DiscordSettings");
			return discordSettings;
		}

		public void Save()
		{
			Log.Out(string.Format("[Discord] Saving settings with DiscordDisabled={0}", this.DiscordDisabled));
			GamePrefs.Instance.Save();
		}

		public const string DiscordSettingsPlayerPrefName = "DiscordSettings";

		[PublicizedFrom(EAccessModifier.Private)]
		public class LegacySettings
		{
			public bool DiscordFirstTimeInfoShown
			{
				set
				{
					GamePrefs.Set(EnumGamePrefs.DiscordFirstTimeInfoShown, value);
				}
			}

			public bool DiscordDisabled
			{
				set
				{
					GamePrefs.Set(EnumGamePrefs.DiscordDisabled, value);
				}
			}

			public DiscordManager.EDiscordAccountType LastAccountType
			{
				set
				{
					GamePrefs.Set(EnumGamePrefs.DiscordLastAccountType, (int)value);
				}
			}

			public string AccessToken
			{
				set
				{
					GamePrefs.Set(EnumGamePrefs.DiscordAccessToken, value);
				}
			}

			public string RefreshToken
			{
				set
				{
					GamePrefs.Set(EnumGamePrefs.DiscordRefreshToken, value);
				}
			}

			public string SelectedOutputDevice
			{
				set
				{
					GamePrefs.Set(EnumGamePrefs.DiscordSelectedOutputDevice, value);
				}
			}

			public string SelectedInputDevice
			{
				set
				{
					GamePrefs.Set(EnumGamePrefs.DiscordSelectedInputDevice, value);
				}
			}

			public int OutputVolume
			{
				set
				{
					GamePrefs.Set(EnumGamePrefs.DiscordOutputVolume, (float)value / 100f);
				}
			}

			public int InputVolume
			{
				set
				{
					GamePrefs.Set(EnumGamePrefs.DiscordInputVolume, (float)value / 100f);
				}
			}

			public bool VoiceModePtt
			{
				set
				{
					GamePrefs.Set(EnumGamePrefs.DiscordVoiceModePtt, value);
				}
			}

			public bool VoiceVadModeAuto
			{
				set
				{
					GamePrefs.Set(EnumGamePrefs.DiscordVoiceVadModeAuto, value);
				}
			}

			public int VoiceVadThreshold
			{
				set
				{
					GamePrefs.Set(EnumGamePrefs.DiscordVoiceVadThreshold, value);
				}
			}

			public bool DmPrivacyMode
			{
				set
				{
					GamePrefs.Set(EnumGamePrefs.DiscordDmPrivacyMode, value);
				}
			}

			public DiscordManager.EAutoJoinVoiceMode AutoJoinVoiceMode
			{
				set
				{
					GamePrefs.Set(EnumGamePrefs.DiscordAutoJoinVoiceMode, (int)value);
				}
			}
		}
	}

	public class DiscordUser : IDisposable, IEquatable<DiscordManager.DiscordUser>
	{
		public bool IsProvisionalAccount
		{
			get; [PublicizedFrom(EAccessModifier.Private)]
			set;
		}

		public StatusType DiscordState
		{
			get
			{
				UserHandle userHandle = this.userHandle;
				if (userHandle == null)
				{
					return 1;
				}
				return userHandle.Status();
			}
		}

		public string DiscordStateLocalized
		{
			get
			{
				string text = "discordState";
				UserHandle userHandle = this.userHandle;
				return Localization.Get(text + ((userHandle != null) ? userHandle.Status() : 1).ToStringCached<StatusType>(), false, null);
			}
		}

		public string DisplayName
		{
			get
			{
				return this.playerName ?? this.DiscordDisplayName;
			}
		}

		public string DiscordDisplayName
		{
			get
			{
				return this.discordDisplayName ?? "<unknown>";
			}
		}

		public string DiscordUserName
		{
			get
			{
				UserHandle userHandle = this.userHandle;
				return ((userHandle != null) ? userHandle.Username() : null) ?? "<unknown>";
			}
		}

		public string PlayerName
		{
			get
			{
				return this.playerName ?? "<unknown>";
			}
		}

		public Texture2D Avatar
		{
			get
			{
				return this.avatar;
			}
		}

		public bool InGlobalLobby
		{
			get
			{
				return this.ownerManager.globalLobby.HasMember(this.ID);
			}
		}

		public bool InPartyLobby
		{
			get
			{
				return this.ownerManager.partyLobby.HasMember(this.ID);
			}
		}

		public bool PendingAction
		{
			get
			{
				return this.PendingIncomingJoinRequest || this.PendingIncomingInvite || this.PendingFriendRequest;
			}
		}

		public DiscordUser(DiscordManager _ownerManager, ulong _id, bool _isLocalAccount = false)
		{
			this.ownerManager = _ownerManager;
			this.ID = _id;
			this.IsLocalAccount = _isLocalAccount;
			this.TryUpdateDiscordHandle();
		}

		public void TryUpdateDiscordHandle()
		{
			this.userHandle = ((!this.ownerManager.IsReady) ? null : this.ownerManager.client.GetUser(this.ID));
			if (this.userHandle != null)
			{
				this.IsProvisionalAccount = this.userHandle.IsProvisional();
				this.updateDisplayName();
			}
			this.UpdateRelationship();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void updateDisplayName()
		{
			string text = this.userHandle.DisplayName();
			foreach (char c in text)
			{
				if (c >= '\ud800' && c < '\ue000')
				{
					this.discordDisplayName = this.userHandle.Username();
					return;
				}
			}
			this.discordDisplayName = text;
		}

		public void RequestAvatar()
		{
			if (!this.avatarStartedDownload && this.userHandle != null)
			{
				ThreadManager.StartCoroutine(this.downloadDiscordAvatar());
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public IEnumerator downloadDiscordAvatar()
		{
			this.avatarStartedDownload = true;
			string text = null;
			try
			{
				text = this.userHandle.AvatarUrl(2, 2);
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
			}
			if (string.IsNullOrEmpty(text))
			{
				yield break;
			}
			MicroStopwatch mswDownload = new MicroStopwatch(true);
			UnityWebRequest www = UnityWebRequestTexture.GetTexture(text);
			www.SendWebRequest();
			while (!www.isDone)
			{
				yield return null;
			}
			if (www.result == 1)
			{
				Texture2D texture = ((DownloadHandlerTexture)www.downloadHandler).texture;
				this.avatar = TextureUtils.CloneTexture(texture, false, false, true);
				Object.DestroyImmediate(texture);
				DiscordManager.FriendsListChangedCallback friendsListChanged = this.ownerManager.FriendsListChanged;
				if (friendsListChanged != null)
				{
					friendsListChanged();
				}
				if (DiscordManager.logLevel == 1)
				{
					Log.Out(string.Format("[Discord] Downloading avatar for user {0} took {1} ms. Size: {2} B, resolution: {3} x {4}", new object[]
					{
						this.DiscordDisplayName,
						mswDownload.ElapsedMilliseconds,
						www.downloadedBytes,
						this.avatar.width,
						this.avatar.height
					}));
				}
			}
			else if (DiscordManager.logLevel <= 3)
			{
				Log.Warning("[Discord] Retrieving avatar for user " + this.DiscordDisplayName + " failed: " + www.error);
			}
			yield break;
		}

		public void UpdatePlayerName(EntityPlayer _entity = null)
		{
			if (_entity != null)
			{
				this.playerName = _entity.PlayerDisplayName;
				return;
			}
			int num;
			if (!this.ownerManager.userMappings.TryGetEntityId(this.ID, out num))
			{
				return;
			}
			if (GameManager.Instance.World == null || !GameManager.Instance.World.Players.dict.TryGetValue(num, out _entity))
			{
				return;
			}
			this.playerName = _entity.PlayerDisplayName;
			DiscordManager.FriendsListChangedCallback friendsListChanged = this.ownerManager.FriendsListChanged;
			if (friendsListChanged == null)
			{
				return;
			}
			friendsListChanged();
		}

		public bool InCurrentVoice
		{
			get
			{
				DiscordManager.LobbyInfo activeVoiceLobby = this.ownerManager.ActiveVoiceLobby;
				DiscordManager.CallInfo.MemberState memberState;
				return activeVoiceLobby != null && activeVoiceLobby.VoiceCall.TryGetMember(this.ID, out memberState);
			}
		}

		public double Volume
		{
			get
			{
				return (double)(this.InCurrentVoice ? (this.ownerManager.ActiveVoiceLobby.VoiceCall.GetParticipantVolume(this) / 100f) : 0f);
			}
			set
			{
				if (this.IsLocalAccount)
				{
					return;
				}
				if (!this.InCurrentVoice)
				{
					return;
				}
				this.ownerManager.ActiveVoiceLobby.VoiceCall.SetParticipantVolume(this, Mathf.Clamp((float)value * 100f, 0f, 200f));
				this.ownerManager.UserSettings.SetUserVolume(this.ID, value);
			}
		}

		public bool IsSpeaking
		{
			get
			{
				DiscordManager.LobbyInfo activeVoiceLobby = this.ownerManager.ActiveVoiceLobby;
				DiscordManager.CallInfo.MemberState memberState;
				return activeVoiceLobby != null && activeVoiceLobby.VoiceCall.TryGetMember(this.ID, out memberState) && memberState.Speaking;
			}
		}

		public bool IsMutedLocalOrRemote
		{
			get
			{
				return this.IsMuted || this.LocalMuted;
			}
		}

		public bool LocalMuted
		{
			get
			{
				DiscordManager.LobbyInfo activeVoiceLobby = this.ownerManager.ActiveVoiceLobby;
				bool? flag;
				if (activeVoiceLobby == null)
				{
					flag = null;
				}
				else
				{
					Call call = activeVoiceLobby.VoiceCall.Call;
					flag = ((call != null) ? new bool?(call.GetLocalMute(this.ID)) : null);
				}
				bool? flag2 = flag;
				return flag2.GetValueOrDefault();
			}
			set
			{
				DiscordManager.LobbyInfo activeVoiceLobby = this.ownerManager.ActiveVoiceLobby;
				if (activeVoiceLobby == null)
				{
					return;
				}
				Call call = activeVoiceLobby.VoiceCall.Call;
				if (call == null)
				{
					return;
				}
				call.SetLocalMute(this.ID, value);
			}
		}

		public bool IsMuted
		{
			get
			{
				DiscordManager.LobbyInfo activeVoiceLobby = this.ownerManager.ActiveVoiceLobby;
				DiscordManager.CallInfo.MemberState memberState;
				return activeVoiceLobby != null && activeVoiceLobby.VoiceCall.TryGetMember(this.ID, out memberState) && memberState.Muted;
			}
		}

		public bool IsDeafened
		{
			get
			{
				DiscordManager.LobbyInfo activeVoiceLobby = this.ownerManager.ActiveVoiceLobby;
				DiscordManager.CallInfo.MemberState memberState;
				return activeVoiceLobby != null && activeVoiceLobby.VoiceCall.TryGetMember(this.ID, out memberState) && memberState.Deafened;
			}
		}

		public IPartyVoice.EVoiceMemberState VoiceState
		{
			get
			{
				DiscordManager.LobbyInfo activeVoiceLobby = this.ownerManager.ActiveVoiceLobby;
				DiscordManager.CallInfo.MemberState memberState;
				if (activeVoiceLobby == null || !activeVoiceLobby.VoiceCall.TryGetMember(this.ID, out memberState))
				{
					return IPartyVoice.EVoiceMemberState.Disabled;
				}
				if (this.LocalMuted || memberState.Muted)
				{
					return IPartyVoice.EVoiceMemberState.Muted;
				}
				if (memberState.Speaking)
				{
					return IPartyVoice.EVoiceMemberState.VoiceActive;
				}
				return IPartyVoice.EVoiceMemberState.Normal;
			}
		}

		public RelationshipType DiscordRelationship
		{
			get; [PublicizedFrom(EAccessModifier.Private)]
			set;
		}

		public RelationshipType GameRelationship
		{
			get; [PublicizedFrom(EAccessModifier.Private)]
			set;
		}

		public bool IsDiscordFriend
		{
			[PublicizedFrom(EAccessModifier.Private)]
			get
			{
				return this.DiscordRelationship == 1;
			}
		}

		public bool IsGameFriend
		{
			[PublicizedFrom(EAccessModifier.Private)]
			get
			{
				return this.GameRelationship == 1;
			}
		}

		public bool IsFriend
		{
			get
			{
				return this.IsDiscordFriend || this.IsGameFriend;
			}
		}

		public bool IsBlocked
		{
			get
			{
				return this.DiscordRelationship == 2;
			}
		}

		public bool PendingFriendRequest
		{
			get
			{
				return !this.isSpamRequest && (this.DiscordRelationship == 3 || this.GameRelationship == 3);
			}
		}

		public void UpdateRelationship()
		{
			if (this.ownerManager.IsReady)
			{
				RelationshipHandle relationshipHandle = this.ownerManager.client.GetRelationshipHandle(this.ID);
				this.DiscordRelationship = relationshipHandle.DiscordRelationshipType();
				this.GameRelationship = relationshipHandle.GameRelationshipType();
				this.isSpamRequest = relationshipHandle.IsSpamRequest();
			}
			else
			{
				this.DiscordRelationship = 0;
				this.GameRelationship = 0;
				this.isSpamRequest = false;
			}
			if (this.IsFriend || this.IsBlocked || this.PendingFriendRequest)
			{
				this.RequestAvatar();
			}
		}

		public void SendFriendRequest(bool _gameFriend)
		{
			if (!this.ownerManager.IsReady)
			{
				return;
			}
			string text = (_gameFriend ? "game" : "Discord");
			switch (_gameFriend ? this.GameRelationship : this.DiscordRelationship)
			{
			case 1:
				Log.Out(string.Format("[Discord] Not sending {0} friend request (already friends) to {1}", text, this));
				return;
			case 3:
				Log.Out(string.Format("[Discord] Accepting {0} friend request from {1}", text, this));
				goto IL_009C;
			case 4:
				Log.Out(string.Format("[Discord] Not sending {0} friend request (already sent) to {1}", text, this));
				return;
			}
			Log.Out(string.Format("[Discord] Sending {0} friend request to {1}", text, this));
			IL_009C:
			if (_gameFriend)
			{
				this.ownerManager.client.SendGameFriendRequestById(this.ID, new Client.UpdateRelationshipCallback(DiscordManager.DiscordUser.<SendFriendRequest>g__SendRequestCallback|77_0));
				return;
			}
			this.ownerManager.client.SendDiscordFriendRequestById(this.ID, new Client.UpdateRelationshipCallback(DiscordManager.DiscordUser.<SendFriendRequest>g__SendRequestCallback|77_0));
		}

		public void DeclineFriendRequest(bool _gameFriend)
		{
			if (!this.ownerManager.IsReady)
			{
				return;
			}
			string text = (_gameFriend ? "game" : "Discord");
			if ((_gameFriend ? this.GameRelationship : this.DiscordRelationship) != 3)
			{
				Log.Out(string.Format("[Discord] Not rejecting {0} friend request (no pending request) from {1}", text, this));
				return;
			}
			Log.Out(string.Format("[Discord] Rejecting {0} friend request from {1}", text, this));
			if (_gameFriend)
			{
				this.ownerManager.client.RejectGameFriendRequest(this.ID, new Client.UpdateRelationshipCallback(DiscordManager.DiscordUser.<DeclineFriendRequest>g__RejectRequestCallback|78_0));
				return;
			}
			this.ownerManager.client.RejectDiscordFriendRequest(this.ID, new Client.UpdateRelationshipCallback(DiscordManager.DiscordUser.<DeclineFriendRequest>g__RejectRequestCallback|78_0));
		}

		public void RemoveFriend()
		{
			if (!this.ownerManager.IsReady)
			{
				return;
			}
			if (!this.IsFriend)
			{
				Log.Out(string.Format("[Discord] Not removing friend (neither a game nor Discord friend): {0}", this));
				return;
			}
			this.ownerManager.client.RemoveDiscordAndGameFriend(this.ID, new Client.UpdateRelationshipCallback(DiscordManager.DiscordUser.<RemoveFriend>g__RemoveFriendCallback|79_0));
		}

		public void BlockUser()
		{
			if (!this.ownerManager.IsReady)
			{
				return;
			}
			if (this.DiscordRelationship == 2)
			{
				Log.Out(string.Format("[Discord] Not blocking user (already blocked): {0}", this));
				return;
			}
			this.ownerManager.client.BlockUser(this.ID, new Client.UpdateRelationshipCallback(DiscordManager.DiscordUser.<BlockUser>g__BlockUserCallback|80_0));
		}

		public void UnblockUser()
		{
			if (!this.ownerManager.IsReady)
			{
				return;
			}
			if (this.DiscordRelationship != 2)
			{
				Log.Out(string.Format("[Discord] Not unblocking user (not blocked): {0}", this));
				return;
			}
			this.ownerManager.client.UnblockUser(this.ID, new Client.UpdateRelationshipCallback(DiscordManager.DiscordUser.<UnblockUser>g__UnblockUserUserCallback|81_0));
		}

		public Activity Activity
		{
			get; [PublicizedFrom(EAccessModifier.Private)]
			set;
		}

		public bool PendingOutgoingJoinRequest { get; set; }

		public bool PendingIncomingJoinRequest
		{
			get
			{
				return this.pendingIncomingJoinRequest;
			}
			set
			{
				if (value == this.pendingIncomingJoinRequest)
				{
					return;
				}
				this.pendingIncomingJoinRequest = value;
				DiscordManager.ActivityInviteReceivedCallback activityInviteReceived = this.ownerManager.ActivityInviteReceived;
				if (activityInviteReceived == null)
				{
					return;
				}
				activityInviteReceived(this, !value, 5);
			}
		}

		public bool PendingIncomingInvite
		{
			get
			{
				return this.incomingInviteActivity != null;
			}
		}

		public bool JoinableActivity
		{
			get
			{
				Activity activity = this.Activity;
				return ((activity != null) ? activity.Party() : null) != null;
			}
		}

		public bool InGame
		{
			get
			{
				return this.Activity != null;
			}
		}

		public bool InSameSession
		{
			get
			{
				int num;
				return this.ownerManager.userMappings.TryGetEntityId(this.ID, out num);
			}
		}

		public void UpdatePresenceInfo()
		{
			if (!this.ownerManager.IsReady)
			{
				this.Activity = null;
				return;
			}
			this.Activity = this.userHandle.GameActivity();
			this.logPresenceInfo();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void logPresenceInfo()
		{
			StatusType statusType = this.userHandle.Status();
			string text = "<null>";
			if (this.Activity != null)
			{
				ulong? num = this.Activity.ApplicationId();
				string text2 = this.Activity.Details();
				string text3 = this.Activity.Name();
				string text4 = this.Activity.State();
				ActivityTypes activityTypes = this.Activity.Type();
				using (ActivityAssets activityAssets = this.Activity.Assets())
				{
					using (ActivityParty activityParty = this.Activity.Party())
					{
						string text5 = "null";
						if (activityParty != null)
						{
							text5 = string.Format("id={0}, size={1}, max={2}", activityParty.Id(), activityParty.CurrentSize(), activityParty.MaxSize());
						}
						using (ActivitySecrets activitySecrets = this.Activity.Secrets())
						{
							string text6 = ((activitySecrets != null) ? activitySecrets.Join() : null) ?? "null";
							using (ActivityTimestamps activityTimestamps = this.Activity.Timestamps())
							{
								string text7 = "null";
								if (activityTimestamps != null)
								{
									text7 = string.Format("start={0}, end={1}", activityTimestamps.Start(), activityTimestamps.End());
								}
								text = string.Format(" appId={0}, type={1}, name='{2}', state={3}, details='{4}', assets={5}, party=<{6}>, secrets.join={7}, timestamps=<{8}> ", new object[]
								{
									num,
									activityTypes.ToStringCached<ActivityTypes>(),
									text3,
									text4,
									text2,
									activityAssets != null,
									text5,
									text6,
									text7
								});
							}
						}
					}
				}
			}
			DiscordManager.logCallbackInfo(string.Format("OnPresenceChanged: user={0}, status={1}, activity=<{2}>", this, statusType.ToStringCached<StatusType>(), text), 3);
		}

		public void SetIncomingInviteActivity(ActivityInvite _invite)
		{
			this.incomingInviteActivity = _invite;
			DiscordManager.ActivityInviteReceivedCallback activityInviteReceived = this.ownerManager.ActivityInviteReceived;
			if (activityInviteReceived == null)
			{
				return;
			}
			activityInviteReceived(this, _invite == null, 1);
		}

		public void SendInvite()
		{
			if (!this.ownerManager.IsReady)
			{
				return;
			}
			Log.Out(string.Format("[Discord] Sending invite to {0}", this));
			this.PendingIncomingJoinRequest = false;
			this.ownerManager.client.SendActivityInvite(this.ID, "", delegate(ClientResult _result)
			{
				DiscordManager.logCallbackInfoWithClientResult("SendActivityInvite", null, _result, true);
			});
		}

		public void SendJoinRequest()
		{
			if (!this.ownerManager.IsReady)
			{
				return;
			}
			Log.Out(string.Format("[Discord] Sending join request to {0}", this));
			this.ownerManager.client.SendActivityJoinRequest(this.ID, delegate(ClientResult _result)
			{
				DiscordManager.logCallbackInfoWithClientResult("SendActivityJoinRequest", null, _result, true);
				this.PendingOutgoingJoinRequest = true;
			});
		}

		public void DeclineJoinRequest()
		{
			if (!this.PendingIncomingJoinRequest)
			{
				Log.Out(string.Format("[Discord] Trying to decline incoming join request without first receiving a request from {0}", this));
				return;
			}
			this.PendingIncomingJoinRequest = false;
		}

		public void DeclineInvite()
		{
			if (!this.PendingIncomingInvite)
			{
				Log.Out(string.Format("[Discord] Trying to decline invite without first receiving an invite from {0}", this));
				return;
			}
			this.incomingInviteActivity = null;
			DiscordManager.ActivityInviteReceivedCallback activityInviteReceived = this.ownerManager.ActivityInviteReceived;
			if (activityInviteReceived == null)
			{
				return;
			}
			activityInviteReceived(this, true, 1);
		}

		public void AcceptInvite(ActivityInvite _invite = null)
		{
			if (!this.ownerManager.IsReady)
			{
				return;
			}
			if (_invite == null)
			{
				_invite = this.incomingInviteActivity;
			}
			this.incomingInviteActivity = null;
			if (_invite == null)
			{
				Log.Out(string.Format("[Discord] Trying to accept invite without first receiving an invite from {0}", this));
				return;
			}
			Log.Out(string.Format("[Discord] Accepting invite from {0}", this));
			this.PendingOutgoingJoinRequest = false;
			DiscordManager.ActivityInviteReceivedCallback activityInviteReceived = this.ownerManager.ActivityInviteReceived;
			if (activityInviteReceived != null)
			{
				activityInviteReceived(this, true, 1);
			}
			this.ownerManager.client.AcceptActivityInvite(_invite, delegate(ClientResult _result, string _secret)
			{
				DiscordManager.logCallbackInfoWithClientResult("AcceptActivityInvite", "secret=" + _secret, _result, true);
				_invite.Dispose();
			});
		}

		public override string ToString()
		{
			return string.Format("<Id={0}, local={1}, Discord='{2}', Player='{3}', DcRel={4}, GameRel={5}>", new object[]
			{
				this.ID,
				this.IsLocalAccount,
				this.DiscordDisplayName,
				this.PlayerName,
				this.DiscordRelationship.ToStringCached<RelationshipType>(),
				this.GameRelationship.ToStringCached<RelationshipType>()
			});
		}

		public void Dispose()
		{
			UserHandle userHandle = this.userHandle;
			if (userHandle != null)
			{
				userHandle.Dispose();
			}
			Activity activity = this.Activity;
			if (activity != null)
			{
				activity.Dispose();
			}
			GC.SuppressFinalize(this);
		}

		public bool Equals(DiscordManager.DiscordUser _other)
		{
			return _other != null && (this == _other || this.ID == _other.ID);
		}

		public override bool Equals(object _obj)
		{
			return _obj != null && (this == _obj || (!(_obj.GetType() != base.GetType()) && this.Equals((DiscordManager.DiscordUser)_obj)));
		}

		public override int GetHashCode()
		{
			return this.ID.GetHashCode();
		}

		[CompilerGenerated]
		[PublicizedFrom(EAccessModifier.Internal)]
		public static void <SendFriendRequest>g__SendRequestCallback|77_0(ClientResult _result)
		{
			DiscordManager.logCallbackInfoWithClientResult("Send*FriendRequestById", null, _result, true);
		}

		[CompilerGenerated]
		[PublicizedFrom(EAccessModifier.Internal)]
		public static void <DeclineFriendRequest>g__RejectRequestCallback|78_0(ClientResult _result)
		{
			DiscordManager.logCallbackInfoWithClientResult("Reject*FriendRequest", null, _result, true);
		}

		[CompilerGenerated]
		[PublicizedFrom(EAccessModifier.Internal)]
		public static void <RemoveFriend>g__RemoveFriendCallback|79_0(ClientResult _result)
		{
			DiscordManager.logCallbackInfoWithClientResult("RemoveDiscordAndGameFriend", null, _result, true);
		}

		[CompilerGenerated]
		[PublicizedFrom(EAccessModifier.Internal)]
		public static void <BlockUser>g__BlockUserCallback|80_0(ClientResult _result)
		{
			DiscordManager.logCallbackInfoWithClientResult("BlockUser", null, _result, true);
		}

		[CompilerGenerated]
		[PublicizedFrom(EAccessModifier.Internal)]
		public static void <UnblockUser>g__UnblockUserUserCallback|81_0(ClientResult _result)
		{
			DiscordManager.logCallbackInfoWithClientResult("UnblockUser", null, _result, true);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly DiscordManager ownerManager;

		[PublicizedFrom(EAccessModifier.Private)]
		public UserHandle userHandle;

		[PublicizedFrom(EAccessModifier.Private)]
		public string discordDisplayName;

		public readonly ulong ID;

		public readonly bool IsLocalAccount;

		[PublicizedFrom(EAccessModifier.Private)]
		public string playerName;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool avatarStartedDownload;

		[PublicizedFrom(EAccessModifier.Private)]
		public Texture2D avatar;

		public bool MessageSentFromGame;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool isSpamRequest;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool pendingIncomingJoinRequest;

		[PublicizedFrom(EAccessModifier.Private)]
		public ActivityInvite incomingInviteActivity;
	}

	public class DiscordUserMappingManager
	{
		public DiscordUserMappingManager(DiscordManager _ownerManager)
		{
			this.ownerManager = _ownerManager;
		}

		public void UpdateMapping(int _entityId, bool _remove, ulong _discordId)
		{
			ulong num;
			if (this.entityIdToDiscordId.TryGetValue(_entityId, out num))
			{
				this.discordIdToEntityId.Remove(num);
			}
			if (_remove)
			{
				this.entityIdToDiscordId.Remove(_entityId);
				return;
			}
			this.entityIdToDiscordId[_entityId] = _discordId;
			this.discordIdToEntityId[_discordId] = _entityId;
		}

		public bool TryGetDiscordId(int _entity, out ulong _discordId)
		{
			return this.entityIdToDiscordId.TryGetValue(_entity, out _discordId);
		}

		public bool TryGetEntityId(ulong _discordId, out int _entity)
		{
			return this.discordIdToEntityId.TryGetValue(_discordId, out _entity);
		}

		public void SendMappingsToClient(ClientInfo _clientInfo)
		{
			List<int> list = new List<int>();
			List<ulong> list2 = new List<ulong>();
			foreach (KeyValuePair<int, ulong> keyValuePair in this.entityIdToDiscordId)
			{
				int num;
				ulong num2;
				keyValuePair.Deconstruct(out num, out num2);
				int num3 = num;
				ulong num4 = num2;
				list.Add(num3);
				list2.Add(num4);
			}
			_clientInfo.SendPackage(NetPackageManager.GetPackage<NetPackageDiscordIdMappings>().Setup(list, list2));
		}

		public void Clear()
		{
			this.discordIdToEntityId.Clear();
			this.entityIdToDiscordId.Clear();
		}

		public void GetAll(Action<int, ulong> _callback)
		{
			if (GameManager.Instance.World == null)
			{
				return;
			}
			foreach (KeyValuePair<int, ulong> keyValuePair in this.entityIdToDiscordId)
			{
				int num;
				ulong num2;
				keyValuePair.Deconstruct(out num, out num2);
				int num3 = num;
				ulong num4 = num2;
				_callback(num3, num4);
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly DiscordManager ownerManager;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly Dictionary<ulong, int> discordIdToEntityId = new Dictionary<ulong, int>();

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly Dictionary<int, ulong> entityIdToDiscordId = new Dictionary<int, ulong>();
	}

	public class DiscordUserSettingsManager
	{
		public double GetUserVolume(ulong _userId)
		{
			int num;
			if (this.userVolumes.TryGetValue(_userId, out num))
			{
				return (double)num / 100.0;
			}
			return 1.0;
		}

		public void SetUserVolume(ulong _userId, double _volume)
		{
			int num = Mathf.RoundToInt((float)(_volume * 100.0));
			num = Mathf.Clamp(num, 0, 200);
			if (num >= 99 && num <= 101)
			{
				this.userVolumes.Remove(_userId);
				return;
			}
			this.userVolumes[_userId] = num;
		}

		public static string DataFilePath
		{
			[PublicizedFrom(EAccessModifier.Private)]
			get
			{
				return GameIO.GetUserGameDataDir() + "/DiscordUserSettings.dat";
			}
		}

		public static DiscordManager.DiscordUserSettingsManager Load()
		{
			DiscordManager.DiscordUserSettingsManager discordUserSettingsManager = new DiscordManager.DiscordUserSettingsManager();
			if (!SdFile.Exists(DiscordManager.DiscordUserSettingsManager.DataFilePath))
			{
				return discordUserSettingsManager;
			}
			DiscordManager.DiscordUserSettingsManager discordUserSettingsManager2;
			try
			{
				using (Stream stream = SdFile.OpenRead(DiscordManager.DiscordUserSettingsManager.DataFilePath))
				{
					using (PooledBinaryReader pooledBinaryReader = MemoryPools.poolBinaryReader.AllocSync(false))
					{
						pooledBinaryReader.SetBaseStream(stream);
						pooledBinaryReader.ReadInt32();
						int num = pooledBinaryReader.ReadInt32();
						for (int i = 0; i < num; i++)
						{
							ulong num2 = pooledBinaryReader.ReadUInt64();
							int num3 = pooledBinaryReader.ReadInt32();
							discordUserSettingsManager.userVolumes[num2] = num3;
						}
						discordUserSettingsManager2 = discordUserSettingsManager;
					}
				}
			}
			catch (Exception ex)
			{
				Log.Error("[Discord] Failed loading UserSettings file: " + ex.Message);
				Log.Exception(ex);
				discordUserSettingsManager2 = new DiscordManager.DiscordUserSettingsManager();
			}
			return discordUserSettingsManager2;
		}

		public void Save()
		{
			using (Stream stream = SdFile.Open(DiscordManager.DiscordUserSettingsManager.DataFilePath, FileMode.Create, FileAccess.Write, FileShare.Read))
			{
				using (PooledBinaryWriter pooledBinaryWriter = MemoryPools.poolBinaryWriter.AllocSync(false))
				{
					pooledBinaryWriter.SetBaseStream(stream);
					pooledBinaryWriter.Write(1);
					pooledBinaryWriter.Write(this.userVolumes.Count);
					foreach (KeyValuePair<ulong, int> keyValuePair in this.userVolumes)
					{
						ulong num;
						int num2;
						keyValuePair.Deconstruct(out num, out num2);
						ulong num3 = num;
						int num4 = num2;
						pooledBinaryWriter.Write(num3);
						pooledBinaryWriter.Write(num4);
					}
				}
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly Dictionary<ulong, int> userVolumes = new Dictionary<ulong, int>();
	}

	public class FriendsServerList : IServerListInterface
	{
		public bool IsPrefiltered
		{
			get
			{
				return false;
			}
		}

		public bool IsRefreshing
		{
			get
			{
				return this.isRefreshing && this.sessionSearchCount > 0;
			}
		}

		public void Init(IPlatform _owner)
		{
			IPlatform crossplatformPlatform = PlatformManager.CrossplatformPlatform;
			this.serverLookup = ((crossplatformPlatform != null) ? crossplatformPlatform.ServerLookupInterface : null);
			if (this.serverLookup == null)
			{
				Log.Warning("[Discord] No crossplatform server lookup interface found, friends session search is not possible");
			}
		}

		public void RegisterGameServerFoundCallback(GameServerFoundCallback _serverFound, MaxResultsReachedCallback _maxResultsCallback, ServerSearchErrorCallback _sessionSearchErrorCallback)
		{
			if (this.serverLookup == null)
			{
				return;
			}
			this.gameServerFoundCallback = _serverFound;
		}

		public void StartSearch(IList<IServerListInterface.ServerFilter> _activeFilters)
		{
			if (this.serverLookup == null)
			{
				return;
			}
			Log.Out("[Discord] FriendsServerList starting search");
			this.isRefreshing = true;
			ThreadManager.StartCoroutine(this.refreshFriendsCo());
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public IEnumerator refreshFriendsCo()
		{
			while (this.isRefreshing && this.gameServerFoundCallback != null)
			{
				this.users.Clear();
				DiscordManager.Instance.GetFriends(this.users);
				using (HashSet<DiscordManager.DiscordUser>.Enumerator enumerator = this.users.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						DiscordManager.DiscordUser discordUser = enumerator.Current;
						Activity activity = discordUser.Activity;
						string text;
						if (activity == null)
						{
							text = null;
						}
						else
						{
							ActivityParty activityParty = activity.Party();
							text = ((activityParty != null) ? activityParty.Id() : null);
						}
						string text2 = text;
						if (!string.IsNullOrEmpty(text2))
						{
							Log.Warning("[DISCORD] LOOKING UP SESSION FROM USER: " + discordUser.DiscordUserName);
							GameServerInfo gameServerInfo = new GameServerInfo();
							gameServerInfo.SetValue(GameInfoString.UniqueId, text2);
							Interlocked.Increment(ref this.sessionSearchCount);
							this.serverLookup.GetSingleServerDetails(gameServerInfo, EServerRelationType.Friends, new GameServerFoundCallback(this.OnServerFound));
						}
					}
					goto IL_0108;
				}
				goto IL_00F1;
				IL_0108:
				if (this.sessionSearchCount <= 0)
				{
					yield return new WaitForSeconds(3f);
					continue;
				}
				IL_00F1:
				yield return null;
				goto IL_0108;
			}
			yield break;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void OnServerFound(IPlatform _sourcePlatform, GameServerInfo _info, EServerRelationType _source)
		{
			Interlocked.Decrement(ref this.sessionSearchCount);
			if (_info == null || this.gameServerFoundCallback == null)
			{
				return;
			}
			Log.Warning("[DISCORD] FOUND SESSION: " + _info.GetValue(GameInfoString.GameHost));
			_info.IsFriends = true;
			GameServerFoundCallback gameServerFoundCallback = this.gameServerFoundCallback;
			if (gameServerFoundCallback == null)
			{
				return;
			}
			gameServerFoundCallback(_sourcePlatform, _info, _source);
		}

		public void StopSearch()
		{
			this.isRefreshing = false;
		}

		public void Disconnect()
		{
			this.StopSearch();
			this.gameServerFoundCallback = null;
		}

		public void GetSingleServerDetails(GameServerInfo _serverInfo, EServerRelationType _relation, GameServerFoundCallback _callback)
		{
			throw new NotImplementedException();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public GameServerFoundCallback gameServerFoundCallback;

		[PublicizedFrom(EAccessModifier.Private)]
		public IServerListInterface serverLookup;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool isRefreshing;

		[PublicizedFrom(EAccessModifier.Private)]
		public int sessionSearchCount;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly HashSet<DiscordManager.DiscordUser> users = new HashSet<DiscordManager.DiscordUser>();
	}

	public class LobbyInfo
	{
		public string Secret
		{
			get
			{
				return this.secret;
			}
			set
			{
				if (value == this.secret)
				{
					return;
				}
				this.secret = value;
				DiscordManager.LobbyStateChangedCallback lobbyStateChanged = this.ownerManager.LobbyStateChanged;
				if (lobbyStateChanged == null)
				{
					return;
				}
				lobbyStateChanged(this, this.IsReady, this.IsJoined);
			}
		}

		public bool IsReady
		{
			get
			{
				return !string.IsNullOrEmpty(this.Secret);
			}
		}

		public bool IsJoined
		{
			get
			{
				return this.Id > 0UL;
			}
		}

		public ulong Id
		{
			get
			{
				return this.id;
			}
			[PublicizedFrom(EAccessModifier.Private)]
			set
			{
				if (value == this.id)
				{
					return;
				}
				this.id = value;
				DiscordManager.LobbyStateChangedCallback lobbyStateChanged = this.ownerManager.LobbyStateChanged;
				if (lobbyStateChanged == null)
				{
					return;
				}
				lobbyStateChanged(this, this.IsReady, this.IsJoined);
			}
		}

		public DiscordManager.CallInfo VoiceCall
		{
			get
			{
				return this.voiceCall;
			}
		}

		public bool IsInVoice
		{
			get
			{
				return this.voiceCall.IsJoined;
			}
		}

		public LobbyInfo(DiscordManager _ownerManager, DiscordManager.ELobbyType _lobbyType)
		{
			this.ownerManager = _ownerManager;
			this.LobbyType = _lobbyType;
			this.voiceCall = new DiscordManager.CallInfo(this, this.ownerManager);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void Finalize()
		{
			try
			{
				LobbyHandle lobbyHandle = this.handle;
				if (lobbyHandle != null)
				{
					lobbyHandle.Dispose();
				}
			}
			finally
			{
				base.Finalize();
			}
		}

		public void Join(bool _errorWithoutSecret = true)
		{
			if (this.IsJoined)
			{
				Log.Warning("[Discord] Lobby.Join failed, already in lobby");
				return;
			}
			if (string.IsNullOrEmpty(this.Secret))
			{
				if (_errorWithoutSecret)
				{
					Log.Error("[Discord] Lobby.Join failed, no secret set");
				}
				return;
			}
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			Dictionary<string, string> dictionary2 = new Dictionary<string, string>();
			GameServerInfo gameServerInfo = (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer ? SingletonMonoBehaviour<ConnectionManager>.Instance.LocalServerInfo : SingletonMonoBehaviour<ConnectionManager>.Instance.LastGameServerInfo);
			dictionary2["GameSession"] = gameServerInfo.GetValue(GameInfoString.UniqueId);
			dictionary2["Platform"] = ((PlatformManager.CrossplatformPlatform != null) ? PlatformManager.CrossplatformPlatform.PlatformIdentifier.ToStringCached<EPlatformIdentifier>() : PlatformManager.NativePlatform.PlatformIdentifier.ToStringCached<EPlatformIdentifier>());
			Log.Out(string.Concat(new string[]
			{
				"[Discord] CreateOrJoinLobby: ",
				this.Secret,
				", session=",
				dictionary2["GameSession"],
				", platform=",
				dictionary2["Platform"]
			}));
			this.ownerManager.client.CreateOrJoinLobbyWithMetadata(this.Secret, dictionary2, dictionary, delegate(ClientResult _result, ulong _lobbyId)
			{
				DiscordManager.logCallbackInfoWithClientResult("CreateOrJoinLobbyResult", string.Format("lobbyId={0}", _lobbyId), _result, false);
				if (_result.Type() != null)
				{
					_result.Dispose();
					return;
				}
				this.Id = _lobbyId;
				this.handle = this.ownerManager.client.GetLobbyHandle(_lobbyId);
				this.UpdateMembers();
				_result.Dispose();
			});
		}

		public void Leave(bool _manual = true)
		{
			if (this.IsInVoice)
			{
				this.VoiceCall.Leave(_manual);
			}
			if (this.IsJoined)
			{
				this.ownerManager.client.LeaveLobby(this.Id, delegate(ClientResult _result)
				{
					DiscordManager.logCallbackInfoWithClientResult("LeaveLobby", string.Format("type={0}", this.LobbyType), _result, true);
				});
			}
			this.Id = 0UL;
			LobbyHandle lobbyHandle = this.handle;
			if (lobbyHandle != null)
			{
				lobbyHandle.Dispose();
			}
			this.handle = null;
			this.UpdateMembers();
		}

		public void UpdateMembers()
		{
			this.lobbyMembers.Clear();
			if (this.IsJoined)
			{
				foreach (ulong num in this.handle.LobbyMemberIds())
				{
					this.ownerManager.GetUser(num);
					this.lobbyMembers.Add(num);
				}
			}
			DiscordManager.LobbyMembersChangedCallback lobbyMembersChanged = this.ownerManager.LobbyMembersChanged;
			if (lobbyMembersChanged == null)
			{
				return;
			}
			lobbyMembersChanged(this);
		}

		public bool HasMember(ulong _userId)
		{
			return this.lobbyMembers.Contains(_userId);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public const string LobbyMetadataKeyGameSession = "GameSession";

		[PublicizedFrom(EAccessModifier.Private)]
		public const string LobbyMetadataKeyPlatform = "Platform";

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly DiscordManager ownerManager;

		public readonly DiscordManager.ELobbyType LobbyType;

		[PublicizedFrom(EAccessModifier.Private)]
		public string secret;

		[PublicizedFrom(EAccessModifier.Private)]
		public ulong id;

		[PublicizedFrom(EAccessModifier.Private)]
		public LobbyHandle handle;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly DiscordManager.CallInfo voiceCall;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly HashSet<ulong> lobbyMembers = new HashSet<ulong>();
	}

	public class PresenceManager
	{
		public bool JoinableActivitySet
		{
			get
			{
				return !string.IsNullOrEmpty(this.joinSecretJson);
			}
		}

		public PresenceManager(DiscordManager _owner)
		{
			this.owner = _owner;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public ActivityAssets initActivity()
		{
			this.activity = new Activity();
			this.activity.SetName("7 Days To Die");
			this.activity.SetType(0);
			ActivityAssets activityAssets = new ActivityAssets();
			activityAssets.SetLargeImage("7dtd");
			return activityAssets;
		}

		public void RegisterDiscordCallbacks()
		{
			this.owner.client.SetActivityInviteCreatedCallback(new Client.ActivityInviteCallback(this.OnActivityInviteCreated));
			this.owner.client.SetActivityInviteUpdatedCallback(new Client.ActivityInviteCallback(this.OnActivityInviteUpdated));
			this.owner.client.SetActivityJoinCallback(new Client.ActivityJoinCallback(this.OnActivityJoin));
			this.SetRichPresenceState(new IRichPresence.PresenceStates?(IRichPresence.PresenceStates.Menu));
		}

		public void SetRichPresenceState(IRichPresence.PresenceStates? _state = null)
		{
			if (GameManager.IsDedicatedServer)
			{
				return;
			}
			IRichPresence.PresenceStates presenceStates = _state.GetValueOrDefault();
			if (_state == null)
			{
				presenceStates = this.currentRichPresenceState;
				_state = new IRichPresence.PresenceStates?(presenceStates);
			}
			if (this.currentRichPresenceState == _state.Value)
			{
				IRichPresence.PresenceStates? presenceStates2 = _state;
				presenceStates = IRichPresence.PresenceStates.InGame;
				if ((presenceStates2.GetValueOrDefault() == presenceStates) & (presenceStates2 != null))
				{
					this.refreshRichPresenceData();
				}
				this.sendCurrentRichPresence();
				return;
			}
			this.timeStartedCurrentActivity = (ulong)((_state.Value == IRichPresence.PresenceStates.InGame) ? Utils.CurrentUnixTime : 0U);
			this.joinSecretJson = null;
			this.currentRichPresenceState = _state.Value;
			this.refreshRichPresenceData();
			this.sendCurrentRichPresence();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void sendCurrentRichPresence()
		{
			if (GameManager.IsDedicatedServer)
			{
				return;
			}
			if (!this.owner.IsReady)
			{
				return;
			}
			if (this.activity == null)
			{
				return;
			}
			this.owner.client.UpdateRichPresence(this.activity, delegate(ClientResult _result)
			{
				DiscordManager.logCallbackInfoWithClientResult("UpdateRichPresence", null, _result, true);
				DiscordManager.FriendsListChangedCallback friendsListChanged = this.owner.FriendsListChanged;
				if (friendsListChanged != null)
				{
					friendsListChanged();
				}
				Activity activity = this.activity;
				if (activity != null)
				{
					activity.Dispose();
				}
				this.activity = null;
			});
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void refreshRichPresenceData()
		{
			if (GameManager.IsDedicatedServer)
			{
				return;
			}
			using (ActivityAssets activityAssets = this.initActivity())
			{
				this.setTimestamps();
				this.setDetailsAndState();
				this.setLargeImageAndTooltip(activityAssets);
				this.setSmallImageAndTooltip(activityAssets);
				this.setParty();
				this.setPlatforms();
				this.activity.SetAssets(activityAssets);
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void setDetailsAndState()
		{
			switch (this.currentRichPresenceState)
			{
			case IRichPresence.PresenceStates.Menu:
				this.activity.SetDetails(Localization.Get("discordPresenceDetailsInMenu", false, DiscordManager.PresenceManager.discordPresenceLocalizationLanguage));
				this.activity.SetState(null);
				return;
			case IRichPresence.PresenceStates.Loading:
				this.activity.SetDetails(Localization.Get("discordPresenceDetailsStartingGame", false, DiscordManager.PresenceManager.discordPresenceLocalizationLanguage));
				this.activity.SetState(null);
				return;
			case IRichPresence.PresenceStates.Connecting:
				this.activity.SetDetails(Localization.Get("discordPresenceDetailsConnectingToServer", false, DiscordManager.PresenceManager.discordPresenceLocalizationLanguage));
				this.activity.SetState(null);
				return;
			case IRichPresence.PresenceStates.InGame:
			{
				World world = GameManager.Instance.World;
				EntityPlayerLocal primaryPlayer = world.GetPrimaryPlayer();
				if (primaryPlayer == null)
				{
					this.activity.SetDetails(null);
					this.activity.SetState(null);
					return;
				}
				if (GameManager.Instance.IsEditMode())
				{
					this.activity.SetDetails(Localization.Get(PrefabEditModeManager.Instance.IsActive() ? "discordPresenceDetailsPoiEditor" : "discordPresenceDetailsWorldEditor", false, DiscordManager.PresenceManager.discordPresenceLocalizationLanguage));
					this.activity.SetState(null);
					return;
				}
				Party party = primaryPlayer.Party;
				int num = ((party != null) ? party.MemberList.Count : 1);
				this.activity.SetState(string.Format(Localization.Get((num > 1) ? "discordPresenceStateInParty" : "discordPresenceStateSolo", false, DiscordManager.PresenceManager.discordPresenceLocalizationLanguage), num));
				if (TwitchManager.HasInstance && TwitchManager.Current.InitState == TwitchManager.InitStates.Ready)
				{
					this.activity.SetDetails(Localization.Get("discordPresenceDetailsTwitchIntegration", false, DiscordManager.PresenceManager.discordPresenceLocalizationLanguage));
					return;
				}
				ulong worldTime = world.worldTime;
				int @int = GameStats.GetInt(EnumUtils.Parse<EnumGameStats>("BloodMoonDay", false));
				ValueTuple<int, int> valueTuple = GameUtils.CalcDuskDawnHours(GamePrefs.GetInt(EnumGamePrefs.DayLightLength));
				if (GameUtils.IsBloodMoonTime(worldTime, valueTuple, @int))
				{
					this.activity.SetDetails(Localization.Get("discordPresenceDetailsBloodMoon", false, DiscordManager.PresenceManager.discordPresenceLocalizationLanguage));
					return;
				}
				Quest activeQuest = primaryPlayer.QuestJournal.ActiveQuest;
				if (activeQuest != null)
				{
					this.activity.SetDetails(string.Format(Localization.Get("discordPresenceDetailsQuesting", false, DiscordManager.PresenceManager.discordPresenceLocalizationLanguage), activeQuest.QuestClass.Name));
					return;
				}
				if (DiscordManager.PresenceManager.<setDetailsAndState>g__PlayerAtHome|16_0(primaryPlayer))
				{
					this.activity.SetDetails(Localization.Get("discordPresenceDetailsAtHome", false, DiscordManager.PresenceManager.discordPresenceLocalizationLanguage));
					return;
				}
				this.activity.SetDetails(Localization.Get("discordPresenceDetailsExploring", false, DiscordManager.PresenceManager.discordPresenceLocalizationLanguage));
				return;
			}
			default:
				throw new ArgumentOutOfRangeException("currentRichPresenceState", this.currentRichPresenceState, null);
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void setTimestamps()
		{
			if (this.timeStartedCurrentActivity <= 0UL)
			{
				this.activity.SetTimestamps(null);
				return;
			}
			using (ActivityTimestamps activityTimestamps = new ActivityTimestamps())
			{
				activityTimestamps.SetStart(this.timeStartedCurrentActivity);
				this.activity.SetTimestamps(activityTimestamps);
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void setLargeImageAndTooltip(ActivityAssets _activityAssets)
		{
			DiscordManager.PresenceManager.<>c__DisplayClass20_0 CS$<>8__locals1;
			CS$<>8__locals1._activityAssets = _activityAssets;
			IRichPresence.PresenceStates presenceStates = this.currentRichPresenceState;
			if (presenceStates <= IRichPresence.PresenceStates.Connecting)
			{
				DiscordManager.PresenceManager.<setLargeImageAndTooltip>g__SetDefaultImage|20_0(ref CS$<>8__locals1);
				return;
			}
			if (presenceStates != IRichPresence.PresenceStates.InGame)
			{
				throw new ArgumentOutOfRangeException("currentRichPresenceState", this.currentRichPresenceState, null);
			}
			if (GameManager.Instance.IsEditMode())
			{
				DiscordManager.PresenceManager.<setLargeImageAndTooltip>g__SetDefaultImage|20_0(ref CS$<>8__locals1);
				return;
			}
			EntityPlayerLocal primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
			if (primaryPlayer == null)
			{
				DiscordManager.PresenceManager.<setLargeImageAndTooltip>g__SetDefaultImage|20_0(ref CS$<>8__locals1);
				return;
			}
			BiomeDefinition biomeStandingOn = primaryPlayer.biomeStandingOn;
			if (biomeStandingOn == null)
			{
				DiscordManager.PresenceManager.<setLargeImageAndTooltip>g__SetDefaultImage|20_0(ref CS$<>8__locals1);
				return;
			}
			string sBiomeName = biomeStandingOn.m_sBiomeName;
			ValueTuple<string, string> valueTuple;
			if (!this.biomeNameToAssetsMap.TryGetValue(sBiomeName, out valueTuple))
			{
				string text = (this.supportedBiomeImages.Contains(biomeStandingOn.m_BiomeType) ? ("biome" + biomeStandingOn.m_BiomeType.ToStringCached<BiomeDefinition.BiomeType>().ToLower()) : "7dtd");
				valueTuple = new ValueTuple<string, string>(Localization.Get("biome_" + sBiomeName, false, DiscordManager.PresenceManager.discordPresenceLocalizationLanguage), text);
				this.biomeNameToAssetsMap[sBiomeName] = valueTuple;
			}
			CS$<>8__locals1._activityAssets.SetLargeImage(valueTuple.Item2);
			CS$<>8__locals1._activityAssets.SetLargeText(valueTuple.Item1);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void setSmallImageAndTooltip(ActivityAssets _activityAssets)
		{
			DiscordManager.PresenceManager.<>c__DisplayClass22_0 CS$<>8__locals1;
			CS$<>8__locals1._activityAssets = _activityAssets;
			IRichPresence.PresenceStates presenceStates = this.currentRichPresenceState;
			if (presenceStates <= IRichPresence.PresenceStates.Connecting)
			{
				DiscordManager.PresenceManager.<setSmallImageAndTooltip>g__ClearImage|22_0(ref CS$<>8__locals1);
				return;
			}
			if (presenceStates != IRichPresence.PresenceStates.InGame)
			{
				throw new ArgumentOutOfRangeException("currentRichPresenceState", this.currentRichPresenceState, null);
			}
			if (GameManager.Instance.IsEditMode())
			{
				DiscordManager.PresenceManager.<setSmallImageAndTooltip>g__ClearImage|22_0(ref CS$<>8__locals1);
				return;
			}
			World world = GameManager.Instance.World;
			ulong worldTime = world.worldTime;
			int bloodMoonWarningHour = World.BloodMoonWarningHour;
			ValueTuple<int, int, int> valueTuple = GameUtils.WorldTimeToElements(worldTime);
			int item = valueTuple.Item1;
			int item2 = valueTuple.Item2;
			int @int = GameStats.GetInt(EnumUtils.Parse<EnumGameStats>("BloodMoonDay", false));
			ValueTuple<int, int> valueTuple2 = GameUtils.CalcDuskDawnHours(GamePrefs.GetInt(EnumGamePrefs.DayLightLength));
			bool flag = world.IsDaytime();
			bool flag2 = GameUtils.IsBloodMoonTime(worldTime, valueTuple2, @int);
			bool flag3 = !flag2 && bloodMoonWarningHour != -1 && GameStats.GetInt(EnumGameStats.BloodMoonDay) == item && bloodMoonWarningHour <= item2;
			string text = ValueDisplayFormatters.WorldTime(worldTime, DiscordManager.PresenceManager.dayTimeFormatString);
			if (flag2)
			{
				CS$<>8__locals1._activityAssets.SetSmallImage("statebloodmoon");
			}
			else if (flag3)
			{
				CS$<>8__locals1._activityAssets.SetSmallImage("statebloodmoonwarning");
			}
			else if (flag)
			{
				CS$<>8__locals1._activityAssets.SetSmallImage("stateday");
			}
			else
			{
				CS$<>8__locals1._activityAssets.SetSmallImage("statenight");
			}
			CS$<>8__locals1._activityAssets.SetSmallText(text);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void setParty()
		{
			if (this.currentRichPresenceState != IRichPresence.PresenceStates.InGame)
			{
				this.activity.SetParty(null);
				this.activity.SetSecrets(null);
				return;
			}
			GameServerInfo currentGameServerInfoServerOrClient = SingletonMonoBehaviour<ConnectionManager>.Instance.CurrentGameServerInfoServerOrClient;
			string value = currentGameServerInfoServerOrClient.GetValue(GameInfoString.UniqueId);
			if (string.IsNullOrEmpty(value))
			{
				this.activity.SetParty(null);
				this.activity.SetSecrets(null);
				return;
			}
			using (ActivityParty activityParty = new ActivityParty())
			{
				activityParty.SetId(value);
				activityParty.SetCurrentSize(GameManager.Instance.World.Players.Count);
				activityParty.SetMaxSize(currentGameServerInfoServerOrClient.GetValue(GameInfoInt.MaxPlayers));
				this.activity.SetParty(activityParty);
				if (string.IsNullOrEmpty(this.joinSecretJson))
				{
					this.joinSecretJson = JsonConvert.SerializeObject(new DiscordManager.PresenceManager.ActivitySecret(currentGameServerInfoServerOrClient));
				}
				using (ActivitySecrets activitySecrets = new ActivitySecrets())
				{
					activitySecrets.SetJoin(this.joinSecretJson);
					this.activity.SetSecrets(activitySecrets);
				}
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void setPlatforms()
		{
			if (this.currentRichPresenceState != IRichPresence.PresenceStates.InGame)
			{
				return;
			}
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.CurrentGameServerInfoServerOrClient.GetValue(GameInfoBool.AllowCrossplay))
			{
				this.activity.SetSupportedPlatforms(131);
				return;
			}
			Activity activity = this.activity;
			ActivityGamePlatforms activityGamePlatforms;
			switch (PlatformManager.NativePlatform.PlatformIdentifier)
			{
			case EPlatformIdentifier.None:
				throw new ArgumentException("None", "SetSupportedPlatforms");
			case EPlatformIdentifier.Local:
				activityGamePlatforms = 1;
				break;
			case EPlatformIdentifier.EOS:
				throw new ArgumentException("EOS", "SetSupportedPlatforms");
			case EPlatformIdentifier.Steam:
				activityGamePlatforms = 1;
				break;
			case EPlatformIdentifier.XBL:
				activityGamePlatforms = 2;
				break;
			case EPlatformIdentifier.PSN:
				activityGamePlatforms = 128;
				break;
			case EPlatformIdentifier.EGS:
				activityGamePlatforms = 1;
				break;
			case EPlatformIdentifier.LAN:
				throw new ArgumentException("LAN", "SetSupportedPlatforms");
			case EPlatformIdentifier.Count:
				throw new ArgumentException("Count", "SetSupportedPlatforms");
			default:
				throw new ArgumentOutOfRangeException();
			}
			activity.SetSupportedPlatforms(activityGamePlatforms);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void OnActivityInviteCreated(ActivityInvite _invite)
		{
			this.handleInvite(true, _invite);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void OnActivityInviteUpdated(ActivityInvite _invite)
		{
			this.handleInvite(false, _invite);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void handleInvite(bool _created, ActivityInvite _invite)
		{
			DiscordManager.logCallbackInfo(string.Format("{0}: sender={1}/type={2}/party={3}/message={4}/valid={5}", new object[]
			{
				_created ? "OnActivityInviteCreated" : "OnActivityInviteUpdated",
				_invite.SenderId(),
				_invite.Type().ToStringCached<ActivityActionTypes>(),
				_invite.PartyId(),
				_invite.MessageId(),
				_invite.IsValid()
			}), 3);
			DiscordManager.DiscordUser user = this.owner.GetUser(_invite.SenderId());
			ActivityActionTypes activityActionTypes = _invite.Type();
			bool flag = _invite.IsValid();
			if (activityActionTypes != 1)
			{
				if (activityActionTypes == 5)
				{
					user.PendingIncomingJoinRequest = flag;
					_invite.Dispose();
					return;
				}
				return;
			}
			else
			{
				if (!user.PendingOutgoingJoinRequest)
				{
					user.SetIncomingInviteActivity(flag ? _invite : null);
					return;
				}
				if (flag)
				{
					user.AcceptInvite(_invite);
					return;
				}
				_invite.Dispose();
				return;
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void OnActivityJoin(string _joinSecret)
		{
			DiscordManager.logCallbackInfo("OnActivityJoin", 3);
			DiscordManager.PresenceManager.ActivitySecret activitySecret;
			try
			{
				activitySecret = JsonConvert.DeserializeObject<DiscordManager.PresenceManager.ActivitySecret>(_joinSecret);
			}
			catch (JsonException ex)
			{
				Log.Error("[Discord] Failed reading invite secret:");
				Log.Exception(ex);
				return;
			}
			DiscordManager.ActivityJoiningCallback activityJoining = this.owner.ActivityJoining;
			if (activityJoining != null)
			{
				activityJoining();
			}
			DiscordInviteListener.ListenerInstance.SetPendingInvite(activitySecret.SessionID, activitySecret.Password);
		}

		[CompilerGenerated]
		[PublicizedFrom(EAccessModifier.Internal)]
		public static bool <setDetailsAndState>g__PlayerAtHome|16_0(EntityPlayerLocal _player)
		{
			SpawnPosition spawnPoint = _player.GetSpawnPoint();
			return (!spawnPoint.IsUndef() && (spawnPoint.position - _player.position).sqrMagnitude <= 2500f) || GameManager.Instance.World.GetLandClaimOwnerInParty(_player, _player.persistentPlayerData);
		}

		[CompilerGenerated]
		[PublicizedFrom(EAccessModifier.Internal)]
		public static void <setLargeImageAndTooltip>g__SetDefaultImage|20_0(ref DiscordManager.PresenceManager.<>c__DisplayClass20_0 A_0)
		{
			A_0._activityAssets.SetLargeImage("7dtd");
			A_0._activityAssets.SetLargeText(null);
		}

		[CompilerGenerated]
		[PublicizedFrom(EAccessModifier.Internal)]
		public static void <setSmallImageAndTooltip>g__ClearImage|22_0(ref DiscordManager.PresenceManager.<>c__DisplayClass22_0 A_0)
		{
			A_0._activityAssets.SetSmallImage(null);
			A_0._activityAssets.SetSmallText(null);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly string discordPresenceLocalizationLanguage = "english";

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly DiscordManager owner;

		[PublicizedFrom(EAccessModifier.Private)]
		public IRichPresence.PresenceStates currentRichPresenceState = IRichPresence.PresenceStates.InGame;

		[PublicizedFrom(EAccessModifier.Private)]
		public ulong timeStartedCurrentActivity = (ulong)Utils.CurrentUnixTime;

		[PublicizedFrom(EAccessModifier.Private)]
		public string joinSecretJson;

		[PublicizedFrom(EAccessModifier.Private)]
		public Activity activity;

		[PublicizedFrom(EAccessModifier.Private)]
		public const string DefaultLargeImageName = "7dtd";

		[TupleElementNames(new string[] { "label", "image" })]
		[PublicizedFrom(EAccessModifier.Private)]
		public readonly Dictionary<string, ValueTuple<string, string>> biomeNameToAssetsMap = new Dictionary<string, ValueTuple<string, string>>();

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly HashSet<BiomeDefinition.BiomeType> supportedBiomeImages = new HashSet<BiomeDefinition.BiomeType>
		{
			BiomeDefinition.BiomeType.burnt_forest,
			BiomeDefinition.BiomeType.Desert,
			BiomeDefinition.BiomeType.PineForest,
			BiomeDefinition.BiomeType.Snow,
			BiomeDefinition.BiomeType.Wasteland
		};

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly string dayTimeFormatString = Localization.Get("xuiDay", false, DiscordManager.PresenceManager.discordPresenceLocalizationLanguage) + " {0}, {1:00}:{2:00}";

		[JsonObject(2)]
		[PublicizedFrom(EAccessModifier.Private)]
		public class ActivitySecret
		{
			public ActivitySecret(GameServerInfo _gsi)
			{
				this.SessionID = _gsi.GetValue(GameInfoString.UniqueId);
				this.ServerIP = _gsi.GetValue(GameInfoString.IP);
				this.ServerPort = _gsi.GetValue(GameInfoInt.Port);
				if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
				{
					this.Password = GamePrefs.GetString(EnumGamePrefs.ServerPassword);
					return;
				}
				this.Password = ServerInfoCache.Instance.GetPassword(_gsi) ?? "";
			}

			[JsonProperty("ID")]
			public readonly string SessionID;

			[JsonProperty("IP")]
			public string ServerIP;

			[JsonProperty("Port")]
			public int ServerPort;

			[JsonProperty("PW")]
			public string Password;
		}
	}
}
