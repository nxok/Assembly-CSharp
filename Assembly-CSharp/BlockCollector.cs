using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Scripting;

[Preserve]
public class BlockCollector : Block
{
	public string[] Outputs
	{
		get
		{
			return this.outputs;
		}
	}

	public BlockCollector.FuelType GetFuelType(string name)
	{
		BlockCollector.FuelType fuelType;
		if (!this.fuelTypes.TryGetValue(name, out fuelType))
		{
			fuelType = null;
		}
		return fuelType;
	}

	public BlockCollector.OutputType GetOutputType(string name)
	{
		BlockCollector.OutputType outputType;
		if (!this.outputTypes.TryGetValue(name, out outputType))
		{
			outputType = null;
		}
		return outputType;
	}

	public string[] ItemIconBackdrop
	{
		get
		{
			return this.itemIconBackdrop;
		}
	}

	public string ItemBackgroundColor
	{
		get
		{
			return this.itemBackgroundColor;
		}
	}

	public string LootLabelKey
	{
		get
		{
			return this.lootLabelKey;
		}
	}

	public string ActivationEvent
	{
		get
		{
			return this.activationEvent;
		}
	}

	public string CloseEvent
	{
		get
		{
			return this.closeEvent;
		}
	}

	public string[] RequiredMods
	{
		get
		{
			return this.requiredMods;
		}
	}

	public bool RequiredModsOnly
	{
		get
		{
			return this.requiredModsOnly;
		}
	}

	public string[] ModTransformEnableNames
	{
		get
		{
			return this.modTransformEnableNames;
		}
	}

	public string[] ModTransformDisableNames
	{
		get
		{
			return this.modTransformDisableNames;
		}
	}

	public int FuelGridInitialHeight
	{
		get
		{
			return this.fuelGridHeight;
		}
	}

	public int CatalystGridInitialHeight
	{
		get
		{
			return this.catalystGridHeight;
		}
	}

	public int OutputGridInitialHeight
	{
		get
		{
			return this.outputGridInitialHeight;
		}
	}

	public string FuelTitleText
	{
		get
		{
			return this.fuelTitleText;
		}
	}

	public string FuelTitleSprite
	{
		get
		{
			return this.fuelTitleSprite;
		}
	}

	public string FuelTitleSpriteAtlas
	{
		get
		{
			return this.fuelTitleSpriteAtlas;
		}
	}

	public string CatalystTitleText
	{
		get
		{
			return this.catalystTitleText;
		}
	}

	public string CatalystTitleSprite
	{
		get
		{
			return this.catalystTitleSprite;
		}
	}

	public string CatalystTitleSpriteAtlas
	{
		get
		{
			return this.catalystTitleSpriteAtlas;
		}
	}

	public string[] FuelTypesSprites
	{
		get
		{
			return this.fuelTypesSprites;
		}
	}

	public string[] FuelTypesSpriteAtlases
	{
		get
		{
			return this.fuelTypesSpriteAtlases;
		}
	}

	public string[] CatalystTypesSprites
	{
		get
		{
			return this.catalystTypesSprites;
		}
	}

	public string[] CatalystTypesSpriteAtlases
	{
		get
		{
			return this.catalystTypesSpriteAtlases;
		}
	}

	public string[] CatalystTypes
	{
		get
		{
			return this.catalystTypes;
		}
	}

	public BlockCollector()
	{
		this.HasTileEntity = true;
	}

	public override void Init()
	{
		base.Init();
		string text = "";
		base.Properties.ParseString(BlockCollector.PropCollectorType, ref text);
		if (text != "")
		{
			Enum.TryParse<BlockCollector.CollectorTypes>(text, out this.CollectorType);
		}
		string text2 = null;
		base.Properties.ParseString(BlockCollector.PropFuelTypes, ref text2);
		string[] array = text2.TrimEnd('}').Split('{', 125, StringSplitOptions.RemoveEmptyEntries);
		for (int i = 0; i < array.Length; i++)
		{
			BlockCollector.FuelType fuelType = new BlockCollector.FuelType(array[i]);
			this.fuelTypes[fuelType.Name] = fuelType;
		}
		string text3 = null;
		base.Properties.ParseString(BlockCollector.PropOutputTypes, ref text3);
		string[] array2 = text3.Split('{', 125, StringSplitOptions.RemoveEmptyEntries);
		for (int j = 0; j < array2.Length; j++)
		{
			array2[j] = array2[j].TrimEnd('}');
			BlockCollector.OutputType outputType = new BlockCollector.OutputType(array2[j]);
			this.outputTypes.Add(outputType.Name, outputType);
		}
		string text4 = null;
		base.Properties.ParseString(BlockCollector.PropOutputs, ref text4);
		this.outputs = text4.Split(',', StringSplitOptions.None);
		for (int k = 0; k < this.outputs.Length; k++)
		{
			BlockCollector.OutputType outputType2 = this.GetOutputType(this.outputs[k]);
			if (outputType2 != null)
			{
				List<int> list;
				if (!this.OrderedSlotOutputs.TryGetValue(outputType2, out list))
				{
					list = new List<int>();
					this.OrderedSlotOutputs[outputType2] = list;
				}
				list.Add(k);
			}
		}
		base.Properties.ParseString(BlockCollector.PropOpenSound, ref this.OpenSound);
		base.Properties.ParseString(BlockCollector.PropRunningSound, ref this.RunningSound);
		base.Properties.ParseString(BlockCollector.PropActivateSound, ref this.ActivateSound);
		base.Properties.ParseString(BlockCollector.PropCloseSound, ref this.CloseSound);
		base.Properties.ParseFloat(BlockCollector.PropTakeDelay, ref this.TakeDelay);
		string text5 = null;
		base.Properties.ParseString(BlockCollector.PropItemIconBackdrop, ref text5);
		this.itemIconBackdrop = text5.Split(',', StringSplitOptions.None);
		base.Properties.ParseString(BlockCollector.PropLootLabelKey, ref this.lootLabelKey);
		base.Properties.ParseString(BlockCollector.PropActivationEvent, ref this.activationEvent);
		base.Properties.ParseString(BlockCollector.PropCloseEvent, ref this.closeEvent);
		base.Properties.ParseString(BlockCollector.PropItemBackgroundColor, ref this.itemBackgroundColor);
		string text6 = "";
		this.requiredMods = null;
		base.Properties.ParseString(BlockCollector.PropRequiredMods, ref text6);
		if (!string.IsNullOrEmpty(text6))
		{
			this.requiredMods = text6.Split(',', StringSplitOptions.None);
		}
		base.Properties.ParseBool(BlockCollector.PropRequiredModsOnly, ref this.requiredModsOnly);
		string text7 = ",,";
		base.Properties.ParseString(BlockCollector.PropModTransformEnableNames, ref text7);
		this.modTransformEnableNames = text7.Split(',', StringSplitOptions.None);
		text7 = ",,";
		base.Properties.ParseString(BlockCollector.PropModTransformDisableNames, ref text7);
		this.modTransformDisableNames = text7.Split(',', StringSplitOptions.None);
		text7 = "Count,Speed,Type";
		base.Properties.ParseString(BlockCollector.PropModTypes, ref text7);
		string[] array3 = text7.Split(',', StringSplitOptions.None);
		this.ModTypes = new BlockCollector.ModEffectTypes[array3.Length];
		int num = 0;
		while (num < array3.Length && num < this.ModTypes.Length)
		{
			this.ModTypes[num] = Enum.Parse<BlockCollector.ModEffectTypes>(array3[num]);
			num++;
		}
		base.Properties.ParseString(BlockCollector.PropEmptyText, ref this.emptyText);
		base.Properties.ParseString(BlockCollector.PropHasItem1Text, ref this.hasItem1Text);
		base.Properties.ParseString(BlockCollector.PropHasItem2Text, ref this.hasItem2Text);
		base.Properties.ParseInt(BlockCollector.PropOutputGridHeight, ref this.outputGridInitialHeight);
		base.Properties.ParseInt(BlockCollector.PropFuelGridHeight, ref this.fuelGridHeight);
		base.Properties.ParseString(BlockCollector.PropFuelTitleText, ref this.fuelTitleText);
		base.Properties.ParseString(BlockCollector.PropFuelTitleSprite, ref this.fuelTitleSprite);
		base.Properties.ParseString(BlockCollector.PropFuelTitleSpriteAtlas, ref this.fuelTitleSpriteAtlas);
		string text8 = null;
		base.Properties.ParseString(BlockCollector.PropFuelTypesSprites, ref text8);
		if (text8 != null)
		{
			string[] array4 = text8.Split(',', StringSplitOptions.None);
			int num2 = array4.Length / 2;
			this.fuelTypesSprites = new string[num2];
			this.fuelTypesSpriteAtlases = new string[num2];
			for (int l = 0; l < num2; l++)
			{
				int num3 = l * 2;
				this.fuelTypesSprites[l] = array4[num3];
				this.FuelTypesSpriteAtlases[l] = array4[num3 + 1];
			}
		}
		base.Properties.ParseInt(BlockCollector.PropCatalystGridHeight, ref this.catalystGridHeight);
		text8 = null;
		base.Properties.ParseString(BlockCollector.PropCatalystTypes, ref text8);
		if (text8 != null)
		{
			this.catalystTypes = text8.Split(',', StringSplitOptions.None);
		}
		base.Properties.ParseString(BlockCollector.PropCatalystTypes, ref text8);
		if (text8 != null)
		{
			string[] array5 = text8.Split(',', StringSplitOptions.None);
			int num4 = array5.Length / 2;
			this.CatalystConverts = new BlockCollector.CatalystConvert[num4];
			for (int m = 0; m < num4; m++)
			{
				this.CatalystConverts[m] = new BlockCollector.CatalystConvert(array5[m * 2], array5[m * 2 + 1]);
			}
		}
		base.Properties.ParseString(BlockCollector.PropCatalystMultiplier, ref text8);
		if (text8 != null)
		{
			array = text8.Split(",", StringSplitOptions.None);
			for (int i = 0; i < array.Length; i++)
			{
				string[] array6 = array[i].Split("=", StringSplitOptions.None);
				if (array6.Length == 2)
				{
					int num5;
					if (!int.TryParse(array6[1], out num5))
					{
						num5 = 1;
					}
					this.CatalystMultipliers.Add(array6[0], num5);
				}
			}
		}
		base.Properties.ParseString(BlockCollector.PropCatalystRequirements, ref text8);
		if (text8 != null)
		{
			array = text8.Split(",", StringSplitOptions.None);
			for (int i = 0; i < array.Length; i++)
			{
				string[] array7 = array[i].Split("=", StringSplitOptions.None);
				if (array7.Length == 2)
				{
					int num6;
					if (!int.TryParse(array7[1], out num6))
					{
						num6 = 1;
					}
					this.CatalystRequirements.Add(array7[0], num6);
				}
			}
		}
		base.Properties.ParseString(BlockCollector.PropCatalystTitleText, ref this.catalystTitleText);
		base.Properties.ParseString(BlockCollector.PropCatalystTitleSprite, ref this.catalystTitleSprite);
		base.Properties.ParseString(BlockCollector.PropCatalystTitleSpriteAtlas, ref this.catalystTitleSpriteAtlas);
		base.Properties.ParseString(BlockCollector.PropCatalystTypesSprites, ref text8);
		if (text8 != null)
		{
			string[] array8 = text8.Split(',', StringSplitOptions.None);
			int num7 = array8.Length / 2;
			this.catalystTypesSprites = new string[num7];
			this.catalystTypesSpriteAtlases = new string[num7];
			for (int n = 0; n < num7; n++)
			{
				int num8 = n * 2;
				this.catalystTypesSprites[n] = array8[num8];
				this.catalystTypesSpriteAtlases[n] = array8[num8 + 1];
			}
		}
	}

	public override void OnBlockAdded(WorldBase world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue, PlatformUserIdentifierAbs _addedByPlayer)
	{
		base.OnBlockAdded(world, _chunk, _blockPos, _blockValue, _addedByPlayer);
		if (_blockValue.ischild)
		{
			return;
		}
		this.addTileEntity(world, _chunk, _blockPos, _blockValue);
	}

	public override bool OnBlockActivated(string _commandName, WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, EntityPlayerLocal _player)
	{
		if (_commandName == "Search")
		{
			return this.OnBlockActivated(_world, _blockPos, _blockValue, _player);
		}
		if (!(_commandName == "take"))
		{
			return false;
		}
		base.takeItemWithTimer(_blockPos, _blockValue, _player, this.TakeDelay);
		return true;
	}

	public override bool HasBlockActivationCommands(WorldBase _world, BlockValue _blockValue, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		return true;
	}

	public override BlockActivationCommand[] GetBlockActivationCommands(WorldBase _world, BlockValue _blockValue, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		bool flag = _world.IsMyLandProtectedBlock(_blockPos, _world.GetGameManager().GetPersistentLocalPlayer(), false);
		this.cmds[1].enabled = flag && this.TakeDelay > 0f;
		return this.cmds;
	}

	public override void PlaceBlock(WorldBase _world, BlockPlacement.Result _result, EntityAlive _ea)
	{
		base.PlaceBlock(_world, _result, _ea);
		TileEntityCollector tileEntityCollector = _world.GetTileEntity(_result.blockPos) as TileEntityCollector;
		if (tileEntityCollector == null)
		{
			return;
		}
		if (_ea != null && _ea.entityType == EntityType.Player)
		{
			tileEntityCollector.worldTimeTouched = _world.GetWorldTime();
			tileEntityCollector.SetEmpty();
		}
	}

	public override string GetActivationText(WorldBase _world, BlockValue _blockValue, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		TileEntityCollector tileEntityCollector = _world.GetTileEntity(_blockPos) as TileEntityCollector;
		if (tileEntityCollector == null)
		{
			return string.Empty;
		}
		string localizedBlockName = _blockValue.Block.GetLocalizedBlockName();
		PlayerActionsLocal playerInput = ((EntityPlayerLocal)_entityFocusing).playerInput;
		string text = playerInput.Activate.GetBindingXuiMarkupString(XUiUtils.EmptyBindingStyle.EmptyString, XUiUtils.DisplayStyle.Plain, null) + playerInput.PermanentActions.Activate.GetBindingXuiMarkupString(XUiUtils.EmptyBindingStyle.EmptyString, XUiUtils.DisplayStyle.Plain, null);
		if (tileEntityCollector.IsWaterEmpty())
		{
			return string.Format(Localization.Get(this.emptyText, false, null), text, localizedBlockName);
		}
		if (tileEntityCollector.HasModConvert)
		{
			return string.Format(Localization.Get(this.hasItem2Text, false, null), text, localizedBlockName);
		}
		return string.Format(Localization.Get(this.hasItem1Text, false, null), text, localizedBlockName);
	}

	public override void OnBlockRemoved(WorldBase world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
	{
		base.OnBlockRemoved(world, _chunk, _blockPos, _blockValue);
		TileEntityCollector tileEntityCollector = world.GetTileEntity(_blockPos) as TileEntityCollector;
		if (tileEntityCollector != null)
		{
			tileEntityCollector.OnDestroy();
		}
		this.removeTileEntity(world, _chunk, _blockPos, _blockValue);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void addTileEntity(WorldBase world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
	{
		TileEntityCollector tileEntityCollector = new TileEntityCollector(_chunk);
		tileEntityCollector.localChunkPos = World.toBlock(_blockPos);
		tileEntityCollector.SetWorldTime();
		_chunk.AddTileEntity(tileEntityCollector);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void removeTileEntity(WorldBase world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
	{
		_chunk.RemoveTileEntityAt<TileEntityCollector>((World)world, World.toBlock(_blockPos));
	}

	public override Block.DestroyedResult OnBlockDestroyedBy(WorldBase _world, BlockValueRef _bvRef, BlockValue _blockValue, int _entityId, bool _bUseHarvestTool)
	{
		TileEntityCollector tileEntityCollector = _world.GetTileEntity(_bvRef) as TileEntityCollector;
		if (tileEntityCollector != null)
		{
			tileEntityCollector.OnDestroy();
		}
		if (!GameManager.IsDedicatedServer)
		{
			XUiC_DewCollectorWindowGroup.CloseIfOpenAtPos(_bvRef, null);
		}
		return Block.DestroyedResult.Downgrade;
	}

	public override void OnBlockEntityTransformBeforeActivated(WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, BlockEntityData _ebcd)
	{
		base.OnBlockEntityTransformBeforeActivated(_world, _blockPos, _blockValue, _ebcd);
		this.UpdateVisible(_world, _blockPos);
	}

	public void UpdateVisible(WorldBase _world, Vector3i _blockPos)
	{
		TileEntityCollector tileEntityCollector = _world.GetTileEntity(_blockPos) as TileEntityCollector;
		if (tileEntityCollector != null)
		{
			tileEntityCollector.UpdateVisible();
		}
	}

	public override bool OnBlockActivated(WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, EntityPlayerLocal _player)
	{
		if (_player.inventory.IsHoldingItemActionRunning())
		{
			return false;
		}
		TileEntityCollector tileEntityCollector = _world.GetTileEntity(_blockPos) as TileEntityCollector;
		if (tileEntityCollector == null)
		{
			return false;
		}
		_player.AimingGun = false;
		LockManager.Instance.LockRequestLocal(tileEntityCollector, null, 0);
		return true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool takeItemWithTimerCanTake(Vector3i _blockPos, EntityAlive _player)
	{
		if ((GameManager.Instance.World.GetTileEntity(_blockPos) as TileEntityCollector).IsEmpty())
		{
			return true;
		}
		GameManager.ShowTooltip(_player as EntityPlayerLocal, Localization.Get("ttWorkstationNotEmpty", false, null), string.Empty, "ui_denied", null, false, false, 0f);
		return false;
	}

	public bool UsesFuel()
	{
		bool flag = false;
		switch (this.CollectorType)
		{
		case BlockCollector.CollectorTypes.DewCollector:
			flag = XUiM_Recipes.DewCollectorInput == 0f;
			break;
		case BlockCollector.CollectorTypes.Apiary:
			flag = XUiM_Recipes.ApiaryInput == 0f;
			break;
		}
		return !flag && this.fuelTypes.Values.Count > 0;
	}

	public bool UsesCatalyst()
	{
		return this.CatalystTypes.Length != 0;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int modifyTime(int time, float modifier)
	{
		if (modifier == 0f)
		{
			time = 0;
		}
		else
		{
			time = (int)(1f / modifier * (float)time);
		}
		return time;
	}

	public int GetSandboxModifiedTime(int time)
	{
		if (time != 0)
		{
			switch (this.CollectorType)
			{
			case BlockCollector.CollectorTypes.DewCollector:
				time = this.modifyTime(time, XUiM_Recipes.DewCollectorTimeModifier);
				break;
			case BlockCollector.CollectorTypes.Apiary:
				time = this.modifyTime(time, XUiM_Recipes.ApiaryTimeModifier);
				break;
			}
			if (time == 0)
			{
				time = -1;
			}
		}
		return time;
	}

	public int GetSandboxModifiedFuelNeeded(int cost)
	{
		switch (this.CollectorType)
		{
		case BlockCollector.CollectorTypes.DewCollector:
			cost = (int)(XUiM_Recipes.DewCollectorInput * (float)cost);
			break;
		case BlockCollector.CollectorTypes.Apiary:
			cost = (int)(XUiM_Recipes.ApiaryInput * (float)cost);
			break;
		}
		return cost;
	}

	public int GetSandboxModifiedOutput(int cost)
	{
		switch (this.CollectorType)
		{
		case BlockCollector.CollectorTypes.DewCollector:
			cost = (int)(XUiM_Recipes.DewCollectorOutput * (float)cost);
			break;
		case BlockCollector.CollectorTypes.Apiary:
			cost = (int)(XUiM_Recipes.ApiaryOutput * (float)cost);
			break;
		}
		return cost;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropCollectorType = "CollectorType";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropFuelTypes = "FuelTypes";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropOutputTypes = "OutputTypes";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropOutputs = "Outputs";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropOpenSound = "OpenSound";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropRunningSound = "RunningSound";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropActivateSound = "ActivateSound";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropCloseSound = "CloseSound";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropTakeDelay = "TakeDelay";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropItemIconBackdrop = "ItemIconBackdrop";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropLootLabelKey = "LootLabelKey";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropActivationEvent = "ActivationEvent";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropCloseEvent = "CloseEvent";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropRequiredMods = "RequiredMods";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropRequiredModsOnly = "RequiredModsOnly";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropModTransformEnableNames = "ModTransformEnableNames";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropModTransformDisableNames = "ModTransformDisableNames";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropModTypes = "ModTypes";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropItemBackgroundColor = "ItemBackgroundColor";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropEmptyText = "EmptyText";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropHasItem1Text = "HasItem1Text";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropHasItem2Text = "HasItem2Text";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropOutputGridHeight = "OutputGridHeight";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropFuelGridHeight = "FuelGridHeight";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropFuelTitleText = "FuelTitleText";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropFuelTitleSprite = "FuelTitleSprite";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropFuelTitleSpriteAtlas = "FuelTitleSpriteAtlas";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropFuelTypesSprites = "FuelTypesSprites";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropCatalystGridHeight = "CatalystGridHeight";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropCatalystTypes = "CatalystTypes";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropCatalystMultiplier = "CatalystMultiplier";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropCatalystRequirements = "CatalystRequirements";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropCatalystConvert = "CatalystConvert";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropCatalystTitleText = "CatalystTitleText";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropCatalystTitleSprite = "CatalystTitleSprite";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropCatalystTitleSpriteAtlas = "CatalystTitleSpriteAtlas";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropCatalystTypesSprites = "CatalystTypesSprites";

	[PublicizedFrom(EAccessModifier.Private)]
	public string[] outputs;

	[PublicizedFrom(EAccessModifier.Protected)]
	public BlockCollector.CollectorTypes CollectorType;

	public Dictionary<BlockCollector.OutputType, List<int>> OrderedSlotOutputs = new Dictionary<BlockCollector.OutputType, List<int>>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<string, BlockCollector.FuelType> fuelTypes = new Dictionary<string, BlockCollector.FuelType>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<string, BlockCollector.OutputType> outputTypes = new Dictionary<string, BlockCollector.OutputType>();

	public string OpenSound;

	public string CloseSound;

	public string RunningSound;

	public string ActivateSound;

	[PublicizedFrom(EAccessModifier.Protected)]
	public float TakeDelay = 2f;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string[] itemIconBackdrop;

	[PublicizedFrom(EAccessModifier.Private)]
	public string itemBackgroundColor = "255,255,255,255";

	[PublicizedFrom(EAccessModifier.Private)]
	public string lootLabelKey;

	[PublicizedFrom(EAccessModifier.Private)]
	public string activationEvent;

	[PublicizedFrom(EAccessModifier.Private)]
	public string closeEvent;

	[PublicizedFrom(EAccessModifier.Private)]
	public string[] requiredMods;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool requiredModsOnly;

	[PublicizedFrom(EAccessModifier.Private)]
	public string[] modTransformEnableNames;

	[PublicizedFrom(EAccessModifier.Private)]
	public string[] modTransformDisableNames;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string emptyText = "CollectorEmptyText";

	[PublicizedFrom(EAccessModifier.Protected)]
	public string hasItem1Text = "CollectorHasItem1Text";

	[PublicizedFrom(EAccessModifier.Protected)]
	public string hasItem2Text = "CollectorHasItem2Text";

	[PublicizedFrom(EAccessModifier.Protected)]
	public int fuelGridHeight = 1;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int catalystGridHeight = 1;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int outputGridInitialHeight = 1;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string fuelTitleText;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string fuelTitleSprite = string.Empty;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string fuelTitleSpriteAtlas = string.Empty;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string catalystTitleText;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string catalystTitleSprite = string.Empty;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string catalystTitleSpriteAtlas = string.Empty;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string[] fuelTypesSprites = new string[0];

	[PublicizedFrom(EAccessModifier.Protected)]
	public string[] fuelTypesSpriteAtlases = new string[0];

	[PublicizedFrom(EAccessModifier.Protected)]
	public string[] catalystTypesSprites = new string[0];

	[PublicizedFrom(EAccessModifier.Protected)]
	public string[] catalystTypesSpriteAtlases = new string[0];

	public BlockCollector.ModEffectTypes[] ModTypes;

	[PublicizedFrom(EAccessModifier.Private)]
	public string[] catalystTypes = new string[0];

	public Dictionary<string, int> CatalystMultipliers = new Dictionary<string, int>();

	public Dictionary<string, int> CatalystRequirements = new Dictionary<string, int>();

	public BlockCollector.CatalystConvert[] CatalystConverts = new BlockCollector.CatalystConvert[0];

	[PublicizedFrom(EAccessModifier.Private)]
	public new BlockActivationCommand[] cmds = new BlockActivationCommand[]
	{
		new BlockActivationCommand("Search", "search", true, false, null),
		new BlockActivationCommand("take", "hand", false, false, null)
	};

	[PublicizedFrom(EAccessModifier.Protected)]
	public enum CollectorTypes
	{
		DewCollector,
		Apiary,
		ChickenCoop
	}

	public class CatalystConvert
	{
		public CatalystConvert(string _convertFrom, string _convertTo)
		{
			this.convertFrom = _convertFrom;
			this.convertTo = _convertTo;
		}

		public ItemStack Convert(ItemStack _inStack)
		{
			ItemStack itemStack = null;
			if (!_inStack.IsEmpty() && _inStack.itemValue.ItemClass.Name == this.convertFrom)
			{
				itemStack = new ItemStack(ItemClass.GetItem(this.convertTo, false), _inStack.count);
			}
			return itemStack;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public string convertFrom;

		[PublicizedFrom(EAccessModifier.Private)]
		public string convertTo;
	}

	public class FuelType
	{
		public FuelType(string ftDef)
		{
			string[] array = ftDef.Split(',', StringSplitOptions.None);
			this.Name = array[0];
			this.Items = array.Skip(1).ToArray<string>();
			for (int i = 0; i < this.Items.Length; i++)
			{
				this.Items[i] = this.Items[i].Trim();
			}
		}

		public string Name;

		public string[] Items;
	}

	public class OutputType
	{
		public OutputType(string otDef)
		{
			string[] array = otDef.Split(",", StringSplitOptions.None);
			int num = 0;
			this.Name = array[num++];
			this.Fuel = array[num++];
			this.FuelCost = this.safeParseInt(array[num++]);
			this.AdditionalFuelCost = this.safeParseInt(array[num++]);
			this.DiscountedFuelDivisor = this.safeParseInt(array[num++]);
			this.OutputItem = array[num++];
			this.OutputItemModded = array[num++];
			this.ModdedConvertSpeedMultiplier = this.safeParseInt(array[num++]);
			this.ModdedConvertCountMultiplier = this.safeParseInt(array[num++]);
			this.MinConvertTime = this.safeParseInt(array[num++]);
			this.MaxConvertTime = this.safeParseInt(array[num++]);
			this.ConvertSound = array[num++];
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public int safeParseInt(string intString)
		{
			int num;
			if (!int.TryParse(intString, out num))
			{
				num = 0;
			}
			return num;
		}

		public string Name;

		public string Fuel;

		public int FuelCost;

		public int AdditionalFuelCost;

		public int DiscountedFuelDivisor;

		public string OutputItem;

		public string OutputItemModded;

		public int ModdedConvertSpeedMultiplier;

		public int ModdedConvertCountMultiplier;

		public int MinConvertTime;

		public int MaxConvertTime;

		public string ConvertSound;
	}

	public enum ModEffectTypes
	{
		Type,
		Speed,
		Count,
		Modify,
		Expand,
		Cost
	}
}
