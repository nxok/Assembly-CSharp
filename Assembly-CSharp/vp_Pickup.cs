using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
[RequireComponent(typeof(AudioSource))]
public abstract class vp_Pickup : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Start()
	{
		this.m_Transform = base.transform;
		this.m_Rigidbody = base.GetComponent<Rigidbody>();
		this.m_Audio = base.GetComponent<AudioSource>();
		if (Camera.main != null)
		{
			this.m_CameraMainTransform = Camera.main.transform;
		}
		base.GetComponent<Collider>().isTrigger = true;
		this.m_Audio.clip = this.PickupSound;
		this.m_Audio.playOnAwake = false;
		this.m_Audio.minDistance = 3f;
		this.m_Audio.maxDistance = 150f;
		this.m_Audio.rolloffMode = 1;
		this.m_Audio.dopplerLevel = 0f;
		this.m_SpawnPosition = this.m_Transform.position;
		this.m_SpawnScale = this.m_Transform.localScale;
		this.RespawnScaleUpDuration = ((this.m_Rigidbody == null) ? Mathf.Abs(this.RespawnScaleUpDuration) : 0f);
		if (this.BobOffset == -1f)
		{
			this.BobOffset = Random.value;
		}
		if (this.RecipientTags.Count == 0)
		{
			this.RecipientTags.Add("Player");
		}
		if (this.RemoveDuration != 0f)
		{
			vp_Timer.In(this.RemoveDuration, new vp_Timer.Callback(this.Remove), null);
		}
		if (this.m_Rigidbody != null)
		{
			if (this.RigidbodyForce != Vector3.zero)
			{
				this.m_Rigidbody.AddForce(this.RigidbodyForce, 1);
			}
			if (this.RigidbodySpin != 0f)
			{
				this.m_Rigidbody.AddTorque(Random.rotation.eulerAngles * this.RigidbodySpin);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Update()
	{
		this.UpdateMotion();
		if (this.m_Depleted && !this.m_Audio.isPlaying)
		{
			this.Remove();
		}
		if (!this.m_Depleted && this.m_Rigidbody != null && this.m_Rigidbody.IsSleeping() && !this.m_Rigidbody.isKinematic)
		{
			this.m_Rigidbody.isKinematic = true;
			foreach (Collider collider in base.GetComponents<Collider>())
			{
				if (!collider.isTrigger)
				{
					collider.enabled = false;
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void UpdateMotion()
	{
		if (this.m_Rigidbody != null)
		{
			return;
		}
		if (this.Billboard)
		{
			if (this.m_CameraMainTransform != null)
			{
				this.m_Transform.localEulerAngles = this.m_CameraMainTransform.eulerAngles;
			}
		}
		else
		{
			this.m_Transform.localEulerAngles += this.Spin * Time.deltaTime;
		}
		if (this.BobRate != 0f && this.BobAmp != 0f)
		{
			this.m_Transform.position = this.m_SpawnPosition + Vector3.up * (Mathf.Cos((Time.time + this.BobOffset) * (this.BobRate * 10f)) * this.BobAmp);
		}
		if (this.m_Transform.localScale != this.m_SpawnScale)
		{
			this.m_Transform.localScale = Vector3.Lerp(this.m_Transform.localScale, this.m_SpawnScale, Time.deltaTime / this.RespawnScaleUpDuration);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnTriggerEnter(Collider col)
	{
		if (this.m_Depleted)
		{
			return;
		}
		foreach (string text in this.RecipientTags)
		{
			if (col.gameObject.tag == text)
			{
				goto IL_004E;
			}
		}
		return;
		IL_004E:
		if (col != this.m_LastCollider)
		{
			this.m_Recipient = col.gameObject.GetComponent<vp_FPPlayerEventHandler>();
		}
		if (this.m_Recipient == null)
		{
			return;
		}
		if (this.TryGive(this.m_Recipient))
		{
			this.m_Audio.pitch = (this.PickupSoundSlomo ? Time.timeScale : 1f);
			this.m_Audio.Play();
			base.GetComponent<Renderer>().enabled = false;
			this.m_Depleted = true;
			this.m_Recipient.HUDText.Send(this.GiveMessage);
			return;
		}
		if (!this.m_AlreadyFailed)
		{
			this.m_Audio.pitch = (this.FailSoundSlomo ? Time.timeScale : 1f);
			this.m_Audio.PlayOneShot(this.PickupFailSound);
			this.m_AlreadyFailed = true;
			this.m_Recipient.HUDText.Send(this.FailMessage);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnTriggerExit(Collider col)
	{
		this.m_AlreadyFailed = false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual bool TryGive(vp_FPPlayerEventHandler player)
	{
		return player.AddItem.Try(new object[] { this.InventoryName, 1 });
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Remove()
	{
		if (this == null)
		{
			return;
		}
		if (this.RespawnDuration == 0f)
		{
			vp_Utility.Destroy(base.gameObject);
			return;
		}
		if (!this.m_RespawnTimer.Active)
		{
			vp_Utility.Activate(base.gameObject, false);
			vp_Timer.In(this.RespawnDuration, new vp_Timer.Callback(this.Respawn), this.m_RespawnTimer);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Respawn()
	{
		if (this.m_Transform == null)
		{
			return;
		}
		if (Camera.main != null)
		{
			this.m_CameraMainTransform = Camera.main.transform;
		}
		this.m_RespawnTimer.Cancel();
		this.m_Transform.position = this.m_SpawnPosition;
		if (this.m_Rigidbody == null && this.RespawnScaleUpDuration > 0f)
		{
			this.m_Transform.localScale = Vector3.zero;
		}
		base.GetComponent<Renderer>().enabled = true;
		vp_Utility.Activate(base.gameObject, true);
		this.m_Audio.pitch = (this.RespawnSoundSlomo ? Time.timeScale : 1f);
		this.m_Audio.PlayOneShot(this.RespawnSound);
		this.m_Depleted = false;
		if (this.BobOffset == -1f)
		{
			this.BobOffset = Random.value;
		}
		if (this.m_Rigidbody != null)
		{
			this.m_Rigidbody.isKinematic = false;
			foreach (Collider collider in base.GetComponents<Collider>())
			{
				if (!collider.isTrigger)
				{
					collider.enabled = true;
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public vp_Pickup()
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Transform m_Transform;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Rigidbody m_Rigidbody;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public AudioSource m_Audio;

	public string InventoryName = "Unnamed";

	public List<string> RecipientTags = new List<string>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Collider m_LastCollider;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public vp_FPPlayerEventHandler m_Recipient;

	public string GiveMessage = "Picked up an item";

	public string FailMessage = "You currently can't pick up this item!";

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Vector3 m_SpawnPosition = Vector3.zero;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Vector3 m_SpawnScale = Vector3.zero;

	public bool Billboard;

	public Vector3 Spin = Vector3.zero;

	public float BobAmp;

	public float BobRate;

	public float BobOffset = -1f;

	public Vector3 RigidbodyForce = Vector3.zero;

	public float RigidbodySpin;

	public float RespawnDuration = 10f;

	public float RespawnScaleUpDuration;

	public float RemoveDuration;

	public AudioClip PickupSound;

	public AudioClip PickupFailSound;

	public AudioClip RespawnSound;

	public bool PickupSoundSlomo = true;

	public bool FailSoundSlomo = true;

	public bool RespawnSoundSlomo = true;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public bool m_Depleted;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public bool m_AlreadyFailed;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public vp_Timer.Handle m_RespawnTimer = new vp_Timer.Handle();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Transform m_CameraMainTransform;
}
