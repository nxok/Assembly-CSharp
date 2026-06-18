using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ItemClassWaterContainer : ItemClass
{
	public int MaxMass
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public override void Init()
	{
		base.Init();
		float num = 0f;
		this.Properties.ParseFloat("WaterCapacity", ref num);
		this.MaxMass = Mathf.Clamp((int)(num * 19500f), 0, 65535);
		this.Properties.ParseFloat("InitialFillRatio", ref this.initialFillRatio);
		this.initialFillRatio = Mathf.Clamp(this.initialFillRatio, 0f, 1f);
	}

	public override int GetInitialMetadata(ItemValue _itemValue)
	{
		return (int)((float)this.MaxMass * this.initialFillRatio);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const string PropWaterCapacity = "WaterCapacity";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string PropInitFillRatio = "InitialFillRatio";

	[PublicizedFrom(EAccessModifier.Private)]
	public float initialFillRatio;
}
