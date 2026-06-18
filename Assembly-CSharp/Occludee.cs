using System;
using System.Collections.Generic;
using UnityEngine;

public class Occludee : MonoBehaviour
{
	public static void Add(GameObject obj)
	{
		if (!OcclusionManager.Instance.isEnabled)
		{
			return;
		}
		obj.AddComponent<Occludee>();
	}

	public static void Refresh(GameObject obj)
	{
		if (!OcclusionManager.Instance.isEnabled)
		{
			return;
		}
		Occludee component = obj.GetComponent<Occludee>();
		if (component && component.node != null)
		{
			component.node.Value.isAreaFound = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnEnable()
	{
		OcclusionManager instance = OcclusionManager.Instance;
		if (instance != null)
		{
			Renderer[] componentsInChildren = base.GetComponentsInChildren<Renderer>(true);
			if (componentsInChildren.Length != 0)
			{
				this.node = instance.RegisterOccludee(componentsInChildren, 32f);
				if (this.node == null)
				{
					Log.Warning("Occludee:OnEnable failed to register {0}", new object[] { base.name });
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDisable()
	{
		OcclusionManager instance = OcclusionManager.Instance;
		if (instance != null && this.node != null)
		{
			instance.UnregisterOccludee(this.node);
			this.node = null;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public LinkedListNode<OcclusionManager.OcclusionEntry> node;
}
