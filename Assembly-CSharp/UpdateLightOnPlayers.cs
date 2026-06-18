using System;
using UnityEngine;

public class UpdateLightOnPlayers : UpdateLight
{
	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		if (GameManager.Instance == null || GameManager.Instance.World == null || !GameManager.Instance.gameStateManager.IsGameStarted())
		{
			return;
		}
		if (GameManager.IsDedicatedServer)
		{
			return;
		}
		if (!this.entity)
		{
			Transform transform = RootTransformRefEntity.FindEntityUpwards(base.transform);
			if (transform)
			{
				this.entity = transform.GetComponent<Entity>();
			}
		}
		else if (this.entity.emodel.IsFPV != this.lastFPV)
		{
			this.lastFPV = this.entity.emodel.IsFPV;
			this.forceUpdateFrame = Time.frameCount + 5;
			this.isForceUpdate = true;
		}
		if (this.isForceUpdate)
		{
			this.appliedLit = -1f;
			this.updateDelay = 0f;
			if (Time.frameCount >= this.forceUpdateFrame)
			{
				this.isForceUpdate = false;
			}
		}
		this.updateDelay -= Time.deltaTime;
		if (this.updateDelay > 0f)
		{
			return;
		}
		this.updateDelay = 0.05f;
		base.UpdateLighting(0.15f);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnApplicationFocus(bool focusStatus)
	{
		this.forceUpdateFrame = Time.frameCount + 3;
		this.isForceUpdate = true;
	}

	public void ForceUpdate()
	{
		this.updateDelay = 0f;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float cUpdateTime = 0.05f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool isForceUpdate;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int forceUpdateFrame;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool lastFPV;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float updateDelay;
}
