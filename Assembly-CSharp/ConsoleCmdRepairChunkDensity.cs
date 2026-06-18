using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdRepairChunkDensity : ConsoleCmdAbstract
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "check and optionally fix densities of a chunk";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "This command is used to check if the densities of blocks in a chunk match the actual block type.\nIf there is a mismatch it can lead to the chunk rendering incorrectly or not at all, typically\nindicated by the error message \"Failed setting triangles. Some indices are referencing out of\nbounds vertices.\". It can also fix such mismatches within a chunk.\nUsage:\n  1. repairchunkdensity <x> <z>\n  2. repairchunkdensity <x> <z> fix\n1. Just checks the chunk and prints mismatched to the server log. x and z are the coordinates of any\n   block within the chunk to check.\n2. Repairs any mismatch found in the chunk.";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[] { "repairchunkdensity", "rcd" };
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (_params.Count < 2 || _params.Count > 3)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Wrong number of arguments, expected 2 or 3, found " + _params.Count.ToString() + ".");
			return;
		}
		int minValue = int.MinValue;
		int minValue2 = int.MinValue;
		if (!int.TryParse(_params[0], out minValue) || !int.TryParse(_params[1], out minValue2))
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("At least one of the given coordinates is not a valid integer");
			return;
		}
		Chunk chunk = GameManager.Instance.World.GetChunkFromWorldPos(minValue, 0, minValue2) as Chunk;
		if (chunk == null)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("No chunk could be loaded from the given coordinates");
			return;
		}
		string text = minValue.ToString() + " / " + minValue2.ToString();
		if (_params.Count == 3)
		{
			if (!string.Equals(_params[2], "fix", StringComparison.OrdinalIgnoreCase))
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Three parameters given but third parameter is not \"fix\"");
				return;
			}
			if (chunk.RepairDensities())
			{
				chunk.isModified = true;
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Chunk at " + text + " repaired. Leave the area and come back to reload the fixed chunk.");
				return;
			}
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Chunk at " + text + " had no issues to repair.");
			return;
		}
		else
		{
			List<Chunk.DensityMismatchInformation> list = chunk.CheckDensities(true);
			if (list.Count > 0)
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Concat(new string[]
				{
					"Found ",
					list.Count.ToString(),
					" issues in chunk at ",
					text,
					"."
				}));
				return;
			}
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("No issues found in chunk " + text + ".");
			return;
		}
	}
}
