using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_ServerJoinRulesDialog : XUiController
{
	public override void Init()
	{
		base.Init();
		XUiC_ServerJoinRulesDialog.ID = base.WindowGroup.Id;
		XUiController childById = base.GetChildById("labelConfirmationText");
		this.labelConfirmationText = (XUiV_Label)((childById != null) ? childById.ViewComponent : null);
		XUiC_SimpleButton xuiC_SimpleButton = base.GetChildById("btnSpawn") as XUiC_SimpleButton;
		if (xuiC_SimpleButton != null)
		{
			xuiC_SimpleButton.OnPressed += this.BtnSpawn_OnPressed;
		}
		XUiC_SimpleButton xuiC_SimpleButton2 = base.GetChildById("btnLeave") as XUiC_SimpleButton;
		if (xuiC_SimpleButton2 != null)
		{
			xuiC_SimpleButton2.OnPressed += this.BtnLeave_OnPressed;
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.xui.playerUI.CursorController.SetCursorHidden(false);
		base.GetChildById("btnLeave").SelectCursorElement(false, false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnLeave_OnPressed(XUiController _sender, int _mouseButton)
	{
		this.xui.playerUI.windowManager.Close(this.windowGroup, false);
		SingletonMonoBehaviour<ConnectionManager>.Instance.Disconnect();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnSpawn_OnPressed(XUiController _sender, int _mouseButton)
	{
		this.xui.playerUI.windowManager.Close(this.windowGroup, false);
		GameManager.Instance.DoSpawn();
	}

	public static void Show(LocalPlayerUI _playerUi, string _confirmationText)
	{
		XUiC_ServerJoinRulesDialog childByType = _playerUi.xui.FindWindowGroupByName(XUiC_ServerJoinRulesDialog.ID).GetChildByType<XUiC_ServerJoinRulesDialog>();
		if (childByType.labelConfirmationText != null)
		{
			childByType.labelConfirmationText.Text = _confirmationText.Replace("\\n", "\n");
		}
		_playerUi.windowManager.Open(XUiC_ServerJoinRulesDialog.ID, true, true);
	}

	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label labelConfirmationText;
}
