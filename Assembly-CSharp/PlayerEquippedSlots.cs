using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerEquippedSlots : MonoBehaviour
{
	public void Init(Transform outfit)
	{
		this.outfitXF = outfit;
		if (this.outfitXF != null)
		{
			this._DisableAllNASubmeshes();
		}
	}

	public void ListParts()
	{
		int count = this.parts.Count;
		for (int i = 0; i < count; i++)
		{
			Log.Warning(this.parts[i].name);
		}
	}

	public void ListEquipment()
	{
		int count = this.equippedParts.Count;
		for (int i = 0; i < count; i++)
		{
			Log.Warning(this.equippedParts[i].name);
		}
	}

	public bool IsEquipped(string partName)
	{
		int count = this.equippedParts.Count;
		for (int i = 0; i < count; i++)
		{
			if (this.equippedParts[i].name == partName)
			{
				return true;
			}
		}
		return false;
	}

	public bool Equip(string partName)
	{
		if (this.IsEquipped(partName))
		{
			return false;
		}
		int count = this.parts.Count;
		for (int i = 0; i < count; i++)
		{
			PlayerEquippedSlots.PartInfo partInfo = this.parts[i];
			if (partInfo.name == partName)
			{
				PlayerEquippedSlots.EquippedPart equippedPart = new PlayerEquippedSlots.EquippedPart();
				equippedPart.name = partName;
				equippedPart.partInfo = partInfo;
				this.equippedParts.Add(equippedPart);
				this._RunRules();
				return true;
			}
		}
		Log.Warning("Part '{0}' not equipped.", new object[] { partName });
		return false;
	}

	public bool UnEquip(string partName)
	{
		int count = this.equippedParts.Count;
		for (int i = 0; i < count; i++)
		{
			if (this.equippedParts[i].name == partName)
			{
				this._EnableNASubmesh(partName, false);
				this.equippedParts.RemoveAt(i);
				this._RunRules();
				return true;
			}
		}
		Log.Warning("Part '{0}' not unequipped.", new object[] { partName });
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void _RunRules()
	{
		int count = this.equippedParts.Count;
		for (int i = 0; i < count; i++)
		{
			PlayerEquippedSlots.EquippedPart equippedPart = this.equippedParts[i];
			equippedPart.wasShowing = equippedPart.isShowing;
			equippedPart.isShowing = true;
		}
		for (int j = 0; j < count; j++)
		{
			PlayerEquippedSlots.EquippedPart equippedPart2 = this.equippedParts[j];
			if (equippedPart2.isShowing)
			{
				PlayerEquippedSlots.PartInfo partInfo = equippedPart2.partInfo;
				if (!string.IsNullOrEmpty(partInfo.rule))
				{
					string rule = partInfo.rule;
					for (int k = 0; k < count; k++)
					{
						if (k != j)
						{
							PlayerEquippedSlots.EquippedPart equippedPart3 = this.equippedParts[k];
							if (equippedPart3.isShowing)
							{
								PlayerEquippedSlots.PartInfo partInfo2 = equippedPart3.partInfo;
								if (partInfo2.IsInSlot(rule))
								{
									Log.Warning(" Note: Part {0} hides part {1} with rule {2}.", new object[] { partInfo.name, partInfo2.name, rule });
									equippedPart3.isShowing = false;
								}
							}
						}
					}
				}
			}
		}
		for (int l = 0; l < count; l++)
		{
			PlayerEquippedSlots.EquippedPart equippedPart4 = this.equippedParts[l];
			if (equippedPart4.isShowing && !equippedPart4.wasShowing)
			{
				this._EnableNASubmesh(equippedPart4.partInfo.name, true);
			}
			else if (!equippedPart4.isShowing && equippedPart4.wasShowing)
			{
				this._EnableNASubmesh(equippedPart4.partInfo.name, false);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Transform _GetNAOutfit()
	{
		return this.outfitXF;
	}

	public void _EnableNASubmesh(string submeshName, bool enable)
	{
		Transform transform = this._GetNAOutfit();
		if (transform == null)
		{
			return;
		}
		int childCount = transform.childCount;
		for (int i = 0; i < childCount; i++)
		{
			Transform child = transform.GetChild(i);
			if (!(child.name == "Origin"))
			{
				GameObject gameObject = child.gameObject;
				if (gameObject.name == submeshName)
				{
					gameObject.SetActive(enable);
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void _DisableAllNASubmeshes()
	{
		Transform transform = this._GetNAOutfit();
		if (transform == null)
		{
			return;
		}
		int childCount = transform.childCount;
		for (int i = 0; i < childCount; i++)
		{
			Transform child = transform.GetChild(i);
			if (!(child.name == "Origin"))
			{
				child.gameObject.SetActive(false);
			}
		}
	}

	public List<PlayerEquippedSlots.PartInfo> parts;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public List<PlayerEquippedSlots.EquippedPart> equippedParts = new List<PlayerEquippedSlots.EquippedPart>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Transform outfitXF;

	[Serializable]
	public class PartInfo
	{
		public bool IsInSlot(string slotReference)
		{
			return PlayerEquippedSlots.PartInfo.RefMatchesSlot(slotReference, this.slot);
		}

		public static bool RefMatchesSlot(string slotReference, string slotName)
		{
			int num = slotReference.IndexOf("*");
			if (num == -1)
			{
				return slotReference.Equals(slotName);
			}
			return string.Compare(slotReference, 0, slotName, 0, num) == 0;
		}

		public string name;

		public string slot;

		public string rule;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public class EquippedPart
	{
		public string name;

		public PlayerEquippedSlots.PartInfo partInfo;

		public bool wasShowing;

		public bool isShowing;
	}
}
