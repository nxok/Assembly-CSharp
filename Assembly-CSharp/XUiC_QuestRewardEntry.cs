using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_QuestRewardEntry : XUiController
{
	public BaseReward Reward
	{
		get
		{
			return this.reward;
		}
		set
		{
			this.reward = value;
			this.IsDirty = true;
		}
	}

	public bool ChainQuest { get; set; }

	public static string ChainQuestTypeKeyword
	{
		get
		{
			if (XUiC_QuestRewardEntry.chainQuestTypeKeyword == "")
			{
				XUiC_QuestRewardEntry.chainQuestTypeKeyword = Localization.Get("RewardTypeChainQuest", false, null);
			}
			return XUiC_QuestRewardEntry.chainQuestTypeKeyword;
		}
	}

	public static string QuestTypeKeyword
	{
		get
		{
			if (XUiC_QuestRewardEntry.questTypeKeyword == "")
			{
				XUiC_QuestRewardEntry.questTypeKeyword = Localization.Get("RewardTypeQuest", false, null);
			}
			return XUiC_QuestRewardEntry.questTypeKeyword;
		}
	}

	public static string OptionalKeyword
	{
		get
		{
			if (XUiC_QuestRewardEntry.optionalKeyword == "")
			{
				XUiC_QuestRewardEntry.optionalKeyword = Localization.Get("optional", false, null);
			}
			return XUiC_QuestRewardEntry.optionalKeyword;
		}
	}

	public static string BonusKeyword
	{
		get
		{
			if (XUiC_QuestRewardEntry.bonusKeyword == "")
			{
				XUiC_QuestRewardEntry.bonusKeyword = Localization.Get("bonus", false, null);
			}
			return XUiC_QuestRewardEntry.bonusKeyword;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string value, string bindingName)
	{
		bool flag = this.reward != null;
		uint num = <PrivateImplementationDetails>.ComputeStringHash(bindingName);
		if (num <= 1771022199U)
		{
			if (num != 463969434U)
			{
				if (num != 1455818684U)
				{
					if (num == 1771022199U)
					{
						if (bindingName == "rewardicon")
						{
							value = (flag ? this.reward.Icon : "");
							return true;
						}
					}
				}
				else if (bindingName == "rewarddescription")
				{
					value = (flag ? this.reward.Description : "");
					return true;
				}
			}
			else if (bindingName == "rewardtype")
			{
				if (flag)
				{
					if (this.ChainQuest)
					{
						value = XUiC_QuestRewardEntry.ChainQuestTypeKeyword;
					}
					else
					{
						string text = (this.reward.Optional ? XUiC_QuestRewardEntry.BonusKeyword : Localization.Get(QuestClass.GetQuest(this.Reward.OwnerQuest.ID).Category, false, null));
						value = this.rewardTypeFormatter.Format(text, XUiC_QuestRewardEntry.QuestTypeKeyword);
					}
				}
				else
				{
					value = "";
				}
				return true;
			}
		}
		else if (num <= 2048631988U)
		{
			if (num != 1864291747U)
			{
				if (num == 2048631988U)
				{
					if (bindingName == "hasreward")
					{
						value = flag.ToString();
						return true;
					}
				}
			}
			else if (bindingName == "rewardvalue")
			{
				value = (flag ? this.reward.ValueText : "");
				return true;
			}
		}
		else if (num != 3261050802U)
		{
			if (num == 3763801510U)
			{
				if (bindingName == "rewardiconatlas")
				{
					value = (flag ? this.reward.IconAtlas : "");
					return true;
				}
			}
		}
		else if (bindingName == "rewardoptional")
		{
			value = (flag ? (this.reward.Optional ? this.rewardOptionalFormatter.Format(XUiC_QuestRewardEntry.OptionalKeyword) : "") : "");
			return true;
		}
		return base.GetBindingValueInternal(ref value, bindingName);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleOnCountChanged(XUiController _sender, OnCountChangedEventArgs _e)
	{
		this.IsDirty = true;
	}

	public override void Update(float _dt)
	{
		base.RefreshBindings();
		this.IsDirty = false;
		base.Update(_dt);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string completeIconName = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public string incompleteIconName = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public string completeColor = "0,255,0,255";

	[PublicizedFrom(EAccessModifier.Private)]
	public string incompleteColor = "255,0,0,255";

	[PublicizedFrom(EAccessModifier.Private)]
	public BaseReward reward;

	[PublicizedFrom(EAccessModifier.Private)]
	public static string chainQuestTypeKeyword = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public static string questTypeKeyword = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public static string optionalKeyword = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public static string bonusKeyword = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<string> rewardOptionalFormatter = new CachedStringFormatter<string>((string _s) => "(" + _s + ") ");

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<string, string> rewardTypeFormatter = new CachedStringFormatter<string, string>((string _s1, string _s2) => _s1 + " " + _s2);
}
