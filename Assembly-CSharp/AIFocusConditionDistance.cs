using System;
using UnityEngine;

public readonly struct AIFocusConditionDistance
{
	public AIFocusConditionDistance(float distance, EntityAlive entityTransform)
	{
		this.ConditionalDistanceSq = distance * distance;
		this.EntityTransform = entityTransform;
		this.DistanceTransform = null;
		this.DistancePosition = Vector3.zero;
		this.HasDistancePosition = false;
	}

	public AIFocusConditionDistance(float distance, Transform distanceTransform)
	{
		this.ConditionalDistanceSq = distance * distance;
		this.EntityTransform = null;
		this.DistanceTransform = distanceTransform;
		this.DistancePosition = Vector3.zero;
		this.HasDistancePosition = false;
	}

	public AIFocusConditionDistance(float distance, Vector3 distancePosition)
	{
		this.ConditionalDistanceSq = distance * distance;
		this.EntityTransform = null;
		this.DistanceTransform = null;
		this.DistancePosition = distancePosition;
		this.HasDistancePosition = true;
	}

	public bool IsFocusDisabled(Entity theEntity)
	{
		if (this.ConditionalDistanceSq > 0f)
		{
			Vector3 vector = Vector3.zero;
			if (this.HasDistancePosition)
			{
				vector = this.DistancePosition;
			}
			else if (this.EntityTransform)
			{
				vector = this.EntityTransform.position;
			}
			else
			{
				if (!this.DistanceTransform)
				{
					return false;
				}
				vector = this.DistanceTransform.position;
			}
			Vector3 position = theEntity.position;
			if (Vector3.SqrMagnitude(vector - position) > this.ConditionalDistanceSq)
			{
				return true;
			}
		}
		return false;
	}

	public readonly float ConditionalDistanceSq;

	public readonly EntityAlive EntityTransform;

	public readonly Transform DistanceTransform;

	public readonly Vector3 DistancePosition;

	public readonly bool HasDistancePosition;
}
