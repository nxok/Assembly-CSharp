using System;
using UnityEngine;

public class DelayedLightIgnition : MonoBehaviour
{
	public void Awake()
	{
		this.myLight = base.GetComponent<Light>();
	}

	public void Start()
	{
		this.myLight.enabled = false;
		this.timer = this.delay;
	}

	public void Update()
	{
		if (this.myLight != null && !this.myLight.enabled)
		{
			if (this.timer <= 0f)
			{
				this.myLight.enabled = true;
			}
			this.timer -= Time.deltaTime;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Light myLight;

	public float delay = 0.5f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float timer;
}
