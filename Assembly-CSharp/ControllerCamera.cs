using System;
using UnityEngine;

public class ControllerCamera : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Private)]
	public void Start()
	{
		this.camLookOffset.x = this.cameraTarget.transform.localPosition.x;
		this.camLookOffset.y = this.cameraTarget.transform.localPosition.y;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void LateUpdate()
	{
		if (this.setCamera == null)
		{
			this.setCamera = Camera.main.transform;
		}
		if (Input.mousePosition.x > 365f && Input.mousePosition.y < 648f && Input.mousePosition.y > 50f)
		{
			if (Input.GetMouseButton(0))
			{
				this.MouseRotationDistance = Input.GetAxisRaw("Mouse X") * 2.7f;
				this.MouseVerticalDistance = Input.GetAxisRaw("Mouse Y") * 2.7f;
			}
			else
			{
				this.MouseRotationDistance = 0f;
				this.MouseVerticalDistance = 0f;
			}
			this.MouseScrollDistance = Input.GetAxisRaw("Mouse ScrollWheel");
			if (Input.GetMouseButton(2))
			{
				this.camLookOffset.x = this.camLookOffset.x + Input.GetAxisRaw("Mouse X") * 0.001f;
				this.camLookOffset.y = this.camLookOffset.y + Input.GetAxisRaw("Mouse Y") * 0.001f;
			}
		}
		else
		{
			this.MouseRotationDistance = 0f;
			this.MouseVerticalDistance = 0f;
		}
		this.followHeight = 1.5f;
		Vector3 vector;
		vector..ctor(this.cameraTarget.transform.eulerAngles.x - this.MouseVerticalDistance, this.cameraTarget.transform.eulerAngles.y - this.MouseRotationDistance, this.cameraTarget.transform.eulerAngles.z);
		this.cameraTarget.transform.eulerAngles = vector;
		Vector3 vector2;
		vector2..ctor(this.camLookOffset.x, this.camLookOffset.y, this.cameraTarget.transform.localPosition.z);
		this.cameraTarget.transform.localPosition = vector2;
		Vector3 vector3;
		vector3..ctor(this.setCamera.localPosition.x, this.setCamera.localPosition.y, Mathf.Clamp(this.setCamera.localPosition.z, -9.73f, -9.66f));
		this.setCamera.localPosition = vector3;
		if (this.setCamera.localPosition.z >= -9.73f && this.setCamera.localPosition.z <= -9.66f && this.MouseScrollDistance != 0f)
		{
			this.setCamera.transform.Translate(-Vector3.forward * this.MouseScrollDistance * 0.02f, base.transform);
		}
	}

	public Transform setCamera;

	public Transform cameraTarget;

	public float followDistance = 5f;

	public float followHeight = 1f;

	public float followSensitivity = 2f;

	public bool useRaycast = true;

	public Vector2 axisSensitivity = new Vector2(4f, 4f);

	public float camFOV = 35f;

	public float camRotation;

	public float camHeight;

	public float camYDamp;

	public Vector2 camLookOffset = new Vector2(0f, 0f);

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float MouseRotationDistance;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float MouseVerticalDistance;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float MouseScrollDistance;
}
