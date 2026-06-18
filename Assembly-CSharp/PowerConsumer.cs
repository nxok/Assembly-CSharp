using System;
using Audio;

public class PowerConsumer : PowerItem
{
	public override void HandlePowerUpdate(bool isOn)
	{
		bool flag = this.isPowered && isOn;
		if (this.TileEntity != null)
		{
			this.TileEntity.Activate(flag);
			if (flag && this.lastActivate != flag)
			{
				this.TileEntity.ActivateOnce();
			}
			this.TileEntity.SetModified();
		}
		this.lastActivate = flag;
		if (this.PowerChildren())
		{
			for (int i = 0; i < this.Children.Count; i++)
			{
				this.Children[i].HandlePowerUpdate(isOn);
			}
		}
	}

	public override void SetValuesFromBlock()
	{
		base.SetValuesFromBlock();
		Block block = Block.list[(int)this.BlockID];
		if (block.Properties.Values.ContainsKey("RequiredPower"))
		{
			this.RequiredPower = ushort.Parse(block.Properties.Values["RequiredPower"]);
		}
		if (block.Properties.Values.ContainsKey("StartSound"))
		{
			this.StartSound = block.Properties.Values["StartSound"];
		}
		if (block.Properties.Values.ContainsKey("EndSound"))
		{
			this.EndSound = block.Properties.Values["EndSound"];
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void IsPoweredChanged(bool newPowered)
	{
		Manager.BroadcastPlay(this.Position.ToVector3(), newPowered ? this.StartSound : this.EndSound, 0f);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public string StartSound = "";

	[PublicizedFrom(EAccessModifier.Protected)]
	public string EndSound = "";

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool lastActivate;
}
