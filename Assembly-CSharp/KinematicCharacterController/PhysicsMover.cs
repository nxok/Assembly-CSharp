using System;
using UnityEngine;

namespace KinematicCharacterController
{
	[RequireComponent(typeof(Rigidbody))]
	public class PhysicsMover : MonoBehaviour
	{
		public int IndexInCharacterSystem { get; set; }

		public Vector3 InitialTickPosition { get; set; }

		public Quaternion InitialTickRotation { get; set; }

		public Transform Transform
		{
			get; [PublicizedFrom(EAccessModifier.Private)]
			set;
		}

		public Vector3 InitialSimulationPosition
		{
			get; [PublicizedFrom(EAccessModifier.Private)]
			set;
		}

		public Quaternion InitialSimulationRotation
		{
			get; [PublicizedFrom(EAccessModifier.Private)]
			set;
		}

		public Vector3 TransientPosition
		{
			get
			{
				return this._internalTransientPosition;
			}
			[PublicizedFrom(EAccessModifier.Private)]
			set
			{
				this._internalTransientPosition = value;
			}
		}

		public Quaternion TransientRotation
		{
			get
			{
				return this._internalTransientRotation;
			}
			[PublicizedFrom(EAccessModifier.Private)]
			set
			{
				this._internalTransientRotation = value;
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void Reset()
		{
			this.ValidateData();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void OnValidate()
		{
			this.ValidateData();
		}

		public void ValidateData()
		{
			this.Rigidbody = base.gameObject.GetComponent<Rigidbody>();
			this.Rigidbody.centerOfMass = Vector3.zero;
			this.Rigidbody.useGravity = false;
			this.Rigidbody.drag = 0f;
			this.Rigidbody.angularDrag = 0f;
			this.Rigidbody.maxAngularVelocity = float.PositiveInfinity;
			this.Rigidbody.maxDepenetrationVelocity = float.PositiveInfinity;
			this.Rigidbody.collisionDetectionMode = 0;
			this.Rigidbody.isKinematic = true;
			this.Rigidbody.constraints = 0;
			this.Rigidbody.interpolation = 0;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void OnEnable()
		{
			KinematicCharacterSystem.EnsureCreation();
			KinematicCharacterSystem.RegisterPhysicsMover(this);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void OnDisable()
		{
			KinematicCharacterSystem.UnregisterPhysicsMover(this);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void Awake()
		{
			this.Transform = base.transform;
			this.ValidateData();
			this.TransientPosition = this.Rigidbody.position;
			this.TransientRotation = this.Rigidbody.rotation;
			this.InitialSimulationPosition = this.Rigidbody.position;
			this.InitialSimulationRotation = this.Rigidbody.rotation;
		}

		public void SetPosition(Vector3 position)
		{
			this.Transform.position = position;
			this.Rigidbody.position = position;
			this.InitialSimulationPosition = position;
			this.TransientPosition = position;
		}

		public void SetRotation(Quaternion rotation)
		{
			this.Transform.rotation = rotation;
			this.Rigidbody.rotation = rotation;
			this.InitialSimulationRotation = rotation;
			this.TransientRotation = rotation;
		}

		public void SetPositionAndRotation(Vector3 position, Quaternion rotation)
		{
			this.Transform.SetPositionAndRotation(position, rotation);
			this.Rigidbody.position = position;
			this.Rigidbody.rotation = rotation;
			this.InitialSimulationPosition = position;
			this.InitialSimulationRotation = rotation;
			this.TransientPosition = position;
			this.TransientRotation = rotation;
		}

		public PhysicsMoverState GetState()
		{
			return new PhysicsMoverState
			{
				Position = this.TransientPosition,
				Rotation = this.TransientRotation,
				Velocity = this.Rigidbody.velocity,
				AngularVelocity = this.Rigidbody.velocity
			};
		}

		public void ApplyState(PhysicsMoverState state)
		{
			this.SetPositionAndRotation(state.Position, state.Rotation);
			this.Rigidbody.velocity = state.Velocity;
			this.Rigidbody.angularVelocity = state.AngularVelocity;
		}

		public void VelocityUpdate(float deltaTime)
		{
			this.InitialSimulationPosition = this.TransientPosition;
			this.InitialSimulationRotation = this.TransientRotation;
			this.MoverController.UpdateMovement(out this._internalTransientPosition, out this._internalTransientRotation, deltaTime);
			if (deltaTime > 0f)
			{
				this.Rigidbody.velocity = (this.TransientPosition - this.InitialSimulationPosition) / deltaTime;
				Quaternion quaternion = this.TransientRotation * Quaternion.Inverse(this.InitialSimulationRotation);
				this.Rigidbody.angularVelocity = 0.017453292f * quaternion.eulerAngles / deltaTime;
			}
		}

		[ReadOnly]
		public Rigidbody Rigidbody;

		[NonSerialized]
		public IMoverController MoverController;

		[PublicizedFrom(EAccessModifier.Private)]
		[NonSerialized]
		public Vector3 _internalTransientPosition;

		[PublicizedFrom(EAccessModifier.Private)]
		[NonSerialized]
		public Quaternion _internalTransientRotation;
	}
}
