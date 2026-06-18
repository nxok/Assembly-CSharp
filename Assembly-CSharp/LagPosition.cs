using System;
using UnityEngine;

public class LagPosition : MonoBehaviour
{
	public void OnRepositionEnd()
	{
		this.Interpolate(1000f);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Interpolate(float delta)
	{
		Transform parent = this.mTrans.parent;
		if (parent != null)
		{
			Vector3 vector = parent.position + parent.rotation * this.mRelative;
			this.mAbsolute.x = Mathf.Lerp(this.mAbsolute.x, vector.x, Mathf.Clamp01(delta * this.speed.x));
			this.mAbsolute.y = Mathf.Lerp(this.mAbsolute.y, vector.y, Mathf.Clamp01(delta * this.speed.y));
			this.mAbsolute.z = Mathf.Lerp(this.mAbsolute.z, vector.z, Mathf.Clamp01(delta * this.speed.z));
			this.mTrans.position = this.mAbsolute;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Awake()
	{
		this.mTrans = base.transform;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnEnable()
	{
		if (this.mStarted)
		{
			this.ResetPosition();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Start()
	{
		this.mStarted = true;
		this.ResetPosition();
	}

	public void ResetPosition()
	{
		this.mAbsolute = this.mTrans.position;
		this.mRelative = this.mTrans.localPosition;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		this.Interpolate(this.ignoreTimeScale ? RealTime.deltaTime : Time.deltaTime);
	}

	public Vector3 speed = new Vector3(10f, 10f, 10f);

	public bool ignoreTimeScale;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Transform mTrans;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 mRelative;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 mAbsolute;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool mStarted;
}
