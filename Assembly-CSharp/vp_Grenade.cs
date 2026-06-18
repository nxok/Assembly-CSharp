using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(vp_DamageHandler))]
public class vp_Grenade : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Awake()
	{
		this.m_Rigidbody = base.GetComponent<Rigidbody>();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Start()
	{
		vp_Timer.In(this.LifeTime, delegate
		{
			base.transform.SendMessage("DieBySources", new Transform[] { this.m_Source, this.m_OriginalSource }, 1);
		}, null);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnEnable()
	{
		if (this.m_Rigidbody == null)
		{
			return;
		}
		if (this.RigidbodyForce != 0f)
		{
			this.m_Rigidbody.AddForce(base.transform.forward * this.RigidbodyForce, 1);
		}
		if (this.RigidbodySpin != 0f)
		{
			this.m_Rigidbody.AddTorque(Random.rotation.eulerAngles * this.RigidbodySpin);
		}
	}

	public void SetSource(Transform source)
	{
		this.m_Source = base.transform;
		this.m_OriginalSource = source;
	}

	public float LifeTime = 3f;

	public float RigidbodyForce = 10f;

	public float RigidbodySpin;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Rigidbody m_Rigidbody;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Transform m_Source;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Transform m_OriginalSource;
}
