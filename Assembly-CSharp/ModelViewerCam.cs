using System;
using UnityEngine;

public class ModelViewerCam : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Private)]
	public void Start()
	{
		Cursor.lockState = 1;
		Cursor.visible = false;
		this.envMaterial.SetTexture("_Tex", this.envTexture[0]);
		DynamicGI.UpdateEnvironment();
		RenderSettings.skybox.SetFloat("_Rotation", (float)this.nextRotation);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		this.rotationX += Input.GetAxis("Mouse X") * this.cameraSensitivity * Time.deltaTime;
		this.rotationY += Input.GetAxis("Mouse Y") * this.cameraSensitivity * Time.deltaTime;
		this.rotationY = Mathf.Clamp(this.rotationY, -90f, 90f);
		base.transform.localRotation = Quaternion.AngleAxis(this.rotationX, Vector3.up);
		base.transform.localRotation *= Quaternion.AngleAxis(this.rotationY, Vector3.left);
		if (Input.GetKey(304) || Input.GetKey(303))
		{
			base.transform.position += base.transform.forward * (this.normalMoveSpeed * this.fastMoveFactor) * Input.GetAxis("Vertical") * Time.deltaTime;
			base.transform.position += base.transform.right * (this.normalMoveSpeed * this.fastMoveFactor) * Input.GetAxis("Horizontal") * Time.deltaTime;
		}
		else if (Input.GetKey(306) || Input.GetKey(305))
		{
			base.transform.position += base.transform.forward * (this.normalMoveSpeed * this.slowMoveFactor) * Input.GetAxis("Vertical") * Time.deltaTime;
			base.transform.position += base.transform.right * (this.normalMoveSpeed * this.slowMoveFactor) * Input.GetAxis("Horizontal") * Time.deltaTime;
		}
		else
		{
			base.transform.position += base.transform.forward * this.normalMoveSpeed * Input.GetAxis("Vertical") * Time.deltaTime;
			base.transform.position += base.transform.right * this.normalMoveSpeed * Input.GetAxis("Horizontal") * Time.deltaTime;
		}
		if (Input.GetKey(101))
		{
			base.transform.position += base.transform.up * this.climbSpeed * Time.deltaTime;
		}
		if (Input.GetKey(113))
		{
			base.transform.position -= base.transform.up * this.climbSpeed * Time.deltaTime;
		}
		if (Input.GetKeyDown(279) && Cursor.lockState == 1)
		{
			Cursor.lockState = SoftCursor.DefaultCursorLockState;
			Cursor.visible = true;
		}
		float axis = Input.GetAxis("Mouse ScrollWheel");
		if (axis > 0f)
		{
			this.currentTexture++;
		}
		else if (axis < 0f)
		{
			this.currentTexture--;
		}
		if (this.currentTexture < 0)
		{
			this.currentTexture = this.envTexture.Length - 1;
		}
		if (this.currentTexture > this.envTexture.Length - 1)
		{
			this.currentTexture = 0;
		}
		this.envMaterial.SetTexture("_Tex", this.envTexture[this.currentTexture]);
		DynamicGI.UpdateEnvironment();
		if (Input.GetKeyDown(102))
		{
			this.flashlight.enabled = !this.flashlight.enabled;
		}
		if (Input.GetMouseButton(0))
		{
			this.nextRotation++;
			RenderSettings.skybox.SetFloat("_Rotation", (float)(this.nextRotation * this.skyRotationSpeed));
		}
		if (Input.GetMouseButtonDown(1))
		{
			this.toggleBool = !this.toggleBool;
			this.spheres.SetActive(this.toggleBool);
		}
		if (Input.GetKeyDown(99))
		{
			this.toggleBoolOff = !this.toggleBoolOff;
			this.characters.SetActive(this.toggleBoolOff);
		}
		if (Input.GetKeyDown(111))
		{
			this.toggleBoolAnimalsOff = !this.toggleBoolAnimalsOff;
			this.animals.SetActive(this.toggleBoolAnimalsOff);
		}
		if (Input.GetKeyDown(112))
		{
			this.toggleBoolOffPlane = !this.toggleBoolOffPlane;
			this.plane.SetActive(this.toggleBoolOffPlane);
		}
		if (Input.GetKeyDown(108))
		{
			this.sunlight.enabled = !this.sunlight.enabled;
		}
		if (Input.GetKey(114))
		{
			this.nextSunRotation++;
			this.sunlight.transform.localEulerAngles = new Vector3(30f, (float)this.nextSunRotation, 0f);
		}
	}

	public float cameraSensitivity = 90f;

	public float climbSpeed = 4f;

	public float normalMoveSpeed = 10f;

	public float slowMoveFactor = 0.25f;

	public float fastMoveFactor = 3f;

	public Material envMaterial;

	public Texture[] envTexture;

	public int currentTexture;

	public Light flashlight;

	public Light sunlight;

	public GameObject spheres;

	public GameObject characters;

	public GameObject plane;

	public GameObject animals;

	public int skyRotationSpeed;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int nextRotation;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int nextSunRotation;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float rotationX = 180f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float rotationY;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool toggleBool = true;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool toggleBoolOff;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool toggleBoolAnimalsOff;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool toggleBoolOffPlane;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool isPaused;
}
