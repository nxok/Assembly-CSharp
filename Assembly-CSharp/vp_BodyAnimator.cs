using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class vp_BodyAnimator : MonoBehaviour
{
	public vp_WeaponHandler WeaponHandler
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			if (this.m_WeaponHandler == null)
			{
				this.m_WeaponHandler = (vp_WeaponHandler)base.transform.root.GetComponentInChildren(typeof(vp_WeaponHandler));
			}
			return this.m_WeaponHandler;
		}
	}

	public Transform Transform
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			if (this.m_Transform == null)
			{
				this.m_Transform = base.transform;
			}
			return this.m_Transform;
		}
	}

	public vp_PlayerEventHandler Player
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			if (this.m_Player == null)
			{
				this.m_Player = (vp_PlayerEventHandler)base.transform.root.GetComponentInChildren(typeof(vp_PlayerEventHandler));
			}
			return this.m_Player;
		}
	}

	public SkinnedMeshRenderer Renderer
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			if (this.m_Renderer == null)
			{
				this.m_Renderer = base.transform.root.GetComponentInChildren<SkinnedMeshRenderer>();
			}
			return this.m_Renderer;
		}
	}

	public Animator Animator
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			if (this.m_Animator == null)
			{
				this.m_Animator = base.GetComponent<Animator>();
			}
			return this.m_Animator;
		}
	}

	public Vector3 m_LocalVelocity
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return vp_MathUtility.SnapToZero(this.Transform.root.InverseTransformDirection(this.Player.Velocity.Get()) / this.m_MaxSpeed, 0.0001f);
		}
	}

	public float m_MaxSpeed
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			if (this.Player.Run.Active)
			{
				return this.m_MaxRunSpeed;
			}
			if (this.Player.Crouch.Active)
			{
				return this.m_MaxCrouchSpeed;
			}
			return this.m_MaxWalkSpeed;
		}
	}

	public GameObject HeadPoint
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			if (this.m_HeadPoint == null)
			{
				this.m_HeadPoint = new GameObject("HeadPoint");
				this.m_HeadPoint.transform.parent = this.m_HeadLookBones[0].transform;
				this.m_HeadPoint.transform.localPosition = Vector3.zero;
				this.HeadPoint.transform.eulerAngles = this.Player.Rotation.Get();
			}
			return this.m_HeadPoint;
		}
	}

	public GameObject DebugLookTarget
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			if (this.m_DebugLookTarget == null)
			{
				this.m_DebugLookTarget = vp_3DUtility.DebugBall(null);
			}
			return this.m_DebugLookTarget;
		}
	}

	public GameObject DebugLookArrow
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			if (this.m_DebugLookArrow == null)
			{
				this.m_DebugLookArrow = vp_3DUtility.DebugPointer(null);
				this.m_DebugLookArrow.transform.parent = this.HeadPoint.transform;
				this.m_DebugLookArrow.transform.localPosition = Vector3.zero;
				this.m_DebugLookArrow.transform.localRotation = Quaternion.identity;
				return this.m_DebugLookArrow;
			}
			return this.m_DebugLookArrow;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnEnable()
	{
		if (this.Player != null)
		{
			this.Player.Register(this);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnDisable()
	{
		if (this.Player != null)
		{
			this.Player.Unregister(this);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Awake()
	{
		if (!this.IsValidSetup())
		{
			return;
		}
		this.InitHashIDs();
		this.InitHeadLook();
		this.InitMaxSpeeds();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void LateUpdate()
	{
		if (Time.timeScale == 0f)
		{
			return;
		}
		if (!this.m_IsValid)
		{
			base.enabled = false;
			return;
		}
		this.UpdatePosition();
		this.UpdateGrounding();
		this.UpdateBody();
		this.UpdateSpine();
		this.UpdateAnimationSpeeds();
		this.UpdateAnimator();
		this.UpdateDebugInfo();
		this.UpdateHeadPoint();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void UpdateAnimationSpeeds()
	{
		if (Time.time > this.m_NextAllowedUpdateTurnTargetTime)
		{
			this.m_CurrentTurnTarget = Mathf.DeltaAngle(this.m_PrevBodyYaw, this.m_BodyYaw) * (this.Player.Crouch.Active ? 100f : 0.2f);
			this.m_NextAllowedUpdateTurnTargetTime = Time.time + 0.1f;
		}
		this.m_CurrentTurn = Mathf.Lerp(this.m_CurrentTurn, this.m_CurrentTurnTarget, Time.deltaTime);
		if (Mathf.Round(this.Transform.root.eulerAngles.y) == Mathf.Round(this.m_LastYaw))
		{
			this.m_CurrentTurn *= 0.6f;
		}
		this.m_LastYaw = this.Transform.root.eulerAngles.y;
		this.m_CurrentTurn = vp_MathUtility.SnapToZero(this.m_CurrentTurn, 0.0001f);
		this.m_CurrentForward = Mathf.Lerp(this.m_CurrentForward, this.m_LocalVelocity.z, Time.deltaTime * 100f);
		this.m_CurrentForward = ((Mathf.Abs(this.m_CurrentForward) > 0.03f) ? this.m_CurrentForward : 0f);
		if (this.Player.Crouch.Active)
		{
			if (Mathf.Abs(this.GetStrafeDirection()) < Mathf.Abs(this.m_CurrentTurn))
			{
				this.m_CurrentStrafe = Mathf.Lerp(this.m_CurrentStrafe, this.m_CurrentTurn, Time.deltaTime * 5f);
			}
			else
			{
				this.m_CurrentStrafe = Mathf.Lerp(this.m_CurrentStrafe, this.GetStrafeDirection(), Time.deltaTime * 5f);
			}
		}
		else
		{
			this.m_CurrentStrafe = Mathf.Lerp(this.m_CurrentStrafe, this.GetStrafeDirection(), Time.deltaTime * 5f);
		}
		this.m_CurrentStrafe = ((Mathf.Abs(this.m_CurrentStrafe) > 0.03f) ? this.m_CurrentStrafe : 0f);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual float GetStrafeDirection()
	{
		if (this.Player.InputMoveVector.Get().x < 0f)
		{
			return -1f;
		}
		if (this.Player.InputMoveVector.Get().x > 0f)
		{
			return 1f;
		}
		return 0f;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void UpdateAnimator()
	{
		this.Animator.SetBool(this.IsRunning, this.Player.Run.Active && this.GetIsMoving());
		this.Animator.SetBool(this.IsCrouching, this.Player.Crouch.Active);
		this.Animator.SetInteger(this.WeaponTypeIndex, this.Player.CurrentWeaponType.Get());
		this.Animator.SetInteger(this.WeaponGripIndex, this.Player.CurrentWeaponGrip.Get());
		this.Animator.SetBool(this.IsSettingWeapon, this.Player.SetWeapon.Active);
		this.Animator.SetBool(this.IsReloading, this.Player.Reload.Active);
		this.Animator.SetBool(this.IsOutOfControl, this.Player.OutOfControl.Active);
		this.Animator.SetBool(this.IsClimbing, this.Player.Climb.Active);
		this.Animator.SetBool(this.IsZooming, this.Player.Zoom.Active);
		this.Animator.SetBool(this.IsGrounded, this.m_Grounded);
		this.Animator.SetBool(this.IsMoving, this.GetIsMoving());
		this.Animator.SetFloat(this.TurnAmount, this.m_CurrentTurn);
		this.Animator.SetFloat(this.ForwardAmount, this.m_CurrentForward);
		this.Animator.SetFloat(this.StrafeAmount, this.m_CurrentStrafe);
		this.Animator.SetFloat(this.PitchAmount, -this.Player.Rotation.Get().x / 90f);
		if (this.m_Grounded)
		{
			this.Animator.SetFloat(this.VerticalMoveAmount, 0f);
			return;
		}
		if (this.Player.Velocity.Get().y < 0f)
		{
			this.Animator.SetFloat(this.VerticalMoveAmount, Mathf.Lerp(this.Animator.GetFloat(this.VerticalMoveAmount), -1f, Time.deltaTime * 3f));
			return;
		}
		this.Animator.SetFloat(this.VerticalMoveAmount, this.Player.MotorThrottle.Get().y * 10f);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void UpdateDebugInfo()
	{
		if (this.ShowDebugObjects)
		{
			this.DebugLookTarget.transform.position = this.m_HeadLookBones[0].transform.position + this.HeadPoint.transform.forward * 1000f;
			this.DebugLookArrow.transform.LookAt(this.DebugLookTarget.transform.position);
			if (!vp_Utility.IsActive(this.m_DebugLookTarget))
			{
				vp_Utility.Activate(this.m_DebugLookTarget, true);
			}
			if (!vp_Utility.IsActive(this.m_DebugLookArrow))
			{
				vp_Utility.Activate(this.m_DebugLookArrow, true);
				return;
			}
		}
		else
		{
			if (this.m_DebugLookTarget != null)
			{
				vp_Utility.Activate(this.m_DebugLookTarget, false);
			}
			if (this.m_DebugLookArrow != null)
			{
				vp_Utility.Activate(this.m_DebugLookArrow, false);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateHeadPoint()
	{
		if (!this.HeadPointDirty)
		{
			return;
		}
		this.HeadPoint.transform.eulerAngles = this.Player.Rotation.Get();
		this.HeadPointDirty = false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void UpdatePosition()
	{
		if (this.Player.IsFirstPerson.Get())
		{
			return;
		}
		if (this.Player.Climb.Active)
		{
			this.Transform.localPosition += this.ClimbOffset;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void UpdateBody()
	{
		this.m_PrevBodyYaw = this.m_BodyYaw;
		this.m_BodyYaw = Mathf.LerpAngle(this.m_BodyYaw, this.m_CurrentBodyYawTarget, Time.deltaTime * ((this.Player.Velocity.Get().magnitude > 0.1f) ? this.FeetAdjustSpeedMoving : this.FeetAdjustSpeedStanding));
		this.m_BodyYaw = ((this.m_BodyYaw < -360f) ? (this.m_BodyYaw += 360f) : this.m_BodyYaw);
		this.m_BodyYaw = ((this.m_BodyYaw > 360f) ? (this.m_BodyYaw -= 360f) : this.m_BodyYaw);
		this.Transform.eulerAngles = this.m_BodyYaw * Vector3.up;
		this.m_CurrentHeadLookYaw = Mathf.DeltaAngle(this.Player.Rotation.Get().y, this.Transform.eulerAngles.y);
		if (Mathf.Max(0f, this.m_CurrentHeadLookYaw - 90f) > 0f)
		{
			this.Transform.eulerAngles = Vector3.up * (this.Transform.root.eulerAngles.y + 90f);
			this.m_BodyYaw = (this.m_CurrentBodyYawTarget = this.Transform.eulerAngles.y);
		}
		else if (Mathf.Min(0f, this.m_CurrentHeadLookYaw - -90f) < 0f)
		{
			this.Transform.eulerAngles = Vector3.up * (this.Transform.root.eulerAngles.y - 90f);
			this.m_BodyYaw = (this.m_CurrentBodyYawTarget = this.Transform.eulerAngles.y);
		}
		if (Mathf.Abs(this.Player.Rotation.Get().y - this.m_BodyYaw) > 180f)
		{
			if (this.m_BodyYaw > 0f)
			{
				this.m_BodyYaw -= 360f;
				this.m_PrevBodyYaw -= 360f;
			}
			else if (this.m_BodyYaw < 0f)
			{
				this.m_BodyYaw += 360f;
				this.m_PrevBodyYaw += 360f;
			}
		}
		if (this.m_CurrentHeadLookYaw > this.FeetAdjustAngle || this.m_CurrentHeadLookYaw < -this.FeetAdjustAngle || this.Player.Velocity.Get().magnitude > 0.1f)
		{
			this.m_CurrentBodyYawTarget = Mathf.LerpAngle(this.m_CurrentBodyYawTarget, this.Transform.root.eulerAngles.y, 0.1f);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void UpdateSpine()
	{
		for (int i = 0; i < this.m_HeadLookBones.Count; i++)
		{
			if (this.Player.IsFirstPerson.Get() || this.Animator.GetBool(this.IsAttacking) || this.Animator.GetBool(this.IsZooming))
			{
				this.m_HeadLookTargetFalloffs[i] = this.m_HeadLookFalloffs[this.m_HeadLookFalloffs.Count - 1 - i];
			}
			else
			{
				this.m_HeadLookTargetFalloffs[i] = this.m_HeadLookFalloffs[i];
			}
			if (this.m_WasMoving && !this.Animator.GetBool(this.IsMoving))
			{
				this.m_HeadLookCurrentFalloffs[i] = this.m_HeadLookTargetFalloffs[i];
			}
			this.m_HeadLookCurrentFalloffs[i] = Mathf.SmoothStep(this.m_HeadLookCurrentFalloffs[i], Mathf.LerpAngle(this.m_HeadLookCurrentFalloffs[i], this.m_HeadLookTargetFalloffs[i], Time.deltaTime * 10f), Time.deltaTime * 20f);
			if (this.Player.IsFirstPerson.Get())
			{
				this.m_HeadLookTargetWorldDir = this.GetLookPoint() - this.m_HeadLookBones[0].transform.position;
				this.m_HeadLookCurrentWorldDir = Vector3.Slerp(this.m_HeadLookTargetWorldDir, vp_3DUtility.HorizontalVector(this.m_HeadLookTargetWorldDir), this.m_HeadLookCurrentFalloffs[i] / this.m_HeadLookFalloffs[0]);
			}
			else
			{
				this.m_ValidLookPoint = this.GetLookPoint();
				this.m_ValidLookPointForward = this.Transform.InverseTransformDirection(this.m_ValidLookPoint - this.m_HeadLookBones[0].transform.position).z;
				if (this.m_ValidLookPointForward < 0f)
				{
					this.m_ValidLookPoint += this.Transform.forward * -this.m_ValidLookPointForward;
				}
				this.m_HeadLookTargetWorldDir = Vector3.Slerp(this.m_HeadLookTargetWorldDir, this.m_ValidLookPoint - this.m_HeadLookBones[0].transform.position, Time.deltaTime * this.HeadYawSpeed);
				this.m_HeadLookCurrentWorldDir = Vector3.Slerp(this.m_HeadLookCurrentWorldDir, vp_3DUtility.HorizontalVector(this.m_HeadLookTargetWorldDir), this.m_HeadLookCurrentFalloffs[i] / this.m_HeadLookFalloffs[0]);
			}
			this.m_HeadLookBones[i].transform.rotation = vp_3DUtility.GetBoneLookRotationInWorldSpace(this.m_HeadLookBones[i].transform.rotation, this.m_HeadLookBones[this.m_HeadLookBones.Count - 1].transform.parent.rotation, this.m_HeadLookCurrentWorldDir, this.m_HeadLookCurrentFalloffs[i], this.m_ReferenceLookDirs[i], this.m_ReferenceUpDirs[i], Quaternion.identity);
			if (!this.Player.IsFirstPerson.Get())
			{
				this.m_CurrentHeadLookPitch = Mathf.SmoothStep(this.m_CurrentHeadLookPitch, Mathf.Clamp(this.Player.Rotation.Get().x, -this.HeadPitchCap, this.HeadPitchCap), Time.deltaTime * this.HeadPitchSpeed);
				this.m_HeadLookBones[i].transform.Rotate(this.HeadPoint.transform.right, this.m_CurrentHeadLookPitch * Mathf.Lerp(this.m_HeadLookFalloffs[i], this.m_HeadLookCurrentFalloffs[i], this.LeaningFactor), 0);
			}
		}
		this.m_WasMoving = this.Animator.GetBool(this.IsMoving);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual bool GetIsMoving()
	{
		return Vector3.Scale(this.Player.Velocity.Get(), Vector3.right + Vector3.forward).magnitude > 0.01f;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual Vector3 GetLookPoint()
	{
		this.m_HeadLookBackup = this.HeadPoint.transform.eulerAngles;
		this.HeadPoint.transform.eulerAngles = this.Player.Rotation.Get();
		this.m_LookPoint = this.HeadPoint.transform.position + this.HeadPoint.transform.forward * 1000f;
		this.HeadPoint.transform.eulerAngles = this.m_HeadLookBackup;
		return this.m_LookPoint;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual List<float> CalculateBoneFalloffs(List<GameObject> boneList)
	{
		List<float> list = new List<float>();
		float num = 0f;
		for (int i = boneList.Count - 1; i > -1; i--)
		{
			if (boneList[i] == null)
			{
				boneList.RemoveAt(i);
			}
			else
			{
				float num2 = Mathf.Lerp(0f, 1f, (float)(i + 1) / (float)boneList.Count);
				list.Add(num2 * num2 * num2);
				num += num2 * num2 * num2;
			}
		}
		if (boneList.Count == 0)
		{
			return list;
		}
		for (int j = 0; j < list.Count; j++)
		{
			List<float> list2 = list;
			int num3 = j;
			list2[num3] *= 1f / num;
		}
		return list;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void StoreReferenceDirections()
	{
		for (int i = 0; i < this.m_HeadLookBones.Count; i++)
		{
			Quaternion quaternion = Quaternion.Inverse(this.m_HeadLookBones[this.m_HeadLookBones.Count - 1].transform.parent.rotation);
			this.m_ReferenceLookDirs.Add(quaternion * this.Transform.rotation * Vector3.forward);
			this.m_ReferenceUpDirs.Add(quaternion * this.Transform.rotation * Vector3.up);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void UpdateGrounding()
	{
		Physics.SphereCast(new Ray(this.Transform.position + Vector3.up * 0.5f, Vector3.down), 0.4f, ref this.m_GroundHit, 1f, 1084850184);
		this.m_Grounded = this.m_GroundHit.collider != null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RefreshWeaponStates()
	{
		if (this.WeaponHandler == null)
		{
			return;
		}
		if (this.WeaponHandler.CurrentWeapon == null)
		{
			return;
		}
		this.WeaponHandler.CurrentWeapon.SetState("Attack", this.Player.Attack.Active, false, false);
		this.WeaponHandler.CurrentWeapon.SetState("Zoom", this.Player.Zoom.Active, false, false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void InitMaxSpeeds()
	{
		vp_FPController vp_FPController = Object.FindAnyObjectByType<vp_FPController>();
		if (vp_FPController != null)
		{
			this.m_MaxWalkSpeed = vp_FPController.CalculateMaxSpeed("Default", 5f);
			this.m_MaxRunSpeed = vp_FPController.CalculateMaxSpeed("Run", 5f);
			this.m_MaxCrouchSpeed = vp_FPController.CalculateMaxSpeed("Crouch", 5f);
			return;
		}
		this.m_MaxWalkSpeed = 3.999999f;
		this.m_MaxRunSpeed = 10.08f;
		this.m_MaxCrouchSpeed = 1.44f;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void InitHashIDs()
	{
		this.ForwardAmount = Animator.StringToHash("Forward");
		this.PitchAmount = Animator.StringToHash("Pitch");
		this.StrafeAmount = Animator.StringToHash("Strafe");
		this.TurnAmount = Animator.StringToHash("Turn");
		this.VerticalMoveAmount = Animator.StringToHash("VerticalMove");
		this.IsAttacking = Animator.StringToHash("IsAttacking");
		this.IsClimbing = Animator.StringToHash("IsClimbing");
		this.IsCrouching = Animator.StringToHash("IsCrouching");
		this.IsGrounded = Animator.StringToHash("IsGrounded");
		this.IsMoving = Animator.StringToHash("IsMoving");
		this.IsOutOfControl = Animator.StringToHash("IsOutOfControl");
		this.IsReloading = Animator.StringToHash("IsReloading");
		this.IsRunning = Animator.StringToHash("IsRunning");
		this.IsSettingWeapon = Animator.StringToHash("IsSettingWeapon");
		this.IsZooming = Animator.StringToHash("IsZooming");
		this.StartClimb = Animator.StringToHash("StartClimb");
		this.StartOutOfControl = Animator.StringToHash("StartOutOfControl");
		this.StartReload = Animator.StringToHash("StartReload");
		this.WeaponGripIndex = Animator.StringToHash("WeaponGrip");
		this.WeaponTypeIndex = Animator.StringToHash("WeaponType");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void InitHeadLook()
	{
		if (!this.m_IsValid)
		{
			return;
		}
		this.m_HeadLookBones.Clear();
		GameObject gameObject = this.HeadBone;
		while (gameObject != this.LowestSpineBone.transform.parent.gameObject)
		{
			this.m_HeadLookBones.Add(gameObject);
			gameObject = gameObject.transform.parent.gameObject;
		}
		this.m_ReferenceUpDirs = new List<Vector3>();
		this.m_ReferenceLookDirs = new List<Vector3>();
		this.m_HeadLookFalloffs = this.CalculateBoneFalloffs(this.m_HeadLookBones);
		this.m_HeadLookCurrentFalloffs = new List<float>(this.m_HeadLookFalloffs);
		this.m_HeadLookTargetFalloffs = new List<float>(this.m_HeadLookFalloffs);
		this.StoreReferenceDirections();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool IsValidSetup()
	{
		if (this.HeadBone == null)
		{
			Debug.LogError("Error (" + ((this != null) ? this.ToString() : null) + ") No gameobject has been assigned for 'HeadBone'.");
		}
		else if (this.LowestSpineBone == null)
		{
			Debug.LogError("Error (" + ((this != null) ? this.ToString() : null) + ") No gameobject has been assigned for 'LowestSpineBone'.");
		}
		else if (!vp_Utility.IsDescendant(this.HeadBone.transform, base.transform.root))
		{
			this.NotInSameHierarchyError(this.HeadBone);
		}
		else if (!vp_Utility.IsDescendant(this.LowestSpineBone.transform, base.transform.root))
		{
			this.NotInSameHierarchyError(this.LowestSpineBone);
		}
		else
		{
			if (vp_Utility.IsDescendant(this.HeadBone.transform, this.LowestSpineBone.transform))
			{
				return true;
			}
			Debug.LogError("Error (" + ((this != null) ? this.ToString() : null) + ") 'HeadBone' must be a child or descendant of 'LowestSpineBone'.");
		}
		this.m_IsValid = false;
		base.enabled = false;
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void NotInSameHierarchyError(GameObject o)
	{
		Debug.LogError(string.Concat(new string[]
		{
			"Error '",
			(o != null) ? o.ToString() : null,
			"' can not be used as a bone for  ",
			(this != null) ? this.ToString() : null,
			" because it is not part of the same hierarchy."
		}));
	}

	public virtual float OnValue_BodyYaw
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return this.Transform.eulerAngles.y;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnStart_Attack()
	{
		this.m_AttackDoneTimer.Cancel();
		this.Animator.SetBool(this.IsAttacking, true);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnStop_Attack()
	{
		vp_Timer.In(0.5f, delegate
		{
			this.Animator.SetBool(this.IsAttacking, false);
			this.RefreshWeaponStates();
		}, this.m_AttackDoneTimer);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnStop_Zoom()
	{
		vp_Timer.In(0.5f, delegate
		{
			if (!this.Player.Attack.Active)
			{
				this.Animator.SetBool(this.IsAttacking, false);
			}
			this.RefreshWeaponStates();
		}, this.m_AttackDoneTimer);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnStart_Reload()
	{
		this.Animator.SetTrigger(this.StartReload);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnStart_OutOfControl()
	{
		this.Animator.SetTrigger(this.StartOutOfControl);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnStart_Climb()
	{
		this.Animator.SetTrigger(this.StartClimb);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnStart_Dead()
	{
		if (this.m_AttackDoneTimer.Active)
		{
			this.m_AttackDoneTimer.Execute();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnStop_Dead()
	{
		this.HeadPointDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnMessage_CameraToggle3rdPerson()
	{
		this.m_WasMoving = !this.m_WasMoving;
		this.HeadPointDirty = true;
	}

	public virtual Vector3 OnValue_HeadLookDirection
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return (this.Player.LookPoint.Get() - this.HeadPoint.transform.position).normalized;
		}
	}

	public virtual Vector3 OnValue_LookPoint
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return this.GetLookPoint();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public bool m_IsValid = true;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Vector3 m_ValidLookPoint = Vector3.zero;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float m_ValidLookPointForward;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public bool HeadPointDirty = true;

	public GameObject HeadBone;

	public GameObject LowestSpineBone;

	[Range(0f, 90f)]
	public float HeadPitchCap = 45f;

	[Range(2f, 20f)]
	public float HeadPitchSpeed = 7f;

	[Range(0.2f, 20f)]
	public float HeadYawSpeed = 2f;

	[Range(0f, 1f)]
	public float LeaningFactor = 0.25f;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public List<GameObject> m_HeadLookBones = new List<GameObject>();

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public List<Vector3> m_ReferenceUpDirs;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public List<Vector3> m_ReferenceLookDirs;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float m_CurrentHeadLookYaw;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float m_CurrentHeadLookPitch;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public List<float> m_HeadLookFalloffs = new List<float>();

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public List<float> m_HeadLookCurrentFalloffs;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public List<float> m_HeadLookTargetFalloffs;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Vector3 m_HeadLookTargetWorldDir;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Vector3 m_HeadLookCurrentWorldDir;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Vector3 m_HeadLookBackup = Vector3.zero;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Vector3 m_LookPoint = Vector3.zero;

	public float FeetAdjustAngle = 80f;

	public float FeetAdjustSpeedStanding = 10f;

	public float FeetAdjustSpeedMoving = 12f;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float m_PrevBodyYaw;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float m_BodyYaw;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float m_CurrentBodyYawTarget;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float m_LastYaw;

	public Vector3 ClimbOffset = Vector3.forward * 0.6f;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float m_CurrentForward;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float m_CurrentStrafe;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float m_CurrentTurn;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float m_CurrentTurnTarget;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float m_MaxWalkSpeed = 1f;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float m_MaxRunSpeed = 1f;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float m_MaxCrouchSpeed = 1f;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public bool m_WasMoving;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public RaycastHit m_GroundHit;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public bool m_Grounded = true;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public vp_Timer.Handle m_AttackDoneTimer = new vp_Timer.Handle();

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float m_NextAllowedUpdateTurnTargetTime;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public const float TURNMODIFIER = 0.2f;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public const float CROUCHTURNMODIFIER = 100f;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public const float MOVEMODIFIER = 100f;

	public bool ShowDebugObjects;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public int ForwardAmount;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public int PitchAmount;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public int StrafeAmount;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public int TurnAmount;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public int VerticalMoveAmount;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public int IsAttacking;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public int IsClimbing;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public int IsCrouching;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public int IsGrounded;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public int IsMoving;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public int IsOutOfControl;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public int IsReloading;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public int IsRunning;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public int IsSettingWeapon;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public int IsZooming;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public int StartClimb;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public int StartOutOfControl;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public int StartReload;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public int WeaponGripIndex;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public int WeaponTypeIndex;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public vp_WeaponHandler m_WeaponHandler;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Transform m_Transform;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public vp_PlayerEventHandler m_Player;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public SkinnedMeshRenderer m_Renderer;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Animator m_Animator;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public GameObject m_HeadPoint;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public GameObject m_DebugLookTarget;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public GameObject m_DebugLookArrow;
}
