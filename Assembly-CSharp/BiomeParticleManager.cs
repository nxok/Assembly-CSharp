using System;
using System.Collections.Generic;
using UnityEngine;

public class BiomeParticleManager
{
	public static void RegisterEffect(string biomeName, string prefabName, float chunkMargin)
	{
		if (GameManager.IsDedicatedServer || BiomeParticleManager.RegistrationCompleted)
		{
			return;
		}
		if (BiomeParticleManager.effects == null)
		{
			BiomeParticleManager.effects = new Dictionary<string, DictionaryList<string, BiomeParticleManager.ParticleEffectData>>();
		}
		BiomeParticleManager.ParticleEffectData particleEffectData;
		particleEffectData.biomeName = biomeName;
		particleEffectData.prefabName = prefabName;
		particleEffectData.chunkMargin = chunkMargin + 1f;
		DataLoader.PreloadBundle(prefabName);
		DictionaryList<string, BiomeParticleManager.ParticleEffectData> dictionaryList;
		if (!BiomeParticleManager.effects.TryGetValue(biomeName, out dictionaryList))
		{
			dictionaryList = new DictionaryList<string, BiomeParticleManager.ParticleEffectData>();
			BiomeParticleManager.effects.Add(biomeName, dictionaryList);
		}
		dictionaryList.Add(prefabName, particleEffectData);
	}

	public static List<GameObject> SpawnParticles(Chunk chunk, Transform _parent)
	{
		if (GameManager.IsDedicatedServer)
		{
			return null;
		}
		Vector3i worldPos = chunk.GetWorldPos();
		BiomeDefinition biome = GameManager.Instance.World.GetBiome(worldPos.x, worldPos.z);
		if (biome == null)
		{
			return null;
		}
		string sBiomeName = biome.m_sBiomeName;
		DictionaryList<string, BiomeParticleManager.ParticleEffectData> dictionaryList;
		if (!BiomeParticleManager.effects.TryGetValue(sBiomeName, out dictionaryList))
		{
			return null;
		}
		if (dictionaryList.list.Count == 0)
		{
			return null;
		}
		float @float = GamePrefs.GetFloat(EnumGamePrefs.OptionsGfxWaterPtlLimiter);
		if (@float <= 0.04f)
		{
			return null;
		}
		int height = (int)GameManager.Instance.World.GetHeight(worldPos.x, worldPos.z);
		List<GameObject> list = new List<GameObject>();
		for (int i = 0; i < dictionaryList.list.Count; i++)
		{
			BiomeParticleManager.ParticleEffectData particleEffectData = dictionaryList.list[i];
			if ((float)chunk.X % particleEffectData.chunkMargin == 0f && (float)chunk.Z % particleEffectData.chunkMargin == 0f)
			{
				GameObject gameObject = DataLoader.LoadAsset<GameObject>(particleEffectData.prefabName, false);
				if (gameObject)
				{
					GameObject gameObject2 = Object.Instantiate<GameObject>(gameObject);
					Vector3 vector;
					vector.x = (float)worldPos.x;
					vector.y = (float)height;
					vector.z = (float)worldPos.z;
					gameObject2.name = gameObject2.name + "_ (" + vector.ToCultureInvariantString() + ")";
					Transform transform = gameObject2.transform;
					transform.SetParent(_parent, false);
					transform.position = vector - Origin.position;
					ParticleSystem component = gameObject2.GetComponent<ParticleSystem>();
					if (component)
					{
						ParticleSystem.MainModule main = component.main;
						main.maxParticles = (int)((float)main.maxParticles * @float);
					}
					list.Add(gameObject2);
				}
			}
		}
		return list;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static Dictionary<string, DictionaryList<string, BiomeParticleManager.ParticleEffectData>> effects;

	public static bool RegistrationCompleted;

	public struct ParticleEffectData
	{
		public string biomeName;

		public string prefabName;

		public float chunkMargin;
	}
}
