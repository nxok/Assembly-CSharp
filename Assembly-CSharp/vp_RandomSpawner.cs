using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class vp_RandomSpawner : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Private)]
	public void Awake()
	{
		if (this.SpawnObjects == null)
		{
			return;
		}
		int num = Random.Range(0, this.SpawnObjects.Count);
		if (this.SpawnObjects[num] == null)
		{
			return;
		}
		((GameObject)vp_Utility.Instantiate(this.SpawnObjects[num], base.transform.position, base.transform.rotation)).transform.Rotate(Random.rotation.eulerAngles);
		this.m_Audio = base.GetComponent<AudioSource>();
		this.m_Audio.playOnAwake = true;
		if (this.Sound != null)
		{
			this.m_Audio.rolloffMode = 1;
			this.m_Audio.clip = this.Sound;
			this.m_Audio.pitch = Random.Range(this.SoundMinPitch, this.SoundMaxPitch) * Time.timeScale;
			this.m_Audio.Play();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public AudioSource m_Audio;

	public AudioClip Sound;

	public float SoundMinPitch = 0.8f;

	public float SoundMaxPitch = 1.2f;

	public bool RandomAngle = true;

	public List<GameObject> SpawnObjects;
}
