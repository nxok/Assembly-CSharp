using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Platform
{
	public interface IPartyVoice
	{
		event Action Initialized;

		EPartyVoiceStatus Status { get; }

		bool InLobby { get; }

		bool InLobbyOrProgress { get; }

		void Init(IPlatform _owner);

		void Destroy();

		void CreateLobby(Action<string> _lobbyCreatedCallback);

		void JoinLobby(string _lobbyId);

		void LeaveLobby();

		void PromoteLeader(PlatformUserIdentifierAbs _newLeaderIdentifier);

		bool IsLobbyOwner();

		event Action<IPartyVoice.EVoiceChannelAction> OnLocalPlayerStateChanged;

		event Action<PlatformUserIdentifierAbs, IPartyVoice.EVoiceChannelAction> OnRemotePlayerStateChanged;

		event Action<PlatformUserIdentifierAbs, IPartyVoice.EVoiceMemberState> OnRemotePlayerVoiceStateChanged;

		[return: TupleElementNames(new string[] { "inputDevices", "outputDevices" })]
		ValueTuple<IList<IPartyVoice.VoiceAudioDevice>, IList<IPartyVoice.VoiceAudioDevice>> GetDevicesList();

		void SetInputDevice(string _device);

		void SetOutputDevice(string _device);

		bool MuteSelf { get; set; }

		bool MuteOthers { get; set; }

		float OutputVolume { get; set; }

		void BlockUser(PlatformUserIdentifierAbs _userIdentifier, bool _block);

		public enum EVoiceMemberState
		{
			Disabled,
			Normal,
			Muted,
			VoiceActive
		}

		public enum EVoiceChannelAction
		{
			Joined,
			Left
		}

		public abstract class VoiceAudioDevice
		{
			public VoiceAudioDevice(bool _isOutput, bool _isDefault)
			{
				this.IsOutput = _isOutput;
				this.IsDefault = _isDefault;
			}

			public abstract string Identifier { get; }

			public readonly bool IsOutput;

			public readonly bool IsDefault;
		}

		public class VoiceAudioDeviceNotFound : IPartyVoice.VoiceAudioDevice
		{
			public VoiceAudioDeviceNotFound()
				: base(false, false)
			{
			}

			public override string ToString()
			{
				return Localization.Get("noAudioDeviceFound", false, null);
			}

			public override string Identifier
			{
				get
				{
					return "";
				}
			}
		}

		public class VoiceAudioDeviceDefault : IPartyVoice.VoiceAudioDevice
		{
			public VoiceAudioDeviceDefault()
				: base(false, false)
			{
			}

			public override string ToString()
			{
				return Localization.Get("defaultAudioDevice", false, null);
			}

			public override string Identifier
			{
				get
				{
					return "";
				}
			}
		}
	}
}
