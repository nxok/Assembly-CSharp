using System;
using System.Collections;
using Steamworks;

namespace Platform.Steam
{
	public class VirtualKeyboard : IVirtualKeyboard
	{
		public void Init(IPlatform _owner)
		{
			_owner.Api.ClientApiInitialized += delegate
			{
				if (GameManager.IsDedicatedServer)
				{
					return;
				}
				if (SteamUtils.IsSteamInBigPictureMode())
				{
					if (this.m_TextInputDismissed == null)
					{
						this.m_TextInputDismissed = Callback<GamepadTextInputDismissed_t>.Create(new Callback<GamepadTextInputDismissed_t>.DispatchDelegate(this.GamePadTextInputDismissed_Callback));
						return;
					}
				}
				else
				{
					Log.Out("Not running in Big Picture Mode, no on-screen keyboard available");
				}
			};
		}

		public string Open(string _title, string _defaultText, Action<bool, string> _onTextReceived, UIInput.InputType _mode = 0, bool _multiLine = false, uint singleLineLength = 200U)
		{
			if (this.onTextReceived != null)
			{
				Log.Warning("The virtual keyboard was already opened and has not closed yet.");
				return null;
			}
			if (_onTextReceived == null)
			{
				throw new ArgumentException("The callback function must not be null");
			}
			this.textBefore = _defaultText;
			this.onTextReceived = _onTextReceived;
			if (SteamUtils.ShowGamepadTextInput((_mode == 2) ? 1 : 0, _multiLine ? 1 : 0, _title, _multiLine ? 500U : singleLineLength, _defaultText))
			{
				return null;
			}
			Log.Out("Opening OnScreen keyboard failed, probably not running in Steam Big Picture Mode");
			this.onTextReceived(false, _defaultText);
			this.onTextReceived = null;
			return Localization.Get("ttSteamBPM", false, null);
		}

		public void Destroy()
		{
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void GamePadTextInputDismissed_Callback(GamepadTextInputDismissed_t _result)
		{
			Action<bool, string> action = this.onTextReceived;
			this.onTextReceived = null;
			string text;
			bool enteredGamepadTextInput = SteamUtils.GetEnteredGamepadTextInput(ref text, 500U);
			Log.Out("OnScreen keyboard result: ok={0}, submitted={1}, text={2}", new object[] { enteredGamepadTextInput, _result.m_bSubmitted, text });
			if (action != null)
			{
				action(_result.m_bSubmitted, _result.m_bSubmitted ? text : (this.textBefore ?? ""));
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public IEnumerator debugOut()
		{
			int i = 0;
			while (i < 100)
			{
				Log.Out("POST: Enabled={3}, Is={0}, Down={1}, Up={2}", new object[]
				{
					PlatformManager.NativePlatform.Input.PrimaryPlayer.GUIActions.Cancel.IsPressed,
					PlatformManager.NativePlatform.Input.PrimaryPlayer.GUIActions.Cancel.WasPressed,
					PlatformManager.NativePlatform.Input.PrimaryPlayer.GUIActions.Cancel.WasReleased,
					PlatformManager.NativePlatform.Input.PrimaryPlayer.GUIActions.Cancel.Enabled
				});
				int num = i;
				i = num + 1;
				yield return null;
			}
			yield break;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public Callback<GamepadTextInputDismissed_t> m_TextInputDismissed;

		[PublicizedFrom(EAccessModifier.Private)]
		public Action<bool, string> onTextReceived;

		[PublicizedFrom(EAccessModifier.Private)]
		public string textBefore;
	}
}
