using System;
using UnityEngine;

[RequireComponent(typeof(vp_Shooter))]
public class vp_SimpleAITurret : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Start()
	{
		this.m_Shooter = base.GetComponent<vp_Shooter>();
		this.m_Transform = base.transform;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Update()
	{
		if (!this.m_Timer.Active)
		{
			vp_Timer.In(this.WakeInterval, delegate
			{
				if (this.m_Target == null)
				{
					this.m_Target = this.ScanForLocalPlayer();
					return;
				}
				this.m_Target = null;
			}, this.m_Timer);
		}
		if (this.m_Target != null)
		{
			this.AttackTarget();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual Transform ScanForLocalPlayer()
	{
		foreach (Collider collider in Physics.OverlapSphere(this.m_Transform.position, this.ViewRange, 1073741824))
		{
			RaycastHit raycastHit;
			Physics.Linecast(this.m_Transform.position, collider.transform.position + Vector3.up, ref raycastHit);
			if (!(raycastHit.collider != null) || !(raycastHit.collider != collider))
			{
				return collider.transform;
			}
		}
		return null;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void AttackTarget()
	{
		Quaternion quaternion = Quaternion.LookRotation(this.m_Target.GetComponent<Collider>().bounds.center - this.m_Transform.position);
		this.m_Transform.rotation = Quaternion.RotateTowards(this.m_Transform.rotation, quaternion, Time.deltaTime * this.AimSpeed);
		if (Mathf.Abs(vp_3DUtility.LookAtAngleHorizontal(this.m_Transform.position, this.m_Transform.forward, this.m_Target.position)) < this.FireAngle)
		{
			this.m_Shooter.TryFire();
		}
	}

	public float ViewRange = 10f;

	public float AimSpeed = 50f;

	public float WakeInterval = 2f;

	public float FireAngle = 10f;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public vp_Shooter m_Shooter;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Transform m_Transform;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Transform m_Target;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public vp_Timer.Handle m_Timer = new vp_Timer.Handle();
}
