using System;
using UnityEngine;

public class vp_AngleBob : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Awake()
	{
		this.m_Transform = base.transform;
		this.m_InitialRotation = this.m_Transform.eulerAngles;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnEnable()
	{
		this.m_Transform.eulerAngles = this.m_InitialRotation;
		if (this.RandomizeBobOffset)
		{
			this.YOffset = Random.value;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Update()
	{
		if (this.BobRate.x != 0f && this.BobAmp.x != 0f)
		{
			this.m_Offset.x = vp_MathUtility.Sinus(this.BobRate.x, this.BobAmp.x, 0f);
		}
		if (this.BobRate.y != 0f && this.BobAmp.y != 0f)
		{
			this.m_Offset.y = vp_MathUtility.Sinus(this.BobRate.y, this.BobAmp.y, 0f);
		}
		if (this.BobRate.z != 0f && this.BobAmp.z != 0f)
		{
			this.m_Offset.z = vp_MathUtility.Sinus(this.BobRate.z, this.BobAmp.z, 0f);
		}
		if (this.LocalMotion)
		{
			this.m_Transform.eulerAngles = this.m_InitialRotation + Vector3.up * this.YOffset;
			this.m_Transform.localEulerAngles += this.m_Transform.TransformDirection(this.m_Offset);
			return;
		}
		if (this.FadeToTarget)
		{
			this.m_Transform.rotation = Quaternion.Lerp(this.m_Transform.rotation, Quaternion.Euler(this.m_InitialRotation + this.m_Offset + Vector3.up * this.YOffset), Time.deltaTime);
			return;
		}
		this.m_Transform.eulerAngles = this.m_InitialRotation + this.m_Offset + Vector3.up * this.YOffset;
	}

	public Vector3 BobAmp = new Vector3(0f, 0.1f, 0f);

	public Vector3 BobRate = new Vector3(0f, 4f, 0f);

	public float YOffset;

	public bool RandomizeBobOffset;

	public bool LocalMotion;

	public bool FadeToTarget;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Transform m_Transform;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Vector3 m_InitialRotation;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Vector3 m_Offset;
}
