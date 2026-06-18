using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdPlaceBlockRotations : ConsoleCmdAbstract
{
	public override bool IsExecuteOnClient
	{
		get
		{
			return true;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Places all rotations of the currently held block";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "Places the block you currently hold in your hand in all supported rotations. Starts\nat the current selection box and spreads out towards the right relative to the\ncurrent view direction of the player. Spaces out each block by 1m meter.";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[] { "placeblockrotations", "pbr" };
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (!_senderInfo.IsLocalGame)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Command can only be used on clients");
			return;
		}
		if (!BlockToolSelection.Instance.SelectionActive)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("No selection active. Running this command requires an active 1x1x1 selection box.");
			return;
		}
		Vector3i selectionSize = BlockToolSelection.Instance.SelectionSize;
		Vector3i selectionStart = BlockToolSelection.Instance.SelectionStart;
		if (selectionSize != Vector3i.one)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Selection box size is not 1x1x1.");
			return;
		}
		EntityPlayerLocal primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
		BlockValue blockValue = primaryPlayer.inventory.holdingItemItemValue.ToBlockValue(false);
		ItemClassBlock.ItemBlockInventoryData itemBlockInventoryData = primaryPlayer.inventory.holdingItemData as ItemClassBlock.ItemBlockInventoryData;
		if (blockValue.isair || itemBlockInventoryData == null)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Player is not holding a block.");
			return;
		}
		float y = primaryPlayer.rotation.y;
		Vector3i zero = Vector3i.zero;
		switch (GameUtils.GetClosestDirection(y, true))
		{
		case GameUtils.DirEightWay.N:
			zero.x = 2;
			goto IL_0123;
		case GameUtils.DirEightWay.E:
			zero.z = -2;
			goto IL_0123;
		case GameUtils.DirEightWay.S:
			zero.x = -2;
			goto IL_0123;
		case GameUtils.DirEightWay.W:
			zero.z = 2;
			goto IL_0123;
		}
		throw new ArgumentOutOfRangeException();
		IL_0123:
		Vector3i vector3i = selectionStart;
		blockValue.rotation = 0;
		do
		{
			ConsoleCmdPlaceBlockRotations.PlaceBlock(blockValue, itemBlockInventoryData, vector3i, primaryPlayer);
			int num = 0;
			blockValue.rotation = blockValue.Block.BlockPlacementHelper.LimitRotation(BlockPlacement.EnumRotationMode.Advanced, ref num, default(HitInfoDetails), true, blockValue, blockValue.rotation);
			vector3i += zero;
		}
		while (blockValue.rotation != 0);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void PlaceBlock(BlockValue _blockValue, ItemClassBlock.ItemBlockInventoryData _holdingData, Vector3i _placementPos, EntityPlayerLocal _player)
	{
		BlockPlacement.Result result = new BlockPlacement.Result(BlockPlacement.EnumPlacement.Voxel, _placementPos + Vector3.one * 0.5f, _placementPos, BlockFace.None, _blockValue, _holdingData.propTransform);
		Block block = _blockValue.Block;
		block.OnBlockPlaceBefore(GameManager.Instance.World, ref result, _player, GameManager.Instance.World.GetGameRandom());
		_blockValue = result.blockValue;
		if (_holdingData.itemValue.TextureFullArray.IsDefault || Block.list[_holdingData.itemValue.type].SelectAlternates)
		{
			block.PlaceBlock(GameManager.Instance.World, result, _player);
			return;
		}
		BlockChangeInfo blockChangeInfo = new BlockChangeInfo(_placementPos, _blockValue)
		{
			textureFull = _holdingData.itemValue.TextureFullArray,
			bChangeTexture = true
		};
		GameManager.Instance.World.SetBlocksRPC(new List<BlockChangeInfo> { blockChangeInfo });
	}
}
