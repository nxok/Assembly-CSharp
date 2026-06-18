using System;
using DynamicMusic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioMixerManager : MonoBehaviour
{
	public void Update()
	{
		if (GameManager.Instance == null)
		{
			return;
		}
		if (GameManager.Instance.World != null)
		{
			EntityPlayerLocal primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
			if (primaryPlayer != null)
			{
				bool isUnderwaterCamera = primaryPlayer.IsUnderwaterCamera;
				if (primaryPlayer.isDeafened)
				{
					if (!this.wasDeafened)
					{
						this.transitionTo(this.deafenedSnapshot);
					}
				}
				else if (primaryPlayer.isStunned)
				{
					if (!this.wasStunned || this.wasDeafened)
					{
						this.transitionTo(this.stunnedSnapshot);
					}
				}
				else if (isUnderwaterCamera)
				{
					if (!this.bCameraWasUnderWater || this.wasStunned || this.wasDeafened)
					{
						this.transitionTo(this.underwaterSnapshot);
					}
				}
				else if (this.wasStunned || this.wasDeafened || this.bCameraWasUnderWater)
				{
					this.transitionTo(this.defaultSnapshot);
				}
				this.bCameraWasUnderWater = isUnderwaterCamera;
				this.wasStunned = primaryPlayer.isStunned;
				this.wasDeafened = primaryPlayer.isDeafened;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void transitionTo(AudioMixerManager.SnapshotController _snapshot)
	{
		_snapshot.snapshot.TransitionTo(_snapshot.transitionToTime);
		if (GamePrefs.GetBool(EnumGamePrefs.OptionsDynamicMusicEnabled) && !GameManager.Instance.IsEditMode())
		{
			MixerController.Instance.OnSnapshotTransition();
		}
	}

	public AudioMixerManager.SnapshotController underwaterSnapshot;

	public AudioMixerManager.SnapshotController stunnedSnapshot;

	public AudioMixerManager.SnapshotController deafenedSnapshot;

	public AudioMixerManager.SnapshotController defaultSnapshot;

	public bool bCameraWasUnderWater;

	public bool wasStunned;

	public bool wasDeafened;

	[Serializable]
	public class SnapshotController
	{
		public AudioMixerSnapshot snapshot;

		public float transitionToTime = 1f;
	}
}
