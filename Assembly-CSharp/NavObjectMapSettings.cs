using System;
using System.Globalization;
using UnityEngine;

public class NavObjectMapSettings : NavObjectSettings
{
	public override void Init()
	{
		base.Init();
		this.Properties.ParseInt("layer", ref this.Layer);
		if (this.Properties.Values.ContainsKey("icon_scale"))
		{
			this.IconScale = StringParsers.ParseFloat(this.Properties.Values["icon_scale"], 0, -1, NumberStyles.Any);
			this.IconScaleVector = new Vector3(this.IconScale, this.IconScale, this.IconScale);
		}
		this.Properties.ParseBool("adjust_center", ref this.AdjustCenter);
		this.Properties.ParseBool("use_rotation", ref this.UseRotation);
	}

	public int Layer;

	public float IconScale = 1f;

	public Vector3 IconScaleVector = Vector3.one;

	public bool UseRotation;

	public bool AdjustCenter;
}
