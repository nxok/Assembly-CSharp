using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[RequireComponent(typeof(Camera))]
[RequireComponent(typeof(AudioListener))]
[Preserve]
public class vp_FPCamera : vp_Component
{
	public bool DrawCameraCollisionDebugLine
	{
		get
		{
			return this.m_DrawCameraCollisionDebugLine;
		}
		set
		{
			this.m_DrawCameraCollisionDebugLine = value;
		}
	}

	public Vector3 CollisionVector
	{
		get
		{
			return this.m_CollisionVector;
		}
	}

	public bool HasOverheadSpace
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public vp_FPPlayerEventHandler Player
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (this.m_Player == null && base.EventHandler != null)
			{
				this.m_Player = (vp_FPPlayerEventHandler)base.EventHandler;
			}
			return this.m_Player;
		}
	}

	public Rigidbody FirstRigidBody
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (this.m_FirstRigidbody == null)
			{
				this.m_FirstRigidbody = base.Transform.root.GetComponentInChildren<Rigidbody>();
			}
			return this.m_FirstRigidbody;
		}
	}

	public Vector2 Angle
	{
		get
		{
			return new Vector2(this.m_Pitch, this.m_Yaw);
		}
		set
		{
			this.Pitch = value.x;
			this.Yaw = value.y;
		}
	}

	public Vector3 Forward
	{
		get
		{
			return this.m_Transform.forward;
		}
	}

	public float Pitch
	{
		get
		{
			return this.m_Pitch;
		}
		set
		{
			if (value > 90f)
			{
				value -= 360f;
			}
			this.m_Pitch = value;
		}
	}

	public float Yaw
	{
		get
		{
			return this.m_Yaw;
		}
		set
		{
			this.m_Yaw = value;
		}
	}

	public virtual bool OnValue_CameraRelativeMovement3P
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return this._cameraRelative3PMovement;
		}
		[PublicizedFrom(EAccessModifier.Protected)]
		set
		{
			this._cameraRelative3PMovement = value;
		}
	}

	public float CurrentCameraDistance
	{
		get
		{
			return Mathf.Min(this.current3rdPersonOffset.z, this.CameraCollisionDistance);
		}
	}

	public float CameraCollisionDistance
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public virtual bool OnValue_IsFirstPerson
	{
		get
		{
			return this.m_IsFirstPerson;
		}
		set
		{
			if (this.m_IsFirstPerson != value)
			{
				this.m_IsFirstPerson = value;
				if (this.m_IsFirstPerson)
				{
					this.m_Yaw = this.yaw3P - 180f;
					Quaternion quaternion = Quaternion.AngleAxis(this.m_Yaw, Vector3.up);
					Quaternion quaternion2 = Quaternion.AngleAxis(0f, Vector3.left);
					base.Parent.rotation = vp_MathUtility.NaNSafeQuaternion(quaternion * quaternion2, base.Parent.rotation);
					this.SnapSprings();
				}
				if (!this.m_IsFirstPerson)
				{
					this.current3rdPersonOffset = Vector3.Lerp(new Vector3(0f, this.Position3rdPersonOffset.y, 0f), this.Position3rdPersonOffset, 0.5f);
					this.pitch3P = -base.transform.forward.y * 57.29578f;
					if (this.Player.Driving.Active)
					{
						this.yaw3P = Mathf.Atan2(-base.transform.forward.x, -base.transform.forward.z) * 57.29578f;
					}
					else
					{
						this.yaw3P = Mathf.Atan2(-base.Parent.transform.forward.x, -base.Parent.transform.forward.z) * 57.29578f;
						this.targetRotation3P = base.transform.eulerAngles.y;
						this.Player.Rotation.Set(new Vector2(this.Pitch, this.targetRotation3P));
					}
					this.SnapSprings();
					this.shake3P = Vector2.zero;
				}
			}
		}
	}

	public virtual Transform OnValue_CameraTransform
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return base.transform;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Awake()
	{
		base.Awake();
		this.FPController = base.Transform.parent.GetComponent<vp_FPController>();
		this.SetRotation(new Vector2(base.Transform.eulerAngles.x, base.Transform.eulerAngles.y));
		this.thisCamera = base.GetComponent<Camera>();
		this.thisCamera.fieldOfView = (float)Constants.cDefaultCameraFieldOfView;
		this.m_PositionSpring = new vp_Spring(base.Transform, vp_Spring.UpdateMode.Position, false);
		this.m_PositionSpring.MinVelocity = 1E-05f;
		this.m_PositionSpring.RestState = this.PositionOffset + this.AimingPositionOffset;
		this.m_PositionSpring2 = new vp_Spring(base.Transform, vp_Spring.UpdateMode.PositionAdditiveLocal, false);
		this.m_PositionSpring2.MinVelocity = 1E-05f;
		this.m_RotationSpring = new vp_Spring(base.Transform, vp_Spring.UpdateMode.RotationAdditiveLocal, false);
		this.m_RotationSpring.MinVelocity = 1E-05f;
		this.m_RotationSpring.SdtdStopping = true;
		this.HasOverheadSpace = true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnEnable()
	{
		base.OnEnable();
		vp_TargetEvent<float>.Register(base.Parent, "CameraBombShake", new Action<float>(this.OnMessage_CameraBombShake));
		vp_TargetEvent<float>.Register(base.Parent, "CameraGroundStomp", new Action<float>(this.OnMessage_CameraGroundStomp));
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnDisable()
	{
		base.OnDisable();
		vp_TargetEvent<float>.Unregister(base.Parent, "CameraBombShake", new Action<float>(this.OnMessage_CameraBombShake));
		vp_TargetEvent<float>.Unregister(base.Parent, "CameraGroundStomp", new Action<float>(this.OnMessage_CameraGroundStomp));
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Start()
	{
		base.Start();
		this.Refresh();
		this.SnapSprings();
		this.SnapZoom();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Init()
	{
		base.Init();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Update()
	{
		base.Update();
		if (Time.timeScale == 0f)
		{
			return;
		}
		this.UpdateInput();
		if (!this.Player.IsFirstPerson.Get())
		{
			if (!this.Player.Driving.Active)
			{
				new Vector2(this.m_Pitch, base.Parent.transform.eulerAngles.y);
				float num = this.targetRotation3P;
				if (this.Player.CameraRelativeMovement3P.Get() || Vector3.Dot(base.Parent.transform.forward, new Vector3(base.transform.forward.x, 0f, base.transform.forward.z)) < 0.95f)
				{
					num = Mathf.LerpAngle(base.Parent.transform.eulerAngles.y, this.targetRotation3P, Time.deltaTime * 16f);
				}
				Vector2 vector;
				vector..ctor(this.m_Pitch, num);
				this.Player.Rotation.Set(vector);
				base.Parent.rotation = Quaternion.Euler(new Vector2(0f, vector.y));
			}
			this.current3rdPersonOffset = Vector3.Lerp(this.current3rdPersonOffset, this.Position3rdPersonOffset, Time.deltaTime * 10f);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void FixedUpdate()
	{
		if (!this.Player.IsFirstPerson.Get())
		{
			if (!this.hasLateUpdateRan)
			{
				return;
			}
			this.hasLateUpdateRan = false;
			if (Time.timeScale > 0f && !this.Locked3rdPerson)
			{
				this.UpdateShakes();
				this.UpdateSprings();
			}
			return;
		}
		else
		{
			if (!this.hasLateUpdateRan)
			{
				return;
			}
			this.hasLateUpdateRan = false;
			base.FixedUpdate();
			if (Time.timeScale == 0f)
			{
				return;
			}
			if (this.Locked3rdPerson)
			{
				return;
			}
			this.UpdateZoom();
			this.UpdateSwaying();
			this.UpdateBob();
			this.UpdateEarthQuake();
			this.UpdateShakes();
			this.UpdateSprings();
			return;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void LateUpdate()
	{
		if (!this.Player.IsFirstPerson.Get())
		{
			this.hasLateUpdateRan = true;
			this.Update3rdPerson();
			return;
		}
		this.hasLateUpdateRan = true;
		base.LateUpdate();
		bool active = this.Player.Driving.Active;
		if (this.Locked3rdPerson)
		{
			if (!active)
			{
				Quaternion quaternion = Quaternion.AngleAxis(this.m_Yaw, Vector3.up);
				Quaternion quaternion2 = Quaternion.AngleAxis(0f, Vector3.left);
				base.Parent.rotation = vp_MathUtility.NaNSafeQuaternion(quaternion * quaternion2, base.Parent.rotation);
			}
			return;
		}
		if (this.FPController.enabled)
		{
			if (this.Player.Driving.Active)
			{
				this.m_Transform.position = this.DrivingPosition;
			}
			else if (this.Player.IsFirstPerson.Get())
			{
				this.m_Transform.position = this.FPController.SmoothPosition;
			}
			else
			{
				this.m_Transform.position = this.FPController.transform.position;
			}
			if (this.Player.IsFirstPerson.Get())
			{
				this.m_Transform.localPosition += this.m_PositionSpring.State + this.m_PositionSpring2.State;
			}
			else if (!this.Player.Driving.Active)
			{
				this.m_Transform.localPosition += this.m_PositionSpring.State + Vector3.Scale(this.m_PositionSpring2.State, Vector3.up);
			}
			if (this.HasCollision)
			{
				this.DoCameraCollision();
			}
		}
		Quaternion quaternion3 = Quaternion.AngleAxis(this.m_Yaw, Vector3.up);
		if (this.Player.Driving.Active)
		{
			Quaternion quaternion4 = Quaternion.AngleAxis(-this.m_Pitch, Vector3.left);
			base.Transform.rotation = vp_MathUtility.NaNSafeQuaternion(quaternion3 * quaternion4, base.Transform.rotation);
		}
		else
		{
			Quaternion quaternion5 = Quaternion.AngleAxis(0f, Vector3.left);
			base.Parent.rotation = vp_MathUtility.NaNSafeQuaternion(quaternion3 * quaternion5, base.Parent.rotation);
			quaternion5 = Quaternion.AngleAxis(-this.m_Pitch, Vector3.left);
			base.Transform.rotation = vp_MathUtility.NaNSafeQuaternion(quaternion3 * quaternion5, base.Transform.rotation);
		}
		base.Transform.localEulerAngles += vp_MathUtility.NaNSafeVector3(Vector3.right * this.m_RotationSpring.State.x, default(Vector3));
		base.Transform.localEulerAngles += vp_MathUtility.NaNSafeVector3(Vector3.forward * this.m_RotationSpring.State.z, default(Vector3));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update3rdPerson()
	{
		if (this.Position3rdPersonOffset == Vector3.zero)
		{
			return;
		}
		if (this.PositionOnDeath != Vector3.zero)
		{
			base.Transform.position = this.PositionOnDeath;
			if (this.FirstRigidBody != null)
			{
				base.Transform.LookAt(this.FirstRigidBody.transform.position + Vector3.up);
				return;
			}
			base.Transform.LookAt(base.Root.position + Vector3.up);
			return;
		}
		else
		{
			bool active = this.Player.Driving.Active;
			bool flag = this.Player.CameraRelativeMovement3P.Get();
			this.pitch3P = Mathf.Clamp(this.pitch3P + this.Player.InputSmoothLook.Get().y, -89.5f, 89.5f);
			this.m_Pitch = this.pitch3P;
			this.yaw3P += this.Player.InputSmoothLook.Get().x;
			if (this.Locked3rdPerson)
			{
				this.targetRotation3P += this.Player.InputSmoothLook.Get().x;
				this.yaw3P = this.targetRotation3P;
				return;
			}
			if (flag)
			{
				Vector2 vector = this.Player.InputMoveVector.Get();
				if (vector != Vector2.zero)
				{
					this.targetRotation3P = Mathf.Atan2(vector.x, vector.y) * 57.29578f;
				}
			}
			else
			{
				this.targetRotation3P = base.transform.eulerAngles.y;
			}
			this.m_Final3rdPersonCameraOffset = base.Transform.position;
			this.pitch3P += this.shake3P.y;
			this.yaw3P += this.shake3P.x;
			this.shake3P = Vector2.zero;
			Vector3 normalized = (Quaternion.Euler(-this.pitch3P, this.yaw3P, 0f) * Vector3.forward).normalized;
			if (active)
			{
				this.FPController.localPlayer.RefreshDrivingCameraPositions();
			}
			Vector3 vector2 = (active ? this.DrivingPosition : this.FPController.SmoothPosition);
			Vector3 vector3 = vector2 + normalized * this.current3rdPersonOffset.z;
			vector3 += Vector3.up * this.current3rdPersonOffset.y;
			base.Transform.position = vector3;
			this.m_Final3rdPersonCameraOffset -= base.Transform.position;
			Vector3 vector4 = vector2 + Vector3.up * this.current3rdPersonOffset.y;
			base.Transform.LookAt(vector4);
			base.Transform.position += base.Transform.TransformDirection(new Vector3(this.current3rdPersonOffset.x, 0f, 0f));
			float num = this.Pitch / 90f;
			float num2 = this.current3rdPersonOffset.z * num * 0.6f;
			num2 = Utils.FastMin(num2, 0.7f);
			base.Transform.position += new Vector3(base.Transform.forward.x, 0f, base.Transform.forward.z).normalized * num2;
			this.DoCameraCollision();
			this.LookPoint = this.GetLookPoint();
			return;
		}
	}

	public void AlignCharacterToCamera()
	{
		this.targetRotation3P = base.Transform.eulerAngles.y;
	}

	public virtual void DoCameraCollision()
	{
		this.HasOverheadSpace = true;
		this.CameraCollisionDistance = float.MaxValue;
		this.m_CameraCollisionStartPos = this.FPController.Transform.TransformPoint(0f, this.PositionOffset.y + this.AimingPositionOffset.y, 0f) - (this.m_Player.IsFirstPerson.Get() ? Vector3.zero : (this.FPController.Transform.position - this.FPController.SmoothPosition));
		if (this.m_Player.IsFirstPerson.Get())
		{
			Vector3 vector = this.m_CameraCollisionStartPos - Vector3.up * (this.FPController.CharacterController.height * 0.5f + 0.05f);
			this.m_CameraCollisionEndPos = vector + Vector3.up * this.FPController.CharacterController.radius * 2.1f;
			if (Physics.SphereCast(vector, this.FPController.CharacterController.radius, Vector3.up, ref this.m_CameraHit, this.FPController.CharacterController.radius * 2.1f, 1082195968) && !this.m_CameraHit.collider.isTrigger)
			{
				this.m_CollisionVector = this.m_CameraCollisionEndPos - this.m_CameraHit.point;
				base.Transform.position = vector + Vector3.up * this.FPController.CharacterController.radius + Vector3.down * this.m_CollisionVector.y;
				this.CameraCollisionDistance = this.m_CameraHit.distance;
				this.HasOverheadSpace = false;
			}
			return;
		}
		this.m_CameraCollisionEndPos = base.Transform.position + (base.Transform.position - this.m_CameraCollisionStartPos).normalized * this.FPController.CharacterController.radius;
		this.m_CollisionVector = Vector3.zero;
		if (Physics.Linecast(this.m_CameraCollisionStartPos, this.m_CameraCollisionEndPos, ref this.m_CameraHit, 1082195968) && !this.m_CameraHit.collider.isTrigger)
		{
			base.Transform.position = this.m_CameraHit.point - (this.m_CameraHit.point - this.m_CameraCollisionStartPos).normalized * this.FPController.CharacterController.radius;
			this.m_CollisionVector = this.m_CameraHit.point - this.m_CameraCollisionEndPos;
			this.CameraCollisionDistance = this.m_CameraHit.distance;
			return;
		}
		Camera playerCamera = GameManager.Instance.World.GetPrimaryPlayer().playerCamera;
		Vector3[] array = new Vector3[4];
		playerCamera.CalculateFrustumCorners(new Rect(0f, 0f, 1f, 1f), playerCamera.nearClipPlane, 2, array);
		this.cameraCollisionEndPosList.Clear();
		for (int i = 0; i < array.Length; i++)
		{
			Vector3 vector2 = array[i] * 1.5f;
			this.m_CameraCollisionEndPos = base.Transform.position + base.Transform.TransformDirection(vector2);
			this.m_CollisionVector = Vector3.zero;
			this.cameraCollisionEndPosList.Add(this.m_CameraCollisionEndPos);
			if (Physics.Linecast(this.m_CameraCollisionStartPos, this.m_CameraCollisionEndPos, ref this.m_CameraHit, 1082195968) && !this.m_CameraHit.collider.isTrigger)
			{
				base.Transform.position = this.m_CameraHit.point - (this.m_CameraHit.point - this.m_CameraCollisionStartPos).normalized * this.FPController.CharacterController.radius;
				this.m_CollisionVector = this.m_CameraHit.point - this.m_CameraCollisionEndPos;
				this.CameraCollisionDistance = this.m_CameraHit.distance;
				return;
			}
		}
	}

	public virtual void AddForce(Vector3 force)
	{
		this.m_PositionSpring.AddForce(force);
	}

	public virtual void AddForce(float x, float y, float z)
	{
		this.AddForce(new Vector3(x, y, z));
	}

	public virtual void AddForce2(Vector3 force)
	{
		this.m_PositionSpring2.AddForce(force);
	}

	public void AddForce2(float x, float y, float z)
	{
		this.AddForce2(new Vector3(x, y, z));
	}

	public virtual void AddRollForce(float force)
	{
		this.m_RotationSpring.AddForce(Vector3.forward * force);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void UpdateInput()
	{
		if (this.Player.Dead.Active)
		{
			return;
		}
		if (this.Player.InputSmoothLook.Get() == Vector2.zero)
		{
			return;
		}
		this.m_Yaw += this.Player.InputSmoothLook.Get().x;
		this.m_Pitch += this.Player.InputSmoothLook.Get().y;
		this.m_Yaw = ((this.m_Yaw < -360f) ? (this.m_Yaw += 360f) : this.m_Yaw);
		this.m_Yaw = ((this.m_Yaw > 360f) ? (this.m_Yaw -= 360f) : this.m_Yaw);
		this.m_Yaw = Mathf.Clamp(this.m_Yaw, this.RotationYawLimit.x, this.RotationYawLimit.y);
		this.m_Pitch = ((this.m_Pitch < -360f) ? (this.m_Pitch += 360f) : this.m_Pitch);
		this.m_Pitch = ((this.m_Pitch > 360f) ? (this.m_Pitch -= 360f) : this.m_Pitch);
		this.m_Pitch = Mathf.Clamp(this.m_Pitch, -this.RotationPitchLimit.x, -this.RotationPitchLimit.y);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void UpdateZoom()
	{
		if (this.m_FinalZoomTime <= Time.time)
		{
			return;
		}
		this.RenderingZoomDamping = Mathf.Max(this.RenderingZoomDamping, 0.01f);
		float num = 1f - (this.m_FinalZoomTime - Time.time) / this.RenderingZoomDamping;
		base.gameObject.GetComponent<Camera>().fieldOfView = Mathf.SmoothStep(base.gameObject.GetComponent<Camera>().fieldOfView, (float)Constants.cDefaultCameraFieldOfView + this.ZoomOffset, num);
	}

	public void RefreshZoom()
	{
		float num = 1f - (this.m_FinalZoomTime - Time.time) / this.RenderingZoomDamping;
		base.gameObject.GetComponent<Camera>().fieldOfView = Mathf.SmoothStep(base.gameObject.GetComponent<Camera>().fieldOfView, (float)Constants.cDefaultCameraFieldOfView + this.ZoomOffset, num);
	}

	public virtual void Zoom()
	{
		this.m_FinalZoomTime = Time.time + this.RenderingZoomDamping;
	}

	public virtual void SnapZoom()
	{
		base.gameObject.GetComponent<Camera>().fieldOfView = (float)Constants.cDefaultCameraFieldOfView + this.ZoomOffset;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void UpdateShakes()
	{
		if (this.ShakeSpeed != 0f)
		{
			this.m_Yaw -= this.m_Shake.y;
			this.m_Pitch -= this.m_Shake.x;
			this.m_Shake = Vector3.Scale(vp_SmoothRandom.GetVector3Centered(this.ShakeSpeed), this.ShakeAmplitude);
			if (float.IsNaN(this.m_Shake.x) || float.IsNaN(this.m_Shake.y) || float.IsNaN(this.m_Shake.z))
			{
				Log.Warning("Shake NaN {0}, time {1}, speed {2}, amp {3}", new object[]
				{
					this.m_Shake,
					Time.time,
					this.ShakeSpeed,
					this.ShakeAmplitude
				});
				this.ShakeSpeed = 0f;
				this.m_Shake = Vector3.zero;
				this.m_Pitch += -1f;
			}
			this.m_Yaw += this.m_Shake.y;
			this.m_Pitch += this.m_Shake.x;
			this.m_RotationSpring.AddForce(Vector3.forward * this.m_Shake.z * Time.timeScale);
		}
		if (this.ShakeSpeed2 != 0f)
		{
			this.m_Yaw -= this.m_Shake2.y;
			this.m_Pitch -= this.m_Shake2.x;
			this.shake3P.x = this.shake3P.x - this.m_Shake2.x;
			this.shake3P.y = this.shake3P.y - this.m_Shake2.y;
			this.m_Shake2 = Vector3.Scale(vp_SmoothRandom.GetVector3Centered(this.ShakeSpeed2), this.ShakeAmplitude2);
			if (float.IsNaN(this.m_Shake2.x) || float.IsNaN(this.m_Shake2.y) || float.IsNaN(this.m_Shake2.z))
			{
				Log.Warning("Shake2 NaN {0}, time {1}, speed {2}, amp {3}", new object[]
				{
					this.m_Shake2,
					Time.time,
					this.ShakeSpeed2,
					this.ShakeAmplitude2
				});
				this.ShakeSpeed2 = 0f;
				this.m_Shake2 = Vector3.zero;
				this.m_Pitch += -1f;
			}
			this.m_Yaw += this.m_Shake2.y;
			this.m_Pitch += this.m_Shake2.x;
			this.m_RotationSpring.AddForce(Vector3.forward * this.m_Shake2.z * Time.timeScale);
			this.shake3P.x = this.shake3P.x + this.m_Shake2.x;
			this.shake3P.y = this.shake3P.y + this.m_Shake2.y;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void UpdateBob()
	{
		if (this.BobAmplitude == Vector4.zero || this.BobRate == Vector4.zero)
		{
			return;
		}
		if (!this.Player.IsFirstPerson.Get())
		{
			return;
		}
		this.m_BobSpeed = ((this.BobRequireGroundContact && !this.FPController.Grounded) ? 0f : this.FPController.CharacterController.velocity.sqrMagnitude);
		this.m_BobSpeed = Mathf.Min(this.m_BobSpeed * this.BobInputVelocityScale, this.BobMaxInputVelocity);
		this.m_BobSpeed = Mathf.Round(this.m_BobSpeed * 1000f) / 1000f;
		if (this.m_BobSpeed == 0f)
		{
			this.m_BobSpeed = Mathf.Min(this.m_LastBobSpeed * 0.93f, this.BobMaxInputVelocity);
		}
		this.m_CurrentBobAmp.y = this.m_BobSpeed * (this.BobAmplitude.y * -0.0001f);
		this.m_CurrentBobVal.y = Mathf.Cos(Time.time * (this.BobRate.y * 10f)) * this.m_CurrentBobAmp.y;
		this.m_CurrentBobAmp.x = this.m_BobSpeed * (this.BobAmplitude.x * 0.0001f);
		this.m_CurrentBobVal.x = Mathf.Cos(Time.time * (this.BobRate.x * 10f)) * this.m_CurrentBobAmp.x;
		this.m_CurrentBobAmp.z = this.m_BobSpeed * (this.BobAmplitude.z * 0.0001f);
		this.m_CurrentBobVal.z = Mathf.Cos(Time.time * (this.BobRate.z * 10f)) * this.m_CurrentBobAmp.z;
		this.m_CurrentBobAmp.w = this.m_BobSpeed * (this.BobAmplitude.w * 0.0001f);
		this.m_CurrentBobVal.w = Mathf.Cos(Time.time * (this.BobRate.w * 10f)) * this.m_CurrentBobAmp.w;
		this.m_PositionSpring.AddForce(this.m_CurrentBobVal * Time.timeScale);
		this.AddRollForce(this.m_CurrentBobVal.w * Time.timeScale);
		this.m_LastBobSpeed = this.m_BobSpeed;
		this.DetectBobStep(this.m_BobSpeed, this.m_CurrentBobVal.y);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void DetectBobStep(float speed, float upBob)
	{
		if (this.BobStepCallback == null)
		{
			return;
		}
		if (speed < this.BobStepThreshold)
		{
			return;
		}
		bool flag = this.m_LastUpBob < upBob;
		this.m_LastUpBob = upBob;
		if (flag && !this.m_BobWasElevating)
		{
			this.BobStepCallback();
		}
		this.m_BobWasElevating = flag;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void UpdateSwaying()
	{
		Vector3 vector = base.Transform.InverseTransformDirection(this.FPController.CharacterController.velocity * 0.016f) * Time.timeScale;
		this.AddRollForce(vector.x * this.RotationStrafeRoll);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void UpdateEarthQuake()
	{
		if (this.Player == null)
		{
			return;
		}
		if (!this.Player.CameraEarthQuake.Active)
		{
			return;
		}
		if (this.m_PositionSpring.State.y >= this.m_PositionSpring.RestState.y)
		{
			Vector3 vector = this.Player.CameraEarthQuakeForce.Get();
			vector.y = -vector.y;
			this.Player.CameraEarthQuakeForce.Set(vector);
		}
		this.m_PositionSpring.AddForce(this.Player.CameraEarthQuakeForce.Get() * this.PositionEarthQuakeFactor);
		this.m_RotationSpring.AddForce(Vector3.forward * (-this.Player.CameraEarthQuakeForce.Get().x * 2f) * this.RotationEarthQuakeFactor);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void UpdateSprings()
	{
		Vector3 position = base.Transform.position;
		this.m_PositionSpring.FixedUpdate();
		this.m_PositionSpring2.FixedUpdate();
		this.m_RotationSpring.FixedUpdate();
		base.Transform.position = position;
	}

	public virtual void DoBomb(Vector3 positionForce, float minRollForce, float maxRollForce)
	{
		this.AddForce2(positionForce);
		float num = Random.Range(minRollForce, maxRollForce);
		if (Random.value > 0.5f)
		{
			num = -num;
		}
		this.AddRollForce(num);
	}

	public override void Refresh()
	{
		if (!Application.isPlaying)
		{
			return;
		}
		if (this.m_PositionSpring != null)
		{
			this.m_PositionSpring.Stiffness = new Vector3(this.PositionSpringStiffness, this.PositionSpringStiffness, this.PositionSpringStiffness);
			this.m_PositionSpring.Damping = Vector3.one - new Vector3(this.PositionSpringDamping, this.PositionSpringDamping, this.PositionSpringDamping);
			this.m_PositionSpring.MinState.y = this.PositionGroundLimit;
			this.m_PositionSpring.RestState = this.PositionOffset + this.AimingPositionOffset;
		}
		if (this.m_PositionSpring2 != null)
		{
			this.m_PositionSpring2.Stiffness = new Vector3(this.PositionSpring2Stiffness, this.PositionSpring2Stiffness, this.PositionSpring2Stiffness);
			this.m_PositionSpring2.Damping = Vector3.one - new Vector3(this.PositionSpring2Damping, this.PositionSpring2Damping, this.PositionSpring2Damping);
			this.m_PositionSpring2.MinState.y = -this.PositionOffset.y - this.AimingPositionOffset.y + this.PositionGroundLimit;
		}
		if (this.m_RotationSpring != null)
		{
			this.m_RotationSpring.Stiffness = new Vector3(this.RotationSpringStiffness, this.RotationSpringStiffness, this.RotationSpringStiffness);
			this.m_RotationSpring.Damping = Vector3.one - new Vector3(this.RotationSpringDamping, this.RotationSpringDamping, this.RotationSpringDamping);
		}
	}

	public virtual void SnapSprings()
	{
		if (this.m_PositionSpring != null)
		{
			this.m_PositionSpring.RestState = this.PositionOffset + this.AimingPositionOffset;
			this.m_PositionSpring.State = this.PositionOffset + this.AimingPositionOffset;
			this.m_PositionSpring.Stop(true);
		}
		if (this.m_PositionSpring2 != null)
		{
			this.m_PositionSpring2.RestState = Vector3.zero;
			this.m_PositionSpring2.State = Vector3.zero;
			this.m_PositionSpring2.Stop(true);
		}
		if (this.m_RotationSpring != null)
		{
			this.m_RotationSpring.RestState = Vector3.zero;
			this.m_RotationSpring.State = Vector3.zero;
			this.m_RotationSpring.Stop(true);
		}
	}

	public virtual void StopSprings()
	{
		if (this.m_PositionSpring != null)
		{
			this.m_PositionSpring.Stop(true);
		}
		if (this.m_PositionSpring2 != null)
		{
			this.m_PositionSpring2.Stop(true);
		}
		if (this.m_RotationSpring != null)
		{
			this.m_RotationSpring.Stop(true);
		}
		this.m_BobSpeed = 0f;
		this.m_LastBobSpeed = 0f;
	}

	public virtual void Stop()
	{
		this.SnapSprings();
		this.SnapZoom();
		this.Refresh();
	}

	public virtual void SetRotation(Vector2 eulerAngles, bool stopZoomAndSprings)
	{
		this.Angle = eulerAngles;
		if (stopZoomAndSprings)
		{
			this.Stop();
		}
	}

	public virtual void SetRotation(Vector2 eulerAngles)
	{
		this.Angle = eulerAngles;
		this.Stop();
	}

	public virtual void SetRotation(Vector2 eulerAngles, bool stopZoomAndSprings, bool obsolete)
	{
		this.SetRotation(eulerAngles, stopZoomAndSprings);
	}

	public Vector3 GetLookPoint()
	{
		if (!this.Player.IsFirstPerson.Get() && Physics.Linecast(base.Transform.position, base.Transform.position + base.Transform.forward * 1000f, ref this.m_LookPointHit, 1082195968) && !this.m_LookPointHit.collider.isTrigger && base.Root.InverseTransformPoint(this.m_LookPointHit.point).z > 0f)
		{
			return this.m_LookPointHit.point;
		}
		return base.Transform.position + base.Transform.forward * 1000f;
	}

	public virtual Vector3 OnValue_LookPoint
	{
		get
		{
			return this.LookPoint;
		}
	}

	public virtual Vector3 OnValue_CameraLookDirection
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return (this.Player.LookPoint.Get() - base.Transform.position).normalized;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnMessage_FallImpact(float impact)
	{
		impact = Mathf.Abs(impact * 55f);
		float num = impact * this.PositionKneeling;
		float num2 = impact * this.RotationKneeling;
		num = Mathf.SmoothStep(0f, 1f, num);
		num2 = Mathf.SmoothStep(0f, 1f, num2);
		num2 = Mathf.SmoothStep(0f, 1f, num2);
		if (this.m_PositionSpring != null)
		{
			this.m_PositionSpring.AddSoftForce(Vector3.down * num, (float)this.PositionKneelingSoftness);
		}
		if (this.m_RotationSpring != null)
		{
			this.m_RotationSpring.AddSoftForce(Vector3.right * (num2 * 2f), (float)this.RotationKneelingSoftness);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnMessage_HeadImpact(float impact)
	{
		if (this.m_RotationSpring != null && Mathf.Abs(this.m_RotationSpring.State.z) < 30f)
		{
			this.m_RotationSpring.AddForce(Vector3.forward * (impact * 20f) * Time.timeScale);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnMessage_CameraGroundStomp(float impact)
	{
		this.AddForce2(new Vector3(0f, -1f, 0f) * impact);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnMessage_CameraBombShake(float impact)
	{
		this.DoBomb(new Vector3(1f, -10f, 1f) * impact, 1f, 2f);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnStart_Zoom()
	{
		if (this.Player == null)
		{
			return;
		}
		this.Player.Run.Stop(0f);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual bool CanStart_Run()
	{
		return this.Player == null || !this.Player.Zoom.Active;
	}

	public virtual Vector2 OnValue_Rotation
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return this.Angle;
		}
		[PublicizedFrom(EAccessModifier.Protected)]
		set
		{
			this.Angle = value;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnMessage_Stop()
	{
		this.Stop();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnStart_Dead()
	{
		if (this.Player.IsFirstPerson.Get())
		{
			return;
		}
		this.PositionOnDeath = base.Transform.position - this.m_Final3rdPersonCameraOffset;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnStop_Dead()
	{
		this.PositionOnDeath = Vector3.zero;
		this.m_Current3rdPersonBlend = 0f;
	}

	public virtual bool OnValue_IsLocal
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return true;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnMessage_CameraToggle3rdPerson()
	{
		this.m_Player.IsFirstPerson.Set(!this.m_Player.IsFirstPerson.Get());
	}

	public Vector3 DrivingPosition;

	public vp_FPController FPController;

	public float RenderingFieldOfView = 60f;

	public float RenderingZoomDamping = 0.2f;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float m_FinalZoomTime;

	public float ZoomOffset;

	public Vector3 PositionOffset = new Vector3(0f, 1.75f, 0.1f);

	public Vector3 AimingPositionOffset = Vector3.zero;

	public float PositionGroundLimit = 0.1f;

	public float PositionSpringStiffness = 0.01f;

	public float PositionSpringDamping = 0.25f;

	public float PositionSpring2Stiffness = 0.95f;

	public float PositionSpring2Damping = 0.25f;

	public float PositionKneeling = 0.025f;

	public int PositionKneelingSoftness = 1;

	public float PositionEarthQuakeFactor = 1f;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public vp_Spring m_PositionSpring;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public vp_Spring m_PositionSpring2;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public bool m_DrawCameraCollisionDebugLine;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Vector3 PositionOnDeath = Vector3.zero;

	public Vector2 RotationPitchLimit = new Vector2(90f, -90f);

	public Vector2 RotationYawLimit = new Vector2(-360f, 360f);

	public float RotationSpringStiffness = 0.01f;

	public float RotationSpringDamping = 0.25f;

	public float RotationKneeling = 0.025f;

	public int RotationKneelingSoftness = 1;

	public float RotationStrafeRoll = 0.01f;

	public float RotationEarthQuakeFactor;

	public Vector3 LookPoint = Vector3.zero;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float m_Pitch;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float m_Yaw;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public vp_Spring m_RotationSpring;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public RaycastHit m_LookPointHit;

	public Vector3 Position3rdPersonOffset = new Vector3(0.5f, 0.1f, 0.75f);

	public bool Locked3rdPerson;

	public float m_Current3rdPersonBlend;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Vector3 m_Final3rdPersonCameraOffset = Vector3.zero;

	public float ShakeSpeed;

	public Vector3 ShakeAmplitude = new Vector3(10f, 10f, 0f);

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Vector3 m_Shake = Vector3.zero;

	public float ShakeSpeed2;

	public Vector3 ShakeAmplitude2 = new Vector3(10f, 10f, 0f);

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Vector3 m_Shake2 = Vector3.zero;

	public Vector4 BobRate = new Vector4(0f, 1.4f, 0f, 0.7f);

	public Vector4 BobAmplitude = new Vector4(0f, 0.25f, 0f, 0.5f);

	public float BobInputVelocityScale = 1f;

	public float BobMaxInputVelocity = 100f;

	public bool BobRequireGroundContact = true;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float m_LastBobSpeed;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Vector4 m_CurrentBobAmp = Vector4.zero;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Vector4 m_CurrentBobVal = Vector4.zero;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float m_BobSpeed;

	public vp_FPCamera.BobStepDelegate BobStepCallback;

	public float BobStepThreshold = 10f;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float m_LastUpBob;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public bool m_BobWasElevating;

	public bool HasCollision = true;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Vector3 m_CollisionVector = Vector3.zero;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Vector3 m_CameraCollisionStartPos = Vector3.zero;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Vector3 m_CameraCollisionEndPos = Vector3.zero;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public RaycastHit m_CameraHit;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public List<Vector3> cameraCollisionEndPosList = new List<Vector3>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public vp_FPPlayerEventHandler m_Player;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Rigidbody m_FirstRigidbody;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float cPlayerRotationSpeed = 16f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float cCameraOffsetSpeed = 10f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 current3rdPersonOffset = Vector3.zero;

	public float yaw3P;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float pitch3P;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float targetRotation3P;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool _cameraRelative3PMovement;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Vector2 shake3P;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Camera thisCamera;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public bool m_IsFirstPerson = true;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool hasLateUpdateRan;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static Vector3 lastPos;

	public delegate void BobStepDelegate();
}
