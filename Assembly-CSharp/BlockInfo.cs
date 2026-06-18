using System;
using UnityEngine.Scripting;

[Preserve]
public class BlockInfo : Block
{
	public override void Init()
	{
		base.Init();
		if (base.Properties.Values.ContainsKey(BlockInfo.PropInfoType))
		{
			this.InformationType = Enum.Parse<BlockInfo.InformationTypes>(base.Properties.Values[BlockInfo.PropInfoType]);
		}
		base.Properties.ParseFloat(BlockInfo.PropTakeDelay, ref this.TakeDelay);
	}

	public override bool HasBlockActivationCommands(WorldBase _world, BlockValue _blockValue, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		return this.TakeDelay > 0f;
	}

	public override BlockActivationCommand[] GetBlockActivationCommands(WorldBase _world, BlockValue _blockValue, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		bool flag = _world.IsMyLandProtectedBlock(_blockPos, _world.GetGameManager().GetPersistentLocalPlayer(), false);
		this.cmds[0].enabled = flag && this.TakeDelay > 0f;
		return this.cmds;
	}

	public override string GetActivationText(WorldBase _world, BlockValue _blockValue, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		string text;
		switch (this.InformationType)
		{
		case BlockInfo.InformationTypes.Time:
			text = "The current time is [DECEA3]" + GameUtils.WorldTimeToHourMinutesString(GameManager.Instance.World.worldTime) + "[-].";
			break;
		case BlockInfo.InformationTypes.Day:
			text = string.Format("The current Day is [DECEA3]{0}[-].", GameUtils.WorldTimeToDays(GameManager.Instance.World.worldTime));
			break;
		case BlockInfo.InformationTypes.TimeAndDay:
			text = string.Format("The current Day is [DECEA3]{0}[-] and the time is [DECEA3]{1}[-].", GameUtils.WorldTimeToDays(GameManager.Instance.World.worldTime), GameUtils.WorldTimeToHourMinutesString(GameManager.Instance.World.worldTime));
			break;
		default:
			text = "";
			break;
		}
		return text;
	}

	public override bool OnBlockActivated(string _commandName, WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, EntityPlayerLocal _player)
	{
		if (_commandName == "take")
		{
			base.takeItemWithTimer(_blockPos, _blockValue, _player, this.TakeDelay);
			return true;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropTakeDelay = "TakeDelay";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropInfoType = "InfoType";

	public BlockInfo.InformationTypes InformationType;

	[PublicizedFrom(EAccessModifier.Protected)]
	public float TakeDelay = 2f;

	[PublicizedFrom(EAccessModifier.Private)]
	public new BlockActivationCommand[] cmds = new BlockActivationCommand[]
	{
		new BlockActivationCommand("take", "hand", false, false, null)
	};

	public enum InformationTypes
	{
		Time,
		Day,
		TimeAndDay
	}
}
