using System;
using UnityEngine;

public class Detonator : MonoBehaviour
{
	public void StartCountdown()
	{
		if (base.isActiveAndEnabled)
		{
			return;
		}
		base.enabled = true;
		base.gameObject.SetActive(true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnEnable()
	{
		this._animTime = 0f;
		this._animTimeDetonator = 0f;
		base.gameObject.SetActive(true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDisable()
	{
		base.gameObject.SetActive(false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		float num = Time.deltaTime;
		num *= this.PulseRateScale;
		this._animTime += num;
		this._animTimeDetonator += num * ((this._timeRate != null) ? this._timeRate.Evaluate(this._animTime) : 1f);
		if (this._light != null && this._lightIntensity != null)
		{
			this._light.intensity = this._lightIntensity.Evaluate(this._animTimeDetonator);
		}
	}

	[SerializeField]
	[PublicizedFrom(EAccessModifier.Private)]
	public Light _light;

	[SerializeField]
	[PublicizedFrom(EAccessModifier.Private)]
	public AnimationCurve _timeRate;

	[SerializeField]
	[PublicizedFrom(EAccessModifier.Private)]
	public AnimationCurve _lightIntensity;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float _animTime;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float _animTimeDetonator;

	public float PulseRateScale = 1f;
}
