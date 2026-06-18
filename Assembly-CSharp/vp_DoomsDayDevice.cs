using System;
using UnityEngine;

public class vp_DoomsDayDevice : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Awake()
	{
		this.m_Player = Object.FindAnyObjectByType<vp_FPPlayerEventHandler>();
		if (this.m_Player != null)
		{
			this.m_PlayerAudioSource = this.m_Player.GetComponent<AudioSource>();
		}
		this.m_DeviceAudioSource = base.GetComponent<AudioSource>();
		this.m_Button = GameObject.Find("ForbiddenButton");
		if (this.m_Button != null)
		{
			this.m_OriginalButtonPos = this.m_Button.transform.localPosition;
			this.m_OriginalButtonColor = this.m_Button.GetComponent<Renderer>().material.color;
		}
		this.m_PulsingLight = this.m_Button.GetComponentInChildren<vp_PulsingLight>();
		if (this.m_PulsingLight != null)
		{
			this.m_OriginalPulsingLightMaxIntensity = this.m_PulsingLight.m_MaxIntensity;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnEnable()
	{
		if (this.m_Player != null)
		{
			this.m_Player.Register(this);
		}
		if (this.m_Button != null)
		{
			this.m_Button.transform.localPosition = this.m_OriginalButtonPos;
			this.m_Button.GetComponent<Renderer>().material.color = this.m_OriginalButtonColor;
		}
		if (this.m_DeviceAudioSource != null)
		{
			this.m_DeviceAudioSource.pitch = 1f;
			this.m_DeviceAudioSource.volume = 1f;
		}
		if (this.m_PulsingLight != null)
		{
			this.m_PulsingLight.m_MaxIntensity = this.m_OriginalPulsingLightMaxIntensity;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnDisable()
	{
		if (this.m_Player != null)
		{
			this.m_Player.Unregister(this);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Update()
	{
		if (this.Initiated)
		{
			if (this.m_Button != null)
			{
				this.m_Button.GetComponent<Renderer>().material.color = Color.Lerp(this.m_Button.GetComponent<Renderer>().material.color, this.m_OriginalButtonColor * 0.2f, Time.deltaTime * 1.5f);
			}
			if (this.m_DeviceAudioSource != null)
			{
				this.m_DeviceAudioSource.pitch -= Time.deltaTime * 0.35f;
			}
			if (this.m_PulsingLight != null)
			{
				this.m_PulsingLight.m_MaxIntensity = 2.5f;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void InitiateDoomsDay()
	{
		if (this.Initiated)
		{
			return;
		}
		this.Initiated = true;
		if (this.m_Button != null)
		{
			this.m_Button.transform.localPosition += Vector3.down * 0.02f;
		}
		if (this.m_PlayerAudioSource != null)
		{
			this.m_PlayerAudioSource.PlayOneShot(this.EarthQuakeSound);
		}
		this.m_Player.CameraEarthQuake.TryStart<Vector3>(new Vector3(0.05f, 0.05f, 10f));
		vp_Timer.In(3f, delegate
		{
			base.SendMessage("Die");
		}, null);
		vp_Timer.In(3f, delegate
		{
			this.Initiated = false;
		}, null);
	}

	public AudioClip EarthQuakeSound;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public vp_FPPlayerEventHandler m_Player;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public bool Initiated;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public GameObject m_Button;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public vp_PulsingLight m_PulsingLight;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public AudioSource m_PlayerAudioSource;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public AudioSource m_DeviceAudioSource;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Vector3 m_OriginalButtonPos;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Color m_OriginalButtonColor;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float m_OriginalPulsingLightMaxIntensity;
}
