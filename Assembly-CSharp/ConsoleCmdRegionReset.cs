using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdRegionReset : ConsoleCmdAbstract
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[] { "regionreset", "rr" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Resets chunks within a target region, or for the entire map.";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "Usage: rr [-p|-pmode <n>] [-g|-gmode <n>] [-r|-region <x> <z>]\n\nExamples: \n'rr' - reset all unprotected chunks in all regions (defaults).\n'rr -p 2' - all regions, protection mode 2.\n'rr -r 1 -2' - only region (1,-2), default modes.\n'rr -r 1 -2 -p 0 -g 3' - region (1,-2) with protection mode 0 and grouping mode 3.\n\nProtection modes (-p|-pmode, default 0): \n'0' - Default: All protection statuses are respected, including the dynamic protection of synced chunks around active player position(s).\n'1' - EXPERIMENTAL: Most protection statuses are respected, excepting the dynamic protection of synced chunks around active player position(s). Chunks whose *only* protection status is \"CurrentlySynced\" will be treated as unprotected and are subject to being reset.\n'2' - EXPERIMENTAL: All protection statuses are ignored. Every chunk in the target area will be reset whether protected or not.\n'3' - EXPERIMENTAL: Most protection statuses are ignored, excepting the dynamic protection of synced chunks around active player position(s). Chunks whose protection status includes \"CurrentlySynced\" will be treated as protected; all other chunks are subject to being reset.\n\nGrouping modes (-g|-gmode, default 3): \n'1' - NoGrouping: Chunks are reset based on their own protection flags only.\n'2' - SeparatePOIs: Each POI gets the combined protection flags from all chunks overlapping the POI.\n'3' - GroupedPOIs: Like SeparatePOIs, but POIs with overlapping chunks also merge into groups. Standard reset; should not cause POI discontinuities.\n\nNotes: \n - Use with caution! This operation permanently deletes all saved data for affected chunks.\n - The experimental protection modes are provided for debug purposes only. They bypass various protections in order to force chunks to be reset. This can cause a significant hitch whilst any synced chunks are regenerated, and may cause other side effects such as failing to clean up nav markers for land claims, etc.";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		ChunkProtectionLevel chunkProtectionLevel;
		int num;
		if (!ChunkResetCommandHelpers.TryParseProtectionMode(_params, out chunkProtectionLevel, out num))
		{
			return;
		}
		EnumResetUnprotectedChunksGroupingMode enumResetUnprotectedChunksGroupingMode;
		if (!ChunkResetCommandHelpers.TryParseGroupingMode(_params, out enumResetUnprotectedChunksGroupingMode))
		{
			return;
		}
		int? num2;
		int? num3;
		if (!ChunkResetCommandHelpers.TryParseRegion(_params, out num2, out num3))
		{
			return;
		}
		string text = ((num2 != null) ? string.Format("-r {0} {1}", num2.Value, num3.Value) : "all regions");
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("Running region reset: -p {0}, -g {1} ({2}), {3}.", new object[]
		{
			num,
			(int)enumResetUnprotectedChunksGroupingMode,
			enumResetUnprotectedChunksGroupingMode,
			text
		}));
		ChunkResetCommandHelpers.ExecuteReset(chunkProtectionLevel, enumResetUnprotectedChunksGroupingMode, num2, num3, "Region reset");
	}
}
