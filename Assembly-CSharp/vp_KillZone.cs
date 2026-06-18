using System;
using UnityEngine;

public class vp_KillZone : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Private)]
	public void OnTriggerEnter(Collider col)
	{
		if (col.gameObject.layer == 29 || col.gameObject.layer == 26)
		{
			return;
		}
		this.m_TargetDamageHandler = vp_DamageHandler.GetDamageHandlerOfCollider(col);
		if (this.m_TargetDamageHandler == null)
		{
			return;
		}
		if (this.m_TargetDamageHandler.CurrentHealth <= 0f)
		{
			return;
		}
		this.m_TargetRespawner = vp_Respawner.GetRespawnerOfCollider(col);
		if (this.m_TargetRespawner != null && Time.time < this.m_TargetRespawner.LastRespawnTime + 1f)
		{
			return;
		}
		this.m_TargetDamageHandler.Damage(new vp_DamageInfo(this.m_TargetDamageHandler.CurrentHealth, this.m_TargetDamageHandler.Transform));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public vp_DamageHandler m_TargetDamageHandler;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public vp_Respawner m_TargetRespawner;
}
