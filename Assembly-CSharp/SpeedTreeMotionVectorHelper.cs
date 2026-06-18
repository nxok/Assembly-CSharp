using System;
using UnityEngine;

public class SpeedTreeMotionVectorHelper : MonoBehaviour
{
	public void Init(Renderer renderer)
	{
		this.renderer = renderer;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnBecameVisible()
	{
		if (!SpeedTreeWindHistoryBufferManager.Instance.TryRegisterActiveRenderer(this.renderer))
		{
			Debug.LogError("Failed to register tree renderer.");
			base.enabled = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnBecameInvisible()
	{
		SpeedTreeWindHistoryBufferManager.Instance.DeregisterActiveRenderer(this.renderer);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Renderer renderer;
}
