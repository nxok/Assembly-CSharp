using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[RequireComponent(typeof(vp_FPPlayerEventHandler))]
[RequireComponent(typeof(CharacterController))]
[Preserve]
public class vp_FPController : vp_Component
{
	public vp_FPPlayerEventHandler Player
	{
		get
		{
			if (this.m_Player == null && base.EventHandler != null)
			{
				this.m_Player = (vp_FPPlayerEventHandler)base.EventHandler;
			}
			return this.m_Player;
		}
	}

	public CharacterController CharacterController
	{
		get
		{
			if (this.m_CharacterController == null)
			{
				this.m_CharacterController = base.gameObject.GetComponent<CharacterController>();
			}
			return this.m_CharacterController;
		}
	}

	public Vector3 SmoothPosition
	{
		get
		{
			return this.m_SmoothPosition;
		}
	}

	public Vector3 Velocity
	{
		get
		{
			return this.m_CharacterController.velocity;
		}
	}

	public float SpeedModifier
	{
		get
		{
			return this.m_SpeedModifier;
		}
		set
		{
			this.m_SpeedModifier = value;
		}
	}

	public void Reposition(Vector3 _deltaVec)
	{
		this.m_SmoothPosition += _deltaVec;
		this.SetPosition(this.m_SmoothPosition);
	}

	public bool Grounded
	{
		get
		{
			return this.m_Grounded;
		}
	}

	public bool HeadContact
	{
		get
		{
			return this.m_HeadContact;
		}
	}

	public Vector3 GroundNormal
	{
		get
		{
			return this.m_GroundHit.normal;
		}
	}

	public float GroundAngle
	{
		get
		{
			return Vector3.Angle(this.m_GroundHit.normal, Vector3.up);
		}
	}

	public Transform GroundTransform
	{
		get
		{
			return this.m_GroundHit.transform;
		}
	}

	public bool IsCollidingWall
	{
		get
		{
			return this.m_WallHit.collider != null;
		}
	}

	public float ProjectedWallMove
	{
		get
		{
			if (!(this.m_WallHit.collider != null))
			{
				return 1f;
			}
			return this.m_ForceMultiplier;
		}
	}

	public Vector3 FixedPosition
	{
		get
		{
			return this.m_FixedPosition;
		}
	}

	public float SkinWidth
	{
		get
		{
			return this.m_SkinWidth;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Awake()
	{
		base.Awake();
		this.SyncCharacterController();
		if (this.originalMotorJumpForce == -1f)
		{
			this.originalMotorJumpForce = this.MotorJumpForce;
		}
	}

	public void SyncCharacterController()
	{
		this.m_NormalHeight = this.CharacterController.height;
		this.CharacterController.center = (this.m_NormalCenter = new Vector3(0f, this.m_NormalHeight * 0.5f, 0f));
		this.CharacterController.radius = this.m_NormalHeight * 0.16666f;
		this.m_CrouchHeight = this.m_NormalHeight * this.PhysicsCrouchHeightModifier;
		this.m_CrouchCenter = this.m_NormalCenter * this.PhysicsCrouchHeightModifier;
		this.m_StepHeight = this.CharacterController.stepOffset;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnEnable()
	{
		base.OnEnable();
		vp_TargetEvent<Vector3>.Register(this.m_Transform, "ForceImpact", new Action<Vector3>(this.AddForce));
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnDisable()
	{
		base.OnDisable();
		vp_TargetEvent<Vector3>.Unregister(this.m_Root, "ForceImpact", new Action<Vector3>(this.AddForce));
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Start()
	{
		base.Start();
		this.SetPosition(base.Transform.position);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Update()
	{
		base.Update();
		this.SmoothMove();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void FixedUpdate()
	{
		if (Time.timeScale == 0f)
		{
			return;
		}
		if (this.Player.Driving.Active)
		{
			this.m_PrevPos = this.m_FixedPosition;
			this.m_FixedPosition = base.Transform.position;
			return;
		}
		base.Transform.position = this.m_FixedPosition;
		this.UpdateMotor();
		this.UpdateJump();
		this.UpdateForces();
		this.UpdateSliding();
		this.UpdateOutOfControl();
		this.FixedMove();
		this.UpdateCollisions();
		this.UpdatePlatformMove();
		this.m_PrevPos = base.Transform.position;
		this.m_FixedPosition = base.Transform.position;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void UpdateMotor()
	{
		if (!this.MotorFreeFly)
		{
			this.UpdateThrottleWalk();
			this.m_MotorThrottle = vp_MathUtility.SnapToZero(this.m_MotorThrottle, 0.0001f);
			return;
		}
		this.localPlayer.SwimModeUpdateThrottle();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void UpdateThrottleWalk()
	{
		this.UpdateSlopeFactor();
		this.m_MotorAirSpeedModifier = (this.m_Grounded ? 1f : this.MotorAirSpeed);
		Vector2 vector = this.Player.InputMoveVector.Get();
		if (vector.magnitude > 1f)
		{
			vector.Normalize();
		}
		float num = 0.1f * this.MotorAcceleration * this.m_MotorAirSpeedModifier * this.m_SlopeFactor;
		if (!this.Player.IsFirstPerson.Get() && this.Player.CameraRelativeMovement3P.Get())
		{
			float num2 = vector.y * num;
			float num3 = vector.x * num;
			Vector3 vector2;
			vector2..ctor(num3, 0f, num2);
			this.m_MotorThrottle += vector2;
		}
		else
		{
			float num4 = ((vector.y > 0f) ? vector.y : (vector.y * this.MotorBackwardsSpeed)) * num;
			float num5 = vector.x * this.MotorSidewaysSpeed * num;
			this.m_MotorThrottle += base.Transform.TransformDirection(Vector3.forward) * num4 + base.Transform.TransformDirection(Vector3.right) * num5;
		}
		this.m_MotorThrottle.x = this.m_MotorThrottle.x / (1f + this.MotorDamping * this.m_MotorAirSpeedModifier * Time.timeScale);
		this.m_MotorThrottle.z = this.m_MotorThrottle.z / (1f + this.MotorDamping * this.m_MotorAirSpeedModifier * Time.timeScale);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void UpdateThrottleFree()
	{
		Vector3 vector = this.Player.CameraLookDirection.Get();
		vector.y *= 2f;
		vector.Normalize();
		this.m_MotorThrottle += this.Player.InputMoveVector.Get().y * vector * (this.MotorAcceleration * 0.1f);
		this.m_MotorThrottle += this.Player.InputMoveVector.Get().x * base.Transform.TransformDirection(Vector3.right * (this.MotorAcceleration * 0.1f));
		this.m_MotorThrottle.x = this.m_MotorThrottle.x / (1f + this.MotorDamping * Time.timeScale);
		this.m_MotorThrottle.z = this.m_MotorThrottle.z / (1f + this.MotorDamping * Time.timeScale);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void UpdateJump()
	{
		if (this.m_HeadContact)
		{
			this.Player.Jump.Stop(1f);
		}
		if (!this.MotorFreeFly)
		{
			this.UpdateJumpForceWalk();
		}
		else
		{
			this.UpdateJumpForceFree();
		}
		this.m_MotorThrottle.y = this.m_MotorThrottle.y + this.m_MotorJumpForceAcc * Time.timeScale;
		this.m_MotorJumpForceAcc /= 1f + this.MotorJumpForceHoldDamping * Time.timeScale;
		this.m_MotorThrottle.y = this.m_MotorThrottle.y / (1f + this.MotorJumpForceDamping * Time.timeScale);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void UpdateJumpForceWalk()
	{
		if (this.Player.Jump.Active && !this.m_Grounded)
		{
			if (this.m_MotorJumpForceHoldSkipFrames > 2)
			{
				if (this.Player.Velocity.Get().y >= 0f)
				{
					this.m_MotorJumpForceAcc += this.MotorJumpForceHold;
					return;
				}
			}
			else
			{
				this.m_MotorJumpForceHoldSkipFrames++;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void UpdateJumpForceFree()
	{
		if (this.Player.Jump.Active && this.Player.Crouch.Active)
		{
			return;
		}
		if (this.Player.Jump.Active)
		{
			this.m_MotorJumpForceAcc += this.MotorJumpForceHold;
			return;
		}
		if (this.Player.Crouch.Active && this.Grounded && this.CharacterController.height == this.m_NormalHeight)
		{
			this.CharacterController.height = this.m_CrouchHeight;
			this.CharacterController.center = this.m_CrouchCenter;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void UpdateForces()
	{
		this.m_GravityForce = Physics.gravity.y * (this.PhysicsGravityModifier * 0.002f) * vp_TimeUtility.AdjustedTimeScale;
		if (this.m_Grounded && this.m_FallSpeed <= 0f)
		{
			this.m_FallSpeed = this.m_GravityForce;
		}
		else
		{
			this.m_FallSpeed += this.m_GravityForce;
		}
		if (this.m_SmoothForceFrame[0] != Vector3.zero)
		{
			this.AddForceInternal(this.m_SmoothForceFrame[0]);
			for (int i = 0; i < 120; i++)
			{
				this.m_SmoothForceFrame[i] = ((i < 119) ? this.m_SmoothForceFrame[i + 1] : Vector3.zero);
				if (this.m_SmoothForceFrame[i] == Vector3.zero)
				{
					break;
				}
			}
		}
		this.m_ExternalForce /= 1f + this.PhysicsForceDamping * vp_TimeUtility.AdjustedTimeScale;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void UpdateSliding()
	{
		bool slideFast = this.m_SlideFast;
		bool slide = this.m_Slide;
		this.m_Slide = false;
		if (!this.m_Grounded)
		{
			this.m_OnSteepGroundSince = 0f;
			this.m_SlideFast = false;
		}
		else if (this.GroundAngle > this.PhysicsSlopeSlideLimit)
		{
			this.m_Slide = true;
			if (this.GroundAngle <= this.Player.SlopeLimit.Get())
			{
				this.m_SlopeSlideSpeed = Mathf.Max(this.m_SlopeSlideSpeed, this.PhysicsSlopeSlidiness * 0.01f);
				this.m_OnSteepGroundSince = 0f;
				this.m_SlideFast = false;
				this.m_SlopeSlideSpeed = ((Mathf.Abs(this.m_SlopeSlideSpeed) < 0.0001f) ? 0f : (this.m_SlopeSlideSpeed / (1f + 0.05f * vp_TimeUtility.AdjustedTimeScale)));
			}
			else
			{
				if (this.m_SlopeSlideSpeed > 0.01f)
				{
					this.m_SlideFast = true;
				}
				if (this.m_OnSteepGroundSince == 0f)
				{
					this.m_OnSteepGroundSince = Time.time;
					this.slideLastGroundN = Vector3.up;
				}
				this.m_SlopeSlideSpeed += this.PhysicsSlopeSlidiness * 0.01f * 5f * Time.deltaTime * vp_TimeUtility.AdjustedTimeScale;
				this.m_SlopeSlideSpeed *= 0.97f;
				float num = Vector3.Dot(this.GroundNormal, this.slideLastGroundN);
				this.slideLastGroundN = this.GroundNormal;
				if (num < 0.2f)
				{
					float num2 = 0.7f;
					if (num < -0.2f)
					{
						num2 = 0.2f;
					}
					this.m_SlopeSlideSpeed *= num2;
					this.m_ExternalForce *= num2;
				}
			}
			this.AddForce(Vector3.Cross(Vector3.Cross(this.GroundNormal, Vector3.down), this.GroundNormal) * this.m_SlopeSlideSpeed * vp_TimeUtility.AdjustedTimeScale);
		}
		else
		{
			this.m_OnSteepGroundSince = 0f;
			this.m_SlideFast = false;
			this.m_SlopeSlideSpeed = 0f;
		}
		if (this.m_MotorThrottle != Vector3.zero)
		{
			this.m_Slide = false;
		}
		if (this.m_SlideFast)
		{
			this.m_SlideFallSpeed = base.Transform.position.y;
		}
		else if (slideFast && !this.Grounded)
		{
			this.m_FallSpeed = Mathf.Min(0f, base.Transform.position.y - this.m_SlideFallSpeed);
		}
		if (slide != this.m_Slide)
		{
			this.Player.SetState("Slide", this.m_Slide, true, false);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateOutOfControl()
	{
		if (this.m_ExternalForce.magnitude > 0.2f || this.m_FallSpeed < -0.2f || this.m_SlideFast)
		{
			this.Player.OutOfControl.Start(0f);
			return;
		}
		if (this.Player.OutOfControl.Active)
		{
			this.Player.OutOfControl.Stop(0f);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void FixedMove()
	{
		Physics.SyncTransforms();
		this.m_MoveDirection = Vector3.zero;
		this.m_MoveDirection += this.m_ExternalForce;
		this.m_MoveDirection += this.m_MotorThrottle;
		this.m_MoveDirection.x = this.m_MoveDirection.x * this.m_SpeedModifier;
		this.m_MoveDirection.z = this.m_MoveDirection.z * this.m_SpeedModifier;
		this.m_MoveDirection.y = this.m_MoveDirection.y + this.m_FallSpeed;
		if (this.MotorFreeFly && this.m_MoveDirection.y < 0f)
		{
			this.m_MotorThrottle.y = this.m_MotorThrottle.y + this.m_FallSpeed;
			this.m_FallSpeed = 0f;
		}
		if (this.MotorFreeFly)
		{
			this.m_MaxHeight = float.MinValue;
			this.m_MaxHeightInitialFallSpeed = 0f;
		}
		else
		{
			float num = base.Transform.position.y + Origin.position.y;
			if (this.m_Grounded || num > this.m_MaxHeight)
			{
				this.m_MaxHeight = num;
			}
		}
		bool flag = this.m_Grounded;
		if (flag)
		{
			Vector3 vector = this.m_MoveDirection;
			vector.y = 0f;
			if (vector != Vector3.zero)
			{
				vector = vector.normalized;
				vector.y = 0.15f;
				vector = vector.normalized;
				Vector3 vector2 = this.m_Transform.position - vector * this.m_CharacterController.radius;
				vector2.y += this.m_CharacterController.height + 0.18f;
				float num2 = this.m_CharacterController.radius * 2f + 0.5f;
				RaycastHit raycastHit;
				if (Physics.SphereCast(vector2, 0.05f, vector, ref raycastHit, num2, 1073807360))
				{
					flag = false;
				}
			}
		}
		if (flag)
		{
			this.m_CharacterController.stepOffset = this.m_StepHeight;
		}
		else
		{
			this.m_CharacterController.stepOffset = 0.1f;
		}
		this.m_CurrentAntiBumpOffset = 0f;
		if (this.m_Grounded && this.m_MotorThrottle.y <= 0.001f)
		{
			this.m_CurrentAntiBumpOffset = Mathf.Max(0.1f, Vector3.Scale(this.m_MoveDirection, Vector3.one - Vector3.up).magnitude);
			this.m_MoveDirection.y = this.m_MoveDirection.y - this.m_CurrentAntiBumpOffset;
		}
		this.m_PredictedPos = base.Transform.position + vp_MathUtility.NaNSafeVector3(this.m_MoveDirection * base.Delta * Time.timeScale, default(Vector3));
		if (this.m_Platform != null && this.m_PositionOnPlatform != Vector3.zero)
		{
			this.Player.Move.Send(vp_MathUtility.NaNSafeVector3(this.m_Platform.TransformPoint(this.m_PositionOnPlatform) - this.m_Transform.position, default(Vector3)));
		}
		this.Player.Move.Send(vp_MathUtility.NaNSafeVector3(this.m_MoveDirection * base.Delta * Time.timeScale, default(Vector3)));
		if (this.Player.Dead.Active)
		{
			this.Player.InputMoveVector.Set(Vector2.zero);
		}
		Physics.SphereCast(new Ray(base.Transform.position + Vector3.up * this.m_CharacterController.radius, Vector3.down), this.m_CharacterController.radius, ref this.m_GroundHit, this.m_SkinWidth + 0.001f, 1084850184);
		this.m_Grounded = this.m_GroundHit.collider != null;
		RaycastHit raycastHit2;
		if (!this.m_Grounded && Physics.SphereCast(new Ray(base.Transform.position + Vector3.up * this.m_CharacterController.radius, Vector3.down), this.m_CharacterController.radius, ref raycastHit2, (this.m_CharacterController.skinWidth + 0.001f) * 4f, 1084850184) && raycastHit2.collider is CharacterController)
		{
			this.m_Grounded = true;
			this.m_GroundHit = raycastHit2;
		}
		if (!this.m_Grounded && this.Player.Velocity.Get().y > 0f)
		{
			Physics.SphereCast(new Ray(base.Transform.position, Vector3.up), this.m_CharacterController.radius, ref this.m_CeilingHit, this.m_CharacterController.height - (this.m_CharacterController.radius - this.m_CharacterController.skinWidth) + 0.01f, 1084850184);
			this.m_HeadContact = this.m_CeilingHit.collider != null;
		}
		else
		{
			this.m_HeadContact = false;
		}
		if (this.m_GroundHit.transform == null && this.m_LastGroundHit.transform != null)
		{
			if (this.m_Platform != null && this.m_PositionOnPlatform != Vector3.zero)
			{
				this.AddForce(this.m_Platform.position - this.m_LastPlatformPos);
				this.m_Platform = null;
			}
			if (this.m_CurrentAntiBumpOffset != 0f)
			{
				this.Player.Move.Send(vp_MathUtility.NaNSafeVector3(this.m_CurrentAntiBumpOffset * Vector3.up, default(Vector3)) * base.Delta * Time.timeScale);
				this.m_PredictedPos += vp_MathUtility.NaNSafeVector3(this.m_CurrentAntiBumpOffset * Vector3.up, default(Vector3)) * base.Delta * Time.timeScale;
				this.m_MoveDirection.y = this.m_MoveDirection.y + this.m_CurrentAntiBumpOffset;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void SmoothMove()
	{
		if (Time.timeScale == 0f)
		{
			return;
		}
		if (this.Player.Driving.Active)
		{
			this.m_FixedPosition = base.Transform.position;
			this.m_SmoothPosition = base.Transform.position;
			return;
		}
		this.m_FixedPosition = base.Transform.position;
		base.Transform.position = this.m_SmoothPosition;
		Physics.SyncTransforms();
		this.Player.Move.Send(vp_MathUtility.NaNSafeVector3(this.m_MoveDirection * base.Delta * Time.timeScale, default(Vector3)));
		this.m_SmoothPosition = base.Transform.position;
		if (Vector3.Distance(base.Transform.position, this.m_SmoothPosition) > this.Player.Radius.Get())
		{
			this.m_SmoothPosition = this.m_FixedPosition;
		}
		if (this.m_Platform != null && (this.m_LastPlatformPos.y < this.m_Platform.position.y || this.m_LastPlatformPos.y > this.m_Platform.position.y))
		{
			this.m_SmoothPosition.y = base.Transform.position.y;
		}
		this.m_SmoothPosition = Vector3.Lerp(this.m_SmoothPosition, this.m_FixedPosition, Time.deltaTime);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void UpdateCollisions()
	{
		if (this.m_GroundHit.transform != null && this.m_GroundHit.transform != this.m_LastGroundHit.transform)
		{
			if (this.m_LastGroundHit.transform == null)
			{
				float num = -(this.CharacterController.velocity.y * 0.01f) * Time.timeScale;
				float num2 = base.Transform.position.y + Origin.position.y;
				float num3 = Mathf.Round((this.m_MaxHeight - num2) * 2f) / 2f;
				if (!this.MotorFreeFly && num3 >= 0f)
				{
					float num4 = Math.Abs(this.m_GravityForce);
					this.m_FallImpact = (float)Math.Sqrt(Math.Pow((double)this.m_MaxHeightInitialFallSpeed, 2.0) + (double)(2f * num4 * num3)) * 0.8677499f;
				}
				else
				{
					this.m_FallImpact = num;
				}
				this.m_MaxHeight = float.MinValue;
				this.m_MaxHeightInitialFallSpeed = 0f;
				this.m_SmoothPosition.y = base.Transform.position.y;
				this.DeflectDownForce();
				this.Player.FallImpact.Send(this.m_FallImpact);
				this.Player.FallImpact2.Send(this.m_FallImpact);
				this.m_MotorThrottle.y = 0f;
				this.m_MotorJumpForceAcc = 0f;
				this.m_MotorJumpForceHoldSkipFrames = 0;
			}
			if (this.m_GroundHit.collider.gameObject.layer == 28)
			{
				this.m_Platform = this.m_GroundHit.transform;
				this.m_LastPlatformAngle = this.m_Platform.eulerAngles.y;
			}
			else
			{
				this.m_Platform = null;
			}
			Terrain component = this.m_GroundHit.transform.GetComponent<Terrain>();
			if (component != null)
			{
				this.m_CurrentTerrain = component;
			}
			else
			{
				this.m_CurrentTerrain = null;
			}
			vp_SurfaceIdentifier component2 = this.m_GroundHit.transform.GetComponent<vp_SurfaceIdentifier>();
			if (component2 != null)
			{
				this.m_CurrentSurface = component2;
			}
			else
			{
				this.m_CurrentSurface = null;
			}
		}
		else
		{
			this.m_FallImpact = 0f;
		}
		this.m_LastGroundHit = this.m_GroundHit;
		if (this.m_PredictedPos.y > base.Transform.position.y && (this.m_ExternalForce.y > 0f || this.m_MotorThrottle.y > 0f))
		{
			this.DeflectUpForce();
		}
		if (this.m_PredictedPos.x != base.Transform.position.x || this.m_PredictedPos.z != base.Transform.position.z)
		{
			this.DeflectHorizontalForce();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void UpdateSlopeFactor()
	{
		if (!this.m_Grounded)
		{
			this.m_SlopeFactor = 1f;
			return;
		}
		Vector3 motorThrottle = this.m_MotorThrottle;
		motorThrottle.y = 0f;
		float num = Vector3.Angle(this.m_GroundHit.normal, motorThrottle);
		this.m_SlopeFactor = 1f + (1f - num / 90f);
		if (Mathf.Abs(1f - this.m_SlopeFactor) < 0.25f)
		{
			this.m_SlopeFactor = 1f;
			return;
		}
		if (this.m_SlopeFactor <= 1f)
		{
			if (this.MotorSlopeSpeedUp == 1f)
			{
				this.m_SlopeFactor *= 1.2f;
			}
			else
			{
				this.m_SlopeFactor *= this.MotorSlopeSpeedUp;
			}
			this.m_SlopeFactor = ((this.GroundAngle > this.Player.SlopeLimit.Get()) ? 0f : this.m_SlopeFactor);
			return;
		}
		if (this.MotorSlopeSpeedDown == 1f)
		{
			this.m_SlopeFactor = 1f / this.m_SlopeFactor;
			this.m_SlopeFactor *= 1.2f;
			return;
		}
		this.m_SlopeFactor *= this.MotorSlopeSpeedDown;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void UpdatePlatformMove()
	{
		if (this.m_Platform == null)
		{
			return;
		}
		this.m_PositionOnPlatform = this.m_Platform.InverseTransformPoint(this.m_Transform.position);
		this.Player.Rotation.Set(new Vector2(this.Player.Rotation.Get().x, this.Player.Rotation.Get().y - Mathf.DeltaAngle(this.m_Platform.eulerAngles.y, this.m_LastPlatformAngle)));
		this.m_LastPlatformAngle = this.m_Platform.eulerAngles.y;
		this.m_LastPlatformPos = this.m_Platform.position;
		this.m_SmoothPosition = base.Transform.position;
	}

	public virtual void SetPosition(Vector3 position)
	{
		base.Transform.position = position;
		this.m_PrevPos = position;
		this.m_SmoothPosition = position;
		this.m_FixedPosition = position;
		Physics.SyncTransforms();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void AddForceInternal(Vector3 force)
	{
		this.m_ExternalForce += force;
	}

	public virtual void AddForce(float x, float y, float z)
	{
		this.AddForce(new Vector3(x, y, z));
	}

	public virtual void AddForce(Vector3 force)
	{
		if (Time.timeScale >= 1f)
		{
			this.AddForceInternal(force);
			return;
		}
		this.AddSoftForce(force, 1f);
	}

	public virtual void AddSoftForce(Vector3 force, float frames)
	{
		force /= Time.timeScale;
		frames = Mathf.Clamp(frames, 1f, 120f);
		this.AddForceInternal(force / frames);
		for (int i = 0; i < Mathf.RoundToInt(frames) - 1; i++)
		{
			this.m_SmoothForceFrame[i] += force / frames;
		}
	}

	public virtual void StopSoftForce()
	{
		int num = 0;
		while (num < 120 && !(this.m_SmoothForceFrame[num] == Vector3.zero))
		{
			this.m_SmoothForceFrame[num] = Vector3.zero;
			num++;
		}
	}

	public virtual void Stop()
	{
		this.Player.Move.Send(Vector3.zero);
		this.m_MotorThrottle = Vector3.zero;
		this.m_MotorJumpDone = true;
		this.m_MotorJumpForceAcc = 0f;
		this.m_ExternalForce = Vector3.zero;
		this.StopSoftForce();
		this.Player.InputMoveVector.Set(Vector2.zero);
		this.m_FallSpeed = 0f;
		this.m_SmoothPosition = base.Transform.position;
		this.m_MaxHeight = float.MinValue;
		this.m_MaxHeightInitialFallSpeed = 0f;
	}

	public virtual void DeflectDownForce()
	{
		if (this.GroundAngle > this.PhysicsSlopeSlideLimit)
		{
			this.m_SlopeSlideSpeed = this.m_FallImpact * (0.25f * Time.timeScale);
		}
		if (this.GroundAngle > 85f)
		{
			this.m_MotorThrottle += vp_3DUtility.HorizontalVector(this.GroundNormal * this.m_FallImpact);
			this.m_Grounded = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void DeflectUpForce()
	{
		if (!this.m_HeadContact)
		{
			return;
		}
		this.m_NewDir = Vector3.Cross(Vector3.Cross(this.m_CeilingHit.normal, Vector3.up), this.m_CeilingHit.normal);
		this.m_ForceImpact = this.m_MotorThrottle.y + this.m_ExternalForce.y;
		Vector3 vector = this.m_NewDir * (this.m_MotorThrottle.y + this.m_ExternalForce.y) * (1f - this.PhysicsWallFriction);
		this.m_ForceImpact -= vector.magnitude;
		this.AddForce(vector * Time.timeScale);
		this.m_MotorThrottle.y = 0f;
		this.m_ExternalForce.y = 0f;
		this.m_FallSpeed = 0f;
		this.m_NewDir.x = base.Transform.InverseTransformDirection(this.m_NewDir).x;
		this.Player.HeadImpact.Send((this.m_NewDir.x < 0f || (this.m_NewDir.x == 0f && Random.value < 0.5f)) ? (-this.m_ForceImpact) : this.m_ForceImpact);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void DeflectHorizontalForce()
	{
		this.m_PredictedPos.y = base.Transform.position.y;
		this.m_PrevPos.y = base.Transform.position.y;
		this.m_PrevDir = (this.m_PredictedPos - this.m_PrevPos).normalized;
		this.CapsuleBottom = this.m_PrevPos + Vector3.up * this.Player.Radius.Get();
		this.CapsuleTop = this.CapsuleBottom + Vector3.up * (this.Player.Height.Get() - this.Player.Radius.Get() * 2f);
		if (!Physics.CapsuleCast(this.CapsuleBottom, this.CapsuleTop, this.Player.Radius.Get(), this.m_PrevDir, ref this.m_WallHit, Vector3.Distance(this.m_PrevPos, this.m_PredictedPos) + 0.07f, 1084850184))
		{
			return;
		}
		this.m_NewDir = Vector3.Cross(this.m_WallHit.normal, Vector3.up).normalized;
		if (Vector3.Dot(Vector3.Cross(this.m_WallHit.point - base.Transform.position, this.m_PrevPos - base.Transform.position), Vector3.up) > 0f)
		{
			this.m_NewDir = -this.m_NewDir;
		}
		this.m_ForceMultiplier = Mathf.Abs(Vector3.Dot(this.m_PrevDir, this.m_NewDir)) * (1f - this.PhysicsWallFriction);
		if (this.PhysicsWallBounce > 0f)
		{
			this.m_NewDir = Vector3.Lerp(this.m_NewDir, Vector3.Reflect(this.m_PrevDir, this.m_WallHit.normal), this.PhysicsWallBounce);
			this.m_ForceMultiplier = Mathf.Lerp(this.m_ForceMultiplier, 1f, this.PhysicsWallBounce * (1f - this.PhysicsWallFriction));
		}
		if (this.m_ExternalForce != Vector3.zero)
		{
			this.m_ForceImpact = 0f;
			float y = this.m_ExternalForce.y;
			this.m_ExternalForce.y = 0f;
			this.m_ForceImpact = this.m_ExternalForce.magnitude;
			this.m_ExternalForce = this.m_NewDir * this.m_ExternalForce.magnitude * this.m_ForceMultiplier;
			this.m_ForceImpact -= this.m_ExternalForce.magnitude;
			int num = 0;
			while (num < 120 && !(this.m_SmoothForceFrame[num] == Vector3.zero))
			{
				this.m_SmoothForceFrame[num] = this.m_SmoothForceFrame[num].magnitude * this.m_NewDir * this.m_ForceMultiplier;
				num++;
			}
			this.m_ExternalForce.y = y;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void RefreshColliders()
	{
		if (this.Player.Crouch.Active && (!this.MotorFreeFly || this.Grounded))
		{
			this.CharacterController.height = this.m_CrouchHeight;
			this.CharacterController.center = this.m_CrouchCenter;
		}
		else
		{
			this.CharacterController.height = this.m_NormalHeight;
			this.CharacterController.center = this.m_NormalCenter;
		}
		if (this.m_TriggerCollider != null)
		{
			this.m_TriggerCollider.radius = this.CharacterController.radius + this.m_SkinWidth;
			this.m_TriggerCollider.height = this.CharacterController.height + this.m_SkinWidth * 2f;
			this.m_TriggerCollider.center = this.CharacterController.center;
		}
	}

	public float CalculateMaxSpeed(string stateName = "Default", float accelDuration = 5f)
	{
		if (stateName != "Default")
		{
			bool flag = false;
			using (List<vp_State>.Enumerator enumerator = this.States.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current.Name == stateName)
					{
						flag = true;
					}
				}
			}
			if (!flag)
			{
				Debug.LogError(string.Concat(new string[]
				{
					"Error (",
					(this != null) ? this.ToString() : null,
					") Controller has no such state: '",
					stateName,
					"'."
				}));
				return 0f;
			}
		}
		Dictionary<vp_State, bool> dictionary = new Dictionary<vp_State, bool>();
		foreach (vp_State vp_State in this.States)
		{
			dictionary.Add(vp_State, vp_State.Enabled);
			vp_State.Enabled = false;
		}
		base.StateManager.Reset();
		if (stateName != "Default")
		{
			base.SetState(stateName, true, false, false);
		}
		float num = 0f;
		float num2 = 5f;
		int num3 = 0;
		while ((float)num3 < 60f * num2)
		{
			num += this.MotorAcceleration * 0.1f * 60f;
			num /= 1f + this.MotorDamping;
			num3++;
		}
		foreach (vp_State vp_State2 in this.States)
		{
			bool flag2;
			dictionary.TryGetValue(vp_State2, out flag2);
			vp_State2.Enabled = flag2;
		}
		return num;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnControllerColliderHit(ControllerColliderHit hit)
	{
		Rigidbody attachedRigidbody = hit.collider.attachedRigidbody;
		if (attachedRigidbody == null || attachedRigidbody.isKinematic)
		{
			return;
		}
		if (hit.moveDirection.y < -0.3f)
		{
			return;
		}
		Vector3 vector;
		vector..ctor(hit.moveDirection.x, 0f, hit.moveDirection.z);
		attachedRigidbody.velocity = vector * (this.PhysicsPushForce / attachedRigidbody.mass);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual bool CanStart_Jump()
	{
		return this.MotorFreeFly || (this.m_Grounded && this.m_MotorJumpDone);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual bool CanStart_Swim()
	{
		return true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnStart_Swim()
	{
		this.m_ExternalForce.y = this.m_ExternalForce.y + this.m_FallSpeed * 0.2f;
		this.m_FallSpeed = 0f;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual bool CanStart_Run()
	{
		return !this.Player.Crouch.Active;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnStart_Jump()
	{
		this.m_SlopeSlideSpeed *= 0.2f;
		this.m_MotorJumpDone = false;
		if (this.MotorFreeFly && !this.Grounded)
		{
			return;
		}
		this.m_MotorThrottle.y = this.MotorJumpForce / Time.timeScale;
		this.m_SmoothPosition.y = base.Transform.position.y;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnStop_Jump()
	{
		this.m_MotorJumpDone = true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual bool CanStop_Crouch()
	{
		if (Physics.SphereCast(new Ray(base.Transform.position, Vector3.up), this.Player.Radius.Get(), this.m_NormalHeight - this.Player.Radius.Get() + 0.01f, 1084850184))
		{
			this.Player.Crouch.NextAllowedStopTime = Time.time + 0.1f;
			return false;
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnStart_Crouch()
	{
		this.Player.Run.Stop(0f);
		this.RefreshColliders();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnStop_Crouch()
	{
		this.RefreshColliders();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnMessage_ForceImpact(Vector3 force)
	{
		this.AddForce(force);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnMessage_Stop()
	{
		this.Stop();
	}

	public virtual Vector3 OnValue_Position
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return base.Transform.position;
		}
		[PublicizedFrom(EAccessModifier.Protected)]
		set
		{
			this.SetPosition(value);
		}
	}

	public virtual Vector3 OnValue_Velocity
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return this.CharacterController.velocity;
		}
	}

	public virtual float OnValue_StepOffset
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return this.CharacterController.stepOffset;
		}
	}

	public virtual float OnValue_SlopeLimit
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return this.CharacterController.slopeLimit;
		}
	}

	public virtual float OnValue_Radius
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return this.CharacterController.radius;
		}
	}

	public virtual float OnValue_Height
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return this.CharacterController.height;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnMessage_Move(Vector3 direction)
	{
		if (this.CharacterController.enabled)
		{
			this.LastMoveCollisionFlags = this.CharacterController.Move(direction);
		}
	}

	public virtual Vector3 OnValue_MotorThrottle
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return this.m_MotorThrottle;
		}
		[PublicizedFrom(EAccessModifier.Protected)]
		set
		{
			this.m_MotorThrottle = value;
		}
	}

	public virtual bool OnValue_MotorJumpDone
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return this.m_MotorJumpDone;
		}
	}

	public virtual float OnValue_FallSpeed
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return this.m_FallSpeed;
		}
		[PublicizedFrom(EAccessModifier.Protected)]
		set
		{
			this.m_FallSpeed = value;
		}
	}

	public virtual Transform OnValue_Platform
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return this.m_Platform;
		}
		[PublicizedFrom(EAccessModifier.Protected)]
		set
		{
			this.m_Platform = value;
		}
	}

	public virtual Texture OnValue_GroundTexture
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			if (this.GroundTransform == null)
			{
				return null;
			}
			if (this.GroundTransform.GetComponent<Renderer>() == null && this.m_CurrentTerrain == null)
			{
				return null;
			}
			int num = -1;
			if (this.m_CurrentTerrain != null)
			{
				num = vp_FootstepManager.GetMainTerrainTexture(this.Player.Position.Get(), this.m_CurrentTerrain);
				if (num > this.m_CurrentTerrain.terrainData.terrainLayers.Length - 1)
				{
					return null;
				}
			}
			if (!(this.m_CurrentTerrain == null))
			{
				return this.m_CurrentTerrain.terrainData.terrainLayers[num].diffuseTexture;
			}
			return this.GroundTransform.GetComponent<Renderer>().material.mainTexture;
		}
	}

	public virtual vp_SurfaceIdentifier OnValue_SurfaceType
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return this.m_CurrentSurface;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnStop_Dead()
	{
		this.Player.OutOfControl.Stop(0f);
	}

	public void ScaleFallSpeed(float scale)
	{
		this.m_FallSpeed *= scale;
		this.m_MaxHeightInitialFallSpeed *= scale;
		float num = base.Transform.position.y + Origin.position.y;
		this.m_MaxHeight = num + scale * scale * (this.m_MaxHeight - num);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public vp_FPPlayerEventHandler m_Player;

	public EntityPlayerLocal localPlayer;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public CharacterController m_CharacterController;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Vector3 m_FixedPosition = Vector3.zero;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Vector3 m_SmoothPosition = Vector3.zero;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float m_SpeedModifier = 1f;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public bool m_Grounded;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public bool m_HeadContact;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public RaycastHit m_GroundHit;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public RaycastHit m_LastGroundHit;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public RaycastHit m_CeilingHit;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public RaycastHit m_WallHit;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float m_FallImpact;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Terrain m_CurrentTerrain;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public vp_SurfaceIdentifier m_CurrentSurface;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public CollisionFlags LastMoveCollisionFlags;

	public float MotorAcceleration = 0.18f;

	public float MotorDamping = 0.17f;

	public float MotorBackwardsSpeed = 0.65f;

	public float MotorSidewaysSpeed = 0.65f;

	public float MotorAirSpeed = 0.35f;

	public float MotorSlopeSpeedUp = 1f;

	public float MotorSlopeSpeedDown = 1f;

	public bool MotorFreeFly;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Vector3 m_MoveDirection = Vector3.zero;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float m_SlopeFactor = 1f;

	public Vector3 m_MotorThrottle = Vector3.zero;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float m_MotorAirSpeedModifier = 1f;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float m_CurrentAntiBumpOffset;

	public float originalMotorJumpForce = -1f;

	public float MotorJumpForce = 0.18f;

	public float MotorJumpForceDamping = 0.08f;

	public float MotorJumpForceHold = 0.003f;

	public float MotorJumpForceHoldDamping = 0.5f;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public int m_MotorJumpForceHoldSkipFrames;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float m_MotorJumpForceAcc;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public bool m_MotorJumpDone = true;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float m_FallSpeed;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float m_MaxHeight = float.MinValue;

	public float m_MaxHeightInitialFallSpeed;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float m_GravityForce;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 slideLastGroundN;

	public float PhysicsForceDamping = 0.05f;

	public float PhysicsPushForce = 5f;

	public float PhysicsGravityModifier = 0.2f;

	public float PhysicsSlopeSlideLimit = 30f;

	public float PhysicsSlopeSlidiness = 0.15f;

	public float PhysicsWallBounce;

	public float PhysicsWallFriction;

	public float PhysicsCrouchHeightModifier = 0.5f;

	public bool PhysicsHasCollisionTrigger = true;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public GameObject m_Trigger;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public CapsuleCollider m_TriggerCollider;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Vector3 m_ExternalForce = Vector3.zero;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Vector3[] m_SmoothForceFrame = new Vector3[120];

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public bool m_Slide;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public bool m_SlideFast;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float m_SlideFallSpeed;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float m_OnSteepGroundSince;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float m_SlopeSlideSpeed;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Vector3 m_PredictedPos = Vector3.zero;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Vector3 m_PrevPos = Vector3.zero;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Vector3 m_PrevDir = Vector3.zero;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Vector3 m_NewDir = Vector3.zero;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float m_ForceImpact;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float m_ForceMultiplier;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Vector3 CapsuleBottom = Vector3.zero;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Vector3 CapsuleTop = Vector3.zero;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float m_StepHeight = 0.7f;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float m_SkinWidth = 0.08f;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Transform m_Platform;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Vector3 m_PositionOnPlatform = Vector3.zero;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float m_LastPlatformAngle;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Vector3 m_LastPlatformPos = Vector3.zero;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float m_NormalHeight;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Vector3 m_NormalCenter = Vector3.zero;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float m_CrouchHeight;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Vector3 m_CrouchCenter = Vector3.zero;
}
