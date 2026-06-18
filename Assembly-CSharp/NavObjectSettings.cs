using System;
using UnityEngine;

public class NavObjectSettings
{
	public virtual void Init()
	{
		this.Properties.ParseString("sprite_name", ref this.SpriteName);
		this.Properties.ParseFloat("min_distance", ref this.MinDistance);
		this.Properties.ParseFloat("max_distance", ref this.MaxDistance);
		this.Properties.ParseVec("offset", ref this.Offset);
		this.Properties.ParseColor("color", ref this.Color);
		this.Properties.ParseBool("has_pulse", ref this.HasPulse);
	}

	public DynamicProperties Properties;

	public string SpriteName = "";

	public float MinDistance;

	public float MaxDistance = -1f;

	public Vector3 Offset;

	public Color Color = Color.white;

	public bool HasPulse;
}
