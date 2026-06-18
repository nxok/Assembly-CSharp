using System;
using UnityEngine.Scripting;

[Preserve]
public class ItemActionAttackData : ItemActionData
{
	public ItemActionAttackData(ItemInventoryData _inventoryData, int _indexInEntityOfAction)
		: base(_inventoryData, _indexInEntityOfAction)
	{
		this.attackDetails = new ItemActionAttack.AttackHitInfo();
	}

	public ItemActionAttackData.HitDelegate hitDelegate;

	public delegate WorldRayHitInfo HitDelegate(out float damageScale);
}
