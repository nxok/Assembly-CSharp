using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_WorldGenerationWindowGroup : XUiC_EditingToolsDialogBase
{
	public override void Init()
	{
		base.Init();
		XUiC_WorldGenerationWindowGroup.ID = this.windowGroup.Id;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void close()
	{
		this.window.StartClose();
	}

	public static string ID = "";

	[XuiBindComponent(true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_WorldGenerationWindow window;
}
