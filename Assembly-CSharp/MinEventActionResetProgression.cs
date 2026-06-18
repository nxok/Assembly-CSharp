using System;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine.Scripting;

[Preserve]
public class MinEventActionResetProgression : MinEventActionTargetedBase
{
	public override void Execute(MinEventParams _params)
	{
		EntityPlayerLocal entityPlayerLocal = this.targets[0] as EntityPlayerLocal;
		if (entityPlayerLocal != null)
		{
			entityPlayerLocal.Progression.ResetProgression(this.resetLevels || this.resetSkills, this.removeBooks, this.removeCrafting);
			if (this.resetLevels)
			{
				entityPlayerLocal.Progression.Level = 1;
				entityPlayerLocal.Progression.ExpToNextLevel = entityPlayerLocal.Progression.GetExpForNextLevel();
				entityPlayerLocal.Progression.SkillPoints = entityPlayerLocal.QuestJournal.GetRewardedSkillPoints();
				entityPlayerLocal.Progression.ExpDeficit = 0;
			}
			if (this.removeCrafting)
			{
				List<Recipe> recipes = CraftingManager.GetRecipes();
				for (int i = 0; i < recipes.Count; i++)
				{
					if (recipes[i].IsLearnable)
					{
						entityPlayerLocal.Buffs.RemoveCustomVar(recipes[i].GetName());
					}
				}
			}
			entityPlayerLocal.Progression.ResetProgression(this.removeBooks, false, false);
			entityPlayerLocal.Progression.bProgressionStatsChanged = true;
			entityPlayerLocal.bPlayerStatsChanged = true;
		}
	}

	public override bool ParseXmlAttribute(XAttribute _attribute)
	{
		bool flag = base.ParseXmlAttribute(_attribute);
		if (!flag)
		{
			string localName = _attribute.Name.LocalName;
			if (!(localName == "reset_books"))
			{
				if (!(localName == "reset_levels"))
				{
					if (!(localName == "reset_skills"))
					{
						if (localName == "reset_crafting")
						{
							this.removeCrafting = StringParsers.ParseBool(_attribute.Value, 0, -1, true);
						}
					}
					else
					{
						this.resetSkills = StringParsers.ParseBool(_attribute.Value, 0, -1, true);
					}
				}
				else
				{
					this.resetLevels = StringParsers.ParseBool(_attribute.Value, 0, -1, true);
				}
			}
			else
			{
				this.removeBooks = StringParsers.ParseBool(_attribute.Value, 0, -1, true);
			}
		}
		return flag;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool resetLevels;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool resetSkills;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool removeBooks;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool removeCrafting;
}
