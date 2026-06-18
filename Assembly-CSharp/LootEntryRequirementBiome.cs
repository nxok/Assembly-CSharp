using System;
using System.Xml.Linq;
using UnityEngine.Scripting;

[Preserve]
public class LootEntryRequirementBiome : BaseLootEntryRequirement
{
	public override void Init(XElement e)
	{
		base.Init(e);
		string text = "";
		if (e.ParseAttribute("biomes", ref text))
		{
			this.biomes = text.Split(',', StringSplitOptions.None);
			return;
		}
		this.biomes = new string[0];
	}

	public override bool CheckRequirement(EntityPlayer player)
	{
		return this.biomes.ContainsCaseInsensitive(player.biomeStandingOn.m_sBiomeName);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string[] biomes;
}
