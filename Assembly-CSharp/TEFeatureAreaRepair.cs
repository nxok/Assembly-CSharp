using System;
using System.Collections;
using System.Collections.Generic;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class TEFeatureAreaRepair : TEFeatureAbs
{
	public bool IsRepairing
	{
		get
		{
			return this.isRepairing;
		}
		set
		{
			this.isRepairing = value;
		}
	}

	public override void Init(TileEntityComposite _parent, TileEntityFeatureData _featureData)
	{
		base.Init(_parent, _featureData);
		this.storageFeature = base.Parent.GetFeature<TEFeatureStorage>();
		this.lockFeature = base.Parent.GetFeature<TEFeatureLockable>();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void CopyFromInternal(TileEntityComposite _other)
	{
	}

	public override void InitBlockActivationCommands(Action<BlockActivationCommand, TileEntityComposite.EBlockCommandOrder, TileEntityFeatureData> _addCallback)
	{
		base.InitBlockActivationCommands(_addCallback);
	}

	public override bool AllowBlockActivationCommand(ITileEntityFeature _module, ReadOnlySpan<char> _commandName, WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, EntityAlive _entityFocusing)
	{
		return base.AllowBlockActivationCommand(_module, _commandName, _world, _blockPos, _blockValue, _entityFocusing) && !(_module is TEFeatureStorage) && (!this.Equals(_module) || ((this.lockFeature == null || GameManager.Instance.IsEditMode() || !this.lockFeature.IsLocked() || this.lockFeature.IsUserAllowed(PlatformManager.InternalLocalUserIdentifier)) && base.Parent.LocalPlayerIsOwner));
	}

	public override bool OnBlockActivated(ReadOnlySpan<char> _commandName, WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, EntityPlayerLocal _player)
	{
		base.OnBlockActivated(_commandName, _world, _blockPos, _blockValue, _player);
		return false;
	}

	public override void Read(PooledBinaryReader _br, TileEntity.StreamModeRead _eStreamMode)
	{
		base.Read(_br, _eStreamMode);
		if (_eStreamMode == TileEntity.StreamModeRead.Persistency)
		{
			if (base.Parent.UseLocalVersioning())
			{
				_br.ReadUInt16();
				return;
			}
			base.Parent.GetLegacyForkVersion();
		}
	}

	public override void Write(PooledBinaryWriter _bw, TileEntity.StreamModeWrite _eStreamMode)
	{
		base.Write(_bw, _eStreamMode);
		if (_eStreamMode == TileEntity.StreamModeWrite.Persistency)
		{
			_bw.Write(18);
		}
	}

	public void RepairAll(World _world, Vector3i _blockPos, int _requesterId)
	{
		GameManager.Instance.StartCoroutine(this.repair(_world, _blockPos, _requesterId));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator repair(World _world, Vector3i _blockPos, int _requesterId)
	{
		Vector2i blocksMin = new Vector2i(_blockPos.x - 22, _blockPos.z - 22);
		Vector2i blocksMax = new Vector2i(blocksMin.x + 44, blocksMin.y + 44);
		Vector2i vector2i = new Vector2i(blocksMin);
		Vector2i vector2i2 = new Vector2i(blocksMax);
		Vector2i startChunk = World.toChunkXZ(vector2i);
		Vector2i endChunk = World.toChunkXZ(vector2i2);
		int num;
		for (int cx = startChunk.x; cx <= endChunk.x; cx = num + 1)
		{
			yield return this.repairChunkCheckBounds(_world, cx, startChunk.y, _world, blocksMin, blocksMax);
			yield return this.repairChunkCheckBounds(_world, cx, endChunk.y, _world, blocksMin, blocksMax);
			num = cx;
		}
		for (int cz = startChunk.y; cz <= endChunk.y - 1; cz = num + 1)
		{
			yield return this.repairChunkCheckBounds(_world, startChunk.x, cz, _world, blocksMin, blocksMax);
			yield return this.repairChunkCheckBounds(_world, endChunk.x, cz, _world, blocksMin, blocksMax);
			num = cz;
		}
		for (int cz = startChunk.y + 1; cz <= endChunk.y - 1; cz = num + 1)
		{
			for (int cx = startChunk.x + 1; cx <= endChunk.x - 1; cx = num + 1)
			{
				yield return this.repairChunk(_world, cx, cz, _world);
				num = cx;
			}
			num = cz;
		}
		EntityPlayer primaryPlayer = _world.GetPrimaryPlayer();
		if (primaryPlayer != null && _requesterId == primaryPlayer.entityId)
		{
			this.IsRepairing = false;
		}
		else
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageLandClaimRepair>().Setup(_blockPos, false), false, _requesterId, -1, -1, null, 192, false);
		}
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator repairChunkCheckBounds(World _world, int cx, int cz, World world, Vector2i blocksMin, Vector2i blocksMax)
	{
		Chunk chunk = (Chunk)world.GetChunkSync(cx, cz);
		if (chunk == null)
		{
			yield break;
		}
		yield return chunk.LoopOverAllBlocksCoroutine(delegate(int x, int y, int z, BlockValue bv)
		{
			if (!bv.isair && bv.damage > 0 && x >= blocksMin.x && z >= blocksMin.y && x < blocksMax.x && z < blocksMax.y)
			{
				this.repairBlock(_world, chunk.worldPosIMin + new Vector3i(x, y, z), bv);
			}
		}, false, false);
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator repairChunk(World _world, int cx, int cz, World world)
	{
		Chunk chunk = (Chunk)world.GetChunkSync(cx, cz);
		if (chunk == null)
		{
			yield break;
		}
		yield return chunk.LoopOverAllBlocksCoroutine(delegate(int x, int y, int z, BlockValue bv)
		{
			if (!bv.isair && bv.damage > 0)
			{
				this.repairBlock(_world, chunk.worldPosIMin + new Vector3i(x, y, z), bv);
			}
		}, false, false);
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void repairBlock(World _world, Vector3i worldPos, BlockValue bv)
	{
		if (this.storageFeature == null)
		{
			return;
		}
		Block block = bv.Block;
		int num = bv.damage;
		float num2 = (float)num / (float)block.MaxDamage;
		List<Block.SItemNameCount> list = block.RepairItems;
		if (block.RepairItemsMeshDamage != null && block.shape.UseRepairDamageState(bv))
		{
			num = 1;
			num2 = 1f;
			list = block.RepairItemsMeshDamage;
		}
		if (list == null)
		{
			return;
		}
		float resourceScale = block.ResourceScale;
		bool flag = true;
		List<ItemStack> list2 = new List<ItemStack>();
		for (int i = 0; i < list.Count; i++)
		{
			string itemName = list[i].ItemName;
			int num3 = Utils.FastMax((int)((float)list[i].Count * num2 * resourceScale), 1);
			ItemStack itemStack = new ItemStack(ItemClass.GetItem(itemName, false), num3);
			if (this.storageFeature.CountItem(itemStack.itemValue.ItemClass) < itemStack.count)
			{
				flag = false;
				break;
			}
			list2.Add(itemStack);
		}
		if (!flag)
		{
			return;
		}
		for (int j = 0; j < list2.Count; j++)
		{
			ItemStack itemStack2 = list2[j];
			this.storageFeature.RemoveItems(itemStack2.itemValue, itemStack2.count);
		}
		bv.Block.DamageBlock(_world, worldPos, bv, -num, -1, null, false, false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const int Version = 18;

	[PublicizedFrom(EAccessModifier.Private)]
	public TEFeatureStorage storageFeature;

	[PublicizedFrom(EAccessModifier.Private)]
	public TEFeatureLockable lockFeature;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isRepairing;
}
