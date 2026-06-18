using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class BlockSleeper : Block
{
	public BlockSleeper()
	{
		this.IsSleeperBlock = true;
		this.HasTileEntity = true;
	}

	public override void Init()
	{
		base.Init();
		base.Properties.ParseInt(BlockSleeper.PropPose, ref this.pose);
		this.look = Vector3.forward;
		base.Properties.ParseVec(BlockSleeper.PropLookIdentity, ref this.look);
		string @string = base.Properties.GetString(BlockSleeper.PropExcludeWalkType);
		if (@string.Length > 0)
		{
			string[] array = @string.Split(',', StringSplitOptions.None);
			this.excludedWalkTypes = new List<int>();
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i] == "Crawler")
				{
					this.excludedWalkTypes.Add(21);
				}
				else
				{
					Log.Warning("Block {0}, invalid ExcludeWalkType {1}", new object[]
					{
						base.GetBlockName(),
						array[i]
					});
				}
			}
		}
		base.Properties.ParseString(BlockSleeper.PropSpawnGroup, ref this.spawnGroup);
		base.Properties.ParseEnum<BlockSleeper.eMode>(BlockSleeper.PropSpawnMode, ref this.spawnMode);
	}

	public override bool CanPlaceBlockAt(WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, bool _bOmitCollideCheck = false)
	{
		return _world.IsEditor() || base.CanPlaceBlockAt(_world, _blockPos, _blockValue, _bOmitCollideCheck);
	}

	public float GetSleeperRotation(BlockValue _blockValue)
	{
		byte rotation = _blockValue.rotation;
		switch (rotation)
		{
		case 1:
			return 90f;
		case 2:
			return 180f;
		case 3:
			return 270f;
		default:
			switch (rotation)
			{
			case 24:
				return 45f;
			case 25:
				return 135f;
			case 26:
				return 225f;
			case 27:
				return 315f;
			default:
				return 0f;
			}
			break;
		}
	}

	public bool ExcludesWalkType(int _walkType)
	{
		return this.excludedWalkTypes != null && this.excludedWalkTypes.Contains(_walkType);
	}

	public override bool IsTileEntitySavedInPrefab()
	{
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly string PropPose = "Pose";

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly string PropLookIdentity = "LookIdentity";

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly string PropExcludeWalkType = "ExcludeWalkType";

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly string PropSpawnGroup = "SpawnGroup";

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly string PropSpawnMode = "SpawnMode";

	public int pose;

	public Vector3 look;

	public string spawnGroup;

	public BlockSleeper.eMode spawnMode;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<int> excludedWalkTypes;

	[PublicizedFrom(EAccessModifier.Private)]
	public new BlockActivationCommand[] cmds = new BlockActivationCommand[]
	{
		new BlockActivationCommand("open", "dummy", true, false, null)
	};

	public enum eMode
	{
		Normal,
		Bandit,
		Infested
	}
}
