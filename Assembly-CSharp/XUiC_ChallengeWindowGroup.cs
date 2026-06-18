using System;
using GUI_2;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_ChallengeWindowGroup : XUiController
{
	public override void Init()
	{
		base.Init();
		this.descriptionWindow = base.GetChildByType<XUiC_ChallengeEntryDescriptionWindow>();
	}

	public void SetEntry(XUiC_ChallengeEntry je)
	{
		this.descriptionWindow.SetChallenge(je);
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (this.xui.playerUI.playerInput.GUIActions.Inspect.WasPressed)
		{
			this.descriptionWindow.CompleteCurrentChallenege();
		}
		if (this.xui.playerUI.playerInput.GUIActions.HalfStack.WasPressed)
		{
			this.descriptionWindow.TrackCurrentChallenege();
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.xui.playerUI.windowManager.Open("windowpaging", false);
		this.xui.CalloutWindow.ClearCallouts(XUiC_GamepadCalloutWindow.CalloutType.Menu);
		this.xui.CalloutWindow.AddCallout(UIUtils.ButtonIcon.FaceButtonSouth, "igcoSelect", XUiC_GamepadCalloutWindow.CalloutType.Menu);
		this.xui.CalloutWindow.AddCallout(UIUtils.ButtonIcon.FaceButtonEast, "igcoExit", XUiC_GamepadCalloutWindow.CalloutType.Menu);
		this.xui.CalloutWindow.AddCallout(UIUtils.ButtonIcon.FaceButtonWest, "igcoTrack", XUiC_GamepadCalloutWindow.CalloutType.Menu);
		this.xui.CalloutWindow.AddCallout(UIUtils.ButtonIcon.FaceButtonNorth, "igcoComplete", XUiC_GamepadCalloutWindow.CalloutType.Menu);
		this.xui.CalloutWindow.EnableCallouts(XUiC_GamepadCalloutWindow.CalloutType.Menu, 0f);
		XUiC_WindowSelector childByType = this.xui.FindWindowGroupByName("windowpaging").GetChildByType<XUiC_WindowSelector>();
		if (childByType != null)
		{
			childByType.SetSelected("challenges");
		}
	}

	public override void OnClose()
	{
		base.OnClose();
		this.xui.CalloutWindow.DisableCallouts(XUiC_GamepadCalloutWindow.CalloutType.Menu);
		this.xui.playerUI.windowManager.Close("windowpaging");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ChallengeEntryDescriptionWindow descriptionWindow;
}
