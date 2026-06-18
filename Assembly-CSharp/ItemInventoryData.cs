using System;
using System.Collections.Generic;
using UnityEngine;

public class ItemInventoryData
{
	public Transform model
	{
		get
		{
			return this.holdingEntity.inventory.models[this.slotIdx];
		}
	}

	public ItemValue itemValue
	{
		get
		{
			return this.holdingEntity.inventory[this.slotIdx];
		}
		set
		{
			this.holdingEntity.inventory[this.slotIdx] = value;
		}
	}

	public void Changed()
	{
		this.holdingEntity.inventory.Changed();
	}

	public ItemInventoryData(ItemClass _item, ItemStack _itemStack, IGameManager _gameManager, EntityAlive _holdingEntity, int _slotIdx)
	{
		this.item = _item;
		this.itemStack = _itemStack;
		this.world = _holdingEntity.world;
		this.holdingEntity = _holdingEntity;
		this.gameManager = _gameManager;
		this.slotIdx = _slotIdx;
		this.actionData = new List<ItemActionData>();
		this.actionData.Add(null);
		this.actionData.Add(null);
	}

	public ItemClass item;

	public ItemStack itemStack;

	public readonly EntityAlive holdingEntity;

	public int holdingEntitySoundID = -2;

	public World world;

	public IGameManager gameManager;

	public List<ItemActionData> actionData;

	public int slotIdx;

	public enum SoundPlayType
	{
		None = -2,
		IdleReady,
		Idle
	}
}
