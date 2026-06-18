using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Audio;
using UnityEngine;
using UnityEngine.Scripting;

namespace SandboxOptions
{
	[Preserve]
	public class SandboxOptionManager
	{
		public static SandboxOptionManager Current
		{
			get
			{
				if (SandboxOptionManager.instance == null)
				{
					SandboxOptionManager.instance = new SandboxOptionManager();
				}
				return SandboxOptionManager.instance;
			}
		}

		public static bool HasInstance
		{
			get
			{
				return SandboxOptionManager.instance != null;
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public SandboxOptionManager()
		{
		}

		public void Cleanup()
		{
			SandboxOptionManager.instance = null;
		}

		public byte CurrentFileVersion { get; set; }

		public bool IsInit
		{
			get
			{
				return this.initRun;
			}
		}

		public void Init()
		{
			SandboxOptionManager.originalGravity = Physics.gravity;
			if (!this.initRun)
			{
				this.SetupOptions();
				this.LoadPresets();
				this.initRun = true;
				return;
			}
			Log.Warning("SandboxOptionManager Init called when it's already init");
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void SetupOptions()
		{
			this.ValueSets.Add("DamageValues", new SandboxOptionValueSetFloat
			{
				DisplayValues = new string[] { "none" },
				FloatValues = new float[]
				{
					0f, 0.25f, 0.35f, 0.5f, 0.65f, 0.75f, 0.85f, 1f, 1.25f, 1.5f,
					2f, 2.5f, 3f
				},
				DisplayFormat = "goPercent"
			});
			this.ValueSets.Add("DamageValuesNoNone", new SandboxOptionValueSetFloat
			{
				FloatValues = new float[]
				{
					0.25f, 0.35f, 0.5f, 0.65f, 0.75f, 0.85f, 1f, 1.25f, 1.5f, 2f,
					2.5f, 3f
				},
				DisplayFormat = "goPercent"
			});
			this.ValueSets.Add("PlayerSpeedValues", new SandboxOptionValueSetFloat
			{
				DisplayValues = new string[] { "none" },
				FloatValues = new float[]
				{
					0f, 0.25f, 0.5f, 0.6f, 0.7f, 0.8f, 0.9f, 1f, 1.1f, 1.2f,
					1.3f, 1.4f, 1.5f, 2f, 3f
				},
				DisplayFormat = "goPercent"
			});
			this.ValueSets.Add("PlayerSpeedValuesWithNone", new SandboxOptionValueSetFloat
			{
				FloatValues = new float[]
				{
					0.25f, 0.5f, 0.6f, 0.7f, 0.8f, 0.9f, 1f, 1.1f, 1.2f, 1.3f,
					1.4f, 1.5f, 2f, 3f
				},
				DisplayFormat = "goPercent"
			});
			this.ValueSets.Add("SpeedValues", new SandboxOptionValueSetFloat
			{
				DisplayValues = new string[] { "none" },
				FloatValues = new float[] { 0f, 0.25f, 0.5f, 0.75f, 1f, 1.25f, 1.5f, 2f, 3f },
				DisplayFormat = "goPercent"
			});
			this.ValueSets.Add("StaminaUsage", new SandboxOptionValueSetFloat
			{
				DisplayValues = new string[] { "none" },
				FloatValues = new float[] { 0f, 0.25f, 0.5f, 0.75f, 1f, 1.25f, 1.5f, 1.75f, 2f },
				DisplayFormat = "goPercent"
			});
			this.ValueSets.Add("LootAbundanceValues", new SandboxOptionValueSetFloat
			{
				DisplayValues = new string[] { "none" },
				FloatValues = new float[]
				{
					0f, 0.25f, 0.35f, 0.5f, 0.65f, 0.75f, 0.85f, 1f, 1.25f, 1.5f,
					2f, 3f, 4f, 5f
				},
				DisplayFormat = "goPercent"
			});
			this.ValueSets.Add("ZombieRageChance", new SandboxOptionValueSetFloat
			{
				DisplayValues = new string[] { "none" },
				FloatValues = new float[] { 0f, 0.15f, 0.3f, 0.35f, 0.4f, 0.5f, 0.6f, 0.75f, 0.9f, 1f },
				DisplayFormat = "goPercent"
			});
			this.ValueSets.Add("ZombieSpeeds", new SandboxOptionValueSetInt
			{
				DisplayValues = new string[] { "goZMWalk", "goZMJog", "goZMRun", "goZMSprint", "goZMNightmare" },
				IntValues = new int[] { 0, 1, 2, 3, 4 }
			});
			this.ValueSets.Add("AISmellMode", new SandboxOptionValueSetInt
			{
				DisplayValues = new string[] { "none", "goZMWalk", "goZMJog", "goZMRun", "goZMSprint", "goZMNightmare" },
				IntValues = new int[] { 0, 1, 2, 3, 4, 5 }
			});
			this.ValueSets.Add("JumpStrength", new SandboxOptionValueSetFloat
			{
				DisplayValues = new string[] { "none" },
				FloatValues = new float[] { 0f, 0.5f, 1f, 1.25f, 1.5f, 2f, 3f },
				DisplayFormat = "goPercent"
			});
			this.ValueSets.Add("StaminaRegen", new SandboxOptionValueSetFloat
			{
				FloatValues = new float[] { 0.25f, 0.5f, 0.75f, 1f, 1.25f, 1.5f, 2f, 3f },
				DisplayFormat = "goPercent"
			});
			this.ValueSets.Add("XPGain", new SandboxOptionValueSetFloat
			{
				DisplayValues = new string[] { "none" },
				FloatValues = new float[]
				{
					0f, 0.25f, 0.5f, 0.75f, 1f, 1.25f, 1.5f, 1.75f, 2f, 3f,
					5f
				},
				DisplayFormat = "goPercent"
			});
			this.ValueSets.Add("JarRefund", new SandboxOptionValueSetFloat
			{
				DisplayValues = new string[] { "none" },
				FloatValues = new float[]
				{
					0f, 0.05f, 0.1f, 0.2f, 0.3f, 0.4f, 0.5f, 0.6f, 0.7f, 0.8f,
					0.9f, 1f
				},
				DisplayFormat = "goPercent"
			});
			this.ValueSets.Add("BarterValues", new SandboxOptionValueSetFloat
			{
				DisplayValues = new string[] { "none" },
				FloatValues = new float[]
				{
					0f, 0.25f, 0.5f, 0.75f, 1f, 1.25f, 1.5f, 1.75f, 2f, 3f,
					4f
				},
				DisplayFormat = "goPercent"
			});
			this.ValueSets.Add("DisabledLowDefaultHigh", new SandboxOptionValueSetFloat
			{
				DisplayValues = new string[] { "goDisabled", "goVeryLow", "goLow", "goDefault", "goHigh", "goVeryHigh" },
				FloatValues = new float[] { 0f, 0.25f, 0.5f, 1f, 1.5f, 2f }
			});
			this.ValueSets.Add("LowDefaultHigh", new SandboxOptionValueSetFloat
			{
				DisplayValues = new string[] { "goVeryLow", "goLow", "goDefault", "goHigh", "goVeryHigh" },
				FloatValues = new float[] { 0.25f, 0.5f, 1f, 1.5f, 2f }
			});
			this.ValueSets.Add("Encumbrance", new SandboxOptionValueSetFloat
			{
				DisplayValues = new string[] { "goDisabled", "goLow", "goDefault", "goHigh", "goVeryHigh", "xuiOptionsVideoTexQualityFull" },
				FloatValues = new float[] { 10f, 1.35f, 1f, 0.7f, 0.35f, 0f }
			});
			this.ValueSets.Add("SkillGainRate", new SandboxOptionValueSetInt
			{
				IntValues = new int[] { 1, 2, 3, 4, 5 }
			});
			this.ValueSets.Add("PointsPer", new SandboxOptionValueSetInt
			{
				DisplayValues = new string[] { "none" },
				IntValues = new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }
			});
			this.ValueSets.Add("StarterSkillPoints", new SandboxOptionValueSetInt
			{
				DisplayValues = new string[] { "none" },
				IntValues = new int[]
				{
					0, 1, 2, 3, 4, 5, 6, 7, 8, 9,
					10
				}
			});
			this.ValueSets.Add("BloodMoonFrequency", new SandboxOptionValueSetInt
			{
				DisplayValues = new string[] { "goDisabled", "goDay" },
				IntValues = new int[]
				{
					0, 1, 2, 3, 4, 5, 6, 7, 8, 9,
					10, 14, 20, 30
				},
				DisplayFormat = "goDays"
			});
			this.ValueSets.Add("BloodMoonRange", new SandboxOptionValueSetInt
			{
				DisplayValues = new string[] { "goDays", "goDay" },
				IntValues = new int[] { 0, 1, 2, 3, 4, 7, 10, 14, 20 },
				DisplayFormat = "goDays"
			});
			this.ValueSets.Add("BloodMoonWarning", new SandboxOptionValueSetInt
			{
				DisplayValues = new string[] { "goDisabled", "goMorning", "goEvening" },
				IntValues = new int[] { 0, 1, 2 }
			});
			this.ValueSets.Add("BloodMoonCount", new SandboxOptionValueSetInt
			{
				IntValues = new int[] { 4, 6, 8, 10, 12, 16, 24, 32, 64 },
				DisplayFormat = "goEnemies"
			});
			this.ValueSets.Add("AirDrops", new SandboxOptionValueSetInt
			{
				DisplayValues = new string[] { "goDisabled", "goAirDropValue" },
				AlternateDisplayValues = new string[] { "", "1", "1-3", "3", "3-7", "7", "1-7" },
				IntValues = new int[] { 0, 1, 2, 3, 4, 5, 6 },
				DisplayFormat = "goDays"
			});
			this.ValueSets.Add("AirDropRandomTime", new SandboxOptionValueSetInt
			{
				DisplayValues = new string[] { "none", "goMorning", "goMidDayOnly", "goEvening", "goNightOnly", "goAllDay", "goAnyValue" },
				IntValues = new int[] { 0, 1, 2, 3, 4, 5, 6 }
			});
			this.ValueSets.Add("StormFrequency", new SandboxOptionValueSetFloat
			{
				DisplayValues = new string[] { "none" },
				FloatValues = new float[] { 0f, 0.5f, 1f, 1.5f, 2f, 3f, 4f, 5f },
				DisplayFormat = "goPercent"
			});
			this.ValueSets.Add("QuestPerTier", new SandboxOptionValueSetInt
			{
				DisplayValues = new string[] { "none" },
				IntValues = new int[]
				{
					0, 1, 2, 3, 4, 5, 6, 7, 8, 9,
					10, 11, 12, 13, 14, 15
				}
			});
			this.ValueSets.Add("QuestPerDay", new SandboxOptionValueSetInt
			{
				DisplayValues = new string[] { "goUnlimited" },
				IntValues = new int[]
				{
					-1, 1, 2, 3, 4, 5, 6, 7, 8, 9,
					10
				}
			});
			this.ValueSets.Add("TraderArea", new SandboxOptionValueSetInt
			{
				DisplayValues = new string[] { "xuiYes", "goClaimable", "goNotClaimable" },
				IntValues = new int[] { 0, 1, 2 }
			});
			this.ValueSets.Add("TraderResetInterval", new SandboxOptionValueSetInt
			{
				DisplayValues = new string[] { "xuiDefault", "goDay" },
				IntValues = new int[] { -1, 1, 2, 3, 4, 5, 6, 7, 14 },
				DisplayFormat = "goDays"
			});
			this.ValueSets.Add("ItemTierOptions", new SandboxOptionValueSetInt
			{
				IntValues = new int[] { 1, 2, 3, 4, 5, 6 }
			});
			this.ValueSets.Add("DewCollectorInput", new SandboxOptionValueSetFloat
			{
				DisplayValues = new string[] { "none" },
				FloatValues = new float[] { 0f, 1f, 2f, 3f },
				DisplayFormat = "goPercent"
			});
			this.ValueSets.Add("ApiaryInput", new SandboxOptionValueSetFloat
			{
				DisplayValues = new string[] { "none" },
				FloatValues = new float[] { 0f, 0.2f, 0.4f, 0.6f, 0.8f, 1f, 1.5f, 2f, 3f },
				DisplayFormat = "goPercent"
			});
			this.ValueSets.Add("CollectorOutput", new SandboxOptionValueSetFloat
			{
				FloatValues = new float[] { 1f, 2f, 3f, 4f, 5f },
				DisplayFormat = "goPercent"
			});
			this.ValueSets.Add("BackpackCrafting", new SandboxOptionValueSetInt
			{
				DisplayValues = new string[] { "xuiNo", "xuiYes", "goLimited", "goWorkbenchOnly" },
				IntValues = new int[] { 0, 1, 2, 3 }
			});
			this.ValueSets.Add("DeathPenalty", new SandboxOptionValueSetInt
			{
				DisplayValues = new string[] { "none", "goXPOnly", "goInjured", "goPermaDeath" },
				IntValues = new int[] { 0, 1, 2, 3 }
			});
			this.ValueSets.Add("DropOnDeath", new SandboxOptionValueSetInt
			{
				DisplayValues = new string[] { "none", "lblAll", "goToolbelt", "goBackpack", "goDeleteAll" },
				IntValues = new int[] { 0, 1, 2, 3, 4 }
			});
			this.ValueSets.Add("DropOnQuit", new SandboxOptionValueSetInt
			{
				DisplayValues = new string[] { "none", "lblAll", "goToolbelt", "goBackpack" },
				IntValues = new int[] { 0, 1, 2, 3 }
			});
			this.ValueSets.Add("LoseItemsOnDeathType", new SandboxOptionValueSetInt
			{
				DisplayValues = new string[] { "none", "lblAll", "goToolbelt", "goBackpack", "goEquipment", "goCarriedOnly" },
				IntValues = new int[] { 0, 1, 2, 3, 4, 5 }
			});
			this.ValueSets.Add("DegradeItemsOnDeath", new SandboxOptionValueSetInt
			{
				DisplayValues = new string[] { "none", "xuiDurability", "xuiMaxDurability", "xuiBoth" },
				IntValues = new int[] { 0, 1, 2, 3 }
			});
			this.ValueSets.Add("TraderHourPresets", new SandboxOptionValueSetInt
			{
				DisplayValues = new string[] { "xuiDefault", "goMorning", "goMidDayOnly", "goEvening", "goNightOnly", "goOnlyClosedOnBM", "goAlwaysOpen" },
				IntValues = new int[] { 0, 1, 2, 3, 4, 5, 6 }
			});
			this.ValueSets.Add("YesNo", new SandboxOptionValueSetBool
			{
				DisplayValues = new string[] { "xuiNo", "xuiYes" },
				BoolValues = new bool[]
				{
					default(bool),
					true
				}
			});
			this.ValueSets.Add("Celebrate", new SandboxOptionValueSetInt
			{
				DisplayValues = new string[] { "xuiNo", "xuiYes", "goHeadshotOnly" },
				IntValues = new int[] { 0, 1, 2 }
			});
			this.ValueSets.Add("ShowXP", new SandboxOptionValueSetInt
			{
				DisplayValues = new string[] { "lblAll", "goBarOnly", "goNotificationsOnly", "none" },
				IntValues = new int[] { 0, 1, 2, 3 }
			});
			this.ValueSets.Add("HeadshotMode", new SandboxOptionValueSetInt
			{
				DisplayValues = new string[] { "none", "goHeadshotOnly", "goHeadshotFinisher" },
				IntValues = new int[] { 0, 1, 2 }
			});
			this.ValueSets.Add("MaxEnemyType", new SandboxOptionValueSetInt
			{
				DisplayValues = new string[] { "goNormals", "goStrongs", "goSpecials", "goFerals", "goRadiated", "goElites" },
				IntValues = new int[] { 0, 1, 2, 3, 4, 5 }
			});
			this.ValueSets.Add("MaxTechType", new SandboxOptionValueSetInt
			{
				DisplayValues = new string[] { "none", "goTech0", "goTech1", "goTech2", "goTech3" },
				IntValues = new int[] { 0, 1, 2, 3, 4 }
			});
			this.ValueSets.Add("LoseItemCount", new SandboxOptionValueSetInt
			{
				DisplayValues = new string[] { "1-3", "1-5", "1-10", "1-20", "3-5", "5-7", "5-10", "7-10", "10-15", "15-20" },
				IntValues = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }
			});
			this.ValueSets.Add("DayNightLength", new SandboxOptionValueSetInt
			{
				IntValues = new int[] { 10, 20, 30, 40, 50, 60, 90, 120 }
			});
			this.ValueSets.Add("DayLightLength", new SandboxOptionValueSetInt
			{
				DisplayValues = new string[]
				{
					"goAlwaysNight", "4", "6", "8", "10", "12", "14", "16", "18", "20",
					"goAlwaysDay"
				},
				IntValues = new int[]
				{
					0, 4, 6, 8, 10, 12, 14, 16, 18, 20,
					24
				}
			});
			this.ValueSets.Add("LootRespawnDays", new SandboxOptionValueSetInt
			{
				DisplayValues = new string[] { "goDisabled" },
				IntValues = new int[] { -1, 5, 7, 10, 15, 20, 30, 40, 50 }
			});
			this.ValueSets.Add("MaxChunkAge", new SandboxOptionValueSetInt
			{
				DisplayValues = new string[] { "goDisabled" },
				IntValues = new int[]
				{
					-1, 1, 3, 5, 7, 10, 20, 30, 40, 50,
					75, 100
				}
			});
			this.ValueSets.Add("Gravity", new SandboxOptionValueSetFloat
			{
				FloatValues = new float[] { 0.5f, 0.6f, 0.7f, 0.8f, 0.9f, 1f },
				DisplayFormat = "goPercent"
			});
			this.ValueSets.Add("SlowToFast", new SandboxOptionValueSetInt
			{
				DisplayValues = new string[] { "xuiDefault", "goVerySlow", "goSlow", "goNormal", "goFast", "goVeryFast" },
				IntValues = new int[] { 0, 1, 2, 3, 4, 5 }
			});
			this.ValueSets.Add("BiomeEnemyDensity", new SandboxOptionValueSetInt
			{
				DisplayValues = new string[] { "xuiDefault", "goVeryLow", "goLow", "goMedium", "goHigh", "goVeryHigh" },
				IntValues = new int[] { 0, 1, 2, 3, 4, 5 }
			});
			this.ValueSets.Add("SmeltingType", new SandboxOptionValueSetBool
			{
				DisplayValues = new string[] { "goSmelter", "lblContextActionRecipes" },
				BoolValues = new bool[]
				{
					default(bool),
					true
				}
			});
			this.ValueSets.Add("RepairTypes", new SandboxOptionValueSetInt
			{
				DisplayValues = new string[] { "none", "goRepairOnly", "goCombineOnly", "xuiBoth" },
				IntValues = new int[] { 0, 1, 2, 3 }
			});
			this.ValueSets.Add("MaxDegradationAmounts", new SandboxOptionValueSetFloat
			{
				DisplayValues = new string[] { "none" },
				FloatValues = new float[] { 0f, 0.05f, 0.1f, 0.15f, 0.2f, 0.25f },
				DisplayFormat = "goPercent"
			});
			this.ValueSets.Add("CropGrowthSpeed", new SandboxOptionValueSetFloat
			{
				DisplayValues = new string[] { "none", "xuiInstant" },
				FloatValues = new float[] { 1000f, 0f, 0.2f, 0.5f, 0.75f, 1f, 1.5f, 2f, 3f },
				DisplayFormat = "goPercent"
			});
			this.ValueSets.Add("ZombieFeralSense", new SandboxOptionValueSetInt
			{
				DisplayValues = new string[] { "goDisabled", "goZMDay", "goZMNight", "goZMAll" },
				IntValues = new int[] { 0, 1, 2, 3 }
			});
			this.AddSandboxOption(new SandboxOptionFloat(SandboxOptions.RangedDamage, "Ranged Damage", "General", "DamageValues", 1f, false, null));
			this.AddSandboxOption(new SandboxOptionFloat(SandboxOptions.MeleeDamage, "Melee Damage", "General", "DamageValues", 1f, false, null));
			this.AddSandboxOption(new SandboxOptionFloat(SandboxOptions.BlockDamage, "Block Damage", "General", "DamageValues", 1f, false, null)
			{
				OverrideOptionName = "goBlockDamagePlayer",
				OverrideDescriptionName = "goBlockDamagePlayerDesc"
			});
			this.AddSandboxOption(new SandboxOptionFloat(SandboxOptions.TerrainDamage, "Terrain Damage", "General", "DamageValues", 1f, false, null));
			this.AddSandboxOption(new SandboxOptionFloat(SandboxOptions.HeadshotMultiplier, "Headshot Multiplier", "General", "DamageValues", 1f, false, null));
			this.AddSandboxOption(new SandboxOptionFloat(SandboxOptions.IncomingDamage, "Incoming Damage", "General", "DamageValues", 1f, false, null));
			this.AddSandboxOption(new SandboxOptionFloat(SandboxOptions.WalkSpeed, "Walk Speed", "General", "PlayerSpeedValuesWithNone", 1f, true, null));
			this.AddSandboxOption(new SandboxOptionFloat(SandboxOptions.RunSpeed, "Run Speed", "General", "PlayerSpeedValues", 1f, false, null));
			this.AddSandboxOption(new SandboxOptionFloat(SandboxOptions.CrouchSpeed, "Crouch Speed", "General", "PlayerSpeedValues", 1f, false, null));
			this.AddSandboxOption(new SandboxOptionFloat(SandboxOptions.JumpStrength, "Jump Height", "General", "JumpStrength", 1f, false, null));
			SandboxOptionFloat.DisabledOptionsOnValue disabledOptionsOnValue = new SandboxOptionFloat.DisabledOptionsOnValue(new SandboxOptions[] { SandboxOptions.StaminaRegen }, 0f, false);
			this.AddSandboxOption(new SandboxOptionFloat(SandboxOptions.StaminaRegen, "Stamina Regen", "General", "StaminaRegen", 1f, false, null));
			this.AddSandboxOption(new SandboxOptionFloat(SandboxOptions.StaminaUsage, "Stamina Usage", "General", "StaminaUsage", 1f, false, disabledOptionsOnValue));
			this.AddSandboxOption(new SandboxOptionFloat(SandboxOptions.XPMultiplier, "XP Multiplier", "General", "XPGain", 1f, true, null));
			this.AddSandboxOption(new SandboxOptionInt(SandboxOptions.ShowXP, "Show XP", "General", "ShowXP", 0, false, null));
			SandboxOptionInt.DisabledOptionsOnValue disabledOptionsOnValue2 = new SandboxOptionInt.DisabledOptionsOnValue(new SandboxOptions[] { SandboxOptions.SkillPointsPerLevel }, 0, false);
			this.AddSandboxOption(new SandboxOptionBoolean(SandboxOptions.PlayerLevelBonusApplied, "Level Health/Stam Bonus", "General", "YesNo", true, false, null));
			this.AddSandboxOption(new SandboxOptionInt(SandboxOptions.SkillGainRate, "Skill Gain Rate", "General", "SkillGainRate", 1, false, disabledOptionsOnValue2));
			this.AddSandboxOption(new SandboxOptionInt(SandboxOptions.SkillPointsPerLevel, "Skill Gain Amount", "General", "PointsPer", 1, false, null));
			disabledOptionsOnValue2 = new SandboxOptionInt.DisabledOptionsOnValue(new SandboxOptions[] { SandboxOptions.LoseItemsOnDeathCount }, 0, false);
			this.AddSandboxOption(new SandboxOptionInt(SandboxOptions.DeathPenalty, "Death Penalty", "General", "DeathPenalty", 1, true, null));
			this.AddSandboxOption(new SandboxOptionInt(SandboxOptions.LoseItemsOnDeathType, "Lose Items Death Type", "General", "LoseItemsOnDeathType", 0, false, null));
			this.AddSandboxOption(new SandboxOptionInt(SandboxOptions.LoseItemsOnDeathCount, "Lose Items Death Count", "General", "LoseItemCount", 1, false, null));
			this.AddSandboxOption(new SandboxOptionInt(SandboxOptions.DegradeItemsOnDeath, "Degrade Items On Death", "General", "DegradeItemsOnDeath", 0, false, null));
			this.AddSandboxOption(new SandboxOptionFloat(SandboxOptions.DegradeAmountOnDeath, "Degrade Amount On Death", "General", "MaxDegradationAmounts", 0.1f, false, null));
			this.AddSandboxOption(new SandboxOptionInt(SandboxOptions.DropOnDeath, "Drop On Death", "General", "DropOnDeath", 1, false, null));
			this.AddSandboxOption(new SandboxOptionInt(SandboxOptions.DropOnQuit, "Drop On Quit", "General", "DropOnQuit", 0, false, null));
			this.AddSandboxOption(new SandboxOptionFloat(SandboxOptions.InfectionRate, "Infection Rate", "General", "SpeedValues", 1f, true, null));
			this.AddSandboxOption(new SandboxOptionBoolean(SandboxOptions.NewbieCoat, "Allow Newbie Coat", "General", "YesNo", true, false, null));
			this.AddSandboxOption(new SandboxOptionFloat(SandboxOptions.EncumbranceModifier, "Encumbrance Modifier", "General", "Encumbrance", 1f, false, null));
			this.AddSandboxOption(new SandboxOptionFloat(SandboxOptions.JarRefund, "Jar Refund", "General", "JarRefund", 0.6f, false, null));
			SandboxOptionBoolean.DisabledOptionsOnValue disabledOptionsOnValue3 = new SandboxOptionBoolean.DisabledOptionsOnValue(new SandboxOptions[]
			{
				SandboxOptions.MaxEnemyTier,
				SandboxOptions.BiomeEnemyDensity,
				SandboxOptions.BiomeZombieRespawn,
				SandboxOptions.BiomeAnimalRespawn,
				SandboxOptions.EntityDamage,
				SandboxOptions.EntityIncomingDamage,
				SandboxOptions.BlockDamageAI,
				SandboxOptions.BlockDamageAIBM,
				SandboxOptions.ZombieMove,
				SandboxOptions.ZombieMoveNight,
				SandboxOptions.ZombieFeralMove,
				SandboxOptions.ZombieBMMove,
				SandboxOptions.ZombieFeralSense,
				SandboxOptions.AISmellMode,
				SandboxOptions.ZombieRageChance,
				SandboxOptions.AllowZombieDigging,
				SandboxOptions.ZombiesEatAnimals,
				SandboxOptions.HeadshotMode
			}, false, false);
			this.AddSandboxOption(new SandboxOptionBoolean(SandboxOptions.EnemySpawnMode, "Enemy Spawn", "Entities", "YesNo", true, false, disabledOptionsOnValue3));
			this.AddSandboxOption(new SandboxOptionInt(SandboxOptions.MaxEnemyTier, "Max Enemy Type", "Entities", "MaxEnemyType", 5, false, null));
			this.AddSandboxOption(new SandboxOptionInt(SandboxOptions.BiomeEnemyDensity, "Biome Enemy Density", "Entities", "BiomeEnemyDensity", 0, false, null));
			this.AddSandboxOption(new SandboxOptionInt(SandboxOptions.BiomeZombieRespawn, "Biome Enemy Respawn", "Entities", "SlowToFast", 0, false, null));
			this.AddSandboxOption(new SandboxOptionInt(SandboxOptions.BiomeAnimalRespawn, "Biome Animal Respawn", "Entities", "SlowToFast", 0, false, null));
			this.AddSandboxOption(new SandboxOptionFloat(SandboxOptions.EntityDamage, "Entity Damage", "Entities", "DamageValues", 1f, true, null));
			this.AddSandboxOption(new SandboxOptionFloat(SandboxOptions.EntityIncomingDamage, "Entity Incoming Damage", "Entities", "DamageValuesNoNone", 1f, false, null));
			this.AddSandboxOption(new SandboxOptionFloat(SandboxOptions.BlockDamageAI, "Entity Block Damage", "Entities", "DamageValues", 1f, false, null));
			this.AddSandboxOption(new SandboxOptionFloat(SandboxOptions.BlockDamageAIBM, "Blood Moon Block Damage", "Entities", "DamageValues", 1f, false, null));
			this.AddSandboxOption(new SandboxOptionInt(SandboxOptions.HeadshotMode, "Headshot Mode", "Entities", "HeadshotMode", 0, false, null));
			this.AddSandboxOption(new SandboxOptionBoolean(SandboxOptions.ShowHealthBars, "Entity Health Bars", "Entities", "YesNo", false, false, null));
			this.AddSandboxOption(new SandboxOptionBoolean(SandboxOptions.ShowEnemyDamage, "Show Entity Damage", "Entities", "YesNo", false, false, null));
			this.AddSandboxOption(new SandboxOptionInt(SandboxOptions.ZombieMove, "Zombie Day Speed", "Entities", "ZombieSpeeds", 0, true, null));
			this.AddSandboxOption(new SandboxOptionInt(SandboxOptions.ZombieMoveNight, "Zombie Night Speed", "Entities", "ZombieSpeeds", 3, false, null));
			this.AddSandboxOption(new SandboxOptionInt(SandboxOptions.ZombieFeralMove, "Zombie Feral Speed", "Entities", "ZombieSpeeds", 3, false, null));
			this.AddSandboxOption(new SandboxOptionInt(SandboxOptions.ZombieBMMove, "Zombie Blood Moon Speed", "Entities", "ZombieSpeeds", 3, false, null));
			this.AddSandboxOption(new SandboxOptionInt(SandboxOptions.ZombieFeralSense, "Zombie Feral Sense", "Entities", "ZombieFeralSense", 0, true, null));
			this.AddSandboxOption(new SandboxOptionInt(SandboxOptions.AISmellMode, "Zombie AI Smell Mode", "Entities", "AISmellMode", 3, false, null));
			this.AddSandboxOption(new SandboxOptionFloat(SandboxOptions.ZombieRageChance, "Zombie Rage Chance", "Entities", "ZombieRageChance", 0.15f, false, null));
			this.AddSandboxOption(new SandboxOptionBoolean(SandboxOptions.AllowZombieDigging, "Allow Zombie Digging", "Entities", "YesNo", true, false, null));
			this.AddSandboxOption(new SandboxOptionBoolean(SandboxOptions.ZombiesEatAnimals, "Zombies Eat Animals", "Entities", "YesNo", true, false, null));
			this.AddSandboxOption(new SandboxOptionFloat(SandboxOptions.GlobalGSModifier, "Global GameStage", "World", "LowDefaultHigh", 1f, false, null));
			this.AddSandboxOption(new SandboxOptionFloat(SandboxOptions.BiomeGSModifier, "Biome GameStage", "World", "LowDefaultHigh", 1f, false, null));
			this.AddSandboxOption(new SandboxOptionBoolean(SandboxOptions.BiomeProgression, "Biome Progression", "World", "YesNo", true, false, null));
			this.AddSandboxOption(new SandboxOptionBoolean(SandboxOptions.TemperatureSurvival, "Temperature Survival", "World", "YesNo", true, false, null));
			this.AddSandboxOption(new SandboxOptionInt(SandboxOptions.MaxTechType, "Max Tech Type", "World", "MaxTechType", 4, false, null));
			this.AddSandboxOption(new SandboxOptionFloat(SandboxOptions.WorkstationsInTheWild, "Workstations in the Wild", "World", "JarRefund", 0f, false, null));
			disabledOptionsOnValue2 = new SandboxOptionInt.DisabledOptionsOnValue(new SandboxOptions[]
			{
				SandboxOptions.BloodMoonRange,
				SandboxOptions.BloodMoonEnemyCount,
				SandboxOptions.BloodMoonWarning
			}, 0, false);
			this.AddSandboxOption(new SandboxOptionInt(SandboxOptions.BloodMoonFrequency, "Blood Moon Frequency", "World", "BloodMoonFrequency", 7, true, disabledOptionsOnValue2));
			this.AddSandboxOption(new SandboxOptionInt(SandboxOptions.BloodMoonRange, "Blood Moon Range", "World", "BloodMoonRange", 0, false, null));
			this.AddSandboxOption(new SandboxOptionInt(SandboxOptions.BloodMoonEnemyCount, "Blood Moon Count", "World", "BloodMoonCount", 8, false, null));
			this.AddSandboxOption(new SandboxOptionInt(SandboxOptions.BloodMoonWarning, "Blood Moon Warning", "World", "BloodMoonWarning", 1, false, null));
			disabledOptionsOnValue2 = new SandboxOptionInt.DisabledOptionsOnValue(new SandboxOptions[]
			{
				SandboxOptions.AirDropRandomTime,
				SandboxOptions.AirDropMarker
			}, 0, false);
			this.AddSandboxOption(new SandboxOptionInt(SandboxOptions.AirDropFrequency, "Air Drops", "World", "AirDrops", 3, false, disabledOptionsOnValue2));
			this.AddSandboxOption(new SandboxOptionInt(SandboxOptions.AirDropRandomTime, "Air Drop Random Time", "World", "AirDropRandomTime", 0, false, null));
			disabledOptionsOnValue = new SandboxOptionFloat.DisabledOptionsOnValue(new SandboxOptions[] { SandboxOptions.StormWarning }, 0f, false);
			this.AddSandboxOption(new SandboxOptionFloat(SandboxOptions.StormFreq, "Storm Frequency", "World", "StormFrequency", 1f, true, disabledOptionsOnValue));
			this.AddSandboxOption(new SandboxOptionBoolean(SandboxOptions.StormWarning, "Storm Warning", "World", "YesNo", true, false, null));
			this.AddSandboxOption(new SandboxOptionFloat(SandboxOptions.HeatMapSensitivity, "Heatmap Sensitivity", "World", "DisabledLowDefaultHigh", 1f, false, null));
			this.AddSandboxOption(new SandboxOptionInt(SandboxOptions.DayNightLength, "24 Day Cycle", "World", "DayNightLength", 60, false, null));
			this.AddSandboxOption(new SandboxOptionInt(SandboxOptions.DayLightLength, "Day Light Length", "World", "DayLightLength", 18, false, null));
			this.AddSandboxOption(new SandboxOptionBoolean(SandboxOptions.AirDropMarker, "Mark Air Drops", "World", "YesNo", true, true, null)
			{
				OverrideOptionName = "goMarkAirDrops",
				OverrideDescriptionName = "goMarkAirDropsDesc"
			});
			this.AddSandboxOption(new SandboxOptionBoolean(SandboxOptions.AllowMap, "Allow Map", "World", "YesNo", true, false, null));
			this.AddSandboxOption(new SandboxOptionBoolean(SandboxOptions.AllowCompass, "Allow Compass", "World", "YesNo", true, false, null));
			this.AddSandboxOption(new SandboxOptionBoolean(SandboxOptions.AllowScreenMarkers, "Allow Screen Markers", "World", "YesNo", true, false, null));
			this.AddSandboxOption(new SandboxOptionBoolean(SandboxOptions.ShowLocationInfo, "Show Location Info", "World", "YesNo", true, false, null));
			disabledOptionsOnValue3 = new SandboxOptionBoolean.DisabledOptionsOnValue(new SandboxOptions[] { SandboxOptions.BloodMoonWarning }, false, false);
			this.AddSandboxOption(new SandboxOptionBoolean(SandboxOptions.ShowDayTime, "Show Day/Time", "World", "YesNo", true, false, disabledOptionsOnValue3));
			this.AddSandboxOption(new SandboxOptionInt(SandboxOptions.LootMaxTier, "Loot Max Tier", "Resources", "ItemTierOptions", 6, false, null));
			this.AddSandboxOption(new SandboxOptionFloat(SandboxOptions.GlobalLSModifier, "Global LootStage", "Resources", "LowDefaultHigh", 1f, false, null));
			this.AddSandboxOption(new SandboxOptionFloat(SandboxOptions.BiomeLSModifier, "Biome LootStage", "Resources", "LowDefaultHigh", 1f, false, null));
			this.AddSandboxOption(new SandboxOptionFloat(SandboxOptions.POITierLSModifier, "POI Tier LootStage", "Resources", "LowDefaultHigh", 1f, false, null));
			this.AddSandboxOption(new SandboxOptionInt(SandboxOptions.LootRespawnDays, "Loot Respawn Days", "Resources", "LootRespawnDays", 7, true, null));
			this.AddSandboxOption(new SandboxOptionFloat(SandboxOptions.LootTimer, "Loot Time", "Resources", "SpeedValues", 1f, false, null));
			this.AddSandboxOption(new SandboxOptionFloat(SandboxOptions.LootBagChance, "Loot Bag Chance", "Resources", "LootAbundanceValues", 1f, false, null));
			disabledOptionsOnValue = new SandboxOptionFloat.DisabledOptionsOnValue(new SandboxOptions[]
			{
				SandboxOptions.FoodLootCount,
				SandboxOptions.DrinkLootCount,
				SandboxOptions.MedicalLootCount,
				SandboxOptions.AmmoLootCount,
				SandboxOptions.ResourceLootCount,
				SandboxOptions.ArmorLootCount,
				SandboxOptions.MeleeLootCount,
				SandboxOptions.RangedLootCount,
				SandboxOptions.DukesLootCount,
				SandboxOptions.CraftingMagazinesLootCount,
				SandboxOptions.TreasureMapChance
			}, 0f, false);
			this.AddSandboxOption(new SandboxOptionFloat(SandboxOptions.GlobalLootCount, "Global Loot Abundance", "Resources", "LootAbundanceValues", 1f, true, disabledOptionsOnValue)
			{
				OverrideOptionName = "goLootAbundance",
				OverrideDescriptionName = "goLootAbundanceDesc"
			});
			this.AddSandboxOption(new SandboxOptionFloat(SandboxOptions.FoodLootCount, "Food Abundance", "Resources", "LootAbundanceValues", 1f, false, null));
			this.AddSandboxOption(new SandboxOptionFloat(SandboxOptions.DrinkLootCount, "Drink Abundance", "Resources", "LootAbundanceValues", 1f, false, null));
			this.AddSandboxOption(new SandboxOptionFloat(SandboxOptions.MedicalLootCount, "Medical Abundance", "Resources", "LootAbundanceValues", 1f, false, null));
			this.AddSandboxOption(new SandboxOptionFloat(SandboxOptions.AmmoLootCount, "Ammo Abundance", "Resources", "LootAbundanceValues", 1f, false, null));
			this.AddSandboxOption(new SandboxOptionFloat(SandboxOptions.ResourceLootCount, "Resource Abundance", "Resources", "LootAbundanceValues", 1f, false, null));
			this.AddSandboxOption(new SandboxOptionFloat(SandboxOptions.ArmorLootCount, "Armor Abundance", "Resources", "LootAbundanceValues", 1f, false, null));
			this.AddSandboxOption(new SandboxOptionFloat(SandboxOptions.MeleeLootCount, "Melee Abundance", "Resources", "LootAbundanceValues", 1f, false, null));
			this.AddSandboxOption(new SandboxOptionFloat(SandboxOptions.RangedLootCount, "Ranged Abundance", "Resources", "LootAbundanceValues", 1f, false, null));
			this.AddSandboxOption(new SandboxOptionFloat(SandboxOptions.DukesLootCount, "Dukes Abundance", "Resources", "LootAbundanceValues", 1f, false, null));
			this.AddSandboxOption(new SandboxOptionFloat(SandboxOptions.CraftingMagazinesLootCount, "Magazines Abundance", "Resources", "LootAbundanceValues", 1f, false, null));
			this.AddSandboxOption(new SandboxOptionFloat(SandboxOptions.TreasureMapChance, "Treasure Map Chance", "Resources", "PlayerSpeedValues", 1f, false, null));
			this.AddSandboxOption(new SandboxOptionFloat(SandboxOptions.MiningOutput, "Mining Output", "Resources", "LootAbundanceValues", 1f, true, null));
			this.AddSandboxOption(new SandboxOptionFloat(SandboxOptions.CropOutput, "Crop Output", "Resources", "LootAbundanceValues", 1f, false, null));
			this.AddSandboxOption(new SandboxOptionFloat(SandboxOptions.SeedDropOutput, "Seed Drop Output", "Resources", "LootAbundanceValues", 1f, false, null));
			this.AddSandboxOption(new SandboxOptionFloat(SandboxOptions.HarvestingOutput, "Harvesting Output", "Resources", "LootAbundanceValues", 1f, false, null));
			this.AddSandboxOption(new SandboxOptionFloat(SandboxOptions.CropGrowthSpeed, "Crop Growth", "Resources", "CropGrowthSpeed", 1f, false, null));
			this.AddSandboxOption(new SandboxOptionBoolean(SandboxOptions.CraftingProgression, "Crafting Progression", "Crafting", "YesNo", true, false, null));
			this.AddSandboxOption(new SandboxOptionInt(SandboxOptions.CraftingMaxTier, "Crafting Max Tier", "Crafting", "ItemTierOptions", 6, false, null));
			this.AddSandboxOption(new SandboxOptionInt(SandboxOptions.PointsPerMagazine, "Magazine Points", "Crafting", "PointsPer", 1, false, null));
			this.AddSandboxOption(new SandboxOptionInt(SandboxOptions.BackpackCrafting, "Backpack Crafting", "Crafting", "BackpackCrafting", 1, false, null));
			this.AddSandboxOption(new SandboxOptionBoolean(SandboxOptions.WorkstationCrafting, "Workstation Crafting", "Crafting", "YesNo", true, false, null));
			this.AddSandboxOption(new SandboxOptionBoolean(SandboxOptions.SmeltingType, "Smelter Type", "Crafting", "SmeltingType", false, false, null));
			this.AddSandboxOption(new SandboxOptionFloat(SandboxOptions.CraftingTime, "Crafting Time", "Crafting", "SpeedValues", 1f, false, null));
			this.AddSandboxOption(new SandboxOptionFloat(SandboxOptions.CraftingInput, "Crafting Input", "Crafting", "SpeedValues", 1f, false, null));
			this.AddSandboxOption(new SandboxOptionFloat(SandboxOptions.CraftingOutput, "Crafting Output", "Crafting", "StaminaRegen", 1f, false, null));
			this.AddSandboxOption(new SandboxOptionFloat(SandboxOptions.ScrappingOutput, "Scrapping Output", "Crafting", "SpeedValues", 1f, false, null));
			this.AddSandboxOption(new SandboxOptionFloat(SandboxOptions.DewCollectorTime, "Dew Collector Time", "Crafting", "SpeedValues", 1f, true, null));
			this.AddSandboxOption(new SandboxOptionFloat(SandboxOptions.DewCollectorOutput, "Dew Collector Output", "Crafting", "CollectorOutput", 1f, false, null));
			this.AddSandboxOption(new SandboxOptionFloat(SandboxOptions.DewCollectorInput, "Dew Collector Input", "Crafting", "DewCollectorInput", 1f, false, null));
			this.AddSandboxOption(new SandboxOptionFloat(SandboxOptions.ApiaryTime, "Apiary Time", "Crafting", "SpeedValues", 1f, false, null));
			this.AddSandboxOption(new SandboxOptionFloat(SandboxOptions.ApiaryOutput, "Apiary Output", "Crafting", "CollectorOutput", 1f, false, null));
			this.AddSandboxOption(new SandboxOptionFloat(SandboxOptions.ApiaryInput, "Apiary Input", "Crafting", "ApiaryInput", 1f, false, null));
			this.AddSandboxOption(new SandboxOptionFloat(SandboxOptions.ItemDegradation, "Item Degradation", "Crafting", "DisabledLowDefaultHigh", 1f, true, null));
			this.AddSandboxOption(new SandboxOptionInt(SandboxOptions.RepairTypes, "Item Repair Types", "Crafting", "RepairTypes", 3, false, null));
			this.AddSandboxOption(new SandboxOptionFloat(SandboxOptions.MaxDegradationAmount, "Max Degrade Amount", "Crafting", "MaxDegradationAmounts", 0f, false, null));
			disabledOptionsOnValue3 = new SandboxOptionBoolean.DisabledOptionsOnValue(new SandboxOptions[]
			{
				SandboxOptions.TraderMaxTier,
				SandboxOptions.TraderItemAbundance,
				SandboxOptions.TraderResetInterval,
				SandboxOptions.TraderSellPrices,
				SandboxOptions.TraderBuyPrices,
				SandboxOptions.TraderBuyLimit,
				SandboxOptions.GlobalTSModifier
			}, false, false);
			this.AddSandboxOption(new SandboxOptionBoolean(SandboxOptions.TradersEnabled, "Trading Enabled", "Traders", "YesNo", true, false, disabledOptionsOnValue3));
			disabledOptionsOnValue3 = new SandboxOptionBoolean.DisabledOptionsOnValue(new SandboxOptions[]
			{
				SandboxOptions.VendingItemAbundance,
				SandboxOptions.VendingResetInterval
			}, false, false);
			this.AddSandboxOption(new SandboxOptionBoolean(SandboxOptions.VendingEnabled, "Vending Machines Enabled", "Traders", "YesNo", true, false, disabledOptionsOnValue3));
			this.AddSandboxOption(new SandboxOptionInt(SandboxOptions.TraderHours, "Trader Hours", "Traders", "TraderHourPresets", 0, false, null));
			this.AddSandboxOption(new SandboxOptionInt(SandboxOptions.TraderProtection, "Trader Protection", "Traders", "TraderArea", 0, false, null));
			this.AddSandboxOption(new SandboxOptionBoolean(SandboxOptions.TraderDialog, "Trading Dialog", "Traders", "YesNo", true, false, null));
			this.AddSandboxOption(new SandboxOptionFloat(SandboxOptions.GlobalTSModifier, "Global TraderStage", "Traders", "LowDefaultHigh", 1f, true, null));
			this.AddSandboxOption(new SandboxOptionInt(SandboxOptions.TraderMaxTier, "Trader Max Tier", "Traders", "ItemTierOptions", 6, false, null));
			this.AddSandboxOption(new SandboxOptionFloat(SandboxOptions.TraderItemAbundance, "Trader Item Abundance", "Traders", "LowDefaultHigh", 1f, false, null));
			this.AddSandboxOption(new SandboxOptionFloat(SandboxOptions.VendingItemAbundance, "Vending Item Abundance", "Traders", "LowDefaultHigh", 1f, false, null));
			this.AddSandboxOption(new SandboxOptionInt(SandboxOptions.TraderResetInterval, "Trader Reset Interval", "Traders", "TraderResetInterval", -1, false, null));
			this.AddSandboxOption(new SandboxOptionInt(SandboxOptions.VendingResetInterval, "Vending Reset Interval", "Traders", "TraderResetInterval", -1, false, null));
			this.AddSandboxOption(new SandboxOptionFloat(SandboxOptions.TraderSellPrices, "Traders Sell Price", "Traders", "BarterValues", 1f, false, null));
			this.AddSandboxOption(new SandboxOptionFloat(SandboxOptions.TraderBuyPrices, "Traders Buy Price", "Traders", "BarterValues", 1f, false, null));
			this.AddSandboxOption(new SandboxOptionInt(SandboxOptions.TraderBuyLimit, "Trader Buy Limit", "Traders", "StarterSkillPoints", 3, false, null));
			disabledOptionsOnValue3 = new SandboxOptionBoolean.DisabledOptionsOnValue(new SandboxOptions[] { SandboxOptions.IntroChallengesEnabled }, false, false);
			this.AddSandboxOption(new SandboxOptionBoolean(SandboxOptions.ChallengesEnabled, "Challenges Enabled", "Tasks", "YesNo", true, false, disabledOptionsOnValue3));
			disabledOptionsOnValue3 = new SandboxOptionBoolean.DisabledOptionsOnValue(new SandboxOptions[]
			{
				SandboxOptions.IntroQuestEnabled,
				SandboxOptions.TraderToTraderQuestsEnabled,
				SandboxOptions.BuriedQuestsEnabled,
				SandboxOptions.POIQuestsEnabled,
				SandboxOptions.QuestsPerTier,
				SandboxOptions.QuestProgressionDailyLimit
			}, false, false);
			this.AddSandboxOption(new SandboxOptionBoolean(SandboxOptions.QuestsEnabled, "Quests Enabled", "Tasks", "YesNo", true, false, null));
			this.AddSandboxOption(new SandboxOptionBoolean(SandboxOptions.IntroChallengesEnabled, "Intro Challenges Enabled", "Tasks", "YesNo", true, false, null));
			this.AddSandboxOption(new SandboxOptionBoolean(SandboxOptions.IntroQuestEnabled, "Intro Quest Enabled", "Tasks", "YesNo", true, false, null));
			this.AddSandboxOption(new SandboxOptionBoolean(SandboxOptions.TraderToTraderQuestsEnabled, "Trader to Trader Quests", "Tasks", "YesNo", true, false, null));
			this.AddSandboxOption(new SandboxOptionBoolean(SandboxOptions.BuriedQuestsEnabled, "Buried Quests Enabled", "Tasks", "YesNo", true, false, null));
			this.AddSandboxOption(new SandboxOptionBoolean(SandboxOptions.POIQuestsEnabled, "POI Quests Enabled", "Tasks", "YesNo", true, false, null));
			this.AddSandboxOption(new SandboxOptionInt(SandboxOptions.QuestsPerTier, "Quests per Tier", "Tasks", "QuestPerTier", 10, false, null));
			this.AddSandboxOption(new SandboxOptionInt(SandboxOptions.QuestProgressionDailyLimit, "Quests per Day", "Tasks", "QuestPerDay", 4, false, null));
			this.AddSandboxOption(new SandboxOptionInt(SandboxOptions.StarterSkillPoints, "Starter Skill Points", "Tasks", "StarterSkillPoints", 4, false, null));
			this.AddSandboxOption(new SandboxOptionFloat(SandboxOptions.VehicleFuelUsage, "Vehicle Fuel Usage", "Misc", "DamageValues", 1f, false, null));
			this.AddSandboxOption(new SandboxOptionFloat(SandboxOptions.VehicleEntityDamage, "Vehicle Entity Damage", "Misc", "DamageValues", 1f, false, null));
			this.AddSandboxOption(new SandboxOptionFloat(SandboxOptions.VehicleBlockDamage, "Vehicle Block Damage", "Misc", "DamageValues", 1f, false, null));
			this.AddSandboxOption(new SandboxOptionFloat(SandboxOptions.VehicleSelfDamage, "Vehicle Self Damage", "Misc", "DamageValues", 1f, false, null));
			this.AddSandboxOption(new SandboxOptionFloat(SandboxOptions.ElectricalOutput, "Electrical Output", "Misc", "StaminaRegen", 1f, true, null));
			this.AddSandboxOption(new SandboxOptionInt(SandboxOptions.SillyCelebrate, "Celebrate Kills", "Misc", "Celebrate", 0, true, null));
			this.AddSandboxOption(new SandboxOptionBoolean(SandboxOptions.SillyBigHeads, "Big Heads", "Misc", "YesNo", false, false, null));
			this.AddSandboxOption(new SandboxOptionBoolean(SandboxOptions.SillyTinyZombies, "Tiny Zombies", "Misc", "YesNo", false, false, null));
			this.AddSandboxOption(new SandboxOptionFloat(SandboxOptions.SillyLowGravity, "Gravity", "Misc", "Gravity", 1f, false, null));
			this.AddSandboxOption(new SandboxOptionBoolean(SandboxOptions.SillySounds, "Silly Sounds", "Misc", "YesNo", false, false, null));
			this.AddSandboxOption(new SandboxOptionBoolean(SandboxOptions.SillyBlackandWhite, "Black and White", "Misc", "YesNo", false, false, null));
			this.InitValueSets();
		}

		public void InitValueSets()
		{
			foreach (SandboxOptionValueSet sandboxOptionValueSet in this.ValueSets.Values)
			{
				sandboxOptionValueSet.Init();
			}
		}

		public List<string> GetAllPresetGroups()
		{
			List<string> list = new List<string>();
			for (int i = 0; i < this.SandboxPresets.Count; i++)
			{
				string group = this.SandboxPresets[i].Group;
				if (!list.Contains(group))
				{
					list.Add(group);
				}
			}
			return list;
		}

		public bool GetPresetsForGroup(string presetGroupName)
		{
			for (int i = 0; i < this.SandboxPresets.Count; i++)
			{
				if (this.SandboxPresets[i].Group == presetGroupName)
				{
					return true;
				}
			}
			return false;
		}

		public static BaseSandboxOption.OptionTypes GetOptionType(SandboxOptions optionType)
		{
			if (SandboxOptionManager.Current.SandboxOptionsDict.ContainsKey(optionType))
			{
				return SandboxOptionManager.Current.SandboxOptionsDict[optionType].OptionType;
			}
			return BaseSandboxOption.OptionTypes.Invalid;
		}

		public static bool GetBool(SandboxOptions optionType)
		{
			if (!SandboxOptionManager.Current.SandboxOptionsDict.ContainsKey(optionType))
			{
				return false;
			}
			if (SandboxOptionManager.overrideList != null && SandboxOptionManager.overrideList.Contains(optionType))
			{
				return SandboxOptionManager.Current.SandboxOptionsDict[optionType].GetDefaultBoolValue();
			}
			return SandboxOptionManager.Current.SandboxOptionsDict[optionType].GetBoolValue();
		}

		public static float GetFloat(SandboxOptions optionType)
		{
			if (!SandboxOptionManager.Current.SandboxOptionsDict.ContainsKey(optionType))
			{
				return 0f;
			}
			if (SandboxOptionManager.overrideList != null && SandboxOptionManager.overrideList.Contains(optionType))
			{
				return SandboxOptionManager.Current.SandboxOptionsDict[optionType].GetDefaultFloatValue();
			}
			return SandboxOptionManager.Current.SandboxOptionsDict[optionType].GetFloatValue();
		}

		public static int GetInt(SandboxOptions optionType)
		{
			if (!SandboxOptionManager.Current.SandboxOptionsDict.ContainsKey(optionType))
			{
				return 0;
			}
			if (SandboxOptionManager.overrideList != null && SandboxOptionManager.overrideList.Contains(optionType))
			{
				return SandboxOptionManager.Current.SandboxOptionsDict[optionType].GetDefaultIntValue();
			}
			return SandboxOptionManager.Current.SandboxOptionsDict[optionType].GetIntValue();
		}

		public static int GetIndex(SandboxOptions optionType)
		{
			if (SandboxOptionManager.Current.SandboxOptionsDict.ContainsKey(optionType))
			{
				return SandboxOptionManager.Current.SandboxOptionsDict[optionType].GetValueIndex();
			}
			return 0;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void AddSandboxOption(BaseSandboxOption option)
		{
			if (!this.OptionsByCategory.dict.ContainsKey(option.CategoryName))
			{
				this.OptionsByCategory.Add(option.CategoryName, new List<BaseSandboxOption>());
			}
			this.OptionsByCategory.dict[option.CategoryName].Add(option);
			this.SandboxOptionsDict.Add(option.Option, option);
		}

		public static BaseSandboxOption GetOption(SandboxOptions optionType)
		{
			if (SandboxOptionManager.Current.SandboxOptionsDict.ContainsKey(optionType))
			{
				return SandboxOptionManager.instance.SandboxOptionsDict[optionType];
			}
			return null;
		}

		public bool SetOption(SandboxOptions optionType, string value)
		{
			BaseSandboxOption baseSandboxOption = this.SandboxOptionsDict[optionType];
			baseSandboxOption.SetValue(value);
			return baseSandboxOption.IsChanged();
		}

		public void SetOption(SandboxOptions optionType, int value)
		{
			SandboxOptionInt sandboxOptionInt = this.SandboxOptionsDict[optionType] as SandboxOptionInt;
			if (sandboxOptionInt != null)
			{
				sandboxOptionInt.SetInt(value);
				if (sandboxOptionInt.DisabledOptions != null)
				{
					SandboxOptionInt.DisabledOptionsOnValue disabledOptions = sandboxOptionInt.DisabledOptions;
					bool flag = (disabledOptions.Inverted ? (value == disabledOptions.Value) : (value != disabledOptions.Value));
					string text = (flag ? "" : string.Format(Localization.Get("xuiSandboxDisabledBy", false, null), sandboxOptionInt.OptionNameText, sandboxOptionInt.ValueOptions.GetDisplayAtIndex(sandboxOptionInt.GetValueIndex())));
					for (int i = 0; i < disabledOptions.DisabledOptions.Length; i++)
					{
						BaseSandboxOption baseSandboxOption = this.SandboxOptionsDict[disabledOptions.DisabledOptions[i]];
						baseSandboxOption.IsEnabled = flag;
						baseSandboxOption.DisabledByText = text;
					}
					return;
				}
			}
			else
			{
				SandboxOptionFloat sandboxOptionFloat = this.SandboxOptionsDict[optionType] as SandboxOptionFloat;
				if (sandboxOptionFloat != null)
				{
					sandboxOptionFloat.SetFloat((float)value / 100f);
					if (sandboxOptionFloat.DisabledOptions != null)
					{
						SandboxOptionFloat.DisabledOptionsOnValue disabledOptions2 = sandboxOptionFloat.DisabledOptions;
						bool flag2 = (disabledOptions2.Inverted ? ((float)value == disabledOptions2.Value) : ((float)value != disabledOptions2.Value));
						string text2 = (flag2 ? "" : string.Format(Localization.Get("xuiSandboxDisabledBy", false, null), sandboxOptionFloat.OptionNameText, sandboxOptionFloat.ValueOptions.GetDisplayAtIndex(sandboxOptionFloat.GetValueIndex())));
						for (int j = 0; j < disabledOptions2.DisabledOptions.Length; j++)
						{
							BaseSandboxOption baseSandboxOption2 = this.SandboxOptionsDict[disabledOptions2.DisabledOptions[j]];
							baseSandboxOption2.IsEnabled = flag2;
							baseSandboxOption2.DisabledByText = text2;
						}
					}
				}
			}
		}

		public void SetOption(SandboxOptions optionType, float value)
		{
			SandboxOptionFloat sandboxOptionFloat = this.SandboxOptionsDict[optionType] as SandboxOptionFloat;
			if (sandboxOptionFloat != null)
			{
				sandboxOptionFloat.SetFloat(value);
				if (sandboxOptionFloat.DisabledOptions != null)
				{
					SandboxOptionFloat.DisabledOptionsOnValue disabledOptions = sandboxOptionFloat.DisabledOptions;
					bool flag = (disabledOptions.Inverted ? (value == disabledOptions.Value) : (value != disabledOptions.Value));
					string text = (flag ? "" : string.Format(Localization.Get("xuiSandboxDisabledBy", false, null), sandboxOptionFloat.OptionNameText, sandboxOptionFloat.ValueOptions.GetDisplayAtIndex(sandboxOptionFloat.GetValueIndex())));
					for (int i = 0; i < disabledOptions.DisabledOptions.Length; i++)
					{
						BaseSandboxOption baseSandboxOption = this.SandboxOptionsDict[disabledOptions.DisabledOptions[i]];
						baseSandboxOption.IsEnabled = flag;
						baseSandboxOption.DisabledByText = text;
					}
				}
			}
		}

		public void SetOption(SandboxOptions optionType, bool value)
		{
			SandboxOptionBoolean sandboxOptionBoolean = this.SandboxOptionsDict[optionType] as SandboxOptionBoolean;
			if (sandboxOptionBoolean != null)
			{
				sandboxOptionBoolean.SetBool(value);
				if (sandboxOptionBoolean.DisabledOptions != null)
				{
					SandboxOptionBoolean.DisabledOptionsOnValue disabledOptions = sandboxOptionBoolean.DisabledOptions;
					bool flag = (disabledOptions.Inverted ? (value == disabledOptions.Value) : (value != disabledOptions.Value));
					string text = (flag ? "" : string.Format(Localization.Get("xuiSandboxDisabledBy", false, null), sandboxOptionBoolean.OptionNameText, sandboxOptionBoolean.ValueOptions.GetDisplayAtIndex(sandboxOptionBoolean.GetValueIndex())));
					for (int i = 0; i < disabledOptions.DisabledOptions.Length; i++)
					{
						BaseSandboxOption baseSandboxOption = this.SandboxOptionsDict[disabledOptions.DisabledOptions[i]];
						baseSandboxOption.IsEnabled = flag;
						baseSandboxOption.DisabledByText = text;
					}
				}
			}
		}

		public void SetOptionToDefault(SandboxOptions optionType)
		{
			this.SandboxOptionsDict[optionType].SetToDefault();
		}

		public void OutputToConsole()
		{
			for (int i = 0; i < 152; i++)
			{
				if (this.SandboxOptionsDict.ContainsKey((SandboxOptions)i))
				{
					BaseSandboxOption baseSandboxOption = this.SandboxOptionsDict[(SandboxOptions)i];
					SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("{0}({1}): {2}", baseSandboxOption.OptionName, baseSandboxOption.Option, baseSandboxOption.GetValueText()));
				}
			}
		}

		public void ResetAllToDefault()
		{
			foreach (BaseSandboxOption baseSandboxOption in this.SandboxOptionsDict.Values)
			{
				baseSandboxOption.SetToDefault();
			}
		}

		public void SetWorldAndGame(string world, string game)
		{
			this.WorldName = world;
			this.GameName = game;
		}

		public SandboxOptionPreset GetPreset(string presetName)
		{
			for (int i = 0; i < this.SandboxPresets.Count; i++)
			{
				SandboxOptionPreset sandboxOptionPreset = this.SandboxPresets[i];
				if (sandboxOptionPreset.Name.EqualsCaseInsensitive(presetName))
				{
					return sandboxOptionPreset;
				}
			}
			return null;
		}

		public SandboxOptionPreset GetPresetByCode(string sandboxCode)
		{
			if (string.IsNullOrEmpty(sandboxCode))
			{
				return null;
			}
			for (int i = 0; i < this.SandboxPresets.Count; i++)
			{
				SandboxOptionPreset sandboxOptionPreset = this.SandboxPresets[i];
				if (sandboxOptionPreset.SandboxCode.EqualsCaseInsensitive(sandboxCode))
				{
					return sandboxOptionPreset;
				}
			}
			return null;
		}

		public SandboxOptionPreset GetDefaultPreset()
		{
			for (int i = this.SandboxPresets.Count - 1; i >= 0; i--)
			{
				SandboxOptionPreset sandboxOptionPreset = this.SandboxPresets[i];
				if (sandboxOptionPreset.IsDefault)
				{
					return sandboxOptionPreset;
				}
			}
			return null;
		}

		public void DeletePreset(string presetName)
		{
			for (int i = 0; i < this.SandboxPresets.Count; i++)
			{
				if (this.SandboxPresets[i].Name.EqualsCaseInsensitive(presetName))
				{
					this.SandboxPresets.RemoveAt(i);
					break;
				}
			}
			string text = GameIO.GetUserGameDataDir() + "/Presets/";
			if (!SdDirectory.Exists(text))
			{
				SdDirectory.CreateDirectory(text);
			}
			SdFile.Delete(text + presetName + ".xml");
			SaveDataUtils.SaveDataManager.CommitAsync();
		}

		public void SaveCurrentToPreset(SandboxOptionPreset preset)
		{
			preset.PresetValues.Clear();
			foreach (KeyValuePair<string, List<BaseSandboxOption>> keyValuePair in this.OptionsByCategory.dict)
			{
				List<BaseSandboxOption> value = keyValuePair.Value;
				for (int i = 0; i < value.Count; i++)
				{
					BaseSandboxOption baseSandboxOption = value[i];
					if (baseSandboxOption.IsChanged())
					{
						preset.PresetValues.Add(baseSandboxOption.Option, baseSandboxOption.GetValueIndex());
					}
				}
			}
		}

		public SandboxOptionPreset SaveCurrentToNewPreset(string presetName, string groupName, bool isUserPreset = false)
		{
			SandboxOptionPreset sandboxOptionPreset = new SandboxOptionPreset();
			sandboxOptionPreset.Name = presetName;
			sandboxOptionPreset.Group = groupName;
			sandboxOptionPreset.IsUserPreset = isUserPreset;
			sandboxOptionPreset.Icon = "Data/Sandbox/icons/user_custom";
			this.SaveCurrentToPreset(sandboxOptionPreset);
			return sandboxOptionPreset;
		}

		public void SavePresetToFile(SandboxOptionPreset preset)
		{
			string text = GameIO.GetUserGameDataDir() + "/Presets/";
			if (!SdDirectory.Exists(text))
			{
				SdDirectory.CreateDirectory(text);
			}
			string text2 = text + preset.Name + ".xml";
			SdFile.Delete(text2);
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.CreateXmlDeclaration();
			XmlElement xmlElement = xmlDocument.AddXmlElement("preset");
			xmlElement.AddXmlElement("property").SetAttrib("name", "code").SetAttrib("value", preset.SandboxCode);
			xmlElement.AddXmlElement("property").SetAttrib("name", "description").SetAttrib("value", preset.Description ?? "");
			xmlElement.AddXmlElement("property").SetAttrib("name", "icon").SetAttrib("value", preset.Icon ?? "");
			StringBuilder stringBuilder = new StringBuilder();
			foreach (KeyValuePair<string, List<BaseSandboxOption>> keyValuePair in this.OptionsByCategory.dict)
			{
				List<BaseSandboxOption> value = keyValuePair.Value;
				bool flag = false;
				for (int i = 0; i < value.Count; i++)
				{
					BaseSandboxOption baseSandboxOption = value[i];
					if (baseSandboxOption.IsChanged())
					{
						if (!flag)
						{
							flag = true;
							stringBuilder.AppendLine();
							stringBuilder.AppendLine("\t\t *** " + keyValuePair.Key.ToUpper() + " ***");
							stringBuilder.AppendLine();
						}
						stringBuilder.AppendLine("\t\t\t" + baseSandboxOption.OptionNameText + ": " + baseSandboxOption.GetValueTextFromIndex(baseSandboxOption.GetValueIndex()));
					}
				}
			}
			if (stringBuilder.Length > 0)
			{
				stringBuilder.Append("\t");
				xmlElement.AddXmlComment(stringBuilder.ToString());
			}
			using (Stream stream = SdFile.OpenWrite(text2))
			{
				using (XmlTextWriter xmlTextWriter = new XmlTextWriter(stream, Encoding.UTF8))
				{
					xmlTextWriter.Formatting = Formatting.Indented;
					xmlTextWriter.Indentation = 1;
					xmlTextWriter.IndentChar = '\t';
					xmlDocument.Save(xmlTextWriter);
				}
				SaveDataUtils.SaveDataManager.CommitAsync();
			}
		}

		public void SavePresetToDict(SandboxOptionPreset preset)
		{
			for (int i = 0; i < this.SandboxPresets.Count; i++)
			{
				if (this.SandboxPresets[i].Name.EqualsCaseInsensitive(preset.Name))
				{
					this.SandboxPresets[i] = preset;
					return;
				}
			}
			this.SandboxPresets.Add(preset);
		}

		public void SavePreset(SandboxOptionPreset preset, bool addToDictionary, bool saveToFile)
		{
			if (saveToFile)
			{
				this.SavePresetToFile(preset);
			}
			if (addToDictionary)
			{
				this.SavePresetToDict(preset);
			}
		}

		public SandboxOptionPreset SaveCurrentSettings(string presetName, bool addToDictionary, bool saveToFile, bool isUserPreset = false)
		{
			SandboxOptionPreset sandboxOptionPreset = this.SaveCurrentToNewPreset(presetName, "User", isUserPreset);
			this.SavePreset(sandboxOptionPreset, addToDictionary, saveToFile);
			return sandboxOptionPreset;
		}

		public void LoadPresets()
		{
			this.SandboxPresets.Clear();
			this.LoadInternalPresets();
			string text = GameIO.GetUserGameDataDir() + "/Presets/";
			if (SdDirectory.Exists(text))
			{
				foreach (string text2 in SdDirectory.GetFiles(text))
				{
					using (Stream stream = SdFile.OpenRead(text2))
					{
						XmlFile xmlFile = new XmlFile(stream);
						if (xmlFile != null)
						{
							SandboxOptionPreset sandboxOptionPreset = this.LoadPreset(xmlFile);
							if (sandboxOptionPreset != null)
							{
								sandboxOptionPreset.Name = Path.GetFileNameWithoutExtension(text2);
								this.SandboxPresets.Add(sandboxOptionPreset);
							}
						}
					}
				}
			}
			this.SandboxPresets.Add(SandboxOptionManager.CustomPreset);
		}

		public SandboxOptionPreset LoadPreset(XmlFile _xmlFile)
		{
			SandboxOptionPreset sandboxOptionPreset = null;
			XElement root = _xmlFile.XmlDoc.Root;
			if (root == null || !root.HasElements)
			{
				return null;
			}
			sandboxOptionPreset = new SandboxOptionPreset
			{
				Name = _xmlFile.Filename,
				Icon = "Data/Sandbox/icons/user_custom"
			};
			string text = "";
			foreach (XElement xelement in root.Elements())
			{
				if (xelement.Name == "property" && xelement.HasAttribute("name") && xelement.HasAttribute("value"))
				{
					string attribute = xelement.GetAttribute("name");
					string attribute2 = xelement.GetAttribute("value");
					if (!(attribute == "code"))
					{
						if (!(attribute == "description"))
						{
							if (attribute == "icon")
							{
								sandboxOptionPreset.Icon = attribute2;
							}
						}
						else
						{
							sandboxOptionPreset.Description = attribute2;
						}
					}
					else
					{
						text = attribute2;
					}
				}
			}
			this.StoreOptionsInPresetFromCode(sandboxOptionPreset, text);
			sandboxOptionPreset.IsUserPreset = true;
			sandboxOptionPreset.Group = "User";
			return sandboxOptionPreset;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void LoadInternalPresets()
		{
			XElement root = XDocument.Parse(((TextAsset)Resources.Load("Data/Sandbox/sandbox_presets")).text, LoadOptions.SetLineInfo).Root;
			if (root == null || !root.HasElements)
			{
				return;
			}
			foreach (XElement xelement in root.Elements())
			{
				if (xelement.Name == "preset")
				{
					this.LoadPresetFromXml(xelement, null, false);
				}
			}
		}

		public void LoadPresetFromXml(XElement childElement, string presetGroupOverride = null, bool isModded = false)
		{
			if (childElement.HasAttribute("name") && childElement.HasAttribute("code") && (childElement.HasAttribute("description") || childElement.HasAttribute("description_key")))
			{
				SandboxOptionPreset sandboxOptionPreset = new SandboxOptionPreset();
				sandboxOptionPreset.Name = childElement.GetAttribute("name");
				sandboxOptionPreset.LocalizedName = childElement.GetAttribute("localized_name");
				sandboxOptionPreset.Description = childElement.GetAttribute("description");
				sandboxOptionPreset.DescriptionKey = childElement.GetAttribute("description_key");
				sandboxOptionPreset.Icon = childElement.GetAttribute("icon");
				if (childElement.HasAttribute("default") && childElement.GetAttribute("default").EqualsCaseInsensitive("true"))
				{
					sandboxOptionPreset.IsDefault = true;
				}
				childElement.ParseAttribute("difficulty_rating", ref sandboxOptionPreset.DifficultyRating);
				if (sandboxOptionPreset.DifficultyRating > 10)
				{
					sandboxOptionPreset.DifficultyRating = 10;
				}
				if (childElement.HasAttribute("always_show"))
				{
					string[] array = childElement.GetAttribute("always_show").Split(',', StringSplitOptions.None);
					for (int i = 0; i < array.Length; i++)
					{
						SandboxOptions sandboxOptions = SandboxOptions.Max;
						if (Enum.TryParse<SandboxOptions>(array[i], out sandboxOptions))
						{
							if (sandboxOptionPreset.AlwaysShow == null)
							{
								sandboxOptionPreset.AlwaysShow = new List<SandboxOptions>();
							}
							sandboxOptionPreset.AlwaysShow.Add(sandboxOptions);
						}
					}
				}
				sandboxOptionPreset.IsModded = isModded;
				if (string.IsNullOrEmpty(presetGroupOverride))
				{
					childElement.ParseAttribute("category", ref sandboxOptionPreset.Group);
				}
				else
				{
					sandboxOptionPreset.Group = presetGroupOverride;
				}
				if (this.StoreOptionsInPresetFromCode(sandboxOptionPreset, childElement.GetAttribute("code")))
				{
					this.SandboxPresets.Add(sandboxOptionPreset);
				}
			}
		}

		public void SetValuesFromPreset(SandboxOptionPreset preset)
		{
			foreach (SandboxOptions sandboxOptions in preset.PresetValues.Keys)
			{
				this.SandboxOptionsDict[sandboxOptions].SetValueFromIndex(preset.PresetValues[sandboxOptions]);
			}
		}

		public void UpdateWorldOptionsWithSandboxOptions()
		{
			if (SandboxOptionManager.GetBool(SandboxOptions.SillyBigHeads))
			{
				GameEventManager.Current.SetGameEventFlag(GameEventManager.GameEventFlagTypes.BigHeadSandbox, true, -1f, true);
			}
			if (SandboxOptionManager.GetBool(SandboxOptions.SillyTinyZombies))
			{
				GameEventManager.Current.SetGameEventFlag(GameEventManager.GameEventFlagTypes.TinyZombiesSandbox, true, -1f, true);
			}
		}

		public void UpdateInGameValuesWithSandboxOptions(bool forceLoad = false)
		{
			GameManager gameManager = GameManager.Instance;
			World world = ((gameManager != null) ? gameManager.World : null);
			if (world == null && !forceLoad)
			{
				return;
			}
			AIDirectorBloodMoonComponent.BloodMoonFrequency = SandboxOptionManager.GetInt(SandboxOptions.BloodMoonFrequency);
			AIDirectorBloodMoonComponent.BloodMoonRange = SandboxOptionManager.GetInt(SandboxOptions.BloodMoonRange);
			AIDirectorBloodMoonComponent.BloodMoonEnemyCount = SandboxOptionManager.GetInt(SandboxOptions.BloodMoonEnemyCount);
			TraderInfo.TraderDialog = SandboxOptionManager.GetBool(SandboxOptions.TraderDialog);
			World.SandboxUseTraderArea = (TraderAreaStates)SandboxOptionManager.GetInt(SandboxOptions.TraderProtection);
			World.BiomeProgressionEnabled = SandboxOptionManager.GetBool(SandboxOptions.BiomeProgression);
			World.TemperatureSurvival = SandboxOptionManager.GetBool(SandboxOptions.TemperatureSurvival);
			World.StormFrequency = SandboxOptionManager.GetFloat(SandboxOptions.StormFreq);
			World.MapEnabled = SandboxOptionManager.GetBool(SandboxOptions.AllowMap);
			TraderManager.VendingEnabled = SandboxOptionManager.GetBool(SandboxOptions.VendingEnabled);
			TraderInfo.GlobalResetInterval = SandboxOptionManager.GetInt(SandboxOptions.TraderResetInterval);
			if (TraderInfo.GlobalResetInterval != -1)
			{
				TraderInfo.GlobalResetIntervalInTicks = TraderInfo.GlobalResetInterval * 24000;
			}
			else
			{
				TraderInfo.GlobalResetIntervalInTicks = -1;
			}
			TraderInfo.VendingResetInterval = SandboxOptionManager.GetInt(SandboxOptions.VendingResetInterval);
			if (TraderInfo.VendingResetInterval != -1)
			{
				TraderInfo.VendingResetIntervalInTicks = TraderInfo.VendingResetInterval * 24000;
			}
			else
			{
				TraderInfo.VendingResetIntervalInTicks = -1;
			}
			TraderInfo.TraderMaxTier = SandboxOptionManager.GetInt(SandboxOptions.TraderMaxTier);
			TraderInfo.TraderBuyLimit = SandboxOptionManager.GetInt(SandboxOptions.TraderBuyLimit);
			TraderInfo.TraderItemAbundance = SandboxOptionManager.GetFloat(SandboxOptions.TraderItemAbundance);
			TraderInfo.VendingItemAbundance = SandboxOptionManager.GetFloat(SandboxOptions.VendingItemAbundance);
			TraderInfo.TraderHoursPreset = (TraderInfo.TraderHourPresets)SandboxOptionManager.GetInt(SandboxOptions.TraderHours);
			LootContainer.GlobalCountModifier = SandboxOptionManager.GetFloat(SandboxOptions.GlobalLootCount);
			LootContainer.AmmoCountModifier = SandboxOptionManager.GetFloat(SandboxOptions.AmmoLootCount);
			LootContainer.FoodCountModifier = SandboxOptionManager.GetFloat(SandboxOptions.FoodLootCount);
			LootContainer.DrinkCountModifier = SandboxOptionManager.GetFloat(SandboxOptions.DrinkLootCount);
			LootContainer.MedicalCountModifier = SandboxOptionManager.GetFloat(SandboxOptions.MedicalLootCount);
			LootContainer.ResourceCountModifier = SandboxOptionManager.GetFloat(SandboxOptions.ResourceLootCount);
			LootContainer.ArmorCountModifier = SandboxOptionManager.GetFloat(SandboxOptions.ArmorLootCount);
			LootContainer.MeleeCountModifier = SandboxOptionManager.GetFloat(SandboxOptions.MeleeLootCount);
			LootContainer.RangedCountModifier = SandboxOptionManager.GetFloat(SandboxOptions.RangedLootCount);
			LootContainer.DukesCountModifier = SandboxOptionManager.GetFloat(SandboxOptions.DukesLootCount);
			LootContainer.MagazinesCountModifier = SandboxOptionManager.GetFloat(SandboxOptions.CraftingMagazinesLootCount);
			LootContainer.TreasureMapChance = SandboxOptionManager.GetFloat(SandboxOptions.TreasureMapChance);
			LootContainer.LootMaxTier = SandboxOptionManager.GetInt(SandboxOptions.LootMaxTier);
			LootContainer.NoLoot = LootContainer.GlobalCountModifier == 0f;
			LootContainer.LootTimerModifier = SandboxOptionManager.GetFloat(SandboxOptions.LootTimer);
			LootContainer.LootBagChance = SandboxOptionManager.GetFloat(SandboxOptions.LootBagChance);
			XUiM_Recipes.CraftingTimeModifier = SandboxOptionManager.GetFloat(SandboxOptions.CraftingTime);
			XUiM_Recipes.CraftingInputModifier = SandboxOptionManager.GetFloat(SandboxOptions.CraftingInput);
			XUiM_Recipes.CraftingOutputModifier = SandboxOptionManager.GetFloat(SandboxOptions.CraftingOutput);
			XUiM_Recipes.DisableSmelter = SandboxOptionManager.GetBool(SandboxOptions.SmeltingType);
			XUiM_Recipes.DewCollectorTimeModifier = SandboxOptionManager.GetFloat(SandboxOptions.DewCollectorTime);
			XUiM_Recipes.DewCollectorOutput = SandboxOptionManager.GetFloat(SandboxOptions.DewCollectorOutput);
			XUiM_Recipes.DewCollectorInput = SandboxOptionManager.GetFloat(SandboxOptions.DewCollectorInput);
			XUiM_Recipes.ApiaryTimeModifier = SandboxOptionManager.GetFloat(SandboxOptions.ApiaryTime);
			XUiM_Recipes.ApiaryOutput = SandboxOptionManager.GetFloat(SandboxOptions.ApiaryOutput);
			XUiM_Recipes.ApiaryInput = SandboxOptionManager.GetFloat(SandboxOptions.ApiaryInput);
			XUiM_Recipes.ScrappingOutputModifier = SandboxOptionManager.GetFloat(SandboxOptions.ScrappingOutput);
			XUiM_Recipes.MiningOutputModifier = SandboxOptionManager.GetFloat(SandboxOptions.MiningOutput);
			XUiM_Recipes.SeedDropOutputModifier = SandboxOptionManager.GetFloat(SandboxOptions.SeedDropOutput);
			XUiM_Recipes.CropOutputModifier = SandboxOptionManager.GetFloat(SandboxOptions.CropOutput);
			XUiM_Recipes.HarvestingOutputModifier = SandboxOptionManager.GetFloat(SandboxOptions.HarvestingOutput);
			XUiM_Recipes.CraftingMaxTier = SandboxOptionManager.GetInt(SandboxOptions.CraftingMaxTier);
			XUiM_Recipes.CraftingProgression = SandboxOptionManager.GetBool(SandboxOptions.CraftingProgression);
			XUiM_Recipes.BackpackCrafting = (BackpackCraftingOptions)SandboxOptionManager.GetInt(SandboxOptions.BackpackCrafting);
			if (XUiM_Recipes.BackpackCrafting != BackpackCraftingOptions.Enabled)
			{
				XUiM_Recipes.UpdateRecipesforBackpackCrafting();
			}
			XUiM_Recipes.WorkstationCrafting = SandboxOptionManager.GetBool(SandboxOptions.WorkstationCrafting);
			Quest.QuestsPerTier = SandboxOptionManager.GetInt(SandboxOptions.QuestsPerTier);
			Progression.SkillPointsGainRate = SandboxOptionManager.GetInt(SandboxOptions.SkillGainRate);
			Progression.SkillPointsPerLevel = ((Progression.SkillPointsPerLevelXML != 1) ? Progression.SkillPointsPerLevelXML : SandboxOptionManager.GetInt(SandboxOptions.SkillPointsPerLevel));
			XUiC_CompassWindow.ShowCompass = SandboxOptionManager.GetBool(SandboxOptions.AllowCompass);
			XUiC_OnScreenIcons.ShowIcons = SandboxOptionManager.GetBool(SandboxOptions.AllowScreenMarkers);
			XUiC_Location.ShowLocation = (XUiC_EnteringArea.ShowLocation = SandboxOptionManager.GetBool(SandboxOptions.ShowLocationInfo));
			DamageText.SandboxEnabled = SandboxOptionManager.GetBool(SandboxOptions.ShowEnemyDamage);
			AIDirector.HeatMapSensitivityModifier = SandboxOptionManager.GetFloat(SandboxOptions.HeatMapSensitivity);
			Physics.gravity = SandboxOptionManager.originalGravity * SandboxOptionManager.GetFloat(SandboxOptions.SillyLowGravity);
			EntityMoveHelper.AllowZombieDigging = SandboxOptionManager.GetBool(SandboxOptions.AllowZombieDigging);
			ItemActionAttack.MeleeDamagePercent = SandboxOptionManager.GetFloat(SandboxOptions.MeleeDamage);
			ItemActionAttack.RangedDamagePercent = SandboxOptionManager.GetFloat(SandboxOptions.RangedDamage);
			ItemActionAttack.BlockDamagePercent = SandboxOptionManager.GetFloat(SandboxOptions.BlockDamage);
			ItemActionAttack.HeadshotMultiplier = SandboxOptionManager.GetFloat(SandboxOptions.HeadshotMultiplier);
			ItemActionAttack.TerrainDamagePercent = SandboxOptionManager.GetFloat(SandboxOptions.TerrainDamage);
			ItemActionAttack.IncomingDamageModifier = SandboxOptionManager.GetFloat(SandboxOptions.IncomingDamage);
			ItemActionAttack.EntityIncomingDamageModifier = SandboxOptionManager.GetFloat(SandboxOptions.EntityIncomingDamage);
			ItemActionAttack.StaminaUsageMultiplier = SandboxOptionManager.GetFloat(SandboxOptions.StaminaUsage);
			ItemActionAttack.EntityPlayerDamagePercent = SandboxOptionManager.GetFloat(SandboxOptions.EntityDamage);
			ItemActionAttack.EntityBlockDamagePercent = SandboxOptionManager.GetFloat(SandboxOptions.BlockDamageAI);
			ItemActionAttack.BMBlockDamagePercent = SandboxOptionManager.GetFloat(SandboxOptions.BlockDamageAIBM);
			EntityAlive.HeadshotMode = (EntityAlive.HeadShotOnlyModes)SandboxOptionManager.GetInt(SandboxOptions.HeadshotMode);
			EntityAlive.CelebrateMode = (EntityAlive.CelebrateModes)SandboxOptionManager.GetInt(SandboxOptions.SillyCelebrate);
			EntityPlayer.GlobalGameStageModifier = SandboxOptionManager.GetFloat(SandboxOptions.GlobalGSModifier);
			EntityPlayer.BiomeGameStageModifier = SandboxOptionManager.GetFloat(SandboxOptions.BiomeGSModifier);
			EntityPlayer.GlobalLootStageModifier = SandboxOptionManager.GetFloat(SandboxOptions.GlobalLSModifier);
			EntityPlayer.BiomeLootStageModifier = SandboxOptionManager.GetFloat(SandboxOptions.BiomeLSModifier);
			EntityPlayer.POITierLootStageModifier = SandboxOptionManager.GetFloat(SandboxOptions.POITierLSModifier);
			EntityPlayer.GlobalTraderStageModifier = SandboxOptionManager.GetFloat(SandboxOptions.GlobalTSModifier);
			EntityPlayer.FallDamageModifier = ((SandboxOptionManager.GetFloat(SandboxOptions.JumpStrength) == 0f) ? 0f : (1f / SandboxOptionManager.GetFloat(SandboxOptions.JumpStrength)));
			EntityVehicle.VehicleFuelUsageModifier = SandboxOptionManager.GetFloat(SandboxOptions.VehicleFuelUsage);
			EntityVehicle.VehicleEntityDamageModifier = SandboxOptionManager.GetFloat(SandboxOptions.VehicleEntityDamage);
			EntityVehicle.VehicleBlockDamageModifier = SandboxOptionManager.GetFloat(SandboxOptions.VehicleBlockDamage);
			EntityVehicle.VehicleSelfDamageModifier = SandboxOptionManager.GetFloat(SandboxOptions.VehicleSelfDamage);
			PowerSource.PowerOutputModifier = SandboxOptionManager.GetFloat(SandboxOptions.ElectricalOutput);
			Progression.XPGain = SandboxOptionManager.GetFloat(SandboxOptions.XPMultiplier);
			Progression.ShowXPType = (Progression.ShowXPTypes)SandboxOptionManager.GetInt(SandboxOptions.ShowXP);
			ItemAction.ItemDegradationModifier = SandboxOptionManager.GetFloat(SandboxOptions.ItemDegradation);
			ItemAction.ItemMaxDegrationAmount = SandboxOptionManager.GetFloat(SandboxOptions.MaxDegradationAmount);
			ItemAction.RepairType = (ItemAction.RepairTypes)SandboxOptionManager.GetInt(SandboxOptions.RepairTypes);
			EntityHuman.SetupRageChance(SandboxOptionManager.GetFloat(SandboxOptions.ZombieRageChance), SandboxOptionManager.GetIndex(SandboxOptions.ZombieRageChance));
			List<EntityPlayerLocal> list = ((world != null) ? world.GetLocalPlayers() : null);
			ChallengeJournal.AllowChallenges = SandboxOptionManager.GetBool(SandboxOptions.ChallengesEnabled);
			ChallengeJournal.IntroChallengesEnabled = SandboxOptionManager.GetBool(SandboxOptions.IntroChallengesEnabled);
			QuestJournal.IntroQuestEnabled = SandboxOptionManager.GetBool(SandboxOptions.IntroQuestEnabled);
			QuestJournal.BuriedQuestsEnabled = SandboxOptionManager.GetBool(SandboxOptions.BuriedQuestsEnabled);
			QuestJournal.POIQuestsEnabled = SandboxOptionManager.GetBool(SandboxOptions.POIQuestsEnabled);
			SkyManager.isAllTimeDay = SandboxOptionManager.GetInt(SandboxOptions.DayLightLength) == 24;
			SkyManager.isAllTimeNight = SandboxOptionManager.GetInt(SandboxOptions.DayLightLength) == 0;
			GameManager gameManager2 = GameManager.Instance;
			if (gameManager2 != null && !gameManager2.IsEditMode())
			{
				GameStats.Set(EnumGameStats.TimeOfDayIncPerSec, 24000 / (SandboxOptionManager.GetInt(SandboxOptions.DayNightLength) * 60));
			}
			ItemClass.MaxTechType = (ItemClass.ItemTechTypes)SandboxOptionManager.GetInt(SandboxOptions.MaxTechType);
			EntityFactory.MaxEntityTier = (EntityClass.EntityTierTypes)SandboxOptionManager.GetInt(SandboxOptions.MaxEnemyTier);
			Log.Out(string.Format("MaxEnemyTier IsEnabled: {0}", this.IsEnabled(SandboxOptions.MaxEnemyTier)));
			EntityFactory.EnemySpawnMode = SandboxOptionManager.GetBool(SandboxOptions.EnemySpawnMode);
			ChunkAreaBiomeSpawnData.RespawnDelayIndexEnemies = SandboxOptionManager.GetInt(SandboxOptions.BiomeZombieRespawn);
			ChunkAreaBiomeSpawnData.RespawnDelayIndexAnimals = SandboxOptionManager.GetInt(SandboxOptions.BiomeAnimalRespawn);
			ChunkAreaBiomeSpawnData.RespawnCountOverride = SandboxOptionManager.GetInt(SandboxOptions.BiomeEnemyDensity);
			EAISetNearestCorpseAsTarget.ZombiesEatAnimalCorpses = SandboxOptionManager.GetBool(SandboxOptions.ZombiesEatAnimals);
			EAIManager.FeralSense = SandboxOptionManager.GetInt(SandboxOptions.ZombieFeralSense);
			this.SetupBloodMoonWarningTimes();
			this.SetupAirDropTimeRanges();
			GameStats.Set(EnumGameStats.GlobalGSModifier, SandboxOptionManager.GetInt(SandboxOptions.GlobalGSModifier));
			GameStats.Set(EnumGameStats.BiomeGSModifier, SandboxOptionManager.GetInt(SandboxOptions.BiomeGSModifier));
			GameStats.Set(EnumGameStats.GlobalLSModifier, SandboxOptionManager.GetInt(SandboxOptions.GlobalLSModifier));
			GameStats.Set(EnumGameStats.BiomeLSModifier, SandboxOptionManager.GetInt(SandboxOptions.BiomeLSModifier));
			int @int = SandboxOptionManager.GetInt(SandboxOptions.XPMultiplier);
			int int2 = SandboxOptionManager.GetInt(SandboxOptions.BlockDamage);
			int int3 = SandboxOptionManager.GetInt(SandboxOptions.BlockDamageAI);
			int int4 = SandboxOptionManager.GetInt(SandboxOptions.BlockDamageAIBM);
			int int5 = SandboxOptionManager.GetInt(SandboxOptions.GlobalLootCount);
			int int6 = SandboxOptionManager.GetInt(SandboxOptions.LootRespawnDays);
			if (!(SingletonMonoBehaviour<ConnectionManager>.Instance != null) || !SingletonMonoBehaviour<ConnectionManager>.Instance.IsClient)
			{
				GamePrefs.Set(EnumGamePrefs.XPMultiplier, @int);
				GamePrefs.Set(EnumGamePrefs.BlockDamagePlayer, int2);
				GamePrefs.Set(EnumGamePrefs.BlockDamageAI, int3);
				GamePrefs.Set(EnumGamePrefs.BlockDamageAIBM, int4);
				GamePrefs.Set(EnumGamePrefs.LootAbundance, int5);
				GamePrefs.Set(EnumGamePrefs.LootRespawnDays, int6);
			}
			GameStats.Set(EnumGameStats.XPMultiplier, @int);
			GameStats.Set(EnumGameStats.BlockDamagePlayer, int2);
			GameStats.Set(EnumGameStats.BlockDamageAI, int3);
			GameStats.Set(EnumGameStats.BlockDamageAIBM, int4);
			GameStats.Set(EnumGameStats.LootAbundance, int5);
			GameStats.Set(EnumGameStats.LootRespawnDays, int6);
			GameStats.Set(EnumGameStats.DayNightLength, SandboxOptionManager.GetInt(SandboxOptions.DayNightLength));
			GameStats.Set(EnumGameStats.DayLightLength, SandboxOptionManager.GetInt(SandboxOptions.DayLightLength));
			this.SetupLostItemsOnDeathValues();
			GameStats.Set(EnumGameStats.BloodMoonEnemyCount, SandboxOptionManager.GetInt(SandboxOptions.BloodMoonEnemyCount));
			GameStats.Set(EnumGameStats.EnemySpawnMode, SandboxOptionManager.GetBool(SandboxOptions.EnemySpawnMode));
			GameStats.Set(EnumGameStats.BloodMoonWarning, SandboxOptionManager.GetInt(SandboxOptions.BloodMoonWarning));
			GameStats.Set(EnumGameStats.StormFreq, SandboxOptionManager.GetInt(SandboxOptions.StormFreq));
			GameStats.Set(EnumGameStats.BiomeProgression, SandboxOptionManager.GetBool(SandboxOptions.BiomeProgression));
			EntityPlayerLocal.DegradeOnDeathType = (EntityPlayerLocal.DegradeOnDeathTypes)SandboxOptionManager.GetInt(SandboxOptions.DegradeItemsOnDeath);
			EntityPlayerLocal.DegradeAmountOnDeath = SandboxOptionManager.GetFloat(SandboxOptions.DegradeAmountOnDeath);
			BlockPlantGrowing.CropGrowthModifier = SandboxOptionManager.GetFloat(SandboxOptions.CropGrowthSpeed);
			if (list != null && list.Count > 0)
			{
				EntityPlayerLocal entityPlayerLocal = list[0];
				entityPlayerLocal.Stats.UpdateSandboxOptions();
				if (!QuestJournal.IntroQuestEnabled)
				{
					entityPlayerLocal.Buffs.SetCustomVar("IntroComplete", 1f, true, CVarOperation.set);
				}
				entityPlayerLocal.Buffs.SetCustomVar("_carrySandboxModifier", SandboxOptionManager.GetFloat(SandboxOptions.EncumbranceModifier), true, CVarOperation.set);
				entityPlayerLocal.Buffs.SetCustomVar("TraderToTraderQuests", (float)(SandboxOptionManager.GetBool(SandboxOptions.TraderToTraderQuestsEnabled) ? 1 : 0), true, CVarOperation.set);
				entityPlayerLocal.Buffs.SetCustomVar("_InfectionRate", SandboxOptionManager.GetFloat(SandboxOptions.InfectionRate) * 1.25f, true, CVarOperation.set);
				entityPlayerLocal.Buffs.SetCustomVar("_InfectionCureRate", SandboxOptionManager.GetFloat(SandboxOptions.InfectionRate) * -3f, true, CVarOperation.set);
				EntityPlayerLocal.StormWarning = SandboxOptionManager.GetBool(SandboxOptions.StormWarning);
				entityPlayerLocal.Progression.UpdateForSandbox();
				Manager.Instance.bUseAltSounds = SandboxOptionManager.GetBool(SandboxOptions.SillySounds);
				if (SandboxOptionManager.GetBool(SandboxOptions.SillyBlackandWhite))
				{
					if (entityPlayerLocal.Buffs.GetBuff("sandbox_blackandwhite") == null)
					{
						entityPlayerLocal.Buffs.AddBuff("sandbox_blackandwhite", -1, true, false, -1f);
					}
				}
				else if (entityPlayerLocal.Buffs.GetBuff("sandbox_blackandwhite") != null)
				{
					entityPlayerLocal.Buffs.RemoveBuff("sandbox_blackandwhite", -1, true);
				}
				if (!ChallengeJournal.IntroChallengesEnabled)
				{
					entityPlayerLocal.challengeJournal.CompleteIntroChallenges();
				}
				if ((!ChallengeJournal.AllowChallenges || !ChallengeJournal.IntroChallengesEnabled) && Quest.StarterQuest != "" && entityPlayerLocal.QuestJournal.FindQuest(Quest.StarterQuest, -1) == null && entityPlayerLocal.Buffs.GetCustomVar("StarterQuest") == 0f)
				{
					GameEventManager.Current.HandleAction("challenge_group_reward_basics", null, entityPlayerLocal, false, "", "", false, true, "", null);
				}
			}
			this.UpdateWorldOptionsWithSandboxOptions();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void SetupBloodMoonWarningTimes()
		{
			switch (SandboxOptionManager.GetInt(SandboxOptions.BloodMoonWarning))
			{
			case 0:
				World.BloodMoonWarningHour = -1;
				return;
			case 1:
				World.BloodMoonWarningHour = 8;
				return;
			case 2:
				World.BloodMoonWarningHour = 18;
				return;
			default:
				return;
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void SetupAirDropTimeRanges()
		{
			ulong num = 12000UL;
			ulong num2 = 12000UL;
			int num3 = 3;
			int num4 = 3;
			switch (SandboxOptionManager.GetInt(SandboxOptions.AirDropRandomTime))
			{
			case 1:
				num = 4000UL;
				num2 = 10000UL;
				break;
			case 2:
				num = 10000UL;
				num2 = 14000UL;
				break;
			case 3:
				num = 15000UL;
				num2 = 20000UL;
				break;
			case 4:
				num = 20000UL;
				num2 = 4000UL;
				break;
			case 5:
				num = 4000UL;
				num2 = 20000UL;
				break;
			case 6:
				num = 0UL;
				num2 = 23999UL;
				break;
			}
			switch (SandboxOptionManager.GetInt(SandboxOptions.AirDropFrequency))
			{
			case 0:
				num4 = (num3 = 0);
				break;
			case 1:
				num4 = (num3 = 1);
				break;
			case 2:
				num3 = 1;
				num4 = 3;
				break;
			case 3:
				num4 = (num3 = 3);
				break;
			case 4:
				num3 = 3;
				num4 = 7;
				break;
			case 5:
				num4 = (num3 = 7);
				break;
			case 6:
				num3 = 1;
				num4 = 7;
				break;
			}
			if (SandboxOptionManager.GetInt(SandboxOptions.AirDropFrequency) != 3)
			{
				GameStats.Set(EnumGameStats.AirDropFrequency, (num3 == num4) ? num3 : 3);
			}
			GameStats.Set(EnumGameStats.AirDropMarker, SandboxOptionManager.GetBool(SandboxOptions.AirDropMarker));
			AIDirectorAirDropComponent.MinTimeOfDay = num;
			AIDirectorAirDropComponent.MaxTimeOfDay = num2;
			AIDirectorAirDropComponent.MinDayCount = num3;
			AIDirectorAirDropComponent.MaxDayCount = num4;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void SetupLostItemsOnDeathValues()
		{
			switch (SandboxOptionManager.GetInt(SandboxOptions.LoseItemsOnDeathCount))
			{
			case 1:
				EntityPlayerLocal.LostItemOnDeathMin = 1;
				EntityPlayerLocal.LostItemOnDeathMax = 3;
				break;
			case 2:
				EntityPlayerLocal.LostItemOnDeathMin = 1;
				EntityPlayerLocal.LostItemOnDeathMax = 5;
				break;
			case 3:
				EntityPlayerLocal.LostItemOnDeathMin = 1;
				EntityPlayerLocal.LostItemOnDeathMax = 10;
				break;
			case 4:
				EntityPlayerLocal.LostItemOnDeathMin = 1;
				EntityPlayerLocal.LostItemOnDeathMax = 20;
				break;
			case 5:
				EntityPlayerLocal.LostItemOnDeathMin = 3;
				EntityPlayerLocal.LostItemOnDeathMax = 5;
				break;
			case 6:
				EntityPlayerLocal.LostItemOnDeathMin = 5;
				EntityPlayerLocal.LostItemOnDeathMax = 7;
				break;
			case 7:
				EntityPlayerLocal.LostItemOnDeathMin = 5;
				EntityPlayerLocal.LostItemOnDeathMax = 10;
				break;
			case 8:
				EntityPlayerLocal.LostItemOnDeathMin = 7;
				EntityPlayerLocal.LostItemOnDeathMax = 10;
				break;
			case 9:
				EntityPlayerLocal.LostItemOnDeathMin = 10;
				EntityPlayerLocal.LostItemOnDeathMax = 15;
				break;
			case 10:
				EntityPlayerLocal.LostItemOnDeathMin = 15;
				EntityPlayerLocal.LostItemOnDeathMax = 20;
				break;
			}
			GameStats.Set(EnumGameStats.DeathPenalty, SandboxOptionManager.GetInt(SandboxOptions.DeathPenalty));
			GameStats.Set(EnumGameStats.DropOnDeath, SandboxOptionManager.GetInt(SandboxOptions.DropOnDeath));
			GameStats.Set(EnumGameStats.DropOnQuit, SandboxOptionManager.GetInt(SandboxOptions.DropOnQuit));
		}

		public bool TryGetPresetDescription(string presetName, out string description)
		{
			SandboxOptionPreset preset = this.GetPreset(presetName);
			description = "";
			if (preset == null)
			{
				return false;
			}
			description = preset.Description;
			return true;
		}

		public bool GetChangedPresetOptions(string presetName, [TupleElementNames(new string[] { "name", "value", "isDefault" })] out List<ValueTuple<string, string, bool>> valuesList)
		{
			SandboxOptionPreset preset = this.GetPreset(presetName);
			return this.GetChangedPresetOptions(preset, out valuesList);
		}

		public bool GetChangedPresetOptions(SandboxOptionPreset preset, [TupleElementNames(new string[] { "name", "value", "isDefault" })] out List<ValueTuple<string, string, bool>> valuesList)
		{
			valuesList = null;
			if (preset == null)
			{
				return false;
			}
			valuesList = new List<ValueTuple<string, string, bool>>();
			foreach (SandboxOptions sandboxOptions in preset.PresetValues.Keys)
			{
				BaseSandboxOption baseSandboxOption = this.SandboxOptionsDict[sandboxOptions];
				valuesList.Add(new ValueTuple<string, string, bool>(baseSandboxOption.OptionNameText, baseSandboxOption.GetValueTextFromIndex(preset.PresetValues[sandboxOptions]), baseSandboxOption.GetDefaultIndex() == preset.PresetValues[sandboxOptions]));
			}
			if (preset.AlwaysShow != null)
			{
				for (int i = 0; i < preset.AlwaysShow.Count; i++)
				{
					SandboxOptions sandboxOptions2 = preset.AlwaysShow[i];
					BaseSandboxOption baseSandboxOption2 = this.SandboxOptionsDict[sandboxOptions2];
					string optionName = baseSandboxOption2.OptionNameText;
					if (valuesList.FindIndex(([TupleElementNames(new string[] { "name", "value", "isDefault" })] ValueTuple<string, string, bool> _tuple) => _tuple.Item1.Equals(optionName)) < 0)
					{
						valuesList.Add(new ValueTuple<string, string, bool>(optionName, baseSandboxOption2.GetDefaultValueText(), true));
					}
				}
			}
			return true;
		}

		public bool StoreOptionsInPresetFromCode(SandboxOptionPreset p, string code)
		{
			if ((code == "" || code[0] != SandboxOptionManager.currentVersion) && p.IsUserPreset)
			{
				return false;
			}
			this.ResetAllToDefault();
			if (code != "")
			{
				code = code.Substring(1);
				int num = code.Length / 3;
				for (int i = 0; i < num; i++)
				{
					int num2 = i * 3;
					string text = code.Substring(num2, 2);
					char c = code[num2 + 2];
					SandboxOptions sandboxOptions = (SandboxOptions)this.Alpha2ToIndex(text);
					int num3 = this.AlphaToIndex(c);
					if (this.SandboxOptionsDict.ContainsKey(sandboxOptions) && this.SandboxOptionsDict[sandboxOptions].GetValueSet().IsValidIndex(num3))
					{
						p.PresetValues.Add(sandboxOptions, num3);
					}
				}
			}
			return true;
		}

		public bool LoadOptionsFromCode(string code)
		{
			this.ResetAllToDefault();
			if (code == "" || code[0] != SandboxOptionManager.currentVersion)
			{
				return false;
			}
			code = code.Substring(1);
			int num = code.Length / 3;
			for (int i = 0; i < num; i++)
			{
				int num2 = i * 3;
				string text = code.Substring(num2, 2);
				char c = code[num2 + 2];
				SandboxOptions sandboxOptions = (SandboxOptions)this.Alpha2ToIndex(text);
				int num3 = this.AlphaToIndex(c);
				if (this.SandboxOptionsDict.ContainsKey(sandboxOptions))
				{
					this.SandboxOptionsDict[sandboxOptions].SetValueFromIndex(num3);
				}
			}
			return true;
		}

		public bool LoadOptionsFromCode(string code, SandboxOptionPreset preset)
		{
			if (code == "" || code[0] != SandboxOptionManager.currentVersion || preset == null)
			{
				return false;
			}
			preset.PresetValues.Clear();
			code = code.Substring(1);
			int num = code.Length / 3;
			for (int i = 0; i < num; i++)
			{
				int num2 = i * 3;
				string text = code.Substring(num2, 2);
				char c = code[num2 + 2];
				SandboxOptions sandboxOptions = (SandboxOptions)this.Alpha2ToIndex(text);
				int num3 = this.AlphaToIndex(c);
				if (this.SandboxOptionsDict.ContainsKey(sandboxOptions) && this.SandboxOptionsDict[sandboxOptions].GetValueSet().IsValidIndex(num3))
				{
					preset.PresetValues.Add(sandboxOptions, num3);
				}
			}
			return true;
		}

		public static char IndexToAlpha(int index)
		{
			if (index < 0 || index > 26)
			{
				throw new ArgumentOutOfRangeException("Index", string.Format("Index was out of range of the code conversion: {0}", index));
			}
			return (char)(65 + index % 26);
		}

		public static string IndexToAlpha2(int index)
		{
			if (index < 0 || index >= 676)
			{
				throw new ArgumentOutOfRangeException("Index", string.Format("Index was out of range of the code conversion: {0}", index));
			}
			char c = (char)(65 + index / 26);
			char c2 = (char)(65 + index % 26);
			return string.Format("{0}{1}", c, c2);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public int AlphaToIndex(char value)
		{
			value = char.ToUpper(value);
			if (value < 'A' || value > 'Z')
			{
				throw new ArgumentException("Value must contain only A–Z.");
			}
			return (int)(value - 'A');
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public int Alpha2ToIndex(string value)
		{
			if (string.IsNullOrWhiteSpace(value) || value.Length != 2)
			{
				throw new ArgumentException("Value must be exactly 2 letters.");
			}
			value = value.ToUpperInvariant();
			if (value[0] < 'A' || value[0] > 'Z' || value[1] < 'A' || value[1] > 'Z')
			{
				throw new ArgumentException("Value must contain only A–Z.");
			}
			return (int)((value[0] - 'A') * '\u001a' + (value[1] - 'A'));
		}

		public void RemoveOverrides()
		{
			for (int i = this.SandboxPresets.Count - 1; i >= 0; i--)
			{
				if (this.SandboxPresets[i].IsModded)
				{
					this.SandboxPresets.RemoveAt(i);
				}
			}
			if (SandboxOptionManager.overrideList != null)
			{
				SandboxOptionManager.overrideList = null;
			}
		}

		public bool IsEnabled(SandboxOptions option)
		{
			return this.SandboxOptionsDict[option].IsEnabled;
		}

		public bool IsOverriden(SandboxOptions option)
		{
			return SandboxOptionManager.overrideList != null && SandboxOptionManager.overrideList.Contains(option);
		}

		public void AddOverride(SandboxOptions option)
		{
			if (SandboxOptionManager.overrideList == null)
			{
				SandboxOptionManager.overrideList = new List<SandboxOptions>();
			}
			if (!SandboxOptionManager.overrideList.Contains(option))
			{
				SandboxOptionManager.overrideList.Add(option);
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static SandboxOptionManager instance = null;

		public const byte FileVersion = 1;

		public const string CustomPresetName = "Custom";

		public const string CustomGroupName = "Custom";

		public const string UserGroupName = "User";

		public const string ModdedGroupName = "Modded";

		public const string IconCustomPresets = "Data/Sandbox/icons/user_custom";

		public List<SandboxOptionCategory> SandboxOptionCategories = new List<SandboxOptionCategory>();

		public Dictionary<SandboxOptions, BaseSandboxOption> SandboxOptionsDict = new Dictionary<SandboxOptions, BaseSandboxOption>();

		public Dictionary<string, SandboxOptionValueSet> ValueSets = new Dictionary<string, SandboxOptionValueSet>();

		public DictionaryList<string, List<BaseSandboxOption>> OptionsByCategory = new DictionaryList<string, List<BaseSandboxOption>>();

		public List<SandboxOptionPreset> SandboxPresets = new List<SandboxOptionPreset>();

		public static readonly SandboxOptionPreset CustomPreset = new SandboxOptionPreset
		{
			Name = "Custom",
			LocalizedName = "sandboxPresetGroupCustom",
			Group = "Custom",
			IsCustomPreset = true,
			Icon = "Data/Sandbox/icons/user_custom"
		};

		public string WorldName;

		public string GameName;

		public string CurrentPresetName = "";

		public static readonly char currentVersion = 'A';

		[PublicizedFrom(EAccessModifier.Private)]
		public static Vector3 originalGravity = Vector3.zero;

		[PublicizedFrom(EAccessModifier.Private)]
		public static List<SandboxOptions> overrideList;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool initRun;
	}
}
