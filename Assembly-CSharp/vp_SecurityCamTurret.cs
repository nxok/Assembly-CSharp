using System;
using UnityEngine;

public class vp_SecurityCamTurret : vp_SimpleAITurret
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Start()
	{
		base.Start();
		this.m_Transform = base.transform;
		this.m_AngleBob = base.gameObject.AddComponent<vp_AngleBob>();
		this.m_AngleBob.BobAmp.y = this.SwivelAmp;
		this.m_AngleBob.BobRate.y = this.SwivelRate;
		this.m_AngleBob.YOffset = this.SwivelOffset;
		this.m_AngleBob.FadeToTarget = true;
		this.SwivelRotation = this.Swivel.transform.eulerAngles;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Update()
	{
		base.Update();
		if (this.m_Target != null && this.m_AngleBob.enabled)
		{
			this.m_AngleBob.enabled = false;
			this.vp_ResumeSwivelTimer.Cancel();
		}
		if (this.m_Target == null && !this.m_AngleBob.enabled && !this.vp_ResumeSwivelTimer.Active)
		{
			vp_Timer.In(this.WakeInterval * 2f, delegate
			{
				this.m_AngleBob.enabled = true;
			}, this.vp_ResumeSwivelTimer);
		}
		this.SwivelRotation.y = this.m_Transform.eulerAngles.y;
		this.Swivel.transform.eulerAngles = this.SwivelRotation;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public vp_AngleBob m_AngleBob;

	public GameObject Swivel;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 SwivelRotation = Vector3.zero;

	public float SwivelAmp = 100f;

	public float SwivelRate = 0.5f;

	public float SwivelOffset;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public vp_Timer.Handle vp_ResumeSwivelTimer = new vp_Timer.Handle();
}
