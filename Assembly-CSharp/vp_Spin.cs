using System;
using UnityEngine;

public class vp_Spin : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Start()
	{
		this.m_Transform = base.transform;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Update()
	{
		this.m_Transform.Rotate(this.RotationSpeed * Time.deltaTime);
	}

	public Vector3 RotationSpeed = new Vector3(0f, 90f, 0f);

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Transform m_Transform;
}
