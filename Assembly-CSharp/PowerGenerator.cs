using System;
using System.Globalization;
using System.IO;

public class PowerGenerator : PowerSource
{
	public override PowerItem.PowerItemTypes PowerItemType
	{
		get
		{
			return PowerItem.PowerItemTypes.Generator;
		}
	}

	public override string OnSound
	{
		get
		{
			return "generator_start";
		}
	}

	public override string OffSound
	{
		get
		{
			return "generator_stop";
		}
	}

	public override void read(BinaryReader _br, byte _version)
	{
		base.read(_br, _version);
		this.CurrentFuel = _br.ReadUInt16();
	}

	public override void write(BinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write(this.CurrentFuel);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool ShouldAutoTurnOff()
	{
		return this.CurrentFuel <= 0;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void TickPowerGeneration()
	{
		if ((float)(this.MaxPower - this.CurrentPower) >= this.OutputPerFuel && this.CurrentFuel > 0)
		{
			this.CurrentFuel -= 1;
			this.CurrentPower += (ushort)this.OutputPerFuel;
		}
	}

	public override void SetValuesFromBlock()
	{
		base.SetValuesFromBlock();
		Block block = Block.list[(int)this.BlockID];
		if (block.Properties.Values.ContainsKey("MaxPower"))
		{
			this.MaxPower = ushort.Parse(block.Properties.Values["MaxPower"]);
		}
		if (block.Properties.Values.ContainsKey("MaxFuel"))
		{
			this.MaxFuel = ushort.Parse(block.Properties.Values["MaxFuel"]);
		}
		else
		{
			this.MaxFuel = 1000;
		}
		if (block.Properties.Values.ContainsKey("OutputPerFuel"))
		{
			this.OutputPerFuel = StringParsers.ParseFloat(block.Properties.Values["OutputPerFuel"], 0, -1, NumberStyles.Any);
			return;
		}
		this.OutputPerFuel = 100f;
	}

	public ushort CurrentFuel;

	public ushort MaxFuel;

	public float OutputPerFuel;
}
