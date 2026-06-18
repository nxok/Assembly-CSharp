using System;

public class GUIWindowNGUI : GUIWindow
{
	public GUIWindowNGUI(EnumNGUIWindow _nguiEnum)
		: base(_nguiEnum.ToStringCached<EnumNGUIWindow>())
	{
		this.nguiEnum = _nguiEnum;
	}

	public override void OnOpen()
	{
		this.playerUI.nguiWindowManager.Show(this.nguiEnum, true);
	}

	public override void OnClose()
	{
		base.OnClose();
		this.playerUI.nguiWindowManager.Show(this.nguiEnum, false);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public EnumNGUIWindow nguiEnum;
}
