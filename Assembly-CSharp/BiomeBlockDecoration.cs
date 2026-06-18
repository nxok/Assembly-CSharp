using System;

public class BiomeBlockDecoration
{
	public BiomeBlockDecoration(string _name, float _prob, float _clusprob, bool _instantiateReferences, int _randomRotateMax, int _checkResource = 2147483647)
	{
		string[] array = _name.Split(',', StringSplitOptions.None);
		if (_instantiateReferences)
		{
			this.blockValues = new BlockValue[array.Length];
			for (int i = 0; i < array.Length; i++)
			{
				BlockValue blockValueForName = WorldBiomes.GetBlockValueForName(array[i]);
				if (_randomRotateMax > 3 && !blockValueForName.isair)
				{
					Block block = blockValueForName.Block;
					if (block.isMultiBlock && (block.multiBlockPos.dim.x > 1 || block.multiBlockPos.dim.z > 1))
					{
						Log.Error("Parsing biomes. Block with name '" + array[i] + "' supports only rotations 0-3, setting it to 3");
						_randomRotateMax = 3;
					}
				}
				this.blockValues[i] = blockValueForName;
			}
		}
		this.prob = _prob;
		this.clusterProb = _clusprob;
		this.randomRotateMax = _randomRotateMax;
		this.checkResourceOffsetY = _checkResource;
	}

	public static byte GetRandomRotation(float _rnd, int _randomRotateMax)
	{
		byte b = (byte)(_rnd * (float)_randomRotateMax + 0.5f);
		if (b >= 4 && b <= 7)
		{
			b = b - 4 + 24;
		}
		return b;
	}

	public BlockValue[] blockValues;

	public float prob;

	public float clusterProb;

	public int randomRotateMax;

	public int checkResourceOffsetY;
}
