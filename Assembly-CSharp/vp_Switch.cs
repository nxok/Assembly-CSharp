using System;
using System.Collections.Generic;
using UnityEngine;

public class vp_Switch : vp_Interactable
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
		if (this.Target == null)
		{
			return false;
		}
		if (this.m_Player == null)
		{
			this.m_Player = player;
		}
		this.PlaySound();
		this.Target.SendMessage(this.TargetMessage, 1);
		return true;
	}

	public virtual void PlaySound()
	{
		if (this.AudioSource == null)
		{
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
		this.TryInteract(this.m_Player);
	}

	public GameObject Target;

	public string TargetMessage = "";

	public AudioSource AudioSource;

	public Vector2 SwitchPitchRange = new Vector2(1f, 1.5f);

	public List<AudioClip> SwitchSounds = new List<AudioClip>();
}
