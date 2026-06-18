using System;
using System.Collections.Generic;
using UnityEngine;

public class SpeedTreeLODController : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Private)]
	public void Awake()
	{
		base.GetComponentsInChildren<Tree>(SpeedTreeLODController.tempTrees);
		foreach (Tree tree in SpeedTreeLODController.tempTrees)
		{
			Renderer renderer;
			if (tree.hasSpeedTreeWind && tree.TryGetComponent<Renderer>(ref renderer) && renderer.motionVectorGenerationMode == 1)
			{
				NGUITools.AddMissingComponent<SpeedTreeMotionVectorHelper>(tree.gameObject).Init(renderer);
			}
		}
		SpeedTreeLODController.tempTrees.Clear();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Start()
	{
		LODGroup component = base.GetComponent<LODGroup>();
		int lodCount = component.lodCount;
		if (lodCount > 0)
		{
			LOD[] lods = component.GetLODs();
			float num = Utils.FastLerp(0.02f, 0.03f, component.size / 5f);
			lods[lodCount - 1].screenRelativeTransitionHeight = num;
			if (lodCount > 1)
			{
				float num2 = 0.17f;
				float num3 = (0.58f - num2) / ((float)(lodCount - 2) + 0.001f);
				for (int i = lodCount - 2; i >= 0; i--)
				{
					lods[i].screenRelativeTransitionHeight = num2;
					num2 += num3;
				}
			}
			component.SetLODs(lods);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static List<Tree> tempTrees = new List<Tree>();
}
