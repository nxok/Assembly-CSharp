using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Twitch
{
	public class TwitchAction
	{
		public bool HasModifiedPrice()
		{
			return this.DefaultCost != this.ModifiedCost;
		}

		public bool HasExtraConditions()
		{
			return ((!this.SingleDayUse && !this.RandomDaily) || this.AllowedDay != -1) && !this.OnCooldown && this.OnlyUsableByType == TwitchAction.OnlyUsableTypes.Everyone;
		}

		public static int GetAdjustedBitPriceCeil(int price)
		{
			int num = TwitchAction.bitPrices.Find((int p) => p >= price);
			if (num == 0)
			{
				return TwitchAction.bitPrices[TwitchAction.bitPrices.Count - 1];
			}
			return num;
		}

		public static int GetAdjustedBitPriceFloor(int price)
		{
			return TwitchAction.bitPrices.FindLast((int p) => p <= price);
		}

		public static int GetAdjustedBitPriceFloorNoZero(int price)
		{
			return Mathf.Max(TwitchAction.GetAdjustedBitPriceFloor(price), TwitchAction.bitPrices[0]);
		}

		public int GetModifiedDiscountCost()
		{
			return TwitchAction.GetAdjustedBitPriceFloorNoZero((int)((float)this.ModifiedCost * TwitchManager.Current.BitPriceMultiplier));
		}

		public void DecreaseCost()
		{
			if (this.PointType == TwitchAction.PointTypes.Bits)
			{
				this.ModifiedCost = TwitchAction.bitPrices[(int)MathUtils.Clamp((float)(TwitchAction.bitPrices.IndexOf(this.ModifiedCost) - 1), 0f, (float)(TwitchAction.bitPrices.Count - 1))];
				return;
			}
			if (this.ModifiedCost > 25)
			{
				this.ModifiedCost -= 25;
			}
		}

		public void IncreaseCost()
		{
			if (this.PointType == TwitchAction.PointTypes.Bits)
			{
				this.ModifiedCost = TwitchAction.bitPrices[(int)MathUtils.Clamp((float)(TwitchAction.bitPrices.IndexOf(this.ModifiedCost) + 1), 0f, (float)(TwitchAction.bitPrices.Count - 1))];
				return;
			}
			if (this.ModifiedCost < 2000)
			{
				this.ModifiedCost += 25;
			}
		}

		public void ResetToDefaultCost()
		{
			if (this.PointType == TwitchAction.PointTypes.Bits)
			{
				this.ModifiedCost = TwitchAction.GetAdjustedBitPriceFloorNoZero(this.DefaultCost);
				return;
			}
			this.ModifiedCost = this.DefaultCost;
		}

		public bool CanUse
		{
			get
			{
				return this.Enabled && this.CheckAllowed();
			}
		}

		public bool RandomDaily
		{
			get
			{
				return this.RandomGroup != "";
			}
		}

		public bool SpecialOnly
		{
			get
			{
				return this.PointType == TwitchAction.PointTypes.SP;
			}
		}

		public bool CheckUsable(TwitchIRCClient.TwitchChatMessage message)
		{
			switch (this.OnlyUsableByType)
			{
			case TwitchAction.OnlyUsableTypes.Broadcaster:
				return message.isBroadcaster;
			case TwitchAction.OnlyUsableTypes.Mods:
				return message.isMod;
			case TwitchAction.OnlyUsableTypes.VIPs:
				return message.isVIP;
			case TwitchAction.OnlyUsableTypes.Subs:
				return message.isSub;
			case TwitchAction.OnlyUsableTypes.Name:
				return this.OnlyUsableBy.ContainsCaseInsensitive(message.UserName);
			default:
				return true;
			}
		}

		public void Init()
		{
			if (this.CategoryNames.Count > 0)
			{
				this.MainCategory = TwitchActionManager.Current.GetCategory(this.CategoryNames[this.CategoryNames.Count - 1]);
				if (this.DisplayCategory == null)
				{
					this.DisplayCategory = this.MainCategory;
				}
			}
			this.OnInit();
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public virtual void OnInit()
		{
		}

		public virtual TwitchActionEntry SetupActionEntry()
		{
			return new TwitchActionEntry();
		}

		public bool IsInPreset(TwitchActionPreset preset)
		{
			return ((!preset.IsEmpty && this.PresetNames != null && this.PresetNames.Contains(preset.Name)) || preset.AddedActions.Contains(this.Name)) && !preset.RemovedActions.Contains(this.Name);
		}

		public bool IsInPresetForList(TwitchActionPreset preset)
		{
			return (!preset.IsEmpty && this.PresetNames != null && this.PresetNames.Contains(preset.Name)) || preset.AddedActions.Contains(this.Name);
		}

		public bool IsInPresetDefault(TwitchActionPreset preset)
		{
			return !preset.IsEmpty && this.PresetNames != null && this.PresetNames.Contains(preset.Name);
		}

		public bool CanPerformAction(EntityPlayer target, TwitchActionEntry entry)
		{
			if (entry.Target == null)
			{
				entry.Target = target;
			}
			return this.OnPerformAction(target, entry);
		}

		public void SetQueued()
		{
			this.lastUse = Time.time;
			if (this.CooldownAdditions != null)
			{
				float actionCooldownModifier = TwitchManager.Current.ActionCooldownModifier;
				for (int i = 0; i < this.CooldownAdditions.Count; i++)
				{
					TwitchActionCooldownAddition twitchActionCooldownAddition = this.CooldownAdditions[i];
					if (twitchActionCooldownAddition.IsAction && TwitchActionManager.TwitchActions.ContainsKey(twitchActionCooldownAddition.ActionName))
					{
						TwitchAction twitchAction = TwitchActionManager.TwitchActions[twitchActionCooldownAddition.ActionName];
						twitchAction.tempCooldown = twitchActionCooldownAddition.CooldownTime * actionCooldownModifier;
						twitchAction.tempCooldownSet = Time.time;
					}
					else if (!twitchActionCooldownAddition.IsAction && TwitchActionManager.TwitchVotes.ContainsKey(twitchActionCooldownAddition.ActionName))
					{
						TwitchVote twitchVote = TwitchActionManager.TwitchVotes[twitchActionCooldownAddition.ActionName];
						twitchVote.tempCooldown = twitchActionCooldownAddition.CooldownTime * actionCooldownModifier;
						twitchVote.tempCooldownSet = Time.time;
					}
				}
			}
			if (this.VoteCooldownAddition != 0f)
			{
				TwitchVotingManager votingManager = TwitchManager.Current.VotingManager;
				votingManager.VoteStartDelayTimeRemaining = Math.Max(this.VoteCooldownAddition, votingManager.VoteStartDelayTimeRemaining);
			}
			if (this.SingleDayUse)
			{
				this.AllowedDay = -1;
			}
		}

		public virtual bool ParseProperties(DynamicProperties properties)
		{
			this.Properties = properties;
			properties.ParseString(TwitchAction.PropTitle, ref this.Title);
			if (properties.Values.ContainsKey(TwitchAction.PropTitleKey))
			{
				this.Title = Localization.Get(properties.Values[TwitchAction.PropTitleKey], false, null);
			}
			if (properties.Values.ContainsKey(TwitchAction.PropCommand))
			{
				this.BaseCommand = (this.Command = properties.Values[TwitchAction.PropCommand].ToLower());
				if (!Regex.IsMatch(this.Command, "^#[a-zA-Z0-9]+(_[a-zA-Z0-9]+)*$"))
				{
					return false;
				}
			}
			if (properties.Values.ContainsKey(TwitchAction.PropCommandKey))
			{
				this.Command = Localization.Get(properties.Values[TwitchAction.PropCommandKey], false, null).ToLower();
				if (Localization.LocalizationChecks)
				{
					if (this.Command.StartsWith("l_"))
					{
						this.Command = this.Command.Substring(3).Insert(0, "#l_");
					}
					else if (this.Command.StartsWith("ul_"))
					{
						this.Command = this.Command.Substring(3).Insert(0, "#ul_");
					}
					else if (this.Command.StartsWith("le_"))
					{
						this.Command = this.Command.Substring(3).Insert(0, "#le_");
					}
				}
				if (!Regex.IsMatch(this.Command, "^#[\\p{L}\\p{N}]+([-_][\\p{L}\\p{N}]+)*$"))
				{
					return false;
				}
			}
			if (properties.Values.ContainsKey(TwitchAction.PropCategory))
			{
				this.CategoryNames.AddRange(properties.Values[TwitchAction.PropCategory].Split(',', StringSplitOptions.None));
			}
			if (properties.Values.ContainsKey(TwitchAction.PropDisplayCategory))
			{
				this.DisplayCategory = TwitchActionManager.Current.GetCategory(properties.Values[TwitchAction.PropDisplayCategory]);
			}
			properties.ParseString(TwitchAction.PropEventName, ref this.EventName);
			properties.ParseString(TwitchAction.PropDescription, ref this.Description);
			if (properties.Values.ContainsKey(TwitchAction.PropDescriptionKey))
			{
				this.Description = Localization.Get(properties.Values[TwitchAction.PropDescriptionKey], false, null);
			}
			properties.ParseInt(TwitchAction.PropDefaultCost, ref this.DefaultCost);
			properties.ParseInt(TwitchAction.PropStartGameStage, ref this.StartGameStage);
			properties.ParseBool(TwitchAction.PropIsPositive, ref this.IsPositive);
			bool flag = false;
			properties.ParseBool(TwitchAction.PropSpecialOnly, ref flag);
			if (flag)
			{
				this.PointType = TwitchAction.PointTypes.SP;
			}
			properties.ParseBool(TwitchAction.PropAddCooldown, ref this.AddsToCooldown);
			properties.ParseInt(TwitchAction.PropCooldownAddAmount, ref this.CooldownAddAmount);
			properties.ParseBool(TwitchAction.PropCooldownBlocked, ref this.CooldownBlocked);
			properties.ParseBool(TwitchAction.PropWaitingBlocked, ref this.WaitingBlocked);
			if (properties.Values.ContainsKey(TwitchAction.PropCooldown))
			{
				this.OriginalCooldown = (this.Cooldown = StringParsers.ParseFloat(properties.Values[TwitchAction.PropCooldown], 0, -1, NumberStyles.Any));
				this.lastUse = Time.time - this.Cooldown;
				this.ModifiedCooldown = this.Cooldown;
			}
			properties.ParseBool(TwitchAction.PropEnabled, ref this.Enabled);
			this.OriginalEnabled = this.Enabled;
			properties.ParseBool(TwitchAction.PropSingleDayUse, ref this.SingleDayUse);
			properties.ParseString(TwitchAction.PropRandomGroup, ref this.RandomGroup);
			properties.ParseBool(TwitchAction.PropShowInActionList, ref this.ShowInActionList);
			if (properties.Values.ContainsKey(TwitchAction.PropSpecialRequirement))
			{
				string[] array = this.Properties.Values[TwitchAction.PropSpecialRequirement].Split(',', StringSplitOptions.None);
				this.SpecialRequirementList = new List<TwitchAction.SpecialRequirements>();
				foreach (string text in array)
				{
					TwitchAction.SpecialRequirements specialRequirements = TwitchAction.SpecialRequirements.None;
					if (Enum.TryParse<TwitchAction.SpecialRequirements>(text, true, out specialRequirements))
					{
						this.SpecialRequirementList.Add(specialRequirements);
					}
					else
					{
						Log.Error("TwitchAction " + this.Title + " has unknown SpecialRequirement " + text);
					}
				}
			}
			properties.ParseString(TwitchAction.PropReplaces, ref this.Replaces);
			properties.ParseBool(TwitchAction.PropTwitchNotify, ref this.TwitchNotify);
			properties.ParseBool(TwitchAction.PropDelayNotify, ref this.DelayNotify);
			properties.ParseBool(TwitchAction.PropPlayBitSound, ref this.PlayBitSound);
			properties.ParseBool(TwitchAction.PropHideOnDisable, ref this.HideOnDisable);
			properties.ParseBool(TwitchAction.PropIgnoreCooldown, ref this.IgnoreCooldown);
			properties.ParseBool(TwitchAction.PropIgnoreDiscount, ref this.IgnoreDiscount);
			properties.ParseBool(TwitchAction.PropStreamerOnly, ref this.StreamerOnly);
			properties.ParseEnum<TwitchAction.OnlyUsableTypes>(TwitchAction.PropOnlyUsableByType, ref this.OnlyUsableByType);
			string text2 = "";
			properties.ParseString(TwitchAction.PropOnlyUsableBy, ref text2);
			if (text2 != "")
			{
				this.OnlyUsableBy = text2.Split(',', StringSplitOptions.None);
			}
			properties.ParseEnum<TwitchAction.PointTypes>(TwitchAction.PropPointTypes, ref this.PointType);
			this.ResetToDefaultCost();
			this.UpdateCost(1f);
			if (this.CooldownAddAmount == -1)
			{
				this.CooldownAddAmount = this.DefaultCost;
			}
			properties.ParseFloat(TwitchAction.PropVoteCooldownAddition, ref this.VoteCooldownAddition);
			if (properties.Values.ContainsKey(TwitchAction.PropPresets))
			{
				this.PresetNames = new List<string>();
				this.PresetNames.AddRange(properties.Values[TwitchAction.PropPresets].Split(',', StringSplitOptions.None));
			}
			properties.ParseBool(TwitchAction.PropOnlyShowInPreset, ref this.OnlyShowInPreset);
			if (properties.Contains(TwitchAction.PropMinRespawnCount) || properties.Contains(TwitchAction.PropMaxRespawnCount))
			{
				properties.ParseInt(TwitchAction.PropMinRespawnCount, ref this.MinRespawnCount);
				properties.ParseInt(TwitchAction.PropMaxRespawnCount, ref this.MaxRespawnCount);
				this.RespawnCountType = TwitchAction.RespawnCountTypes.Both;
			}
			else
			{
				this.RespawnCountType = TwitchAction.RespawnCountTypes.None;
			}
			properties.ParseEnum<TwitchAction.RespawnCountTypes>(TwitchAction.PropRespawnCountType, ref this.RespawnCountType);
			properties.ParseInt(TwitchAction.PropRespawnThreshold, ref this.RespawnThreshold);
			return true;
		}

		public bool UpdateCost(float bitPriceModifier = 1f)
		{
			int currentCost = this.CurrentCost;
			if (this.PointType == TwitchAction.PointTypes.Bits)
			{
				if (!this.IgnoreDiscount && bitPriceModifier != 1f)
				{
					this.CurrentCost = TwitchAction.GetAdjustedBitPriceFloorNoZero((int)((float)this.ModifiedCost * bitPriceModifier));
				}
				else
				{
					this.CurrentCost = TwitchAction.GetAdjustedBitPriceFloorNoZero(this.ModifiedCost);
				}
			}
			else
			{
				this.CurrentCost = this.ModifiedCost;
			}
			return currentCost != this.CurrentCost;
		}

		public void AddRequirement(BaseTwitchRequirement requirement)
		{
			if (this.TwitchRequirements == null)
			{
				this.TwitchRequirements = new List<BaseTwitchRequirement>();
			}
			requirement.OwnerAction = this;
			requirement.OwnerType = BaseTwitchRequirement.OwnerTypes.Action;
			this.TwitchRequirements.Add(requirement);
		}

		public bool CheckAllowed()
		{
			if (this.TwitchRequirements != null)
			{
				EntityPlayerLocal localPlayer = TwitchManager.Current.LocalPlayer;
				for (int i = 0; i < this.TwitchRequirements.Count; i++)
				{
					BaseTwitchRequirement baseTwitchRequirement = this.TwitchRequirements[i];
					if (baseTwitchRequirement.HideAction && !baseTwitchRequirement.CanPerform(localPlayer))
					{
						return false;
					}
				}
			}
			return true;
		}

		public virtual bool IsReady(TwitchManager twitchManager)
		{
			if (this.TwitchRequirements != null)
			{
				EntityPlayerLocal localPlayer = twitchManager.LocalPlayer;
				for (int i = 0; i < this.TwitchRequirements.Count; i++)
				{
					BaseTwitchRequirement baseTwitchRequirement = this.TwitchRequirements[i];
					if (!baseTwitchRequirement.HideAction && !baseTwitchRequirement.CanPerform(localPlayer))
					{
						return false;
					}
				}
			}
			if (this.SpecialRequirementList != null)
			{
				for (int j = 0; j < this.SpecialRequirementList.Count; j++)
				{
					switch (this.SpecialRequirementList[j])
					{
					case TwitchAction.SpecialRequirements.HasSpawnedEntities:
						if (twitchManager.actionSpawnLiveList.Count == 0)
						{
							return false;
						}
						break;
					case TwitchAction.SpecialRequirements.NoSpawnedEntities:
						if (twitchManager.actionSpawnLiveList.Count > 0)
						{
							return false;
						}
						break;
					case TwitchAction.SpecialRequirements.Bloodmoon:
						if (!twitchManager.isBMActive)
						{
							return false;
						}
						break;
					case TwitchAction.SpecialRequirements.NotBloodmoon:
						if (twitchManager.isBMActive)
						{
							return false;
						}
						break;
					case TwitchAction.SpecialRequirements.NotBloodmoonDay:
						if (SkyManager.IsBloodMoonVisible())
						{
							return false;
						}
						if (GameUtils.WorldTimeToDays(GameManager.Instance.World.worldTime) == GameStats.GetInt(EnumGameStats.BloodMoonDay))
						{
							return false;
						}
						break;
					case TwitchAction.SpecialRequirements.EarlyDay:
					{
						int num = GameUtils.WorldTimeToHours(GameManager.Instance.World.worldTime);
						if ((float)num > SkyManager.GetDuskTime() - 5f || (float)num < SkyManager.GetDawnTime())
						{
							return false;
						}
						break;
					}
					case TwitchAction.SpecialRequirements.Daytime:
					{
						int num2 = GameUtils.WorldTimeToHours(GameManager.Instance.World.worldTime);
						if ((float)num2 > SkyManager.GetDuskTime() || (float)num2 < SkyManager.GetDawnTime())
						{
							return false;
						}
						break;
					}
					case TwitchAction.SpecialRequirements.Night:
						GameUtils.WorldTimeToHours(GameManager.Instance.World.worldTime);
						if (!SkyManager.IsDark())
						{
							return false;
						}
						break;
					case TwitchAction.SpecialRequirements.NotAllDaySetting:
						if (SkyManager.isAllTimeDay || SkyManager.isAllTimeDay)
						{
							return false;
						}
						break;
					case TwitchAction.SpecialRequirements.IsCooldown:
						if (!TwitchManager.HasInstance || !twitchManager.IsReady)
						{
							return false;
						}
						if (twitchManager.CurrentCooldownPreset.CooldownType != CooldownPreset.CooldownTypes.Fill)
						{
							return false;
						}
						if (twitchManager.CooldownType != TwitchManager.CooldownTypes.MaxReached)
						{
							return false;
						}
						break;
					case TwitchAction.SpecialRequirements.InLandClaim:
						if (!twitchManager.LocalPlayerInLandClaim)
						{
							return false;
						}
						break;
					case TwitchAction.SpecialRequirements.NotInLandClaim:
						if (twitchManager.LocalPlayerInLandClaim)
						{
							return false;
						}
						break;
					case TwitchAction.SpecialRequirements.Safe:
						if (!twitchManager.LocalPlayer.TwitchSafe)
						{
							return false;
						}
						break;
					case TwitchAction.SpecialRequirements.NotSafe:
						if (twitchManager.LocalPlayer.TwitchSafe)
						{
							return false;
						}
						break;
					case TwitchAction.SpecialRequirements.NoFullProgression:
						if (!twitchManager.IsReady)
						{
							return false;
						}
						if (!twitchManager.UseProgression || twitchManager.OverrideProgession)
						{
							return false;
						}
						break;
					case TwitchAction.SpecialRequirements.NotOnVehicle:
						if (!twitchManager.IsReady)
						{
							return false;
						}
						if (twitchManager.LocalPlayer.AttachedToEntity != null)
						{
							return false;
						}
						break;
					case TwitchAction.SpecialRequirements.NotInTrader:
						if (twitchManager.LocalPlayer.IsInTrader)
						{
							return false;
						}
						break;
					case TwitchAction.SpecialRequirements.Encumbrance:
						if ((int)EffectManager.GetValue(PassiveEffects.CarryCapacity, null, 0f, twitchManager.LocalPlayer, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false) <= 30)
						{
							return false;
						}
						break;
					case TwitchAction.SpecialRequirements.WeatherGracePeriod:
						if (GameManager.Instance.World.GetWorldTime() <= 30000UL)
						{
							return false;
						}
						break;
					case TwitchAction.SpecialRequirements.NotOnQuest:
						if (QuestEventManager.Current.QuestBounds.width != 0f)
						{
							return false;
						}
						break;
					case TwitchAction.SpecialRequirements.OnQuest:
						if (QuestEventManager.Current.QuestBounds.width == 0f)
						{
							return false;
						}
						break;
					}
				}
			}
			bool flag = (!this.SingleDayUse && !this.RandomDaily) || this.AllowedDay == GameUtils.WorldTimeToDays(GameManager.Instance.World.worldTime);
			if (this.tempCooldown > 0f && twitchManager.CurrentUnityTime - this.tempCooldownSet < this.tempCooldown)
			{
				return false;
			}
			this.tempCooldown = 0f;
			this.tempCooldownSet = 0f;
			return twitchManager.CurrentUnityTime - this.lastUse >= this.ModifiedCooldown && flag;
		}

		public void UpdateModifiedCooldown(float modifier)
		{
			this.ModifiedCooldown = modifier * this.Cooldown;
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public virtual bool OnPerformAction(EntityPlayer Target, TwitchActionEntry entry)
		{
			return false;
		}

		public void AddCooldownAddition(TwitchActionCooldownAddition newCooldown)
		{
			if (this.CooldownAdditions == null)
			{
				this.CooldownAdditions = new List<TwitchActionCooldownAddition>();
			}
			this.CooldownAdditions.Add(newCooldown);
		}

		public void ResetCooldown(float currentUnityTime)
		{
			this.lastUse = currentUnityTime - this.ModifiedCooldown;
			this.tempCooldown = 0f;
			this.tempCooldownSet = 0f;
		}

		public void SetCooldown(float currentUnityTime, float newCooldownTime)
		{
			this.lastUse = currentUnityTime - this.ModifiedCooldown;
			this.tempCooldown = newCooldownTime;
			this.tempCooldownSet = currentUnityTime;
		}

		public string Name = "";

		public string Title = "";

		public string BaseCommand = "";

		public string Command = "";

		public string EventName = "";

		public List<string> CategoryNames = new List<string>();

		public int DefaultCost = 5;

		public int ModifiedCost = 5;

		public int CurrentCost = 5;

		public int StartGameStage;

		public string Description;

		public float OriginalCooldown;

		public float Cooldown;

		public bool IsPositive;

		public bool AddsToCooldown;

		public int CooldownAddAmount = -1;

		public bool CooldownBlocked;

		public bool WaitingBlocked;

		public bool OnCooldown;

		public bool Enabled = true;

		public bool SingleDayUse;

		public bool DelayNotify;

		public bool TwitchNotify = true;

		public bool PlayBitSound = true;

		public string RandomGroup = "";

		public string Replaces = "";

		[PublicizedFrom(EAccessModifier.Private)]
		public float lastUse;

		public int AllowedDay = -1;

		public int groupIndex;

		public float tempCooldownSet;

		public float tempCooldown;

		public bool IgnoreCooldown;

		public bool IgnoreDiscount;

		public float ModifiedCooldown;

		public List<TwitchActionCooldownAddition> CooldownAdditions;

		public bool ShowInActionList = true;

		public List<TwitchAction.SpecialRequirements> SpecialRequirementList;

		public List<BaseTwitchRequirement> TwitchRequirements;

		public bool HideOnDisable;

		public TwitchAction.OnlyUsableTypes OnlyUsableByType;

		public string[] OnlyUsableBy;

		public bool OriginalEnabled = true;

		public TwitchAction.PointTypes PointType;

		public TwitchActionManager.ActionCategory MainCategory;

		public TwitchActionManager.ActionCategory DisplayCategory;

		public List<string> PresetNames;

		public bool OnlyShowInPreset;

		public float VoteCooldownAddition;

		public bool StreamerOnly;

		public TwitchAction.RespawnCountTypes RespawnCountType;

		public int MinRespawnCount = -1;

		public int MaxRespawnCount = -1;

		public int RespawnThreshold;

		public DynamicProperties Properties;

		public static string PropCommand = "command";

		public static string PropCommandKey = "command_key";

		public static string PropTitle = "title";

		public static string PropTitleKey = "title_key";

		public static string PropCategory = "category";

		public static string PropDisplayCategory = "display_category";

		public static string PropEventName = "event_name";

		public static string PropDefaultCost = "default_cost";

		public static string PropDescription = "description";

		public static string PropDescriptionKey = "description_key";

		public static string PropStartGameStage = "start_gamestage";

		public static string PropCooldown = "cooldown";

		public static string PropIsPositive = "is_positive";

		public static string PropSpecialOnly = "special_only";

		public static string PropAddCooldown = "add_cooldown";

		public static string PropCooldownAddAmount = "cooldown_add_amount";

		public static string PropCooldownBlocked = "cooldown_blocked";

		public static string PropWaitingBlocked = "waiting_blocked";

		public static string PropEnabled = "enabled";

		public static string PropSingleDayUse = "single_day";

		public static string PropRandomGroup = "random_group";

		public static string PropReplaces = "replaces";

		public static string PropShowInActionList = "show_in_action_list";

		public static string PropPlayBitSound = "play_bit_sound";

		public static string PropSpecialRequirement = "special_requirement";

		public static string PropTwitchNotify = "twitch_notify";

		public static string PropDelayNotify = "delay_notify";

		public static string PropHideOnDisable = "hide_on_disable";

		public static string PropOnlyUsableByType = "only_usable_type";

		public static string PropOnlyUsableBy = "only_usable_by";

		public static string PropIgnoreCooldown = "ignore_cooldown";

		public static string PropIgnoreDiscount = "ignore_discount";

		public static string PropPointTypes = "point_type";

		public static string PropPresets = "presets";

		public static string PropOnlyShowInPreset = "only_show_in_preset";

		public static string PropVoteCooldownAddition = "vote_cooldown_add";

		public static string PropStreamerOnly = "streamer_only";

		public static string PropMinRespawnCount = "min_respawn_count";

		public static string PropMaxRespawnCount = "max_respawn_count";

		public static string PropRespawnCountType = "respawn_count_type";

		public static string PropRespawnThreshold = "respawn_threshold";

		public static HashSet<string> ExtendsExcludes = new HashSet<string>
		{
			TwitchAction.PropShowInActionList,
			TwitchAction.PropCommandKey,
			TwitchAction.PropCommand,
			TwitchAction.PropTitleKey,
			TwitchAction.PropDescriptionKey
		};

		[PublicizedFrom(EAccessModifier.Private)]
		public static List<int> bitPrices = new List<int>
		{
			10, 15, 20, 25, 30, 35, 40, 45, 50, 55,
			60, 65, 70, 75, 80, 85, 90, 95, 100, 105,
			110, 115, 120, 125, 130, 135, 140, 145, 150, 155,
			160, 165, 170, 175, 180, 185, 190, 195, 200, 250,
			300, 350, 400, 450, 500, 550, 600, 650, 700, 750,
			800, 850, 900, 950, 1000, 1100, 1200, 1250, 1300, 1400,
			1500, 1600, 1700, 1750, 1800, 1900, 2000, 2250, 2500, 2750,
			3000, 3500, 4000, 4500, 5000, 5500, 6000, 6500, 7000, 7500,
			8000, 8500, 9000, 9500, 10000
		};

		public enum SpecialRequirements
		{
			None,
			HasSpawnedEntities,
			NoSpawnedEntities,
			Bloodmoon,
			NotBloodmoon,
			NotBloodmoonDay,
			EarlyDay,
			Daytime,
			Night,
			NotAllDaySetting,
			IsCooldown,
			InLandClaim,
			NotInLandClaim,
			Safe,
			NotSafe,
			NoFullProgression,
			NotOnVehicle,
			NotInTrader,
			Encumbrance,
			WeatherGracePeriod,
			NotOnQuest,
			OnQuest
		}

		public enum OnlyUsableTypes
		{
			Everyone,
			Broadcaster,
			Mods,
			VIPs,
			Subs,
			Name
		}

		public enum PointTypes
		{
			PP,
			SP,
			Bits
		}

		public enum RespawnCountTypes
		{
			None,
			Both,
			BlocksOnly,
			SpawnsOnly
		}
	}
}
