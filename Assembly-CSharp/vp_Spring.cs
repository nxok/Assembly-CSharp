using System;
using UnityEngine;

public class vp_Spring
{
	public bool SdtdStopping { get; set; }

	public Transform Transform
	{
		get
		{
			return this.m_Transform;
		}
		set
		{
			this.m_Transform = value;
			this.RefreshUpdateMode();
		}
	}

	public vp_Spring(Transform transform, vp_Spring.UpdateMode mode, bool autoUpdate = true)
	{
		this.Mode = mode;
		this.Transform = transform;
		this.m_AutoUpdate = autoUpdate;
	}

	public void FixedUpdate()
	{
		if (this.m_VelocityFadeInEndTime > Time.time)
		{
			this.m_VelocityFadeInCap = Mathf.Clamp01(1f - (this.m_VelocityFadeInEndTime - Time.time) / this.m_VelocityFadeInLength);
		}
		else
		{
			this.m_VelocityFadeInCap = 1f;
		}
		if (this.m_SoftForceFrame[0] != Vector3.zero)
		{
			this.AddForceInternal(this.m_SoftForceFrame[0]);
			for (int i = 0; i < 120; i++)
			{
				this.m_SoftForceFrame[i] = ((i < 119) ? this.m_SoftForceFrame[i + 1] : Vector3.zero);
				if (this.m_SoftForceFrame[i] == Vector3.zero)
				{
					break;
				}
			}
		}
		this.Calculate();
		this.m_UpdateFunc();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Position()
	{
		this.m_Transform.localPosition = this.State;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Rotation()
	{
		this.m_Transform.localEulerAngles = this.State;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Scale()
	{
		this.m_Transform.localScale = this.State;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PositionAdditiveLocal()
	{
		this.m_Transform.localPosition += this.State;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PositionAdditiveGlobal()
	{
		this.m_Transform.position += this.State;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PositionAdditiveSelf()
	{
		this.m_Transform.Translate(this.State, this.m_Transform);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RotationAdditiveLocal()
	{
		this.m_Transform.localEulerAngles += this.State;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RotationAdditiveGlobal()
	{
		this.m_Transform.eulerAngles += this.State;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ScaleAdditiveLocal()
	{
		this.m_Transform.localScale += this.State;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void None()
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void RefreshUpdateMode()
	{
		this.m_UpdateFunc = new vp_Spring.UpdateDelegate(this.None);
		switch (this.Mode)
		{
		case vp_Spring.UpdateMode.Position:
			this.State = this.m_Transform.localPosition;
			if (this.m_AutoUpdate)
			{
				this.m_UpdateFunc = new vp_Spring.UpdateDelegate(this.Position);
			}
			break;
		case vp_Spring.UpdateMode.PositionAdditiveLocal:
			this.State = this.m_Transform.localPosition;
			if (this.m_AutoUpdate)
			{
				this.m_UpdateFunc = new vp_Spring.UpdateDelegate(this.PositionAdditiveLocal);
			}
			break;
		case vp_Spring.UpdateMode.PositionAdditiveGlobal:
			this.State = this.m_Transform.position;
			if (this.m_AutoUpdate)
			{
				this.m_UpdateFunc = new vp_Spring.UpdateDelegate(this.PositionAdditiveGlobal);
			}
			break;
		case vp_Spring.UpdateMode.PositionAdditiveSelf:
			this.State = this.m_Transform.position;
			if (this.m_AutoUpdate)
			{
				this.m_UpdateFunc = new vp_Spring.UpdateDelegate(this.PositionAdditiveSelf);
			}
			break;
		case vp_Spring.UpdateMode.Rotation:
			this.State = this.m_Transform.localEulerAngles;
			if (this.m_AutoUpdate)
			{
				this.m_UpdateFunc = new vp_Spring.UpdateDelegate(this.Rotation);
			}
			break;
		case vp_Spring.UpdateMode.RotationAdditiveLocal:
			this.State = this.m_Transform.localEulerAngles;
			if (this.m_AutoUpdate)
			{
				this.m_UpdateFunc = new vp_Spring.UpdateDelegate(this.RotationAdditiveLocal);
			}
			break;
		case vp_Spring.UpdateMode.RotationAdditiveGlobal:
			this.State = this.m_Transform.eulerAngles;
			if (this.m_AutoUpdate)
			{
				this.m_UpdateFunc = new vp_Spring.UpdateDelegate(this.RotationAdditiveGlobal);
			}
			break;
		case vp_Spring.UpdateMode.Scale:
			this.State = this.m_Transform.localScale;
			if (this.m_AutoUpdate)
			{
				this.m_UpdateFunc = new vp_Spring.UpdateDelegate(this.Scale);
			}
			break;
		case vp_Spring.UpdateMode.ScaleAdditiveLocal:
			this.State = this.m_Transform.localScale;
			if (this.m_AutoUpdate)
			{
				this.m_UpdateFunc = new vp_Spring.UpdateDelegate(this.ScaleAdditiveLocal);
			}
			break;
		}
		this.RestState = this.State;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void Calculate()
	{
		if ((!this.SdtdStopping) ? (this.State == this.RestState) : (this.m_Velocity.sqrMagnitude <= this.MinVelocity * this.MinVelocity && (this.RestState - this.State).sqrMagnitude <= this.SdtdMinDeltaState * this.SdtdMinDeltaState))
		{
			return;
		}
		this.m_Velocity += Vector3.Scale(this.RestState - this.State, this.Stiffness);
		this.m_Velocity = Vector3.Scale(this.m_Velocity, this.Damping);
		this.m_Velocity = Vector3.ClampMagnitude(this.m_Velocity, this.MaxVelocity);
		if (this.m_Velocity.sqrMagnitude > this.MinVelocity * this.MinVelocity || (this.SdtdStopping && (this.RestState - this.State).sqrMagnitude > this.SdtdMinDeltaState * this.SdtdMinDeltaState))
		{
			this.Move();
			return;
		}
		this.Reset();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void AddForceInternal(Vector3 force)
	{
		force *= this.m_VelocityFadeInCap;
		this.m_Velocity += force;
		this.m_Velocity = Vector3.ClampMagnitude(this.m_Velocity, this.MaxVelocity);
		this.Move();
	}

	public void AddForce(Vector3 force)
	{
		if (Time.timeScale < 1f)
		{
			this.AddSoftForce(force, 1f);
			return;
		}
		this.AddForceInternal(force);
	}

	public void AddSoftForce(Vector3 force, float frames)
	{
		force /= Time.timeScale;
		frames = Mathf.Clamp(frames, 1f, 120f);
		this.AddForceInternal(force / frames);
		for (int i = 0; i < Mathf.RoundToInt(frames) - 1; i++)
		{
			this.m_SoftForceFrame[i] += force / frames;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void Move()
	{
		this.State += this.m_Velocity * Time.timeScale;
		this.State.x = Mathf.Clamp(this.State.x, this.MinState.x, this.MaxState.x);
		this.State.y = Mathf.Clamp(this.State.y, this.MinState.y, this.MaxState.y);
		this.State.z = Mathf.Clamp(this.State.z, this.MinState.z, this.MaxState.z);
	}

	public void Reset()
	{
		this.m_Velocity = Vector3.zero;
		this.State = this.RestState;
	}

	public void Stop(bool includeSoftForce = false)
	{
		this.m_Velocity = Vector3.zero;
		if (includeSoftForce)
		{
			this.StopSoftForce();
		}
	}

	public void StopSoftForce()
	{
		for (int i = 0; i < 120; i++)
		{
			this.m_SoftForceFrame[i] = Vector3.zero;
		}
	}

	public void ForceVelocityFadeIn(float seconds)
	{
		this.m_VelocityFadeInLength = seconds;
		this.m_VelocityFadeInEndTime = Time.time + seconds;
		this.m_VelocityFadeInCap = 0f;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public vp_Spring.UpdateMode Mode;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool m_AutoUpdate = true;

	[PublicizedFrom(EAccessModifier.Protected)]
	public vp_Spring.UpdateDelegate m_UpdateFunc;

	public Vector3 State = Vector3.zero;

	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector3 m_Velocity = Vector3.zero;

	public Vector3 RestState = Vector3.zero;

	public Vector3 Stiffness = new Vector3(0.5f, 0.5f, 0.5f);

	public Vector3 Damping = new Vector3(0.75f, 0.75f, 0.75f);

	[PublicizedFrom(EAccessModifier.Protected)]
	public float m_VelocityFadeInCap = 1f;

	[PublicizedFrom(EAccessModifier.Protected)]
	public float m_VelocityFadeInEndTime;

	[PublicizedFrom(EAccessModifier.Protected)]
	public float m_VelocityFadeInLength;

	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector3[] m_SoftForceFrame = new Vector3[120];

	public float MaxVelocity = 10000f;

	public float MinVelocity = 1E-07f;

	public Vector3 MaxState = new Vector3(10000f, 10000f, 10000f);

	public Vector3 MinState = new Vector3(-10000f, -10000f, -10000f);

	public float SdtdMinDeltaState = 0.0001f;

	[PublicizedFrom(EAccessModifier.Protected)]
	public Transform m_Transform;

	public enum UpdateMode
	{
		Position,
		PositionAdditiveLocal,
		PositionAdditiveGlobal,
		PositionAdditiveSelf,
		Rotation,
		RotationAdditiveLocal,
		RotationAdditiveGlobal,
		Scale,
		ScaleAdditiveLocal
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public delegate void UpdateDelegate();
}
