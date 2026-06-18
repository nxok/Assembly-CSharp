using System;

public class TileEntityPoweredBlock : TileEntityPowered
{
	public TileEntityPoweredBlock(Chunk _chunk)
		: base(_chunk)
	{
	}

	public bool IsToggled
	{
		get
		{
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && this.PowerItem is PowerConsumerToggle)
			{
				return (this.PowerItem as PowerConsumerToggle).IsToggled;
			}
			return this.isToggled;
		}
		set
		{
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				if (this.PowerItem is PowerConsumerToggle)
				{
					(this.PowerItem as PowerConsumerToggle).IsToggled = value;
				}
				this.isToggled = value;
				base.SetModified();
				return;
			}
			this.isToggled = value;
			base.SetModified();
		}
	}

	public override int PowerUsed
	{
		get
		{
			if (this.IsToggled)
			{
				return base.PowerUsed;
			}
			return 0;
		}
	}

	public override void UpdateTick(World world)
	{
		base.UpdateTick(world);
	}

	public override bool Activate(bool activated)
	{
		World world = GameManager.Instance.World;
		BlockValue block = this.chunk.GetBlock(base.localChunkPos);
		return block.Block.ActivateBlock(world, base.ToWorldPos(), block, activated, activated);
	}

	public override bool ActivateOnce()
	{
		World world = GameManager.Instance.World;
		BlockValue block = this.chunk.GetBlock(base.localChunkPos);
		return block.Block.ActivateBlockOnce(world, base.ToWorldPos(), block);
	}

	public override void OnRemove(World world)
	{
		base.OnRemove(world);
		if (PowerManager.Instance.ClientUpdateList.Contains(this))
		{
			PowerManager.Instance.ClientUpdateList.Remove(this);
		}
	}

	public override void OnUnload(World world)
	{
		base.OnUnload(world);
		if (PowerManager.Instance.ClientUpdateList.Contains(this))
		{
			PowerManager.Instance.ClientUpdateList.Remove(this);
		}
	}

	public override void OnSetLocalChunkPosition()
	{
		base.OnSetLocalChunkPosition();
		if (GameManager.Instance == null)
		{
			return;
		}
		World world = GameManager.Instance.World;
		if (this.chunk != null)
		{
			BlockValue block = this.chunk.GetBlock(base.localChunkPos);
			Block block2 = block.Block;
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				block2.ActivateBlock(world, base.ToWorldPos(), block, base.IsPowered, base.IsPowered);
			}
		}
	}

	public virtual void ClientUpdate()
	{
	}

	public override void read(PooledBinaryReader _br, TileEntity.StreamModeRead _eStreamMode)
	{
		base.read(_br, _eStreamMode);
		if (_eStreamMode == TileEntity.StreamModeRead.Persistency)
		{
			if (base.UseLocalVersioning())
			{
				_br.ReadUInt16();
			}
			else
			{
				base.GetLegacyForkVersion();
			}
		}
		if (_eStreamMode != TileEntity.StreamModeRead.Persistency)
		{
			if (_eStreamMode == TileEntity.StreamModeRead.FromClient)
			{
				this.isToggled = _br.ReadBoolean();
				if (this.PowerItem is PowerConsumerToggle)
				{
					(this.PowerItem as PowerConsumerToggle).IsToggled = this.isToggled;
					return;
				}
			}
			else
			{
				this.isToggled = _br.ReadBoolean();
			}
		}
	}

	public override void write(PooledBinaryWriter _bw, TileEntity.StreamModeWrite _eStreamMode)
	{
		base.write(_bw, _eStreamMode);
		if (_eStreamMode == TileEntity.StreamModeWrite.Persistency)
		{
			_bw.Write(18);
		}
		if (_eStreamMode != TileEntity.StreamModeWrite.Persistency)
		{
			_bw.Write(this.IsToggled);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public new const int Version = 18;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isToggled = true;

	public float DelayTime;

	[PublicizedFrom(EAccessModifier.Protected)]
	public float updateTime;
}
