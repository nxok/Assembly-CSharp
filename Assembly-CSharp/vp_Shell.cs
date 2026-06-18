using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(AudioSource))]
public class vp_Shell : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Private)]
	public void Awake()
	{
		this.m_Transform = base.transform;
		this.m_Rigidbody = base.GetComponent<Rigidbody>();
		this.m_Audio = base.GetComponent<AudioSource>();
		base.GetComponent<AudioSource>().playOnAwake = false;
		base.GetComponent<AudioSource>().dopplerLevel = 0f;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnEnable()
	{
		this.m_RestAngleFunc = null;
		this.m_RemoveTime = Time.time + this.LifeTime;
		this.m_RestTime = Time.time + this.LifeTime * 0.25f;
		this.m_Rigidbody.maxAngularVelocity = 100f;
		this.m_Rigidbody.velocity = Vector3.zero;
		this.m_Rigidbody.angularVelocity = Vector3.zero;
		this.m_Rigidbody.constraints = 0;
		base.GetComponent<Collider>().enabled = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		if (this.m_RestAngleFunc == null)
		{
			if (Time.time > this.m_RestTime)
			{
				this.DecideRestAngle();
			}
		}
		else
		{
			this.m_RestAngleFunc();
		}
		if (Time.time > this.m_RemoveTime)
		{
			this.m_Transform.localScale = Vector3.Lerp(this.m_Transform.localScale, Vector3.zero, Time.deltaTime * 60f * 0.2f);
			if (Time.time > this.m_RemoveTime + 0.5f)
			{
				vp_Utility.Destroy(base.gameObject);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnCollisionEnter(Collision collision)
	{
		if (collision.relativeVelocity.magnitude > 2f)
		{
			if (Random.value > 0.5f)
			{
				this.m_Rigidbody.AddRelativeTorque(-Random.rotation.eulerAngles * 0.15f);
			}
			else
			{
				this.m_Rigidbody.AddRelativeTorque(Random.rotation.eulerAngles * 0.15f);
			}
			if (this.m_Audio != null && this.m_BounceSounds.Count > 0)
			{
				this.m_Audio.pitch = Time.timeScale;
				this.m_Audio.PlayOneShot(this.m_BounceSounds[Random.Range(0, this.m_BounceSounds.Count)]);
				return;
			}
		}
		else if (Random.value > this.m_Persistence)
		{
			base.GetComponent<Collider>().enabled = false;
			this.m_RemoveTime = Time.time + 0.5f;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void DecideRestAngle()
	{
		if (Mathf.Abs(this.m_Transform.eulerAngles.x - 270f) < 55f)
		{
			RaycastHit raycastHit;
			if (Physics.Raycast(new Ray(this.m_Transform.position, Vector3.down), ref raycastHit, 1f) && raycastHit.normal == Vector3.up)
			{
				this.m_RestAngleFunc = new vp_Shell.RestAngleFunc(this.UpRight);
				this.m_Rigidbody.constraints = 80;
			}
			return;
		}
		this.m_RestAngleFunc = new vp_Shell.RestAngleFunc(this.TippedOver);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void UpRight()
	{
		this.m_Transform.rotation = Quaternion.Lerp(this.m_Transform.rotation, Quaternion.Euler(-90f, this.m_Transform.rotation.y, this.m_Transform.rotation.z), Time.time * (Time.deltaTime * 60f * 0.05f));
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void TippedOver()
	{
		this.m_Transform.localRotation = Quaternion.Lerp(this.m_Transform.localRotation, Quaternion.Euler(0f, this.m_Transform.localEulerAngles.y, this.m_Transform.localEulerAngles.z), Time.time * (Time.deltaTime * 60f * 0.005f));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Transform m_Transform;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Rigidbody m_Rigidbody;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public AudioSource m_Audio;

	public float LifeTime = 10f;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float m_RemoveTime;

	public float m_Persistence = 1f;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public vp_Shell.RestAngleFunc m_RestAngleFunc;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float m_RestTime;

	public List<AudioClip> m_BounceSounds = new List<AudioClip>();

	public delegate void RestAngleFunc();
}
