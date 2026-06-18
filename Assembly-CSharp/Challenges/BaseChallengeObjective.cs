using System;
using System.Globalization;
using System.IO;
using System.Xml.Linq;
using UnityEngine.Scripting;

namespace Challenges
{
	[Preserve]
	public class BaseChallengeObjective
	{
		public byte CurrentFileVersion { get; set; }

		public virtual ChallengeObjectiveType ObjectiveType
		{
			get
			{
				return ChallengeObjectiveType.Invalid;
			}
		}

		public virtual ChallengeClass.UINavTypes NavType
		{
			get
			{
				return ChallengeClass.UINavTypes.None;
			}
		}

		public virtual bool NeedsConstantUIUpdate
		{
			get
			{
				return false;
			}
		}

		public bool Complete
		{
			get
			{
				return this.complete;
			}
			set
			{
				if (this.complete != value)
				{
					this.complete = value;
					this.HandleValueChanged();
				}
			}
		}

		public int Current
		{
			get
			{
				return this.current;
			}
			set
			{
				if (this.current != value)
				{
					this.current = value;
					this.HandleValueChanged();
				}
			}
		}

		public virtual void BaseInit()
		{
		}

		public EntityPlayerLocal Player
		{
			get
			{
				return this.Owner.Owner.Player;
			}
		}

		public void ResetComplete()
		{
			this.Complete = false;
			this.current = 0;
		}

		public event ObjectiveValueChanged ValueChanged;

		public string ObjectiveText
		{
			get
			{
				return string.Format("{0} {1}", this.DescriptionText, this.StatusText);
			}
		}

		public virtual string DescriptionText
		{
			get
			{
				return "";
			}
		}

		public virtual string StatusText
		{
			get
			{
				if (this.Owner != null && this.Owner.AutoCompleted)
				{
					return "--";
				}
				return string.Format("{0}/{1}", this.current, this.MaxCount);
			}
		}

		public virtual float FillAmount
		{
			get
			{
				return (float)this.current / (float)this.MaxCount;
			}
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public void HandleValueChanged()
		{
			if (this.ValueChanged != null)
			{
				this.ValueChanged();
			}
		}

		public virtual void Init()
		{
		}

		public virtual void HandleOnCreated()
		{
		}

		public virtual bool HandleCheckStatus()
		{
			this.Complete = this.CheckObjectiveComplete(false);
			return this.Complete;
		}

		public virtual void UpdateStatus()
		{
		}

		public virtual void HandleAddHooks()
		{
		}

		public virtual void HandleRemoveHooks()
		{
		}

		public virtual void HandleTrackingStarted()
		{
			this.IsTracking = true;
		}

		public virtual void CopyValues(BaseChallengeObjective obj, BaseChallengeObjective objFromClass)
		{
			this.current = obj.current;
			this.MaxCount = objFromClass.MaxCount;
			this.ShowRequirements = objFromClass.ShowRequirements;
			this.Biome = objFromClass.Biome;
			this.complete = this.Current >= this.MaxCount;
		}

		public virtual void HandleTrackingEnded()
		{
			this.IsTracking = false;
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public virtual void HandleUpdatingCurrent()
		{
		}

		public void HandleAutoComplete()
		{
			this.current = this.MaxCount;
			this.Complete = true;
		}

		public static BaseChallengeObjective ReadObjective(byte _currentVersion, ChallengeObjectiveType _type, BinaryReader _br)
		{
			BaseChallengeObjective baseChallengeObjective = null;
			switch (_type)
			{
			case ChallengeObjectiveType.BlockPlace:
				baseChallengeObjective = new ChallengeObjectiveBlockPlace();
				break;
			case ChallengeObjectiveType.BlockUpgrade:
				baseChallengeObjective = new ChallengeObjectiveBlockUpgrade();
				break;
			case ChallengeObjectiveType.Bloodmoon:
				baseChallengeObjective = new ChallengeObjectiveBloodmoon();
				break;
			case ChallengeObjectiveType.Craft:
				baseChallengeObjective = new ChallengeObjectiveCraft();
				break;
			case ChallengeObjectiveType.CureDebuff:
				baseChallengeObjective = new ChallengeObjectiveCureDebuff();
				break;
			case ChallengeObjectiveType.EnterBiome:
				baseChallengeObjective = new ChallengeObjectiveEnterBiome();
				break;
			case ChallengeObjectiveType.Gather:
				baseChallengeObjective = new ChallengeObjectiveGather();
				break;
			case ChallengeObjectiveType.GatherIngredient:
				baseChallengeObjective = new ChallengeObjectiveGatherIngredient();
				break;
			case ChallengeObjectiveType.Harvest:
				baseChallengeObjective = new ChallengeObjectiveHarvest();
				break;
			case ChallengeObjectiveType.Hold:
				baseChallengeObjective = new ChallengeObjectiveHold();
				break;
			case ChallengeObjectiveType.Kill:
				baseChallengeObjective = new ChallengeObjectiveKill();
				break;
			case ChallengeObjectiveType.QuestComplete:
				baseChallengeObjective = new ChallengeObjectiveQuestComplete();
				break;
			case ChallengeObjectiveType.Scrap:
				baseChallengeObjective = new ChallengeObjectiveScrap();
				break;
			case ChallengeObjectiveType.Survive:
				baseChallengeObjective = new ChallengeObjectiveSurvive();
				break;
			case ChallengeObjectiveType.Trader:
				baseChallengeObjective = new ChallengeObjectiveTrader();
				break;
			case ChallengeObjectiveType.Wear:
				baseChallengeObjective = new ChallengeObjectiveWear();
				break;
			case ChallengeObjectiveType.Use:
				baseChallengeObjective = new ChallengeObjectiveUseItem();
				break;
			case ChallengeObjectiveType.ChallengeComplete:
				baseChallengeObjective = new ChallengeObjectiveChallengeComplete();
				break;
			case ChallengeObjectiveType.MeetTrader:
				baseChallengeObjective = new ChallengeObjectiveMeetTrader();
				break;
			case ChallengeObjectiveType.KillByTag:
				baseChallengeObjective = new ChallengeObjectiveKillByTag();
				break;
			case ChallengeObjectiveType.ChallengeStatAwarded:
				baseChallengeObjective = new ChallengeObjectiveChallengeStatAwarded();
				break;
			case ChallengeObjectiveType.SpendSkillPoint:
				baseChallengeObjective = new ChallengeObjectiveSpendSkillPoint();
				break;
			case ChallengeObjectiveType.Twitch:
				baseChallengeObjective = new ChallengeObjectiveTwitch();
				break;
			case ChallengeObjectiveType.Time:
				baseChallengeObjective = new ChallengeObjectiveTime();
				break;
			case ChallengeObjectiveType.GatherByTag:
				baseChallengeObjective = new ChallengeObjectiveGatherByTag();
				break;
			case ChallengeObjectiveType.LootContainer:
				baseChallengeObjective = new ChallengeObjectiveLootContainer();
				break;
			}
			if (baseChallengeObjective != null)
			{
				baseChallengeObjective.Read(_currentVersion, _br);
			}
			return baseChallengeObjective;
		}

		public virtual Recipe GetRecipeItem()
		{
			return null;
		}

		public virtual Recipe[] GetRecipeItems()
		{
			if (this.Owner.NeedsPreRequisites)
			{
				Recipe recipeFromRequirements = this.Owner.GetRecipeFromRequirements();
				if (recipeFromRequirements != null)
				{
					return new Recipe[] { recipeFromRequirements };
				}
			}
			return null;
		}

		public virtual void Read(byte _currentVersion, BinaryReader _br)
		{
			this.current = _br.ReadInt32();
		}

		public void WriteObjective(BinaryWriter _bw)
		{
			_bw.Write((byte)this.ObjectiveType);
			this.Write(_bw);
		}

		public virtual void Write(BinaryWriter _bw)
		{
			_bw.Write(this.current);
		}

		public virtual bool CheckObjectiveComplete(bool handleComplete = true)
		{
			this.HandleUpdatingCurrent();
			if (this.Current >= this.MaxCount)
			{
				this.Current = this.MaxCount;
				this.Complete = true;
				if (handleComplete)
				{
					this.Owner.HandleComplete(true);
				}
				return true;
			}
			if (handleComplete)
			{
				this.Owner.HandleComplete(true);
			}
			this.Complete = false;
			return false;
		}

		public virtual bool CheckBaseRequirements()
		{
			return this.Owner.ChallengeGroup.IsActive && (this.Biome != "" && (this.Owner.Owner.Player.biomeStandingOn == null || this.Owner.Owner.Player.biomeStandingOn.m_sBiomeName != this.Biome));
		}

		public virtual BaseChallengeObjective Clone()
		{
			return null;
		}

		public virtual void ParseElement(XElement e)
		{
			if (e.HasAttribute("count"))
			{
				this.MaxCount = StringParsers.ParseSInt32(e.GetAttribute("count"), 0, -1, NumberStyles.Integer);
			}
			if (e.HasAttribute("show_requirements"))
			{
				this.ShowRequirements = StringParsers.ParseBool(e.GetAttribute("show_requirements"), 0, -1, true);
			}
			if (e.HasAttribute("biome"))
			{
				this.Biome = e.GetAttribute("biome");
			}
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public virtual void Update(float deltaTime)
		{
		}

		public void HandleUpdate(float deltaTime)
		{
			if (this.Owner.IsActive)
			{
				this.Update(deltaTime);
			}
		}

		public virtual void CompleteObjective(bool handleComplete = true)
		{
			this.Current = this.MaxCount;
			this.CheckObjectiveComplete(handleComplete);
		}

		public static byte FileVersion = 1;

		public int MaxCount = 1;

		public bool IsRequirement;

		public Challenge Owner;

		public ChallengeClass OwnerClass;

		public string Biome = "";

		public bool ShowRequirements = true;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool complete;

		public bool IsTracking;

		[PublicizedFrom(EAccessModifier.Protected)]
		public int current;
	}
}
