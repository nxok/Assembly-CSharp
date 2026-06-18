using System;
using UnityEngine.Scripting;

[Preserve]
public class AIDirectorPlayerState : IMemoryPoolableObject
{
	public AIDirectorPlayerState Construct(EntityPlayer _player)
	{
		this.Player = _player;
		this.m_dead = false;
		return this;
	}

	public void Reset()
	{
		this.Player = null;
	}

	public void Cleanup()
	{
	}

	public AIDirectorPlayerInventory Inventory
	{
		get
		{
			return this.m_inventory;
		}
		set
		{
			this.m_inventory = value;
		}
	}

	public bool Dead
	{
		get
		{
			return this.m_dead;
		}
		set
		{
			this.m_dead = value;
		}
	}

	public const float kCheckUndergroundTime = 5f;

	public const int kNumBlocksUnderground = 10;

	public EntityPlayer Player;

	[PublicizedFrom(EAccessModifier.Private)]
	public AIDirectorPlayerInventory m_inventory;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool m_dead;
}
