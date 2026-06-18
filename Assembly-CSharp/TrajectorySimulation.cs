using System;
using System.Collections.Generic;
using UnityEngine;

public class TrajectorySimulation
{
	public bool TryGetSolution(out Trajectory.AimTrajectory trajectory)
	{
		if (this.TrajectoryState == TrajectorySimulation.State.DoneWithSolution)
		{
			trajectory = this.SimTrajectories[this.SimTrajectoryIndex];
			return true;
		}
		trajectory = default(Trajectory.AimTrajectory);
		return false;
	}

	public TrajectorySimulation(EntityAlive owner, EntityAlive target, float maxSimTime, TrajectorySimulation.ProjectileInfo projectile, float renderDuration, Action<TrajectorySimulation> onSimulationComplete)
	{
		this.TrajectoryState = TrajectorySimulation.State.ReadyToSimulate;
		this.Owner = owner;
		this.Target = target;
		this.MaxSimTime = maxSimTime;
		this.Projectile = projectile;
		this.RenderDuration = renderDuration;
		this.LaunchPosition = this.Owner.position;
		this.TargetPosition = this.Target.position;
		this.OnSimulationComplete = onSimulationComplete;
		this.CullPlane = new Plane(Vector3.up, this.TargetPosition - Vector3.up * this.Projectile.ProjectileTargetRadius);
	}

	public bool IsValid()
	{
		if (this.TrajectoryState == TrajectorySimulation.State.Invalid)
		{
			return false;
		}
		if (!(this.Owner != null) || !(this.Target != null))
		{
			return false;
		}
		Vector3 position = this.Owner.transform.position;
		Vector3 position2 = this.Target.transform.position;
		float sqrMagnitude = (position - this.LaunchPosition).sqrMagnitude;
		float sqrMagnitude2 = (position2 - this.TargetPosition).sqrMagnitude;
		if (sqrMagnitude > 16f)
		{
			this.TrajectoryState = TrajectorySimulation.State.Invalid;
			return false;
		}
		if (sqrMagnitude2 > 16f)
		{
			this.TrajectoryState = TrajectorySimulation.State.Invalid;
			return false;
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TickSimulation(int maxIterations = 20)
	{
		if (this.Owner == null || this.Target == null)
		{
			this.TrajectoryState = TrajectorySimulation.State.Invalid;
			this.OnSimulationComplete(this);
			return;
		}
		if (this.TrajectoryState == TrajectorySimulation.State.ReadyToSimulate)
		{
			this.LaunchPosition = this.Owner.getHeadPosition();
			this.TargetPosition = this.Target.position;
			this.SimTrajectoryIndex = 0;
			this.SimTrajectoryCount = Trajectory.Calculate(this.LaunchPosition, this.TargetPosition, this.Projectile.ProjectileSpeed, this.Projectile.ProjectileGravity, this.SimTrajectories);
			if (this.SimTrajectoryCount == 0)
			{
				this.TrajectoryState = TrajectorySimulation.State.DoneWithoutSolution;
				this.OnSimulationComplete(this);
				return;
			}
			this.SimTime = 0f;
			this.SimVelocity = this.SimTrajectories[this.SimTrajectoryIndex].LaunchVelocity;
			this.SimCurrentPos = (this.SimLastPos = this.Owner.getHeadPosition());
			this.TrajectoryState = TrajectorySimulation.State.Simulating;
		}
		if (this.SimTrajectoryIndex >= this.SimTrajectoryCount)
		{
			this.TrajectoryState = TrajectorySimulation.State.DoneWithoutSolution;
			this.OnSimulationComplete(this);
			return;
		}
		if (this.TrajectoryState != TrajectorySimulation.State.Simulating)
		{
			return;
		}
		World world = GameManager.Instance.World;
		while (this.SimTime < this.MaxSimTime)
		{
			if (maxIterations <= 0)
			{
				return;
			}
			if (this.CullPlane.GetDistanceToPoint(this.SimCurrentPos) < 0f)
			{
				break;
			}
			if (this.SimVelocity.magnitude < 1f)
			{
				if (this.RenderDuration > 0f)
				{
					Utils.DrawBounds(new Bounds(this.SimCurrentPos, Vector3.one * this.Projectile.ProjectileRadius), Color.magenta, this.RenderDuration);
					break;
				}
				break;
			}
			else
			{
				Vector3 vector = this.SimVelocity * 0.05f;
				this.SimCurrentPos += vector;
				this.SimVelocity.y = this.SimVelocity.y + this.Projectile.ProjectileGravity * 0.05f;
				if (this.RenderDuration > 0f)
				{
					Debug.DrawLine(this.SimLastPos, this.SimCurrentPos, Color.green, this.RenderDuration);
				}
				float num;
				Vector3 vector2 = vector.NormalizeReturnMagnitude(out num);
				Ray ray;
				ray..ctor(this.SimCurrentPos, vector2);
				if (Voxel.Raycast(world, ray, num, 1082195968, 8191, this.Projectile.ProjectileRadius))
				{
					if (Voxel.phyxRaycastHit.distance == 0f)
					{
						if (this.RenderDuration > 0f)
						{
							Utils.DrawBounds(new Bounds(Voxel.phyxRaycastHit.point, Vector3.one * this.Projectile.ProjectileRadius), Color.red, this.RenderDuration);
							break;
						}
						break;
					}
					else
					{
						this.SimCurrentPos += ray.GetPoint(Voxel.phyxRaycastHit.distance);
						this.SimVelocity = Vector3.Reflect(this.SimVelocity, Voxel.phyxRaycastHit.normal);
						this.SimVelocity *= 0.25f;
						float num2 = num - Voxel.phyxRaycastHit.distance;
						if (num2 > 0f)
						{
							this.SimCurrentPos = Voxel.phyxRaycastHit.point + Vector3.Reflect(vector2, Voxel.phyxRaycastHit.normal) * num2;
						}
						if (this.RenderDuration > 0f)
						{
							Debug.DrawRay(Voxel.phyxRaycastHit.point, Voxel.phyxRaycastHit.normal * 0.2f, Color.yellow, this.RenderDuration);
						}
						if (Vector3.Dot(Voxel.phyxRaycastHit.normal, Vector3.up) > 0.707f)
						{
							break;
						}
					}
				}
				this.SimLastPos = this.SimCurrentPos;
				this.SimTime += 0.05f;
				maxIterations--;
			}
		}
		if (MathUtils.DistanceToSegment(this.SimCurrentPos, this.TargetPosition, this.Target.getChestPosition()) < this.Projectile.ProjectileTargetRadius)
		{
			this.TrajectoryState = TrajectorySimulation.State.DoneWithSolution;
			if (this.RenderDuration > 0f)
			{
				Utils.DrawBounds(new Bounds(this.SimCurrentPos, new Vector3(1f, 0f, 1f) * this.Projectile.ProjectileRadius), Color.blue, this.RenderDuration);
			}
			this.OnSimulationComplete(this);
			return;
		}
		this.SimTrajectoryIndex++;
		if (this.SimTrajectoryIndex < this.SimTrajectoryCount)
		{
			this.SimTime = 0f;
			this.SimVelocity = this.SimTrajectories[this.SimTrajectoryIndex].LaunchVelocity;
			this.SimCurrentPos = (this.SimLastPos = this.Owner.getHeadPosition());
			this.TrajectoryState = TrajectorySimulation.State.Simulating;
			return;
		}
		this.TrajectoryState = TrajectorySimulation.State.DoneWithoutSolution;
		this.OnSimulationComplete(this);
	}

	public static void Cleanup()
	{
		TrajectorySimulation.Trajectories.Clear();
	}

	public static void Queue(TrajectorySimulation trajectory)
	{
		if (!TrajectorySimulation.Trajectories.Contains(trajectory))
		{
			TrajectorySimulation.Trajectories.Enqueue(trajectory);
		}
		trajectory.TrajectoryState = TrajectorySimulation.State.ReadyToSimulate;
	}

	public static void UpdateSimulationQueue()
	{
		TrajectorySimulation trajectorySimulation;
		if (TrajectorySimulation.Trajectories.TryPeek(out trajectorySimulation))
		{
			trajectorySimulation.TickSimulation(20);
			if (trajectorySimulation.TrajectoryState != TrajectorySimulation.State.Simulating)
			{
				TrajectorySimulation.Trajectories.Dequeue();
			}
		}
	}

	public TrajectorySimulation.State TrajectoryState;

	public readonly EntityAlive Owner;

	public readonly EntityAlive Target;

	public readonly float MaxSimTime;

	public readonly TrajectorySimulation.ProjectileInfo Projectile;

	public readonly float RenderDuration;

	[PublicizedFrom(EAccessModifier.Private)]
	public Action<TrajectorySimulation> OnSimulationComplete;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 LaunchPosition;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 TargetPosition;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Trajectory.AimTrajectory[] SimTrajectories = new Trajectory.AimTrajectory[2];

	[PublicizedFrom(EAccessModifier.Private)]
	public int SimTrajectoryIndex;

	[PublicizedFrom(EAccessModifier.Private)]
	public int SimTrajectoryCount;

	[PublicizedFrom(EAccessModifier.Private)]
	public float SimTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 SimVelocity;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 SimLastPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 SimCurrentPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public Plane CullPlane;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cMaxOwnerDelta = 16f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cMaxTargetDelta = 16f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cSimInterval = 0.05f;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Queue<TrajectorySimulation> Trajectories = new Queue<TrajectorySimulation>();

	public enum State
	{
		Init,
		Invalid,
		ReadyToSimulate,
		Simulating,
		DoneWithSolution,
		DoneWithoutSolution
	}

	public struct ProjectileInfo
	{
		public float ProjectileSpeed;

		public float ProjectileGravity;

		public float ProjectileRadius;

		public float ProjectileTargetRadius;
	}
}
