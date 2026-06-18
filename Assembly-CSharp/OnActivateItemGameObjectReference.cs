using System;
using UnityEngine;

public class OnActivateItemGameObjectReference : MonoBehaviour
{
	public void ActivateItem(bool _activate)
	{
		if (this.ActivateGameObjectTransform != null)
		{
			this.ActivateGameObjectTransform.gameObject.SetActive(_activate);
		}
	}

	public bool IsActivated()
	{
		return this.ActivateGameObjectTransform != null && this.ActivateGameObjectTransform.gameObject.activeSelf;
	}

	public Transform ActivateGameObjectTransform;
}
