using System;
using System.Collections.Generic;
using UnityEngine;

public class CharacterGazeController : MonoBehaviour
{
	public Transform LookAtTarget
	{
		get
		{
			if (this.lookAtTarget == null)
			{
				this.lookAtTarget = new GameObject("LookAtTarget").transform;
				this.lookAtTarget.parent = this.rootTransform;
				this.lookAtTarget.localPosition = this.lookAtTargetDefaultPosition;
				return this.lookAtTarget;
			}
			return this.lookAtTarget;
		}
	}

	public Transform LookAtTransformOverride
	{
		get
		{
			return this.lookAtTransformOverride;
		}
		set
		{
			this.lookAtTransformOverride = value;
		}
	}

	public GameRandom Random
	{
		get
		{
			if (this.random != null)
			{
				return this.random;
			}
			return this.random = GameRandomManager.Instance.CreateGameRandom();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Start()
	{
		this.entityAlive = base.GetComponentInParent<EntityAlive>();
		this.eyeMaterial = new Material(this.eyeSkinnedMeshRenderer.material);
		if (this.eyeMaterial == null || this.eyeMaterial.shader.name != "Game/SDCS/Eye")
		{
			Debug.LogError("Eye Material is not valid");
			base.enabled = false;
			return;
		}
		this.eyeSkinnedMeshRenderer.material = this.eyeMaterial;
		this.gazeTimer = Time.realtimeSinceStartup + this.Random.RandomRange(0.25f, 2f);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnEnable()
	{
		CharacterGazeController.instances.Add(this);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDisable()
	{
		if (CharacterGazeController.instances.Contains(this))
		{
			CharacterGazeController.instances.Remove(this);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDestroy()
	{
		if (CharacterGazeController.instances.Contains(this))
		{
			CharacterGazeController.instances.Remove(this);
		}
		if (this.lookAtTarget != null)
		{
			Object.Destroy(this.lookAtTarget.gameObject);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void LateUpdate()
	{
		if (this.entityAlive != null)
		{
			if (this.entityAlive.emodel.IsRagdollActive)
			{
				return;
			}
			this.isDead = this.entityAlive.IsDead();
		}
		this.UpdateLookAtTarget();
		this.UpdateHeadRotation();
		this.UpdateEyeGaze();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateLookAtTarget()
	{
		if (this.lookAtTransformOverride != null)
		{
			this.lookAtCamera = true;
			this.LookAtTarget.position = this.lookAtTransformOverride.position;
			return;
		}
		CharacterGazeController characterGazeController = null;
		float num = float.PositiveInfinity;
		for (int i = CharacterGazeController.instances.Count - 1; i >= 0; i--)
		{
			CharacterGazeController characterGazeController2 = CharacterGazeController.instances[i];
			if (characterGazeController2 == null)
			{
				CharacterGazeController.instances.RemoveAt(i);
			}
			else if (characterGazeController2.enabled && characterGazeController2 != this)
			{
				Vector3 vector = characterGazeController2.headTransform.position - this.headTransform.position;
				float num2 = Vector3.Angle(this.neckTransform.forward, vector);
				float magnitude = vector.magnitude;
				if (num2 < this.eyeLookAtTargetAngle && magnitude <= this.maxLookAtDistance && num2 < num)
				{
					characterGazeController = characterGazeController2;
					num = num2;
				}
			}
		}
		if (characterGazeController != null)
		{
			this.lookAtCamera = true;
			this.LookAtTarget.position = characterGazeController.headTransform.position;
		}
		else
		{
			bool flag = this.entityAlive is EntityPlayerLocal;
			EntityPlayerLocal entityPlayerLocal;
			if (GameManager.Instance != null && GameManager.Instance.World != null)
			{
				entityPlayerLocal = GameManager.Instance.World.GetPrimaryPlayer();
			}
			else
			{
				entityPlayerLocal = null;
			}
			Vector3 vector2 = this.rootTransform.TransformPoint(this.lookAtTargetDefaultPosition);
			if (entityPlayerLocal != null && entityPlayerLocal.cameraTransform != null)
			{
				if (flag)
				{
					if (entityPlayerLocal.bFirstPersonView)
					{
						vector2 = entityPlayerLocal.cameraTransform.position;
					}
					else if (entityPlayerLocal.IsCameraFacingCharacter())
					{
						vector2 = entityPlayerLocal.cameraTransform.position;
					}
					else
					{
						vector2 = entityPlayerLocal.cameraTransform.position + entityPlayerLocal.cameraTransform.forward * 10f;
					}
				}
				else if (entityPlayerLocal.bFirstPersonView)
				{
					vector2 = entityPlayerLocal.cameraTransform.position;
				}
				else
				{
					vector2 = entityPlayerLocal.getHeadPosition();
				}
			}
			else if (Camera.main != null)
			{
				vector2 = Camera.main.transform.position;
			}
			Vector3 vector3 = vector2 - this.headTransform.position;
			float num3 = Vector3.Angle(this.neckTransform.forward, vector3);
			float magnitude2 = vector3.magnitude;
			this.lookAtCamera = false;
			if (num3 < this.headLookAtTargetAngle && magnitude2 <= this.maxLookAtDistance)
			{
				this.lookAtCamera = true;
			}
			else if (entityPlayerLocal != null && flag && !entityPlayerLocal.bFirstPersonView && !entityPlayerLocal.inventory.holdingItem.IsActionRunning(entityPlayerLocal.inventory.holdingItemData))
			{
				this.lookAtCamera = true;
			}
			if (this.lookAtCamera)
			{
				this.LookAtTarget.position = vector2;
			}
			else
			{
				this.LookAtTarget.localPosition = this.lookAtTargetDefaultPosition;
			}
		}
		if (Time.realtimeSinceStartup > this.gazeTimer)
		{
			this.lookatOffsetIndex = this.Random.RandomRange(0, this.lookatOffsets.Count);
			this.gazeTimer = Time.realtimeSinceStartup + this.Random.RandomRange(0.25f, 2f);
		}
	}

	public void SnapNextUpdate()
	{
		this.shouldSnapHeadNextUpdate = true;
		this.shouldSnapEyesNextUpdate = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateHeadRotation()
	{
		Vector3 normalized = (this.LookAtTarget.position - this.headTransform.position).normalized;
		float num = Vector3.Angle(this.neckTransform.forward, normalized);
		Vector3 normalized2 = this.neckTransform.InverseTransformDirection(normalized).normalized;
		float num2 = Mathf.Atan2(normalized2.x, normalized2.z) * 57.29578f;
		float num3 = Mathf.Atan2(normalized2.y, new Vector2(normalized2.x, normalized2.z).magnitude) * 57.29578f;
		num2 *= 0.5f;
		float num4 = num3 * 0.75f;
		Quaternion quaternion = Quaternion.AngleAxis(num2, this.neckTransform.up);
		Vector3 vector = quaternion * this.neckTransform.forward;
		Vector3 vector2 = quaternion * this.neckTransform.right;
		Vector3 normalized3 = (Quaternion.AngleAxis(-num4, vector2) * vector).normalized;
		if (num < this.headLookAtTargetAngle && !this.isDead)
		{
			Quaternion quaternion2 = Quaternion.FromToRotation(this.neckTransform.forward, normalized3);
			quaternion2 *= Quaternion.Euler(0f, 0f, -(num2 / this.headLookAtTargetAngle) * 15f);
			if (this.shouldSnapHeadNextUpdate)
			{
				this.currentHeadRotation = quaternion2;
				this.shouldSnapHeadNextUpdate = false;
			}
			else
			{
				this.currentHeadRotation = Quaternion.Slerp(this.currentHeadRotation, quaternion2, this.headRotationSpeed * Time.deltaTime);
			}
		}
		else if (this.shouldSnapHeadNextUpdate)
		{
			this.currentHeadRotation = Quaternion.identity;
			this.shouldSnapHeadNextUpdate = false;
		}
		else
		{
			this.currentHeadRotation = Quaternion.Slerp(this.currentHeadRotation, Quaternion.identity, this.headRotationSpeed * Time.deltaTime);
		}
		this.headTransform.rotation = this.currentHeadRotation * this.neckTransform.rotation;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateEyeGaze()
	{
		Vector3 localPosition = this.LookAtTarget.localPosition;
		if (!this.lookAtCamera)
		{
			this.LookAtTarget.position += this.lookatOffsets[this.lookatOffsetIndex];
		}
		Vector3 vector = this.LookAtTarget.position - this.headTransform.TransformPoint(this.leftEyeLocalPosition);
		Vector3 vector2 = this.LookAtTarget.position - this.headTransform.TransformPoint(this.rightEyeLocalPosition);
		Quaternion quaternion = Quaternion.FromToRotation(this.leftEyeTransform.forward, vector);
		Quaternion quaternion2 = Quaternion.FromToRotation(this.rightEyeTransform.forward, vector2);
		float num = Mathf.Sin(Time.time * this.twitchSpeed) * 0.5f + 0.5f;
		if (this.isDead)
		{
			quaternion = Quaternion.identity;
			quaternion2 = Quaternion.identity;
		}
		if (this.shouldSnapEyesNextUpdate)
		{
			this.currentLeftEyeRotation = quaternion;
			this.currentRightEyeRotation = quaternion2;
			this.shouldSnapEyesNextUpdate = false;
		}
		else
		{
			this.currentLeftEyeRotation = Quaternion.Slerp(this.currentLeftEyeRotation, quaternion, this.eyeRotationSpeed * num * Time.deltaTime);
			this.currentRightEyeRotation = Quaternion.Slerp(this.currentRightEyeRotation, quaternion2, this.eyeRotationSpeed * num * Time.deltaTime);
		}
		this.eyeMaterial.SetVector("_LeftEyeRotation", new Vector4(-this.currentLeftEyeRotation.x, this.currentLeftEyeRotation.y, this.currentLeftEyeRotation.z, this.currentLeftEyeRotation.w));
		this.eyeMaterial.SetVector("_RightEyeRotation", new Vector4(-this.currentRightEyeRotation.x, this.currentRightEyeRotation.y, this.currentRightEyeRotation.z, this.currentRightEyeRotation.w));
		this.eyeMaterial.SetVector("_LeftEyePosition", Vector3.Scale(this.leftEyeLocalPosition, this.leftEyeTransform.lossyScale));
		this.eyeMaterial.SetVector("_RightEyePosition", Vector3.Scale(this.rightEyeLocalPosition, this.rightEyeTransform.lossyScale));
		this.LookAtTarget.localPosition = localPosition;
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public static void Cleanup()
	{
		CharacterGazeController.instances.Clear();
	}

	public static List<CharacterGazeController> instances = new List<CharacterGazeController>();

	public Transform leftEyeTransform;

	public Transform rightEyeTransform;

	public Vector3 leftEyeLocalPosition;

	public Vector3 rightEyeLocalPosition;

	public Transform rootTransform;

	public Transform neckTransform;

	public Transform headTransform;

	[Range(0f, 100f)]
	public float eyeRotationSpeed = 30f;

	[Range(0f, 100f)]
	public float headRotationSpeed = 7f;

	[Range(0f, 50f)]
	public float twitchSpeed = 25f;

	public float eyeLookAtTargetAngle = 35f;

	public float headLookAtTargetAngle = 75f;

	[Range(0f, 20f)]
	public float maxLookAtDistance = 5f;

	public SkinnedMeshRenderer eyeSkinnedMeshRenderer;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Material eyeMaterial;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Transform lookAtTransformOverride;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool lookAtCamera;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Transform lookAtTarget;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public GameRandom random;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public List<Vector3> lookatOffsets = new List<Vector3>
	{
		new Vector3(0f, 0f, 0f),
		new Vector3(0f, 0f, 0f),
		new Vector3(0f, 0f, 0f),
		new Vector3(0f, 0f, 0f),
		new Vector3(-1f, 0f, 0f),
		new Vector3(1f, 0f, 0f),
		new Vector3(-2f, 0f, 0f),
		new Vector3(2f, 0f, 0f),
		new Vector3(0f, 0f, -1f),
		new Vector3(0f, 0f, 1f),
		new Vector3(0f, 0f, -2f),
		new Vector3(0f, 0f, 2f)
	};

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public EntityAlive entityAlive;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Quaternion currentLeftEyeRotation;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Quaternion currentRightEyeRotation;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Quaternion currentHeadRotation;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float gazeTimer;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int lookatOffsetIndex;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool shouldSnapHeadNextUpdate;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool shouldSnapEyesNextUpdate;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool isDead;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 lookAtTargetDefaultPosition = new Vector3(0f, 1.7f, 10f);
}
