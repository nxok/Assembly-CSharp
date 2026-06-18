using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_SignLayerDragAndDropIcon : XUiController
{
	public override void Init()
	{
		base.Init();
		this.signMaterial = base.GetChildById("signMaterial").ViewComponent as XUiV_Texture;
		XUiV_Texture xuiV_Texture = this.signMaterial;
		xuiV_Texture.OnRenderTexture = (UIDrawCall.OnRenderCallback)Delegate.Combine(xuiV_Texture.OnRenderTexture, new UIDrawCall.OnRenderCallback(this.OnWillRender));
		this.signMaterial.Texture = Texture2D.whiteTexture;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (this.IsDirty)
		{
			base.RefreshBindings();
			this.IsDirty = false;
		}
		if (this.viewComponent.IsVisible)
		{
			Vector2 screenPosition = this.xui.playerUI.CursorController.GetScreenPosition();
			Vector3 vector = this.xui.playerUI.camera.ScreenToWorldPoint(screenPosition);
			Transform transform = this.xui.transform;
			vector.z = transform.position.z - 3f * transform.lossyScale.z;
			base.ViewComponent.UiTransform.position = vector;
		}
	}

	public void SetLayer(GlobalSignId _signId, SignData.SignLayer _layer)
	{
		this.signId = _signId;
		this.layer = _layer;
		this.IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnWillRender(Material mat)
	{
		if (this.layer != null)
		{
			SignUIStyle signUIStyle = ((this.layer.renderSettings.mode == SignData.SignRenderSettings.Mode.ColorOnly || this.layer.renderSettings.mode == SignData.SignRenderSettings.Mode.ColorAndMask) ? SignUIStyle.LayerThumbnail : SignUIStyle.MaskThumbnail);
			SignDataManager.Instance.TryApplyRenderingData(this.signId, 1f, mat, this.layer, signUIStyle);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string _value, string _bindingName)
	{
		if (base.GetBindingValueInternal(ref _value, _bindingName))
		{
			return true;
		}
		if (_bindingName == "showdraganddrop")
		{
			_value = (this.layer != null).ToString();
			return true;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Texture signMaterial;

	[PublicizedFrom(EAccessModifier.Private)]
	public GlobalSignId signId;

	[PublicizedFrom(EAccessModifier.Private)]
	public SignData.SignLayer layer;
}
