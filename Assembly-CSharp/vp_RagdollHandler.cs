using System;
using System.Collections.Generic;
using UnityEngine;

public class vp_RagdollHandler : MonoBehaviour
{
	public vp_PlayerEventHandler Player
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			if (this.m_Player == null && !this.m_TriedToFetchPlayer)
			{
				this.m_Player = base.transform.root.GetComponentInChildren<vp_PlayerEventHandler>();
				this.m_TriedToFetchPlayer = true;
			}
			return this.m_Player;
		}
	}

	public vp_FPCamera FPCamera
	{
		get
		{
			if (this.m_FPCamera == null)
			{
				this.m_FPCamera = base.transform.root.GetComponentInChildren<vp_FPCamera>();
			}
			return this.m_FPCamera;
		}
	}

	public CharacterController CharacterController
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			if (this.m_CharacterController == null)
			{
				this.m_CharacterController = base.transform.root.GetComponentInChildren<CharacterController>();
			}
			return this.m_CharacterController;
		}
	}

	public List<Collider> Colliders
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			if (this.m_Colliders == null)
			{
				this.m_Colliders = new List<Collider>();
				foreach (Collider collider in base.GetComponentsInChildren<Collider>())
				{
					if (collider.gameObject.layer != 23)
					{
						this.m_Colliders.Add(collider);
					}
				}
			}
			return this.m_Colliders;
		}
	}

	public List<Rigidbody> Rigidbodies
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			if (this.m_Rigidbodies == null)
			{
				this.m_Rigidbodies = new List<Rigidbody>(base.GetComponentsInChildren<Rigidbody>());
			}
			return this.m_Rigidbodies;
		}
	}

	public List<Transform> Transforms
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			if (this.m_Transforms == null)
			{
				this.m_Transforms = new List<Transform>();
				foreach (Rigidbody rigidbody in this.Rigidbodies)
				{
					this.m_Transforms.Add(rigidbody.transform);
				}
			}
			return this.m_Transforms;
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

	public vp_BodyAnimator BodyAnimator
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			if (this.m_BodyAnimator == null)
			{
				this.m_BodyAnimator = base.GetComponent<vp_BodyAnimator>();
			}
			return this.m_BodyAnimator;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Awake()
	{
		if (this.Colliders == null || this.Colliders.Count == 0 || this.Rigidbodies == null || this.Rigidbodies.Count == 0 || this.Transforms == null || this.Transforms.Count == 0 || this.Animator == null || this.BodyAnimator == null)
		{
			Debug.LogError("Error (" + ((this != null) ? this.ToString() : null) + ") Could not be initialized. Please make sure hierarchy has ragdoll colliders, Animator and vp_BodyAnimator.");
			base.enabled = false;
			return;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Start()
	{
		this.SetRagdoll(false);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void SaveStartPose()
	{
		foreach (Transform transform in this.Transforms)
		{
			if (!this.TransformRotations.ContainsKey(transform))
			{
				this.TransformRotations.Add(transform.transform, transform.localRotation);
			}
			if (!this.TransformPositions.ContainsKey(transform))
			{
				this.TransformPositions.Add(transform.transform, transform.localPosition);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void RestoreStartPose()
	{
		foreach (Transform transform in this.Transforms)
		{
			if (this.TransformRotations.TryGetValue(transform, out this.m_Rot))
			{
				transform.localRotation = this.m_Rot;
			}
			if (this.TransformPositions.TryGetValue(transform, out this.m_Pos))
			{
				transform.localPosition = this.m_Pos;
			}
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

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void LateUpdate()
	{
		this.UpdateDeathCamera();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void UpdateDeathCamera()
	{
		if (this.Player == null)
		{
			return;
		}
		if (!this.Player.Dead.Active)
		{
			return;
		}
		if (this.HeadBone == null)
		{
			return;
		}
		if (!this.Player.IsFirstPerson.Get())
		{
			return;
		}
		this.FPCamera.Transform.position = this.HeadBone.transform.position;
		this.m_HeadRotationCorrection = this.HeadBone.transform.localEulerAngles;
		if (Time.time - this.m_TimeOfDeath < this.CameraFreezeDelay)
		{
			this.FPCamera.Transform.localEulerAngles = (this.m_CameraFreezeAngle = new Vector3(-this.m_HeadRotationCorrection.z, -this.m_HeadRotationCorrection.x, this.m_HeadRotationCorrection.y));
			return;
		}
		this.FPCamera.Transform.localEulerAngles = this.m_CameraFreezeAngle;
	}

	public virtual void SetRagdoll(bool enabled = true)
	{
		if (this.Animator != null)
		{
			this.Animator.enabled = !enabled;
		}
		if (this.BodyAnimator != null)
		{
			this.BodyAnimator.enabled = !enabled;
		}
		if (this.CharacterController != null)
		{
			this.CharacterController.enabled = !enabled;
		}
		foreach (Rigidbody rigidbody in this.Rigidbodies)
		{
			rigidbody.isKinematic = !enabled;
		}
		foreach (Collider collider in this.Colliders)
		{
			collider.enabled = enabled;
		}
		if (!enabled)
		{
			this.RestoreStartPose();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnStart_Dead()
	{
		this.SetRagdoll(true);
		foreach (Rigidbody rigidbody in this.Rigidbodies)
		{
			rigidbody.AddForce(this.Player.Velocity.Get() * this.VelocityMultiplier);
		}
		this.m_TimeOfDeath = Time.time;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnStop_Dead()
	{
		this.SetRagdoll(false);
		this.Player.OutOfControl.Stop(0f);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public List<Collider> m_Colliders;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public List<Rigidbody> m_Rigidbodies;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public List<Transform> m_Transforms;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Animator m_Animator;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public vp_BodyAnimator m_BodyAnimator;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public vp_PlayerEventHandler m_Player;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public vp_FPCamera m_FPCamera;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public CharacterController m_CharacterController;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Vector3 m_HeadRotationCorrection = Vector3.zero;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float m_TimeOfDeath;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Vector3 m_CameraFreezeAngle = Vector3.zero;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Dictionary<Transform, Quaternion> TransformRotations = new Dictionary<Transform, Quaternion>();

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Dictionary<Transform, Vector3> TransformPositions = new Dictionary<Transform, Vector3>();

	public float VelocityMultiplier = 30f;

	public float CameraFreezeDelay = 2.5f;

	public GameObject HeadBone;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Quaternion m_Rot;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Vector3 m_Pos;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool m_TriedToFetchPlayer;
}
