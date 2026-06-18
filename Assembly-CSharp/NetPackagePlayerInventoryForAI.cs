using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine.Scripting;

[Preserve]
public class NetPackagePlayerInventoryForAI : NetPackage
{
	public override NetPackageDirection PackageDirection
	{
		get
		{
			return NetPackageDirection.ToServer;
		}
	}

	public NetPackagePlayerInventoryForAI Setup(EntityAlive entity, AIDirectorPlayerInventory inventory)
	{
		this.m_entityId = entity.entityId;
		this.m_inventory = inventory;
		return this;
	}

	public override int GetLength()
	{
		int num = 8;
		if (this.m_inventory.bag != null)
		{
			num += 4 * this.m_inventory.bag.Count;
		}
		if (this.m_inventory.belt != null)
		{
			num += 4 * this.m_inventory.belt.Count;
		}
		return num;
	}

	public override void read(PooledBinaryReader _reader)
	{
		this.m_entityId = _reader.ReadInt32();
		this.m_inventory.bag = NetPackagePlayerInventoryForAI.ReadInventorySet(_reader);
		this.m_inventory.belt = NetPackagePlayerInventoryForAI.ReadInventorySet(_reader);
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write(this.m_entityId);
		NetPackagePlayerInventoryForAI.WriteInventorySet(_writer, this.m_inventory.bag);
		NetPackagePlayerInventoryForAI.WriteInventorySet(_writer, this.m_inventory.belt);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null || _world.aiDirector == null)
		{
			return;
		}
		AIDirectorPlayerInventory aidirectorPlayerInventory;
		aidirectorPlayerInventory.bag = this.m_inventory.bag;
		aidirectorPlayerInventory.belt = this.m_inventory.belt;
		_world.aiDirector.UpdatePlayerInventory(this.m_entityId, aidirectorPlayerInventory);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static List<AIDirectorPlayerInventory.ItemId> ReadInventorySet(BinaryReader stream)
	{
		List<AIDirectorPlayerInventory.ItemId> list = null;
		int num = (int)stream.ReadInt16();
		for (int i = 0; i < num; i++)
		{
			if (list == null)
			{
				list = new List<AIDirectorPlayerInventory.ItemId>();
			}
			list.Add(AIDirectorPlayerInventory.ItemId.Read(stream));
		}
		return list;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void WriteInventorySet(BinaryWriter stream, List<AIDirectorPlayerInventory.ItemId> items)
	{
		int num = ((items != null) ? items.Count : 0);
		stream.Write((short)num);
		if (num > 0)
		{
			for (int i = 0; i < items.Count; i++)
			{
				items[i].Write(stream);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int m_entityId;

	[PublicizedFrom(EAccessModifier.Private)]
	public AIDirectorPlayerInventory m_inventory;
}
