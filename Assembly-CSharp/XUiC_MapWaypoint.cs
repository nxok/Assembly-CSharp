using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_MapWaypoint : XUiController
{
	public override void Init()
	{
		base.Init();
		this.waypointList = (XUiC_MapWaypointList)base.Parent.GetChildById("waypointList");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleWaypointSetPressed(XUiController _sender, EventArgs _e)
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_MapWaypointList waypointList;
}
