using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class vp_Debris : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Private)]
	public void Awake()
	{
		this.m_Audio = base.GetComponent<AudioSource>();
		this.m_Colliders = base.GetComponentsInChildren<Collider>();
		foreach (Collider collider in this.m_Colliders)
		{
			if (collider.GetComponent<Rigidbody>())
			{
				this.m_PiecesInitial.Add(collider, new Dictionary<string, object>
				{
					{
						"Position",
						collider.transform.localPosition
					},
					{
						"Rotation",
						collider.transform.localRotation
					}
				});
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnEnable()
	{
		this.m_Destroy = false;
		this.m_Audio.playOnAwake = true;
		foreach (Collider collider in this.m_Colliders)
		{
			if (collider.GetComponent<Rigidbody>())
			{
				collider.transform.localPosition = (Vector3)this.m_PiecesInitial[collider]["Position"];
				collider.transform.localRotation = (Quaternion)this.m_PiecesInitial[collider]["Rotation"];
				collider.GetComponent<Rigidbody>().velocity = Vector3.zero;
				collider.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
				collider.GetComponent<Rigidbody>().AddExplosionForce(this.Force / Time.timeScale / vp_TimeUtility.AdjustedTimeScale, base.transform.position, this.Radius, this.UpForce);
				Collider c = collider;
				vp_Timer.In(Random.Range(this.LifeTime * 0.5f, this.LifeTime * 0.95f), delegate
				{
					if (c != null)
					{
						vp_Utility.Destroy(c.gameObject);
					}
				}, null);
			}
		}
		vp_Timer.In(this.LifeTime, delegate
		{
			this.m_Destroy = true;
		}, null);
		if (this.Sounds.Count > 0)
		{
			this.m_Audio.rolloffMode = 1;
			this.m_Audio.clip = this.Sounds[Random.Range(0, this.Sounds.Count)];
			this.m_Audio.pitch = Random.Range(this.SoundMinPitch, this.SoundMaxPitch) * Time.timeScale;
			this.m_Audio.Play();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		if (this.m_Destroy && !base.GetComponent<AudioSource>().isPlaying)
		{
			vp_Utility.Destroy(base.gameObject);
		}
	}

	public float Radius = 2f;

	public float Force = 10f;

	public float UpForce = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public AudioSource m_Audio;

	public List<AudioClip> Sounds = new List<AudioClip>();

	public float SoundMinPitch = 0.8f;

	public float SoundMaxPitch = 1.2f;

	public float LifeTime = 5f;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public bool m_Destroy;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Collider[] m_Colliders;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Dictionary<Collider, Dictionary<string, object>> m_PiecesInitial = new Dictionary<Collider, Dictionary<string, object>>();
}
