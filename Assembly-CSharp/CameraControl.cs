using System;
using UnityEngine;
using UnityEngine.UI;

public class CameraControl : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Private)]
	public void Start()
	{
		this.originalRotation = base.transform.localRotation;
		this.textObject.GetComponentInChildren<Text>().text = "PAUSED";
		this.cameraLight = base.transform.GetComponent<Light>();
		this.cameraLight.enabled = false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		if (Input.GetKeyDown(27))
		{
			this.bPaused = !this.bPaused;
		}
		this.textObject.SetActive(this.bPaused);
		if (this.bPaused)
		{
			return;
		}
		if (Input.GetKeyDown(102))
		{
			this.cameraLight.enabled = !this.cameraLight.enabled;
		}
		float axis = Input.GetAxis("Mouse ScrollWheel");
		if (axis > 0f)
		{
			this.cameraLight.spotAngle += 3f;
		}
		else if (axis < 0f)
		{
			this.cameraLight.spotAngle -= 3f;
		}
		Vector3 vector = Vector3.zero;
		if (Input.GetKey(119))
		{
			vector += base.transform.forward;
		}
		if (Input.GetKey(115))
		{
			vector -= base.transform.forward;
		}
		if (Input.GetKey(97))
		{
			vector -= base.transform.right;
		}
		if (Input.GetKey(100))
		{
			vector += base.transform.right;
		}
		if (Input.GetKey(32))
		{
			vector += base.transform.up;
		}
		if (Input.GetKey(306) || Input.GetKey(99))
		{
			vector -= base.transform.up;
		}
		float num = (Input.GetKey(304) ? (this.speed * 2f) : this.speed);
		base.transform.position += vector * num;
		this.rotationX += Input.GetAxis("Mouse X") * this.sensitivityX;
		this.rotationY += Input.GetAxis("Mouse Y") * this.sensitivityY;
		Quaternion quaternion = Quaternion.AngleAxis(this.rotationX, Vector3.up);
		Quaternion quaternion2 = Quaternion.AngleAxis(this.rotationY, -Vector3.right);
		base.transform.localRotation = this.originalRotation * quaternion * quaternion2;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Quaternion originalRotation;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Light cameraLight;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float rotationX;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float rotationY;

	public float sensitivityX = 2f;

	public float sensitivityY = 2f;

	public float speed = 0.1f;

	public GameObject textObject;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool bPaused;
}
