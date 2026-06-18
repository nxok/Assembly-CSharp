using System;
using Audio;
using UnityEngine;

public class AudioPlayer : MonoBehaviour
{
	public void Play()
	{
		if (!this.refEntity)
		{
			this.refEntity = RootTransformRefEntity.AddIfEntity(base.transform);
			if (this.refEntity)
			{
				this.attachedEntity = this.refEntity.RootTransform.GetComponent<Entity>();
			}
		}
		if (this.attachedEntity)
		{
			Manager.Play(this.attachedEntity, this.soundName, 1f, false);
			this.isPlaying = true;
			this.queuedForPlaying = false;
		}
		else if (this.refEntity)
		{
			Vector3 position = this.refEntity.transform.position;
			if (position == Vector3.zero)
			{
				this.queuedForPlaying = true;
			}
			else
			{
				this.PlayAtPos(position);
			}
		}
		else
		{
			Vector3 position2 = base.transform.position;
			if (position2 == Vector3.zero)
			{
				this.queuedForPlaying = true;
			}
			else
			{
				this.PlayAtPos(position2);
			}
		}
		if (this.isPlaying && this.duration > 0f)
		{
			this.startTime = Time.time;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PlayAtPos(Vector3 _pos)
	{
		this.playPos = _pos + Origin.position;
		Manager.Play(this.playPos, this.soundName, -1, false, 1f);
		this.isPlaying = true;
		this.queuedForPlaying = false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		if (this.startDelay > 0f)
		{
			this.startDelay -= Time.deltaTime;
			return;
		}
		if (this.queuedForPlaying)
		{
			this.Play();
		}
		if (this.isPlaying && this.duration > 0f && Time.time > this.startTime + this.duration)
		{
			this.StopAudio();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void StopAudio()
	{
		if (this.isPlaying)
		{
			if (this.attachedEntity)
			{
				Manager.Stop(this.attachedEntity.entityId, this.soundName);
			}
			else
			{
				Manager.Stop(this.playPos, this.soundName);
			}
			this.isPlaying = false;
		}
	}

	public void OnEnable()
	{
		if (!this.playOnDemand)
		{
			this.queuedForPlaying = true;
		}
	}

	public void OnDisable()
	{
		this.StopAudio();
	}

	public void OnDestroy()
	{
		this.StopAudio();
	}

	public string soundName;

	public float duration = -1f;

	public bool playOnDemand;

	public float startDelay;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Entity attachedEntity;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public RootTransformRefEntity refEntity;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool queuedForPlaying;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool isPlaying;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float startTime;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 playPos;
}
