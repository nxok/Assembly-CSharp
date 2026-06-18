using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdPrintChunkExpiryInfo : ConsoleCmdAbstract
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[] { "expiryinfo" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Prints location and expiry day/time for the next [x] chunks set to expire.";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "expiryinfo [x]\n" + this.GetDescription();
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		ChunkProviderGenerateWorld chunkProviderGenerateWorld = GameManager.Instance.World.ChunkCache.ChunkProvider as ChunkProviderGenerateWorld;
		if (chunkProviderGenerateWorld == null)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Failed to retrieve chunk expiry info: ChunkProviderGenerateWorld could not be found for current world instance.");
			return;
		}
		int num;
		if (_params.Count != 1 || !int.TryParse(_params[0], out num) || num < 1)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output(this.GetHelp());
			return;
		}
		try
		{
			List<KeyValuePair<long, ulong>> expiryTimes = new List<KeyValuePair<long, ulong>>();
			chunkProviderGenerateWorld.IterateChunkExpiryTimes(delegate(long chunkKey, ulong expiry)
			{
				expiryTimes.Add(new KeyValuePair<long, ulong>(chunkKey, expiry));
			});
			num = Mathf.Min(num, expiryTimes.Count);
			if (num == 0)
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("No chunks are currently set to expire. Ensure max chunk age is enabled.");
			}
			else
			{
				expiryTimes.Sort((KeyValuePair<long, ulong> a, KeyValuePair<long, ulong> b) => a.Value.CompareTo(b.Value));
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Chunk\t\tExpiry");
				for (int i = 0; i < num; i++)
				{
					long key = expiryTimes[i].Key;
					ulong value = expiryTimes[i].Value;
					int num2 = WorldChunkCache.extractX(key);
					int num3 = WorldChunkCache.extractZ(key);
					ValueTuple<int, int, int> valueTuple = GameUtils.WorldTimeToElements(value);
					int item = valueTuple.Item1;
					int item2 = valueTuple.Item2;
					int item3 = valueTuple.Item3;
					SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("{0}, {1}\t\tDay {2}, {3}", new object[]
					{
						num2,
						num3,
						item,
						string.Format("{0:D2}:{1:D2}", item2, item3)
					}));
				}
			}
		}
		catch (Exception ex)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Failed to retrieve chunk expiry info with exception: " + ex.Message);
		}
	}
}
