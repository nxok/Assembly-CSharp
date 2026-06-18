using System;
using UnityEngine;

public class BlockSwitchSingleController : MonoBehaviour
{
	public bool Activated
	{
		get
		{
			return this.activated;
		}
		set
		{
			this.activated = value;
			this.SetState();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Start()
	{
		this.SetState();
	}

	public void SetState(bool _activated)
	{
		this.activated = _activated;
		this.SetState();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void SetState()
	{
		if (this.ItemPrefab != null)
		{
			this.ItemPrefab.SetActive(!this.activated);
		}
	}

	public GameObject ItemPrefab;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool activated;
}
