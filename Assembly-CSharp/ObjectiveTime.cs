using System;
using System.Globalization;
using System.IO;
using Twitch;
using UnityEngine.Scripting;

[Preserve]
public class ObjectiveTime : BaseObjective
{
	public override BaseObjective.ObjectiveValueTypes ObjectiveValueType
	{
		get
		{
			return BaseObjective.ObjectiveValueTypes.Time;
		}
	}

	public override bool UpdateUI
	{
		get
		{
			return true;
		}
	}

	public override bool ShowInQuestLog
	{
		get
		{
			return false;
		}
	}

	public override string StatusText
	{
		get
		{
			if (this.currentTime > 0f)
			{
				return XUiM_PlayerBuffs.GetTimeString(this.currentTime);
			}
			if (base.Optional)
			{
				base.ObjectiveState = BaseObjective.ObjectiveStates.Failed;
				return Localization.Get("failed", false, null);
			}
			base.ObjectiveState = BaseObjective.ObjectiveStates.Complete;
			return Localization.Get("completed", false, null);
		}
	}

	public override void SetupObjective()
	{
		this.keyword = Localization.Get("ObjectiveTime_keyword", false, null);
		this.dayLengthInSeconds = GamePrefs.GetInt(EnumGamePrefs.DayNightLength) * 60;
	}

	public override void SetupDisplay()
	{
		base.Description = string.Format("{0}:", this.keyword);
	}

	public override void AddHooks()
	{
		QuestEventManager.Current.AddObjectiveToBeUpdated(this);
	}

	public override void RemoveHooks()
	{
		QuestEventManager.Current.RemoveObjectiveToBeUpdated(this);
	}

	public override void Refresh()
	{
		this.SetupDisplay();
		if (base.Optional)
		{
			base.Complete = this.currentTime > 0f;
		}
		else
		{
			base.Complete = this.currentTime <= 0f;
		}
		if (base.Complete)
		{
			base.OwnerQuest.RefreshQuestCompletion(QuestClass.CompletionTypes.AutoComplete, null, true, null);
		}
	}

	public override void Read(BinaryReader _br)
	{
		this.currentTime = (float)_br.ReadUInt16();
		this.currentValue = 1;
	}

	public override void Write(BinaryWriter _bw)
	{
		_bw.Write((ushort)this.currentTime);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void CopyValues(BaseObjective objective)
	{
		base.CopyValues(objective);
		ObjectiveTime objectiveTime = (ObjectiveTime)objective;
		objectiveTime.currentTime = this.currentTime;
		objectiveTime.overrideType = this.overrideType;
		objectiveTime.overrideOffset = this.overrideOffset;
	}

	public override BaseObjective Clone()
	{
		ObjectiveTime objectiveTime = new ObjectiveTime();
		this.CopyValues(objectiveTime);
		return objectiveTime;
	}

	public override void Update(float updateTime)
	{
		if (this.firstRun)
		{
			if (this.overrideType == ObjectiveTime.OverrideTypes.VoteTime && TwitchManager.HasInstance && TwitchManager.Current.IsVoting)
			{
				this.currentTime = TwitchManager.Current.VotingManager.VoteTimeRemaining + this.overrideOffset;
				this.firstRun = false;
				return;
			}
			if (this.Value.EqualsCaseInsensitive("day"))
			{
				this.currentTime = (float)this.dayLengthInSeconds;
			}
			else
			{
				this.currentTime = StringParsers.ParseFloat(this.Value, 0, -1, NumberStyles.Any);
			}
			this.firstRun = false;
		}
		this.currentTime -= updateTime;
		if (this.currentTime < 0f)
		{
			this.Refresh();
			base.HandleRemoveHooks();
		}
	}

	public override void HandleFailed()
	{
		this.currentTime = 0f;
		base.Complete = false;
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		properties.ParseString(ObjectiveTime.PropTime, ref this.Value);
		properties.ParseEnum<ObjectiveTime.OverrideTypes>(ObjectiveTime.PropOverrideType, ref this.overrideType);
		properties.ParseFloat(ObjectiveTime.PropOverrideOffset, ref this.overrideOffset);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public float currentTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public ObjectiveTime.OverrideTypes overrideType;

	[PublicizedFrom(EAccessModifier.Private)]
	public float overrideOffset;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool firstRun = true;

	public static string PropTime = "time";

	public static string PropOverrideType = "override_type";

	public static string PropOverrideOffset = "override_offset";

	[PublicizedFrom(EAccessModifier.Private)]
	public int dayLengthInSeconds;

	[PublicizedFrom(EAccessModifier.Private)]
	public enum OverrideTypes
	{
		None,
		VoteTime
	}
}
