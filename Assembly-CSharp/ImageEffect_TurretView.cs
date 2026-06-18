using System;
using UnityEngine;

public class ImageEffect_TurretView : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Private)]
	public void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		this.material.SetTexture("_BackBuffer", source);
		Graphics.Blit(source, destination, this.material);
	}

	public Material material;
}
