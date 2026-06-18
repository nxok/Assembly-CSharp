using System;
using UnityEngine;
using UnityEngine.Scripting;

namespace GameEvent.SequenceRequirements
{
	[Preserve]
	public class RequirementIsBlock : BaseRequirement
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		public override void OnInit()
		{
		}

		public override bool CanPerform(Entity target)
		{
			return this.CheckBlock();
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public bool CheckBlock()
		{
			if (this.Owner.TargetPosition == Vector3.zero)
			{
				return false;
			}
			WorldBase world = GameManager.Instance.World;
			Vector3i vector3i = new Vector3i(Utils.Fastfloor(this.Owner.TargetPosition.x), Utils.Fastfloor(this.Owner.TargetPosition.y), Utils.Fastfloor(this.Owner.TargetPosition.z));
			if (world.GetBlock(vector3i).Block.GetBlockName().EqualsCaseInsensitive(this.BlockName))
			{
				return !this.Invert;
			}
			return this.Invert;
		}

		public override void ParseProperties(DynamicProperties properties)
		{
			base.ParseProperties(properties);
			if (properties.Values.ContainsKey(RequirementIsBlock.PropBlockName))
			{
				this.BlockName = properties.Values[RequirementIsBlock.PropBlockName];
			}
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override BaseRequirement CloneChildSettings()
		{
			return new RequirementIsBlock
			{
				BlockName = this.BlockName,
				Invert = this.Invert
			};
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public string BlockName = "";

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropBlockName = "block_name";
	}
}
