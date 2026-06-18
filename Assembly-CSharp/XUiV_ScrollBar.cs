using System;
using UnityEngine;

public class XUiV_ScrollBar : XUiView
{
	public override UIRect UiRect
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return null;
		}
	}

	public UIScrollBar ScrollBar
	{
		get
		{
			return this.scrollBar;
		}
	}

	public float ScrollPosition
	{
		get
		{
			return this.scrollBar.value;
		}
		set
		{
			this.scrollBar.value = value;
			base.SetDirty();
		}
	}

	public override Vector3[] WorldCorners
	{
		get
		{
			if (this.background != null)
			{
				return this.background.WorldCorners;
			}
			XUiV_Sprite xuiV_Sprite = this.thumb;
			if (xuiV_Sprite == null)
			{
				return null;
			}
			return xuiV_Sprite.WorldCorners;
		}
	}

	public XUiV_ScrollBar(XUi _xui, string _id)
		: base(_xui, _id)
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void createComponents(GameObject _go)
	{
		base.createComponents(_go);
		_go.AddComponent<UIScrollBar>();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void captureComponents()
	{
		base.captureComponents();
		this.scrollBar = this.uiTransform.gameObject.GetComponent<UIScrollBar>();
		this.scrollBar.value = 0f;
	}

	public override void InitView()
	{
		base.InitView();
		if (!base.Controller.TryGetChildView<XUiV_Sprite>("thumb", out this.thumb))
		{
			Log.Error("[XUi] XUiV_ScrollBar without child sprite 'thumb'. Hierarchy: " + base.GetXuiHierarchy());
			return;
		}
		if (!this.thumb.HasAnyEvent)
		{
			Log.Error("[XUi] XUiV_ScrollBar child sprite 'thumb' has no events enabled. Hierarchy: " + base.GetXuiHierarchy());
			return;
		}
		if (base.Controller.TryGetChildView<XUiV_Sprite>("background", out this.background) && !this.background.HasAnyEvent)
		{
			Log.Error("[XUi] XUiV_ScrollBar child sprite 'background' has no events enabled. Hierarchy: " + base.GetXuiHierarchy());
			return;
		}
		base.SetDirty();
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.scrollBar.alpha = 0f;
		if (this.thumb != null)
		{
			Color color = this.thumb.Color;
			color.a = 0f;
			this.thumb.Color = color;
		}
		if (this.background != null)
		{
			Color color2 = this.background.Color;
			color2.a = 0f;
			this.background.Color = color2;
		}
		this.onOpenValue = this.scrollBar.value;
		this.openFrame = Time.frameCount;
	}

	public override void OnVisibilityChanged(bool _visibleInScene)
	{
		base.OnVisibilityChanged(_visibleInScene);
		if (_visibleInScene)
		{
			this.onOpenValue = this.scrollBar.value;
			this.openFrame = Time.frameCount;
		}
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (Time.frameCount <= this.openFrame + 1)
		{
			float value = this.scrollBar.value;
			if (!Mathf.Approximately(this.onOpenValue, value))
			{
				this.scrollBar.value = this.onOpenValue;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void updateData()
	{
		this.refreshBoxCollider();
		base.updateData();
	}

	public void Connect(XUiEvent_OnScrollEventHandler _onScrolled)
	{
		this.scrollBar.foregroundWidget = this.thumb.Sprite;
		this.thumb.Controller.OnScroll += _onScrolled;
		if (this.background != null)
		{
			this.scrollBar.backgroundWidget = this.background.Sprite;
			this.background.Controller.OnScroll += _onScrolled;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public UIScrollBar scrollBar;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite thumb;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite background;

	[PublicizedFrom(EAccessModifier.Private)]
	public float onOpenValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public int openFrame;
}
