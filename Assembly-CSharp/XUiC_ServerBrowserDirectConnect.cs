using System;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_ServerBrowserDirectConnect : XUiController
{
	public string Ip
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			XUiC_TextInput xuiC_TextInput = this.txtIp;
			if (xuiC_TextInput == null)
			{
				return null;
			}
			return xuiC_TextInput.Text.Trim();
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			this.txtIp.Text = value;
			this.IsDirty = true;
		}
	}

	public int Port
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			int num;
			if (int.TryParse(this.txtPort.Text, out num))
			{
				return num;
			}
			return -1;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			this.txtPort.Text = value.ToString();
			this.IsDirty = true;
		}
	}

	[XuiXmlBinding("validdata")]
	public bool ValidData
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this.validData;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			this.validData = value;
			this.IsDirty = true;
		}
	}

	public override void Init()
	{
		base.Init();
		this.txtIp.SelectOnTab = this.txtPort;
		this.txtPort.SelectOnTab = this.txtIp;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		base.handleDirtyUpdateDefault();
		if (XUiUtils.HotkeysAllowedFor(this.viewComponent, false))
		{
			if (this.xui.playerUI.playerInput.PermanentActions.Cancel.WasReleased)
			{
				this.BtnCancel_OnPressed(this, -1);
			}
			if (this.xui.playerUI.playerInput.GUIActions.Apply.WasReleased)
			{
				this.BtnConnect_OnPressed(this, -1);
			}
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		if (this.txtIp != null)
		{
			this.Ip = GamePrefs.GetString(EnumGamePrefs.ConnectToServerIP);
			this.Port = GamePrefs.GetInt(EnumGamePrefs.ConnectToServerPort);
			this.txtIp.SetSelected(true, false);
			this.validateIpPort(null, null, true);
		}
		XUiC_Button xuiC_Button = this.btnCancel;
		if (xuiC_Button == null)
		{
			return;
		}
		xuiC_Button.SelectCursorElement(true, false);
	}

	public override void OnClose()
	{
		base.OnClose();
		this.saveIpPort();
		XUiC_MultiplayerPrivilegeNotification.Close();
		this.cancelled = null;
		this.confirmed = null;
	}

	[XuiBindEvent("OnClipboardHandler", "txtIp")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void TxtIp_OnClipboardHandler(UIInput.ClipboardAction _actionType, string _oldText, int _selStart, int _selEnd, string _actionResultText)
	{
		if (_actionType != 1)
		{
			return;
		}
		if (_selEnd - _selStart != _oldText.Length)
		{
			return;
		}
		Match match = XUiC_ServerBrowserDirectConnect.ipPortMatcher.Match(_actionResultText);
		if (!match.Success)
		{
			return;
		}
		string value = match.Groups[1].Value;
		int num;
		if (!StringParsers.TryParseSInt32(match.Groups[2].Value, out num))
		{
			return;
		}
		this.Ip = value;
		this.Port = num;
	}

	[XuiBindEvent("OnSubmitHandler", "txtIp")]
	[XuiBindEvent("OnSubmitHandler", "txtPort")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void Txt_OnSubmitHandler(XUiController _sender, string _text)
	{
		this.BtnConnect_OnPressed(_sender, -1);
	}

	[XuiBindEvent("OnPress", "btnConnect")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnConnect_OnPressed(XUiController _sender, int _mouseButton)
	{
		if (!this.btnConnect.ViewComponent.Enabled)
		{
			return;
		}
		Action action = this.confirmed;
		this.xui.playerUI.windowManager.Close(this.windowGroup, false);
		if (action != null)
		{
			action();
		}
		XUiC_MultiplayerPrivilegeNotification.ResolvePrivilegesWithDialog(EUserPerms.Multiplayer, delegate(bool _result)
		{
			if (!_result)
			{
				return;
			}
			string text = this.Ip;
			long num;
			if (!long.TryParse(text.Replace(".", ""), out num))
			{
				try
				{
					IPHostEntry hostEntry = Dns.GetHostEntry(text);
					if (hostEntry.AddressList.Length == 0)
					{
						Log.Out("No valid IP for server found");
						return;
					}
					text = hostEntry.AddressList[0].ToString();
					if (hostEntry.AddressList[0].AddressFamily != AddressFamily.InterNetwork)
					{
						for (int i = 1; i < hostEntry.AddressList.Length; i++)
						{
							if (hostEntry.AddressList[i].AddressFamily == AddressFamily.InterNetwork)
							{
								text = hostEntry.AddressList[i].ToString();
								break;
							}
						}
					}
				}
				catch (SocketException ex)
				{
					string text2 = "No such hostname: \"";
					string text3 = text;
					string text4 = "\": ";
					SocketException ex2 = ex;
					Log.Out(text2 + text3 + text4 + ((ex2 != null) ? ex2.ToString() : null));
					return;
				}
			}
			Log.Out("Connect by IP");
			GameServerInfo gameServerInfo = new GameServerInfo();
			gameServerInfo.SetValue(GameInfoString.IP, text);
			gameServerInfo.SetValue(GameInfoInt.Port, this.Port);
			GameManager.Instance.showOpenerMovieOnLoad = false;
			SingletonMonoBehaviour<ConnectionManager>.Instance.LastGameServerInfo = gameServerInfo;
			SingletonMonoBehaviour<ConnectionManager>.Instance.Connect(SingletonMonoBehaviour<ConnectionManager>.Instance.LastGameServerInfo);
		}, -1f, false, null);
	}

	[XuiBindEvent("OnPress", "btnCancel")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnCancel_OnPressed(XUiController _sender, int _mouseButton)
	{
		Action action = this.cancelled;
		this.xui.playerUI.windowManager.Close(this.windowGroup, false);
		if (action == null)
		{
			return;
		}
		action();
	}

	[XuiBindEvent("OnChangeHandler", "txtIp")]
	[XuiBindEvent("OnChangeHandler", "txtPort")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void validateIpPort(XUiController _sender, string _newText, bool _changeFromCode)
	{
		int port = this.Port;
		this.ValidData = port > 0 && port < 65533 && this.Ip.Length > 0;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void saveIpPort()
	{
		if (!this.ValidData)
		{
			return;
		}
		GamePrefs.Set(EnumGamePrefs.ConnectToServerIP, this.Ip);
		GamePrefs.Set(EnumGamePrefs.ConnectToServerPort, this.Port);
	}

	public static void Open(XUi _xui, Action _onConfirmed, Action _onCancelled)
	{
		XUiC_ServerBrowserDirectConnect childByType = _xui.GetChildByType<XUiC_ServerBrowserDirectConnect>();
		childByType.confirmed = _onConfirmed;
		childByType.cancelled = _onCancelled;
		_xui.playerUI.windowManager.Open(childByType.windowGroup, false);
	}

	[XuiBindComponent("txtIp", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_TextInput txtIp;

	[XuiBindComponent("txtPort", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_TextInput txtPort;

	[XuiBindComponent("btnConnect", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnConnect;

	[XuiBindComponent("btnCancel", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnCancel;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Regex ipPortMatcher = new Regex("^(.*):(\\d+)$");

	[PublicizedFrom(EAccessModifier.Private)]
	public Action confirmed;

	[PublicizedFrom(EAccessModifier.Private)]
	public Action cancelled;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool validData;
}
