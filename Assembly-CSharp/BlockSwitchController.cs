using System;
using UnityEngine;

public class BlockSwitchController : MonoBehaviour
{
	public bool Powered
	{
		get
		{
			return this.powered;
		}
		set
		{
			this.powered = value;
			this.UpdateLights();
		}
	}

	public bool Activated
	{
		get
		{
			return this.activated;
		}
		set
		{
			this.activated = value;
			this.UpdateLights();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Start()
	{
		this.UpdateLights();
	}

	public void SetState(bool _powered, bool _activated)
	{
		this.powered = _powered;
		this.activated = _activated;
		this.UpdateLights();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void UpdateLights()
	{
		if (!this.Powered)
		{
			this.GreenLight.SetActive(false);
			this.RedLight.SetActive(false);
			return;
		}
		this.GreenLight.SetActive(this.activated);
		this.RedLight.SetActive(!this.activated);
	}

	public GameObject RedLight;

	public GameObject GreenLight;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool powered;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool activated;
}
