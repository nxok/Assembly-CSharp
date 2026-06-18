using System;
using System.Collections.Generic;
using UnityEngine;

public class BiomeAtmosphereEffects
{
	public void Init(World _world)
	{
		this.world = _world;
		this.worldColorSpectrums = new AtmosphereEffect[255];
		this.worldColorSpectrums[0] = AtmosphereEffect.Load("default", null);
		foreach (KeyValuePair<uint, BiomeDefinition> keyValuePair in _world.Biomes.GetBiomeMap())
		{
			this.worldColorSpectrums[(int)keyValuePair.Value.m_Id] = AtmosphereEffect.Load(keyValuePair.Value.m_SpectrumName, this.worldColorSpectrums[0]);
		}
		this.ForceDefault = false;
	}

	public void Reload()
	{
		this.Init(this.world);
		this.Update();
	}

	public virtual void Update()
	{
		EntityPlayerLocal primaryPlayer = this.world.GetPrimaryPlayer();
		if (primaryPlayer == null)
		{
			return;
		}
		if (this.ForceDefault)
		{
			this.currentBiomeIntensity = BiomeIntensity.Default;
			return;
		}
		Vector3i blockPosition = primaryPlayer.GetBlockPosition();
		BiomeIntensity biomeIntensity;
		if (!blockPosition.Equals(this.playerPosition) && this.world.GetBiomeIntensity(blockPosition, out biomeIntensity))
		{
			this.playerPosition = blockPosition;
			if (!this.currentBiomeIntensity.Equals(biomeIntensity))
			{
				WorldBiomes biomes = GameManager.Instance.World.Biomes;
				BiomeDefinition biomeDefinition = biomes.GetBiome(biomeIntensity.biomeId0);
				if (biomeDefinition != null)
				{
					biomeDefinition.currentPlayerIntensity = biomeIntensity.intensity0;
				}
				this.nearBiomes[0] = biomeDefinition;
				biomeDefinition = biomes.GetBiome(biomeIntensity.biomeId1);
				if (biomeDefinition != null)
				{
					biomeDefinition.currentPlayerIntensity = biomeIntensity.intensity1;
				}
				this.nearBiomes[1] = biomeDefinition;
				biomeDefinition = biomes.GetBiome(biomeIntensity.biomeId2);
				if (biomeDefinition != null)
				{
					biomeDefinition.currentPlayerIntensity = biomeIntensity.intensity1;
				}
				this.nearBiomes[2] = biomeDefinition;
				biomeDefinition = biomes.GetBiome(biomeIntensity.biomeId3);
				if (biomeDefinition != null)
				{
					biomeDefinition.currentPlayerIntensity = biomeIntensity.intensity2;
				}
				this.nearBiomes[3] = biomeDefinition;
			}
			this.currentBiomeIntensity = biomeIntensity;
		}
	}

	public virtual Color GetSkyColorSpectrum(float _v)
	{
		return this.getColorFromSpectrum(this.currentBiomeIntensity, _v, AtmosphereEffect.ESpecIdx.Sky);
	}

	public virtual Color GetAmbientColorSpectrum(float _v)
	{
		return this.getColorFromSpectrum(this.currentBiomeIntensity, _v, AtmosphereEffect.ESpecIdx.Ambient);
	}

	public virtual Color GetSunColorSpectrum(float _v)
	{
		return this.getColorFromSpectrum(this.currentBiomeIntensity, _v, AtmosphereEffect.ESpecIdx.Sun);
	}

	public virtual Color GetMoonColorSpectrum(float _v)
	{
		return this.getColorFromSpectrum(this.currentBiomeIntensity, _v, AtmosphereEffect.ESpecIdx.Moon);
	}

	public virtual Color GetFogColorSpectrum(float _v)
	{
		return this.getColorFromSpectrum(this.currentBiomeIntensity, _v, AtmosphereEffect.ESpecIdx.Fog);
	}

	public virtual Color GetFogFadeColorSpectrum(float _v)
	{
		return this.getColorFromSpectrum(this.currentBiomeIntensity, _v, AtmosphereEffect.ESpecIdx.FogFade);
	}

	public virtual Color GetCloudsColor(float _v)
	{
		return Color.white;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Color getColorFromSpectrum(BiomeIntensity _bi, float _v, AtmosphereEffect.ESpecIdx _spectrumIdx)
	{
		float intensity = _bi.intensity0;
		float intensity2 = _bi.intensity1;
		float intensity3 = _bi.intensity2;
		float num = _bi.intensity3;
		num = 0f;
		return (this.worldColorSpectrums[(int)_bi.biomeId0].spectrums[(int)_spectrumIdx].GetValue(_v) * intensity + this.worldColorSpectrums[(int)_bi.biomeId1].spectrums[(int)_spectrumIdx].GetValue(_v) * intensity2 + this.worldColorSpectrums[(int)_bi.biomeId2].spectrums[(int)_spectrumIdx].GetValue(_v) * intensity3 + this.worldColorSpectrums[(int)_bi.biomeId3].spectrums[(int)_spectrumIdx].GetValue(_v) * num) / (intensity + intensity2 + intensity3 + num);
	}

	public BiomeDefinition[] nearBiomes = new BiomeDefinition[4];

	[PublicizedFrom(EAccessModifier.Private)]
	public BiomeIntensity currentBiomeIntensity = BiomeIntensity.Default;

	[PublicizedFrom(EAccessModifier.Private)]
	public World world;

	[PublicizedFrom(EAccessModifier.Private)]
	public AtmosphereEffect[] worldColorSpectrums;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i playerPosition;

	public bool ForceDefault;
}
