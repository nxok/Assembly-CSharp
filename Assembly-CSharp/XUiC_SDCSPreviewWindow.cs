using System;
using HorizonBasedAmbientOcclusion;
using PI.NGSS;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
[PublicizedFrom(EAccessModifier.Internal)]
public class XUiC_SDCSPreviewWindow : XUiController
{
	[XuiXmlAttribute("can_zoom", false)]
	public bool CanZoom
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	} = true;

	[XuiXmlAttribute("gamepad_rotation_mode", false)]
	public XUiC_SDCSPreviewWindow.eGamepadRotationMode GamepadRotationMode
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public override void Init()
	{
		base.Init();
		this.textPreview = (XUiV_Texture)base.GetChildById("playerPreview").ViewComponent;
		this.renderTextureSystem = new RenderTextureSystem();
		base.OnScroll += this.OnScrolled;
		base.OnDrag += this.OnDragged;
	}

	public override void Cleanup()
	{
		base.Cleanup();
		base.OnScroll -= this.OnScrolled;
		base.OnDrag -= this.OnDragged;
	}

	[XuiBindEvent("OnPress", "zoomButton")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void ZoomButton_OnPress(XUiController _sender, int _mouseButton)
	{
		this.toggleHeadZoom();
	}

	[XuiBindEvent("OnVisiblity", null)]
	[PublicizedFrom(EAccessModifier.Private)]
	public void visibilityChanged(XUiController _sender, bool _visibleSelf, bool _visibleInScene)
	{
		if (_visibleInScene)
		{
			this.startViz();
			return;
		}
		this.endViz();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void startViz()
	{
		this.originalPixelLightCount = QualitySettings.pixelLightCount;
		QualitySettings.pixelLightCount = 4;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void endViz()
	{
		SDCSUtils.UnloadViz(this.previewTransform);
		this.renderTextureSystem.Cleanup();
		this.lastProfile = "";
		this.lastFieldOfView = 54f;
		QualitySettings.pixelLightCount = this.originalPixelLightCount;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		string text = ProfileSDF.CurrentProfileName();
		if (this.viewComponent.IsActiveInHierarchy && text != this.lastProfile)
		{
			this.Archetype = Archetype.GetArchetype(text);
			if (this.Archetype == null)
			{
				this.Archetype = ProfileSDF.CreateTempArchetype(text);
			}
			this.MakePreview();
			this.lastProfile = text;
			if (this.CanZoom)
			{
				this.state = XUiC_SDCSPreviewWindow.ZoomStates.Head;
				this.setToHeadZoom();
			}
		}
		if (PlatformManager.NativePlatform.Input.CurrentInputStyle != PlayerInputManager.InputStyle.Keyboard)
		{
			this.updateController();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateController()
	{
		if (XUiUtils.HotkeysAllowedFor(this.viewComponent, false))
		{
			float num = 0f;
			switch (this.GamepadRotationMode)
			{
			case XUiC_SDCSPreviewWindow.eGamepadRotationMode.Bumper:
				num = this.xui.playerUI.playerInput.GUIActions.BumperAxis.Value;
				break;
			case XUiC_SDCSPreviewWindow.eGamepadRotationMode.Trigger:
				num = this.xui.playerUI.playerInput.GUIActions.TriggerAxis.Value;
				break;
			case XUiC_SDCSPreviewWindow.eGamepadRotationMode.RightStick:
				num = -this.xui.playerUI.playerInput.GUIActions.Camera.Value.x;
				break;
			}
			if (num != 0f)
			{
				this.cameraRotate(-num);
			}
			if (this.xui.playerUI.playerInput.GUIActions.HalfStack.WasPressed)
			{
				this.toggleHeadZoom();
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDragged(XUiController _sender, EDragType _dragType, Vector2 _mousePositionDelta)
	{
		float x = _mousePositionDelta.x;
		if (this.xui.playerUI.CursorController.GetMouseButton(1))
		{
			this.cameraRotate(x);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void cameraVerticalPan(float _value)
	{
		this.renderCamera.transform.localPosition -= new Vector3(0f, _value, 0f);
		if (this.renderCamera.transform.localPosition.y < -1.5f)
		{
			this.renderCamera.transform.localPosition = new Vector3(this.renderCamera.transform.localPosition.x, -1.5f, this.renderCamera.transform.localPosition.z);
			return;
		}
		if (this.renderCamera.transform.localPosition.y > 0f)
		{
			this.renderCamera.transform.localPosition = new Vector3(this.renderCamera.transform.localPosition.x, 0f, this.renderCamera.transform.localPosition.z);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void cameraRotate(float _value)
	{
		this.renderCamera.transform.RotateAround(this.previewTransform.transform.position, Vector3.up, _value);
		this.renderTextureSystem.LightGO.transform.RotateAround(this.previewTransform.transform.position, Vector3.up, _value);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnScrolled(XUiController _sender, float _delta)
	{
	}

	public void MakePreview()
	{
		SDCSUtils.CreateVizUI(this.Archetype, ref this.previewTransform, ref this.uiBoneCatalog);
		this.previewTransform.GetComponentInChildren<Animator>().Update(0f);
		this.init();
		this.previewTransform.transform.parent = this.renderTextureSystem.TargetGO.transform;
		this.previewTransform.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
		this.previewTransform.transform.localPosition = new Vector3(0f, -0.9f, 0f);
		this.characterGazeController = this.previewTransform.GetComponentInChildren<CharacterGazeController>();
		Utils.SetLayerRecursively(this.renderTextureSystem.TargetGO, 11);
		this.textPreview.Texture = this.renderTexture;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void toggleHeadZoom()
	{
		XUiC_SDCSPreviewWindow.ZoomStates zoomStates;
		switch (this.state)
		{
		case XUiC_SDCSPreviewWindow.ZoomStates.Eyes:
			zoomStates = XUiC_SDCSPreviewWindow.ZoomStates.Head;
			break;
		case XUiC_SDCSPreviewWindow.ZoomStates.Head:
			zoomStates = XUiC_SDCSPreviewWindow.ZoomStates.Chest;
			break;
		case XUiC_SDCSPreviewWindow.ZoomStates.Chest:
			zoomStates = XUiC_SDCSPreviewWindow.ZoomStates.FullBody;
			break;
		case XUiC_SDCSPreviewWindow.ZoomStates.FullBody:
			zoomStates = XUiC_SDCSPreviewWindow.ZoomStates.Eyes;
			break;
		default:
			zoomStates = this.state;
			break;
		}
		this.state = zoomStates;
		switch (this.state)
		{
		case XUiC_SDCSPreviewWindow.ZoomStates.Eyes:
			this.setToEyeZoom();
			return;
		case XUiC_SDCSPreviewWindow.ZoomStates.Head:
			this.setToHeadZoom();
			return;
		case XUiC_SDCSPreviewWindow.ZoomStates.Chest:
			this.setToChestZoom();
			return;
		case XUiC_SDCSPreviewWindow.ZoomStates.FullBody:
			this.setToFullBodyZoom();
			return;
		default:
			return;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setToFullBodyZoom()
	{
		this.renderCamera.transform.SetLocalPositionAndRotation(this.originalCamPosition, Quaternion.AngleAxis(19f, new Vector3(1f, 0f, 0f)) * this.originalCamRotation);
		this.renderTextureSystem.LightGO.transform.SetLocalPositionAndRotation(this.originalLightPosition, this.originalLightRotation);
		this.renderCamera.fieldOfView = 54f;
		this.setCameraInitialRotationOffset();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setToHeadZoom()
	{
		this.renderCamera.transform.SetLocalPositionAndRotation(this.originalCamPosition, Quaternion.AngleAxis(1.5f, new Vector3(1f, 0f, 0f)) * this.originalCamRotation);
		this.renderTextureSystem.LightGO.transform.SetLocalPositionAndRotation(this.originalLightPosition, this.originalLightRotation);
		this.renderCamera.fieldOfView = 12f;
		this.renderTextureSystem.TargetGO.transform.localPosition = new Vector3(0.015f, -0.78f, 2.14f);
		this.setCameraInitialRotationOffset();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setToChestZoom()
	{
		this.renderCamera.transform.SetLocalPositionAndRotation(this.originalCamPosition, Quaternion.AngleAxis(5f, new Vector3(1f, 0f, 0f)) * this.originalCamRotation);
		this.renderTextureSystem.LightGO.transform.SetLocalPositionAndRotation(this.originalLightPosition, this.originalLightRotation);
		this.renderCamera.fieldOfView = 20f;
		this.renderTextureSystem.TargetGO.transform.localPosition = new Vector3(0.02f, -0.78f, 2.14f);
		this.setCameraInitialRotationOffset();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setToEyeZoom()
	{
		this.renderCamera.transform.SetLocalPositionAndRotation(this.originalCamPosition, this.originalCamRotation);
		this.renderTextureSystem.LightGO.transform.SetLocalPositionAndRotation(this.originalLightPosition, this.originalLightRotation);
		this.renderCamera.fieldOfView = 6f;
		this.renderTextureSystem.TargetGO.transform.localPosition = new Vector3(0.015f, -0.78f, 2.14f);
		this.setCameraInitialRotationOffset();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setCameraInitialRotationOffset()
	{
		this.renderCamera.transform.RotateAround(this.previewTransform.transform.position, Vector3.up, -30f);
		this.renderTextureSystem.LightGO.transform.RotateAround(this.previewTransform.transform.position, Vector3.up, -30f);
		if (this.characterGazeController != null)
		{
			this.characterGazeController.SnapNextUpdate();
		}
	}

	public void ZoomToHead()
	{
		if (this.state == XUiC_SDCSPreviewWindow.ZoomStates.Head)
		{
			return;
		}
		if (PlatformManager.NativePlatform.Input.CurrentInputStyle != PlayerInputManager.InputStyle.Keyboard && this.xui.playerUI.playerInput.GUIActions.TriggerAxis.Value != 0f)
		{
			return;
		}
		this.state = XUiC_SDCSPreviewWindow.ZoomStates.Head;
		this.setToHeadZoom();
	}

	public void ZoomToEye()
	{
		if (PlatformManager.NativePlatform.Input.CurrentInputStyle != PlayerInputManager.InputStyle.Keyboard && this.xui.playerUI.playerInput.GUIActions.TriggerAxis.Value != 0f)
		{
			return;
		}
		this.state = XUiC_SDCSPreviewWindow.ZoomStates.Eyes;
		this.setToEyeZoom();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void init()
	{
		if (this.renderTextureSystem.ParentGO != null)
		{
			return;
		}
		this.renderTextureSystem.Create("characterpreview", new GameObject(), new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f), this.textPreview.Size, true, true, 1f);
		this.renderTextureSystem.TargetGO.transform.localPosition = new Vector3(0f, -0.75f, 2.15f);
		this.renderTexture = this.renderTextureSystem.RenderTex;
		this.renderCamera = this.renderTextureSystem.CameraGO.GetComponent<Camera>();
		this.renderCamera.orthographic = false;
		this.renderCamera.backgroundColor = new Color(0f, 0f, 0f, 0f);
		this.renderCamera.fieldOfView = 54f;
		this.renderCamera.renderingPath = 3;
		this.renderCamera.tag = "MainCamera";
		this.renderCamera.gameObject.AddComponent<StreamingController>();
		this.renderTextureSystem.CameraGO.AddComponent<NGSS_Local>().NGSS_PCSS_SOFTNESS_NEAR = 0.05f;
		HBAO hbao = this.renderTextureSystem.CameraGO.AddComponent<HBAO>();
		hbao.SetAoPerPixelNormals(2);
		hbao.SetAoIntensity(0.5f);
		this.renderTextureSystem.LightGO.GetComponent<Light>().enabled = false;
		GameObject gameObject = new GameObject("Key Light", new Type[] { typeof(Light) });
		gameObject.transform.SetParent(this.renderTextureSystem.LightGO.transform, false);
		gameObject.transform.SetPositionAndRotation(new Vector3(0.25f, 0.475f, 0.62f), Quaternion.Euler(33f, -8f, 0f));
		gameObject.AddComponent<NGSS_Directional>().NGSS_PCSS_ENABLED = true;
		Light component = gameObject.GetComponent<Light>();
		component.color = new Color(0.9f, 0.8f, 0.7f, 1f);
		component.type = 0;
		component.range = 20f;
		component.spotAngle = 60f;
		component.intensity = 1.5f;
		component.shadows = 1;
		component.shadowStrength = 0.2f;
		component.shadowBias = 0.005f;
		NGSS_FrustumShadows ngss_FrustumShadows = this.renderTextureSystem.CameraGO.AddComponent<NGSS_FrustumShadows>();
		ngss_FrustumShadows.mainShadowsLight = component;
		ngss_FrustumShadows.m_fastBlur = false;
		ngss_FrustumShadows.m_shadowsBlur = 1f;
		ngss_FrustumShadows.m_shadowsBlurIterations = 4;
		ngss_FrustumShadows.m_rayThickness = 0.025f;
		GameObject gameObject2 = new GameObject("Fill Light", new Type[] { typeof(Light) });
		gameObject2.transform.SetParent(this.renderTextureSystem.LightGO.transform, false);
		gameObject2.transform.SetPositionAndRotation(new Vector3(-1.15f, 1.4f, 1f), Quaternion.Euler(50f, 45f, 0f));
		Light component2 = gameObject2.GetComponent<Light>();
		component2.color = new Color(1f, 1f, 1f, 1f);
		component2.type = 0;
		component2.range = 20f;
		component2.spotAngle = 60f;
		component2.intensity = 0.5f;
		component2.shadows = 1;
		component2.shadowStrength = 0.2f;
		component2.shadowBias = 0.005f;
		GameObject gameObject3 = new GameObject("Fill 2 Light", new Type[] { typeof(Light) });
		gameObject3.transform.SetParent(this.renderTextureSystem.LightGO.transform, false);
		gameObject3.transform.SetPositionAndRotation(new Vector3(0f, -1.5f, -0.5f), Quaternion.Euler(-15f, 0f, 0f));
		Light component3 = gameObject3.GetComponent<Light>();
		component3.color = new Color(1f, 1f, 1f, 1f);
		component3.type = 0;
		component3.range = 20f;
		component3.spotAngle = 60f;
		component3.intensity = 0.5f;
		component3.shadows = 1;
		component3.shadowStrength = 0.2f;
		component3.shadowBias = 0.005f;
		GameObject gameObject4 = new GameObject("Back Light", new Type[] { typeof(Light) });
		gameObject4.transform.SetParent(this.renderTextureSystem.LightGO.transform, false);
		gameObject4.transform.SetPositionAndRotation(new Vector3(-0.6f, 0.75f, 2.6f), Quaternion.Euler(55f, 133f, 0f));
		Light component4 = gameObject4.GetComponent<Light>();
		component4.color = new Color(0.4f, 0.75f, 1f, 1f);
		component4.type = 0;
		component4.spotAngle = 60f;
		component4.range = 20f;
		component4.intensity = 1.5f;
		component4.shadows = 1;
		component4.shadowStrength = 0.2f;
		component4.shadowBias = 0.005f;
		this.originalCamPosition = this.renderCamera.transform.localPosition;
		this.originalCamRotation = this.renderCamera.transform.localRotation;
		this.originalLightPosition = this.renderTextureSystem.LightGO.transform.localPosition;
		this.originalLightRotation = this.renderTextureSystem.LightGO.transform.localRotation;
		this.renderCamera.transform.localPosition = new Vector3(0f, -0.75f, 0f);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public RenderTextureSystem renderTextureSystem;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Texture textPreview;

	[PublicizedFrom(EAccessModifier.Private)]
	public RenderTexture renderTexture;

	[PublicizedFrom(EAccessModifier.Private)]
	public Camera renderCamera;

	public Transform TargetTransform;

	public GameObject RotateTable;

	[PublicizedFrom(EAccessModifier.Private)]
	public GameObject previewTransform;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lastProfile;

	[PublicizedFrom(EAccessModifier.Private)]
	public float lastFieldOfView;

	public Archetype Archetype;

	[PublicizedFrom(EAccessModifier.Private)]
	public CharacterGazeController characterGazeController;

	[XuiBindComponent("zoomButton", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button zoomButton;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 originalCamPosition;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 originalLightPosition;

	[PublicizedFrom(EAccessModifier.Private)]
	public Quaternion originalCamRotation;

	[PublicizedFrom(EAccessModifier.Private)]
	public Quaternion originalLightRotation;

	[PublicizedFrom(EAccessModifier.Private)]
	public int originalPixelLightCount;

	[PublicizedFrom(EAccessModifier.Private)]
	public SDCSUtils.TransformCatalog uiBoneCatalog;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SDCSPreviewWindow.ZoomStates state = XUiC_SDCSPreviewWindow.ZoomStates.FullBody;

	[PublicizedFrom(EAccessModifier.Private)]
	public float baseOrtho = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public enum eGamepadRotationMode
	{
		Bumper,
		Trigger,
		RightStick
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public enum ZoomStates
	{
		Eyes,
		Head,
		Chest,
		FullBody
	}
}
