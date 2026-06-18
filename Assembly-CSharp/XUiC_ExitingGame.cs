using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_ExitingGame : XUiController
{
	public override void Init()
	{
		base.Init();
		XUiC_ExitingGame.ID = base.WindowGroup.Id;
	}

	public override void OnOpen()
	{
		base.OnOpen();
		((XUiV_Window)this.viewComponent).ForceVisible(1f);
	}

	public static string ID;
}
