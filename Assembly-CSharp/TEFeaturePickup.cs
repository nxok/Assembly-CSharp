using System;
using UnityEngine.Scripting;

[Preserve]
public class TEFeaturePickup : TEFeatureAbs, IFeatureSavedInPrefab
{
	public override void Init(TileEntityComposite _parent, TileEntityFeatureData _featureData)
	{
		base.Init(_parent, _featureData);
		_featureData.Props.ParseFloat(TEFeaturePickup.PropTakeDelay, ref this.TakeDelay);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void CopyFromInternal(TileEntityComposite _other)
	{
	}

	public override bool CanLockLocally(ILockContext _context, ushort _channel)
	{
		return LocalPlayerUI.GetUIForPrimaryPlayer() != null;
	}

	public override void InitBlockActivationCommands(Action<BlockActivationCommand, TileEntityComposite.EBlockCommandOrder, TileEntityFeatureData> _addCallback)
	{
		base.InitBlockActivationCommands(_addCallback);
		_addCallback(new BlockActivationCommand("Take", "hand", false, false, null), TileEntityComposite.EBlockCommandOrder.Normal, base.FeatureData);
	}

	public override bool AllowBlockActivationCommand(ITileEntityFeature _module, ReadOnlySpan<char> _commandName, WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, EntityAlive _entityFocusing)
	{
		return base.AllowBlockActivationCommand(_module, _commandName, _world, _blockPos, _blockValue, _entityFocusing) && (!this.Equals(_module) || (_world.IsMyLandProtectedBlock(_blockPos, _world.GetGameManager().GetPersistentLocalPlayer(), false) && base.Parent.PlayerPlaced && this.TakeDelay > 0f));
	}

	public override bool OnBlockActivated(ReadOnlySpan<char> _commandName, WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, EntityPlayerLocal _player)
	{
		base.OnBlockActivated(_commandName, _world, _blockPos, _blockValue, _player);
		if (base.CommandIs(_commandName, "Take"))
		{
			_player.AimingGun = false;
			Block.TakeItemWithTimer(_blockPos, _blockValue, _player, this.TakeDelay, null);
			return true;
		}
		return false;
	}

	public override void Read(PooledBinaryReader _br, TileEntity.StreamModeRead _eStreamMode)
	{
		base.Read(_br, _eStreamMode);
		int num;
		if (_eStreamMode != TileEntity.StreamModeRead.Persistency)
		{
			num = 2;
		}
		else
		{
			num = (int)_br.ReadUInt16();
		}
		if (num < 2)
		{
			_br.ReadBoolean();
		}
	}

	public override void Write(PooledBinaryWriter _bw, TileEntity.StreamModeWrite _eStreamMode)
	{
		base.Write(_bw, _eStreamMode);
		if (_eStreamMode == TileEntity.StreamModeWrite.Persistency)
		{
			_bw.Write(2);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const int Version = 2;

	[PublicizedFrom(EAccessModifier.Private)]
	public static string PropTakeDelay = "TakeDelay";

	[PublicizedFrom(EAccessModifier.Protected)]
	public float TakeDelay = 2f;
}
