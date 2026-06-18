using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Audio;
using UnityEngine;

public class TileEntityCollector : TileEntity, IInventory
{
	public Vector2i containerSize
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			this.containerSizeInternal.x = 3;
			this.containerSizeInternal.y = this.OutputWindowHeight;
			return this.containerSizeInternal;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			this.containerSizeInternal = value;
		}
	}

	public BlockCollector collector
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return (BlockCollector)base.blockValue.Block;
		}
	}

	public string[] GetFuelTypes()
	{
		List<string> list = new List<string>();
		foreach (string text in this.collector.Outputs)
		{
			BlockCollector.OutputType outputType = this.collector.GetOutputType(text);
			foreach (string text2 in this.collector.GetFuelType(outputType.Fuel).Items)
			{
				list.Add(text2);
			}
		}
		return list.ToArray();
	}

	public string[] GetCatalystTypes()
	{
		return this.collector.CatalystTypes;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int getFuelCost(BlockCollector.OutputType outputType)
	{
		return this.collector.GetSandboxModifiedFuelNeeded(outputType.FuelCost);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int getAdditionalFuelCost(BlockCollector.OutputType outputType)
	{
		return outputType.AdditionalFuelCost;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public ulong getCurrentConvertSpeed(BlockCollector.OutputType outputType)
	{
		return (ulong)((long)(this.HasModSpeed ? outputType.ModdedConvertSpeedMultiplier : 1));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int getCurrentConvertCount(BlockCollector.OutputType outputType)
	{
		int num = 0;
		int catalystCount = this.getCatalystCount();
		int num2;
		if (!this.collector.CatalystRequirements.TryGetValue(outputType.Name, out num2))
		{
			num2 = 0;
		}
		if (catalystCount >= num2)
		{
			int num3;
			if (!this.collector.CatalystMultipliers.TryGetValue(outputType.Name, out num3))
			{
				num3 = 1;
			}
			if (this.collector.UsesCatalyst() && num3 > 0)
			{
				num = this.getCatalystCount() * num3;
			}
			else
			{
				num = (this.HasModCount ? outputType.ModdedConvertCountMultiplier : 1);
			}
		}
		return this.collector.GetSandboxModifiedOutput(num);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int getCatalystCount()
	{
		int num = 0;
		for (int i = 0; i < this.CatalystSlots.Length; i++)
		{
			ItemStack itemStack = this.CatalystSlots[i];
			if (!itemStack.IsEmpty())
			{
				string name = itemStack.itemValue.ItemClass.Name;
				string[] catalystTypes = this.collector.CatalystTypes;
				for (int j = 0; j < catalystTypes.Length; j++)
				{
					if (catalystTypes[j] == name)
					{
						num++;
						break;
					}
				}
			}
		}
		return num;
	}

	public bool IsSlotDisabled(int _slot)
	{
		BlockCollector.OutputType outputType = this.collector.GetOutputType(this.collector.Outputs[_slot]);
		return this.isDisabled(outputType);
	}

	public BlockCollector.OutputType GetSlotOutputType(int slot)
	{
		return this.collector.GetOutputType(this.collector.Outputs[slot]);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool anyEnabled()
	{
		bool flag = false;
		foreach (string text in this.collector.Outputs)
		{
			BlockCollector.OutputType outputType = this.collector.GetOutputType(text);
			flag |= !this.isDisabled(outputType);
			if (flag)
			{
				break;
			}
		}
		return flag;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isDisabled(BlockCollector.OutputType _outputType)
	{
		return this.isBlocked || this.outputTypeIsOutOfFuel(_outputType) || this.isUnderwater || this.outputTypeIsFull(_outputType);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool outputTypeIsFull(BlockCollector.OutputType outputType)
	{
		bool flag;
		if (!this.isFull.TryGetValue(outputType.Name, out flag))
		{
			flag = true;
		}
		return flag;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool outputTypeIsOutOfFuel(BlockCollector.OutputType outputType)
	{
		bool flag;
		if (!this.outOfFuel.TryGetValue(outputType.Name, out flag))
		{
			flag = true;
		}
		return flag;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void safeIstackArrayCopy(ItemStack[] inArray, ItemStack[] outArray)
	{
		int i;
		for (i = 0; i < inArray.Length; i++)
		{
			if (i >= outArray.Length)
			{
				break;
			}
			outArray[i] = inArray[i].Clone();
		}
		while (i < outArray.Length)
		{
			outArray[i] = ItemStack.Empty;
			i++;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void safeFillArrayCopy(float[] inArray, float[] outArray)
	{
		int i;
		for (i = 0; i < inArray.Length; i++)
		{
			if (i >= outArray.Length)
			{
				break;
			}
			outArray[i] = inArray[i];
		}
		while (i < outArray.Length)
		{
			outArray[i] = -1f;
			i++;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemStack[] safeItemStackArray(int count)
	{
		ItemStack[] array = new ItemStack[count];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = ItemStack.Empty;
		}
		return array;
	}

	public ItemStack[] Items
	{
		get
		{
			if (this.itemsInternal == null)
			{
				this.itemsInternal = this.safeItemStackArray(this.containerSize.x * this.containerSize.y);
			}
			return this.itemsInternal;
		}
		set
		{
			this.safeIstackArrayCopy(value, this.Items);
		}
	}

	public ItemStack[] ModSlots
	{
		get
		{
			if (this.modSlotsInternal == null)
			{
				this.modSlotsInternal = this.safeItemStackArray(3);
			}
			return this.modSlotsInternal;
		}
	}

	public void SetModSlots(ItemStack[] value, bool fromRead = false)
	{
		this.safeIstackArrayCopy(value, this.ModSlots);
		this.visibleChanged = true;
		this.modsChanged = true;
		this.HandleModChanged(fromRead);
	}

	public ItemStack[] FuelSlots
	{
		get
		{
			if (this.fuelSlotsInternal == null)
			{
				this.fuelSlotsInternal = this.safeItemStackArray(this.FuelGridLength);
			}
			return this.fuelSlotsInternal;
		}
		set
		{
			this.safeIstackArrayCopy(value, this.FuelSlots);
			this.visibleChanged = true;
		}
	}

	public int FuelGridHeight
	{
		get
		{
			return this.collector.FuelGridInitialHeight;
		}
	}

	public int FuelGridLength
	{
		get
		{
			return this.FuelGridHeight * 3;
		}
	}

	public int CatalystGridHeight
	{
		get
		{
			return this.collector.CatalystGridInitialHeight + (this.HasModExpand ? 1 : 0);
		}
	}

	public int OutputWindowHeight
	{
		get
		{
			return this.collector.OutputGridInitialHeight + (this.HasModExpand ? 1 : 0);
		}
	}

	public int CatalystGridLength
	{
		get
		{
			return this.CatalystGridHeight * 3;
		}
	}

	public ItemStack[] CatalystSlots
	{
		get
		{
			if (this.catalystSlotsInternal == null)
			{
				this.catalystSlotsInternal = this.safeItemStackArray(this.CatalystGridLength);
			}
			return this.catalystSlotsInternal;
		}
		set
		{
			this.safeIstackArrayCopy(value, this.CatalystSlots);
			this.visibleChanged = true;
		}
	}

	public TileEntityCollector(Chunk _chunk)
		: base(_chunk)
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public TileEntityCollector(TileEntityCollector _other)
		: base(null)
	{
		this.containerSize = _other.containerSize;
		this.Items = _other.Items;
		this.FuelSlots = _other.FuelSlots;
		this.CatalystSlots = _other.CatalystSlots;
		this.SetModSlots(_other.ModSlots, false);
		this.worldTimeTouched = _other.worldTimeTouched;
		this.bUserAccessing = _other.bUserAccessing;
	}

	public void SetupModMeshes()
	{
		BlockEntityData blockEntity = base.GetChunk().GetBlockEntity(base.ToWorldPos());
		if (blockEntity != null && blockEntity.transform != null)
		{
			int num = this.collector.ModTransformEnableNames.Length;
			this.modEnableRenderers = new GameObject[num];
			this.modDisableRenderers = new GameObject[num];
			List<Transform> list = new List<Transform>();
			blockEntity.transform.GetComponentsInChildren<Transform>(true, list);
			for (int i = 0; i < num; i++)
			{
				this.modEnableRenderers[i] = null;
				if (!string.IsNullOrEmpty(this.collector.ModTransformEnableNames[i]))
				{
					foreach (Transform transform in list)
					{
						if (transform.name == this.collector.ModTransformEnableNames[i])
						{
							this.modEnableRenderers[i] = transform.gameObject;
							break;
						}
					}
				}
				this.modDisableRenderers[i] = null;
				if (!string.IsNullOrEmpty(this.collector.ModTransformDisableNames[i]))
				{
					foreach (Transform transform2 in list)
					{
						if (transform2.name == this.collector.ModTransformDisableNames[i])
						{
							this.modDisableRenderers[i] = transform2.gameObject;
							break;
						}
					}
				}
			}
			this.modMeshesSetup = true;
		}
	}

	public override TileEntity Clone()
	{
		return new TileEntityCollector(this);
	}

	public override void UpdateTick(World world)
	{
		base.UpdateTick(world);
		this.HandleUpdate(world);
	}

	public void HandleUpdate(World world)
	{
		this.HandleModChanged(false);
		this.isUnderwater = base.blockValue.Block.IsUnderwater(GameManager.Instance.World, base.ToWorldPos(), base.blockValue);
		if (this.countdownBlockedCheck.HasPassed())
		{
			this.isBlocked = this.HandleSkyCheck();
			this.countdownBlockedCheck.ResetAndRestart();
		}
		bool flag = false;
		foreach (KeyValuePair<BlockCollector.OutputType, List<int>> keyValuePair in this.collector.OrderedSlotOutputs)
		{
			flag |= this.handleUpdateForOutputType(world, keyValuePair.Key, keyValuePair.Value);
		}
		if (flag)
		{
			base.NotifyListeners();
			base.emitHeatMapEvent(world, EnumAIDirectorChunkEvent.Campfire);
			this.setModified();
		}
		int num = (this.anyEnabled() ? 1 : 0);
		if (this.productionEnabled < 1 && num == 1)
		{
			if (this.productionEnabled > -1)
			{
				Manager.BroadcastPlay(base.ToWorldPos(), this.collector.ActivateSound, 0f);
			}
			Manager.BroadcastPlay(base.ToWorldPos(), this.collector.RunningSound, 0f);
		}
		else if (this.productionEnabled == 1 && num < 1)
		{
			Manager.BroadcastStop(base.ToWorldPos(), this.collector.RunningSound);
		}
		this.productionEnabled = num;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int getFirstFreeIndex(List<int> _slotIndices)
	{
		int num = -1;
		foreach (int num2 in _slotIndices)
		{
			if (num2 < this.Items.Length)
			{
				ItemStack itemStack = this.Items[num2];
				if (itemStack == null || itemStack.IsEmpty())
				{
					num = num2;
					break;
				}
			}
		}
		return num;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool handleUpdateForOutputType(World world, BlockCollector.OutputType _outputType, List<int> _slotIndicess)
	{
		bool flag = false;
		BlockCollector.FuelType fuelType = this.collector.GetFuelType(_outputType.Fuel);
		this.outOfFuel[_outputType.Name] = this.getMaxProductionCount(_outputType, fuelType) <= 0;
		int num = this.getFirstFreeIndex(_slotIndicess);
		this.isFull[_outputType.Name] = num < 0;
		bool flag2 = this.isDisabled(_outputType);
		bool flag3 = this.wasDisabled < 0 || this.wasDisabled != (flag2 ? 1 : 0);
		this.wasDisabled = (flag2 ? 1 : 0);
		if (flag2)
		{
			this.resetTimeValues(_outputType);
		}
		if (flag3)
		{
			this.setModified();
		}
		this.worldTimeTouched = world.worldTime;
		ulong worldTime = world.worldTime;
		ulong num2;
		if (!this.lastWorldTimes.TryGetValue(_outputType.Name, out num2))
		{
			num2 = worldTime;
			this.lastWorldTimes[_outputType.Name] = worldTime;
		}
		ulong num3 = worldTime - num2;
		int num4 = (int)((num2 != 0UL) ? (num3 * this.getCurrentConvertSpeed(_outputType)) : 0UL);
		int sandboxModifiedTime = this.collector.GetSandboxModifiedTime(num4);
		bool flag4 = false;
		if (sandboxModifiedTime >= 0)
		{
			num4 = sandboxModifiedTime;
		}
		else
		{
			flag4 = true;
		}
		this.lastWorldTimes[_outputType.Name] = world.worldTime;
		while (!flag2 && (num4 > 0 || flag4))
		{
			TileEntityCollector.FillData fillData;
			if (!this.fillDataLookup.TryGetValue(_outputType.Name, out fillData))
			{
				int num5 = GameManager.Instance.World.GetGameRandom().RandomRange(_outputType.MinConvertTime, _outputType.MaxConvertTime);
				fillData = new TileEntityCollector.FillData(num, num5, num5);
				this.fillDataLookup.Add(_outputType.Name, fillData);
			}
			if (fillData.fillTimeLeft <= num4 || flag4)
			{
				num4 -= fillData.fillTimeLeft;
				this.Items[fillData.slot] = this.newItem(_outputType, fuelType);
				this.fillDataLookup.Remove(_outputType.Name);
				num = this.getFirstFreeIndex(_slotIndicess);
				this.isFull[_outputType.Name] = num < 0;
				this.outOfFuel[_outputType.Name] = this.getMaxProductionCount(_outputType, fuelType) <= 0;
				flag2 = this.isDisabled(_outputType);
			}
			else
			{
				fillData.fillTimeLeft -= num4;
				num4 = 0;
			}
		}
		return flag;
	}

	public TileEntityCollector.FillData GetSlotFillData(int _slot)
	{
		TileEntityCollector.FillData fillData = null;
		foreach (TileEntityCollector.FillData fillData2 in this.fillDataLookup.Values)
		{
			if (fillData2.slot == _slot)
			{
				fillData = fillData2;
				break;
			}
		}
		return fillData;
	}

	public bool IsCurrentStack(int _slot)
	{
		bool flag = false;
		using (Dictionary<string, TileEntityCollector.FillData>.ValueCollection.Enumerator enumerator = this.fillDataLookup.Values.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				if (enumerator.Current.slot == _slot)
				{
					flag = true;
					break;
				}
			}
		}
		return flag;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int getFuelSlotFuelCount(ItemStack itemStack, BlockCollector.FuelType fuelType)
	{
		int num = 0;
		string[] items = fuelType.Items;
		for (int i = 0; i < items.Length; i++)
		{
			if (!itemStack.IsEmpty() && items[i] == itemStack.itemValue.ItemClass.Name)
			{
				num = itemStack.count;
				break;
			}
		}
		return num;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int getFuelCount(BlockCollector.FuelType fuelType)
	{
		int num = 0;
		foreach (ItemStack itemStack in this.FuelSlots)
		{
			num += this.getFuelSlotFuelCount(itemStack, fuelType);
		}
		return num;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int fuelCost(BlockCollector.OutputType outputType, int itemsToCreate)
	{
		int num = 1;
		if (this.HasModCost)
		{
			num = outputType.DiscountedFuelDivisor;
		}
		return (this.getFuelCost(outputType) + this.getAdditionalFuelCost(outputType) * (itemsToCreate - 1) + num - 1) / num;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int getMaxProductionCount(BlockCollector.OutputType outputType, BlockCollector.FuelType fuelType)
	{
		int num = this.getCurrentConvertCount(outputType);
		int fuelCount = this.getFuelCount(fuelType);
		if (!this.collector.UsesFuel())
		{
			return num;
		}
		while (num > 0 && this.fuelCost(outputType, num) > fuelCount)
		{
			num--;
		}
		return num;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void removeFuel(int itemCount, BlockCollector.OutputType outputType, BlockCollector.FuelType fuelType)
	{
		int num = this.fuelCost(outputType, itemCount);
		num = this.collector.GetSandboxModifiedFuelNeeded(num);
		if (num == 0)
		{
			return;
		}
		ItemStack[] fuelSlots = this.FuelSlots;
		for (int i = 0; i < fuelSlots.Length; i++)
		{
			ItemStack itemStack = fuelSlots[i];
			int fuelSlotFuelCount = this.getFuelSlotFuelCount(itemStack, fuelType);
			if (fuelSlotFuelCount > 0)
			{
				if (fuelSlotFuelCount >= num)
				{
					itemStack.count -= num;
					return;
				}
				num -= itemStack.count;
				itemStack.count = 0;
				if (itemStack.count < 1)
				{
					fuelSlots[i] = ItemStack.Empty;
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void resetTimeValues(BlockCollector.OutputType _outputType)
	{
		this.lastWorldTimes[_outputType.Name] = GameManager.Instance.World.worldTime;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleModChanged(bool fromRead = false)
	{
		if (this.modsChanged)
		{
			if (!this.modMeshesSetup)
			{
				this.SetupModMeshes();
			}
			this.modsChanged = !this.modMeshesSetup;
			this.HasModConvert = false;
			this.HasModModify = false;
			this.HasModCount = false;
			this.HasModSpeed = false;
			this.HasModCost = false;
			this.HasModExpand = false;
			ItemStack[] modSlots = this.ModSlots;
			for (int i = 0; i < modSlots.Length; i++)
			{
				bool flag = modSlots[i].IsEmpty();
				if (!flag)
				{
					switch (this.collector.ModTypes[i])
					{
					case BlockCollector.ModEffectTypes.Type:
						this.HasModConvert = true;
						break;
					case BlockCollector.ModEffectTypes.Speed:
						this.HasModSpeed = true;
						break;
					case BlockCollector.ModEffectTypes.Count:
						this.HasModCount = true;
						break;
					case BlockCollector.ModEffectTypes.Modify:
						this.HasModModify = true;
						break;
					case BlockCollector.ModEffectTypes.Expand:
						this.HasModExpand = true;
						break;
					case BlockCollector.ModEffectTypes.Cost:
						this.HasModCost = true;
						break;
					}
				}
				if (this.modMeshesSetup)
				{
					if (this.modEnableRenderers[i] != null)
					{
						this.modEnableRenderers[i].gameObject.SetActive(!flag);
					}
					if (this.modDisableRenderers[i] != null)
					{
						this.modDisableRenderers[i].gameObject.SetActive(flag);
					}
				}
			}
			if (!fromRead)
			{
				List<ItemStack> list = new List<ItemStack>();
				int num = this.CatalystGridLength;
				int num2 = this.CatalystSlots.Length;
				if (num2 != num)
				{
					ItemStack[] array = this.safeItemStackArray(num);
					this.safeIstackArrayCopy(this.CatalystSlots, array);
					if (num2 > num)
					{
						for (int j = num; j < this.CatalystSlots.Length; j++)
						{
							list.Add(this.CatalystSlots[j]);
						}
					}
					this.catalystSlotsInternal = null;
					this.CatalystSlots = array;
				}
				num = this.containerSize.x * this.containerSize.y;
				num2 = this.itemsInternal.Length;
				if (num2 != num)
				{
					ItemStack[] array2 = this.safeItemStackArray(num);
					this.safeIstackArrayCopy(this.itemsInternal, array2);
					if (num2 > num)
					{
						for (int k = num; k < this.itemsInternal.Length; k++)
						{
							list.Add(this.itemsInternal[k]);
						}
					}
					this.itemsInternal = null;
					this.Items = array2;
				}
				this.dropItems(list);
			}
		}
		base.NotifyListeners();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool HandleSkyCheck()
	{
		Vector3i localChunkPos = base.localChunkPos;
		for (int i = 0; i < 7; i++)
		{
			localChunkPos.y++;
			if (localChunkPos.y >= 256)
			{
				break;
			}
			BlockValue block = this.chunk.GetBlock(localChunkPos);
			if (block.Block != base.blockValue.Block && block.Block.IsCollideArrows)
			{
				return true;
			}
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemStack newItem(BlockCollector.OutputType outputType, BlockCollector.FuelType fuelType)
	{
		ItemStack itemStack = null;
		int maxProductionCount = this.getMaxProductionCount(outputType, fuelType);
		if (maxProductionCount > 0)
		{
			this.removeFuel(maxProductionCount, outputType, fuelType);
			itemStack = new ItemStack(new ItemValue(this.HasModConvert ? ItemClass.GetItemClass(outputType.OutputItemModded, false).Id : ItemClass.GetItemClass(outputType.OutputItem, false).Id, false), maxProductionCount);
		}
		return itemStack;
	}

	public void SetWorldTime()
	{
		ulong worldTime = GameManager.Instance.World.worldTime;
		foreach (string text in this.collector.Outputs)
		{
			this.lastWorldTimes[text] = worldTime;
		}
	}

	public Vector2i GetContainerSize()
	{
		return this.containerSize;
	}

	public override void read(PooledBinaryReader _br, TileEntity.StreamModeRead _eStreamMode)
	{
		base.read(_br, _eStreamMode);
		BlockCollector.OutputType outputType = this.collector.OrderedSlotOutputs.Keys.ElementAt(0);
		IEnumerable<BlockCollector.OutputType> enumerable = this.collector.OrderedSlotOutputs.Keys.Skip(1);
		int num;
		if (_eStreamMode != TileEntity.StreamModeRead.Persistency)
		{
			num = 21;
		}
		else if (base.UseLocalVersioning())
		{
			num = (int)_br.ReadUInt16();
		}
		else
		{
			num = base.GetLegacyForkVersion();
		}
		if (num < 20)
		{
			Vector2i vector2i = default(Vector2i);
			vector2i.x = (int)_br.ReadUInt16();
			vector2i.y = (int)_br.ReadUInt16();
		}
		int num2;
		if (num >= 20)
		{
			num2 = (int)_br.ReadUInt16();
			if (base.bWaitingForServerResponse)
			{
				for (int i = 0; i < num2; i++)
				{
					_br.ReadString();
					_br.ReadUInt64();
				}
			}
			else
			{
				for (int j = 0; j < num2; j++)
				{
					string text = _br.ReadString();
					ulong num3 = _br.ReadUInt64();
					if (!string.IsNullOrEmpty(text))
					{
						this.lastWorldTimes[text] = num3;
					}
				}
			}
		}
		else if (base.bWaitingForServerResponse)
		{
			_br.ReadUInt64();
		}
		else
		{
			this.lastWorldTimes[outputType.Name] = _br.ReadUInt64();
			ulong worldTime = GameManager.Instance.World.worldTime;
			foreach (BlockCollector.OutputType outputType2 in enumerable)
			{
				this.lastWorldTimes[outputType2.Name] = worldTime;
			}
		}
		float num4 = 0f;
		short num5 = 0;
		if (num < 20)
		{
			num4 = _br.ReadSingle();
			num5 = _br.ReadInt16();
			_br.ReadSingle();
		}
		if (num >= 20)
		{
			num2 = (int)_br.ReadUInt16();
			this.fillDataLookup.Clear();
			while (num2 != 0)
			{
				if (base.bWaitingForServerResponse)
				{
					_br.ReadString();
					_br.ReadUInt32();
					_br.ReadUInt32();
					_br.ReadUInt32();
				}
				else
				{
					string text2 = _br.ReadString();
					int num6 = (int)_br.ReadUInt32();
					int num7 = (int)_br.ReadUInt32();
					int num8 = (int)_br.ReadUInt32();
					this.fillDataLookup[text2] = new TileEntityCollector.FillData(num6, num7, num8);
				}
				num2--;
			}
		}
		if (num >= 15)
		{
			if (base.bWaitingForServerResponse)
			{
				_br.ReadBoolean();
				_br.ReadBoolean();
				if (num >= 16)
				{
					if (num >= 20)
					{
						num2 = (int)_br.ReadUInt16();
						for (int k = 0; k < num2; k++)
						{
							_br.ReadString();
							_br.ReadByte();
						}
					}
					else
					{
						_br.ReadBoolean();
					}
				}
				if (num >= 16)
				{
					if (num >= 20)
					{
						num2 = (int)_br.ReadUInt16();
						for (int l = 0; l < num2; l++)
						{
							_br.ReadString();
							_br.ReadByte();
						}
					}
					else
					{
						_br.ReadBoolean();
					}
				}
			}
			else
			{
				this.isUnderwater = _br.ReadBoolean();
				this.isBlocked = _br.ReadBoolean();
				if (num >= 21)
				{
					num2 = (int)_br.ReadUInt16();
					for (int m = 0; m < num2; m++)
					{
						string text3 = _br.ReadString();
						bool flag = _br.ReadByte() > 0;
						this.outOfFuel[text3] = flag;
					}
				}
				else
				{
					bool flag2 = _br.ReadByte() > 0;
					this.outOfFuel[outputType.Name] = flag2;
					foreach (BlockCollector.OutputType outputType3 in enumerable)
					{
						this.outOfFuel[outputType3.Name] = true;
					}
				}
				if (num >= 16)
				{
					this.isFull.Clear();
					if (num >= 20)
					{
						num2 = (int)_br.ReadUInt16();
						for (int n = 0; n < num2; n++)
						{
							string text4 = _br.ReadString();
							bool flag3 = _br.ReadByte() > 0;
							this.isFull[text4] = flag3;
						}
					}
					else
					{
						bool flag4 = _br.ReadBoolean();
						this.isFull[outputType.Name] = flag4;
						foreach (BlockCollector.OutputType outputType4 in enumerable)
						{
							this.isFull[outputType4.Name] = true;
						}
					}
				}
			}
		}
		num2 = (int)_br.ReadInt16();
		ItemStack[] array = this.safeItemStackArray(num2);
		for (int num9 = 0; num9 < num2; num9++)
		{
			array[num9].Read(_br);
		}
		if (num < 20)
		{
			float[] array2 = new float[num2];
			for (int num10 = 0; num10 < num2; num10++)
			{
				array2[num10] = _br.ReadSingle();
			}
			if (!base.bWaitingForServerResponse)
			{
				this.fillDataLookup.Clear();
				for (int num11 = 0; num11 < num2; num11++)
				{
					if (array2[num11] >= 0f)
					{
						this.fillDataLookup[outputType.Name] = new TileEntityCollector.FillData((int)num5, (int)array2[num11], (int)num4);
						break;
					}
				}
			}
		}
		if (num >= 11)
		{
			int num12 = (int)_br.ReadInt16();
			ItemStack[] array3 = this.safeItemStackArray(num12);
			for (int num13 = 0; num13 < num12; num13++)
			{
				array3[num13].Read(_br);
			}
			if (!base.bWaitingForServerResponse)
			{
				this.modsChanged = true;
				this.SetModSlots(array3, true);
			}
		}
		if (!base.bWaitingForServerResponse)
		{
			this.Items = array;
		}
		if (num >= 14)
		{
			int num14 = (int)_br.ReadInt16();
			ItemStack[] array4 = this.safeItemStackArray(num14);
			for (int num15 = 0; num15 < num14; num15++)
			{
				array4[num15].Read(_br);
			}
			if (!base.bWaitingForServerResponse)
			{
				this.FuelSlots = array4;
			}
		}
		if (num >= 19)
		{
			int num16 = (int)_br.ReadInt16();
			ItemStack[] array5 = this.safeItemStackArray(num16);
			for (int num17 = 0; num17 < num16; num17++)
			{
				array5[num17].Read(_br);
			}
			if (!base.bWaitingForServerResponse)
			{
				this.CatalystSlots = array5;
			}
		}
		foreach (BlockCollector.OutputType outputType5 in this.collector.OrderedSlotOutputs.Keys)
		{
			if (this.isDisabled(outputType5))
			{
				this.resetTimeValues(outputType5);
			}
		}
	}

	public override void write(PooledBinaryWriter stream, TileEntity.StreamModeWrite _eStreamMode)
	{
		base.write(stream, _eStreamMode);
		if (_eStreamMode == TileEntity.StreamModeWrite.Persistency)
		{
			stream.Write(21);
		}
		stream.Write((ushort)this.lastWorldTimes.Count);
		foreach (KeyValuePair<string, ulong> keyValuePair in this.lastWorldTimes)
		{
			stream.Write(keyValuePair.Key);
			stream.Write(keyValuePair.Value);
		}
		stream.Write((ushort)this.fillDataLookup.Count);
		foreach (KeyValuePair<string, TileEntityCollector.FillData> keyValuePair2 in this.fillDataLookup)
		{
			stream.Write(keyValuePair2.Key);
			stream.Write((uint)keyValuePair2.Value.slot);
			stream.Write((uint)keyValuePair2.Value.fillTime);
			stream.Write((uint)keyValuePair2.Value.fillTimeLeft);
		}
		stream.Write(this.isUnderwater);
		stream.Write(this.isBlocked);
		stream.Write((ushort)this.outOfFuel.Count);
		foreach (KeyValuePair<string, bool> keyValuePair3 in this.outOfFuel)
		{
			stream.Write(keyValuePair3.Key);
			stream.Write(keyValuePair3.Value ? 1 : 0);
		}
		stream.Write((ushort)this.isFull.Count);
		foreach (KeyValuePair<string, bool> keyValuePair4 in this.isFull)
		{
			stream.Write(keyValuePair4.Key);
			stream.Write(keyValuePair4.Value ? 1 : 0);
		}
		stream.Write((short)this.Items.Length);
		foreach (ItemStack itemStack in this.Items)
		{
			if (itemStack == null)
			{
				itemStack = ItemStack.Empty;
			}
			itemStack.Clone().Write(stream);
		}
		ItemStack[] modSlots = this.ModSlots;
		stream.Write((short)modSlots.Length);
		ItemStack[] array = modSlots;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Clone().Write(stream);
		}
		stream.Write((short)this.FuelSlots.Length);
		array = this.FuelSlots;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Clone().Write(stream);
		}
		stream.Write((short)this.CatalystSlots.Length);
		array = this.CatalystSlots;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Clone().Write(stream);
		}
	}

	public override TileEntityType GetTileEntityType()
	{
		return TileEntityType.Collector;
	}

	public ItemStack[] GetItems()
	{
		return this.Items;
	}

	public override void UpgradeDowngradeFrom(TileEntity _other)
	{
		base.UpgradeDowngradeFrom(_other);
		this.OnDestroy();
		if (_other is TileEntityCollector)
		{
			TileEntityCollector tileEntityCollector = _other as TileEntityCollector;
			this.worldTimeTouched = tileEntityCollector.worldTimeTouched;
			this.Items = ItemStack.Clone(tileEntityCollector.Items, 0, this.containerSize.x * this.containerSize.y);
			if (this.Items.Length != this.containerSize.x * this.containerSize.y)
			{
				Log.Error("UpgradeDowngradeFrom: other.size={0}, other.length={1}, this.size={2}, this.length={3}", new object[]
				{
					tileEntityCollector.containerSize,
					tileEntityCollector.Items.Length,
					this.containerSize,
					this.Items.Length
				});
			}
		}
	}

	public override void ReplacedBy(BlockValue _bvOld, BlockValue _bvNew, TileEntity _teNew)
	{
		base.ReplacedBy(_bvOld, _bvNew, _teNew);
		TileEntityCollector tileEntityCollector;
		if (!_teNew.TryGetSelfOrFeature(out tileEntityCollector))
		{
			List<ItemStack> list = new List<ItemStack>();
			list.AddRange(this.Items);
			list.AddRange(this.ModSlots);
			list.AddRange(this.FuelSlots);
			list.AddRange(this.CatalystSlots);
			this.dropItems(list);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void dropItems(List<ItemStack> list)
	{
		Vector3 vector = base.ToWorldCenterPos();
		vector.y += 0.9f;
		GameManager.Instance.DropContentInLootContainerServer(-1, "DroppedLootContainer", vector, list.ToArray(), true, null);
	}

	public void UpdateSlot(int _idx, ItemStack _item)
	{
		if (_item == null)
		{
			this.Items[_idx] = ItemStack.Empty;
		}
		else
		{
			this.Items[_idx] = _item.Clone();
		}
		base.NotifyListeners();
	}

	public bool IsWaterEmpty()
	{
		for (int i = 0; i < this.Items.Length; i++)
		{
			if (this.Items[i] != null && !this.Items[i].IsEmpty())
			{
				return false;
			}
		}
		return true;
	}

	public bool IsEmpty()
	{
		for (int i = 0; i < this.Items.Length; i++)
		{
			if (this.Items[i] != null && !this.Items[i].IsEmpty())
			{
				return false;
			}
		}
		ItemStack[] modSlots = this.ModSlots;
		for (int j = 0; j < modSlots.Length; j++)
		{
			if (!modSlots[j].IsEmpty())
			{
				return false;
			}
		}
		return true;
	}

	public void SetEmpty()
	{
		for (int i = 0; i < this.Items.Length; i++)
		{
			this.Items[i].Clear();
		}
		this.fillDataLookup.Clear();
		base.NotifyListeners();
		this.setModified();
	}

	[return: TupleElementNames(new string[] { "anyMoved", "allMoved" })]
	public ValueTuple<bool, bool> TryStackItem(int startIndex, ItemStack _itemStack)
	{
		if (!_itemStack.CanMoveTo(XUiC_ItemStack.StackLocationTypes.Collector))
		{
			return new ValueTuple<bool, bool>(false, false);
		}
		int count = _itemStack.count;
		int num = 0;
		bool flag = false;
		for (int i = startIndex; i < this.Items.Length; i++)
		{
			num = _itemStack.count;
			if (_itemStack.itemValue.type == this.Items[i].itemValue.type && this.Items[i].CanStackPartly(ref num))
			{
				this.Items[i].count += num;
				_itemStack.count -= num;
				this.setModified();
				flag = true;
				if (_itemStack.count == 0)
				{
					base.NotifyListeners();
					return new ValueTuple<bool, bool>(true, true);
				}
			}
		}
		if (_itemStack.count != count)
		{
			flag = true;
			base.NotifyListeners();
		}
		return new ValueTuple<bool, bool>(flag, false);
	}

	public bool AddItem(ItemStack _itemStack)
	{
		if (!_itemStack.CanMoveTo(XUiC_ItemStack.StackLocationTypes.Collector))
		{
			return false;
		}
		for (int i = 0; i < this.Items.Length; i++)
		{
			if (this.Items[i] != null && this.Items[i].IsEmpty())
			{
				this.UpdateSlot(i, _itemStack);
				base.NotifyListeners();
				return true;
			}
		}
		return false;
	}

	public bool HasItem(ItemValue _item)
	{
		for (int i = 0; i < this.Items.Length; i++)
		{
			if (this.Items[i] != null && this.Items[i].itemValue.ItemClass == _item.ItemClass)
			{
				return true;
			}
		}
		return false;
	}

	public void RemoveItem(ItemValue _item)
	{
		bool flag = false;
		for (int i = 0; i < this.Items.Length; i++)
		{
			if (this.Items[i] != null && this.Items[i].itemValue.ItemClass == _item.ItemClass)
			{
				this.UpdateSlot(i, ItemStack.Empty);
				flag = true;
			}
		}
		if (flag)
		{
			base.NotifyListeners();
		}
	}

	public override bool CanLockLocally(ILockContext _context, ushort _channel)
	{
		return LocalPlayerUI.GetUIForPrimaryPlayer() != null;
	}

	public override void OnLockedLocal(bool _success, ILockContext _context, ushort _channel)
	{
		this.UpdateTick(GameManager.Instance.World);
		LocalPlayerUI uiforPrimaryPlayer = LocalPlayerUI.GetUIForPrimaryPlayer();
		if (!_success)
		{
			GameManager.ShowTooltip(uiforPrimaryPlayer.entityPlayer, Localization.Get("ttNoInteractItem", false, null), string.Empty, "ui_denied", null, false, false, 0f);
			return;
		}
		((XUiC_DewCollectorWindowGroup)((XUiWindowGroup)uiforPrimaryPlayer.windowManager.GetWindow("dewcollector")).Controller).SetTileEntity(this);
		uiforPrimaryPlayer.windowManager.Open("dewcollector", true);
	}

	public void UpdateVisible()
	{
		this.HandleModChanged(false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public new const int Version = 21;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2i containerSizeInternal = new Vector2i(-1, -1);

	[PublicizedFrom(EAccessModifier.Private)]
	public GameObject[] modEnableRenderers;

	[PublicizedFrom(EAccessModifier.Private)]
	public GameObject[] modDisableRenderers;

	public bool HasModConvert;

	public bool HasModCount;

	public bool HasModSpeed;

	public bool HasModModify;

	public bool HasModCost;

	public bool HasModExpand;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Random r = new Random();

	[PublicizedFrom(EAccessModifier.Private)]
	public CountdownTimer countdownBlockedCheck = new CountdownTimer(5f + (float)TileEntityCollector.r.NextDouble(), true);

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isBlocked;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isUnderwater;

	public int wasDisabled = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool visibleChanged;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool modsChanged;

	[PublicizedFrom(EAccessModifier.Private)]
	public int productionEnabled = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<string, bool> isFull = new Dictionary<string, bool>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<string, bool> outOfFuel = new Dictionary<string, bool>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<string, ulong> lastWorldTimes = new Dictionary<string, ulong>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<string, TileEntityCollector.FillData> fillDataLookup = new Dictionary<string, TileEntityCollector.FillData>();

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemStack[] itemsInternal;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemStack[] modSlotsInternal;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemStack[] fuelSlotsInternal;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemStack[] catalystSlotsInternal;

	public ulong worldTimeTouched;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool modMeshesSetup;

	public class FillData
	{
		public FillData(int _slot, int _fillTime, int _fillTimeLeft)
		{
			this.slot = _slot;
			this.fillTime = _fillTime;
			this.fillTimeLeft = _fillTimeLeft;
		}

		public int slot;

		public int fillTime;

		public int fillTimeLeft;
	}
}
