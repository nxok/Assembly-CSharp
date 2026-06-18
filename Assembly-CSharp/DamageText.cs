using System;
using TMPro;
using UnityEngine;

public class DamageText : MonoBehaviour
{
	public static bool Enabled
	{
		get
		{
			return DamageText.enabled || DamageText.SandboxEnabled;
		}
		set
		{
			DamageText.enabled = value;
		}
	}

	public static void Create(string _text, Color _color, Vector3 _worldPos, Vector3 _velocity, float _scale = 1f)
	{
		GameObject gameObject = (GameObject)Object.Instantiate(Resources.Load("Prefabs/DamageText"));
		DamageText component = gameObject.GetComponent<DamageText>();
		TextMeshPro component2 = gameObject.GetComponent<TextMeshPro>();
		component.textMeshPro = component2;
		component2.text = _text;
		component2.color = _color;
		component2.rectTransform.localScale = new Vector3(_scale, _scale, _scale);
		component.worldPos = _worldPos;
		component.velocity = _velocity;
		component.cameraT = Camera.main.transform;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void LateUpdate()
	{
		float deltaTime = Time.deltaTime;
		this.TimeDuration -= deltaTime;
		if (this.TimeDuration <= 0f || !this.cameraT)
		{
			Object.Destroy(base.gameObject);
			return;
		}
		this.textMeshPro.alpha = Utils.FastLerp(0f, 1f, this.TimeDuration * 2f);
		this.velocityDecayDelay -= deltaTime;
		if (this.velocityDecayDelay <= 0f)
		{
			this.velocityDecayDelay = 0.1f;
			this.velocity *= 0.8f;
		}
		this.worldPos += this.velocity * deltaTime;
		base.transform.SetPositionAndRotation(Vector3.MoveTowards(this.worldPos - Origin.position, this.cameraT.position + this.cameraT.forward * 0.18f, 0.25f), this.cameraT.rotation);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static bool enabled;

	public static bool SandboxEnabled;

	public float TimeDuration = 1.5f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public TextMeshPro textMeshPro;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 worldPos;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 velocity;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float velocityDecayDelay = 0.2f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Transform cameraT;
}
