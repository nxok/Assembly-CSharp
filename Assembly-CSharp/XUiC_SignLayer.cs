using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_SignLayer : XUiC_SignGridEntry
{
	public GlobalSignId signId
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public SignData.SignLayer layer
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public int index
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public override bool IsSelectable
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			GlobalSignId signId = this.signId;
			return this.signId.IsValid || this.IsPlaceholder;
		}
	}

	public override Color32 BackgroundColor
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			if (this.parentGrind == null || !this.parentGrind.MultiSelectedLayerIndices.Contains(this.index))
			{
				return XUiC_SignGridEntry.backgroundColor;
			}
			return this.multiSelectColor;
		}
	}

	public bool LayerValid
	{
		get
		{
			return this.layerValid;
		}
	}

	public bool IsPlaceholder
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public float LeftInsertX
	{
		get
		{
			return this.leftInsertPoint.ViewComponent.UiTransform.position.x;
		}
	}

	public float RightInsertX
	{
		get
		{
			return this.rightInsertPoint.ViewComponent.UiTransform.position.x;
		}
	}

	public override void Init()
	{
		base.Init();
		this.signMaterial = base.GetChildById("signMaterial").ViewComponent as XUiV_Texture;
		this.parentGrind = base.GetParentByType<XUiC_SignLayerGrid>();
		this.editorWindow = base.GetParentByType<XUiC_SignEditorWindow>();
		this.signMaterial.CreateMaterial("Game/SignTech/UI");
		XUiV_Texture xuiV_Texture = this.signMaterial;
		xuiV_Texture.OnRenderTexture = (UIDrawCall.OnRenderCallback)Delegate.Combine(xuiV_Texture.OnRenderTexture, new UIDrawCall.OnRenderCallback(this.OnWillRender));
		this.signMaterial.Texture = Texture2D.whiteTexture;
		XUiC_SignEditorWindow xuiC_SignEditorWindow = this.editorWindow;
		xuiC_SignEditorWindow.OnRefreshed = (Action)Delegate.Combine(xuiC_SignEditorWindow.OnRefreshed, new Action(this.RefreshLayerValidity));
		this.rightInsertPoint = base.GetChildById("rightInsertPoint");
		this.leftInsertPoint = base.GetChildById("leftInsertPoint");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RefreshLayerValidity()
	{
		this.layerValid = this.<RefreshLayerValidity>g__IsLayerValid|36_0(this.layer);
	}

	public void SetAsPlaceholder(GlobalSignId signId, int index)
	{
		this.IsPlaceholder = true;
		this.signId = signId;
		this.layer = null;
		this.index = index;
		this.signMaterial.IsVisible = signId.IsValid;
		base.ViewComponent.Enabled = signId.IsValid;
		this.viewComponent.ToolTip = Localization.Get("lblSignAddLayer", false, null);
		base.RefreshBindings();
	}

	public void SetLayer(GlobalSignId signId, SignData.SignLayer layer, int index)
	{
		this.IsPlaceholder = false;
		this.signId = signId;
		this.layer = layer;
		this.index = index;
		this.signMaterial.IsVisible = signId.IsValid;
		base.ViewComponent.Enabled = signId.IsValid;
		this.RefreshTooltip();
		base.RefreshBindings();
		this.RefreshLayerValidity();
	}

	public void RefreshTooltip()
	{
		if (!this.signId.IsValid)
		{
			this.viewComponent.ToolTip = string.Empty;
			return;
		}
		if (string.IsNullOrEmpty(this.layer.name))
		{
			this.viewComponent.ToolTip = string.Format("{0} {1}", Localization.Get("xuiPaintLayer", false, null), (this.index + 1).ToString("00"));
			return;
		}
		this.viewComponent.ToolTip = this.layer.name;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void HandleClick()
	{
		if (this.IsPlaceholder)
		{
			return;
		}
		Action<XUiC_SignLayer> onClicked = this.OnClicked;
		if (onClicked == null)
		{
			return;
		}
		onClicked(this);
	}

	public override void OnCursorSelected(bool _isActualElement)
	{
		base.OnCursorSelected(_isActualElement);
		Action<XUiC_SignLayer> onBecameCursorSelected = this.OnBecameCursorSelected;
		if (onBecameCursorSelected == null)
		{
			return;
		}
		onBecameCursorSelected(this);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnWillRender(Material mat)
	{
		if (this.IsPlaceholder)
		{
			SignDataManager.Instance.TryApplyRenderingData(this.signId, 1f, mat, null, SignUIStyle.AddLayer);
			return;
		}
		if (this.layer != null)
		{
			bool flag = this.layer.renderSettings.mode == SignData.SignRenderSettings.Mode.ColorOnly || this.layer.renderSettings.mode == SignData.SignRenderSettings.Mode.ColorAndMask;
			SignUIStyle signUIStyle;
			if (this.layerValid)
			{
				signUIStyle = (flag ? SignUIStyle.LayerThumbnail : SignUIStyle.MaskThumbnail);
			}
			else
			{
				signUIStyle = (flag ? SignUIStyle.LayerThumbnailInvalid : SignUIStyle.MaskThumbnailInvalid);
			}
			SignDataManager.Instance.TryApplyRenderingData(this.signId, 1f, mat, this.layer, signUIStyle);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetItemNameText(string name)
	{
		this.viewComponent.ToolTip = name;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string value, string bindingName)
	{
		if (base.GetBindingValueInternal(ref value, bindingName))
		{
			return true;
		}
		if (bindingName == "layernumber")
		{
			value = (this.signId.IsValid ? (this.index + 1).ToString("00") : string.Empty);
			return true;
		}
		if (!(bindingName == "maskicon"))
		{
			if (!(bindingName == "showmaskicon"))
			{
				return false;
			}
			if (!this.signId.IsValid || this.layer == null)
			{
				value = false.ToString();
				return true;
			}
			value = (this.layer.renderSettings.mode > SignData.SignRenderSettings.Mode.ColorOnly).ToString();
			return true;
		}
		else
		{
			if (!this.signId.IsValid || this.layer == null)
			{
				value = "ui_game_symbol_lock";
				return true;
			}
			switch (this.layer.renderSettings.mode)
			{
			case SignData.SignRenderSettings.Mode.ColorOnly:
				value = "ui_game_symbol_lock";
				return true;
			case SignData.SignRenderSettings.Mode.ColorAndMask:
				value = "ui_game_symbol_signs_color_and_mask";
				return true;
			case SignData.SignRenderSettings.Mode.MaskOnly:
				value = "ui_game_symbol_signs_mask_only";
				return true;
			case SignData.SignRenderSettings.Mode.PunchOut:
				value = "ui_game_symbol_signs_punch_out";
				return true;
			default:
				return false;
			}
		}
	}

	public override bool ParseAttribute(string name, string value)
	{
		if (base.ParseAttribute(name, value))
		{
			return true;
		}
		if (name == "multi_select_color")
		{
			this.multiSelectColor = StringParsers.ParseColor32(value);
			return true;
		}
		return false;
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool <RefreshLayerValidity>g__IsLayerValid|36_0(SignData.SignLayer layer)
	{
		LayerComplexityInfo layerComplexityInfo;
		return layer != null && this.editorWindow.signComplexityInfo.IsValid && this.editorWindow.signComplexityInfo.TryGetLayerComplexityInfo(layer, out layerComplexityInfo) && layerComplexityInfo.MaxCompStackIndex <= 7 && layerComplexityInfo.MaxUVStackIndex <= 7;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Texture signMaterial;

	public Action<XUiC_SignLayer> OnBecameCursorSelected;

	public Action<XUiC_SignLayer> OnClicked;

	[PublicizedFrom(EAccessModifier.Protected)]
	public Color32 multiSelectColor;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SignLayerGrid parentGrind;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SignEditorWindow editorWindow;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool layerValid = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController leftInsertPoint;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController rightInsertPoint;
}
