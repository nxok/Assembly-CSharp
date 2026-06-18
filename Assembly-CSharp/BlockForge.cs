using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class BlockForge : BlockWorkstation
{
	public BlockForge()
	{
		this.CraftingParticleLightIntensity = 1.6f;
	}

	public override void OnBlockEntityTransformAfterActivated(WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, BlockEntityData _ebcd)
	{
		base.OnBlockEntityTransformAfterActivated(_world, _blockPos, _blockValue, _ebcd);
		this.MaterialUpdate(_world, _blockPos, _blockValue);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void checkParticles(WorldBase _world, Vector3i _blockPos, BlockValue _blockValue)
	{
		base.checkParticles(_world, _blockPos, _blockValue);
		if (_blockValue.ischild)
		{
			return;
		}
		this.MaterialUpdate(_world, _blockPos, _blockValue);
	}

	public override string GetActivationText(WorldBase _world, BlockValue _blockValue, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		if (XUiM_Recipes.DisableSmelter)
		{
			TileEntityWorkstation tileEntityWorkstation = (TileEntityWorkstation)_world.GetTileEntity(_blockPos);
			if (tileEntityWorkstation.InputSlotCount > 0 && !tileEntityWorkstation.InputIsEmpty())
			{
				return Localization.Get("useForgeMaterials", false, null);
			}
		}
		return _blockValue.Block.GetLocalizedBlockName() + "\n" + Localization.Get("useWorkstation", false, null);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void MaterialUpdate(WorldBase _world, Vector3i _blockPos, BlockValue _blockValue)
	{
		Chunk chunk = (Chunk)_world.GetChunkFromWorldPos(_blockPos);
		if (chunk != null)
		{
			BlockEntityData blockEntity = chunk.GetBlockEntity(_blockPos);
			if (blockEntity != null && blockEntity.bHasTransform)
			{
				Renderer[] componentsInChildren = blockEntity.transform.GetComponentsInChildren<MeshRenderer>(true);
				Renderer[] array = componentsInChildren;
				if (array.Length != 0)
				{
					Material material = array[0].material;
					if (material)
					{
						float num = (float)((_blockValue.meta == 0) ? 0 : 20);
						material.SetFloat("_EmissionMultiply", num);
						for (int i = 1; i < array.Length; i++)
						{
							array[i].material = material;
						}
					}
				}
			}
		}
	}
}
