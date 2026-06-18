using System;
using UnityEngine;

public class ColorSwatchApplicator : MonoBehaviour
{
	public static string baseHairColorLoc
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return "@:Entities/Player/Common/HairColorSwatches";
		}
	}

	public void ApplyColorSwatch(string color)
	{
		if (!string.IsNullOrEmpty(color))
		{
			string text = ColorSwatchApplicator.baseHairColorLoc + "/" + color + ".asset";
			ScriptableObject scriptableObject = DataLoader.LoadAsset<ScriptableObject>(text, false);
			if (scriptableObject == null)
			{
				Log.Warning(string.Concat(new string[] { "SDCSUtils::", text, " not found for hair color ", color, "!" }));
				return;
			}
			HairColorSwatch hairColorSwatch = scriptableObject as HairColorSwatch;
			if (hairColorSwatch != null)
			{
				this.ApplySwatchToGameObject(hairColorSwatch);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ApplySwatchToGameObject(HairColorSwatch hairSwatch)
	{
		foreach (Renderer renderer in base.transform.gameObject.GetComponentsInChildren<Renderer>(true))
		{
			Material[] materials = renderer.materials;
			for (int j = 0; j < materials.Length; j++)
			{
				if (materials[j].shader.name == "Game/SDCS/Hair" && !materials[j].name.Contains("lashes"))
				{
					materials[j] = new Material(materials[j]);
					hairSwatch.ApplyToMaterial(materials[j]);
				}
			}
			renderer.materials = materials;
		}
	}
}
