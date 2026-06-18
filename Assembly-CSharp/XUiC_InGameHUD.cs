using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_InGameHUD : XUiController
{
	public override void Init()
	{
		base.Init();
		XUiController[] childrenByType = base.GetChildrenByType<XUiC_HUDStatBar>(null);
		this.statBarList = childrenByType;
		this.IsDirty = true;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (this.IsDirty)
		{
			EntityPlayerLocal entityPlayer = this.xui.playerUI.entityPlayer;
			this.IsDirty = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController[] statBarList;
}
