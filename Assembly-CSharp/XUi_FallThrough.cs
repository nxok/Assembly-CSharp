using System;
using GUI_2;
using UnityEngine;

public class XUi_FallThrough : MonoBehaviour
{
	public void SetXUi(XUi _xui)
	{
		this.xui = _xui;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Start()
	{
		UICamera.fallThrough = base.gameObject;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDestroy()
	{
		if (UICamera.fallThrough == base.gameObject)
		{
			UICamera.fallThrough = null;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		bool flag = this.xui.DragAndDropWindow != null && !this.xui.DragAndDropWindow.CurrentStack.IsEmpty();
		bool flag2 = flag && UICamera.hoveredObject == this.xui.gameObject && this.xui.DragAndDropWindow.CurrentStack.itemValue.ItemClassOrMissing.CanDrop(null);
		int num = ((this.xui.DragAndDropWindow == null) ? 0 : this.xui.DragAndDropWindow.CurrentStack.count);
		bool flag3 = false;
		LocalPlayerUI playerUI = this.xui.playerUI;
		if (flag2 && null != playerUI && null != playerUI.uiCamera)
		{
			PlayerActionsLocal playerInput = playerUI.playerInput;
			if (((playerInput != null) ? playerInput.GUIActions : null) != null)
			{
				PlayerActionsGUI guiactions = playerUI.playerInput.GUIActions;
				bool flag4 = false;
				bool flag5 = false;
				if (guiactions.LastInputType == 1)
				{
					flag4 |= guiactions.Submit.WasReleased;
					flag5 |= guiactions.HalfStack.WasReleased;
				}
				else
				{
					flag4 |= playerUI.CursorController.GetMouseButtonDown(0);
					flag5 |= playerUI.CursorController.GetMouseButtonDown(1);
				}
				if (flag4 || (num == 1 && flag5))
				{
					this.xui.DragAndDropWindow.DropCurrentItem();
					flag3 = true;
				}
				else if (num > 1 && flag5)
				{
					this.xui.DragAndDropWindow.DropCurrentItem(1);
					num--;
					flag3 = true;
				}
			}
		}
		if (flag2 != this.canDrop || flag3)
		{
			this.canDrop = flag2;
			this.xui.CalloutWindow.ClearCallouts(XUiC_GamepadCalloutWindow.CalloutType.MenuHoverAir);
			if (flag && this.canDrop)
			{
				if (num > 1)
				{
					this.xui.CalloutWindow.AddCallout(UIUtils.ButtonIcon.FaceButtonSouth, "igcoDropAll", XUiC_GamepadCalloutWindow.CalloutType.MenuHoverAir);
					this.xui.CalloutWindow.AddCallout(UIUtils.ButtonIcon.FaceButtonWest, "igcoDropOne", XUiC_GamepadCalloutWindow.CalloutType.MenuHoverAir);
				}
				else
				{
					this.xui.CalloutWindow.AddCallout(UIUtils.ButtonIcon.FaceButtonSouth, "igcoDrop", XUiC_GamepadCalloutWindow.CalloutType.MenuHoverAir);
				}
				this.xui.CalloutWindow.DisableCallouts(XUiC_GamepadCalloutWindow.CalloutType.MenuHoverItem);
				this.xui.CalloutWindow.EnableCallouts(XUiC_GamepadCalloutWindow.CalloutType.MenuHoverAir, 0f);
				return;
			}
			this.xui.CalloutWindow.DisableCallouts(XUiC_GamepadCalloutWindow.CalloutType.MenuHoverAir);
			this.xui.CalloutWindow.EnableCallouts(XUiC_GamepadCalloutWindow.CalloutType.MenuHoverItem, 0f);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public XUi xui;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool canDrop;
}
