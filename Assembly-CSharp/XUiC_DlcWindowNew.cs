using System;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_DlcWindowNew : XUiController
{
	public override void Init()
	{
		base.Init();
		XUiC_DlcWindowNew.ID = base.WindowGroup.Id;
		base.registerForInputStyleChanges();
	}

	[XuiBindEvent("ListEntryClicked", "dlcList")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void entryClicked(XUiC_List<XUiC_DlcList.DlcEntry> _list, XUiC_DlcList.DlcEntry _entry)
	{
		EntitlementManager.Instance.OpenStore(_entry.DlcSet, delegate(EntitlementSetEnum _)
		{
			this.dlcList.RebuildList(false);
		});
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.windowGroup.openWindowOnEsc = XUiC_MainMenu.ID;
		this.updatePagingButtonIcons(PlatformManager.NativePlatform.Input.CurrentInputStyle);
	}

	[XuiBindEvent("OnPress", "btnBack")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnBack_OnPressed(XUiController _sender, int _mouseButton)
	{
		this.xui.playerUI.windowManager.Close(this.windowGroup, true);
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (XUiUtils.HotkeysAllowedFor(this.viewComponent, false))
		{
			XUi.HandlePaging(this.xui, delegate
			{
				this.dlcList.ChangePage(1);
				return true;
			}, delegate
			{
				this.dlcList.ChangePage(-1);
				return true;
			}, false);
			if (this.xui.playerUI.playerInput.GUIActions.Submit.WasReleased && !this.xui.playerUI.CursorController.CursorModeActive)
			{
				int currentBaseIndex = this.dlcList.CurrentBaseIndex;
				int pageLength = this.dlcList.PageLength;
				int num = currentBaseIndex + pageLength / 2;
				XUiC_DlcList.DlcEntry entryByIndex = this.dlcList.GetEntryByIndex(num);
				this.entryClicked(this.dlcList, entryByIndex);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void inputStyleChanged(PlayerInputManager.InputStyle _oldStyle, PlayerInputManager.InputStyle _newStyle)
	{
		this.updatePagingButtonIcons(_newStyle);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updatePagingButtonIcons(PlayerInputManager.InputStyle _inputStyle)
	{
		bool flag = _inputStyle != PlayerInputManager.InputStyle.Keyboard && this.dlcList.AllEntries().Count > this.dlcList.PageLength;
		base.GetChildById("LB_Icon").ViewComponent.IsVisible = (base.GetChildById("RB_Icon").ViewComponent.IsVisible = flag);
	}

	public static string ID = "";

	[XuiBindComponent("btnBack", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnBack;

	[XuiBindComponent(true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_DlcList dlcList;
}
