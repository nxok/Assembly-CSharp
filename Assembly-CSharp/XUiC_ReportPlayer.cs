using System;
using System.Collections.Generic;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_ReportPlayer : XUiController
{
	[XuiXmlBinding("reportedplayer")]
	public string ReportedPlayerName
	{
		get
		{
			return this.reportedPlayerName ?? "";
		}
		set
		{
			this.reportedPlayerName = value;
			this.IsDirty = true;
		}
	}

	public int ReportedEntityId
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this.reportedEntityId;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			this.reportedEntityId = value;
			this.IsDirty = true;
		}
	}

	[XuiXmlBinding("playeronserver")]
	public bool PlayerOnServer
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && this.ReportedEntityId != -1;
		}
	}

	[XuiXmlBinding("inputWarning")]
	public string InputErrorMessage
	{
		get
		{
			return this.inputErrorMessage ?? "";
		}
		set
		{
			this.inputErrorMessage = value;
			this.IsDirty = true;
		}
	}

	public override void Init()
	{
		base.Init();
		XUiC_ReportPlayer.id = base.WindowGroup.Id;
		IPlayerReporting playerReporting = PlatformManager.MultiPlatform.PlayerReporting;
		IList<IPlayerReporting.PlayerReportCategory> list = ((playerReporting != null) ? playerReporting.ReportCategories() : null);
		if (list != null)
		{
			foreach (IPlayerReporting.PlayerReportCategory playerReportCategory in list)
			{
				this.cbxCategory.Elements.Add(playerReportCategory);
			}
		}
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		base.handleDirtyUpdateDefault();
	}

	[XuiBindEvent("OnPress", "btnCancel")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnCancel_OnPressed(XUiController _sender, int _mouseButton)
	{
		this.xui.playerUI.windowManager.Close(this.windowGroup, false);
	}

	[XuiBindEvent("OnPress", "btnKickPlayer")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnKick_OnPressed(XUiController _sender, int _mouseButton)
	{
		if (this.reportedPlayerData != null)
		{
			PlatformUserIdentifierAbs platformUserIdentifierAbs;
			ClientInfo clientInfo;
			if (ConsoleHelper.ParseParamPartialNameOrId(this.reportedPlayerData.PlayerName.Text, out platformUserIdentifierAbs, out clientInfo, true) == 1)
			{
				DateTime maxValue = DateTime.MaxValue;
				string text = "";
				if (clientInfo != null)
				{
					ClientInfo clientInfo2 = clientInfo;
					GameUtils.EKickReason ekickReason = GameUtils.EKickReason.ManualKick;
					int num = 0;
					string text2 = (string.IsNullOrEmpty(text) ? "" : text);
					GameUtils.KickPlayerForClientInfo(clientInfo2, new GameUtils.KickPlayerData(ekickReason, num, maxValue, text2));
				}
			}
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("[xui] Kick Succeeded");
		}
		else
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("[xui] Failed to find player to kick");
		}
		this.xui.playerUI.windowManager.Close(this.windowGroup, false);
	}

	[XuiBindEvent("OnPress", "btnSend")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnSend_OnPressed(XUiController _sender, int _mouseButton)
	{
		if (this.reportedPlayerData == null)
		{
			this.reportSentMessageBox(false);
			return;
		}
		string text = this.txtMessage.Text;
		if (!string.IsNullOrEmpty(this.initialText))
		{
			text = text + "\n" + string.Format(Localization.Get("xuiReportAutomatedMessage", false, null), this.initialText, this.initialCategory);
		}
		IPlayerReporting playerReporting = PlatformManager.MultiPlatform.PlayerReporting;
		if (playerReporting != null)
		{
			playerReporting.ReportPlayer(this.reportedPlayerData.PrimaryId, this.cbxCategory.Value, text, new Action<bool>(this.reportSentMessageBox));
		}
		this.xui.playerUI.windowManager.Close(this.windowGroup, false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void reportSentMessageBox(bool _success)
	{
		string text = (_success ? Localization.Get("xuiReportPlayerSuccess", false, null) : Localization.Get("xuiReportPlayerFail", false, null));
		XUiC_MessageBoxWindowGroup.ShowOk(this.xui, Localization.Get("xuiReportPlayerHeader", false, null), text, null, false, XUiC_ReportPlayer.OpenDialogsModal, false);
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.txtMessage.Text = this.initialText;
		this.InputErrorMessage = null;
		XUiC_ComboBoxList<IPlayerReporting.PlayerReportCategory> xuiC_ComboBoxList = this.cbxCategory;
		List<IPlayerReporting.PlayerReportCategory> elements = this.cbxCategory.Elements;
		IPlayerReporting playerReporting = PlatformManager.MultiPlatform.PlayerReporting;
		xuiC_ComboBoxList.SelectedIndex = elements.IndexOf((playerReporting != null) ? playerReporting.GetPlayerReportCategoryMapping(this.initialCategory) : null);
		if (this.cbxCategory.SelectedIndex == -1)
		{
			this.cbxCategory.SelectedIndex = 0;
		}
		if (this.reportedPlayerData != null)
		{
			this.ReportedPlayerName = GeneratedTextManager.GetDisplayTextImmediately(this.reportedPlayerData.PlayerName, false, GeneratedTextManager.TextFilteringMode.Filter, GeneratedTextManager.BbCodeSupportMode.SupportedAndAddEscapes);
			PersistentPlayerList persistentPlayers = GameManager.Instance.persistentPlayers;
			int? num;
			if (persistentPlayers == null)
			{
				num = null;
			}
			else
			{
				PersistentPlayerData playerData = persistentPlayers.GetPlayerData(this.reportedPlayerData.PrimaryId);
				num = ((playerData != null) ? new int?(playerData.EntityId) : null);
			}
			this.ReportedEntityId = num ?? (-1);
			return;
		}
		Log.Out("Sign does not have an author, cannot report player.");
		this.xui.playerUI.windowManager.Close(this.windowGroup, false);
	}

	public override void OnClose()
	{
		base.OnClose();
		this.reportedPlayerData = null;
	}

	[XuiBindEvent("OnInputErrorHandler", "txtMessage")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void updateErrorMessage(XUiController _sender, string _errorMessage)
	{
		this.InputErrorMessage = _errorMessage;
	}

	public static void Open(PlayerData _reportedPlayerData, EnumReportCategory _initialCategory = EnumReportCategory.None, string _initialText = "")
	{
		if (LocalPlayerUI.primaryUI.windowManager.IsWindowOpen(XUiC_ReportPlayer.id))
		{
			return;
		}
		XUiC_ReportPlayer childByType = LocalPlayerUI.primaryUI.xui.GetChildByType<XUiC_ReportPlayer>();
		childByType.reportedPlayerData = _reportedPlayerData;
		childByType.initialCategory = _initialCategory;
		childByType.initialText = _initialText;
		LocalPlayerUI.primaryUI.windowManager.Open(childByType.windowGroup, XUiC_ReportPlayer.OpenDialogsModal);
	}

	public static bool OpenDialogsModal
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return GameManager.Instance.gameStateManager.IsGameStarted();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string id;

	[PublicizedFrom(EAccessModifier.Private)]
	public string initialText = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public EnumReportCategory initialCategory;

	[XuiBindComponent("cbxCategory", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ComboBoxList<IPlayerReporting.PlayerReportCategory> cbxCategory;

	[XuiBindComponent("txtMessage", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_TextInput txtMessage;

	[XuiBindComponent("btnKick", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnKickPlayer;

	[XuiBindComponent("btnSend", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnSend;

	[XuiBindComponent("btnCancel", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnCancel;

	[PublicizedFrom(EAccessModifier.Private)]
	public string reportedPlayerName;

	[PublicizedFrom(EAccessModifier.Private)]
	public int reportedEntityId;

	[PublicizedFrom(EAccessModifier.Private)]
	public string inputErrorMessage;

	[PublicizedFrom(EAccessModifier.Private)]
	public PlayerData reportedPlayerData;
}
