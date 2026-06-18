using System;
using System.Collections.Generic;

public class TraderStageTemplateGroup
{
	public bool IsWithin(int traderStage, int quality)
	{
		for (int i = 0; i < this.Templates.Count; i++)
		{
			if (this.Templates[i].IsWithin(traderStage, quality))
			{
				return true;
			}
		}
		return false;
	}

	public string Name = "";

	public List<TraderStageTemplate> Templates = new List<TraderStageTemplate>();
}
