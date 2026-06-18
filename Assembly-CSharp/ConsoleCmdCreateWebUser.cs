using System;
using System.Collections.Generic;
using System.Text;
using Platform;
using UnityEngine.Scripting;
using Webserver;

[Preserve]
public class ConsoleCmdCreateWebUser : ConsoleCmdAbstract
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[] { "createwebuser" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Create a web dashboard user account";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "";
	}

	public override int DefaultPermissionLevel
	{
		get
		{
			return 1000;
		}
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (_senderInfo.NetworkConnection != null)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Command can only be executed from the in-game console.");
			return;
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			if (_senderInfo.IsLocalGame)
			{
				string @string = GamePrefs.GetString(EnumGamePrefs.PlayerName);
				PlatformUserIdentifierAbs platformUserId = PlatformManager.NativePlatform.User.PlatformUserId;
				IPlatform crossplatformPlatform = PlatformManager.CrossplatformPlatform;
				PlatformUserIdentifierAbs platformUserIdentifierAbs;
				if (crossplatformPlatform == null)
				{
					platformUserIdentifierAbs = null;
				}
				else
				{
					IUserClient user = crossplatformPlatform.User;
					platformUserIdentifierAbs = ((user != null) ? user.PlatformUserId : null);
				}
				string text = this.createToken(@string, platformUserId, platformUserIdentifierAbs);
				string text2 = this.createRegistrationPageUrl(text, true);
				this.openUserRegistrationPage(text2);
				return;
			}
			string text3 = this.createToken(_senderInfo.RemoteClientInfo.playerName, _senderInfo.RemoteClientInfo.PlatformId, _senderInfo.RemoteClientInfo.CrossplatformId);
			string text4 = this.createRegistrationPageUrl(text3, false);
			_senderInfo.RemoteClientInfo.SendPackage(NetPackageManager.GetPackage<NetPackageConsoleCmdClient>().Setup(this.GetCommands()[0] + " " + Convert.ToBase64String(Encoding.UTF8.GetBytes(text4)), true));
			return;
		}
		else
		{
			if (_params.Count < 1)
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Missing URL in server reply");
				return;
			}
			string string2 = Encoding.UTF8.GetString(Convert.FromBase64String(_params[0]));
			this.openUserRegistrationPage(string2);
			return;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void openUserRegistrationPage(string _url)
	{
		GUIWindowConsole.Close();
		XUiC_MessageBoxWindowGroup.ShowUrlConfirmationDialog(LocalPlayerUI.GetUIForPrimaryPlayer().xui, _url, true, new Func<string, bool>(Utils.OpenSystemBrowser), null, Localization.Get("xuiOpenUserCreationConfirmationText", false, null), null, false);
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Requested browser for user creation at " + _url);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string createRegistrationPageUrl(string _token, bool _isLocalOnListenServer = false)
	{
		int @int = GamePrefs.GetInt(EnumGamePrefs.WebDashboardPort);
		string text;
		if (_isLocalOnListenServer)
		{
			text = string.Format("http://localhost:{0}/", @int);
		}
		else
		{
			string @string = GamePrefs.GetString(EnumGamePrefs.WebDashboardUrl);
			if (!string.IsNullOrEmpty(@string))
			{
				string text2 = @string;
				string text3 = @string;
				text = text2 + ((text3[text3.Length - 1] == '/') ? "" : "/");
			}
			else
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Server does not specify an explicit WebDashboardUrl, using game server's public IP");
				text = string.Format("http://{0}:{1}/", SingletonMonoBehaviour<ConnectionManager>.Instance.LocalServerInfo.GetValue(GameInfoString.IP), @int);
			}
		}
		return text + "app/createuser?token=" + _token;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string createToken(string _playerName, PlatformUserIdentifierAbs _platformUserId, PlatformUserIdentifierAbs _crossPlatformUserId)
	{
		return UserRegistrationTokens.CreateToken(_playerName, _platformUserId, _crossPlatformUserId);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const string registrationPagePath = "app/createuser";
}
