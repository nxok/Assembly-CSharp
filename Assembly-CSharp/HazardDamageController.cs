using System;
using System.Collections.Generic;
using UnityEngine;

public class HazardDamageController : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		if (!this.IsActive)
		{
			if (this.CollidersThisFrame != null && this.CollidersThisFrame.Count > 0)
			{
				this.CollidersThisFrame.Clear();
			}
			return;
		}
		if (this.CollidersThisFrame == null || this.CollidersThisFrame.Count == 0)
		{
			return;
		}
		for (int i = 0; i < this.CollidersThisFrame.Count; i++)
		{
			this.touched(this.CollidersThisFrame[i]);
		}
		this.CollidersThisFrame.Clear();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnTriggerEnter(Collider other)
	{
		if (!this.IsActive)
		{
			return;
		}
		if (this.CollidersThisFrame == null)
		{
			this.CollidersThisFrame = new List<Collider>();
		}
		if (!this.CollidersThisFrame.Contains(other))
		{
			this.CollidersThisFrame.Add(other);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnTriggerStay(Collider other)
	{
		if (!this.IsActive)
		{
			return;
		}
		if (this.CollidersThisFrame == null)
		{
			this.CollidersThisFrame = new List<Collider>();
		}
		if (!this.CollidersThisFrame.Contains(other))
		{
			this.CollidersThisFrame.Add(other);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnTriggerExit(Collider other)
	{
		if (!this.IsActive)
		{
			return;
		}
		if (this.CollidersThisFrame == null)
		{
			this.CollidersThisFrame = new List<Collider>();
		}
		if (!this.CollidersThisFrame.Contains(other))
		{
			this.CollidersThisFrame.Add(other);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void touched(Collider collider)
	{
		if (!this.IsActive || collider == null)
		{
			return;
		}
		Transform transform = collider.transform;
		if (transform != null)
		{
			EntityAlive entityAlive = transform.GetComponent<EntityAlive>();
			if (entityAlive == null)
			{
				entityAlive = transform.GetComponentInParent<EntityAlive>();
			}
			if (entityAlive == null && transform.parent != null)
			{
				entityAlive = transform.parent.GetComponentInChildren<EntityAlive>();
			}
			if (entityAlive == null)
			{
				entityAlive = transform.GetComponentInChildren<EntityAlive>();
			}
			if (entityAlive != null && entityAlive.IsAlive() && this.buffActions != null)
			{
				for (int i = 0; i < this.buffActions.Count; i++)
				{
					if (entityAlive.emodel != null && entityAlive.emodel.transform != null && !entityAlive.Buffs.HasBuff(this.buffActions[i]))
					{
						entityAlive.Buffs.AddBuff(this.buffActions[i], -1, true, false, -1f);
					}
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public List<Collider> CollidersThisFrame;

	public bool IsActive;

	public List<string> buffActions;
}
