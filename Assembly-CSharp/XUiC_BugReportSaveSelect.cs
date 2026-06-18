using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_BugReportSaveSelect : XUiController
{
	public override void Init()
	{
		base.Init();
		this.List = base.GetChildByType<XUiC_BugReportSavesList>();
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.rebuildList();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void rebuildList()
	{
		this.List.RebuildList(SaveInfoProvider.Instance.SaveEntryInfos, false);
	}

	public XUiC_BugReportSavesList List;
}
