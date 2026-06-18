using System;
using UnityEngine;

public class SkinningTestSceneTools : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Private)]
	public void Start()
	{
		this.anim = base.GetComponent<Animator>();
		if (this.anim != null)
		{
			this.maxLayers = this.anim.layerCount;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		if (Input.GetKeyUp(32) && this.anim != null)
		{
			this.layerIndex++;
			if (this.layerIndex == this.maxLayers)
			{
				this.layerIndex = 0;
				for (int i = 1; i < this.maxLayers - 1; i++)
				{
					this.anim.SetLayerWeight(i, 0f);
				}
			}
			this.targetLayerWeight = 0f;
			this.endLayerWeight = 1f;
		}
		if (this.layerIndex == 0)
		{
			this.endLayerWeight = Mathf.Lerp(this.endLayerWeight, 0f, 0.01f);
			this.anim.SetLayerWeight(this.maxLayers - 1, this.endLayerWeight);
		}
		else
		{
			this.targetLayerWeight = Mathf.Lerp(this.targetLayerWeight, 1f, 0.01f);
			this.anim.SetLayerWeight(this.layerIndex, this.targetLayerWeight);
		}
		if (Input.GetKey(97))
		{
			base.transform.Rotate(0f, this.turnRate * Time.deltaTime * -1f, 0f);
		}
		if (Input.GetKey(100))
		{
			base.transform.Rotate(0f, this.turnRate * Time.deltaTime, 0f);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Animator anim;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int layerIndex;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int maxLayers;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float turnRate = 600f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int totalModels;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int totalBodyParts;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int randomMaterial;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float targetLayerWeight;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float endLayerWeight;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int currentModel;
}
