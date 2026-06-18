using System;
using UnityEngine;

public class RootTransformRefEntity : RootTransformRef
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Start()
	{
		if (this.RootTransform == null)
		{
			this.RootTransform = RootTransformRefEntity.FindEntityUpwards(base.transform);
		}
	}

	public static RootTransformRefEntity AddIfEntity(Transform _t)
	{
		Transform transform = RootTransformRefEntity.FindEntityUpwards(_t);
		if (transform)
		{
			RootTransformRefEntity rootTransformRefEntity = NGUITools.AddMissingComponent<RootTransformRefEntity>(_t.gameObject);
			rootTransformRefEntity.RootTransform = transform;
			return rootTransformRefEntity;
		}
		return null;
	}

	public static Transform FindEntityUpwards(Transform _t)
	{
		while (!_t.GetComponent<Entity>())
		{
			_t = _t.parent;
			if (!_t)
			{
				return null;
			}
		}
		return _t;
	}

	public void GunOpen()
	{
	}

	public void GunClose()
	{
	}

	public void GunRemoveRound()
	{
	}

	public void GunLoadRound()
	{
	}

	public void GunCockBack()
	{
	}

	public void GunCockForward()
	{
	}
}
