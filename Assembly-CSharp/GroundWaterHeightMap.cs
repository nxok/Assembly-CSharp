using System;

public class GroundWaterHeightMap
{
	public GroundWaterHeightMap(World _world)
	{
		this.world = _world;
	}

	public bool TryInit()
	{
		if (this.poiColors != null && this.biomes != null)
		{
			return true;
		}
		ChunkProviderGenerateWorldFromRaw chunkProviderGenerateWorldFromRaw = this.world.ChunkCache.ChunkProvider as ChunkProviderGenerateWorldFromRaw;
		if (chunkProviderGenerateWorldFromRaw == null)
		{
			return false;
		}
		WorldDecoratorPOIFromImage poiFromImage = chunkProviderGenerateWorldFromRaw.poiFromImage;
		if (poiFromImage == null)
		{
			return false;
		}
		this.poiColors = poiFromImage.m_Poi;
		this.biomes = this.world.Biomes;
		return this.poiColors != null && this.biomes != null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public PoiMapElement GetPoiMapElement(int _worldX, int _worldZ)
	{
		if (!this.poiColors.Contains(_worldX, _worldZ))
		{
			return null;
		}
		byte data = this.poiColors.GetData(_worldX, _worldZ);
		if (data == 0)
		{
			return null;
		}
		return this.biomes.getPoiForColor((uint)data);
	}

	public bool TryGetWaterHeightAt(int _worldX, int _worldZ, out int _height)
	{
		PoiMapElement poiMapElement = this.GetPoiMapElement(_worldX, _worldZ);
		if (poiMapElement == null)
		{
			_height = 0;
			return false;
		}
		if (poiMapElement.m_BlockValue.type != 240)
		{
			_height = 0;
			return false;
		}
		_height = poiMapElement.m_YPosFill;
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public World world;

	[PublicizedFrom(EAccessModifier.Private)]
	public WorldGridCompressedData<byte> poiColors;

	[PublicizedFrom(EAccessModifier.Private)]
	public WorldBiomes biomes;
}
