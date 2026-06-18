using System;
using UnityEngine;
using UnityEngine.Rendering;

public class SignRenderer : MonoBehaviour
{
	public Renderer Renderer
	{
		get
		{
			return this.renderer;
		}
	}

	public bool IsDecal
	{
		get
		{
			return this.isDecal;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnEnable()
	{
		if (this.isDecal && this.signBlendMode == SignCanvas.SignBlendMode.AlphaBlend && this.InitializeAlphaBlendDecal())
		{
			this.BuildDecalCommandBuffer();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDisable()
	{
		if (this.isDecal && this.signBlendMode == SignCanvas.SignBlendMode.AlphaBlend)
		{
			this.TeardownDecalIfNeeded();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool InitializeAlphaBlendDecal()
	{
		Camera camera;
		if (!(this.alphaBlendCamera == null))
		{
			camera = this.alphaBlendCamera;
		}
		else
		{
			EntityPlayerLocal primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
			camera = ((primaryPlayer != null) ? primaryPlayer.playerCamera : null);
		}
		this.targetCamera = camera;
		if (this.signMaterial == null)
		{
			this.signMaterial = DataLoader.LoadAsset<Material>("@:Entities/Crafting/Materials/sign_decal.mat", false);
		}
		if (this.targetCamera == null || this.renderer == null || this.signMaterial == null)
		{
			Debug.LogError("[SignRenderer] Alpha blended rendering mode enabled but references are missing.", this);
			return false;
		}
		this.renderer.enabled = false;
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TeardownDecalIfNeeded()
	{
		this.renderer.enabled = true;
		this.RemoveDecalCommandBuffer();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BuildDecalCommandBuffer()
	{
		this.decalCommandBuffer = new CommandBuffer
		{
			name = "SignAlphaBlendedDecal: " + base.gameObject.name
		};
		this.decalCommandBuffer.DrawRenderer(this.renderer, this.signMaterial, 0, 0);
		this.targetCamera.AddCommandBuffer(5, this.decalCommandBuffer);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RemoveDecalCommandBuffer()
	{
		if (this.targetCamera != null && this.decalCommandBuffer != null)
		{
			this.targetCamera.RemoveCommandBuffer(5, this.decalCommandBuffer);
		}
		CommandBuffer commandBuffer = this.decalCommandBuffer;
		if (commandBuffer != null)
		{
			commandBuffer.Dispose();
		}
		this.decalCommandBuffer = null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetRenderingOn()
	{
		if (this.isDecal && this.signBlendMode == SignCanvas.SignBlendMode.AlphaBlend)
		{
			this.renderer.enabled = false;
		}
	}

	public void SetRenderParameters(MaterialPropertyBlock mpb, SignCanvas.SignBlendMode blendMode = SignCanvas.SignBlendMode.Cutout, Camera alphaBlendCamera = null)
	{
		this.renderer.SetPropertyBlock(mpb);
		if (this.IsDecal)
		{
			this.signBlendMode = blendMode;
			this.alphaBlendCamera = alphaBlendCamera;
			if (blendMode == SignCanvas.SignBlendMode.Cutout)
			{
				this.TeardownDecalIfNeeded();
				return;
			}
			if (blendMode != SignCanvas.SignBlendMode.AlphaBlend)
			{
				return;
			}
			if (this.InitializeAlphaBlendDecal())
			{
				this.RemoveDecalCommandBuffer();
				this.BuildDecalCommandBuffer();
			}
		}
	}

	[SerializeField]
	[PublicizedFrom(EAccessModifier.Private)]
	public MeshRenderer renderer;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Camera targetCamera;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public SignCanvas.SignBlendMode signBlendMode;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public CommandBuffer decalCommandBuffer;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Material signMaterial;

	[SerializeField]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool isDecal;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const string cAlphaBlendedSignMaterialPath = "@:Entities/Crafting/Materials/sign_decal.mat";

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Camera alphaBlendCamera;
}
