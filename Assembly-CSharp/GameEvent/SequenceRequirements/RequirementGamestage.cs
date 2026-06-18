using System;
using UnityEngine.Scripting;

namespace GameEvent.SequenceRequirements
{
	[Preserve]
	public class RequirementGamestage : BaseOperationRequirement
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		public override void OnInit()
		{
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override object LeftSide(Entity target)
		{
			EntityPlayer entityPlayer = target as EntityPlayer;
			return (entityPlayer != null) ? entityPlayer.gameStage : 0;
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override object RightSide(Entity target)
		{
			return GameEventManager.GetIntValue(target as EntityAlive, this.gamestageText, 0);
		}

		public override void ParseProperties(DynamicProperties properties)
		{
			base.ParseProperties(properties);
			properties.ParseString(RequirementGamestage.PropGamestage, ref this.gamestageText);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override BaseRequirement CloneChildSettings()
		{
			return new RequirementGamestage
			{
				Invert = this.Invert,
				operation = this.operation,
				gamestageText = this.gamestageText
			};
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public string gamestageText;

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropGamestage = "game_stage";
	}
}
