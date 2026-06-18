using System;
using UnityEngine.Scripting;

[Preserve]
public class ItemActionData
{
	public ItemActionData(ItemInventoryData _inventoryData, int _indexInEntityOfAction)
	{
		this.invData = _inventoryData;
		this.indexInEntityOfAction = _indexInEntityOfAction;
		this.ActionTags = FastTags<TagGroup.Global>.Parse((_indexInEntityOfAction == 0) ? "primary" : ((_indexInEntityOfAction == 1) ? "secondary" : "action2"));
		this.EventParms = new MinEventParams();
		this.hitInfo = new WorldRayHitInfo();
	}

	public WorldRayHitInfo GetUpdatedHitInfo()
	{
		this.hitInfo.CopyFrom(Voxel.voxelRayHitInfo);
		return this.hitInfo;
	}

	public ItemInventoryData invData;

	public float lastUseTime;

	public int indexInEntityOfAction;

	public bool bWaitForRelease;

	public ItemActionAttack.AttackHitInfo attackDetails;

	public FastTags<TagGroup.Global> ActionTags;

	public bool HasExecuted;

	public bool uiOpenedByMe;

	public MinEventParams EventParms;

	public WorldRayHitInfo hitInfo;
}
