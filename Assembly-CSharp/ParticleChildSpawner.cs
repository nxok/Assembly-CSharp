using System;
using UnityEngine;

public class ParticleChildSpawner : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Private)]
	public void Start()
	{
		EntityAlive componentInParent = base.GetComponentInParent<EntityAlive>();
		if (!componentInParent)
		{
			Log.Warning("ParticleChildSpawner !entity");
			return;
		}
		if (this.HasAllTags)
		{
			if (!componentInParent.HasAllTags(FastTags<TagGroup.Global>.Parse(this.tags)))
			{
				return;
			}
		}
		else if (!componentInParent.HasAnyTags(FastTags<TagGroup.Global>.Parse(this.tags)))
		{
			return;
		}
		if (!string.IsNullOrEmpty(this.notTags) && componentInParent.HasAnyTags(FastTags<TagGroup.Global>.Parse(this.notTags)))
		{
			return;
		}
		for (int i = 0; i < this.particles.Length; i++)
		{
			float num = EntityClass.list[componentInParent.entityClass].MassKg * 2.2f;
			if (num >= this.particles[i].mass.x && num <= this.particles[i].mass.y)
			{
				Transform transform = componentInParent.emodel.GetModelTransform();
				transform = transform.FindInChildren(this.particles[i].boneName);
				if (transform)
				{
					GameObject asset = LoadManager.LoadAssetFromAddressables<GameObject>("ParticleEffects/" + this.particles[i].particleName + ".prefab", null, null, false, true, false).Asset;
					if (!asset)
					{
						Log.Warning("ParticleChildSpawner {0}, no asset {1}", new object[]
						{
							base.name,
							this.particles[i].particleName
						});
					}
					else
					{
						GameObject gameObject = Object.Instantiate<GameObject>(asset);
						gameObject.transform.SetParent(transform, false);
						this.particles[i].spawnedObj = gameObject;
					}
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDestroy()
	{
		for (int i = 0; i < this.particles.Length; i++)
		{
			GameObject spawnedObj = this.particles[i].spawnedObj;
			if (spawnedObj)
			{
				Object.Destroy(spawnedObj);
			}
		}
	}

	[Tooltip("One or more tags separated by commas.\nMatches if any 'Tag' is present")]
	public string tags;

	[Tooltip("Check to require all 'Tags' to be present to match")]
	public bool HasAllTags;

	[Tooltip("One or more tags separated by commas.\nFails to match if any 'Not Tag' is present")]
	public string notTags;

	public ParticleChildSpawner.Data[] particles;

	[Serializable]
	public struct Data
	{
		public string particleName;

		public string boneName;

		public Vector2 mass;

		public GameObject spawnedObj;
	}
}
