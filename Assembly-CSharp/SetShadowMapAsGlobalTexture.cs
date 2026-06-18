using System;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode]
[RequireComponent(typeof(Light))]
public class SetShadowMapAsGlobalTexture : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Private)]
	public void OnEnable()
	{
		this.lightComponent = base.GetComponent<Light>();
		this.SetupCommandBuffer();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDisable()
	{
		this.lightComponent.RemoveCommandBuffer(1, this.commandBuffer);
		this.ReleaseCommandBuffer();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetupCommandBuffer()
	{
		this.commandBuffer = new CommandBuffer();
		this.commandBuffer.name = "SetShadowMapAsGlobalTexture";
		RenderTargetIdentifier renderTargetIdentifier = 1;
		this.commandBuffer.SetGlobalTexture(this.textureSemanticName, renderTargetIdentifier);
		this.lightComponent.AddCommandBuffer(1, this.commandBuffer);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ReleaseCommandBuffer()
	{
		this.commandBuffer.Clear();
	}

	public string textureSemanticName = "_SunCascadedShadowMap";

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public RenderTexture shadowMapRenderTexture;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public CommandBuffer commandBuffer;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Light lightComponent;
}
