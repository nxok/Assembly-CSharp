using System;

public class PowerConsumerSingle : PowerItem
{
	public override PowerItem.PowerItemTypes PowerItemType
	{
		get
		{
			return PowerItem.PowerItemTypes.ConsumerToggle;
		}
	}

	public override void HandlePowerUpdate(bool isOn)
	{
		bool isPowered = this.isPowered;
		if (isPowered && this.lastActivate != isPowered && this.TileEntity != null)
		{
			this.TileEntity.ActivateOnce();
		}
		this.lastActivate = isPowered;
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
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool lastActivate;
}
