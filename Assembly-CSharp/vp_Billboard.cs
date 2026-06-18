using System;
using UnityEngine;

public class vp_Billboard : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Start()
	{
		this.m_Transform = base.transform;
		if (this.m_CameraTransform == null)
		{
			this.m_CameraTransform = Camera.main.transform;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Update()
	{
		if (this.m_CameraTransform != null)
		{
			this.m_Transform.localEulerAngles = this.m_CameraTransform.eulerAngles;
		}
		this.m_Transform.localEulerAngles = this.m_Transform.localEulerAngles;
	}

	public Transform m_CameraTransform;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Transform m_Transform;
}
