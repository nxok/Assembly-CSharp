using System;
using UnityEngine;

public class RotateObject : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Private)]
	public void Start()
	{
		if (!this.rotateTransform)
		{
			this.rotateTransform = base.transform;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		this.rotateTransform.localEulerAngles += this.RPM * (Time.deltaTime * 6f);
	}

	public Transform rotateTransform;

	public Vector3 RPM;
}
