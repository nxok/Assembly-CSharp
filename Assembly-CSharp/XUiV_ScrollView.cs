using System;
using System.Collections.Generic;
using UnityEngine;

public class XUiV_ScrollView : XUiV_Panel
{
	[XuiXmlAttribute("movement", false)]
	public UIScrollView.Movement Movement
	{
		get
		{
			return this.movement;
		}
		set
		{
			if (value == this.movement)
			{
				return;
			}
			this.movement = value;
			base.SetDirty();
		}
	}

	[XuiXmlAttribute("drageffect", false)]
	public UIScrollView.DragEffect DragEffect
	{
		get
		{
			return this.dragEffect;
		}
		set
		{
			if (value == this.dragEffect)
			{
				return;
			}
			this.dragEffect = value;
			base.SetDirty();
		}
	}

	[XuiXmlAttribute("scrollfactor", false)]
	public float ScrollFactor
	{
		get
		{
			return this.scrollFactor;
		}
		set
		{
			if (Mathf.Approximately(value, this.scrollFactor))
			{
				return;
			}
			this.scrollFactor = value;
			base.SetDirty();
		}
	}

	[XuiXmlAttribute("scrollwithoutfocus", false)]
	public bool ScrollWithoutFocus
	{
		get
		{
			return this.scrollWithoutFocus;
		}
		set
		{
			if (value == this.scrollWithoutFocus)
			{
				return;
			}
			this.scrollWithoutFocus = value;
			base.SetDirty();
		}
	}

	public XUiV_ScrollView(XUi _xui, string _id)
		: base(_xui, _id)
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void createComponents(GameObject _go)
	{
		base.createComponents(_go);
		_go.AddComponent<UIScrollView>();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void captureComponents()
	{
		base.captureComponents();
		this.scrollView = this.uiTransform.gameObject.GetComponent<UIScrollView>();
	}

	public override void InitView()
	{
		base.InitView();
		this.controller.Parent.OnScroll += this.OnScrolled;
		this.xui.OnBuilt += this.OnXuiLoadDone;
		base.SetDirty();
	}

	public override void Cleanup()
	{
		base.Cleanup();
		this.xui.OnBuilt -= this.OnXuiLoadDone;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnXuiLoadDone()
	{
		this.xui.OnBuilt -= this.OnXuiLoadDone;
		this.applyScrollEventToChildren(this.controller);
		this.controller.Parent.TryGetChildView<XUiV_ScrollBar>("", out this.scrollBar);
		XUiV_ScrollBar xuiV_ScrollBar = this.scrollBar;
		if (xuiV_ScrollBar == null)
		{
			return;
		}
		xuiV_ScrollBar.Connect(new XUiEvent_OnScrollEventHandler(this.OnScrolled));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void applyScrollEventToChildren(XUiController _controller)
	{
		foreach (XUiController xuiController in _controller.Children)
		{
			this.applyScrollEventToChildren(xuiController);
			xuiController.ViewComponent.EventOnScroll = true;
			xuiController.OnScroll += this.OnScrolled;
		}
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		this.controllerScroll(Time.unscaledDeltaTime);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void controllerScroll(float _dt)
	{
		if (!XUiUtils.HotkeysAllowedFor(this.controller.Parent.ViewComponent, false))
		{
			return;
		}
		XUiView currentTarget = this.xui.playerUI.CursorController.CurrentTarget;
		if (!this.ScrollWithoutFocus && (currentTarget == null || !currentTarget.Controller.IsSelfOrChildOf(this.controller.Parent)))
		{
			return;
		}
		Vector2 vector = this.xui.playerUI.playerInput.GUIActions.Camera.Vector;
		float num = Mathf.Abs(vector.x);
		float num2 = Mathf.Abs(vector.y);
		if (this.movement == 1)
		{
			if ((double)num2 < (double)num * 1.8)
			{
				return;
			}
			if ((double)num2 > 0.1)
			{
				this.scrollView.Scroll(vector.y * _dt);
				return;
			}
		}
		else
		{
			if ((double)num < (double)num2 * 1.8)
			{
				return;
			}
			if ((double)num > 0.1)
			{
				this.scrollView.Scroll(vector.x * _dt);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void updateData()
	{
		base.Clipping = 3;
		this.scrollView.movement = this.movement;
		this.scrollView.dragEffect = this.dragEffect;
		this.scrollView.scrollWheelFactor = this.scrollFactor;
		this.scrollView.constrainOnDrag = true;
		this.scrollView.disableDragIfFits = true;
		this.scrollView.smoothDragStart = true;
		XUiV_ScrollBar xuiV_ScrollBar = this.scrollBar;
		UIScrollBar uiscrollBar = ((xuiV_ScrollBar != null) ? xuiV_ScrollBar.ScrollBar : null);
		if (uiscrollBar != null)
		{
			uiscrollBar.fillDirection = ((this.movement == null) ? 0 : 3);
			UIScrollView.Movement movement = this.movement;
			if (movement != null)
			{
				if (movement == 1)
				{
					this.scrollView.horizontalScrollBar = null;
					this.scrollView.verticalScrollBar = uiscrollBar;
				}
			}
			else
			{
				this.scrollView.horizontalScrollBar = uiscrollBar;
				this.scrollView.verticalScrollBar = null;
			}
			this.scrollView.CheckScrollbars();
		}
		base.updateData();
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.ResetPosition();
	}

	public override void OnVisibilityChanged(bool _visibleInScene)
	{
		base.OnVisibilityChanged(_visibleInScene);
		if (_visibleInScene)
		{
			this.UpdatePosition();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnScrolled(XUiController _sender, float _delta)
	{
		if (InputUtils.ShiftKeyPressed)
		{
			return;
		}
		this.scrollView.Scroll(_delta);
	}

	public void ResetPosition()
	{
		this.scrollView.ResetPosition();
	}

	public void UpdatePosition()
	{
		this.scrollView.UpdatePosition();
	}

	public void MakeVisible(XUiView _viewComponent)
	{
		if (_viewComponent == null)
		{
			return;
		}
		Vector3[] worldCorners = _viewComponent.WorldCorners;
		this.MakeVisible(worldCorners);
	}

	public void MakeVisible(IList<Vector3> _worldCorners)
	{
		if (_worldCorners == null || _worldCorners.Count < 4)
		{
			return;
		}
		Transform cachedTransform = this.panel.cachedTransform;
		Vector3[] worldCorners = this.panel.worldCorners;
		Vector3 vector = cachedTransform.InverseTransformPoint(worldCorners[1]);
		Vector3 vector2 = cachedTransform.InverseTransformPoint(worldCorners[3]);
		Vector3 vector3 = cachedTransform.InverseTransformPoint(_worldCorners[1]);
		Vector3 vector4 = cachedTransform.InverseTransformPoint(_worldCorners[3]);
		Vector3 zero = Vector3.zero;
		if (!this.scrollView.canMoveHorizontally)
		{
			zero.x = 0f;
		}
		else if (vector3.x < vector.x)
		{
			zero.x = vector.x - vector3.x + base.ClippingSoftness.x;
		}
		else if (vector4.x > vector2.x)
		{
			zero.x = vector2.x - vector4.x - base.ClippingSoftness.x;
		}
		if (!this.scrollView.canMoveVertically)
		{
			zero.y = 0f;
		}
		else if (vector3.y > vector.y)
		{
			zero.y = vector.y - vector3.y - base.ClippingSoftness.y;
		}
		else if (vector4.y < vector2.y)
		{
			zero.y = vector2.y - vector4.y + base.ClippingSoftness.y;
		}
		zero.z = 0f;
		cachedTransform.localPosition += zero;
		Vector2 clipOffset = this.panel.clipOffset;
		clipOffset.x -= zero.x;
		clipOffset.y -= zero.y;
		this.panel.clipOffset = clipOffset;
	}

	public void CenterOn(XUiView _target)
	{
		if (_target == null)
		{
			return;
		}
		Vector3[] worldCorners = this.panel.worldCorners;
		Vector3 vector = (worldCorners[2] + worldCorners[0]) * 0.5f;
		Transform cachedTransform = this.panel.cachedTransform;
		Vector3 vector2 = cachedTransform.InverseTransformPoint(_target.UiTransform.position);
		Vector3 vector3 = cachedTransform.InverseTransformPoint(vector);
		Vector3 vector4 = vector2 - vector3;
		if (!this.scrollView.canMoveHorizontally)
		{
			vector4.x = 0f;
		}
		if (!this.scrollView.canMoveVertically)
		{
			vector4.y = 0f;
		}
		vector4.z = 0f;
		cachedTransform.localPosition -= vector4;
		Vector2 clipOffset = this.panel.clipOffset;
		clipOffset.x += vector4.x;
		clipOffset.y += vector4.y;
		this.panel.clipOffset = clipOffset;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public UIScrollView scrollView;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_ScrollBar scrollBar;

	[PublicizedFrom(EAccessModifier.Private)]
	public UIScrollView.Movement movement = 1;

	[PublicizedFrom(EAccessModifier.Private)]
	public UIScrollView.DragEffect dragEffect;

	[PublicizedFrom(EAccessModifier.Private)]
	public float scrollFactor = 2f;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool scrollWithoutFocus;
}
