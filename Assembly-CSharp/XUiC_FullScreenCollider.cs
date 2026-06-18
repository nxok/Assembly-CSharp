using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_FullScreenCollider : XUiController
{
	public override void Cleanup()
	{
		base.Cleanup();
		XUiC_FullScreenCollider.visibleInstances.Remove(this);
	}

	[XuiBindEvent("OnVisiblity", null)]
	[PublicizedFrom(EAccessModifier.Private)]
	public void visibilityChanged(XUiController _sender, bool _visibleSelf, bool _visibleInScene)
	{
		if (_visibleInScene)
		{
			XUiC_FullScreenCollider.visibleInstances.Add(this);
			return;
		}
		XUiC_FullScreenCollider.visibleInstances.Remove(this);
	}

	public static bool IsBlocked(XUiView _view)
	{
		return XUiC_FullScreenCollider.visibleInstances.Count != 0 && XUiC_FullScreenCollider.visibleInstances.Max.ViewComponent.Depth > _view.Depth + 5;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly SortedSet<XUiC_FullScreenCollider> visibleInstances = new SortedSet<XUiC_FullScreenCollider>(XUiControllerComparerDepth.Instance);
}
