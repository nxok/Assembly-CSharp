using System;
using UnityEngine;

[CreateAssetMenu(fileName = "HairColorSwatch", menuName = "Hair Color Management/Hair Color Swatch", order = 1)]
public class HairColorSwatch : ScriptableObject
{
	public void ApplyToMaterial(Material material)
	{
		material.SetColor("_Tint1", this.tint1);
		material.SetColor("_Tint2", this.tint2);
		material.SetColor("_Tint3", this.tint3);
		material.SetFloat("_TintSharpness", this.tintSharpness);
		material.SetFloat("_IDMapStrength", this.idMapStrength);
		material.SetFloat("_RootDarkening", this.rootDarkening);
		material.SetFloat("_Metallic", this.metallic);
		material.SetColor("_CuticleSpecularColor", this.cuticleSpecularColor);
		material.SetColor("_CortexSpecularColor", this.cortexSpecularColor);
		material.SetFloat("_SmoothnessMultiplier", this.smoothnessMultiplier);
		material.SetColor("_SubsurfaceAmbient", this.subsurfaceAmbient);
		material.SetColor("_SubsurfaceColor", this.subsurfaceColor);
	}

	public void ApplySwatchToGameObject(GameObject targetGameObject)
	{
		Shader shader = Shader.Find("Game/SDCS/Hair");
		if (targetGameObject != null)
		{
			foreach (Renderer renderer in targetGameObject.GetComponentsInChildren<Renderer>(true))
			{
				Material[] array;
				if (Application.isPlaying)
				{
					array = renderer.materials;
				}
				else
				{
					array = renderer.sharedMaterials;
				}
				foreach (Material material in array)
				{
					if (material.shader == shader)
					{
						this.ApplyToMaterial(material);
					}
				}
			}
			return;
		}
		Debug.LogWarning("No target GameObject selected.");
	}

	[ColorUsage(false, false)]
	public Color tint1 = Color.red;

	[ColorUsage(false, false)]
	public Color tint2 = Color.green;

	[ColorUsage(false, false)]
	public Color tint3 = Color.blue;

	[Range(0f, 1f)]
	public float tintSharpness = 0.5f;

	[Range(0f, 1f)]
	public float idMapStrength;

	[Range(0f, 1f)]
	public float rootDarkening;

	[Range(0f, 1f)]
	public float metallic;

	[ColorUsage(false, true)]
	public Color cuticleSpecularColor = Color.white;

	[ColorUsage(false, true)]
	public Color cortexSpecularColor = Color.white;

	[Range(0f, 10f)]
	public float smoothnessMultiplier = 1f;

	[ColorUsage(false, false)]
	public Color subsurfaceAmbient = Color.white;

	[ColorUsage(false, false)]
	public Color subsurfaceColor = Color.white;
}
