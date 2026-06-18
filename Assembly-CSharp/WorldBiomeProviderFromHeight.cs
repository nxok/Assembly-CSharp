using System;
using System.Collections;

public class WorldBiomeProviderFromHeight : IBiomeProvider
{
	public WorldBiomeProviderFromHeight(string _levelName, ITerrainGenerator _terrainGenerator, int _waterLevel, BiomeDefinition _waterBiome, BiomeDefinition _otherBiome)
	{
		this.worldName = _levelName;
		this.waterBiome = _waterBiome;
		this.otherBiome = _otherBiome;
		this.waterLevel = _waterLevel;
		this.terrainGenerator = _terrainGenerator;
	}

	public string GetWorldName()
	{
		return this.worldName;
	}

	public void SetBiomeSizeFactor(float _factor)
	{
	}

	public IEnumerator InitData()
	{
		yield break;
	}

	public void Init(int _seed, string _worldName, WorldBiomes _biomes, string _params1, string _params2)
	{
	}

	public int GetSubBiomeIdxAt(BiomeDefinition bd, int _x, int _y, int _z)
	{
		return -1;
	}

	public BiomeDefinition GetBiomeAt(int x, int z, out float _intensity)
	{
		_intensity = 1f;
		return this.GetBiomeAt(x, z);
	}

	public BiomeDefinition GetBiomeAt(int x, int z)
	{
		if (this.terrainGenerator.GetTerrainHeightAt(x, z) <= (float)this.waterLevel)
		{
			return this.waterBiome;
		}
		return this.otherBiome;
	}

	public Vector2i GetSize()
	{
		return new Vector2i(1, 1);
	}

	public BiomeIntensity GetBiomeIntensityAt(int _x, int _z)
	{
		return BiomeIntensity.Default;
	}

	public float GetHumidityAt(int x, int z)
	{
		return 0f;
	}

	public float GetTemperatureAt(int x, int z)
	{
		return 0f;
	}

	public float GetRadiationAt(int x, int z)
	{
		return 0f;
	}

	public BlockValue GetTopmostBlockValue(int xWorld, int zWorld)
	{
		return BlockValue.Air;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string worldName;

	[PublicizedFrom(EAccessModifier.Private)]
	public BiomeDefinition waterBiome;

	[PublicizedFrom(EAccessModifier.Private)]
	public BiomeDefinition otherBiome;

	[PublicizedFrom(EAccessModifier.Private)]
	public ITerrainGenerator terrainGenerator;

	[PublicizedFrom(EAccessModifier.Private)]
	public int waterLevel;
}
