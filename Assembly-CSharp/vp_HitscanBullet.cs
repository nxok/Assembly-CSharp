using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class vp_HitscanBullet : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Private)]
	public void Awake()
	{
		this.m_Transform = base.transform;
		this.m_Renderer = base.GetComponent<Renderer>();
		this.m_Audio = base.GetComponent<AudioSource>();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Start()
	{
		this.m_Initialized = true;
		this.DoHit();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnEnable()
	{
		if (!this.m_Initialized)
		{
			return;
		}
		this.DoHit();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DoHit()
	{
		Ray ray;
		ray..ctor(this.m_Transform.position, this.m_Transform.forward);
		RaycastHit raycastHit;
		if (!Physics.Raycast(ray, ref raycastHit, this.Range, this.IgnoreLocalPlayer ? (-538750989) : (-738197525)))
		{
			vp_Utility.Destroy(base.gameObject);
			return;
		}
		Vector3 localScale = this.m_Transform.localScale;
		this.m_Transform.parent = raycastHit.transform;
		this.m_Transform.localPosition = raycastHit.transform.InverseTransformPoint(raycastHit.point);
		this.m_Transform.rotation = Quaternion.LookRotation(raycastHit.normal);
		if (raycastHit.transform.lossyScale == Vector3.one)
		{
			this.m_Transform.Rotate(Vector3.forward, (float)Random.Range(0, 360), 1);
		}
		else
		{
			this.m_Transform.parent = null;
			this.m_Transform.localScale = localScale;
			this.m_Transform.parent = raycastHit.transform;
		}
		Rigidbody attachedRigidbody = raycastHit.collider.attachedRigidbody;
		if (attachedRigidbody != null && !attachedRigidbody.isKinematic)
		{
			attachedRigidbody.AddForceAtPosition(ray.direction * this.Force / Time.timeScale / vp_TimeUtility.AdjustedTimeScale, raycastHit.point);
		}
		if (this.m_ImpactPrefab != null)
		{
			vp_Utility.Instantiate(this.m_ImpactPrefab, this.m_Transform.position, this.m_Transform.rotation);
		}
		if (this.m_DustPrefab != null)
		{
			vp_Utility.Instantiate(this.m_DustPrefab, this.m_Transform.position, this.m_Transform.rotation);
		}
		if (this.m_SparkPrefab != null && Random.value < this.m_SparkFactor)
		{
			vp_Utility.Instantiate(this.m_SparkPrefab, this.m_Transform.position, this.m_Transform.rotation);
		}
		if (this.m_DebrisPrefab != null)
		{
			vp_Utility.Instantiate(this.m_DebrisPrefab, this.m_Transform.position, this.m_Transform.rotation);
		}
		if (this.m_ImpactSounds.Count > 0)
		{
			this.m_Audio.pitch = Random.Range(this.SoundImpactPitch.x, this.SoundImpactPitch.y) * Time.timeScale;
			this.m_Audio.clip = this.m_ImpactSounds[Random.Range(0, this.m_ImpactSounds.Count)];
			this.m_Audio.Stop();
			this.m_Audio.Play();
		}
		if (this.m_Source != null)
		{
			raycastHit.collider.SendMessageUpwards(this.DamageMethodName, new vp_DamageInfo(this.Damage, this.m_Source), 1);
		}
		else
		{
			raycastHit.collider.SendMessageUpwards(this.DamageMethodName, this.Damage, 1);
		}
		if (this.NoDecalOnTheseLayers.Length != 0)
		{
			foreach (int num in this.NoDecalOnTheseLayers)
			{
				if (raycastHit.transform.gameObject.layer == num)
				{
					this.m_Renderer.enabled = false;
					this.TryDestroy();
					return;
				}
			}
		}
		if (this.m_Renderer != null)
		{
			vp_DecalManager.Add(base.gameObject);
			return;
		}
		vp_Timer.In(1f, new vp_Timer.Callback(this.TryDestroy), null);
	}

	public void SetSource(Transform source)
	{
		this.m_Source = source;
		if (source.transform.root == Camera.main.transform.root)
		{
			this.IgnoreLocalPlayer = true;
		}
	}

	[Obsolete("Please use 'SetSource' instead.")]
	public void SetSender(Transform sender)
	{
		this.SetSource(sender);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TryDestroy()
	{
		if (this == null)
		{
			return;
		}
		if (!this.m_Audio.isPlaying)
		{
			this.m_Renderer.enabled = true;
			vp_Utility.Destroy(base.gameObject);
			return;
		}
		vp_Timer.In(1f, new vp_Timer.Callback(this.TryDestroy), null);
	}

	public bool IgnoreLocalPlayer = true;

	public float Range = 100f;

	public float Force = 100f;

	public float Damage = 1f;

	public string DamageMethodName = "Damage";

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Transform m_Source;

	public float m_SparkFactor = 0.5f;

	public GameObject m_ImpactPrefab;

	public GameObject m_DustPrefab;

	public GameObject m_SparkPrefab;

	public GameObject m_DebrisPrefab;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public AudioSource m_Audio;

	public List<AudioClip> m_ImpactSounds = new List<AudioClip>();

	public Vector2 SoundImpactPitch = new Vector2(1f, 1.5f);

	public int[] NoDecalOnTheseLayers;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Transform m_Transform;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Renderer m_Renderer;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public bool m_Initialized;
}
