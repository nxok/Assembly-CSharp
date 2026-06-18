using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class vp_FPWeapon : vp_Weapon
{
	public CameraMatrixOverride CameraMatrixOverride
	{
		get
		{
			return this.m_CameraMatrixOverride;
		}
	}

	public GameObject WeaponModel
	{
		get
		{
			return this.m_WeaponModel;
		}
	}

	public Vector3 DefaultPosition
	{
		get
		{
			return (Vector3)base.DefaultState.Preset.GetFieldValue("PositionOffset");
		}
	}

	public Vector3 DefaultRotation
	{
		get
		{
			return (Vector3)base.DefaultState.Preset.GetFieldValue("RotationOffset");
		}
	}

	public bool DrawRetractionDebugLine
	{
		get
		{
			return this.m_DrawRetractionDebugLine;
		}
		set
		{
			this.m_DrawRetractionDebugLine = value;
		}
	}

	public vp_FPPlayerEventHandler FPPlayer
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (this.m_FPPlayer == null && base.EventHandler != null)
			{
				this.m_FPPlayer = base.EventHandler as vp_FPPlayerEventHandler;
			}
			return this.m_FPPlayer;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Awake()
	{
		base.Awake();
		if (base.transform.parent == null)
		{
			Debug.LogError("Error (" + ((this != null) ? this.ToString() : null) + ") Must not be placed in scene root. Disabling self.");
			vp_Utility.Activate(base.gameObject, false);
			return;
		}
		this.Controller = base.Transform.gameObject.GetComponentInParent<CharacterController>();
		if (this.Controller == null)
		{
			Debug.LogError("Error (" + ((this != null) ? this.ToString() : null) + ") Could not find CharacterController. Disabling self.");
			vp_Utility.Activate(base.gameObject, false);
			return;
		}
		base.Transform.eulerAngles = Vector3.zero;
		Transform transform = base.transform;
		while (transform != null && !transform.TryGetComponent<CameraMatrixOverride>(ref this.m_CameraMatrixOverride))
		{
			this.m_CameraMatrixOverride = null;
			transform = transform.parent;
		}
		if (this.m_CameraMatrixOverride == null)
		{
			Debug.LogWarning("vp_FPWeapon could not find CameraMatrixOverride in hierarchy, some functionality will be unavailable");
		}
		if (base.GetComponent<Collider>() != null)
		{
			base.GetComponent<Collider>().enabled = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Start()
	{
		this.InstantiateWeaponModel();
		base.Start();
		this.originalRenderingFieldOfView = this.RenderingFieldOfView;
		Transform transform = ((base.Transform.parent != null && base.Transform.parent.gameObject.name == "UFPSRoot") ? base.Transform.parent : base.Transform.FindParent("UFPSRoot"));
		if (transform == null)
		{
			transform = new GameObject("UFPSRoot").transform;
			transform.parent = base.Transform.parent;
		}
		this.m_WeaponGroup = ((transform != null) ? transform.gameObject : null);
		this.m_WeaponGroupTransform = this.m_WeaponGroup.transform;
		this.m_WeaponGroupTransform.localPosition = this.PositionOffset;
		vp_Layer.Set(this.m_WeaponGroup, 10, false);
		base.Transform.parent = this.m_WeaponGroupTransform;
		base.Transform.localPosition = Vector3.zero;
		this.m_WeaponGroupTransform.localEulerAngles = this.RotationOffset;
		this.m_Pivot = GameObject.CreatePrimitive(0);
		this.m_Pivot.name = "Pivot";
		this.m_Pivot.GetComponent<Collider>().enabled = false;
		this.m_Pivot.gameObject.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
		this.m_Pivot.transform.parent = this.m_WeaponGroupTransform;
		this.m_Pivot.transform.localPosition = Vector3.zero;
		this.m_Pivot.layer = 10;
		vp_Utility.Activate(this.m_Pivot.gameObject, false);
		Material material = new Material(Shader.Find("Transparent/Diffuse"));
		material.color = new Color(0f, 0f, 1f, 0.5f);
		this.m_Pivot.GetComponent<Renderer>().material = material;
		this.m_PositionSpring = new vp_Spring(this.m_WeaponGroup.gameObject.transform, vp_Spring.UpdateMode.Position, true);
		this.m_PositionSpring.RestState = this.PositionOffset;
		this.m_PositionPivotSpring = new vp_Spring(base.Transform, vp_Spring.UpdateMode.Position, true);
		this.m_PositionPivotSpring.RestState = this.PositionPivot;
		this.m_PositionSpring2 = new vp_Spring(base.Transform, vp_Spring.UpdateMode.PositionAdditiveLocal, true);
		this.m_PositionSpring2.MinVelocity = 1E-05f;
		this.m_RotationSpring = new vp_Spring(this.m_WeaponGroup.gameObject.transform, vp_Spring.UpdateMode.Rotation, true);
		this.m_RotationSpring.RestState = this.RotationOffset;
		this.m_RotationPivotSpring = new vp_Spring(base.Transform, vp_Spring.UpdateMode.Rotation, true);
		this.m_RotationPivotSpring.RestState = this.RotationPivot;
		this.m_RotationSpring2 = new vp_Spring(this.m_WeaponGroup.gameObject.transform, vp_Spring.UpdateMode.RotationAdditiveLocal, true);
		this.m_RotationSpring2.MinVelocity = 1E-05f;
		this.SnapSprings();
		this.Refresh();
	}

	public virtual void InstantiateWeaponModel()
	{
		if (this.WeaponPrefab != null)
		{
			if (this.m_WeaponModel != null && this.m_WeaponModel != base.gameObject)
			{
				Object.Destroy(this.m_WeaponModel);
			}
			this.m_WeaponModel = Object.Instantiate<GameObject>(this.WeaponPrefab);
			this.m_WeaponModel.transform.parent = base.transform;
			this.m_WeaponModel.transform.localPosition = Vector3.zero;
			this.m_WeaponModel.transform.localScale = new Vector3(1f, 1f, this.RenderingZScale);
			this.m_WeaponModel.transform.localEulerAngles = Vector3.zero;
		}
		else
		{
			this.m_WeaponModel = base.gameObject;
		}
		base.CacheRenderers();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Init()
	{
		base.Init();
		this.ScheduleAmbientAnimation();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Update()
	{
		base.Update();
		if (Time.timeScale != 0f)
		{
			this.UpdateInput();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void FixedUpdate()
	{
		if (Time.timeScale != 0f)
		{
			this.UpdateZoom();
			this.UpdateSwaying();
			this.UpdateBob();
			this.UpdateEarthQuake();
			this.UpdateStep();
			this.UpdateShakes();
			this.UpdateRetraction(true);
			this.UpdateSprings();
			this.UpdateLookDown();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void LateUpdate()
	{
	}

	public virtual void AddForce(Vector3 force)
	{
		this.m_PositionSpring.AddForce(force);
	}

	public virtual void AddForce(float x, float y, float z)
	{
		this.AddForce(new Vector3(x, y, z));
	}

	public virtual void AddForce(Vector3 positional, Vector3 angular)
	{
		this.m_PositionSpring.AddForce(positional);
		this.m_RotationSpring.AddForce(angular);
	}

	public virtual void AddForce(float xPos, float yPos, float zPos, float xRot, float yRot, float zRot)
	{
		this.AddForce(new Vector3(xPos, yPos, zPos), new Vector3(xRot, yRot, zRot));
	}

	public virtual void AddSoftForce(Vector3 force, int frames)
	{
		this.m_PositionSpring.AddSoftForce(force, (float)frames);
	}

	public virtual void AddSoftForce(float x, float y, float z, int frames)
	{
		this.AddSoftForce(new Vector3(x, y, z), frames);
	}

	public virtual void AddSoftForce(Vector3 positional, Vector3 angular, int frames)
	{
		this.m_PositionSpring.AddSoftForce(positional, (float)frames);
		this.m_RotationSpring.AddSoftForce(angular, (float)frames);
	}

	public virtual void AddSoftForce(float xPos, float yPos, float zPos, float xRot, float yRot, float zRot, int frames)
	{
		this.AddSoftForce(new Vector3(xPos, yPos, zPos), new Vector3(xRot, yRot, zRot), frames);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void UpdateInput()
	{
		if (base.Player.Dead.Active)
		{
			return;
		}
		this.m_LookInput = this.FPPlayer.InputRawLook.Get() / base.Delta * Time.timeScale * Time.timeScale;
		this.m_LookInput *= this.RotationInputVelocityScale;
		this.m_LookInput = Vector3.Min(this.m_LookInput, Vector3.one * this.RotationMaxInputVelocity);
		this.m_LookInput = Vector3.Max(this.m_LookInput, Vector3.one * -this.RotationMaxInputVelocity);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void UpdateZoom()
	{
		if (this.m_FinalZoomTime <= Time.time)
		{
			return;
		}
		if (!this.m_Wielded)
		{
			return;
		}
		this.RenderingZoomDamping = Mathf.Max(this.RenderingZoomDamping, 0.01f);
		float num = 1f - (this.m_FinalZoomTime - Time.time) / this.RenderingZoomDamping;
		if (this.m_CameraMatrixOverride != null)
		{
			this.m_CameraMatrixOverride.fov = Mathf.SmoothStep(this.m_CameraMatrixOverride.fov, this.RenderingFieldOfView, num * 15f);
		}
	}

	public virtual void Zoom()
	{
		this.m_FinalZoomTime = Time.time + this.RenderingZoomDamping;
	}

	public virtual void SnapZoom()
	{
		if (this.m_CameraMatrixOverride != null)
		{
			this.m_CameraMatrixOverride.fov = this.RenderingFieldOfView;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void UpdateShakes()
	{
		if (this.ShakeSpeed != 0f)
		{
			this.m_Shake = Vector3.Scale(vp_SmoothRandom.GetVector3Centered(this.ShakeSpeed), this.ShakeAmplitude);
			this.m_RotationSpring.AddForce(this.m_Shake * Time.timeScale);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void UpdateRetraction(bool firstIteration = true)
	{
		if (this.RetractionDistance == 0f)
		{
			return;
		}
		Vector3 vector = this.WeaponModel.transform.TransformPoint(this.RetractionOffset);
		Vector3 vector2 = vector + this.WeaponModel.transform.forward * this.RetractionDistance;
		RaycastHit raycastHit;
		if (Physics.Linecast(vector, vector2, ref raycastHit, 1084850184) && !raycastHit.collider.isTrigger)
		{
			this.WeaponModel.transform.position = raycastHit.point - (raycastHit.point - vector).normalized * (this.RetractionDistance * 0.99f);
			this.WeaponModel.transform.localPosition = Vector3.forward * Mathf.Min(this.WeaponModel.transform.localPosition.z, 0f);
			return;
		}
		if (firstIteration && this.WeaponModel.transform.localPosition != Vector3.zero && this.WeaponModel != base.gameObject)
		{
			this.WeaponModel.transform.localPosition = Vector3.forward * Mathf.SmoothStep(this.WeaponModel.transform.localPosition.z, 0f, this.RetractionRelaxSpeed * Time.timeScale);
			this.UpdateRetraction(false);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void UpdateBob()
	{
		if (this.BobAmplitude == Vector4.zero || this.BobRate == Vector4.zero)
		{
			return;
		}
		this.m_BobSpeed = ((this.BobRequireGroundContact && !this.Controller.isGrounded) ? 0f : this.Controller.velocity.sqrMagnitude);
		this.m_BobSpeed = Mathf.Min(this.m_BobSpeed * this.BobInputVelocityScale, this.BobMaxInputVelocity);
		this.m_BobSpeed = Mathf.Round(this.m_BobSpeed * 1000f) / 1000f;
		if (this.m_BobSpeed == 0f)
		{
			this.m_BobSpeed = Mathf.Min(this.m_LastBobSpeed * 0.93f, this.BobMaxInputVelocity);
		}
		this.m_CurrentBobAmp.x = this.m_BobSpeed * (this.BobAmplitude.x * -0.0001f);
		this.m_CurrentBobVal.x = Mathf.Cos(Time.time * (this.BobRate.x * 10f)) * this.m_CurrentBobAmp.x;
		this.m_CurrentBobAmp.y = this.m_BobSpeed * (this.BobAmplitude.y * 0.0001f);
		this.m_CurrentBobVal.y = Mathf.Cos(Time.time * (this.BobRate.y * 10f)) * this.m_CurrentBobAmp.y;
		this.m_CurrentBobAmp.z = this.m_BobSpeed * (this.BobAmplitude.z * 0.0001f);
		this.m_CurrentBobVal.z = Mathf.Cos(Time.time * (this.BobRate.z * 10f)) * this.m_CurrentBobAmp.z;
		this.m_CurrentBobAmp.w = this.m_BobSpeed * (this.BobAmplitude.w * 0.0001f);
		this.m_CurrentBobVal.w = Mathf.Cos(Time.time * (this.BobRate.w * 10f)) * this.m_CurrentBobAmp.w;
		this.m_RotationSpring.AddForce(this.m_CurrentBobVal * Time.timeScale);
		this.m_PositionSpring.AddForce(Vector3.forward * this.m_CurrentBobVal.w * Time.timeScale);
		this.m_LastBobSpeed = this.m_BobSpeed;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void UpdateEarthQuake()
	{
		if (this.FPPlayer == null)
		{
			return;
		}
		if (!this.FPPlayer.CameraEarthQuake.Active)
		{
			return;
		}
		if (!this.Controller.isGrounded)
		{
			return;
		}
		Vector3 vector = this.FPPlayer.CameraEarthQuakeForce.Get();
		this.AddForce(new Vector3(0f, 0f, -vector.z * 0.015f), new Vector3(vector.y * 2f, -vector.x, vector.x * 2f));
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void UpdateSprings()
	{
		this.m_PositionSpring.FixedUpdate();
		this.m_PositionPivotSpring.FixedUpdate();
		this.m_RotationPivotSpring.FixedUpdate();
		this.m_RotationSpring.FixedUpdate();
		this.m_PositionSpring2.FixedUpdate();
		this.m_RotationSpring2.FixedUpdate();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateLookDown()
	{
		if (!this.LookDownActive)
		{
			return;
		}
		if (this.FPPlayer.Rotation.Get().x < 0f && this.m_LookDownPitch == 0f && this.m_LookDownYaw == 0f)
		{
			return;
		}
		if (this.FPPlayer.Rotation.Get().x > 0f)
		{
			this.m_LookDownPitch = Mathf.Lerp(this.m_LookDownPitch, vp_MathUtility.SnapToZero(Mathf.Max(0f, this.FPPlayer.Rotation.Get().x / 90f), 0.0001f), Time.deltaTime * 2f);
			this.m_LookDownYaw = Mathf.Lerp(this.m_LookDownYaw, vp_MathUtility.SnapToZero(Mathf.DeltaAngle(this.FPPlayer.Rotation.Get().y, this.FPPlayer.BodyYaw.Get()), 0.0001f) / 90f * vp_MathUtility.SnapToZero(Mathf.Max(0f, (this.FPPlayer.Rotation.Get().x - this.LookDownYawLimit) / (90f - this.LookDownYawLimit)), 0.0001f), Time.deltaTime * 2f);
		}
		else
		{
			this.m_LookDownPitch *= 0.9f;
			this.m_LookDownYaw *= 0.9f;
			if (this.m_LookDownPitch < 0.01f)
			{
				this.m_LookDownPitch = 0f;
			}
			if (this.m_LookDownYaw < 0.01f)
			{
				this.m_LookDownYaw = 0f;
			}
		}
		this.m_WeaponGroupTransform.localPosition = vp_MathUtility.NaNSafeVector3(Vector3.Lerp(this.m_WeaponGroupTransform.localPosition, this.LookDownPositionOffsetMiddle, this.m_LookDownCurve.Evaluate(this.m_LookDownPitch)), default(Vector3));
		this.m_WeaponGroupTransform.localRotation = vp_MathUtility.NaNSafeQuaternion(Quaternion.Slerp(this.m_WeaponGroupTransform.localRotation, Quaternion.Euler(this.LookDownRotationOffsetMiddle), this.m_LookDownPitch), default(Quaternion));
		if (this.m_LookDownYaw > 0f)
		{
			this.m_WeaponGroupTransform.localPosition = vp_MathUtility.NaNSafeVector3(Vector3.Lerp(this.m_WeaponGroupTransform.localPosition, this.LookDownPositionOffsetLeft, Mathf.SmoothStep(0f, 1f, this.m_LookDownYaw)), default(Vector3));
			this.m_WeaponGroupTransform.localRotation = vp_MathUtility.NaNSafeQuaternion(Quaternion.Slerp(this.m_WeaponGroupTransform.localRotation, Quaternion.Euler(this.LookDownRotationOffsetLeft), this.m_LookDownYaw), default(Quaternion));
		}
		else
		{
			this.m_WeaponGroupTransform.localPosition = vp_MathUtility.NaNSafeVector3(Vector3.Lerp(this.m_WeaponGroupTransform.localPosition, this.LookDownPositionOffsetRight, Mathf.SmoothStep(0f, 1f, -this.m_LookDownYaw)), default(Vector3));
			this.m_WeaponGroupTransform.localRotation = vp_MathUtility.NaNSafeQuaternion(Quaternion.Slerp(this.m_WeaponGroupTransform.localRotation, Quaternion.Euler(this.LookDownRotationOffsetRight), -this.m_LookDownYaw), default(Quaternion));
		}
		this.m_CurrentPosRestState = Vector3.Lerp(this.m_CurrentPosRestState, this.m_PositionSpring.RestState, Time.fixedDeltaTime);
		this.m_CurrentRotRestState = Vector3.Lerp(this.m_CurrentRotRestState, this.m_RotationSpring.RestState, Time.fixedDeltaTime);
		this.m_WeaponGroupTransform.localPosition += vp_MathUtility.NaNSafeVector3((this.m_PositionSpring.State - this.m_CurrentPosRestState) * (this.m_LookDownPitch * this.LookDownPositionSpringPower), default(Vector3));
		this.m_WeaponGroupTransform.localEulerAngles -= vp_MathUtility.NaNSafeVector3(new Vector3(Mathf.DeltaAngle(this.m_RotationSpring.State.x, this.m_CurrentRotRestState.x), Mathf.DeltaAngle(this.m_RotationSpring.State.y, this.m_CurrentRotRestState.y), Mathf.DeltaAngle(this.m_RotationSpring.State.z, this.m_CurrentRotRestState.z)) * (this.m_LookDownPitch * this.LookDownRotationSpringPower), default(Vector3));
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void UpdateStep()
	{
		if (this.StepMinVelocity <= 0f || (this.BobRequireGroundContact && !this.Controller.isGrounded) || this.Controller.velocity.sqrMagnitude < this.StepMinVelocity)
		{
			return;
		}
		bool flag = this.m_LastUpBob < this.m_CurrentBobVal.x;
		this.m_LastUpBob = this.m_CurrentBobVal.x;
		if (flag && !this.m_BobWasElevating)
		{
			if (Mathf.Cos(Time.time * (this.BobRate.x * 5f)) > 0f)
			{
				this.m_PosStep = this.StepPositionForce - this.StepPositionForce * this.StepPositionBalance;
				this.m_RotStep = this.StepRotationForce - this.StepPositionForce * this.StepRotationBalance;
			}
			else
			{
				this.m_PosStep = this.StepPositionForce + this.StepPositionForce * this.StepPositionBalance;
				this.m_RotStep = Vector3.Scale(this.StepRotationForce - this.StepPositionForce * this.StepRotationBalance, -Vector3.one + Vector3.right * 2f);
			}
			this.AddSoftForce(this.m_PosStep * this.StepForceScale, this.m_RotStep * this.StepForceScale, this.StepSoftness);
		}
		this.m_BobWasElevating = flag;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void UpdateSwaying()
	{
		this.m_SwayVel = this.Controller.velocity * this.PositionInputVelocityScale;
		this.m_SwayVel = Vector3.Min(this.m_SwayVel, Vector3.one * this.PositionMaxInputVelocity);
		this.m_SwayVel = Vector3.Max(this.m_SwayVel, Vector3.one * -this.PositionMaxInputVelocity);
		this.m_SwayVel *= Time.timeScale;
		Vector3 vector = base.Transform.InverseTransformDirection(this.m_SwayVel / 60f);
		this.m_RotationSpring.AddForce(new Vector3(this.m_LookInput.y * (this.RotationLookSway.x * 0.025f), this.m_LookInput.x * (this.RotationLookSway.y * -0.025f), this.m_LookInput.x * (this.RotationLookSway.z * -0.025f)));
		this.m_FallSway = this.RotationFallSway * (this.m_SwayVel.y * 0.005f);
		if (this.Controller.isGrounded)
		{
			this.m_FallSway *= this.RotationSlopeSway;
		}
		this.m_FallSway.z = Mathf.Max(0f, this.m_FallSway.z);
		this.m_RotationSpring.AddForce(this.m_FallSway);
		this.m_FallSwaySpringLimit.x = Mathf.Abs(this.RotationFallSwayAbsoluteSpringLimit.x);
		this.m_FallSwaySpringLimit.y = Mathf.Abs(this.RotationFallSwayAbsoluteSpringLimit.y);
		this.m_FallSwaySpringLimit.z = Mathf.Abs(this.RotationFallSwayAbsoluteSpringLimit.z);
		if (this.m_FallSwaySpringLimit.x > 0f)
		{
			this.m_RotationSpring.State.x = Utils.FastClamp(this.m_RotationSpring.State.x, -this.m_FallSwaySpringLimit.x, this.m_FallSwaySpringLimit.x);
		}
		if (this.m_FallSwaySpringLimit.y > 0f)
		{
			this.m_RotationSpring.State.y = Utils.FastClamp(this.m_RotationSpring.State.y, -this.m_FallSwaySpringLimit.y, this.m_FallSwaySpringLimit.y);
		}
		if (this.m_FallSwaySpringLimit.z > 0f)
		{
			this.m_RotationSpring.State.z = Utils.FastClamp(this.m_RotationSpring.State.z, -this.m_FallSwaySpringLimit.z, this.m_FallSwaySpringLimit.z);
		}
		this.m_PositionSpring.AddForce(Vector3.forward * -Mathf.Abs(this.m_SwayVel.y * (this.PositionFallRetract * 2.5E-05f)));
		this.m_PositionSpring.AddForce(new Vector3(vector.x * (this.PositionWalkSlide.x * 0.0016f), -Mathf.Abs(vector.x * (this.PositionWalkSlide.y * 0.0016f)), -vector.z * (this.PositionWalkSlide.z * 0.0016f)));
		this.m_RotationSpring.AddForce(new Vector3(-Mathf.Abs(vector.x * (this.RotationStrafeSway.x * 0.16f)), -(vector.x * (this.RotationStrafeSway.y * 0.16f)), vector.x * (this.RotationStrafeSway.z * 0.16f)));
	}

	public virtual void ResetSprings(float positionReset, float rotationReset, float positionPauseTime = 0f, float rotationPauseTime = 0f)
	{
		this.m_PositionSpring.State = Vector3.Lerp(this.m_PositionSpring.State, this.m_PositionSpring.RestState, positionReset);
		this.m_RotationSpring.State = Vector3.Lerp(this.m_RotationSpring.State, this.m_RotationSpring.RestState, rotationReset);
		this.m_PositionPivotSpring.State = Vector3.Lerp(this.m_PositionPivotSpring.State, this.m_PositionPivotSpring.RestState, positionReset);
		this.m_RotationPivotSpring.State = Vector3.Lerp(this.m_RotationPivotSpring.State, this.m_RotationPivotSpring.RestState, rotationReset);
		if (positionPauseTime != 0f)
		{
			this.m_PositionSpring.ForceVelocityFadeIn(positionPauseTime);
		}
		if (rotationPauseTime != 0f)
		{
			this.m_RotationSpring.ForceVelocityFadeIn(rotationPauseTime);
		}
		if (positionPauseTime != 0f)
		{
			this.m_PositionPivotSpring.ForceVelocityFadeIn(positionPauseTime);
		}
		if (rotationPauseTime != 0f)
		{
			this.m_RotationPivotSpring.ForceVelocityFadeIn(rotationPauseTime);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static Vector3 SmoothStep(Vector3 from, Vector3 to, float amount)
	{
		return new Vector3(Mathf.SmoothStep(from.x, to.x, amount), Mathf.SmoothStep(from.y, to.y, amount), Mathf.SmoothStep(from.z, to.z, amount));
	}

	public override void Refresh()
	{
		if (!Application.isPlaying)
		{
			return;
		}
		float num = 1f - (this.m_FinalZoomTime - Time.time) / Mathf.Max(this.RenderingZoomDamping, 0.01f);
		if (this.m_Wielded)
		{
			this.PositionOffset = vp_FPWeapon.SmoothStep(this.PositionOffset, this.DefaultPosition + this.AimingPositionOffset, num * 15f);
		}
		else
		{
			this.PositionOffset = vp_FPWeapon.SmoothStep(this.PositionOffset, this.PositionExitOffset, num * 15f);
		}
		if (this.m_PositionSpring != null)
		{
			this.m_PositionSpring.Stiffness = new Vector3(this.PositionSpringStiffness, this.PositionSpringStiffness, this.PositionSpringStiffness);
			this.m_PositionSpring.Damping = Vector3.one - new Vector3(this.PositionSpringDamping, this.PositionSpringDamping, this.PositionSpringDamping);
			this.m_PositionSpring.RestState = this.PositionOffset - this.PositionPivot;
		}
		if (this.m_PositionPivotSpring != null)
		{
			this.m_PositionPivotSpring.Stiffness = new Vector3(this.PositionPivotSpringStiffness, this.PositionPivotSpringStiffness, this.PositionPivotSpringStiffness);
			this.m_PositionPivotSpring.Damping = Vector3.one - new Vector3(this.PositionPivotSpringDamping, this.PositionPivotSpringDamping, this.PositionPivotSpringDamping);
			this.m_PositionPivotSpring.RestState = this.PositionPivot;
		}
		if (this.m_RotationPivotSpring != null)
		{
			this.m_RotationPivotSpring.Stiffness = new Vector3(this.RotationPivotSpringStiffness, this.RotationPivotSpringStiffness, this.RotationPivotSpringStiffness);
			this.m_RotationPivotSpring.Damping = Vector3.one - new Vector3(this.RotationPivotSpringDamping, this.RotationPivotSpringDamping, this.RotationPivotSpringDamping);
			this.m_RotationPivotSpring.RestState = this.RotationPivot;
		}
		if (this.m_PositionSpring2 != null)
		{
			this.m_PositionSpring2.Stiffness = new Vector3(this.PositionSpring2Stiffness, this.PositionSpring2Stiffness, this.PositionSpring2Stiffness);
			this.m_PositionSpring2.Damping = Vector3.one - new Vector3(this.PositionSpring2Damping, this.PositionSpring2Damping, this.PositionSpring2Damping);
			this.m_PositionSpring2.RestState = Vector3.zero;
		}
		if (this.m_RotationSpring != null)
		{
			this.m_RotationSpring.Stiffness = new Vector3(this.RotationSpringStiffness, this.RotationSpringStiffness, this.RotationSpringStiffness);
			this.m_RotationSpring.Damping = Vector3.one - new Vector3(this.RotationSpringDamping, this.RotationSpringDamping, this.RotationSpringDamping);
			this.m_RotationSpring.RestState = this.RotationOffset;
		}
		if (this.m_RotationSpring2 != null)
		{
			this.m_RotationSpring2.Stiffness = new Vector3(this.RotationSpring2Stiffness, this.RotationSpring2Stiffness, this.RotationSpring2Stiffness);
			this.m_RotationSpring2.Damping = Vector3.one - new Vector3(this.RotationSpring2Damping, this.RotationSpring2Damping, this.RotationSpring2Damping);
			this.m_RotationSpring2.RestState = Vector3.zero;
		}
		if (base.Rendering)
		{
			this.Zoom();
		}
	}

	public override void Activate()
	{
		base.Activate();
		this.SnapZoom();
		if (this.m_WeaponGroup != null && !vp_Utility.IsActive(this.m_WeaponGroup))
		{
			vp_Utility.Activate(this.m_WeaponGroup, true);
		}
		this.SetPivotVisible(false);
	}

	public override void Deactivate()
	{
		this.m_Wielded = false;
		if (this.m_WeaponGroup != null && vp_Utility.IsActive(this.m_WeaponGroup))
		{
			vp_Utility.Activate(this.m_WeaponGroup, false);
		}
	}

	public virtual void SnapPivot()
	{
		if (this.m_PositionSpring != null)
		{
			this.m_PositionSpring.RestState = this.PositionOffset - this.PositionPivot;
			this.m_PositionSpring.State = this.PositionOffset - this.PositionPivot;
		}
		if (this.m_WeaponGroup != null)
		{
			this.m_WeaponGroupTransform.localPosition = this.PositionOffset - this.PositionPivot;
		}
		if (this.m_PositionPivotSpring != null)
		{
			this.m_PositionPivotSpring.RestState = this.PositionPivot;
			this.m_PositionPivotSpring.State = this.PositionPivot;
		}
		if (this.m_RotationPivotSpring != null)
		{
			this.m_RotationPivotSpring.RestState = this.RotationPivot;
			this.m_RotationPivotSpring.State = this.RotationPivot;
		}
		base.Transform.localPosition = this.PositionPivot;
		base.Transform.localEulerAngles = this.RotationPivot;
	}

	public virtual void SetPivotVisible(bool visible)
	{
		if (this.m_Pivot == null)
		{
			return;
		}
		vp_Utility.Activate(this.m_Pivot.gameObject, visible);
	}

	public virtual void SnapToExit()
	{
		this.RotationOffset = this.RotationExitOffset;
		this.PositionOffset = this.PositionExitOffset;
		this.SnapSprings();
		this.SnapPivot();
	}

	public override void SnapSprings()
	{
		base.SnapSprings();
		if (this.m_PositionSpring != null)
		{
			this.m_PositionSpring.RestState = this.PositionOffset - this.PositionPivot;
			this.m_PositionSpring.State = this.PositionOffset - this.PositionPivot;
			this.m_PositionSpring.Stop(true);
		}
		if (this.m_WeaponGroup != null)
		{
			this.m_WeaponGroupTransform.localPosition = this.PositionOffset - this.PositionPivot;
		}
		if (this.m_PositionPivotSpring != null)
		{
			this.m_PositionPivotSpring.RestState = this.PositionPivot;
			this.m_PositionPivotSpring.State = this.PositionPivot;
			this.m_PositionPivotSpring.Stop(true);
		}
		base.Transform.localPosition = this.PositionPivot;
		if (this.m_RotationPivotSpring != null)
		{
			this.m_RotationPivotSpring.RestState = this.RotationPivot;
			this.m_RotationPivotSpring.State = this.RotationPivot;
			this.m_RotationPivotSpring.Stop(true);
		}
		base.Transform.localEulerAngles = this.RotationPivot;
		if (this.m_RotationSpring != null)
		{
			this.m_RotationSpring.RestState = this.RotationOffset;
			this.m_RotationSpring.State = this.RotationOffset;
			this.m_RotationSpring.Stop(true);
		}
	}

	public override void StopSprings()
	{
		if (this.m_PositionSpring != null)
		{
			this.m_PositionSpring.Stop(true);
		}
		if (this.m_PositionPivotSpring != null)
		{
			this.m_PositionPivotSpring.Stop(true);
		}
		if (this.m_RotationSpring != null)
		{
			this.m_RotationSpring.Stop(true);
		}
		if (this.m_RotationPivotSpring != null)
		{
			this.m_RotationPivotSpring.Stop(true);
		}
	}

	public override void Wield(bool isWielding = true)
	{
		if (isWielding)
		{
			this.SnapToExit();
		}
		this.PositionOffset = (isWielding ? (this.DefaultPosition + this.AimingPositionOffset) : this.PositionExitOffset);
		this.RotationOffset = (isWielding ? this.DefaultRotation : this.RotationExitOffset);
		this.m_Wielded = isWielding;
		this.Refresh();
		base.StateManager.CombineStates();
		if (base.Audio != null && (isWielding ? this.SoundWield : this.SoundUnWield) != null && vp_Utility.IsActive(base.gameObject))
		{
			base.Audio.pitch = Time.timeScale;
			base.Audio.PlayOneShot(isWielding ? this.SoundWield : this.SoundUnWield);
		}
		if ((isWielding ? this.AnimationWield : this.AnimationUnWield) != null && vp_Utility.IsActive(base.gameObject))
		{
			if (isWielding)
			{
				this.m_WeaponModel.GetComponent<Animation>().CrossFade(this.AnimationWield.name);
				return;
			}
			this.m_WeaponModel.GetComponent<Animation>().CrossFade(this.AnimationUnWield.name);
		}
	}

	public virtual void ScheduleAmbientAnimation()
	{
		if (this.AnimationAmbient.Count == 0 || !vp_Utility.IsActive(base.gameObject))
		{
			return;
		}
		vp_Timer.In(Random.Range(this.AmbientInterval.x, this.AmbientInterval.y), delegate
		{
			if (vp_Utility.IsActive(base.gameObject))
			{
				this.m_CurrentAmbientAnimation = Random.Range(0, this.AnimationAmbient.Count);
				if (this.AnimationAmbient[this.m_CurrentAmbientAnimation] != null)
				{
					this.m_WeaponModel.GetComponent<Animation>().CrossFadeQueued(this.AnimationAmbient[this.m_CurrentAmbientAnimation].name);
					this.ScheduleAmbientAnimation();
				}
			}
		}, this.m_AnimationAmbientTimer);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnMessage_FallImpact(float impact)
	{
		if (this.m_PositionSpring != null)
		{
			this.m_PositionSpring.AddSoftForce(Vector3.down * impact * this.PositionKneeling, (float)this.PositionKneelingSoftness);
		}
		if (this.m_RotationSpring != null)
		{
			this.m_RotationSpring.AddSoftForce(Vector3.right * impact * this.RotationKneeling, (float)this.RotationKneelingSoftness);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnMessage_HeadImpact(float impact)
	{
		this.AddForce(Vector3.zero, Vector3.forward * (impact * 20f) * Time.timeScale);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnMessage_CameraGroundStomp(float impact)
	{
		this.AddForce(Vector3.zero, new Vector3(-0.25f, 0f, 0f) * impact);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnMessage_CameraBombShake(float impact)
	{
		this.AddForce(Vector3.zero, new Vector3(-0.3f, 0.1f, 0.5f) * impact);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnMessage_CameraToggle3rdPerson()
	{
		this.RefreshWeaponModel();
	}

	public override Vector3 OnValue_AimDirection
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			if (this.FPPlayer.IsFirstPerson.Get())
			{
				return this.FPPlayer.HeadLookDirection.Get();
			}
			if (this.Weapon3rdPersonModel == null)
			{
				return this.FPPlayer.HeadLookDirection.Get();
			}
			return (this.Weapon3rdPersonModel.transform.position - this.FPPlayer.LookPoint.Get()).normalized;
		}
	}

	public GameObject WeaponPrefab;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public CharacterController Controller;

	public float RenderingZoomDamping = 0.5f;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float m_FinalZoomTime;

	public float RenderingFieldOfView = 75f;

	public float originalRenderingFieldOfView;

	public Vector2 RenderingClippingPlanes = new Vector2(0.01f, 10f);

	public float RenderingZScale = 1f;

	public float PositionSpringStiffness = 0.01f;

	public float PositionSpringDamping = 0.25f;

	public float PositionFallRetract = 1f;

	public float PositionPivotSpringStiffness = 0.01f;

	public float PositionPivotSpringDamping = 0.25f;

	public float PositionKneeling = 0.06f;

	public int PositionKneelingSoftness = 1;

	public Vector3 PositionWalkSlide = new Vector3(0.5f, 0.75f, 0.5f);

	public Vector3 PositionPivot = Vector3.zero;

	public Vector3 RotationPivot = Vector3.zero;

	public float PositionInputVelocityScale = 1f;

	public float PositionMaxInputVelocity = 25f;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public vp_Spring m_PositionSpring;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public vp_Spring m_PositionPivotSpring;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public vp_Spring m_RotationPivotSpring;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public CameraMatrixOverride m_CameraMatrixOverride;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public GameObject m_WeaponGroup;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public GameObject m_Pivot;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Transform m_WeaponGroupTransform;

	public float RotationSpringStiffness = 0.01f;

	public float RotationSpringDamping = 0.25f;

	public float RotationPivotSpringStiffness = 0.01f;

	public float RotationPivotSpringDamping = 0.25f;

	public float RotationKneeling;

	public int RotationKneelingSoftness = 1;

	public Vector3 RotationLookSway = new Vector3(1f, 0.7f, 0f);

	public Vector3 RotationStrafeSway = new Vector3(0.3f, 1f, 1.5f);

	public Vector3 RotationFallSway = new Vector3(1f, -0.5f, -3f);

	public float RotationSlopeSway = 0.5f;

	public float RotationInputVelocityScale = 1f;

	public float RotationMaxInputVelocity = 15f;

	public Vector3 RotationFallSwayAbsoluteSpringLimit = new Vector3(0f, 0f, 3f);

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public vp_Spring m_RotationSpring;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Vector3 m_SwayVel = Vector3.zero;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Vector3 m_FallSway = Vector3.zero;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Vector3 m_FallSwaySpringLimit = Vector3.zero;

	public float RetractionDistance;

	public Vector2 RetractionOffset = new Vector2(0f, 0f);

	public float RetractionRelaxSpeed = 0.25f;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public bool m_DrawRetractionDebugLine;

	public float ShakeSpeed = 0.05f;

	public Vector3 ShakeAmplitude = new Vector3(0.25f, 0f, 2f);

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Vector3 m_Shake = Vector3.zero;

	public Vector4 BobRate = new Vector4(0.9f, 0.45f, 0f, 0f);

	public Vector4 BobAmplitude = new Vector4(0.35f, 0.5f, 0f, 0f);

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

	public Vector3 StepPositionForce = new Vector3(0f, -0.0012f, -0.0012f);

	public Vector3 StepRotationForce = new Vector3(0f, 0f, 0f);

	public int StepSoftness = 4;

	public float StepMinVelocity;

	public float StepPositionBalance;

	public float StepRotationBalance;

	public float StepForceScale = 1f;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float m_LastUpBob;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public bool m_BobWasElevating;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Vector3 m_PosStep = Vector3.zero;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Vector3 m_RotStep = Vector3.zero;

	public bool LookDownActive;

	public float LookDownYawLimit = 60f;

	public Vector3 LookDownPositionOffsetMiddle = new Vector3(0.32f, -0.37f, 0.78f);

	public Vector3 LookDownPositionOffsetLeft = new Vector3(0.27f, -0.31f, 0.7f);

	public Vector3 LookDownPositionOffsetRight = new Vector3(0.6f, -0.41f, 0.86f);

	public float LookDownPositionSpringPower = 1f;

	public Vector3 LookDownRotationOffsetMiddle = new Vector3(-3.9f, 2.24f, 4.69f);

	public Vector3 LookDownRotationOffsetLeft = new Vector3(-7f, -10.5f, 15.6f);

	public Vector3 LookDownRotationOffsetRight = new Vector3(-9.2f, -9.8f, 48.84f);

	public float LookDownRotationSpringPower = 1f;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Vector3 m_CurrentPosRestState = Vector3.zero;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Vector3 m_CurrentRotRestState = Vector3.zero;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public AnimationCurve m_LookDownCurve = new AnimationCurve(new Keyframe[]
	{
		new Keyframe(0f, 0f),
		new Keyframe(0.8f, 0.2f, 0.9f, 1.5f),
		new Keyframe(1f, 1f)
	});

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float m_LookDownPitch;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float m_LookDownYaw;

	public AudioClip SoundWield;

	public AudioClip SoundUnWield;

	public AnimationClip AnimationWield;

	public AnimationClip AnimationUnWield;

	public List<Object> AnimationAmbient = new List<Object>();

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public List<bool> m_AmbAnimPlayed = new List<bool>();

	public Vector2 AmbientInterval = new Vector2(2.5f, 7.5f);

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public int m_CurrentAmbientAnimation;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public vp_Timer.Handle m_AnimationAmbientTimer = new vp_Timer.Handle();

	public Vector3 PositionExitOffset = new Vector3(0f, -1f, 0f);

	public Vector3 RotationExitOffset = new Vector3(40f, 0f, 0f);

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Vector2 m_LookInput = Vector2.zero;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public const float LOOKDOWNSPEED = 2f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public vp_FPPlayerEventHandler m_FPPlayer;
}
