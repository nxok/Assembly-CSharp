using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public struct BiomeImageLoader
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static uint BiomeValueFromARGB32(BiomeImageLoader.BiomePixel _argb)
	{
		uint num = (uint)((uint)_argb.c2 << 16);
		uint num2 = (uint)((uint)_argb.c3 << 8);
		uint c = (uint)_argb.c4;
		return num | num2 | c;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static uint BiomeValueFromRGBA32(BiomeImageLoader.BiomePixel _rgba)
	{
		uint num = (uint)((uint)_rgba.c1 << 16);
		uint num2 = (uint)((uint)_rgba.c2 << 8);
		uint c = (uint)_rgba.c3;
		return num | num2 | c;
	}

	public BiomeImageLoader(Texture2D _biomesTex, Dictionary<uint, BiomeDefinition> _biomeDefinitions)
	{
		if (_biomesTex.format == 5)
		{
			this.toBiomeValue = BiomeImageLoader.fromARGB32;
		}
		else
		{
			if (_biomesTex.format != 4)
			{
				throw new Exception(string.Format("Unsupported biome texture format: {0}", _biomesTex.format));
			}
			this.toBiomeValue = BiomeImageLoader.fromRGBA32;
		}
		this.biomesTex = _biomesTex;
		this.biomeDefinitions = _biomeDefinitions;
		int num = 16;
		this.biomeMap = new GridCompressedData<byte>(this.biomesTex.width, this.biomesTex.height, num, num);
		this.isError = false;
		this.lastBiomeValue = 0U;
		this.biomeId = byte.MaxValue;
	}

	public IEnumerator Load()
	{
		this.isError = false;
		this.lastBiomeValue = 0U;
		this.biomeId = byte.MaxValue;
		MicroStopwatch msw = new MicroStopwatch(true);
		NativeArray<BiomeImageLoader.BiomePixel> biomePixs = this.biomesTex.GetPixelData<BiomeImageLoader.BiomePixel>(0);
		int blockSize = this.biomeMap.cellSizeX;
		int blockIndex = 0;
		int num5;
		for (int blockY = 0; blockY < this.biomeMap.heightCells; blockY = num5 + 1)
		{
			for (int blockX = 0; blockX < this.biomeMap.widthCells; blockX = num5 + 1)
			{
				int num = blockY * this.biomeMap.cellSizeY;
				int num2 = blockX * this.biomeMap.cellSizeX;
				int num3 = num2 + num * this.biomesTex.width;
				BiomeImageLoader.BiomePixel biomePixel = biomePixs[num2 + num * this.biomesTex.width];
				uint num4 = this.toBiomeValue(biomePixel);
				this.biomeMap.SetSameValue(blockIndex, this.GetBiomeId(num4));
				for (int i = 0; i < blockSize; i++)
				{
					for (int j = 0; j < blockSize; j++)
					{
						num3 = num2 + j + (num + i) * this.biomesTex.width;
						num4 = this.toBiomeValue(biomePixs[num3]);
						this.biomeMap.SetValue(blockIndex, j, i, this.GetBiomeId(num4));
					}
				}
				if (num3 % 8192 == 0 && msw.ElapsedMilliseconds > (long)Constants.cMaxLoadTimePerFrameMillis)
				{
					yield return null;
					msw.ResetAndRestart();
				}
				num5 = blockIndex;
				blockIndex = num5 + 1;
				num5 = blockX;
			}
			num5 = blockY;
		}
		biomePixs.Dispose();
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public byte GetBiomeId(uint iBiomeValue)
	{
		if (this.lastBiomeValue != iBiomeValue)
		{
			this.lastBiomeValue = iBiomeValue;
			BiomeDefinition biomeDefinition;
			if (this.biomeDefinitions.TryGetValue(iBiomeValue, out biomeDefinition))
			{
				this.biomeId = biomeDefinition.m_Id;
			}
		}
		return this.biomeId;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Color32 BiomeIdToColor32(uint iBiomeValue)
	{
		byte b = (byte)((iBiomeValue >> 16) & 255U);
		byte b2 = (byte)((iBiomeValue >> 8) & 255U);
		byte b3 = (byte)(iBiomeValue & 255U);
		return new Color32(b, b2, b3, 0);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static BiomeImageLoader.PixelToBiomeValue fromARGB32 = new BiomeImageLoader.PixelToBiomeValue(BiomeImageLoader.BiomeValueFromARGB32);

	[PublicizedFrom(EAccessModifier.Private)]
	public static BiomeImageLoader.PixelToBiomeValue fromRGBA32 = new BiomeImageLoader.PixelToBiomeValue(BiomeImageLoader.BiomeValueFromRGBA32);

	[PublicizedFrom(EAccessModifier.Private)]
	public BiomeImageLoader.PixelToBiomeValue toBiomeValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public Texture2D biomesTex;

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<uint, BiomeDefinition> biomeDefinitions;

	public GridCompressedData<byte> biomeMap;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isError;

	[PublicizedFrom(EAccessModifier.Private)]
	public uint lastBiomeValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public byte biomeId;

	[PublicizedFrom(EAccessModifier.Private)]
	public struct BiomePixel
	{
		public byte c1;

		public byte c2;

		public byte c3;

		public byte c4;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public delegate uint PixelToBiomeValue(BiomeImageLoader.BiomePixel pix);
}
