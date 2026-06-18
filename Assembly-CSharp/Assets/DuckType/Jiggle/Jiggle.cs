using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.DuckType.Jiggle
{
	public class Jiggle : MonoBehaviour
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public void OnDrawGizmos()
		{
			if (!this.ShowViewportGizmos)
			{
				return;
			}
			Vector3 vector = base.transform.localToWorldMatrix.MultiplyPoint3x4(this.CenterOfMass);
			Gizmos.color = Color.green;
			if (this.UseCenterOfMass)
			{
				Gizmos.DrawSphere(vector, this.CenterOfMassInertia * 5f * this.GizmoScale);
				Gizmos.DrawLine(base.transform.position, vector);
			}
			if (this.Hinge)
			{
				this.DrawGizmosArc(base.transform.position, base.transform.position + this.GetRestRotationWorld() * this.CenterOfMass * 11f * this.GizmoScale, this.GetHingeNormalWorld(), 360f);
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void OnDrawGizmosSelected()
		{
			if (!this.ShowViewportGizmos)
			{
				return;
			}
			if (this.UseAngleLimit & (this.AngleLimit > 0f))
			{
				float num = 10f * this.GizmoScale;
				Vector3 vector = this.GetRestRotationWorld() * this.CenterOfMass;
				List<Vector3> list;
				if (this.Hinge)
				{
					Vector3 vector2 = Vector3.Cross(vector, this.GetHingeNormalWorld());
					list = new List<Vector3>
					{
						vector2,
						-vector2
					};
				}
				else
				{
					list = vector.GetOrthogonalVectors(12);
				}
				foreach (Vector3 vector3 in list)
				{
					Gizmos.color = Color.red;
					Vector3 vector4 = ((this.AngleLimit < 90f) ? Vector3.RotateTowards(vector3, base.transform.rotation * this.CenterOfMass, (90f - this.AngleLimit) * 0.017453292f, 1f) : Vector3.RotateTowards(vector3, base.transform.rotation * -this.CenterOfMass, (this.AngleLimit - 90f) * 0.017453292f, 1f));
					vector4 *= num;
					Gizmos.DrawRay(base.transform.position, vector4);
					if (this.UseSoftLimit)
					{
						Gizmos.color = Color.yellow;
						Vector3 vector5 = base.transform.position + vector4;
						this.DrawGizmosArc(base.transform.position, vector5, Vector3.Cross(vector4, vector), this.AngleLimit * this.SoftLimitInfluence);
					}
				}
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void Start()
		{
			this.m_Initialised = true;
			this.m_RestLocalRotation = base.transform.localRotation;
			this.m_LastWorldRotation = base.transform.rotation;
			this.m_LastCenterOfMassWorld = base.transform.localToWorldMatrix.MultiplyPoint3x4(this.CenterOfMass);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void LateUpdate()
		{
			JiggleScheduler.Update(this);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void OnEnable()
		{
			JiggleScheduler.Register(this);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void OnDisable()
		{
			JiggleScheduler.Deregister(this);
		}

		public void ScheduledUpdate(float deltaTime)
		{
			Quaternion quaternion;
			if (this.RotationInertia > 0f)
			{
				quaternion = Quaternion.SlerpUnclamped(base.transform.rotation, this.m_LastWorldRotation, this.RotationInertia);
			}
			else
			{
				quaternion = base.transform.rotation;
			}
			if (this.UseCenterOfMass && this.CenterOfMassInertia > 0f)
			{
				Quaternion quaternion2 = Quaternion.FromToRotation(quaternion * this.CenterOfMass, this.m_LastCenterOfMassWorld - base.transform.position);
				Debug.DrawLine(this.m_LastCenterOfMassWorld, base.transform.position);
				quaternion = quaternion2.Scale(this.CenterOfMassInertia) * quaternion;
			}
			quaternion *= this.m_Torque.Scale(deltaTime / 0.001f);
			Quaternion quaternion3 = ((base.transform.parent != null) ? base.transform.parent.rotation : Quaternion.identity) * this.m_RestLocalRotation;
			if (this.BlendToOriginalRotation)
			{
				quaternion3 = base.transform.rotation;
			}
			float num = Quaternion.Angle(quaternion, quaternion3);
			if (this.UseAngleLimit && num > this.AngleLimit)
			{
				quaternion = Quaternion.Slerp(quaternion3, quaternion, this.AngleLimit / num);
			}
			if (this.Hinge)
			{
				Vector3 vector = quaternion * this.CenterOfMass;
				Vector3 hingeNormalWorld = this.GetHingeNormalWorld();
				Vector3 vector2 = Vector3.Cross(hingeNormalWorld, Vector3.Cross(vector, hingeNormalWorld));
				quaternion = Quaternion.FromToRotation(vector, vector2) * quaternion;
			}
			base.transform.rotation = quaternion;
			if (this.SpringStrength > 0f)
			{
				Quaternion quaternion4 = base.transform.rotation.FromToRotation(quaternion3);
				quaternion4 = quaternion4.Scale(0.001f * this.SpringStrength * 250f * deltaTime);
				this.m_Torque = this.m_Torque.Append(quaternion4);
			}
			if (this.UseCenterOfMass)
			{
				if (this.Gravity > 0f)
				{
					Quaternion closestRotationFromTo = this.GetClosestRotationFromTo(base.transform.rotation * this.CenterOfMass, Vector3.down);
					this.m_Torque = this.m_Torque.Append(closestRotationFromTo.Scale(0.001f * this.Gravity * 50f * deltaTime));
				}
				if (this.AddWind)
				{
					Quaternion closestRotationFromTo2 = this.GetClosestRotationFromTo(base.transform.rotation * this.CenterOfMass, this.WindDirection);
					this.m_Torque = this.m_Torque.Append(closestRotationFromTo2.Scale(0.001f * this.WindStrength * 50f * deltaTime));
				}
				if (this.AddNoise)
				{
					Vector3 noiseVector = this.GetNoiseVector(base.transform.localToWorldMatrix.MultiplyPoint3x4(this.CenterOfMass), this.NoiseScale * 10f, this.m_NoisePhase += deltaTime * this.NoiseSpeed);
					Quaternion closestRotationFromTo3 = this.GetClosestRotationFromTo(base.transform.rotation * this.CenterOfMass, noiseVector);
					this.m_Torque = this.m_Torque.Append(closestRotationFromTo3.Scale(0.001f * this.NoiseStrength * 50f * deltaTime));
				}
			}
			if (this.UseSoftLimit && this.UseAngleLimit && this.AngleLimit > 0f && this.SoftLimitStrength > 0f)
			{
				num = Quaternion.Angle(quaternion, quaternion3);
				float num2 = this.AngleLimit * (1f - this.SoftLimitInfluence);
				if (num > num2)
				{
					float num3 = Mathf.Min((num - num2) / (this.AngleLimit - num2), 1f);
					Quaternion quaternion5 = base.transform.rotation.FromToRotation(quaternion3);
					this.m_Torque = this.m_Torque.Append(quaternion5.Scale(0.001f * num3 * this.SoftLimitStrength * 250f * deltaTime));
				}
			}
			this.m_Torque = this.m_Torque.Scale((1f - this.Dampening * 10f * deltaTime).Clamp01());
			this.m_LastCenterOfMassWorld = base.transform.localToWorldMatrix.MultiplyPoint3x4(this.CenterOfMass);
			this.m_LastWorldRotation = base.transform.rotation;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public Vector3 GetHingeNormalWorld()
		{
			Vector3 vector = ((Mathf.Abs(this.CenterOfMass.normalized.y) != 1f) ? (Quaternion.AngleAxis(this.HingeAngle, this.CenterOfMass) * Vector3.Cross(this.CenterOfMass, Vector3.up)) : (Quaternion.AngleAxis(this.HingeAngle, this.CenterOfMass) * Vector3.Cross(this.CenterOfMass, Vector3.right)));
			return this.GetRestRotationWorld() * vector;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public Quaternion GetRestRotationWorld()
		{
			if (!this.m_Initialised)
			{
				return base.transform.rotation;
			}
			if (!(base.transform.parent != null))
			{
				return this.m_RestLocalRotation;
			}
			return base.transform.parent.rotation * this.m_RestLocalRotation;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public Vector3 GetNoiseVector(Vector3 pos, float scale, float phase)
		{
			pos /= scale;
			return new Vector3(Mathf.PerlinNoise(pos.x, pos.y + phase) - 0.5f, Mathf.PerlinNoise(pos.y, pos.z + phase) - 0.5f, Mathf.PerlinNoise(pos.z, pos.x + phase) - 0.5f);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public Quaternion GetClosestRotationFromTo(Vector3 from, Vector3 to)
		{
			Quaternion quaternion = Quaternion.FromToRotation(from, to) * base.transform.rotation;
			return base.transform.rotation.FromToRotation(quaternion);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void DrawGizmosArc(Vector3 center, Vector3 startPoint, Vector3 normal, float degrees)
		{
			int num = (int)Mathf.Ceil(degrees / 20f);
			Quaternion quaternion = Quaternion.AngleAxis(degrees / (float)num, normal);
			for (int i = 0; i < num; i++)
			{
				Vector3 vector = quaternion * (startPoint - center) + center;
				Gizmos.DrawRay(startPoint, vector - startPoint);
				startPoint = vector;
			}
		}

		public string DebugString()
		{
			return string.Format("{0} - enabled: {1}, m_Initialized: {2}, m_RestLocalRotation: {3}, UseCenterOfMass: {4}, CenterOfMass: {5}, Hinge: {6}, HingeAngle: {7}, UseAngleLimit: {8}, AngleLimit: {9}", new object[]
			{
				base.gameObject.name,
				base.isActiveAndEnabled,
				this.m_Initialised,
				this.m_RestLocalRotation,
				this.UseCenterOfMass,
				this.CenterOfMass,
				this.Hinge,
				this.HingeAngle,
				this.UseAngleLimit,
				this.AngleLimit
			});
		}

		[PublicizedFrom(EAccessModifier.Private)]
		[NonSerialized]
		public const float TORQUE_FACTOR = 0.001f;

		[PublicizedFrom(EAccessModifier.Private)]
		[NonSerialized]
		public bool m_Initialised;

		[PublicizedFrom(EAccessModifier.Private)]
		[NonSerialized]
		public Quaternion m_RestLocalRotation;

		[PublicizedFrom(EAccessModifier.Private)]
		[NonSerialized]
		public Quaternion m_LastWorldRotation;

		[PublicizedFrom(EAccessModifier.Private)]
		[NonSerialized]
		public Quaternion m_Torque = Quaternion.identity;

		[PublicizedFrom(EAccessModifier.Private)]
		[NonSerialized]
		public Vector3 m_LastCenterOfMassWorld;

		[PublicizedFrom(EAccessModifier.Private)]
		[NonSerialized]
		public float m_NoisePhase;

		public bool UpdateWithPhysics;

		public bool UseCenterOfMass = true;

		public Vector3 CenterOfMass = new Vector3(1f, 0f, 0f);

		public float CenterOfMassInertia = 1f;

		public bool AddWind;

		public Vector3 WindDirection = new Vector3(1f, 0f, 0f);

		public float WindStrength = 1f;

		public bool AddNoise;

		public float NoiseStrength = 1f;

		public float NoiseScale = 1f;

		public float NoiseSpeed = 1f;

		public float RotationInertia = 1f;

		public float Gravity;

		public float SpringStrength = 0.4f;

		public float Dampening = 0.4f;

		public bool BlendToOriginalRotation;

		public bool Hinge;

		public float HingeAngle;

		public bool UseAngleLimit;

		public float AngleLimit = 180f;

		public bool UseSoftLimit;

		public float SoftLimitInfluence = 0.5f;

		public float SoftLimitStrength = 0.5f;

		public bool ShowViewportGizmos = true;

		public float GizmoScale = 0.1f;
	}
}
