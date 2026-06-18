using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_SignInstanceWindow : XUiController
{
	public override void Init()
	{
		base.Init();
		this.comboBlendMode = base.GetChildByType<XUiC_ComboBoxEnum<SignCanvas.SignBlendMode>>();
		this.toggleShowOnImposter = base.GetChildById("toggleShowOnImposter") as XUiC_ToggleButton;
		this.cbxRotation = base.GetChildById("cbxRotation").GetChildByType<XUiC_ComboBoxInt>();
		this.cbxRotation.OnValueChangedGeneric += this.HandleRotationChanged;
		this.previewTexture = (XUiV_Texture)base.GetChildById("previewMaterial").ViewComponent;
		XUiController childById = base.GetChildById("previewArea");
		childById.OnDrag += this.HandlePreviewDragged;
		childById.OnScroll += this.HandlePreviewScrolled;
		base.GetChildById("btnToggleLight").OnPress += this.HandleToggleLight;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (this.IsDirty && base.ViewComponent.IsVisible)
		{
			this.IsDirty = false;
		}
	}

	public override void OnClose()
	{
		base.OnClose();
		this.comboBlendMode.OnValueChanged -= this.ComboBlendMode_OnValueChanged;
		this.toggleShowOnImposter.OnValueChanged -= this.ToggleShowOnImposter_OnValueChanged;
		this.CleanupPreview();
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.comboBlendMode.OnValueChanged -= this.ComboBlendMode_OnValueChanged;
		this.comboBlendMode.OnValueChanged += this.ComboBlendMode_OnValueChanged;
		this.toggleShowOnImposter.OnValueChanged -= this.ToggleShowOnImposter_OnValueChanged;
		this.toggleShowOnImposter.OnValueChanged += this.ToggleShowOnImposter_OnValueChanged;
		base.RefreshBindings();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ComboBlendMode_OnValueChanged(XUiController _sender, SignCanvas.SignBlendMode _oldValue, SignCanvas.SignBlendMode _newValue)
	{
		TEFeatureCanvas tefeatureCanvas = this.signTileEntity;
		if (((tefeatureCanvas != null) ? tefeatureCanvas.Canvas : null) != null)
		{
			this.signTileEntity.Canvas.BlendMode = _newValue;
			this.RefreshPreviewRenderers();
			PrefabEditModeManager.Instance.NeedsSaving = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ToggleShowOnImposter_OnValueChanged(XUiC_ToggleButton _sender, bool _newValue)
	{
		TEFeatureCanvas tefeatureCanvas = this.signTileEntity;
		if (((tefeatureCanvas != null) ? tefeatureCanvas.Canvas : null) != null)
		{
			this.signTileEntity.Canvas.ShowOnImposter = _newValue;
			this.signTileEntity.SetModified();
			PrefabEditModeManager.Instance.NeedsSaving = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string value, string bindingName)
	{
		if (bindingName == "blockname")
		{
			value = ((this.signTileEntity != null) ? this.signTileEntity.Parent.block.GetBlockName() : "");
			return true;
		}
		if (bindingName == "showblendmode")
		{
			TEFeatureCanvas tefeatureCanvas = this.signTileEntity;
			value = (((tefeatureCanvas != null) ? tefeatureCanvas.Canvas : null) != null && this.signTileEntity.Canvas.IsDecal).ToString();
			return true;
		}
		if (!(bindingName == "allowedonimposter"))
		{
			return base.GetBindingValueInternal(ref value, bindingName);
		}
		TEFeatureCanvas tefeatureCanvas2 = this.signTileEntity;
		value = (((tefeatureCanvas2 != null) ? tefeatureCanvas2.Canvas : null) != null && this.signTileEntity.Canvas.AllowedOnImposter).ToString();
		return true;
	}

	public void InitialiseTo(TEFeatureCanvas sign)
	{
		this.signTileEntity = sign;
		this.comboBlendMode.OnValueChanged -= this.ComboBlendMode_OnValueChanged;
		this.toggleShowOnImposter.OnValueChanged -= this.ToggleShowOnImposter_OnValueChanged;
		TEFeatureCanvas tefeatureCanvas = this.signTileEntity;
		if (((tefeatureCanvas != null) ? tefeatureCanvas.Canvas : null) != null)
		{
			this.comboBlendMode.Value = sign.Canvas.BlendMode;
			this.cbxRotation.Value = (long)(sign.Canvas.CanvasRotation * (CanvasRotationMode)90);
			this.toggleShowOnImposter.Value = sign.Canvas.ShowOnImposter;
		}
		else
		{
			this.comboBlendMode.Value = SignCanvas.SignBlendMode.Cutout;
			this.cbxRotation.Value = 0L;
			this.toggleShowOnImposter.Value = false;
		}
		this.comboBlendMode.OnValueChanged += this.ComboBlendMode_OnValueChanged;
		this.toggleShowOnImposter.OnValueChanged += this.ToggleShowOnImposter_OnValueChanged;
		base.RefreshBindings();
		this.SetupPreview(sign);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetupPreview(TEFeatureCanvas sign)
	{
		this.CleanupPreview();
		if (sign == null)
		{
			return;
		}
		BlockShapeModelEntity blockShapeModelEntity = sign.Parent.block.shape as BlockShapeModelEntity;
		if (blockShapeModelEntity == null)
		{
			return;
		}
		GameObject gameObject = new GameObject("SignPreviewPivot");
		this.renderTextureSystem = new RenderTextureSystem();
		this.renderTextureSystem.Create("signPreview", gameObject, Vector3.zero, new Vector3(1f, 1.5f, -1f), new Vector2i(this.previewTexture.Size.x, this.previewTexture.Size.y), true, false, 1f);
		Transform transform = blockShapeModelEntity.CloneModel(sign.Parent.blockValue, gameObject.transform);
		Utils.SetLayerRecursively(gameObject, 11);
		Quaternion rotationStatic = BlockShapeNew.GetRotationStatic((int)sign.Parent.blockValue.rotation);
		Vector3 vector = rotationStatic * Vector3.forward;
		if (Mathf.Abs(vector.y) < 0.5f)
		{
			vector.y = 0f;
			float num = Vector3.SignedAngle(vector, Vector3.back, Vector3.up);
			gameObject.transform.rotation = Quaternion.AngleAxis(num, Vector3.up);
		}
		else
		{
			gameObject.transform.rotation = Quaternion.AngleAxis(180f, Vector3.up) * Quaternion.Inverse(rotationStatic);
		}
		transform.GetComponentsInChildren<SignRenderer>(true, this.previewSignRenderers);
		this.renderCamera = this.renderTextureSystem.CameraGO.GetComponent<Camera>();
		this.renderCamera.fieldOfView = 40f;
		this.renderCamera.backgroundColor = new Color(0f, 0f, 0f, 0f);
		this.renderCamera.renderingPath = 3;
		this.renderCamera.farClipPlane = 32f;
		Bounds bounds = XUiC_SignInstanceWindow.ComputeBounds(gameObject);
		Vector3 center = bounds.center;
		float num2 = this.renderCamera.fieldOfView * 0.5f * 0.017453292f;
		float num3 = (float)this.previewTexture.Size.x / (float)this.previewTexture.Size.y;
		float num4 = Mathf.Max(bounds.extents.x, Mathf.Max(bounds.extents.y, bounds.extents.z));
		this.maxDistance = Mathf.Max(num4 / Mathf.Tan(num2) * 1.2f, 1.5f);
		this.minDistance = this.maxDistance * 0.1f;
		float num5 = bounds.extents.y / Mathf.Tan(num2);
		float num6 = bounds.extents.x / (Mathf.Tan(num2) * num3);
		this.currentDistance = Mathf.Clamp(Mathf.Max(num5, num6) * 1.15f, this.minDistance, this.maxDistance);
		gameObject.transform.position -= center;
		this.renderCamera.transform.position = new Vector3(0.1f, 0.15f, -1f).normalized * this.currentDistance;
		this.renderCamera.transform.LookAt(Vector3.zero);
		if (sign.Canvas.IsDecal)
		{
			if (this.decalBackgroundMesh == null)
			{
				this.InitializeDecalBackgroundMesh();
			}
			this.decalBackgroundMesh.SetActive(true);
			this.decalBackgroundMesh.transform.SetParent(transform.transform);
			this.decalBackgroundMesh.transform.localEulerAngles = new Vector3(0f, 180f, 0f);
			this.decalBackgroundMesh.transform.localPosition = new Vector3(0f, 2f, -0.5f);
		}
		else
		{
			GameObject gameObject2 = this.decalBackgroundMesh;
			if (gameObject2 != null)
			{
				gameObject2.SetActive(false);
			}
		}
		Light component = this.renderTextureSystem.LightGO.GetComponent<Light>();
		component.type = 1;
		component.intensity = 1f;
		component.color = Color.white;
		this.renderTextureSystem.LightGO.transform.position = this.renderCamera.transform.position;
		this.renderTextureSystem.LightGO.transform.rotation = this.renderCamera.transform.rotation;
		this.previewLight = component;
		this.previewTexture.Texture = this.renderTextureSystem.RenderTex;
		this.RefreshPreviewRenderers();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void InitializeDecalBackgroundMesh()
	{
		this.decalBackgroundMesh = GameObject.CreatePrimitive(5);
		this.decalBackgroundMesh.name = "SignInstancePreviewBackgroundMesh";
		this.decalBackgroundMesh.layer = 11;
		this.decalBackgroundMesh.transform.localScale = Vector3.one * 32f;
		MeshRenderer component = this.decalBackgroundMesh.GetComponent<MeshRenderer>();
		component.material = Resources.Load<Material>("Materials/SignPreviewBackground");
		component.material.SetVector("_MainTex_ST", new Vector4(4f, 4f, 0f, 0f));
		this.decalBackgroundMesh.GetComponent<Collider>().enabled = false;
	}

	public void RefreshPreviewRenderers()
	{
		if (this.signTileEntity == null || this.previewSignRenderers.Count == 0)
		{
			return;
		}
		SignDataManager.Instance.TryApplyRenderingData(this.signTileEntity.Canvas.DisplaySignId, delegate(MaterialPropertyBlock mpb)
		{
			mpb.SetInteger(SignShaderIDs._UseTexture, 0);
			mpb.SetTexture(SignShaderIDs._BakedTexture, Texture2D.whiteTexture);
			mpb.SetFloat(SignShaderIDs._CanvasRotation, this.signTileEntity.Canvas.CanvasRotationRadians);
		}, this.previewSignRenderers, this.signTileEntity.Canvas.BlendMode, this.renderCamera);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static Bounds ComputeBounds(GameObject go)
	{
		Renderer[] componentsInChildren = go.GetComponentsInChildren<Renderer>();
		if (componentsInChildren.Length == 0)
		{
			return new Bounds(go.transform.position, Vector3.one);
		}
		TemporaryObject componentInChildren = go.GetComponentInChildren<TemporaryObject>();
		Transform transform = ((componentInChildren != null) ? componentInChildren.transform : null);
		Bounds bounds = componentsInChildren[0].bounds;
		for (int i = 1; i < componentsInChildren.Length; i++)
		{
			if (!(transform != null) || !componentsInChildren[i].transform.IsChildOf(transform))
			{
				bounds.Encapsulate(componentsInChildren[i].bounds);
			}
		}
		return bounds;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CleanupPreview()
	{
		if (this.decalBackgroundMesh != null)
		{
			this.decalBackgroundMesh.transform.SetParent(base.ViewComponent.UiTransform);
			this.decalBackgroundMesh.SetActive(false);
		}
		this.previewSignRenderers.Clear();
		if (this.renderTextureSystem != null)
		{
			this.renderTextureSystem.Cleanup();
			this.renderTextureSystem = null;
		}
		this.renderCamera = null;
		this.previewLight = null;
		if (this.previewTexture != null)
		{
			this.previewTexture.Texture = null;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandlePreviewDragged(XUiController _sender, EDragType _dragType, Vector2 _mousePositionDelta)
	{
		if (this.renderCamera == null)
		{
			return;
		}
		KeyCode currentKey = UICamera.currentKey;
		if (currentKey == 323)
		{
			this.OrbitCamera(_mousePositionDelta);
			return;
		}
		if (currentKey - 324 > 1)
		{
			return;
		}
		this.ZoomCamera(_mousePositionDelta.y * 0.005f);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandlePreviewScrolled(XUiController _sender, float _delta)
	{
		this.ZoomCamera(_delta);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OrbitCamera(Vector2 _mousePositionDelta)
	{
		if (Mathf.Abs(_mousePositionDelta.x) > 0.01f)
		{
			this.renderCamera.transform.RotateAround(Vector3.zero, Vector3.up, _mousePositionDelta.x * 0.5f);
		}
		if (Mathf.Abs(_mousePositionDelta.y) > 0.01f)
		{
			this.renderCamera.transform.RotateAround(Vector3.zero, this.renderCamera.transform.right, -_mousePositionDelta.y * 0.5f);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ZoomCamera(float _zoomDelta)
	{
		if (this.renderCamera == null)
		{
			return;
		}
		this.currentDistance = Mathf.Clamp(this.currentDistance - _zoomDelta * this.maxDistance, this.minDistance, this.maxDistance);
		this.renderCamera.transform.position = this.renderCamera.transform.position.normalized * this.currentDistance;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleToggleLight(XUiController _sender, int _mouseButton)
	{
		if (this.previewLight != null)
		{
			this.previewLight.enabled = !this.previewLight.enabled;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleRotationChanged(XUiController _sender)
	{
		if (this.signTileEntity != null)
		{
			this.signTileEntity.Canvas.CanvasRotation = (CanvasRotationMode)(this.cbxRotation.Value / 90L);
			this.cbxRotation.Value = (long)(this.signTileEntity.Canvas.CanvasRotation * (CanvasRotationMode)90);
			this.RefreshPreviewRenderers();
			PrefabEditModeManager.Instance.NeedsSaving = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public TEFeatureCanvas signTileEntity;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxEnum<SignCanvas.SignBlendMode> comboBlendMode;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxInt cbxRotation;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ToggleButton toggleShowOnImposter;

	[PublicizedFrom(EAccessModifier.Private)]
	public RenderTextureSystem renderTextureSystem;

	[PublicizedFrom(EAccessModifier.Private)]
	public Camera renderCamera;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Texture previewTexture;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<SignRenderer> previewSignRenderers = new List<SignRenderer>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Light previewLight;

	[PublicizedFrom(EAccessModifier.Private)]
	public float maxDistance;

	[PublicizedFrom(EAccessModifier.Private)]
	public float minDistance;

	[PublicizedFrom(EAccessModifier.Private)]
	public float currentDistance;

	[PublicizedFrom(EAccessModifier.Private)]
	public GameObject decalBackgroundMesh;
}
