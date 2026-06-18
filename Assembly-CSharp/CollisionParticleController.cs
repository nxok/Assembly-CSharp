using System;
using UnityEngine;

public class CollisionParticleController
{
	public void Init(int _entityId, string _colliderSurfaceCategory, string _collisionSurfaceCategory, int _layerMask)
	{
		this.entityId = _entityId;
		this.particleEffectName = string.Format("impact_{0}_on_{1}", _colliderSurfaceCategory, _collisionSurfaceCategory);
		this.soundName = string.Format("{0}hit{1}", _colliderSurfaceCategory, _collisionSurfaceCategory);
		this.layerMask = _layerMask;
		this.Reset();
	}

	public void CheckCollision(Vector3 worldPos, Vector3 direction, float distance, int originEntityId = -1)
	{
		if (this.hasHit)
		{
			return;
		}
		RaycastHit raycastHit;
		if (Physics.Raycast(new Ray(worldPos - Origin.position, direction), ref raycastHit, distance, this.layerMask))
		{
			Vector3 vector = raycastHit.point + Origin.position;
			float lightBrightness = GameManager.Instance.World.GetLightBrightness(World.worldToBlockPos(vector));
			GameManager.Instance.SpawnParticleEffectServer(new ParticleEffect(this.particleEffectName, vector, Quaternion.FromToRotation(Vector3.up, raycastHit.normal), lightBrightness, Color.white, this.soundName, null, 1f, ""), (originEntityId == -1) ? this.entityId : originEntityId, false, false);
			this.hasHit = true;
		}
	}

	public void Reset()
	{
		this.hasHit = false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool hasHit;

	[PublicizedFrom(EAccessModifier.Private)]
	public int entityId;

	[PublicizedFrom(EAccessModifier.Private)]
	public string particleEffectName;

	[PublicizedFrom(EAccessModifier.Private)]
	public string soundName;

	[PublicizedFrom(EAccessModifier.Private)]
	public int layerMask;
}
