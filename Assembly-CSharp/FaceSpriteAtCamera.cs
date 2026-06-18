using System;
using UnityEngine;

public class FaceSpriteAtCamera : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public void Start()
	{
		if (GameManager.IsDedicatedServer)
		{
			Object.Destroy(this);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void Awake()
	{
		if (GameManager.IsDedicatedServer)
		{
			Object.Destroy(this);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void OnEnable()
	{
		this.mainCamera = null;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void Update()
	{
		if (this.mainCamera == null)
		{
			this.mainCamera = Camera.main;
		}
		if (this.mainCamera != null)
		{
			base.transform.LookAt(this.mainCamera.transform.position, -Vector3.up);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Camera mainCamera;
}
