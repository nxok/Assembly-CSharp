using System;
using UnityEngine;

public class TripWireController : MonoBehaviour
{
	public void Init(DynamicProperties _properties)
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnTriggerEnter(Collider other)
	{
		this.checkIfTriggered(other);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnTriggerStay(Collider other)
	{
		this.checkIfTriggered(other);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void checkIfTriggered(Collider other)
	{
		if (this.TileEntityParent == null || this.WireNode == null)
		{
			return;
		}
		EntityAlive entityAlive = other.transform.GetComponent<EntityAlive>();
		if (entityAlive == null)
		{
			entityAlive = other.transform.GetComponentInParent<EntityAlive>();
		}
		if (entityAlive == null)
		{
			entityAlive = other.transform.parent.GetComponentInChildren<EntityAlive>();
		}
		if (entityAlive == null)
		{
			entityAlive = other.transform.GetComponentInChildren<EntityAlive>();
		}
		if (entityAlive != null && entityAlive as EntityVehicle != null && !(entityAlive as EntityVehicle).HasDriver)
		{
			return;
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && this.TileEntityParent.IsPowered)
		{
			this.TileEntityChild.IsTriggered = true;
		}
	}

	public TileEntityPoweredTrigger TileEntityParent;

	public TileEntityPoweredTrigger TileEntityChild;

	public IWireNode WireNode;
}
