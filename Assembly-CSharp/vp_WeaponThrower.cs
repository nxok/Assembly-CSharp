using System;
using UnityEngine;

public class vp_WeaponThrower : MonoBehaviour
{
	public Transform Transform
	{
		get
		{
			if (this.m_Transform == null)
			{
				this.m_Transform = base.transform;
			}
			return this.m_Transform;
		}
	}

	public Transform Root
	{
		get
		{
			if (this.m_Root == null)
			{
				this.m_Root = this.Transform.root;
			}
			return this.m_Root;
		}
	}

	public vp_Weapon Weapon
	{
		get
		{
			if (this.m_Weapon == null)
			{
				this.m_Weapon = (vp_Weapon)this.Transform.GetComponent(typeof(vp_Weapon));
			}
			return this.m_Weapon;
		}
	}

	public vp_WeaponShooter Shooter
	{
		get
		{
			if (this.m_Shooter == null)
			{
				this.m_Shooter = (vp_WeaponShooter)this.Transform.GetComponent(typeof(vp_WeaponShooter));
			}
			return this.m_Shooter;
		}
	}

	public vp_UnitBankType UnitBankType
	{
		get
		{
			if (this.ItemIdentifier == null)
			{
				return null;
			}
			vp_ItemType itemType = this.m_ItemIdentifier.GetItemType();
			if (itemType == null)
			{
				return null;
			}
			vp_UnitBankType vp_UnitBankType = itemType as vp_UnitBankType;
			if (vp_UnitBankType == null)
			{
				return null;
			}
			return vp_UnitBankType;
		}
	}

	public vp_UnitBankInstance UnitBank
	{
		get
		{
			if (this.m_UnitBank == null && this.UnitBankType != null && this.Inventory != null)
			{
				foreach (vp_UnitBankInstance vp_UnitBankInstance in this.Inventory.UnitBankInstances)
				{
					if (vp_UnitBankInstance.UnitType == this.UnitBankType.Unit)
					{
						this.m_UnitBank = vp_UnitBankInstance;
					}
				}
			}
			return this.m_UnitBank;
		}
	}

	public vp_ItemIdentifier ItemIdentifier
	{
		get
		{
			if (this.m_ItemIdentifier == null)
			{
				this.m_ItemIdentifier = (vp_ItemIdentifier)this.Transform.GetComponent(typeof(vp_ItemIdentifier));
			}
			return this.m_ItemIdentifier;
		}
	}

	public vp_PlayerEventHandler Player
	{
		get
		{
			if (this.m_Player == null)
			{
				this.m_Player = (vp_PlayerEventHandler)this.Root.GetComponentInChildren(typeof(vp_PlayerEventHandler));
			}
			return this.m_Player;
		}
	}

	public vp_PlayerInventory Inventory
	{
		get
		{
			if (this.m_Inventory == null)
			{
				this.m_Inventory = (vp_PlayerInventory)this.Root.GetComponentInChildren(typeof(vp_PlayerInventory));
			}
			return this.m_Inventory;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnEnable()
	{
		if (this.Player == null)
		{
			return;
		}
		this.Player.Register(this);
		this.TryStoreAttackMinDuration();
		this.Inventory.SetItemCap(this.ItemIdentifier.Type, 1, true);
		this.Inventory.CapsEnabled = true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnDisable()
	{
		this.TryRestoreAttackMinDuration();
		if (this.Player != null)
		{
			this.Player.Unregister(this);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Start()
	{
		this.TryStoreAttackMinDuration();
		if (this.Weapon == null)
		{
			Debug.LogError("Throwing weapon setup error (" + ((this != null) ? this.ToString() : null) + ") requires a vp_Weapon or vp_FPWeapon component.");
			return;
		}
		if (this.UnitBankType == null)
		{
			Debug.LogError("Throwing weapon setup error (" + ((this != null) ? this.ToString() : null) + ") requires a vp_ItemIdentifier component with a valid UnitBank.");
			return;
		}
		if (this.Weapon.AnimationType != 3)
		{
			string[] array = new string[5];
			array[0] = "Throwing weapon setup error (";
			array[1] = ((this != null) ? this.ToString() : null);
			array[2] = ") Please set 'Animation -> Type' of '";
			int num = 3;
			vp_Weapon weapon = this.Weapon;
			array[num] = ((weapon != null) ? weapon.ToString() : null);
			array[4] = "' item type to 'Thrown'.";
			Debug.LogError(string.Concat(array));
		}
		if (this.UnitBankType.Capacity != 1)
		{
			Debug.LogError(string.Concat(new string[]
			{
				"Throwing weapon setup error (",
				(this != null) ? this.ToString() : null,
				") Please set 'Capacity' for the '",
				this.UnitBankType.name,
				"' item type to '1'."
			}));
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TryStoreAttackMinDuration()
	{
		if (this.Player.Attack == null)
		{
			return;
		}
		if (this.m_OriginalAttackMinDuration == 0f)
		{
			return;
		}
		this.m_OriginalAttackMinDuration = this.Player.Attack.MinDuration;
		this.Player.Attack.MinDuration = this.AttackMinDuration;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TryRestoreAttackMinDuration()
	{
		if (this.Player.Attack == null)
		{
			return;
		}
		if (this.m_OriginalAttackMinDuration != 0f)
		{
			return;
		}
		this.Player.Attack.MinDuration = this.m_OriginalAttackMinDuration;
	}

	public bool HaveAmmoForCurrentWeapon
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return this.Player.CurrentWeaponAmmoCount.Get() > 0 || this.Player.CurrentWeaponClipCount.Get() > 0;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual bool TryReload()
	{
		return this.UnitBank != null && this.Inventory.TryReload(this.UnitBank);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnStart_Attack()
	{
		if (!this.Player.IsFirstPerson.Get())
		{
			vp_Timer.In(this.Shooter.ProjectileSpawnDelay, delegate
			{
				this.Weapon.Weapon3rdPersonModel.GetComponent<Renderer>().enabled = false;
			}, null);
			vp_Timer.In(this.Shooter.ProjectileSpawnDelay + 1f, delegate
			{
				if (this.HaveAmmoForCurrentWeapon)
				{
					this.Weapon.Weapon3rdPersonModel.GetComponent<Renderer>().enabled = true;
				}
			}, null);
		}
		if (this.Player.CurrentWeaponAmmoCount.Get() < 1)
		{
			this.TryReload();
		}
		vp_Timer.In(this.Shooter.ProjectileSpawnDelay + 0.5f, delegate
		{
			this.Player.Attack.Stop(0f);
		}, null);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual bool CanStart_Reload()
	{
		return false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnStop_Attack()
	{
		this.TryReload();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnStop_SetWeapon()
	{
		this.m_UnitBank = null;
	}

	public float AttackMinDuration = 1f;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float m_OriginalAttackMinDuration;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Transform m_Transform;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Transform m_Root;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public vp_Weapon m_Weapon;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public vp_WeaponShooter m_Shooter;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public vp_UnitBankType m_UnitBankType;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public vp_UnitBankInstance m_UnitBank;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public vp_ItemIdentifier m_ItemIdentifier;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public vp_PlayerEventHandler m_Player;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public vp_PlayerInventory m_Inventory;
}
