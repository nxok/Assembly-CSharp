using System;
using UnityEngine;

public class OnAnimatorIKForwardCall : MonoBehaviour
{
	public void OnAnimatorIK(int layerIndex)
	{
		if (this.Callback != null)
		{
			this.Callback.OnAnimatorIK(layerIndex);
		}
	}

	public IOnAnimatorIKCallback Callback;
}
