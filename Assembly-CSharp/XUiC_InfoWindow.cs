using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_InfoWindow : XUiController
{
	public virtual void Deselect()
	{
	}

	public override void Init()
	{
		base.Init();
		base.OnVisiblity += this.OnVisibilityChanged;
	}

	public override void Cleanup()
	{
		base.Cleanup();
		base.OnVisiblity -= this.OnVisibilityChanged;
	}

	public void OnVisibilityChanged(XUiController _sender, bool _visibleSelf, bool _visibleInScene)
	{
		bool flag = this.windowGroup == null || this.windowGroup.isShowing;
		if (_visibleInScene)
		{
			List<XUiC_InfoWindow> childrenByType = this.xui.GetChildrenByType<XUiC_InfoWindow>();
			for (int i = 0; i < childrenByType.Count; i++)
			{
				if (childrenByType[i] != this && (flag || !childrenByType[i].windowGroup.isShowing))
				{
					childrenByType[i].ViewComponent.IsVisible = false;
				}
			}
			return;
		}
		this.Deselect();
	}
}
