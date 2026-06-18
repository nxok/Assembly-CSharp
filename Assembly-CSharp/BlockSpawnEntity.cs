using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class BlockSpawnEntity : Block
{
	public override void Init()
	{
		base.Init();
		if (!base.Properties.Values.ContainsKey("SpawnClass"))
		{
			throw new Exception(string.Format("Need 'SpawnClass' in block {0}", base.GetBlockName()));
		}
		this.spawnClasses = base.Properties.Values["SpawnClass"].Split(',', StringSplitOptions.None);
	}

	public override void OnBlockAdded(WorldBase _world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue, PlatformUserIdentifierAbs _addedByPlayer)
	{
		base.OnBlockAdded(_world, _chunk, _blockPos, _blockValue, _addedByPlayer);
		if (!GameManager.Instance.IsEditMode() && SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && this.spawnClasses.Length != 0)
		{
			EntityCreationData entityCreationData = new EntityCreationData();
			entityCreationData.id = -1;
			string text = this.spawnClasses[(int)_blockValue.meta % this.spawnClasses.Length];
			entityCreationData.entityClass = text.GetHashCode();
			entityCreationData.pos = _blockPos.ToVector3() + new Vector3(0.5f, 0.25f, 0.5f);
			entityCreationData.rot = new Vector3(0f, (float)(90 * (_blockValue.rotation & 3)), 0f);
			_chunk.AddEntityStub(entityCreationData);
			_world.GetWBT().AddScheduledBlockUpdate(_blockPos, this.blockID, 80UL);
		}
	}

	public override void OnBlockLoaded(WorldBase _world, Vector3i _blockPos, BlockValue _blockValue)
	{
		base.OnBlockLoaded(_world, _blockPos, _blockValue);
		if (_blockValue.ischild)
		{
			return;
		}
		ChunkCluster chunkCache = _world.ChunkCache;
		if (chunkCache == null)
		{
			return;
		}
		Chunk chunk = (Chunk)chunkCache.GetChunkFromWorldPos(_blockPos);
		if (chunk == null)
		{
			return;
		}
		chunk.AddEntityBlockStub(new BlockEntityData(_blockValue, _blockPos)
		{
			bNeedsTemperature = true
		});
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			_world.GetWBT().AddScheduledBlockUpdate(_blockPos, this.blockID, 300UL);
		}
	}

	public override void OnBlockEntityTransformAfterActivated(WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, BlockEntityData _ebcd)
	{
		base.OnBlockEntityTransformAfterActivated(_world, _blockPos, _blockValue, _ebcd);
		if (_blockValue.ischild)
		{
			return;
		}
		if (!_world.IsEditor())
		{
			_ebcd.transform.gameObject.SetActive(false);
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			_world.GetWBT().AddScheduledBlockUpdate(_blockPos, this.blockID, 300UL);
		}
	}

	public override bool UpdateTick(WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, bool _bRandomTick, ulong _ticksIfLoaded, GameRandom _rnd)
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			if (GameManager.Instance.World.GetEntitiesInBounds(null, new Bounds(_blockPos.ToVector3(), Vector3.one * 2f)).Count == 0 && !GameManager.Instance.IsEditMode())
			{
				ChunkCluster chunkCache = _world.ChunkCache;
				if (chunkCache == null)
				{
					return false;
				}
				if ((Chunk)chunkCache.GetChunkFromWorldPos(_blockPos) == null)
				{
					return false;
				}
				if (this.spawnClasses.Length != 0)
				{
					string text = this.spawnClasses[(int)_blockValue.meta % this.spawnClasses.Length];
					int num = -1;
					foreach (KeyValuePair<int, EntityClass> keyValuePair in EntityClass.list.Dict)
					{
						if (keyValuePair.Value.entityClassName == text)
						{
							num = keyValuePair.Key;
						}
					}
					Vector3 vector = _blockPos.ToVector3() + new Vector3(0.5f, 0.25f, 0.5f);
					Vector3 vector2;
					vector2..ctor(0f, (float)(90 * (_blockValue.rotation & 3)), 0f);
					Entity entity = EntityFactory.CreateEntity(num, vector, vector2);
					entity.SetSpawnerSource(EnumSpawnerSource.StaticSpawner);
					GameManager.Instance.World.SpawnEntityInWorld(entity);
					Log.Out("BlockSpawnEntity:: Spawn New Trader.");
				}
			}
			_world.GetWBT().AddScheduledBlockUpdate(_blockPos, this.blockID, 320UL);
		}
		return base.UpdateTick(_world, _blockPos, _blockValue, _bRandomTick, _ticksIfLoaded, _rnd);
	}

	public override bool HasBlockActivationCommands(WorldBase _world, BlockValue _blockValue, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		return true;
	}

	public override string GetActivationText(WorldBase _world, BlockValue _blockValue, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		if (!_world.IsEditor())
		{
			return null;
		}
		byte meta = _blockValue.meta;
		PlayerActionsLocal playerInput = ((EntityPlayerLocal)_entityFocusing).playerInput;
		string text = playerInput.Activate.GetBindingXuiMarkupString(XUiUtils.EmptyBindingStyle.EmptyString, XUiUtils.DisplayStyle.Plain, null) + playerInput.PermanentActions.Activate.GetBindingXuiMarkupString(XUiUtils.EmptyBindingStyle.EmptyString, XUiUtils.DisplayStyle.Plain, null);
		return string.Format(Localization.Get("editBlockSpawnEntity", false, null), text, ((int)meta < this.spawnClasses.Length) ? this.spawnClasses[(int)_blockValue.meta] : "-");
	}

	public override bool OnBlockActivated(string _commandName, WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, EntityPlayerLocal _player)
	{
		if (!(_commandName == "edit"))
		{
			return false;
		}
		if (!_world.IsEditor())
		{
			return false;
		}
		XUiC_SpawnBlockEditor.Open(_player.PlayerUI, _blockPos, _blockValue, this);
		return true;
	}

	public override BlockActivationCommand[] GetBlockActivationCommands(WorldBase _world, BlockValue _blockValue, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		this.cmds[0].enabled = _world.IsEditor();
		return this.cmds;
	}

	public string[] spawnClasses;

	[PublicizedFrom(EAccessModifier.Private)]
	public new BlockActivationCommand[] cmds = new BlockActivationCommand[]
	{
		new BlockActivationCommand("edit", "tool", false, false, null)
	};
}
