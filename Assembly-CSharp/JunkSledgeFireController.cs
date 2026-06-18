using System;
using Audio;
using UnityEngine;

public class JunkSledgeFireController : MiniTurretFireController
{
	public new void Update()
	{
		base.Update();
		if (this.ArmState == JunkSledgeFireController.ArmStates.Idle)
		{
			return;
		}
		if (this.ArmState == JunkSledgeFireController.ArmStates.Extending)
		{
			float num = Mathf.Clamp01(this.timeCounter / (this.fireRateMax * 0.5f * 0.25f));
			this.Arm1.localPosition = new Vector3(this.Arm1.localPosition.x, this.Arm1.localPosition.y, Mathf.Lerp(this.Arm1StartZ, this.Arm1EndZ, num));
			this.Arm2.localPosition = new Vector3(this.Arm2.localPosition.x, this.Arm2.localPosition.y, Mathf.Lerp(this.Arm2StartZ, this.Arm2EndZ, num));
			if (num >= 1f)
			{
				if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
				{
					this.hitTarget();
				}
				this.ArmState = JunkSledgeFireController.ArmStates.Retracting;
				this.timeCounter = 0f;
			}
		}
		else
		{
			float num2 = Mathf.Clamp01(this.timeCounter / (this.fireRateMax * 0.5f * 0.75f));
			this.Arm1.localPosition = new Vector3(this.Arm1.localPosition.x, this.Arm1.localPosition.y, Mathf.Lerp(this.Arm1EndZ, this.Arm1StartZ, num2));
			this.Arm2.localPosition = new Vector3(this.Arm2.localPosition.x, this.Arm2.localPosition.y, Mathf.Lerp(this.Arm2EndZ, this.Arm2StartZ, num2));
			if (num2 >= 1f)
			{
				this.ArmState = JunkSledgeFireController.ArmStates.Idle;
				this.timeCounter = 0f;
			}
		}
		this.timeCounter += Time.deltaTime;
	}

	public override void Fire()
	{
		this.ArmState = JunkSledgeFireController.ArmStates.Extending;
		Manager.Play(this.entityTurret, this.fireSound, 1f, true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void hitTarget()
	{
		Vector3 position = this.Cone.transform.position;
		EntityAlive entityAlive = GameManager.Instance.World.GetEntity(this.entityTurret.belongsPlayerId) as EntityAlive;
		float maxDistance = base.MaxDistance;
		Vector3 vector = this.Cone.transform.forward;
		vector *= -1f;
		Ray ray;
		ray..ctor(position + Origin.position, vector);
		Voxel.Raycast(GameManager.Instance.World, ray, maxDistance, -538750989, 128, 0.15f);
		ItemActionAttack.Hit(Voxel.voxelRayHitInfo.Clone(), this.entityTurret.belongsPlayerId, EnumDamageTypes.Bashing, base.GetDamageBlock(this.entityTurret.OriginalItemValue, BlockValue.Air, entityAlive, 1), base.GetDamageEntity(this.entityTurret.OriginalItemValue, entityAlive, 1), 1f, this.entityTurret.OriginalItemValue.PercentUsesLeft, 0f, 0f, "metal", this.damageMultiplier, this.buffActions, new ItemActionAttack.AttackHitInfo(), 1, 0, 0f, null, null, ItemActionAttack.EnumAttackMode.RealNoHarvesting, null, this.entityTurret.entityId, this.entityTurret.OriginalItemValue, false, false, false, null);
	}

	public JunkSledgeFireController.ArmStates ArmState;

	public Transform Arm1;

	public float Arm1StartZ;

	public float Arm1EndZ;

	public Transform Arm2;

	public float Arm2StartZ;

	public float Arm2EndZ;

	public float ExtentionTime;

	public float RetractionTime;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float timeCounter;

	public enum ArmStates
	{
		Idle,
		Extending,
		Retracting
	}
}
