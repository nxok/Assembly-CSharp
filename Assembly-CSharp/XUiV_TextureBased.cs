using System;
using UnityEngine;

public class XUiV_TextureBased : XUiV_ImageBased
{
	public virtual Texture Texture
	{
		get
		{
			return this.texture;
		}
		set
		{
			this.texture = value;
			base.SetDirty();
		}
	}

	public Material Material
	{
		get
		{
			return this.material;
		}
		set
		{
			this.material = value;
			base.SetDirty();
		}
	}

	public UIDrawCall.OnRenderCallback OnRenderTexture
	{
		get
		{
			return this.uiTexture.onRender;
		}
		set
		{
			this.uiTexture.onRender = value;
		}
	}

	public Rect UVRect
	{
		get
		{
			return this.uvRect;
		}
		set
		{
			this.uvRect = value;
			base.SetDirty();
		}
	}

	public Vector4 Border
	{
		get
		{
			return this.border;
		}
		set
		{
			this.border = value;
			base.SetDirty();
		}
	}

	[XuiXmlAttribute("color", false)]
	public Color Color
	{
		get
		{
			return this.color;
		}
		set
		{
			this.color = value;
			base.SetDirty();
		}
	}

	public UIBasicSprite.FillDirection FillDirection
	{
		get
		{
			return this.fillDirection;
		}
		set
		{
			this.fillDirection = value;
			base.SetDirty();
		}
	}

	[XuiXmlAttribute("sourceaspectratiorespectpivot", false)]
	public bool SourceAspectRatioRespectPivot
	{
		get
		{
			return this.sourceAspectRatioRespectPivot;
		}
		set
		{
			if (this.sourceAspectRatioRespectPivot == value)
			{
				return;
			}
			this.sourceAspectRatioRespectPivot = value;
			base.SetDirty();
		}
	}

	public XUiV_TextureBased(XUi _xui, string _id)
		: base(_xui, _id)
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void createComponents(GameObject _go)
	{
		base.createComponents(_go);
		_go.AddComponent<UITexture>();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void captureComponents()
	{
		base.captureComponents();
		this.widget = (this.uiTexture = this.uiTransform.gameObject.GetComponent<UITexture>());
	}

	public void CreateMaterial(string _shaderName = "Unlit/Transparent Colored Emissive TextureArray")
	{
		Shader shader = GlobalAssets.FindShader(_shaderName);
		if (shader == null)
		{
			Log.Error("Could not find shader " + _shaderName);
			return;
		}
		this.Material = new Material(shader);
		this.isCreatedMaterial = true;
	}

	public override void Cleanup()
	{
		base.Cleanup();
		if (!this.isCreatedMaterial)
		{
			return;
		}
		Object.Destroy(this.material);
		this.material = null;
		this.isCreatedMaterial = false;
	}

	public override void InitView()
	{
		base.InitView();
		this.updateData();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void updateData()
	{
		this.uiTexture.enabled = this.texture != null;
		this.uiTexture.mainTexture = this.texture;
		this.uiTexture.color = base.opacityModColor(this.color);
		this.uiTexture.keepAspectRatio = this.keepAspectRatio;
		this.uiTexture.aspectRatio = this.aspectRatio;
		this.uiTexture.fixedAspect = this.keepSourceAspectRatio;
		this.uiTexture.fixedAspectRespectPivot = this.sourceAspectRatioRespectPivot;
		this.uiTexture.SetDimensions(this.size.x, this.size.y);
		this.uiTexture.type = this.type;
		this.uiTexture.border = this.border;
		this.uiTexture.uvRect = this.uvRect;
		this.uiTexture.flip = this.flip;
		this.uiTexture.centerType = (this.fillCenter ? 1 : 0);
		this.uiTexture.fillDirection = this.fillDirection;
		this.uiTexture.material = this.material;
		base.updateData();
	}

	public void SetTextureDirty()
	{
		this.uiTexture.mainTexture = null;
		base.SetDirty();
	}

	public virtual void UnloadTexture()
	{
		if (this.Texture == null)
		{
			return;
		}
		this.uiTexture.mainTexture = null;
		this.texture = null;
		base.SetDirty();
	}

	[XuiXmlAttribute("material", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public void attributeMaterial(string _value)
	{
		this.xui.LoadData<Material>(_value, delegate(Material _o)
		{
			this.material = new Material(_o);
		});
	}

	[XuiXmlAttribute("rect_offset", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public void attributeRectOffset(Vector2 _value)
	{
		Rect uvrect = this.UVRect;
		uvrect.x = _value.x;
		uvrect.y = _value.y;
		this.UVRect = uvrect;
	}

	[XuiXmlAttribute("rect_size", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public void attributeRectSize(Vector2 _value)
	{
		Rect uvrect = this.UVRect;
		uvrect.width = _value.x;
		uvrect.height = _value.y;
		this.UVRect = uvrect;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public UITexture uiTexture;

	[PublicizedFrom(EAccessModifier.Private)]
	public Texture texture;

	[PublicizedFrom(EAccessModifier.Private)]
	public Material material;

	[PublicizedFrom(EAccessModifier.Private)]
	public Rect uvRect = new Rect(0f, 0f, 1f, 1f);

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector4 border = Vector4.zero;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color color = Color.white;

	[PublicizedFrom(EAccessModifier.Private)]
	public UIBasicSprite.FillDirection fillDirection;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isCreatedMaterial;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool sourceAspectRatioRespectPivot;
}
