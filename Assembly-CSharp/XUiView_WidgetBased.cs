using System;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;

public abstract class XUiView_WidgetBased : XUiView
{
	public override UIRect UiRect
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return this.widget;
		}
	}

	public override Vector3 LocalCenter
	{
		get
		{
			return this.widget.localCenter;
		}
	}

	public override Vector3[] WorldCorners
	{
		get
		{
			return this.widget.worldCorners;
		}
	}

	[XuiXmlAttribute("keep_aspect_ratio", false)]
	public UIWidget.AspectRatioSource KeepAspectRatio
	{
		get
		{
			return this.keepAspectRatio;
		}
		set
		{
			if (this.keepAspectRatio == value)
			{
				return;
			}
			this.keepAspectRatio = value;
			base.SetDirty();
		}
	}

	[XuiXmlAttribute("aspect_ratio", false)]
	public float AspectRatio
	{
		get
		{
			return this.aspectRatio;
		}
		set
		{
			if (Mathf.Approximately(this.aspectRatio, value))
			{
				return;
			}
			this.aspectRatio = value;
			base.SetDirty();
		}
	}

	[XuiXmlAttribute("pivot", false)]
	public UIWidget.Pivot Pivot
	{
		get
		{
			return this.pivot;
		}
		set
		{
			if (this.pivot == value)
			{
				return;
			}
			this.pivot = value;
			base.SetDirty();
		}
	}

	[XuiXmlAttribute("auto_resize_collider", false)]
	public bool AutoResizeCollider
	{
		get
		{
			return this.autoResizeCollider;
		}
		set
		{
			if (this.autoResizeCollider == value)
			{
				return;
			}
			this.autoResizeCollider = value;
			base.SetDirty();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiView_WidgetBased(XUi _xui, string _id)
		: base(_xui, _id)
	{
	}

	public override void InitView()
	{
		base.InitView();
		this.widget.pivot = this.pivot;
		this.widget.depth = this.depth;
		this.uiTransform.localScale = Vector3.one;
		this.uiTransform.localPosition = new Vector3((float)base.PaddedPosition.x, (float)base.PaddedPosition.y, 0f);
	}

	public override void SetDefaults(XUiController _parent)
	{
		this.Pivot = 0;
		base.SetDefaults(_parent);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void refreshBoxCollider()
	{
		if (this.AutoResizeCollider || this.widget.isAnchored)
		{
			this.widget.autoResizeBoxCollider = true;
			return;
		}
		this.collider.center = this.widget.localCenter;
		this.collider.size = new Vector3(this.widget.localSize.x * base.ColliderScale + (float)(2 * base.ColliderPadding.x), this.widget.localSize.y * base.ColliderScale + (float)(2 * base.ColliderPadding.y), 0f);
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (this.anchoredLeftAndRight)
		{
			this.size.x = Mathf.RoundToInt(this.widget.localSize.x);
		}
		if (this.anchoredTopAndBottom)
		{
			this.size.y = Mathf.RoundToInt(this.widget.localSize.y);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void updateData()
	{
		this.widget.keepAspectRatio = this.keepAspectRatio;
		this.widget.aspectRatio = this.aspectRatio;
		base.updateData();
		this.refreshBoxCollider();
	}

	public virtual bool anchorsKeepSize
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return true;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void anchorsParsed()
	{
		base.anchorsParsed();
		if (this.anchorsKeepSize)
		{
			if (!this.anchoredLeftAndRight && this.widget.width != this.size.x)
			{
				this.widget.width = this.size.x;
			}
			if (!this.anchoredTopAndBottom && this.widget.height != this.size.y)
			{
				this.widget.height = this.size.y;
			}
		}
		ThreadManager.StartCoroutine(this.<anchorsParsed>g__MarkAsChangedLater|31_0());
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator <anchorsParsed>g__MarkAsChangedLater|31_0()
	{
		yield return new WaitForEndOfFrame();
		this.widget.MarkAsChanged();
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public UIWidget widget;

	[PublicizedFrom(EAccessModifier.Protected)]
	public UIWidget.AspectRatioSource keepAspectRatio;

	[PublicizedFrom(EAccessModifier.Protected)]
	public float aspectRatio;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool autoResizeCollider;

	[PublicizedFrom(EAccessModifier.Protected)]
	public UIWidget.Pivot pivot;
}
