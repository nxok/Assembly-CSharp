using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions
{
	[Preserve]
	public class ActionBaseBlockAction : BaseAction
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		public virtual bool NeedsDamage()
		{
			return false;
		}

		public override BaseAction.ActionCompleteStates OnPerformAction()
		{
			List<BlockChangeInfo> list = new List<BlockChangeInfo>();
			this.startPoint = ((base.Owner.TargetPosition.y != 0f) ? base.Owner.TargetPosition : base.Owner.Target.position);
			World world = GameManager.Instance.World;
			this.random = world.GetGameRandom();
			FastTags<TagGroup.Global> fastTags = ((this.blockTags != null) ? FastTags<TagGroup.Global>.Parse(this.blockTags) : FastTags<TagGroup.Global>.none);
			FastTags<TagGroup.Global> fastTags2 = ((this.excludeTags != null) ? FastTags<TagGroup.Global>.Parse(this.excludeTags) : FastTags<TagGroup.Global>.none);
			IChunk chunk = null;
			if (base.Owner.Target != null && !base.Owner.Target.onGround)
			{
				return BaseAction.ActionCompleteStates.InComplete;
			}
			for (int i = this.minOffset.y; i <= this.maxOffset.y; i++)
			{
				for (int j = this.minOffset.z; j <= this.maxOffset.z; j += this.spacing + 1)
				{
					int num = (int)Utils.FastAbs((float)j);
					for (int k = this.minOffset.x; k <= this.maxOffset.x; k += this.spacing + 1)
					{
						if ((this.innerOffset == -1 || Utils.FastAbs((float)k) > (float)this.innerOffset || num > this.innerOffset) && (this.randomChance <= 0f || this.random.RandomFloat <= this.randomChance))
						{
							Vector3i vector3i = new Vector3i(Utils.Fastfloor(this.startPoint.x + (float)k), Utils.Fastfloor(this.startPoint.y + (float)i), Utils.Fastfloor(this.startPoint.z + (float)j));
							if (vector3i.y >= 0 && world.GetChunkFromWorldPos(vector3i, ref chunk) && world.GetTraderAreaAt(vector3i) == null && (!this.checkSafe || (!this.safeAllowed && world.CanPlaceBlockAt(vector3i, null, false))))
							{
								int num2 = World.toBlockXZ(vector3i.x);
								int num3 = World.toBlockXZ(vector3i.z);
								BlockValue blockValue = (this.NeedsDamage() ? chunk.GetBlock(num2, vector3i.y, num3) : chunk.GetBlockNoDamage(num2, vector3i.y, num3));
								if (!blockValue.ischild && (this.allowTerrain || !blockValue.Block.shape.IsTerrain()) && (this.blockTags == null || blockValue.Block.Tags.Test_AnySet(fastTags)) && (this.excludeTags == null || !blockValue.Block.Tags.Test_AnySet(fastTags2)) && this.CheckValid(world, vector3i))
								{
									BlockChangeInfo blockChangeInfo = this.UpdateBlock(world, vector3i, blockValue);
									if (blockChangeInfo != null)
									{
										list.Add(blockChangeInfo);
									}
								}
							}
						}
					}
				}
			}
			if (list.Count > 0)
			{
				if (this.maxCount != -1 && this.maxCount < list.Count)
				{
					int num4 = list.Count - this.maxCount;
					for (int l = 0; l < num4; l++)
					{
						list.RemoveAt(this.random.RandomRange(list.Count));
					}
				}
				this.ChangesComplete();
				this.ProcessChanges(world, list);
				return BaseAction.ActionCompleteStates.Complete;
			}
			return BaseAction.ActionCompleteStates.InCompleteRefund;
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public virtual void ProcessChanges(World world, List<BlockChangeInfo> blockChanges)
		{
			world.SetBlocksRPC(blockChanges);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public virtual void ChangesComplete()
		{
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public virtual bool CheckValid(World world, Vector3i currentPos)
		{
			return true;
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public virtual BlockChangeInfo UpdateBlock(World world, Vector3i currentPos, BlockValue blockValue)
		{
			return null;
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public IEnumerator UpdateBlocks(List<BlockChangeInfo> blockChanges)
		{
			yield return new WaitForSeconds(0.5f);
			GameManager.Instance.World.SetBlocksRPC(blockChanges);
			yield break;
		}

		public override BaseAction Clone()
		{
			ActionBaseBlockAction actionBaseBlockAction = base.Clone() as ActionBaseBlockAction;
			actionBaseBlockAction.minOffset = this.minOffset;
			actionBaseBlockAction.maxOffset = this.maxOffset;
			actionBaseBlockAction.spacing = this.spacing;
			actionBaseBlockAction.randomChance = this.randomChance;
			actionBaseBlockAction.safeAllowed = this.safeAllowed;
			actionBaseBlockAction.checkSafe = this.checkSafe;
			actionBaseBlockAction.blockTags = this.blockTags;
			actionBaseBlockAction.excludeTags = this.excludeTags;
			actionBaseBlockAction.innerOffset = this.innerOffset;
			actionBaseBlockAction.allowTerrain = this.allowTerrain;
			actionBaseBlockAction.maxCount = this.maxCount;
			return actionBaseBlockAction;
		}

		public override void ParseProperties(DynamicProperties properties)
		{
			base.ParseProperties(properties);
			properties.ParseVec(ActionBaseBlockAction.PropMinOffset, ref this.minOffset);
			properties.ParseVec(ActionBaseBlockAction.PropMaxOffset, ref this.maxOffset);
			properties.ParseInt(ActionBaseBlockAction.PropSpacing, ref this.spacing);
			properties.ParseInt(ActionBaseBlockAction.PropInnerOffset, ref this.innerOffset);
			properties.ParseFloat(ActionBaseBlockAction.PropRandomChance, ref this.randomChance);
			if (properties.Contains(ActionBaseBlockAction.PropSafeAllowed))
			{
				properties.ParseBool(ActionBaseBlockAction.PropSafeAllowed, ref this.safeAllowed);
				this.checkSafe = true;
			}
			properties.ParseString(ActionBaseBlockAction.PropBlockTags, ref this.blockTags);
			properties.ParseString(ActionBaseBlockAction.PropExcludeTags, ref this.excludeTags);
			properties.ParseBool(ActionBaseBlockAction.PropAllowTerrain, ref this.allowTerrain);
			properties.ParseInt(ActionBaseBlockAction.PropMaxCount, ref this.maxCount);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public Vector3i minOffset = Vector3i.zero;

		[PublicizedFrom(EAccessModifier.Protected)]
		public Vector3i maxOffset = Vector3i.zero;

		[PublicizedFrom(EAccessModifier.Protected)]
		public string blockTags;

		[PublicizedFrom(EAccessModifier.Protected)]
		public string excludeTags;

		[PublicizedFrom(EAccessModifier.Protected)]
		public int spacing;

		[PublicizedFrom(EAccessModifier.Protected)]
		public int innerOffset = -1;

		[PublicizedFrom(EAccessModifier.Protected)]
		public bool safeAllowed;

		[PublicizedFrom(EAccessModifier.Protected)]
		public bool checkSafe;

		[PublicizedFrom(EAccessModifier.Protected)]
		public bool allowTerrain;

		[PublicizedFrom(EAccessModifier.Protected)]
		public float randomChance = -1f;

		[PublicizedFrom(EAccessModifier.Protected)]
		public int maxCount = -1;

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropBlockTags = "block_tags";

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropExcludeTags = "exclude_tags";

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropMinOffset = "min_offset";

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropMaxOffset = "max_offset";

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropSpacing = "spacing";

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropRandomChance = "random_chance";

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropSafeAllowed = "safe_allowed";

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropInnerOffset = "inner_offset";

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropAllowTerrain = "allow_terrain";

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropMaxCount = "max_count";

		[PublicizedFrom(EAccessModifier.Protected)]
		public GameRandom random;

		[PublicizedFrom(EAccessModifier.Protected)]
		public Vector3 startPoint = Vector3.zero;
	}
}
