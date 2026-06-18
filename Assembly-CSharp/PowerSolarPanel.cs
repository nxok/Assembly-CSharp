using System;
using System.IO;
using UnityEngine;

public class PowerSolarPanel : PowerSource
{
	public bool HasLight
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public override PowerItem.PowerItemTypes PowerItemType
	{
		get
		{
			return PowerItem.PowerItemTypes.SolarPanel;
		}
	}

	public override string OnSound
	{
		get
		{
			return "solarpanel_on";
		}
	}

	public override string OffSound
	{
		get
		{
			return "solarpanel_off";
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CheckLightLevel()
	{
		if (this.TileEntity != null)
		{
			Chunk chunk = this.TileEntity.GetChunk();
			Vector3i localChunkPos = this.TileEntity.localChunkPos;
			this.sunLight = chunk.GetLight(localChunkPos.x, localChunkPos.y, localChunkPos.z, Chunk.LIGHT_TYPE.SUN);
		}
		this.lastHasLight = this.HasLight;
		this.HasLight = this.sunLight == 15 && GameManager.Instance.World.IsDaytime();
		if (this.lastHasLight != this.HasLight)
		{
			this.HandleOnOffSound();
			if (!this.HasLight)
			{
				this.CurrentPower = 0;
				this.HandleDisconnect();
				return;
			}
			base.SendHasLocalChangesToRoot();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void TickPowerGeneration()
	{
		if (this.HasLight)
		{
			this.CurrentPower = this.MaxOutput;
		}
	}

	public override void HandleSendPower()
	{
		if (base.IsOn)
		{
			if (Time.time > this.lightUpdateTime)
			{
				this.lightUpdateTime = Time.time + 2f;
				this.CheckLightLevel();
			}
			if (this.HasLight)
			{
				if (this.CurrentPower < this.MaxPower)
				{
					this.TickPowerGeneration();
				}
				else if (this.CurrentPower > this.MaxPower)
				{
					this.CurrentPower = this.MaxPower;
				}
				if (this.ShouldAutoTurnOff())
				{
					this.CurrentPower = 0;
					base.IsOn = false;
				}
				if (this.hasChangesLocal)
				{
					this.LastPowerUsed = 0;
					ushort num = (ushort)Mathf.Min((int)this.MaxOutput, (int)this.CurrentPower);
					ushort num2 = num;
					World world = GameManager.Instance.World;
					for (int i = 0; i < this.Children.Count; i++)
					{
						num = num2;
						this.Children[i].HandlePowerReceived(ref num2);
						this.LastPowerUsed += num - num2;
					}
				}
				if (this.LastPowerUsed >= this.CurrentPower)
				{
					base.SendHasLocalChangesToRoot();
					this.CurrentPower = 0;
					return;
				}
				this.CurrentPower -= this.LastPowerUsed;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool ShouldClearPower()
	{
		return this.sunLight != 15 || !GameManager.Instance.World.IsDaytime();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void RefreshPowerStats()
	{
		base.RefreshPowerStats();
		this.MaxPower = this.MaxOutput;
	}

	public override void read(BinaryReader _br, byte _version)
	{
		base.read(_br, _version);
		if (PowerManager.Instance.CurrentFileVersion >= 2)
		{
			this.sunLight = _br.ReadByte();
		}
	}

	public override void write(BinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write(this.sunLight);
	}

	public ushort InputFromSun;

	[PublicizedFrom(EAccessModifier.Private)]
	public byte sunLight;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool lastHasLight;

	[PublicizedFrom(EAccessModifier.Private)]
	public string runningSound = "solarpanel_idle";

	[PublicizedFrom(EAccessModifier.Private)]
	public float lightUpdateTime;
}
