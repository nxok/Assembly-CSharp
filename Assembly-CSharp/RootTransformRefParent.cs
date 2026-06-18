using System;
using UnityEngine;

public class RootTransformRefParent : RootTransformRef
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public void Awake()
	{
		if (this.RootTransform)
		{
			return;
		}
		this.RootTransform = this.FindTopTransform(base.transform);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Transform FindTopTransform(Transform _t)
	{
		Transform parent;
		while ((parent = _t.parent) != null)
		{
			_t = parent;
		}
		return _t;
	}

	public static Transform FindRoot(Transform _t)
	{
		Transform transform = _t;
		RootTransformRefParent rootTransformRefParent;
		while (!transform.TryGetComponent<RootTransformRefParent>(ref rootTransformRefParent))
		{
			transform = transform.parent;
			if (!transform)
			{
				return _t;
			}
		}
		return rootTransformRefParent.RootTransform;
	}
}
