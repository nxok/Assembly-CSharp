using System;
using UnityEngine;

public class XUiV_Rect : XUiView_WidgetBased
{
	public XUiV_Rect(XUi _xui, string _id)
		: base(_xui, _id)
	{
	}

	[XuiXmlAttribute("disablefallthrough", false)]
	public bool DisableFallthrough
	{
		get
		{
			return this.disableFallthrough;
		}
		set
		{
			this.disableFallthrough = value;
		}
	}

	public override bool HasAnyEvent
	{
		get
		{
			return this.disableFallthrough || base.HasAnyEvent;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void createComponents(GameObject _go)
	{
		_go.AddComponent<UIWidget>();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void captureComponents()
	{
		base.captureComponents();
		this.widget = this.uiTransform.gameObject.GetComponent<UIWidget>();
	}

	public override void InitView()
	{
		base.InitView();
		UIWidget widget = this.widget;
		widget.onChange = (UIWidget.OnDimensionsChanged)Delegate.Combine(widget.onChange, new UIWidget.OnDimensionsChanged(base.SetDirty));
		this.updateData();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void refreshBoxCollider()
	{
		base.refreshBoxCollider();
		if (!this.disableFallthrough)
		{
			return;
		}
		Vector3 center = this.collider.center;
		center.z = 100f;
		this.collider.center = center;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool disableFallthrough;
}
