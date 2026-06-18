using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class vp_FPEarthquake : MonoBehaviour
{
	public vp_FPPlayerEventHandler FPPlayer
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (this.m_FPPlayer == null)
			{
				this.m_FPPlayer = Object.FindAnyObjectByType<vp_FPPlayerEventHandler>();
			}
			return this.m_FPPlayer;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnEnable()
	{
		if (this.FPPlayer != null)
		{
			this.FPPlayer.Register(this);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnDisable()
	{
		if (this.FPPlayer != null)
		{
			this.FPPlayer.Unregister(this);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void FixedUpdate()
	{
		if (Time.timeScale != 0f)
		{
			this.UpdateEarthQuake();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void UpdateEarthQuake()
	{
		if (!this.FPPlayer.CameraEarthQuake.Active)
		{
			this.m_CameraEarthQuakeForce = Vector3.zero;
			return;
		}
		this.m_CameraEarthQuakeForce = Vector3.Scale(vp_SmoothRandom.GetVector3Centered(1f), this.m_Magnitude.x * (Vector3.right + Vector3.forward) * Mathf.Min(this.m_Endtime - Time.time, 1f) * Time.timeScale);
		this.m_CameraEarthQuakeForce.y = 0f;
		if (Random.value < 0.3f * Time.timeScale)
		{
			this.m_CameraEarthQuakeForce.y = Random.Range(0f, this.m_Magnitude.y * 0.35f) * Mathf.Min(this.m_Endtime - Time.time, 1f);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnStart_CameraEarthQuake()
	{
		Vector3 vector = (Vector3)this.FPPlayer.CameraEarthQuake.Argument;
		this.m_Magnitude.x = vector.x;
		this.m_Magnitude.y = vector.y;
		this.m_Endtime = Time.time + vector.z;
		this.FPPlayer.CameraEarthQuake.AutoDuration = vector.z;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnMessage_CameraBombShake(float impact)
	{
		this.FPPlayer.CameraEarthQuake.TryStart<Vector3>(new Vector3(impact * 0.5f, impact * 0.5f, 1f));
	}

	public virtual Vector3 OnValue_CameraEarthQuakeForce
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return this.m_CameraEarthQuakeForce;
		}
		[PublicizedFrom(EAccessModifier.Protected)]
		set
		{
			this.m_CameraEarthQuakeForce = value;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Vector3 m_CameraEarthQuakeForce;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float m_Endtime;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Vector2 m_Magnitude = Vector2.zero;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public vp_FPPlayerEventHandler m_FPPlayer;
}
