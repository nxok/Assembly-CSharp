using System;
using System.Globalization;
using System.IO;
using System.Xml.Linq;
using UnityEngine.Scripting;

namespace Challenges
{
	[Preserve]
	public class ChallengeObjectiveTime : BaseChallengeObjective
	{
		public float CurrentTime
		{
			get
			{
				return this.currentTime;
			}
			set
			{
				this.currentTime = value;
				base.HandleValueChanged();
			}
		}

		public override ChallengeObjectiveType ObjectiveType
		{
			get
			{
				return ChallengeObjectiveType.Time;
			}
		}

		public override string DescriptionText
		{
			get
			{
				if (this.Biome == "")
				{
					return Localization.Get("challengeObjectiveTime", false, null) + ":";
				}
				return string.Format(Localization.Get("challengeObjectiveTimeInBiome", false, null), Localization.Get("biome_" + this.Biome, false, null));
			}
		}

		public override string StatusText
		{
			get
			{
				if (this.currentTime == 0f)
				{
					return string.Format("{0}{1}", this.currentTime, Localization.Get("timeAbbreviationSeconds", false, null)) + " / " + XUiM_PlayerBuffs.GetTimeString(this.maxTime);
				}
				return XUiM_PlayerBuffs.GetTimeString(this.currentTime) + "/ " + XUiM_PlayerBuffs.GetTimeString(this.maxTime);
			}
		}

		public override float FillAmount
		{
			get
			{
				return this.currentTime / this.maxTime;
			}
		}

		public override bool NeedsConstantUIUpdate
		{
			get
			{
				return !base.Complete;
			}
		}

		public override void Init()
		{
		}

		public override void HandleAddHooks()
		{
			QuestEventManager.Current.AddObjectiveToBeUpdated(this);
		}

		public override void HandleRemoveHooks()
		{
			QuestEventManager.Current.RemoveObjectiveToBeUpdated(this);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void Update(float deltaTime)
		{
			this.nextCheck -= deltaTime;
			if (this.nextCheck <= 0f)
			{
				this.isInvalid = this.CheckBaseRequirements();
				this.nextCheck = 2f;
			}
			if (this.player == null)
			{
				this.player = this.Owner.Owner.Player;
			}
			if (this.isInvalid || this.player == null || this.player.IsDead())
			{
				return;
			}
			this.CurrentTime += deltaTime;
			if (this.currentTime >= this.maxTime)
			{
				base.Current = this.MaxCount;
				this.CheckObjectiveComplete(true);
			}
		}

		public override void ParseElement(XElement e)
		{
			base.ParseElement(e);
			if (e.HasAttribute("max_time"))
			{
				this.maxTime = StringParsers.ParseFloat(e.GetAttribute("max_time"), 0, -1, NumberStyles.Any);
			}
		}

		public override void Read(byte _currentVersion, BinaryReader _br)
		{
			base.Read(_currentVersion, _br);
			this.currentTime = _br.ReadSingle();
		}

		public override void Write(BinaryWriter _bw)
		{
			base.Write(_bw);
			_bw.Write(this.currentTime);
		}

		public override void CopyValues(BaseChallengeObjective obj, BaseChallengeObjective objFromClass)
		{
			base.CopyValues(obj, objFromClass);
			ChallengeObjectiveTime challengeObjectiveTime = obj as ChallengeObjectiveTime;
			if (challengeObjectiveTime != null)
			{
				this.currentTime = challengeObjectiveTime.currentTime;
			}
		}

		public override BaseChallengeObjective Clone()
		{
			return new ChallengeObjectiveTime
			{
				Biome = this.Biome,
				maxTime = this.maxTime,
				currentTime = this.currentTime,
				nextCheck = this.nextCheck,
				isInvalid = this.isInvalid
			};
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public float currentTime;

		[PublicizedFrom(EAccessModifier.Protected)]
		public float maxTime;

		[PublicizedFrom(EAccessModifier.Protected)]
		public float nextCheck;

		[PublicizedFrom(EAccessModifier.Protected)]
		public bool isInvalid = true;

		[PublicizedFrom(EAccessModifier.Private)]
		public EntityPlayerLocal player;
	}
}
