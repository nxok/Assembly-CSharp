using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_DiscordInfo : XUiController
{
	public override void Init()
	{
		base.Init();
		XUiC_DiscordInfo.ID = base.WindowGroup.Id;
	}

	[XuiBindEvent("OnPress", "btnFullAccount")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void btnFullAccount_OnPressed(XUiController _sender, int _mouseButton)
	{
		this.closeAndOpenLoginWindow();
		DiscordManager.Instance.AuthManager.LoginDiscordUser();
	}

	[XuiBindEvent("OnPress", "btnOk")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void btnOk_OnPressed(XUiController _sender, int _mouseButton)
	{
		this.closeAndOpenMainMenu();
	}

	[XuiBindEvent("OnPress", "btnNotNow")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void btnNotNow_OnPressed(XUiController _sender, int _mouseButton)
	{
		this.closeAndOpenMainMenu();
	}

	[XuiBindEvent("OnPress", "btnSettings")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void btnSettings_OnPressed(XUiController _sender, int _mouseButton)
	{
		this.xui.playerUI.windowManager.Close(this.windowGroup, false);
		XUiC_OptionsAudio childByType = this.xui.GetChildByType<XUiC_OptionsAudio>();
		if (childByType == null)
		{
			return;
		}
		childByType.OpenAtTab("xuiOptionsAudioDiscord");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void closeAndOpenMainMenu()
	{
		this.xui.playerUI.windowManager.Close(this.windowGroup, false);
		this.xui.playerUI.windowManager.Open(XUiC_MainMenu.ID, true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void closeAndOpenLoginWindow()
	{
		this.xui.playerUI.windowManager.Close(this.windowGroup, false);
		XUiC_DiscordLogin.Open(null, true, false, false, true, false);
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.windowGroup.isEscClosable = false;
		DiscordManager.Instance.Settings.DiscordFirstTimeInfoShown = true;
		DiscordManager.Instance.Settings.Save();
	}

	[XuiXmlBinding("supports_full_accounts")]
	public bool SupportsFullAccounts
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return DiscordManager.SupportsFullAccounts;
		}
	}

	[XuiXmlBinding("supports_provisional_accounts")]
	public bool SupportsProvisionalAccounts
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return DiscordManager.SupportsProvisionalAccounts;
		}
	}

	public static string ID = "";

	[XuiBindComponent("btnFullAccount", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnFullAccount;

	[XuiBindComponent("btnOk", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnOk;

	[XuiBindComponent("btnNotNow", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnNotNow;

	[XuiBindComponent("btnSettings", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnSettings;
}
