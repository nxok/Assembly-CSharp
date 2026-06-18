using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class EAIRunawayWhenHurt : EAIRunAway
{
	public EAIRunawayWhenHurt()
	{
		this.MutexBits = 1;
	}

	public override void SetData(Dictionary<string, string> data)
	{
		base.SetData(data);
		string text;
		if (data.TryGetValue("runChance", out text))
		{
			this.lowHealthPercent = 0f;
			if (StringParsers.ParseFloat(text, 0, -1, NumberStyles.Any) >= base.RandomFloat)
			{
				base.GetData(data, "healthPer", ref this.lowHealthPercent);
				if (data.TryGetValue("healthPerMax", out text))
				{
					float num = StringParsers.ParseFloat(text, 0, -1, NumberStyles.Any);
					this.lowHealthPercent += base.RandomFloat * (num - this.lowHealthPercent);
				}
			}
		}
	}

	public override bool CanExecute()
	{
		this.enemy = this.theEntity.GetRevengeTarget();
		return this.enemy && (this.lowHealthPercent >= 1f || (float)this.theEntity.Health / (float)this.theEntity.GetMaxHealth() < this.lowHealthPercent) && base.CanExecute();
	}

	public override bool Continue()
	{
		return base.Continue();
	}

	public override void Update()
	{
		base.Update();
		this.theEntity.navigator.setMoveSpeed(this.theEntity.IsInWater() ? this.theEntity.GetMoveSpeed() : this.theEntity.GetMoveSpeedPanic());
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override Vector3 GetFleeFromPos()
	{
		if (this.enemy)
		{
			return this.enemy.position;
		}
		return this.theEntity.position;
	}

	public override string ToString()
	{
		return string.Format("{0}, per {1}", base.ToString(), this.lowHealthPercent);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public float lowHealthPercent = 1f;
}
