using System;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class LocalPlayerCamera : MonoBehaviour
{
	public event Action<LocalPlayerCamera> PreCull;

	public event Action<LocalPlayerCamera> PreRender;

	public static LocalPlayerCamera AddToCamera(Camera camera, LocalPlayerCamera.CameraType camType)
	{
		LocalPlayerCamera localPlayerCamera = NGUITools.AddMissingComponent<LocalPlayerCamera>(camera.gameObject);
		if (camType != LocalPlayerCamera.CameraType.UI)
		{
			localPlayerCamera.Init(camType);
		}
		return localPlayerCamera;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Init(LocalPlayerCamera.CameraType camType)
	{
		this.camera = base.GetComponent<Camera>();
		this.cameraType = camType;
		if (camType != LocalPlayerCamera.CameraType.UI)
		{
			this.camera.allowDynamicResolution = true;
		}
		this.entityPlayerLocal = base.GetComponentInParent<EntityPlayerLocal>();
		this.localPlayer = base.GetComponentInParent<LocalPlayer>();
	}

	public void SetUI(LocalPlayerUI ui)
	{
		this.playerUI = ui;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnEnable()
	{
		this.playerUI = base.GetComponentInChildren<LocalPlayerUI>();
		if (this.playerUI)
		{
			this.Init(LocalPlayerCamera.CameraType.UI);
			this.playerUI.UpdateChildCameraIndices();
		}
		LocalPlayerManager.OnLocalPlayersChanged += this.HandleLocalPlayersChanged;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDisable()
	{
		LocalPlayerManager.OnLocalPlayersChanged -= this.HandleLocalPlayersChanged;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool IsAttachedToLocalPlayer()
	{
		return this.localPlayer != null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleLocalPlayersChanged()
	{
		this.ModifyCameraProperties();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ModifyCameraProperties()
	{
		this.camera.enabled = true;
		if (this.IsAttachedToLocalPlayer())
		{
			this.camera.fieldOfView = (float)Constants.cDefaultCameraFieldOfView * LocalPlayerCamera.splitScreenFOVFactors;
			return;
		}
		UIRect[] componentsInChildren = base.GetComponentsInChildren<UIRect>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].UpdateAnchors();
		}
		this.SetCameraDepth();
	}

	public void SetCameraDepth()
	{
		this.camera.depth = 1.01f + (float)this.playerUI.playerIndex * 0.01f + (float)this.uiChildIndex * 0.001f;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnPreCull()
	{
		if (this.cameraType == LocalPlayerCamera.CameraType.Main)
		{
			OcclusionManager.Instance.LocalPlayerOnPreCull();
		}
		if (this.PreCull != null)
		{
			this.PreCull(this);
		}
		if (GameRenderManager.dynamicIsEnabled && (this.cameraType == LocalPlayerCamera.CameraType.Main || this.cameraType == LocalPlayerCamera.CameraType.Weapon))
		{
			this.camera.targetTexture = this.entityPlayerLocal.renderManager.GetDynamicRenderTexture();
		}
		if (this.cameraType == LocalPlayerCamera.CameraType.Main)
		{
			this.entityPlayerLocal.renderManager.UpscalingPreCull();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnPreRender()
	{
		if (this.PreRender != null)
		{
			this.PreRender(this);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static readonly float splitScreenFOVFactors = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Camera camera;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public LocalPlayerCamera.CameraType cameraType;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public LocalPlayerUI playerUI;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public EntityPlayerLocal entityPlayerLocal;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public LocalPlayer localPlayer;

	public int uiChildIndex;

	public enum CameraType
	{
		None,
		Main,
		Weapon,
		UI
	}
}
