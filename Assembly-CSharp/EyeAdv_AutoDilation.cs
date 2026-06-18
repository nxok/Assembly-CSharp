using System;
using UnityEngine;

public class EyeAdv_AutoDilation : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Private)]
	public void Start()
	{
		this.eyeRenderer = base.gameObject.GetComponent<Renderer>();
		if (this.sceneLightObject != null)
		{
			this.sceneLight = this.sceneLightObject.GetComponent<Light>();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void LateUpdate()
	{
		if (this.sceneLight != null)
		{
			this.lightIntensity = this.sceneLight.intensity;
			if (this.enableAutoDilation)
			{
				if (this.currTargetDilation != this.targetDilation || this.currLightSensitivity != this.lightSensitivity)
				{
					this.dilateTime = 0f;
					this.currTargetDilation = this.targetDilation;
					this.currLightSensitivity = this.lightSensitivity;
				}
				this.lightAngle = Vector3.Angle(this.sceneLightObject.transform.forward, base.transform.forward) / 180f;
				this.targetDilation = Mathf.Lerp(1f, 0f, this.lightAngle * this.lightIntensity * this.lightSensitivity);
				this.dilateTime += Time.deltaTime * this.dilationSpeed;
				this.pupilDilation = Mathf.Clamp(this.pupilDilation, 0f, this.maxDilation);
				this.pupilDilation = Mathf.Lerp(this.pupilDilation, this.targetDilation, this.dilateTime);
				this.eyeRenderer.sharedMaterial.SetFloat("_pupilSize", this.pupilDilation);
			}
		}
	}

	public bool enableAutoDilation = true;

	public Transform sceneLightObject;

	public float lightSensitivity = 1f;

	public float dilationSpeed = 0.1f;

	public float maxDilation = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Light sceneLight;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float lightIntensity;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float lightAngle;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float dilateTime;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float pupilDilation = 0.5f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float currTargetDilation = -1f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float targetDilation;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float currLightSensitivity = -1f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Renderer eyeRenderer;
}
