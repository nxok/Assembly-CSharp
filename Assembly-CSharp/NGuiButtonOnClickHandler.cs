using System;
using UnityEngine;

public class NGuiButtonOnClickHandler : MonoBehaviour
{
	public virtual void OnClick()
	{
		if (this.OnClickDelegate != null)
		{
			this.OnClickDelegate.NGuiButtonOnClick(base.transform);
		}
	}

	public virtual void OnDoubleClick()
	{
		if (this.OnDoubleClickDelegate != null)
		{
			this.OnDoubleClickDelegate.NGuiButtonOnDoubleClick(base.transform);
		}
	}

	public void OnHover(bool _isOver)
	{
		if (this.OnHoverDelegate != null)
		{
			this.OnHoverDelegate.NGuiButtonOnHover(base.transform, _isOver);
		}
	}

	public void OnIsHeld()
	{
		if (this.OnIsHeldDelegate != null)
		{
			this.OnIsHeldDelegate.NGuiButtonOnIsHeld(base.transform);
		}
	}

	public INGuiButtonOnClick OnClickDelegate;

	public INGuiButtonOnDoubleClick OnDoubleClickDelegate;

	public INGuiButtonOnHover OnHoverDelegate;

	public INGuiButtonOnIsHeld OnIsHeldDelegate;
}
