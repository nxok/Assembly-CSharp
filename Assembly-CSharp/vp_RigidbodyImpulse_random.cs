using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class vp_RigidbodyImpulse_random : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Awake()
	{
		this.m_Rigidbody = base.GetComponent<Rigidbody>();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnEnable()
	{
		if (this.m_Rigidbody == null)
		{
			return;
		}
		Vector3 vector;
		vector..ctor(Random.Range(this.minForce.x, this.maxForce.x), Random.Range(this.minForce.y, this.maxForce.y), Random.Range(this.minForce.z, this.maxForce.z));
		float num = Random.Range(this.minRigidBodySpin, this.maxRigidBodySpin);
		if (vector != Vector3.zero)
		{
			this.m_Rigidbody.AddForce(vector, 1);
		}
		if (num != 0f)
		{
			this.m_Rigidbody.AddTorque(Random.rotation.eulerAngles * num);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Rigidbody m_Rigidbody;

	public float minRigidBodySpin = 0.2f;

	public float maxRigidBodySpin = 0.2f;

	public Vector3 minForce = new Vector3(0f, 0f, 0f);

	public Vector3 maxForce = new Vector3(0f, 0f, 0f);
}
