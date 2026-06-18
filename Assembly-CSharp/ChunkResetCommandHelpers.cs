using System;
using System.Collections.Generic;

public static class ChunkResetCommandHelpers
{
	public static bool TryParseProtectionMode(List<string> _params, out ChunkProtectionLevel _protection, out int _pmode)
	{
		_protection = ChunkProtectionLevel.All;
		_pmode = 0;
		int num = ChunkResetCommandHelpers.FindFlag(_params, new string[] { "-p", "-pmode" });
		if (num < 0)
		{
			return true;
		}
		int num2;
		if (num + 1 >= _params.Count || !int.TryParse(_params[num + 1], out num2))
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Invalid argument: '" + _params[num] + "' requires an integer value.");
			return false;
		}
		switch (num2)
		{
		case 0:
			_protection = ChunkProtectionLevel.All;
			break;
		case 1:
			_protection = ~ChunkProtectionLevel.CurrentlySynced;
			break;
		case 2:
			_protection = ChunkProtectionLevel.None;
			break;
		case 3:
			_protection = ChunkProtectionLevel.CurrentlySynced;
			break;
		default:
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("Invalid argument: '{0}' is not a supported protection mode value.", num2));
			return false;
		}
		_pmode = num2;
		return true;
	}

	public static bool TryParseGroupingMode(List<string> _params, out EnumResetUnprotectedChunksGroupingMode _grouping)
	{
		_grouping = EnumResetUnprotectedChunksGroupingMode.GroupedPOIs;
		int num = ChunkResetCommandHelpers.FindFlag(_params, new string[] { "-g", "-gmode" });
		if (num < 0)
		{
			return true;
		}
		int num2;
		if (num + 1 >= _params.Count || !int.TryParse(_params[num + 1], out num2) || num2 < 1 || num2 > 3)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Invalid argument: '" + _params[num] + "' requires an integer value of 1, 2, or 3.");
			return false;
		}
		_grouping = (EnumResetUnprotectedChunksGroupingMode)num2;
		return true;
	}

	public static bool TryParseRegion(List<string> _params, out int? _regionX, out int? _regionZ)
	{
		_regionX = null;
		_regionZ = null;
		int num = ChunkResetCommandHelpers.FindFlag(_params, new string[] { "-r", "-region" });
		if (num < 0)
		{
			return true;
		}
		int num2;
		int num3;
		if (num + 2 >= _params.Count || !int.TryParse(_params[num + 1], out num2) || !int.TryParse(_params[num + 2], out num3))
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Invalid argument: '" + _params[num] + "' requires two integer coordinates (x, z).");
			return false;
		}
		_regionX = new int?(num2);
		_regionZ = new int?(num3);
		return true;
	}

	public static void ExecuteReset(ChunkProtectionLevel _protectionMask, EnumResetUnprotectedChunksGroupingMode _groupingMode, int? _regionX, int? _regionZ, string _opName)
	{
		World world = GameManager.Instance.World;
		ChunkCluster chunkCache = world.ChunkCache;
		ChunkProviderGenerateWorld chunkProviderGenerateWorld = chunkCache.ChunkProvider as ChunkProviderGenerateWorld;
		if (chunkProviderGenerateWorld == null)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output(_opName + " failed: ChunkProviderGenerateWorld could not be found for current world instance.");
			return;
		}
		chunkProviderGenerateWorld.MainThreadCacheProtectedPositions();
		HashSetLong hashSetLong = ((_regionX != null && _regionZ != null) ? chunkProviderGenerateWorld.ResetRegion(_regionX.Value, _regionZ.Value, _protectionMask, _groupingMode) : chunkProviderGenerateWorld.ResetAllChunks(_protectionMask, _groupingMode));
		if ((_protectionMask & ChunkProtectionLevel.CurrentlySynced) == ChunkProtectionLevel.None)
		{
			HashSetLong hashSetLong2 = new HashSetLong();
			HashSetLong hashSetLong3 = new HashSetLong();
			foreach (long num in hashSetLong)
			{
				if (chunkCache.ContainsChunkSync(num))
				{
					hashSetLong2.Add(num);
				}
			}
			if (hashSetLong2.Count > 0)
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("Regenerating {0} synced chunks.", hashSetLong2.Count));
				foreach (long num2 in hashSetLong2)
				{
					if (!chunkProviderGenerateWorld.GenerateSingleChunk(chunkCache, num2, true))
					{
						SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("{0} failed regenerating chunk at world XZ position: {1}, {2}", _opName, WorldChunkCache.extractX(num2) << 4, WorldChunkCache.extractZ(num2) << 4));
					}
					else
					{
						hashSetLong3.Add(num2);
					}
				}
				world.m_ChunkManager.ResendChunksToClients(hashSetLong3);
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Regeneration complete.");
			}
		}
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("{0} complete. Reset {1} chunks.", _opName, hashSetLong.Count));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static int FindFlag(List<string> _params, params string[] _flags)
	{
		for (int i = 0; i < _params.Count; i++)
		{
			string text = _params[i].ToLowerInvariant();
			for (int j = 0; j < _flags.Length; j++)
			{
				if (text == _flags[j])
				{
					return i;
				}
			}
		}
		return -1;
	}
}
