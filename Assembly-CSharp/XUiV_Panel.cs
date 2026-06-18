using System;
using UnityEngine;

public class XUiV_Panel : XUiView
{
	public override UIRect UiRect
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return this.panel;
		}
	}

	[XuiXmlAttribute("clipping", false)]
	public UIDrawCall.Clipping Clipping
	{
		get
		{
			return this.clipping;
		}
		set
		{
			if (value == this.clipping)
			{
				return;
			}
			this.clipping = value;
			base.SetDirty();
		}
	}

	[XuiXmlAttribute("clippingsize", false)]
	public Vector2 ClippingSize
	{
		get
		{
			return this.clippingSize;
		}
		set
		{
			if (value == this.clippingSize)
			{
				return;
			}
			this.clippingSize = value;
			base.SetDirty();
		}
	}

	[XuiXmlAttribute("clippingcenter", false)]
	public Vector2 ClippingCenter
	{
		get
		{
			return this.clippingCenter;
		}
		set
		{
			if (value == this.clippingCenter)
			{
				return;
			}
			this.clippingCenter = value;
			base.SetDirty();
		}
	}

	[XuiXmlAttribute("clippingsoftness", false)]
	public Vector2 ClippingSoftness
	{
		get
		{
			return this.clippingSoftness;
		}
		set
		{
			if (value == this.clippingSoftness)
			{
				return;
			}
			this.clippingSoftness = value;
			base.SetDirty();
		}
	}

	[XuiXmlAttribute("clippingoffset", false)]
	public Vector2 ClipOffset
	{
		get
		{
			return this.clippingOffset;
		}
		set
		{
			if (value == this.clippingOffset)
			{
				return;
			}
			this.clippingOffset = value;
			base.SetDirty();
		}
	}

	public bool StaticWidgets
	{
		get
		{
			return this.panel.widgetsAreStatic;
		}
		set
		{
			this.panel.widgetsAreStatic = value;
		}
	}

	public override Vector3[] WorldCorners
	{
		get
		{
			return this.panel.worldCorners;
		}
	}

	public XUiV_Panel(XUi _xui, string _id)
		: base(_xui, _id)
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void createComponents(GameObject _go)
	{
		_go.AddComponent<UIPanel>();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void captureComponents()
	{
		base.captureComponents();
		this.panel = this.uiTransform.gameObject.GetComponent<UIPanel>();
	}

	public override void InitView()
	{
		base.InitView();
		this.panel.depth = this.depth;
		this.updateClipping();
		base.SetDirty();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void updateData()
	{
		base.updateData();
		this.updateClipping();
		this.refreshBoxCollider();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateClipping()
	{
		if (this.clipping == null)
		{
			return;
		}
		this.panel.clipping = this.clipping;
		Vector2 zero = this.clippingOffset;
		Vector2 vector = this.clippingCenter;
		Vector2 vector2 = this.clippingSize;
		Vector2 vector3 = this.clippingSoftness;
		if (zero == new Vector2(-10000f, -10000f))
		{
			zero = Vector2.zero;
		}
		if (vector == new Vector2(-10000f, -10000f))
		{
			vector..ctor((float)this.size.x / 2f, (float)(-(float)this.size.y) / 2f);
		}
		if (vector2 == new Vector2(-10000f, -10000f))
		{
			vector2..ctor((float)this.size.x, (float)this.size.y);
		}
		if (vector2.x < 0f)
		{
			vector2.x = 0f;
		}
		if (vector2.y < 0f)
		{
			vector2.y = 0f;
		}
		this.panel.clipSoftness = vector3;
		this.panel.baseClipRegion = new Vector4(vector.x, vector.y, vector2.x, vector2.y);
		this.panel.clipOffset = zero;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public UIPanel panel;

	[PublicizedFrom(EAccessModifier.Private)]
	public UIDrawCall.Clipping clipping;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2 clippingOffset = new Vector2(-10000f, -10000f);

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2 clippingSize = new Vector2(-10000f, -10000f);

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2 clippingCenter = new Vector2(-10000f, -10000f);

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2 clippingSoftness;
}
