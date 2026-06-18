using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Platform;
using UnityEngine.Scripting;

namespace Challenges
{
	[Preserve]
	public class ChallengeClass
	{
		public int OrderIndex
		{
			get; [PublicizedFrom(EAccessModifier.Private)]
			set;
		}

		public bool HasNavType
		{
			get
			{
				return this.GetNavType() > ChallengeClass.UINavTypes.None;
			}
		}

		public ChallengeClass(string name)
		{
			this.Name = name;
			this.OrderIndex = ChallengeClass.nextIndex++;
		}

		public static ChallengeClass NewClass(string id)
		{
			if (ChallengeClass.s_Challenges.ContainsKey(id))
			{
				return null;
			}
			ChallengeClass challengeClass = new ChallengeClass(id.ToLower());
			ChallengeClass.s_Challenges[id] = challengeClass;
			return challengeClass;
		}

		public static void Cleanup()
		{
			ChallengeClass.s_Challenges.Clear();
		}

		public static void InitChallenges()
		{
			foreach (string text in ChallengeClass.s_Challenges.Keys)
			{
				ChallengeClass.s_Challenges[text].Init();
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void Init()
		{
			for (int i = 0; i < this.ObjectiveList.Count; i++)
			{
				this.ObjectiveList[i].BaseInit();
				if (this.ObjectiveList[i].NeedsConstantUIUpdate)
				{
					this.NeedsConstantUIUpdate = true;
				}
			}
		}

		public bool HasEventsOrPassives()
		{
			return this.Effects != null;
		}

		public void ModifyValue(EntityAlive _ea, PassiveEffects _effect, ref float _base_value, ref float _perc_value, FastTags<TagGroup.Global> _tags = default(FastTags<TagGroup.Global>))
		{
			if (this.Effects == null)
			{
				return;
			}
			this.Effects.ModifyValue(_ea, _effect, ref _base_value, ref _perc_value, 0f, _tags, 1);
		}

		public static ChallengeClass GetChallenge(string name)
		{
			if (ChallengeClass.s_Challenges.ContainsKey(name))
			{
				return ChallengeClass.s_Challenges[name];
			}
			return null;
		}

		public Challenge CreateChallenge(ChallengeJournal ownerJournal)
		{
			Challenge challenge = new Challenge();
			challenge.ChallengeClass = this;
			challenge.Owner = ownerJournal;
			for (int i = 0; i < this.ObjectiveList.Count; i++)
			{
				BaseChallengeObjective baseChallengeObjective = this.ObjectiveList[i];
				BaseChallengeObjective baseChallengeObjective2 = baseChallengeObjective.Clone();
				baseChallengeObjective2.Owner = challenge;
				baseChallengeObjective2.IsRequirement = baseChallengeObjective.IsRequirement;
				baseChallengeObjective2.MaxCount = baseChallengeObjective.MaxCount;
				baseChallengeObjective2.ShowRequirements = baseChallengeObjective.ShowRequirements;
				baseChallengeObjective2.Biome = baseChallengeObjective.Biome;
				baseChallengeObjective2.HandleOnCreated();
				challenge.ObjectiveList.Add(baseChallengeObjective2);
			}
			return challenge;
		}

		public string GetNextChallengeName()
		{
			if (this.NextChallenge != null)
			{
				return this.NextChallenge.Name;
			}
			return "";
		}

		public void AddObjective(BaseChallengeObjective objective)
		{
			this.ObjectiveList.Add(objective);
			objective.OwnerClass = this;
		}

		public bool ResetObjectives(Challenge challenge)
		{
			int count = challenge.ObjectiveList.Count;
			if (count > this.ObjectiveList.Count)
			{
				challenge.ObjectiveList.RemoveRange(this.ObjectiveList.Count, count - this.ObjectiveList.Count);
			}
			for (int i = 0; i < this.ObjectiveList.Count; i++)
			{
				BaseChallengeObjective baseChallengeObjective = this.ObjectiveList[i];
				if (i < count && baseChallengeObjective.GetType() != challenge.ObjectiveList[i].GetType())
				{
					return false;
				}
				BaseChallengeObjective baseChallengeObjective2 = baseChallengeObjective.Clone();
				baseChallengeObjective2.Owner = challenge;
				baseChallengeObjective2.IsRequirement = baseChallengeObjective.IsRequirement;
				baseChallengeObjective2.MaxCount = baseChallengeObjective.MaxCount;
				baseChallengeObjective2.ShowRequirements = baseChallengeObjective.ShowRequirements;
				baseChallengeObjective2.Biome = baseChallengeObjective.Biome;
				baseChallengeObjective2.HandleOnCreated();
				if (i < count)
				{
					BaseChallengeObjective baseChallengeObjective3 = challenge.ObjectiveList[i];
					baseChallengeObjective2.CopyValues(baseChallengeObjective3, baseChallengeObjective);
					challenge.ObjectiveList[i] = baseChallengeObjective2;
					baseChallengeObjective2.Current = Utils.FastMin(baseChallengeObjective2.Current, baseChallengeObjective2.MaxCount);
				}
				else
				{
					challenge.ObjectiveList.Add(baseChallengeObjective2);
				}
				if (!challenge.IsActive)
				{
					baseChallengeObjective2.Current = baseChallengeObjective2.MaxCount;
					baseChallengeObjective2.Complete = true;
				}
			}
			return true;
		}

		public string GetHint(bool isPreReq)
		{
			if (this.ChallengeHint == null)
			{
				return "";
			}
			if (isPreReq)
			{
				if (PlatformManager.NativePlatform.Input.CurrentInputStyle != PlayerInputManager.InputStyle.Keyboard)
				{
					string text = this.PreReqChallengeHint + "_alt";
					if (Localization.Exists(text, false))
					{
						return Localization.Get(text, false, null);
					}
				}
				return Localization.Get(this.PreReqChallengeHint, false, null);
			}
			if (PlatformManager.NativePlatform.Input.CurrentInputStyle != PlayerInputManager.InputStyle.Keyboard)
			{
				string text2 = this.ChallengeHint + "_alt";
				if (Localization.Exists(text2, false))
				{
					return Localization.Get(text2, false, null);
				}
			}
			return Localization.Get(this.ChallengeHint, false, null);
		}

		public string GetDescription()
		{
			if (PlatformManager.NativePlatform.Input.CurrentInputStyle != PlayerInputManager.InputStyle.Keyboard)
			{
				string text = this.Description + "_alt";
				if (Localization.Exists(text, false))
				{
					return Localization.Get(text, false, null);
				}
			}
			return Localization.Get(this.Description, false, null);
		}

		public void FireEvent(MinEventTypes _eventType, MinEventParams _params)
		{
			if (this.Effects != null)
			{
				this.Effects.FireEvent(_eventType, _params);
			}
		}

		public void ParseElement(XElement e)
		{
			if (e.HasAttribute("icon"))
			{
				this.Icon = e.GetAttribute("icon");
			}
			if (e.HasAttribute("title_key"))
			{
				this.Title = Localization.Get(e.GetAttribute("title_key"), false, null);
			}
			else if (e.HasAttribute("title"))
			{
				this.Title = e.GetAttribute("title");
			}
			else
			{
				this.Title = this.Name;
			}
			if (e.HasAttribute("group"))
			{
				ChallengeGroup challengeGroup = ChallengeGroup.s_ChallengeGroups[e.GetAttribute("group")];
				this.ChallengeGroup = challengeGroup;
				challengeGroup.AddChallenge(this);
			}
			if (e.HasAttribute("prerequisite_hint"))
			{
				this.PreReqChallengeHint = e.GetAttribute("prerequisite_hint");
			}
			if (e.HasAttribute("hint"))
			{
				this.ChallengeHint = e.GetAttribute("hint");
			}
			if (e.HasAttribute("short_description_key"))
			{
				this.ShortDescription = Localization.Get(e.GetAttribute("short_description_key"), false, null);
			}
			else if (e.HasAttribute("short_description"))
			{
				this.ShortDescription = e.GetAttribute("short_description");
			}
			if (e.HasAttribute("description_key"))
			{
				this.Description = e.GetAttribute("description_key");
			}
			else if (e.HasAttribute("description"))
			{
				this.Description = e.GetAttribute("description");
			}
			if (e.HasAttribute("reward_event"))
			{
				this.RewardEvent = e.GetAttribute("reward_event");
			}
			else
			{
				this.RewardEvent = ChallengesFromXml.DefaultRewardEvent;
			}
			if (e.HasAttribute("reward_text_key"))
			{
				this.RewardText = Localization.Get(e.GetAttribute("reward_text_key"), false, null);
			}
			else if (e.HasAttribute("reward_text"))
			{
				this.RewardText = e.GetAttribute("reward_text");
			}
			else
			{
				this.RewardText = ChallengesFromXml.DefaultRewardText;
			}
			if (e.HasAttribute("tags"))
			{
				this.TagName = e.GetAttribute("tags");
				this.Tags = FastTags<TagGroup.Global>.Parse(this.TagName);
			}
			if (e.HasAttribute("redeem_always"))
			{
				this.RedeemAlways = StringParsers.ParseBool(e.GetAttribute("redeem_always"), 0, -1, true);
			}
			if (e.HasAttribute("harvest_required"))
			{
				string[] array = e.GetAttribute("harvest_required").Split(',', StringSplitOptions.None);
				for (int i = 0; i < array.Length; i++)
				{
					ChallengeClass.ResourceRequiredTypes resourceRequiredTypes = ChallengeClass.ResourceRequiredTypes.Resource;
					if (Enum.TryParse<ChallengeClass.ResourceRequiredTypes>(array[i], true, out resourceRequiredTypes))
					{
						if (this.RequiredResourceTypeList == null)
						{
							this.RequiredResourceTypeList = new List<ChallengeClass.ResourceRequiredTypes>();
						}
						this.RequiredResourceTypeList.Add(resourceRequiredTypes);
					}
				}
			}
		}

		public ChallengeClass.UINavTypes GetNavType()
		{
			for (int i = 0; i < this.ObjectiveList.Count; i++)
			{
				BaseChallengeObjective baseChallengeObjective = this.ObjectiveList[i];
				if (baseChallengeObjective.NavType != ChallengeClass.UINavTypes.None)
				{
					return baseChallengeObjective.NavType;
				}
			}
			return ChallengeClass.UINavTypes.None;
		}

		public bool HandleResourceRequirement()
		{
			if (ItemActionAttack.BlockDamagePercent == 0f)
			{
				return false;
			}
			if (this.RequiredResourceTypeList == null)
			{
				return true;
			}
			for (int i = 0; i < this.RequiredResourceTypeList.Count; i++)
			{
				switch (this.RequiredResourceTypeList[i])
				{
				case ChallengeClass.ResourceRequiredTypes.Resource:
					if (XUiM_Recipes.HarvestingOutputModifier == 0f)
					{
						return false;
					}
					break;
				case ChallengeClass.ResourceRequiredTypes.Ore:
					if (XUiM_Recipes.MiningOutputModifier == 0f)
					{
						return false;
					}
					break;
				case ChallengeClass.ResourceRequiredTypes.Seed:
					if (XUiM_Recipes.SeedDropOutputModifier == 0f)
					{
						return false;
					}
					break;
				case ChallengeClass.ResourceRequiredTypes.Crop:
					if (XUiM_Recipes.CropOutputModifier == 0f)
					{
						return false;
					}
					break;
				}
			}
			return true;
		}

		public static Dictionary<string, ChallengeClass> s_Challenges = new CaseInsensitiveStringDictionary<ChallengeClass>();

		public string Name;

		public string Title;

		public string Icon;

		public ChallengeGroup ChallengeGroup;

		public string ShortDescription;

		public string Description;

		public string PreReqChallengeHint;

		public string ChallengeHint;

		public string RewardEvent;

		public string RewardText = "";

		public string TagName = string.Empty;

		public FastTags<TagGroup.Global> Tags = FastTags<TagGroup.Global>.none;

		public List<BaseChallengeObjective> ObjectiveList = new List<BaseChallengeObjective>();

		public ChallengeClass NextChallenge;

		[PublicizedFrom(EAccessModifier.Private)]
		public static int nextIndex = 0;

		public bool RedeemAlways;

		public bool NeedsConstantUIUpdate;

		public MinEffectController Effects;

		public List<ChallengeClass.ResourceRequiredTypes> RequiredResourceTypeList;

		public enum UINavTypes
		{
			None,
			Crafting,
			TwitchActions
		}

		public enum ResourceRequiredTypes
		{
			Resource,
			Ore,
			Seed,
			Crop
		}
	}
}
