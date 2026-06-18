using System;
using System.Globalization;
using System.IO;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class EntityLootContainer : EntityItem
{
	public override string LocalizedEntityName
	{
		get
		{
			if (!string.IsNullOrEmpty(this.OverrideName))
			{
				return Localization.Get(this.OverrideName, false, null);
			}
			return base.LocalizedEntityName;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Start()
	{
		base.Start();
		foreach (Collider collider in base.transform.GetComponentsInChildren<Collider>())
		{
			collider.gameObject.tag = "E_BP_Body";
			collider.enabled = true;
			collider.gameObject.layer = 13;
			NGUITools.AddMissingComponent<RootTransformRefEntity>(collider.gameObject).RootTransform = base.transform;
		}
		this.SetDead();
	}

	public void SetContent(ItemStack[] _inventory)
	{
		this.SetContent(_inventory, 0);
	}

	public void SetContent(ItemStack[] _inventory, int _slotCountOverride)
	{
		int num = 0;
		if (_slotCountOverride > 0)
		{
			num = _slotCountOverride;
		}
		else
		{
			LootContainer lootContainer = LootContainer.GetLootContainer(this.GetLootList(), true);
			if (lootContainer != null)
			{
				Vector2i size = lootContainer.size;
				num = size.x * size.y;
			}
			num = Math.Max(num, (_inventory != null) ? _inventory.Length : 0);
		}
		if (num <= 0)
		{
			return;
		}
		ItemStack[] array = ItemStack.CreateArray(num);
		int num2 = 0;
		while (_inventory != null && num2 < _inventory.Length && num2 < array.Length)
		{
			array[num2] = _inventory[num2].Clone();
			num2++;
		}
		this.bag.SetSlots(array);
		this.bag.Touched = true;
	}

	public override void CopyPropertiesFromEntityClass()
	{
		base.CopyPropertiesFromEntityClass();
		EntityClass entityClass = EntityClass.list[this.entityClass];
		if (entityClass.Properties.Values.ContainsKey(EntityClass.PropTimeStayAfterDeath))
		{
			this.timeStayAfterDeath = (int)(StringParsers.ParseFloat(entityClass.Properties.Values[EntityClass.PropTimeStayAfterDeath], 0, -1, NumberStyles.Any) * 20f);
			return;
		}
		this.timeStayAfterDeath = 100;
	}

	public override void OnUpdateEntity()
	{
		base.OnUpdateEntity();
		try
		{
			if (!(SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer ? LockManager.Instance.IsLockedServer(this, 0) : LockManager.Instance.IsLockedByLocalPlayer(this, 0)))
			{
				if (this.bag.Touched && this.bag.IsEmpty())
				{
					this.removeBackpack();
				}
				else if (this.deathUpdateTime >= this.timeStayAfterDeath - 1)
				{
					this.removeBackpack();
				}
				else
				{
					this.deathUpdateTime++;
				}
			}
		}
		finally
		{
		}
	}

	public override bool AllowActivationCommand(ReadOnlySpan<char> _commandName, EntityPlayerLocal _playerFocusing)
	{
		if (base.CommandIs(_commandName, "search"))
		{
			return this.bag != null;
		}
		return base.AllowActivationCommand(_commandName, _playerFocusing);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void removeBackpack()
	{
		this.deathUpdateTime = this.timeStayAfterDeath;
		this.bRemoved = true;
		this.MarkToUnload();
	}

	public override int DamageEntity(DamageSource _damageSource, int _strength, bool _criticalHit, float impulseScale = 1f)
	{
		if (_strength >= 99999)
		{
			this.removeBackpack();
		}
		return base.DamageEntity(_damageSource, _strength, _criticalHit, impulseScale);
	}

	public override bool IsMarkedForUnload()
	{
		return base.IsMarkedForUnload() && this.bRemoved;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void createMesh()
	{
	}

	public override string GetLootList()
	{
		if (!(this.OverrideLootList != ""))
		{
			return this.lootList;
		}
		return this.OverrideLootList;
	}

	public override void Write(BinaryWriter _bw, bool _bNetworkWrite)
	{
		base.Write(_bw, _bNetworkWrite);
		_bw.Write(this.OverrideLootList);
		_bw.Write(this.OverrideName);
		this.bag.Write(_bw);
	}

	public override void Read(byte _version, BinaryReader _br)
	{
		base.Read(_version, _br);
		if (_version < 35)
		{
			int num = (int)_br.ReadUInt16();
			ItemStack[] array = ItemStack.CreateArray(num);
			for (int i = 0; i < num; i++)
			{
				array[i].Read(_br);
			}
			num = (int)_br.ReadUInt16();
			for (int j = 0; j < num; j++)
			{
				new ItemStack().Read(_br);
			}
			if (_version >= 30)
			{
				this.OverrideLootList = _br.ReadString();
				this.OverrideName = _br.ReadString();
			}
			LootContainer lootContainer = LootContainer.GetLootContainer(this.GetLootList(), true);
			int num2 = array.Length;
			if (lootContainer != null)
			{
				num2 = Math.Max(num2, lootContainer.size.x * lootContainer.size.y);
			}
			if (num2 > array.Length)
			{
				ItemStack[] array2 = ItemStack.CreateArray(num2);
				for (int k = 0; k < array.Length; k++)
				{
					array2[k] = array[k];
				}
				array = array2;
			}
			this.bag.SetSlots(array);
			return;
		}
		if (_version >= 30)
		{
			this.OverrideLootList = _br.ReadString();
			this.OverrideName = _br.ReadString();
		}
		if (_version >= 35)
		{
			this.bag.ReadInto(_br);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void HandleNavObject()
	{
		if (EntityClass.list[this.entityClass].NavObject != "")
		{
			this.NavObject = NavObjectManager.Instance.RegisterNavObject(EntityClass.list[this.entityClass].NavObject, this, "", false);
		}
	}

	public override string ToString()
	{
		return string.Format("[type={0}, name={1}]", base.GetType().Name, (this.itemClass != null) ? this.itemClass.GetItemName() : "?");
	}

	public override void OnUnlockedServer(int _unlockingPlayerId, ushort _channel)
	{
		if (this.bag.IsEmpty())
		{
			this.KillLootContainer();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int deathUpdateTime;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int timeStayAfterDeath;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool bRemoved;

	public string OverrideLootList = "";

	public string OverrideName = "";
}
