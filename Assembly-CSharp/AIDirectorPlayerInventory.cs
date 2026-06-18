using System;
using System.Collections.Generic;
using System.IO;

public struct AIDirectorPlayerInventory : IEquatable<AIDirectorPlayerInventory>
{
	public static AIDirectorPlayerInventory FromEntity(EntityAlive entity)
	{
		AIDirectorPlayerInventory aidirectorPlayerInventory;
		aidirectorPlayerInventory.bag = AIDirectorPlayerInventory.TrackedItemsFromBag(entity.bag);
		aidirectorPlayerInventory.belt = AIDirectorPlayerInventory.TrackedItemsFromInventory(entity.inventory);
		return aidirectorPlayerInventory;
	}

	public bool Equals(AIDirectorPlayerInventory other)
	{
		return this.bag != null == (other.bag != null) && this.belt != null == (other.belt != null) && AIDirectorPlayerInventory.OrderIndependantEquals(this.bag, other.bag) && AIDirectorPlayerInventory.OrderIndependantEquals(this.belt, other.belt);
	}

	public static List<AIDirectorPlayerInventory.ItemId> TrackedItemsFromBag(Bag bag)
	{
		return null;
	}

	public static List<AIDirectorPlayerInventory.ItemId> TrackedItemsFromInventory(Inventory inv)
	{
		return null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void AppendId(List<AIDirectorPlayerInventory.ItemId> list, AIDirectorPlayerInventory.ItemId id)
	{
		for (int i = 0; i < list.Count; i++)
		{
			AIDirectorPlayerInventory.ItemId itemId = list[i];
			if (itemId.id == id.id)
			{
				itemId.count += id.count;
				list[i] = itemId;
				return;
			}
		}
		list.Add(id);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool OrderIndependantEquals(List<AIDirectorPlayerInventory.ItemId> a, List<AIDirectorPlayerInventory.ItemId> b)
	{
		if (a == null && b == null)
		{
			return true;
		}
		if (a == null || b == null)
		{
			return false;
		}
		if (a.Count != b.Count)
		{
			return false;
		}
		for (int i = 0; i < a.Count; i++)
		{
			AIDirectorPlayerInventory.ItemId itemId = a[i];
			if (!b.Contains(itemId))
			{
				return false;
			}
		}
		return true;
	}

	public List<AIDirectorPlayerInventory.ItemId> bag;

	public List<AIDirectorPlayerInventory.ItemId> belt;

	public struct ItemId : IEquatable<AIDirectorPlayerInventory.ItemId>
	{
		public static AIDirectorPlayerInventory.ItemId FromStack(ItemStack stack)
		{
			AIDirectorPlayerInventory.ItemId itemId;
			itemId.id = stack.itemValue.type;
			itemId.count = stack.count;
			return itemId;
		}

		public static AIDirectorPlayerInventory.ItemId Read(BinaryReader stream)
		{
			AIDirectorPlayerInventory.ItemId itemId;
			itemId.id = (int)stream.ReadInt16();
			itemId.count = (int)stream.ReadInt16();
			return itemId;
		}

		public void Write(BinaryWriter stream)
		{
			stream.Write((short)this.id);
			stream.Write((short)this.count);
		}

		public bool Equals(AIDirectorPlayerInventory.ItemId other)
		{
			return this.id == other.id && this.count == other.count;
		}

		public const int kNetworkSize = 4;

		public int id;

		public int count;
	}
}
