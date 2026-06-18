using System;
using System.Collections.Generic;
using UnityEngine;

public class CharacterMatrixOverride : MonoBehaviour
{
	public void Init(EntityPlayerLocal epl)
	{
		this.referenceCamera = epl.playerCamera;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnEnable()
	{
		Camera.onPreCull = (Camera.CameraCallback)Delegate.Combine(Camera.onPreCull, new Camera.CameraCallback(this.OnPreCullCallback));
		Camera.onPreRender = (Camera.CameraCallback)Delegate.Combine(Camera.onPreRender, new Camera.CameraCallback(this.OnPreRenderCallback));
		Camera.onPostRender = (Camera.CameraCallback)Delegate.Combine(Camera.onPostRender, new Camera.CameraCallback(this.OnPostRenderCallback));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDisable()
	{
		Camera.onPreCull = (Camera.CameraCallback)Delegate.Remove(Camera.onPreCull, new Camera.CameraCallback(this.OnPreCullCallback));
		Camera.onPreRender = (Camera.CameraCallback)Delegate.Remove(Camera.onPreRender, new Camera.CameraCallback(this.OnPreRenderCallback));
		Camera.onPostRender = (Camera.CameraCallback)Delegate.Remove(Camera.onPostRender, new Camera.CameraCallback(this.OnPostRenderCallback));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnPreCullCallback(Camera camera)
	{
		if (this.Active && camera == this.referenceCamera)
		{
			this.UpdateRendererList();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateRendererList()
	{
		this.overriddenRenderers.Clear();
		base.GetComponentsInChildren<Renderer>(this.overriddenRenderers);
		foreach (Renderer renderer in this.overriddenRenderers)
		{
			CharacterMatrixOverride.RendererSettings rendererSettings;
			if (!this.rendererSettingsMap.TryGetValue(renderer, out rendererSettings))
			{
				rendererSettings = new CharacterMatrixOverride.RendererSettings();
				rendererSettings.originalProperties = new MaterialPropertyBlock();
				rendererSettings.overriddenProperties = new MaterialPropertyBlock();
				this.rendererSettingsMap[renderer] = rendererSettings;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnPreRenderCallback(Camera camera)
	{
		if (!this.Active || camera != this.referenceCamera)
		{
			return;
		}
		Matrix4x4 matrix4x = Matrix4x4.Perspective(90f, 1f, 10000f, 10000.1f) * this.referenceCamera.worldToCameraMatrix;
		foreach (Renderer renderer in this.overriddenRenderers)
		{
			CharacterMatrixOverride.RendererSettings rendererSettings;
			if (!this.rendererSettingsMap.TryGetValue(renderer, out rendererSettings))
			{
				Log.Error("[CharacterMatrixOverride] Failed to retrieve RendererSettings for overridden renderer");
			}
			else
			{
				renderer.GetPropertyBlock(rendererSettings.originalProperties);
				renderer.GetPropertyBlock(rendererSettings.overriddenProperties);
				MaterialPropertyBlock overriddenProperties = rendererSettings.overriddenProperties;
				overriddenProperties.SetMatrix("unity_MatrixVP", matrix4x);
				renderer.SetPropertyBlock(overriddenProperties);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnPostRenderCallback(Camera camera)
	{
		if (camera != this.referenceCamera)
		{
			return;
		}
		foreach (Renderer renderer in this.overriddenRenderers)
		{
			CharacterMatrixOverride.RendererSettings rendererSettings;
			if (!(renderer == null) && this.rendererSettingsMap.TryGetValue(renderer, out rendererSettings))
			{
				renderer.SetPropertyBlock(rendererSettings.originalProperties);
			}
		}
	}

	public bool Active;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Camera referenceCamera;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public List<Renderer> overriddenRenderers = new List<Renderer>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Dictionary<Renderer, CharacterMatrixOverride.RendererSettings> rendererSettingsMap = new Dictionary<Renderer, CharacterMatrixOverride.RendererSettings>();

	[PublicizedFrom(EAccessModifier.Private)]
	public class RendererSettings
	{
		public MaterialPropertyBlock originalProperties;

		public MaterialPropertyBlock overriddenProperties;
	}
}
