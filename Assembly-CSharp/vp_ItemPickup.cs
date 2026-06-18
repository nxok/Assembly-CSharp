using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
[Serializable]
public class vp_ItemPickup : MonoBehaviour
{
	public Type ItemType
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			if (this.m_ItemType == null)
			{
				this.m_ItemType = this.m_Item.Type.GetType();
			}
			return this.m_ItemType;
		}
	}

	public vp_ItemType ItemTypeObject
	{
		get
		{
			if (this.m_ItemTypeObject == null)
			{
				this.m_ItemTypeObject = this.m_Item.Type;
			}
			return this.m_ItemTypeObject;
		}
	}

	public AudioSource Audio
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			if (this.m_Audio == null)
			{
				if (base.GetComponent<AudioSource>() == null)
				{
					base.gameObject.AddComponent<AudioSource>();
				}
				this.m_Audio = base.GetComponent<AudioSource>();
			}
			return this.m_Audio;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Awake()
	{
		if (this.ItemType == typeof(vp_UnitType))
		{
			this.Amount = Mathf.Max(1, this.Amount);
		}
		base.GetComponent<Collider>().isTrigger = true;
		this.m_Rigidbody = base.GetComponent<Rigidbody>();
		this.m_Transform = base.transform;
		if (this.m_Sound.PickupSound != null || this.m_Sound.PickupFailSound != null)
		{
			this.Audio.clip = this.m_Sound.PickupSound;
			this.Audio.playOnAwake = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Update()
	{
		if (this.m_Depleted && !this.Audio.isPlaying)
		{
			base.SendMessage("Die", 1);
		}
		if (!this.m_Depleted && this.m_Rigidbody != null && this.m_Rigidbody.IsSleeping() && !this.m_Rigidbody.isKinematic)
		{
			vp_Timer.In(0.5f, delegate
			{
				this.m_Rigidbody.isKinematic = true;
				foreach (Collider collider in base.GetComponents<Collider>())
				{
					if (!collider.isTrigger)
					{
						collider.enabled = false;
					}
				}
			}, null);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnEnable()
	{
		if (this.m_Rigidbody != null)
		{
			this.m_Rigidbody.isKinematic = false;
			foreach (Collider collider in base.GetComponents<Collider>())
			{
				if (!collider.isTrigger)
				{
					collider.enabled = true;
				}
			}
		}
		base.GetComponent<Renderer>().enabled = true;
		this.m_Depleted = false;
		this.m_AlreadyFailed = false;
		vp_GlobalEvent<vp_ItemPickup>.Send("NetworkRespawnPickup", this);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnTriggerEnter(Collider col)
	{
		if (this.ItemType == null)
		{
			return;
		}
		if (!vp_Gameplay.isMaster)
		{
			return;
		}
		if (!base.GetComponent<Collider>().enabled)
		{
			return;
		}
		this.TryGiveTo(col);
	}

	public void TryGiveTo(Collider col)
	{
		if (this.m_Depleted)
		{
			return;
		}
		vp_Inventory vp_Inventory;
		if (!vp_ItemPickup.m_ColliderInventories.TryGetValue(col, out vp_Inventory))
		{
			vp_Inventory = vp_TargetEventReturn<vp_Inventory>.SendUpwards(col, "GetInventory", vp_TargetEventOptions.DontRequireReceiver);
			vp_ItemPickup.m_ColliderInventories.Add(col, vp_Inventory);
		}
		if (vp_Inventory == null)
		{
			return;
		}
		if (this.m_Recipient.Tags.Count > 0 && !this.m_Recipient.Tags.Contains(col.gameObject.tag))
		{
			return;
		}
		bool flag = false;
		int num = vp_TargetEventReturn<vp_ItemType, int>.SendUpwards(col, "GetItemCount", this.m_Item.Type, vp_TargetEventOptions.DontRequireReceiver);
		if (this.ItemType == typeof(vp_ItemType))
		{
			flag = vp_TargetEventReturn<vp_ItemType, int, bool>.SendUpwards(col, "TryGiveItem", this.m_Item.Type, this.ID, vp_TargetEventOptions.DontRequireReceiver);
		}
		else if (this.ItemType == typeof(vp_UnitBankType))
		{
			flag = vp_TargetEventReturn<vp_UnitBankType, int, int, bool>.SendUpwards(col, "TryGiveUnitBank", this.m_Item.Type as vp_UnitBankType, this.Amount, this.ID, vp_TargetEventOptions.DontRequireReceiver);
		}
		else if (this.ItemType == typeof(vp_UnitType))
		{
			flag = vp_TargetEventReturn<vp_UnitType, int, bool>.SendUpwards(col, "TryGiveUnits", this.m_Item.Type as vp_UnitType, this.Amount, vp_TargetEventOptions.DontRequireReceiver);
		}
		else if (this.ItemType.BaseType == typeof(vp_ItemType))
		{
			flag = vp_TargetEventReturn<vp_ItemType, int, bool>.SendUpwards(col, "TryGiveItem", this.m_Item.Type, this.ID, vp_TargetEventOptions.DontRequireReceiver);
		}
		else if (this.ItemType.BaseType == typeof(vp_UnitBankType))
		{
			flag = vp_TargetEventReturn<vp_UnitBankType, int, int, bool>.SendUpwards(col, "TryGiveUnitBank", this.m_Item.Type as vp_UnitBankType, this.Amount, this.ID, vp_TargetEventOptions.DontRequireReceiver);
		}
		else if (this.ItemType.BaseType == typeof(vp_UnitType))
		{
			flag = vp_TargetEventReturn<vp_UnitType, int, bool>.SendUpwards(col, "TryGiveUnits", this.m_Item.Type as vp_UnitType, this.Amount, vp_TargetEventOptions.DontRequireReceiver);
		}
		if (flag)
		{
			this.m_PickedUpAmount = vp_TargetEventReturn<vp_ItemType, int>.SendUpwards(col, "GetItemCount", this.m_Item.Type, vp_TargetEventOptions.DontRequireReceiver) - num;
			this.OnSuccess(col.transform);
			return;
		}
		this.OnFail(col.transform);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnTriggerExit()
	{
		this.m_AlreadyFailed = false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnSuccess(Transform recipient)
	{
		this.m_Depleted = true;
		if (this.m_Sound.PickupSound != null)
		{
			this.Audio.pitch = (this.m_Sound.PickupSoundSlomo ? Time.timeScale : 1f);
			this.Audio.Play();
		}
		base.GetComponent<Renderer>().enabled = false;
		string text;
		if (this.m_PickedUpAmount < 2 || this.ItemType == typeof(vp_UnitBankType) || this.ItemType.BaseType == typeof(vp_UnitBankType))
		{
			text = string.Format(this.m_Messages.SuccessSingle, new object[]
			{
				this.m_Item.Type.IndefiniteArticle,
				this.m_Item.Type.DisplayName,
				this.m_Item.Type.DisplayNameFull,
				this.m_Item.Type.Description,
				this.m_PickedUpAmount.ToString()
			});
		}
		else
		{
			text = string.Format(this.m_Messages.SuccessMultiple, new object[]
			{
				this.m_Item.Type.IndefiniteArticle,
				this.m_Item.Type.DisplayName,
				this.m_Item.Type.DisplayNameFull,
				this.m_Item.Type.Description,
				this.m_PickedUpAmount.ToString()
			});
		}
		vp_GlobalEvent<Transform, string>.Send("HUDText", recipient, text);
		if (vp_Gameplay.isMultiplayer && vp_Gameplay.isMaster)
		{
			vp_GlobalEvent<vp_ItemPickup, Transform>.Send("NetworkGivePickup", this, recipient);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Die()
	{
		vp_Utility.Activate(base.gameObject, false);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnFail(Transform recipient)
	{
		if (!this.m_AlreadyFailed && this.m_Sound.PickupFailSound != null)
		{
			this.Audio.pitch = (this.m_Sound.FailSoundSlomo ? Time.timeScale : 1f);
			this.Audio.PlayOneShot(this.m_Sound.PickupFailSound);
		}
		this.m_AlreadyFailed = true;
		string text;
		if (this.m_PickedUpAmount < 2 || this.ItemType == typeof(vp_UnitBankType) || this.ItemType.BaseType == typeof(vp_UnitBankType))
		{
			text = string.Format(this.m_Messages.FailSingle, new object[]
			{
				this.m_Item.Type.IndefiniteArticle,
				this.m_Item.Type.DisplayName,
				this.m_Item.Type.DisplayNameFull,
				this.m_Item.Type.Description,
				this.Amount.ToString()
			});
		}
		else
		{
			text = string.Format(this.m_Messages.FailMultiple, new object[]
			{
				this.m_Item.Type.IndefiniteArticle,
				this.m_Item.Type.DisplayName,
				this.m_Item.Type.DisplayNameFull,
				this.m_Item.Type.Description,
				this.Amount.ToString()
			});
		}
		vp_GlobalEvent<Transform, string>.Send("HUDText", recipient, text);
	}

	public int ID;

	public int Amount;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Type m_ItemType;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public vp_ItemType m_ItemTypeObject;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public AudioSource m_Audio;

	[SerializeField]
	[PublicizedFrom(EAccessModifier.Protected)]
	public vp_ItemPickup.ItemSection m_Item;

	[SerializeField]
	[PublicizedFrom(EAccessModifier.Protected)]
	public vp_ItemPickup.RecipientTagsSection m_Recipient;

	[SerializeField]
	[PublicizedFrom(EAccessModifier.Protected)]
	public vp_ItemPickup.SoundSection m_Sound;

	[SerializeField]
	[PublicizedFrom(EAccessModifier.Protected)]
	public vp_ItemPickup.MessageSection m_Messages;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public bool m_Depleted;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public int m_PickedUpAmount;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Rigidbody m_Rigidbody;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Transform m_Transform;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public string MissingItemTypeError = "Warning: {0} has no ItemType object!";

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public bool m_AlreadyFailed;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static Dictionary<Collider, vp_Inventory> m_ColliderInventories = new Dictionary<Collider, vp_Inventory>();

	[Serializable]
	public class ItemSection
	{
		public vp_ItemType Type;
	}

	[Serializable]
	public class RecipientTagsSection
	{
		public List<string> Tags = new List<string>();
	}

	[Serializable]
	public class SoundSection
	{
		public AudioClip PickupSound;

		public bool PickupSoundSlomo = true;

		public AudioClip PickupFailSound;

		public bool FailSoundSlomo = true;
	}

	[Serializable]
	public class MessageSection
	{
		public string SuccessSingle = "Picked up {2}.";

		public string SuccessMultiple = "Picked up {4} {1}s.";

		public string FailSingle = "Can't pick up {2} right now.";

		public string FailMultiple = "Can't pick up {4} {1}s right now.";
	}
}
