using System;
using System.Collections.Generic;
using UnityEngine;

public class vp_PlatformSwitch : vp_Interactable
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Start()
	{
		base.Start();
		if (this.AudioSource == null)
		{
			this.AudioSource = ((base.GetComponent<AudioSource>() == null) ? base.gameObject.AddComponent<AudioSource>() : base.GetComponent<AudioSource>());
		}
	}

	public override bool TryInteract(vp_FPPlayerEventHandler player)
	{
		if (this.Platform == null)
		{
			return false;
		}
		if (this.m_Player == null)
		{
			this.m_Player = player;
		}
		if (Time.time < this.m_Timeout)
		{
			return false;
		}
		this.PlaySound();
		this.Platform.SendMessage("GoTo", (this.Platform.TargetedWaypoint == 0) ? 1 : 0, 1);
		this.m_Timeout = Time.time + this.SwitchTimeout;
		this.m_IsSwitched = !this.m_IsSwitched;
		return true;
	}

	public virtual void PlaySound()
	{
		if (this.AudioSource == null)
		{
			Debug.LogWarning("Audio Source is not set");
			return;
		}
		if (this.SwitchSounds.Count == 0)
		{
			return;
		}
		AudioClip audioClip = this.SwitchSounds[Random.Range(0, this.SwitchSounds.Count)];
		if (audioClip == null)
		{
			return;
		}
		this.AudioSource.pitch = Random.Range(this.SwitchPitchRange.x, this.SwitchPitchRange.y);
		this.AudioSource.PlayOneShot(audioClip);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnTriggerEnter(Collider col)
	{
		if (this.InteractType != vp_Interactable.vp_InteractType.Trigger)
		{
			return;
		}
		foreach (string text in this.RecipientTags)
		{
			if (col.gameObject.tag == text)
			{
				goto IL_004F;
			}
		}
		return;
		IL_004F:
		if (this.m_Player == null)
		{
			this.m_Player = Object.FindAnyObjectByType<vp_FPPlayerEventHandler>();
		}
		if (this.m_Player.Interactable.Get() != null && this.m_Player.Interactable.Get().GetComponent<Collider>() == col)
		{
			return;
		}
		this.TryInteract(this.m_Player);
	}

	public float SwitchTimeout;

	public vp_MovingPlatform Platform;

	public AudioSource AudioSource;

	public Vector2 SwitchPitchRange = new Vector2(1f, 1.5f);

	public List<AudioClip> SwitchSounds = new List<AudioClip>();

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public bool m_IsSwitched;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float m_Timeout;
}
