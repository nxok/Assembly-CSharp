using System;
using UnityEngine;

public class ClothFix : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Private)]
	public void Awake()
	{
		this.cloth = base.GetComponent<Cloth>();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnEnable()
	{
		this.cloth.enabled = false;
		this.cloth.enabled = true;
		MeshCollider[] components = base.GetComponents<MeshCollider>();
		for (int i = 0; i < components.Length; i++)
		{
			Object.Destroy(components[i]);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDisable()
	{
		this.cloth.enabled = false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Cloth cloth;
}
