using System;
using UnityEngine;

public class DroneLightManager : MonoBehaviour
{
	public void InitMaterials(string key)
	{
		DroneLightManager.LightEffect lightEffect = this.getLightEffect(key);
		if (lightEffect == null)
		{
			Debug.LogWarning("Failed to find drone light with name: " + key, this);
			return;
		}
		for (int i = 0; i < lightEffect.linkedObjects.Length; i++)
		{
			lightEffect.linkedObjects[i].SetActive(true);
		}
		for (int j = 0; j < base.transform.childCount; j++)
		{
			SkinnedMeshRenderer component = base.transform.GetChild(j).GetComponent<SkinnedMeshRenderer>();
			if (component)
			{
				Material[] materials = component.materials;
				for (int k = materials.Length - 1; k >= 0; k--)
				{
					if (materials[k].name.Replace(" (Instance)", "") == lightEffect.material.name)
					{
						materials[k].SetColor("_EmissionColor", lightEffect.material.GetColor("_EmissionColor"));
						break;
					}
				}
			}
		}
	}

	public void DisableMaterials(string key)
	{
		DroneLightManager.LightEffect lightEffect = this.getLightEffect(key);
		if (lightEffect == null)
		{
			Debug.LogWarning("Failed to find drone light with name: " + key, this);
			return;
		}
		for (int i = 0; i < lightEffect.linkedObjects.Length; i++)
		{
			lightEffect.linkedObjects[i].SetActive(false);
		}
		for (int j = 0; j < base.transform.childCount; j++)
		{
			SkinnedMeshRenderer component = base.transform.GetChild(j).GetComponent<SkinnedMeshRenderer>();
			if (component)
			{
				Material[] materials = component.materials;
				for (int k = materials.Length - 1; k >= 0; k--)
				{
					if (materials[k].name.Replace(" (Instance)", "") == lightEffect.material.name)
					{
						materials[k].SetColor("_EmissionColor", Color.black);
						break;
					}
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public DroneLightManager.LightEffect getLightEffect(string key)
	{
		for (int i = 0; i < this.LightEffects.Length; i++)
		{
			if (this.LightEffects[i].material.name == key)
			{
				return this.LightEffects[i];
			}
		}
		return null;
	}

	public DroneLightManager.LightEffect[] LightEffects;

	[Serializable]
	public class LightEffect
	{
		public bool startsOn;

		public Material material;

		public GameObject[] linkedObjects;
	}
}
