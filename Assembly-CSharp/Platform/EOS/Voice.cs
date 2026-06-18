using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Epic.OnlineServices;
using Epic.OnlineServices.Lobby;
using Epic.OnlineServices.Platform;
using Epic.OnlineServices.RTC;
using Epic.OnlineServices.RTCAudio;
using UnityEngine;

namespace Platform.EOS
{
	public class Voice : IPartyVoice
	{
		public UserIdentifierEos localUserIdentifier
		{
			[PublicizedFrom(EAccessModifier.Private)]
			get
			{
				IPlatform platform = this.owner;
				object obj;
				if (platform == null)
				{
					obj = null;
				}
				else
				{
					IUserClient user = platform.User;
					obj = ((user != null) ? user.PlatformUserId : null);
				}
				return (UserIdentifierEos)obj;
			}
		}

		public ProductUserId localProductUserId
		{
			[PublicizedFrom(EAccessModifier.Private)]
			get
			{
				UserIdentifierEos localUserIdentifier = this.localUserIdentifier;
				if (localUserIdentifier == null)
				{
					return null;
				}
				return localUserIdentifier.ProductUserId;
			}
		}

		public event Action Initialized
		{
			add
			{
				object obj = this.initializedDelegateLock;
				lock (obj)
				{
					this.initializedDelegates = (Action)Delegate.Combine(this.initializedDelegates, value);
					if (this.Status == EPartyVoiceStatus.Ok)
					{
						value();
					}
				}
			}
			remove
			{
				object obj = this.initializedDelegateLock;
				lock (obj)
				{
					this.initializedDelegates = (Action)Delegate.Remove(this.initializedDelegates, value);
				}
			}
		}

		public EPartyVoiceStatus Status
		{
			get; [PublicizedFrom(EAccessModifier.Private)]
			set;
		} = EPartyVoiceStatus.Uninitialized;

		public bool InLobby
		{
			get
			{
				return this.lobbyId != null && this.roomEntered;
			}
		}

		public bool InLobbyOrProgress
		{
			get
			{
				return this.InLobby || this.createInProgress || this.joinInProgress;
			}
		}

		public void Init(IPlatform _owner)
		{
			this.owner = _owner;
			this.api = (Api)this.owner.Api;
			this.api.ClientApiInitialized += this.OnClientApiInitialized;
		}

		public void Destroy()
		{
			if (this.Status == EPartyVoiceStatus.Ok)
			{
				this.OnPartyVoiceUninitialize();
			}
			this.Status = EPartyVoiceStatus.Uninitialized;
			this.lobbyInterface = null;
			this.audioInterface = null;
			this.rtcInterface = null;
			this.api.ClientApiInitialized -= this.OnClientApiInitialized;
			this.api = null;
			this.owner = null;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void OnClientApiInitialized()
		{
			PlatformInterface platformInterface = this.api.PlatformInterface;
			object lockObject = AntiCheatCommon.LockObject;
			lock (lockObject)
			{
				this.rtcInterface = ((platformInterface != null) ? platformInterface.GetRTCInterface() : null);
				RTCInterface rtcinterface = this.rtcInterface;
				this.audioInterface = ((rtcinterface != null) ? rtcinterface.GetAudioInterface() : null);
				this.lobbyInterface = ((platformInterface != null) ? platformInterface.GetLobbyInterface() : null);
			}
			lockObject = this.initializedDelegateLock;
			lock (lockObject)
			{
				if (this.rtcInterface != null && this.audioInterface != null && this.lobbyInterface != null)
				{
					this.Status = EPartyVoiceStatus.Ok;
					this.OnPartyVoiceInitialized();
					Log.Out("[EOS-Voice] Successfully initialized.");
					Action action = this.initializedDelegates;
					if (action != null)
					{
						action();
					}
				}
				else
				{
					this.Status = EPartyVoiceStatus.PermanentError;
					Log.Warning("[EOS-Voice] Failed to initialize.");
				}
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void OnPartyVoiceInitialized()
		{
			this.AddNotifications();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void OnPartyVoiceUninitialize()
		{
			this.RemoveNotifications();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void AddNotifications()
		{
			if (this.Status != EPartyVoiceStatus.Ok)
			{
				Log.Error("[EOS-Voice] Can not add notifications because voice is currently not ready.");
				return;
			}
			this.AddAudioDevicesNotifications();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void RemoveNotifications()
		{
			if (this.Status != EPartyVoiceStatus.Ok)
			{
				Log.Error("[EOS-Voice] Can not remove notifications because voice is currently not ready.");
				return;
			}
			this.RemoveAudioDevicesNotifications();
		}

		public void CreateLobby(Action<string> _lobbyCreatedCallback)
		{
			if (this.Status != EPartyVoiceStatus.Ok)
			{
				Log.Error("[EOS-Voice] Can not create lobby because voice is currently not valid.");
				return;
			}
			if (this.lobbyId != null)
			{
				Log.Error("[EOS-Voice] Can not create lobby while already in another");
				Log.Error(StackTraceUtility.ExtractStackTrace());
				return;
			}
			this.createInProgress = true;
			Log.Out("[EOS-Voice] Creating lobby");
			EosHelpers.AssertMainThread("Voice.Create");
			CreateLobbyOptions createLobbyOptions = default(CreateLobbyOptions);
			createLobbyOptions.LocalUserId = this.localProductUserId;
			createLobbyOptions.MaxLobbyMembers = 8U;
			createLobbyOptions.PermissionLevel = 1;
			createLobbyOptions.PresenceEnabled = false;
			createLobbyOptions.AllowInvites = false;
			createLobbyOptions.EnableRTCRoom = true;
			createLobbyOptions.BucketId = "PartyVoice";
			LocalRTCOptions localRTCOptions = default(LocalRTCOptions);
			localRTCOptions.LocalAudioDeviceInputStartsMuted = true;
			createLobbyOptions.LocalRTCOptions = new LocalRTCOptions?(localRTCOptions);
			CreateLobbyOptions createLobbyOptions2 = createLobbyOptions;
			object lockObject = AntiCheatCommon.LockObject;
			lock (lockObject)
			{
				this.lobbyInterface.CreateLobby(ref createLobbyOptions2, _lobbyCreatedCallback, delegate(ref CreateLobbyCallbackInfo _callbackData)
				{
					if (_callbackData.ResultCode != null)
					{
						Log.Error("[EOS-Voice] Create lobby failed: " + _callbackData.ResultCode.ToStringCached<Result>());
						this.createInProgress = false;
						Action<string> action = (Action<string>)_callbackData.ClientData;
						if (action == null)
						{
							return;
						}
						action(null);
						return;
					}
					else
					{
						this.lobbyEntered(_callbackData.LobbyId);
						Action<string> action2 = (Action<string>)_callbackData.ClientData;
						if (action2 == null)
						{
							return;
						}
						action2(this.lobbyId);
						return;
					}
				});
			}
		}

		public void JoinLobby(string _lobbyId)
		{
			if (this.Status != EPartyVoiceStatus.Ok)
			{
				Log.Error("[EOS-Voice] Can not join lobby because voice is currently not ready.");
				return;
			}
			if (string.IsNullOrEmpty(_lobbyId))
			{
				Log.Error("[EOS-Voice] Can not join lobby, missing id");
				return;
			}
			if (this.lobbyId != null)
			{
				if (this.lobbyId != _lobbyId)
				{
					Log.Error("[EOS-Voice] Can not join lobby while already in another");
				}
				return;
			}
			Log.Out("[EOS-Voice] Joining lobby");
			this.joinInProgress = true;
			ThreadManager.StartCoroutine(this.tryJoinLobbyCo(_lobbyId));
		}

		public void LeaveLobby()
		{
			if (this.Status != EPartyVoiceStatus.Ok)
			{
				Log.Error("[EOS-Voice] Can not leave lobby because voice is currently not ready.");
				return;
			}
			if (this.lobbyId == null)
			{
				return;
			}
			if (this.leaveInProgress)
			{
				return;
			}
			this.leaveInProgress = true;
			Log.Out("[EOS-Voice] Leaving lobby");
			EosHelpers.AssertMainThread("Voice.Leave");
			LeaveLobbyOptions leaveLobbyOptions = default(LeaveLobbyOptions);
			leaveLobbyOptions.LocalUserId = this.localProductUserId;
			leaveLobbyOptions.LobbyId = this.lobbyId;
			LeaveLobbyOptions leaveLobbyOptions2 = leaveLobbyOptions;
			object lockObject = AntiCheatCommon.LockObject;
			lock (lockObject)
			{
				this.lobbyInterface.LeaveLobby(ref leaveLobbyOptions2, null, delegate(ref LeaveLobbyCallbackInfo _callbackData)
				{
					this.lobbyLeft();
					this.leaveInProgress = false;
					if (_callbackData.ResultCode != null)
					{
						Log.Error("[EOS-Voice] Leave lobby failed: " + _callbackData.ResultCode.ToStringCached<Result>());
					}
				});
			}
		}

		public void PromoteLeader(PlatformUserIdentifierAbs _newLeaderIdentifier)
		{
			if (this.Status != EPartyVoiceStatus.Ok)
			{
				Log.Error("[EOS-Voice] Can not promote leader because voice is currently not ready.");
				return;
			}
			if (!this.IsLobbyOwner())
			{
				return;
			}
			UserIdentifierEos userIdentifierEos = _newLeaderIdentifier as UserIdentifierEos;
			if (userIdentifierEos == null)
			{
				Log.Error(string.Format("[EOS-Voice] New leader user identifier is not an EOS identifier: {0}", _newLeaderIdentifier));
				return;
			}
			Log.Out("[EOS-Voice] Promoting lobby owner");
			EosHelpers.AssertMainThread("Voice.Prom");
			PromoteMemberOptions promoteMemberOptions = default(PromoteMemberOptions);
			promoteMemberOptions.LocalUserId = this.localProductUserId;
			promoteMemberOptions.LobbyId = this.lobbyId;
			promoteMemberOptions.TargetUserId = userIdentifierEos.ProductUserId;
			PromoteMemberOptions promoteMemberOptions2 = promoteMemberOptions;
			object lockObject = AntiCheatCommon.LockObject;
			lock (lockObject)
			{
				this.lobbyInterface.PromoteMember(ref promoteMemberOptions2, null, delegate(ref PromoteMemberCallbackInfo _callbackData)
				{
					if (_callbackData.ResultCode != null)
					{
						Log.Error("[EOS-Voice] Promoting leader failed: " + _callbackData.ResultCode.ToStringCached<Result>());
					}
				});
			}
		}

		public bool IsLobbyOwner()
		{
			if (this.Status != EPartyVoiceStatus.Ok)
			{
				Log.Error("[EOS-Voice] Can not check if lobby owner because voice is currently not ready.");
				return false;
			}
			if (this.lobbyId == null)
			{
				return false;
			}
			EosHelpers.AssertMainThread("Voice.Own");
			CopyLobbyDetailsHandleOptions copyLobbyDetailsHandleOptions = default(CopyLobbyDetailsHandleOptions);
			copyLobbyDetailsHandleOptions.LocalUserId = this.localProductUserId;
			copyLobbyDetailsHandleOptions.LobbyId = this.lobbyId;
			CopyLobbyDetailsHandleOptions copyLobbyDetailsHandleOptions2 = copyLobbyDetailsHandleOptions;
			object obj = AntiCheatCommon.LockObject;
			LobbyDetails lobbyDetails;
			Result result;
			lock (obj)
			{
				result = this.lobbyInterface.CopyLobbyDetailsHandle(ref copyLobbyDetailsHandleOptions2, ref lobbyDetails);
			}
			if (result != null)
			{
				Log.Error("[EOS-Voice] Getting local lobby details failed: " + result.ToStringCached<Result>());
				return false;
			}
			LobbyDetailsGetLobbyOwnerOptions lobbyDetailsGetLobbyOwnerOptions = default(LobbyDetailsGetLobbyOwnerOptions);
			obj = AntiCheatCommon.LockObject;
			ProductUserId lobbyOwner;
			lock (obj)
			{
				lobbyOwner = lobbyDetails.GetLobbyOwner(ref lobbyDetailsGetLobbyOwnerOptions);
			}
			bool flag2 = lobbyOwner == this.localProductUserId;
			lobbyDetails.Release();
			return flag2;
		}

		public event Action<IPartyVoice.EVoiceChannelAction> OnLocalPlayerStateChanged;

		public event Action<PlatformUserIdentifierAbs, IPartyVoice.EVoiceChannelAction> OnRemotePlayerStateChanged;

		public event Action<PlatformUserIdentifierAbs, IPartyVoice.EVoiceMemberState> OnRemotePlayerVoiceStateChanged;

		[PublicizedFrom(EAccessModifier.Private)]
		public void AddAudioDevicesNotifications()
		{
			if (this.audioDevicesChangedNotificationId == 0UL)
			{
				this.RefreshAudioDevices();
				AddNotifyAudioDevicesChangedOptions addNotifyAudioDevicesChangedOptions = default(AddNotifyAudioDevicesChangedOptions);
				object lockObject = AntiCheatCommon.LockObject;
				lock (lockObject)
				{
					this.audioDevicesChangedNotificationId = this.audioInterface.AddNotifyAudioDevicesChanged(ref addNotifyAudioDevicesChangedOptions, null, new OnAudioDevicesChangedCallback(this.<AddAudioDevicesNotifications>g__OnAudioDevicesChanged|60_0));
				}
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void RemoveAudioDevicesNotifications()
		{
			if (this.audioDevicesChangedNotificationId != 0UL)
			{
				object lockObject = AntiCheatCommon.LockObject;
				lock (lockObject)
				{
					RTCAudioInterface rtcaudioInterface = this.audioInterface;
					if (rtcaudioInterface != null)
					{
						rtcaudioInterface.RemoveNotifyAudioDevicesChanged(this.audioDevicesChangedNotificationId);
					}
				}
				this.audioDevicesChangedNotificationId = 0UL;
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void RefreshAudioDevices()
		{
			if (this.Status != EPartyVoiceStatus.Ok)
			{
				Log.Error("[EOS-Voice] Can not refresh audio devices because voice is currently not ready.");
				return;
			}
			QueryInputDevicesInformationOptions queryInputDevicesInformationOptions = default(QueryInputDevicesInformationOptions);
			object obj = AntiCheatCommon.LockObject;
			lock (obj)
			{
				this.audioInterface.QueryInputDevicesInformation(ref queryInputDevicesInformationOptions, null, new OnQueryInputDevicesInformationCallback(this.OnQueryInputDevicesInformation));
			}
			QueryOutputDevicesInformationOptions queryOutputDevicesInformationOptions = default(QueryOutputDevicesInformationOptions);
			obj = AntiCheatCommon.LockObject;
			lock (obj)
			{
				this.audioInterface.QueryOutputDevicesInformation(ref queryOutputDevicesInformationOptions, null, new OnQueryOutputDevicesInformationCallback(this.OnQueryOutputDevicesInformation));
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void OnQueryInputDevicesInformation(ref OnQueryInputDevicesInformationCallbackInfo data)
		{
			if (data.ResultCode != null)
			{
				Log.Error("[EOS-Voice] Query Input Devices Information Failed: " + data.ResultCode.ToStringCached<Result>());
				return;
			}
			if (this.Status != EPartyVoiceStatus.Ok)
			{
				Log.Warning(string.Format("[EOS-Voice] can not query input devices information. Voice status: {0}", this.Status));
				return;
			}
			GetInputDevicesCountOptions getInputDevicesCountOptions = default(GetInputDevicesCountOptions);
			object obj = AntiCheatCommon.LockObject;
			uint inputDevicesCount;
			lock (obj)
			{
				inputDevicesCount = this.audioInterface.GetInputDevicesCount(ref getInputDevicesCountOptions);
			}
			IList<IPartyVoice.VoiceAudioDevice> list = new List<IPartyVoice.VoiceAudioDevice>();
			for (uint num = 0U; num < inputDevicesCount; num += 1U)
			{
				CopyInputDeviceInformationByIndexOptions copyInputDeviceInformationByIndexOptions = default(CopyInputDeviceInformationByIndexOptions);
				copyInputDeviceInformationByIndexOptions.DeviceIndex = num;
				CopyInputDeviceInformationByIndexOptions copyInputDeviceInformationByIndexOptions2 = copyInputDeviceInformationByIndexOptions;
				obj = AntiCheatCommon.LockObject;
				InputDeviceInformation? inputDeviceInformation;
				Result result;
				lock (obj)
				{
					result = this.audioInterface.CopyInputDeviceInformationByIndex(ref copyInputDeviceInformationByIndexOptions2, ref inputDeviceInformation);
				}
				try
				{
					if (result != null || inputDeviceInformation == null)
					{
						Log.Warning(string.Format("[EOS-Voice] Could not query input device {0}: {1}", num, result.ToStringCached<Result>()));
					}
					else
					{
						list.Add(new Voice.EosAudioDevice(inputDeviceInformation.Value));
					}
				}
				finally
				{
					bool flag2 = inputDeviceInformation != null;
				}
			}
			this.inputDevices = list;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void OnQueryOutputDevicesInformation(ref OnQueryOutputDevicesInformationCallbackInfo data)
		{
			if (data.ResultCode != null)
			{
				Log.Error("[EOS-Voice] Query Output Devices Information Failed: " + data.ResultCode.ToStringCached<Result>());
				return;
			}
			if (this.Status != EPartyVoiceStatus.Ok)
			{
				Log.Warning(string.Format("[EOS-Voice] can not query output devices information. Voice status: {0}", this.Status));
				return;
			}
			IList<IPartyVoice.VoiceAudioDevice> list = new List<IPartyVoice.VoiceAudioDevice>();
			GetOutputDevicesCountOptions getOutputDevicesCountOptions = default(GetOutputDevicesCountOptions);
			object obj = AntiCheatCommon.LockObject;
			uint outputDevicesCount;
			lock (obj)
			{
				outputDevicesCount = this.audioInterface.GetOutputDevicesCount(ref getOutputDevicesCountOptions);
			}
			for (uint num = 0U; num < outputDevicesCount; num += 1U)
			{
				CopyOutputDeviceInformationByIndexOptions copyOutputDeviceInformationByIndexOptions = default(CopyOutputDeviceInformationByIndexOptions);
				copyOutputDeviceInformationByIndexOptions.DeviceIndex = num;
				CopyOutputDeviceInformationByIndexOptions copyOutputDeviceInformationByIndexOptions2 = copyOutputDeviceInformationByIndexOptions;
				obj = AntiCheatCommon.LockObject;
				OutputDeviceInformation? outputDeviceInformation;
				Result result;
				lock (obj)
				{
					result = this.audioInterface.CopyOutputDeviceInformationByIndex(ref copyOutputDeviceInformationByIndexOptions2, ref outputDeviceInformation);
				}
				try
				{
					if (result != null || outputDeviceInformation == null)
					{
						Log.Warning(string.Format("[EOS-Voice] Could not query output device {0}: {1}", num, result.ToStringCached<Result>()));
					}
					else
					{
						list.Add(new Voice.EosAudioDevice(outputDeviceInformation.Value));
					}
				}
				finally
				{
					bool flag2 = outputDeviceInformation != null;
				}
			}
			this.outputDevices = list;
		}

		[return: TupleElementNames(new string[] { "inputDevices", "outputDevices" })]
		public ValueTuple<IList<IPartyVoice.VoiceAudioDevice>, IList<IPartyVoice.VoiceAudioDevice>> GetDevicesList()
		{
			if (this.Status != EPartyVoiceStatus.Ok)
			{
				Log.Error("[EOS-Voice] Can not get voice devices because voice is currently not ready.");
				return new ValueTuple<IList<IPartyVoice.VoiceAudioDevice>, IList<IPartyVoice.VoiceAudioDevice>>(Array.Empty<IPartyVoice.VoiceAudioDevice>(), Array.Empty<IPartyVoice.VoiceAudioDevice>());
			}
			EosHelpers.AssertMainThread("Voice.GetDev");
			return new ValueTuple<IList<IPartyVoice.VoiceAudioDevice>, IList<IPartyVoice.VoiceAudioDevice>>(this.inputDevices, this.outputDevices);
		}

		public void SetInputDevice(string _device)
		{
			Voice.<>c__DisplayClass66_0 CS$<>8__locals1 = new Voice.<>c__DisplayClass66_0();
			CS$<>8__locals1._device = _device;
			CS$<>8__locals1.<>4__this = this;
			if (this.Status != EPartyVoiceStatus.Ok)
			{
				Log.Error("[EOS-Voice] Can not set input device because voice is currently not ready.");
				return;
			}
			EosHelpers.AssertMainThread("Voice.SetIn");
			SetInputDeviceSettingsOptions setInputDeviceSettingsOptions = default(SetInputDeviceSettingsOptions);
			setInputDeviceSettingsOptions.LocalUserId = this.localProductUserId;
			setInputDeviceSettingsOptions.RealDeviceId = CS$<>8__locals1._device;
			setInputDeviceSettingsOptions.PlatformAEC = true;
			SetInputDeviceSettingsOptions setInputDeviceSettingsOptions2 = setInputDeviceSettingsOptions;
			object lockObject = AntiCheatCommon.LockObject;
			lock (lockObject)
			{
				this.audioInterface.SetInputDeviceSettings(ref setInputDeviceSettingsOptions2, null, new OnSetInputDeviceSettingsCallback(CS$<>8__locals1.<SetInputDevice>g__OnSetInputDeviceSettings|0));
			}
		}

		public void SetOutputDevice(string _device)
		{
			Voice.<>c__DisplayClass67_0 CS$<>8__locals1 = new Voice.<>c__DisplayClass67_0();
			CS$<>8__locals1._device = _device;
			CS$<>8__locals1.<>4__this = this;
			if (this.Status != EPartyVoiceStatus.Ok)
			{
				Log.Error("[EOS-Voice] Can not set output device because voice is currently not ready.");
				return;
			}
			EosHelpers.AssertMainThread("Voice.SetOut");
			SetOutputDeviceSettingsOptions setOutputDeviceSettingsOptions = default(SetOutputDeviceSettingsOptions);
			setOutputDeviceSettingsOptions.LocalUserId = this.localProductUserId;
			setOutputDeviceSettingsOptions.RealDeviceId = CS$<>8__locals1._device;
			SetOutputDeviceSettingsOptions setOutputDeviceSettingsOptions2 = setOutputDeviceSettingsOptions;
			object lockObject = AntiCheatCommon.LockObject;
			lock (lockObject)
			{
				this.audioInterface.SetOutputDeviceSettings(ref setOutputDeviceSettingsOptions2, null, new OnSetOutputDeviceSettingsCallback(CS$<>8__locals1.<SetOutputDevice>g__OnSetOutputDeviceSettings|0));
			}
		}

		public bool MuteSelf
		{
			get
			{
				return this.muteSelf;
			}
			set
			{
				if (this.Status != EPartyVoiceStatus.Ok)
				{
					Log.Error("[EOS-Voice] Can not mute self because voice is currently not ready.");
					return;
				}
				if (value == this.muteSelf)
				{
					return;
				}
				this.muteSelf = value;
				if (this.roomName == null)
				{
					return;
				}
				EosHelpers.AssertMainThread("Voice.Mute");
				UpdateSendingOptions updateSendingOptions = default(UpdateSendingOptions);
				updateSendingOptions.LocalUserId = this.localProductUserId;
				updateSendingOptions.RoomName = this.roomName;
				updateSendingOptions.AudioStatus = (value ? 2 : 1);
				UpdateSendingOptions updateSendingOptions2 = updateSendingOptions;
				object lockObject = AntiCheatCommon.LockObject;
				lock (lockObject)
				{
					this.audioInterface.UpdateSending(ref updateSendingOptions2, null, delegate(ref UpdateSendingCallbackInfo _callbackData)
					{
						if (_callbackData.ResultCode != null)
						{
							Log.Error("[EOS-Voice] Toggling voice sending state failed: " + _callbackData.ResultCode.ToStringCached<Result>());
						}
					});
				}
			}
		}

		public bool MuteOthers
		{
			get
			{
				return this.muteOthers;
			}
			set
			{
				if (this.Status != EPartyVoiceStatus.Ok)
				{
					Log.Error("[EOS-Voice] Can not mute others because voice is currently not ready.");
					return;
				}
				if (value == this.muteOthers)
				{
					return;
				}
				this.muteOthers = value;
				if (this.roomName == null)
				{
					return;
				}
				EosHelpers.AssertMainThread("Voice.MuteOth");
				UpdateReceivingOptions updateReceivingOptions = default(UpdateReceivingOptions);
				updateReceivingOptions.LocalUserId = this.localProductUserId;
				updateReceivingOptions.RoomName = this.roomName;
				updateReceivingOptions.AudioEnabled = !value;
				UpdateReceivingOptions updateReceivingOptions2 = updateReceivingOptions;
				object lockObject = AntiCheatCommon.LockObject;
				lock (lockObject)
				{
					this.audioInterface.UpdateReceiving(ref updateReceivingOptions2, null, delegate(ref UpdateReceivingCallbackInfo _callbackData)
					{
						if (_callbackData.ResultCode != null)
						{
							Log.Error("[EOS-Voice] Toggling voice receiving state failed: " + _callbackData.ResultCode.ToStringCached<Result>());
							return;
						}
						Log.Out(string.Format("[EOS-Voice] Mute all state changed to: {0}", value));
					});
				}
			}
		}

		public float OutputVolume
		{
			get
			{
				return this.outputVolume;
			}
			set
			{
				if (this.Status != EPartyVoiceStatus.Ok)
				{
					Log.Error("[EOS-Voice] Can not set output volume because voice is currently not ready.");
					return;
				}
				if ((double)value > (double)this.outputVolume - 0.01 && (double)value < (double)this.outputVolume + 0.01)
				{
					return;
				}
				this.outputVolume = value;
				this.SetRoomReceivingVolume(this.outputVolume);
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void SetRoomReceivingVolume(float platformVolume)
		{
			if (this.Status != EPartyVoiceStatus.Ok)
			{
				Log.Error("[EOS-Voice] Can not set room receiving volume because voice is currently not ready.");
				return;
			}
			if (this.roomName == null)
			{
				return;
			}
			EosHelpers.AssertMainThread("Voice.Vol");
			UpdateReceivingVolumeOptions updateReceivingVolumeOptions = default(UpdateReceivingVolumeOptions);
			updateReceivingVolumeOptions.LocalUserId = this.localProductUserId;
			updateReceivingVolumeOptions.RoomName = this.roomName;
			updateReceivingVolumeOptions.Volume = platformVolume * 50f;
			UpdateReceivingVolumeOptions updateReceivingVolumeOptions2 = updateReceivingVolumeOptions;
			object lockObject = AntiCheatCommon.LockObject;
			lock (lockObject)
			{
				this.audioInterface.UpdateReceivingVolume(ref updateReceivingVolumeOptions2, null, new OnUpdateReceivingVolumeCallback(Voice.<SetRoomReceivingVolume>g__OnUpdateReceivingVolume|80_0));
			}
		}

		public void BlockUser(PlatformUserIdentifierAbs _userIdentifier, bool _block)
		{
			if (this.Status != EPartyVoiceStatus.Ok)
			{
				Log.Error("[EOS-Voice] Can not block user because voice is currently not ready.");
				return;
			}
			if (this.roomName == null)
			{
				return;
			}
			if (this.muteOthers)
			{
				return;
			}
			UserIdentifierEos userIdentifierEos = _userIdentifier as UserIdentifierEos;
			if (userIdentifierEos == null)
			{
				Log.Error(string.Format("[EOS-Voice] Block user identifier is not an EOS identifier: {0}", _userIdentifier));
				return;
			}
			Log.Out(string.Format("[EOS-Voice] Blocking user: {0} = {1}", userIdentifierEos, _block));
			EosHelpers.AssertMainThread("Voice.Block");
			BlockParticipantOptions blockParticipantOptions = default(BlockParticipantOptions);
			blockParticipantOptions.LocalUserId = this.localProductUserId;
			blockParticipantOptions.RoomName = this.roomName;
			blockParticipantOptions.ParticipantId = userIdentifierEos.ProductUserId;
			blockParticipantOptions.Blocked = _block;
			BlockParticipantOptions blockParticipantOptions2 = blockParticipantOptions;
			object lockObject = AntiCheatCommon.LockObject;
			lock (lockObject)
			{
				this.rtcInterface.BlockParticipant(ref blockParticipantOptions2, null, delegate(ref BlockParticipantCallbackInfo _callbackData)
				{
					if (_callbackData.ResultCode != null)
					{
						Log.Error("[EOS-Voice] Blocking user failed: " + _callbackData.ResultCode.ToStringCached<Result>());
						return;
					}
					if (_block)
					{
						this.blockedUsers.Add(_userIdentifier);
						Action<PlatformUserIdentifierAbs, IPartyVoice.EVoiceMemberState> onRemotePlayerVoiceStateChanged = this.OnRemotePlayerVoiceStateChanged;
						if (onRemotePlayerVoiceStateChanged == null)
						{
							return;
						}
						onRemotePlayerVoiceStateChanged(_userIdentifier, IPartyVoice.EVoiceMemberState.Muted);
						return;
					}
					else
					{
						this.blockedUsers.Remove(_userIdentifier);
						Action<PlatformUserIdentifierAbs, IPartyVoice.EVoiceMemberState> onRemotePlayerVoiceStateChanged2 = this.OnRemotePlayerVoiceStateChanged;
						if (onRemotePlayerVoiceStateChanged2 == null)
						{
							return;
						}
						onRemotePlayerVoiceStateChanged2(_userIdentifier, IPartyVoice.EVoiceMemberState.Normal);
						return;
					}
				});
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public IEnumerator tryJoinLobbyCo(string _lobbyId)
		{
			Voice.<>c__DisplayClass83_0 CS$<>8__locals1 = new Voice.<>c__DisplayClass83_0();
			CS$<>8__locals1.<>4__this = this;
			EosHelpers.AssertMainThread("Voice.Join");
			int attempts = 0;
			CS$<>8__locals1.lobbyIdFound = null;
			CS$<>8__locals1.lobbyDetails = null;
			object obj;
			while (attempts < 5)
			{
				Voice.<>c__DisplayClass83_1 CS$<>8__locals2 = new Voice.<>c__DisplayClass83_1();
				CS$<>8__locals2.CS$<>8__locals1 = CS$<>8__locals1;
				int num = attempts;
				attempts = num + 1;
				Log.Out(string.Format("[EOS-Voice] Trying to find lobby for id {0}, attempt {1}", _lobbyId, attempts));
				CreateLobbySearchOptions createLobbySearchOptions = default(CreateLobbySearchOptions);
				createLobbySearchOptions.MaxResults = 10U;
				CreateLobbySearchOptions createLobbySearchOptions2 = createLobbySearchOptions;
				obj = AntiCheatCommon.LockObject;
				Result result;
				lock (obj)
				{
					result = this.lobbyInterface.CreateLobbySearch(ref createLobbySearchOptions2, ref CS$<>8__locals2.lobbySearch);
				}
				if (result != null)
				{
					Log.Error("[EOS-Voice] Create lobby search failed: " + result.ToStringCached<Result>());
					this.joinInProgress = false;
					yield break;
				}
				LobbySearchSetLobbyIdOptions lobbySearchSetLobbyIdOptions = default(LobbySearchSetLobbyIdOptions);
				lobbySearchSetLobbyIdOptions.LobbyId = _lobbyId;
				LobbySearchSetLobbyIdOptions lobbySearchSetLobbyIdOptions2 = lobbySearchSetLobbyIdOptions;
				obj = AntiCheatCommon.LockObject;
				lock (obj)
				{
					result = CS$<>8__locals2.lobbySearch.SetLobbyId(ref lobbySearchSetLobbyIdOptions2);
				}
				if (result != null)
				{
					Log.Error("[EOS-Voice] Set lobby search lobbyid failed: " + result.ToStringCached<Result>());
					obj = AntiCheatCommon.LockObject;
					lock (obj)
					{
						CS$<>8__locals2.lobbySearch.Release();
					}
					this.joinInProgress = false;
					yield break;
				}
				CS$<>8__locals2.findDone = false;
				CS$<>8__locals2.findError = false;
				CS$<>8__locals2.CS$<>8__locals1.lobbyDetails = null;
				LobbySearchFindOptions lobbySearchFindOptions = default(LobbySearchFindOptions);
				lobbySearchFindOptions.LocalUserId = this.localProductUserId;
				LobbySearchFindOptions lobbySearchFindOptions2 = lobbySearchFindOptions;
				obj = AntiCheatCommon.LockObject;
				lock (obj)
				{
					CS$<>8__locals2.lobbySearch.Find(ref lobbySearchFindOptions2, null, delegate(ref LobbySearchFindCallbackInfo _callbackData)
					{
						object obj2;
						if (_callbackData.ResultCode == 18)
						{
							obj2 = AntiCheatCommon.LockObject;
							lock (obj2)
							{
								CS$<>8__locals2.lobbySearch.Release();
							}
							CS$<>8__locals2.findDone = true;
							return;
						}
						if (_callbackData.ResultCode != null)
						{
							Log.Error("[EOS-Voice] Find lobby failed: " + _callbackData.ResultCode.ToStringCached<Result>());
							obj2 = AntiCheatCommon.LockObject;
							lock (obj2)
							{
								CS$<>8__locals2.lobbySearch.Release();
							}
							CS$<>8__locals2.findDone = true;
							CS$<>8__locals2.findError = true;
							return;
						}
						LobbySearchGetSearchResultCountOptions lobbySearchGetSearchResultCountOptions = default(LobbySearchGetSearchResultCountOptions);
						obj2 = AntiCheatCommon.LockObject;
						uint searchResultCount;
						lock (obj2)
						{
							searchResultCount = CS$<>8__locals2.lobbySearch.GetSearchResultCount(ref lobbySearchGetSearchResultCountOptions);
						}
						if (searchResultCount != 1U)
						{
							Log.Error(string.Format("[EOS-Voice] Find lobby returned unexpected number of results ({0})", searchResultCount));
							obj2 = AntiCheatCommon.LockObject;
							lock (obj2)
							{
								CS$<>8__locals2.lobbySearch.Release();
							}
							CS$<>8__locals2.findDone = true;
							return;
						}
						LobbySearchCopySearchResultByIndexOptions lobbySearchCopySearchResultByIndexOptions = default(LobbySearchCopySearchResultByIndexOptions);
						lobbySearchCopySearchResultByIndexOptions.LobbyIndex = 0U;
						LobbySearchCopySearchResultByIndexOptions lobbySearchCopySearchResultByIndexOptions2 = lobbySearchCopySearchResultByIndexOptions;
						obj2 = AntiCheatCommon.LockObject;
						Result result2;
						lock (obj2)
						{
							result2 = CS$<>8__locals2.lobbySearch.CopySearchResultByIndex(ref lobbySearchCopySearchResultByIndexOptions2, ref CS$<>8__locals2.CS$<>8__locals1.lobbyDetails);
						}
						if (result2 != null)
						{
							Log.Error("[EOS-Voice] Get lobby details failed: " + result2.ToStringCached<Result>());
							obj2 = AntiCheatCommon.LockObject;
							lock (obj2)
							{
								CS$<>8__locals2.lobbySearch.Release();
							}
							CS$<>8__locals2.CS$<>8__locals1.lobbyDetails = null;
							CS$<>8__locals2.findDone = true;
							CS$<>8__locals2.findError = true;
							return;
						}
						LobbyDetailsCopyInfoOptions lobbyDetailsCopyInfoOptions = default(LobbyDetailsCopyInfoOptions);
						obj2 = AntiCheatCommon.LockObject;
						LobbyDetailsInfo? lobbyDetailsInfo;
						lock (obj2)
						{
							result2 = CS$<>8__locals2.CS$<>8__locals1.lobbyDetails.CopyInfo(ref lobbyDetailsCopyInfoOptions, ref lobbyDetailsInfo);
						}
						if (result2 != null)
						{
							Log.Error("[EOS-Voice] Get lobby details info failed: " + result2.ToStringCached<Result>());
							obj2 = AntiCheatCommon.LockObject;
							lock (obj2)
							{
								CS$<>8__locals2.CS$<>8__locals1.lobbyDetails.Release();
							}
							CS$<>8__locals2.CS$<>8__locals1.lobbyDetails = null;
							obj2 = AntiCheatCommon.LockObject;
							lock (obj2)
							{
								CS$<>8__locals2.lobbySearch.Release();
							}
							CS$<>8__locals2.findDone = true;
							CS$<>8__locals2.findError = true;
							return;
						}
						CS$<>8__locals2.CS$<>8__locals1.lobbyIdFound = lobbyDetailsInfo.Value.LobbyId;
						Log.Out("[EOS-Voice] Found lobby: " + CS$<>8__locals2.CS$<>8__locals1.lobbyIdFound);
						CS$<>8__locals2.findDone = true;
						obj2 = AntiCheatCommon.LockObject;
						lock (obj2)
						{
							CS$<>8__locals2.lobbySearch.Release();
						}
					});
					goto IL_0278;
				}
				goto IL_0261;
				IL_0278:
				if (CS$<>8__locals2.findDone)
				{
					if (CS$<>8__locals2.findError)
					{
						CS$<>8__locals2.CS$<>8__locals1.lobbyDetails = null;
						Log.Error("[EOS-Voice] Failed joining voice lobby");
						this.joinInProgress = false;
						yield break;
					}
					if (CS$<>8__locals2.CS$<>8__locals1.lobbyIdFound == null)
					{
						yield return new WaitForSeconds(0.5f);
						CS$<>8__locals2 = null;
						continue;
					}
					break;
				}
				IL_0261:
				yield return null;
				goto IL_0278;
			}
			if (CS$<>8__locals1.lobbyDetails == null)
			{
				Log.Error("[EOS-Voice] Did not find lobby");
				this.joinInProgress = false;
				yield break;
			}
			Log.Out(string.Format("[EOS-Voice] Found lobby on {0} attempt", attempts));
			JoinLobbyOptions joinLobbyOptions = default(JoinLobbyOptions);
			joinLobbyOptions.LocalUserId = this.localProductUserId;
			joinLobbyOptions.PresenceEnabled = false;
			joinLobbyOptions.LobbyDetailsHandle = CS$<>8__locals1.lobbyDetails;
			LocalRTCOptions localRTCOptions = default(LocalRTCOptions);
			localRTCOptions.LocalAudioDeviceInputStartsMuted = true;
			joinLobbyOptions.LocalRTCOptions = new LocalRTCOptions?(localRTCOptions);
			JoinLobbyOptions joinLobbyOptions2 = joinLobbyOptions;
			obj = AntiCheatCommon.LockObject;
			lock (obj)
			{
				this.lobbyInterface.JoinLobby(ref joinLobbyOptions2, null, delegate(ref JoinLobbyCallbackInfo _callbackData)
				{
					object obj3;
					if (_callbackData.ResultCode != null)
					{
						Log.Error("[EOS-Voice] Join lobby failed: " + _callbackData.ResultCode.ToStringCached<Result>());
						obj3 = AntiCheatCommon.LockObject;
						lock (obj3)
						{
							CS$<>8__locals1.lobbyDetails.Release();
						}
						CS$<>8__locals1.<>4__this.joinInProgress = false;
						return;
					}
					CS$<>8__locals1.<>4__this.lobbyEntered(CS$<>8__locals1.lobbyIdFound);
					obj3 = AntiCheatCommon.LockObject;
					lock (obj3)
					{
						CS$<>8__locals1.lobbyDetails.Release();
					}
				});
				yield break;
			}
			yield break;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void lobbyEntered(string _lobbyId)
		{
			this.blockedUsers.Clear();
			this.lobbyId = _lobbyId;
			GetRTCRoomNameOptions getRTCRoomNameOptions = default(GetRTCRoomNameOptions);
			getRTCRoomNameOptions.LocalUserId = this.localProductUserId;
			getRTCRoomNameOptions.LobbyId = this.lobbyId;
			GetRTCRoomNameOptions getRTCRoomNameOptions2 = getRTCRoomNameOptions;
			object obj = AntiCheatCommon.LockObject;
			Utf8String utf8String;
			Result rtcroomName;
			lock (obj)
			{
				rtcroomName = this.lobbyInterface.GetRTCRoomName(ref getRTCRoomNameOptions2, ref utf8String);
			}
			if (rtcroomName != null)
			{
				Log.Error("[EOS-Voice] Getting local lobby room name failed: " + rtcroomName.ToStringCached<Result>());
			}
			this.roomName = utf8String;
			AddNotifyParticipantStatusChangedOptions addNotifyParticipantStatusChangedOptions = default(AddNotifyParticipantStatusChangedOptions);
			addNotifyParticipantStatusChangedOptions.LocalUserId = this.localProductUserId;
			addNotifyParticipantStatusChangedOptions.RoomName = this.roomName;
			AddNotifyParticipantStatusChangedOptions addNotifyParticipantStatusChangedOptions2 = addNotifyParticipantStatusChangedOptions;
			obj = AntiCheatCommon.LockObject;
			lock (obj)
			{
				this.participantStatusChangedHandle = this.rtcInterface.AddNotifyParticipantStatusChanged(ref addNotifyParticipantStatusChangedOptions2, null, new OnParticipantStatusChangedCallback(this.participantStatusChanged));
			}
			AddNotifyParticipantUpdatedOptions addNotifyParticipantUpdatedOptions = default(AddNotifyParticipantUpdatedOptions);
			addNotifyParticipantUpdatedOptions.LocalUserId = this.localProductUserId;
			addNotifyParticipantUpdatedOptions.RoomName = this.roomName;
			AddNotifyParticipantUpdatedOptions addNotifyParticipantUpdatedOptions2 = addNotifyParticipantUpdatedOptions;
			obj = AntiCheatCommon.LockObject;
			lock (obj)
			{
				this.participantUpdatedHandle = this.audioInterface.AddNotifyParticipantUpdated(ref addNotifyParticipantUpdatedOptions2, null, new OnParticipantUpdatedCallback(this.participantVoiceChanged));
			}
			this.SetRoomReceivingVolume(this.outputVolume);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void participantVoiceChanged(ref ParticipantUpdatedCallbackInfo _data)
		{
			if (Api.DebugLevel == Api.EDebugLevel.Verbose)
			{
				Log.Out(string.Format("[EOS-Voice] Participant update: {0}, speaking={1}, audio={2}", _data.ParticipantId, _data.Speaking, _data.AudioStatus));
			}
			UserIdentifierEos userIdentifierEos = new UserIdentifierEos(_data.ParticipantId);
			if (userIdentifierEos.Equals(this.localUserIdentifier))
			{
				return;
			}
			if (!this.blockedUsers.Contains(userIdentifierEos))
			{
				Action<PlatformUserIdentifierAbs, IPartyVoice.EVoiceMemberState> onRemotePlayerVoiceStateChanged = this.OnRemotePlayerVoiceStateChanged;
				if (onRemotePlayerVoiceStateChanged != null)
				{
					PlatformUserIdentifierAbs platformUserIdentifierAbs = userIdentifierEos;
					IPartyVoice.EVoiceMemberState evoiceMemberState;
					switch (_data.AudioStatus)
					{
					case 0:
						evoiceMemberState = IPartyVoice.EVoiceMemberState.Disabled;
						break;
					case 1:
						evoiceMemberState = IPartyVoice.EVoiceMemberState.VoiceActive;
						break;
					case 2:
						evoiceMemberState = IPartyVoice.EVoiceMemberState.Normal;
						break;
					case 3:
						evoiceMemberState = IPartyVoice.EVoiceMemberState.Muted;
						break;
					case 4:
						evoiceMemberState = IPartyVoice.EVoiceMemberState.Muted;
						break;
					default:
						throw new ArgumentOutOfRangeException();
					}
					onRemotePlayerVoiceStateChanged(platformUserIdentifierAbs, evoiceMemberState);
				}
				return;
			}
			Action<PlatformUserIdentifierAbs, IPartyVoice.EVoiceMemberState> onRemotePlayerVoiceStateChanged2 = this.OnRemotePlayerVoiceStateChanged;
			if (onRemotePlayerVoiceStateChanged2 == null)
			{
				return;
			}
			onRemotePlayerVoiceStateChanged2(userIdentifierEos, IPartyVoice.EVoiceMemberState.Muted);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void participantStatusChanged(ref ParticipantStatusChangedCallbackInfo _data)
		{
			if (this.Status != EPartyVoiceStatus.Ok)
			{
				return;
			}
			if (Api.DebugLevel == Api.EDebugLevel.Verbose)
			{
				Log.Out(string.Format("[EOS-Voice] Participant state changed: {0}, {1}", _data.ParticipantId, _data.ParticipantStatus));
			}
			UserIdentifierEos userIdentifierEos = new UserIdentifierEos(_data.ParticipantId);
			if (userIdentifierEos.Equals(this.localUserIdentifier))
			{
				this.roomEntered = _data.ParticipantStatus == 0;
				if (this.roomEntered)
				{
					this.createInProgress = false;
					this.joinInProgress = false;
					this.muteSelf = true;
					this.muteOthers = false;
				}
				Action<IPartyVoice.EVoiceChannelAction> onLocalPlayerStateChanged = this.OnLocalPlayerStateChanged;
				if (onLocalPlayerStateChanged == null)
				{
					return;
				}
				onLocalPlayerStateChanged((_data.ParticipantStatus == null) ? IPartyVoice.EVoiceChannelAction.Joined : IPartyVoice.EVoiceChannelAction.Left);
				return;
			}
			else
			{
				Action<PlatformUserIdentifierAbs, IPartyVoice.EVoiceChannelAction> onRemotePlayerStateChanged = this.OnRemotePlayerStateChanged;
				if (onRemotePlayerStateChanged == null)
				{
					return;
				}
				onRemotePlayerStateChanged(userIdentifierEos, (_data.ParticipantStatus == null) ? IPartyVoice.EVoiceChannelAction.Joined : IPartyVoice.EVoiceChannelAction.Left);
				return;
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void lobbyLeft()
		{
			if (this.Status != EPartyVoiceStatus.Ok)
			{
				return;
			}
			this.lobbyId = null;
			this.roomName = null;
			this.muteSelf = true;
			this.muteOthers = false;
			this.blockedUsers.Clear();
			if (this.participantStatusChangedHandle != 0UL)
			{
				object obj = AntiCheatCommon.LockObject;
				lock (obj)
				{
					this.rtcInterface.RemoveNotifyParticipantStatusChanged(this.participantStatusChangedHandle);
				}
			}
			if (this.participantUpdatedHandle != 0UL)
			{
				object obj = AntiCheatCommon.LockObject;
				lock (obj)
				{
					this.audioInterface.RemoveNotifyParticipantUpdated(this.participantUpdatedHandle);
				}
			}
			this.participantStatusChangedHandle = 0UL;
			this.participantUpdatedHandle = 0UL;
		}

		[CompilerGenerated]
		[PublicizedFrom(EAccessModifier.Private)]
		public void <AddAudioDevicesNotifications>g__OnAudioDevicesChanged|60_0(ref AudioDevicesChangedCallbackInfo data)
		{
			this.RefreshAudioDevices();
		}

		[CompilerGenerated]
		[PublicizedFrom(EAccessModifier.Internal)]
		public static void <SetRoomReceivingVolume>g__OnUpdateReceivingVolume|80_0(ref UpdateReceivingVolumeCallbackInfo data)
		{
			if (data.ResultCode != null)
			{
				Log.Error(string.Format("[EOS-Voice] Setting voice output volume for room '{0}' failed: {1}", data.RoomName, data.ResultCode.ToStringCached<Result>()));
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public const int voiceLobbyConnectAttempts = 5;

		[PublicizedFrom(EAccessModifier.Private)]
		public const float voiceLobbyConnectAttemptInterval = 0.5f;

		[PublicizedFrom(EAccessModifier.Private)]
		public const float platformVolumeToEosRtcVolume = 50f;

		[PublicizedFrom(EAccessModifier.Private)]
		public IPlatform owner;

		[PublicizedFrom(EAccessModifier.Private)]
		public Api api;

		[PublicizedFrom(EAccessModifier.Private)]
		public RTCInterface rtcInterface;

		[PublicizedFrom(EAccessModifier.Private)]
		public RTCAudioInterface audioInterface;

		[PublicizedFrom(EAccessModifier.Private)]
		public LobbyInterface lobbyInterface;

		[PublicizedFrom(EAccessModifier.Private)]
		public string lobbyId;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool createInProgress;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool joinInProgress;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool leaveInProgress;

		[PublicizedFrom(EAccessModifier.Private)]
		public string roomName;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool roomEntered;

		[PublicizedFrom(EAccessModifier.Private)]
		public ulong participantStatusChangedHandle;

		[PublicizedFrom(EAccessModifier.Private)]
		public ulong participantUpdatedHandle;

		[PublicizedFrom(EAccessModifier.Private)]
		public Action initializedDelegates;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly object initializedDelegateLock = new object();

		[PublicizedFrom(EAccessModifier.Private)]
		public string activeInputDeviceId;

		[PublicizedFrom(EAccessModifier.Private)]
		public string activeOutputDeviceId;

		[PublicizedFrom(EAccessModifier.Private)]
		public ulong audioDevicesChangedNotificationId;

		[PublicizedFrom(EAccessModifier.Private)]
		public IList<IPartyVoice.VoiceAudioDevice> inputDevices;

		[PublicizedFrom(EAccessModifier.Private)]
		public IList<IPartyVoice.VoiceAudioDevice> outputDevices;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool muteSelf;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool muteOthers;

		[PublicizedFrom(EAccessModifier.Private)]
		public float outputVolume = 1f;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly HashSet<PlatformUserIdentifierAbs> blockedUsers = new HashSet<PlatformUserIdentifierAbs>();

		public class EosAudioDevice : IPartyVoice.VoiceAudioDevice
		{
			public EosAudioDevice(InputDeviceInformation _device)
				: base(false, _device.DefaultDevice)
			{
				this.Id = _device.DeviceId;
				this.Name = _device.DeviceName;
				this.LogDevice("Input");
			}

			public EosAudioDevice(OutputDeviceInformation _device)
				: base(true, _device.DefaultDevice)
			{
				this.Id = _device.DeviceId;
				this.Name = _device.DeviceName;
				this.LogDevice("Output");
			}

			[PublicizedFrom(EAccessModifier.Private)]
			public void LogDevice(string _inOutString)
			{
				if (GameUtils.GetLaunchArgument("debugeos") != null)
				{
					Log.Out(string.Format("[EOS-Voice] {0} device: Id={1}, Name={2}, Default={3}", new object[] { _inOutString, this.Id, this.Name, this.IsDefault }));
				}
			}

			public override string ToString()
			{
				if (!this.IsDefault)
				{
					return this.Name;
				}
				return "(Default) " + this.Name;
			}

			public override string Identifier
			{
				get
				{
					return this.Id;
				}
			}

			public readonly string Id;

			public readonly string Name;
		}
	}
}
