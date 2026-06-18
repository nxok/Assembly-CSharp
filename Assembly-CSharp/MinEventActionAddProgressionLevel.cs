using System;
using System.Globalization;
using System.Xml.Linq;
using SandboxOptions;
using UnityEngine.Scripting;

[Preserve]
public class MinEventActionAddProgressionLevel : MinEventActionTargetedBase
{
	public string progressionName
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public int level
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public override void Execute(MinEventParams _params)
	{
		if (this.targets == null)
		{
			return;
		}
		for (int i = 0; i < this.targets.Count; i++)
		{
			if (this.targets[i].Progression != null)
			{
				ProgressionValue progressionValue = this.targets[i].Progression.GetProgressionValue(this.progressionName);
				if (progressionValue != null)
				{
					int level = progressionValue.Level;
					int num = this.level;
					if (progressionValue.ProgressionClass.Type == ProgressionType.Crafting && this.level == 1)
					{
						num = SandboxOptionManager.GetInt(global::SandboxOptions.SandboxOptions.PointsPerMagazine);
					}
					progressionValue.Level += num;
					if (progressionValue.Level > progressionValue.ProgressionClass.MaxLevel)
					{
						progressionValue.Level = progressionValue.ProgressionClass.MaxLevel;
					}
					if (progressionValue.Level < 0)
					{
						progressionValue.Level = 0;
					}
					if (level != progressionValue.Level && progressionValue.ProgressionClass.IsCrafting && this.targets[i] is EntityPlayerLocal)
					{
						EntityPlayerLocal entityPlayerLocal = this.targets[i] as EntityPlayerLocal;
						if (XUiM_Recipes.CraftingProgression)
						{
							entityPlayerLocal.PlayerUI.xui.CollectedItemList.AddCraftingSkillNotification(progressionValue, false);
							if (this.showMessage)
							{
								progressionValue.ProgressionClass.HandleCheckCrafting(entityPlayerLocal, level, progressionValue.Level);
							}
						}
					}
					this.targets[i].Progression.bProgressionStatsChanged = !this.targets[i].isEntityRemote;
					this.targets[i].bPlayerStatsChanged |= !this.targets[i].isEntityRemote;
				}
			}
		}
	}

	public override bool CanExecute(MinEventTypes _eventType, MinEventParams _params)
	{
		return base.CanExecute(_eventType, _params) && this.progressionName != null && this.level >= -1;
	}

	public override bool ParseXmlAttribute(XAttribute _attribute)
	{
		bool flag = base.ParseXmlAttribute(_attribute);
		if (!flag)
		{
			string localName = _attribute.Name.LocalName;
			if (!(localName == "progression_name"))
			{
				if (!(localName == "level"))
				{
					if (localName == "show_message")
					{
						this.showMessage = StringParsers.ParseBool(_attribute.Value, 0, -1, true);
					}
				}
				else
				{
					this.level = StringParsers.ParseSInt32(_attribute.Value, 0, -1, NumberStyles.Integer);
				}
			}
			else
			{
				this.progressionName = _attribute.Value;
			}
		}
		return flag;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool showMessage = true;
}
