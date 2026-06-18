using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Platform;

public static class VoiceHelpers
{
	public static bool VoiceAllowed
	{
		get
		{
			return PermissionsManager.IsCommunicationAllowed();
		}
	}

	public static bool PlatformVoiceEnabled
	{
		get
		{
			return GamePrefs.GetBool(EnumGamePrefs.OptionsVoiceChatEnabled) && VoiceHelpers.VoiceAllowed;
		}
	}

	public static bool InAnyVoiceChat
	{
		get
		{
			return VoiceHelpers.VoiceAllowed && (DiscordManager.Instance.ActiveVoiceLobby != null || PartyVoice.Instance.InVoice);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool pushToTalkButtonValid(LocalPlayerUI _playerUI)
	{
		bool controlKeyPressed = InputUtils.ControlKeyPressed;
		bool flag = _playerUI.windowManager.IsInputActive();
		bool flag2 = PlatformManager.NativePlatform.Input.CurrentInputStyle != PlayerInputManager.InputStyle.Keyboard && GameManager.Instance.isAnyCursorWindowOpen(null);
		return (!GameManager.Instance.IsEditMode() || !controlKeyPressed) && !flag && !flag2;
	}

	public static bool PushToTalkPressed()
	{
		LocalPlayerUI uiforPrimaryPlayer = LocalPlayerUI.GetUIForPrimaryPlayer();
		return !(uiforPrimaryPlayer == null) && uiforPrimaryPlayer.playerInput != null && uiforPrimaryPlayer.playerInput.PermanentActions.PushToTalk.IsPressed && VoiceHelpers.pushToTalkButtonValid(uiforPrimaryPlayer);
	}

	public static bool PushToTalkWasPressed()
	{
		LocalPlayerUI uiforPrimaryPlayer = LocalPlayerUI.GetUIForPrimaryPlayer();
		return !(uiforPrimaryPlayer == null) && uiforPrimaryPlayer.playerInput != null && uiforPrimaryPlayer.playerInput.PermanentActions.PushToTalk.WasPressed && VoiceHelpers.pushToTalkButtonValid(uiforPrimaryPlayer);
	}

	public static bool LocalPlayerTalking()
	{
		if (PartyVoice.Instance.SendingVoice)
		{
			return true;
		}
		if (DiscordManager.Instance.IsReady)
		{
			DiscordManager.LobbyInfo activeVoiceLobby = DiscordManager.Instance.ActiveVoiceLobby;
			return activeVoiceLobby != null && activeVoiceLobby.VoiceCall.IsSpeaking;
		}
		return false;
	}

	public static IPartyVoice.EVoiceMemberState GetPlayerVoiceState(EntityPlayer _player, bool _partyOnly = false)
	{
		if (_player == null)
		{
			return IPartyVoice.EVoiceMemberState.Disabled;
		}
		IPartyVoice.EVoiceMemberState playerDiscordVoiceState = VoiceHelpers.GetPlayerDiscordVoiceState(_player, _partyOnly);
		IPartyVoice.EVoiceMemberState evoiceMemberState;
		if (GamePrefs.GetBool(EnumGamePrefs.OptionsVoiceChatEnabled))
		{
			PersistentPlayerData playerDataFromEntityID = GameManager.Instance.persistentPlayers.GetPlayerDataFromEntityID(_player.entityId);
			evoiceMemberState = ((playerDataFromEntityID == null) ? IPartyVoice.EVoiceMemberState.Disabled : PartyVoice.Instance.GetVoiceMemberState(playerDataFromEntityID.PrimaryId));
		}
		else
		{
			evoiceMemberState = IPartyVoice.EVoiceMemberState.Disabled;
		}
		if (playerDiscordVoiceState < evoiceMemberState)
		{
			return evoiceMemberState;
		}
		return playerDiscordVoiceState;
	}

	public static IPartyVoice.EVoiceMemberState GetPlayerDiscordVoiceState(EntityPlayer _player, bool _partyOnly = false)
	{
		if (_player == null)
		{
			return IPartyVoice.EVoiceMemberState.Disabled;
		}
		if (!DiscordManager.Instance.IsReady)
		{
			return IPartyVoice.EVoiceMemberState.Disabled;
		}
		DiscordManager.LobbyInfo activeVoiceLobby = DiscordManager.Instance.ActiveVoiceLobby;
		if (activeVoiceLobby == null)
		{
			return IPartyVoice.EVoiceMemberState.Disabled;
		}
		if (!activeVoiceLobby.IsInVoice)
		{
			return IPartyVoice.EVoiceMemberState.Disabled;
		}
		if (_partyOnly && activeVoiceLobby.LobbyType != DiscordManager.ELobbyType.Party)
		{
			return IPartyVoice.EVoiceMemberState.Disabled;
		}
		DiscordManager.DiscordUser discordUser;
		if (!DiscordManager.Instance.TryGetUserFromEntity(_player, out discordUser) || !discordUser.InCurrentVoice)
		{
			return IPartyVoice.EVoiceMemberState.Disabled;
		}
		return discordUser.VoiceState;
	}

	public static bool IsSnapWithoutMicPermission()
	{
		if (VoiceHelpers.snapWithoutMicPermission != null)
		{
			return VoiceHelpers.snapWithoutMicPermission.Value;
		}
		if (GameIO.IsRunningAsSnap() == null)
		{
			VoiceHelpers.snapWithoutMicPermission = new bool?(false);
			return false;
		}
		VoiceHelpers.snapWithoutMicPermission = new bool?(!VoiceHelpers.<IsSnapWithoutMicPermission>g__SnapHasMicPermission|13_0());
		return VoiceHelpers.snapWithoutMicPermission.Value;
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Internal)]
	public static bool <IsSnapWithoutMicPermission>g__SnapHasMicPermission|13_0()
	{
		ProcessStartInfo processStartInfo;
		if (GameIO.IsRunningInSteamRuntime())
		{
			processStartInfo = new ProcessStartInfo
			{
				FileName = "/usr/bin/steam-runtime-launch-client",
				Arguments = " --alongside-steam -- /usr/bin/snapctl is-connected audio-record"
			};
		}
		else
		{
			processStartInfo = new ProcessStartInfo
			{
				FileName = "/usr/bin/snapctl",
				Arguments = "is-connected audio-record"
			};
		}
		processStartInfo.CreateNoWindow = true;
		processStartInfo.UseShellExecute = false;
		processStartInfo.RedirectStandardError = true;
		processStartInfo.RedirectStandardOutput = true;
		bool flag;
		try
		{
			Process process = Process.Start(processStartInfo);
			if (process == null)
			{
				Log.Out("Snap microphone permission check: Could not run snapctl");
				flag = false;
			}
			else
			{
				string text = process.StandardError.ReadToEnd();
				process.WaitForExit(150);
				if (!process.HasExited)
				{
					process.Kill();
					Log.Error("Snap microphone permission check: snapctl did not terminate");
					flag = false;
				}
				else
				{
					bool flag2 = process.ExitCode == 0;
					Log.Out(string.Format("Snap microphone permission check done: Has permission: {0}", flag2));
					if (text.Length > 5)
					{
						Log.Out("ErrOut: " + text);
					}
					flag = flag2;
				}
			}
		}
		catch (Exception ex)
		{
			Log.Error("Snap microphone permission check failed with exception: " + ex.Message);
			flag = false;
		}
		return flag;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool? snapWithoutMicPermission;
}
