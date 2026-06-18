using System;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions
{
	[Preserve]
	public class ActionModifyProgression : ActionBaseClientAction
	{
		public override void OnClientPerform(Entity target)
		{
			EntityPlayerLocal entityPlayerLocal = target as EntityPlayerLocal;
			if (entityPlayerLocal != null)
			{
				for (int i = 0; i < this.ProgressionNames.Length; i++)
				{
					string text = ((this.Values != null && this.Values.Length > i) ? this.Values[i] : "1");
					ProgressionValue progressionValue = entityPlayerLocal.Progression.GetProgressionValue(this.ProgressionNames[i]);
					int intValue = GameEventManager.GetIntValue(entityPlayerLocal, text, 1);
					ActionModifyProgression.ModifyTypes modifyType = this.ModifyType;
					if (modifyType != ActionModifyProgression.ModifyTypes.Set)
					{
						if (modifyType == ActionModifyProgression.ModifyTypes.Add)
						{
							progressionValue.Level += intValue;
						}
					}
					else
					{
						progressionValue.Level = intValue;
					}
				}
			}
		}

		public override void ParseProperties(DynamicProperties properties)
		{
			base.ParseProperties(properties);
			if (properties.Values.ContainsKey(ActionModifyProgression.PropProgressionNames))
			{
				this.ProgressionNames = properties.Values[ActionModifyProgression.PropProgressionNames].Replace(" ", "").Split(',', StringSplitOptions.None);
				if (properties.Values.ContainsKey(ActionModifyProgression.PropValues))
				{
					this.Values = properties.Values[ActionModifyProgression.PropValues].Replace(" ", "").Split(',', StringSplitOptions.None);
				}
				else
				{
					this.Values = null;
				}
			}
			else
			{
				this.ProgressionNames = null;
				this.Values = null;
			}
			properties.ParseEnum<ActionModifyProgression.ModifyTypes>(ActionModifyProgression.PropModifyType, ref this.ModifyType);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override BaseAction CloneChildSettings()
		{
			return new ActionModifyProgression
			{
				ModifyType = this.ModifyType,
				ProgressionNames = this.ProgressionNames,
				Values = this.Values,
				targetGroup = this.targetGroup
			};
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public ActionModifyProgression.ModifyTypes ModifyType;

		public string[] ProgressionNames;

		public string[] Values;

		public static string PropModifyType = "modify_type";

		public static string PropProgressionNames = "progression_names";

		public static string PropValues = "values";

		[PublicizedFrom(EAccessModifier.Protected)]
		public enum ModifyTypes
		{
			Set,
			Add,
			Remove
		}
	}
}
