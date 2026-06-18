using System;
using Twitch;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_TwitchVoteEntry : XUiController
{
	public XUiC_TwitchWindow Owner { get; set; }

	public TwitchVoteEntry Vote
	{
		get
		{
			return this.vote;
		}
		set
		{
			this.vote = value;
			this.IsDirty = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string value, string bindingName)
	{
		bool flag = this.vote != null;
		uint num = <PrivateImplementationDetails>.ComputeStringHash(bindingName);
		if (num <= 3057604319U)
		{
			if (num <= 2104701544U)
			{
				if (num != 546437152U)
				{
					if (num != 596770009U)
					{
						if (num == 2104701544U)
						{
							if (bindingName == "votecolor")
							{
								if (flag)
								{
									if (this.vote.Owner.IsHighest(this.vote))
									{
										value = this.positiveColor;
									}
									else
									{
										value = this.negativeColor;
									}
								}
								else
								{
									value = "0,0,0,0";
								}
								return true;
							}
						}
					}
					else if (bindingName == "hasvoteline2")
					{
						if (flag)
						{
							value = (this.vote.VoteClass.VoteLine2 != "").ToString();
						}
						else
						{
							value = "false";
						}
						return true;
					}
				}
				else if (bindingName == "hasvoteline1")
				{
					if (flag)
					{
						value = (this.vote.VoteClass.VoteLine1 != "").ToString();
					}
					else
					{
						value = "false";
					}
					return true;
				}
			}
			else if (num != 2382705624U)
			{
				if (num != 2840813878U)
				{
					if (num == 3057604319U)
					{
						if (bindingName == "voteline2")
						{
							if (flag)
							{
								value = this.vote.VoteClass.VoteLine2;
							}
							return true;
						}
					}
				}
				else if (bindingName == "votename")
				{
					if (flag)
					{
						if (this.vote.Index == 2 && this.vote.Owner.UseMystery)
						{
							value = "?????";
						}
						else
						{
							value = this.vote.VoteClass.Display;
						}
					}
					else
					{
						value = "";
					}
					return true;
				}
			}
			else if (bindingName == "votefill")
			{
				if (flag)
				{
					if (this.vote.VoteCount > 0)
					{
						value = ((float)this.vote.VoteCount / (float)this.vote.Owner.VoteCount).ToString();
					}
					else
					{
						value = "0";
					}
				}
				else
				{
					value = "0";
				}
				return true;
			}
		}
		else if (num <= 3604996386U)
		{
			if (num != 3074381938U)
			{
				if (num != 3366025627U)
				{
					if (num == 3604996386U)
					{
						if (bindingName == "votecommand")
						{
							if (this.isWinner)
							{
								value = "";
							}
							else
							{
								value = (flag ? this.vote.VoteCommand : "");
							}
							return true;
						}
					}
				}
				else if (bindingName == "hasvote")
				{
					value = flag.ToString();
					return true;
				}
			}
			else if (bindingName == "voteline1")
			{
				if (flag)
				{
					value = this.vote.VoteClass.VoteLine1;
				}
				return true;
			}
		}
		else if (num != 3735793316U)
		{
			if (num != 3918524111U)
			{
				if (num == 4025064268U)
				{
					if (bindingName == "line1textcolor")
					{
						if (flag)
						{
							switch (this.vote.VoteClass.DisplayType)
							{
							case TwitchVote.VoteDisplayTypes.GoodBad:
								value = this.textBadColor;
								break;
							case TwitchVote.VoteDisplayTypes.Special:
								value = this.textGoodColor;
								break;
							case TwitchVote.VoteDisplayTypes.HordeBuffed:
								value = this.selectedTextColor;
								break;
							}
						}
						else
						{
							value = "255,255,255";
						}
						return true;
					}
				}
			}
			else if (bindingName == "line2textcolor")
			{
				if (flag)
				{
					if (this.vote.VoteClass.DisplayType == TwitchVote.VoteDisplayTypes.GoodBad)
					{
						value = this.textBadColor;
					}
				}
				else
				{
					value = "255,255,255";
				}
				return true;
			}
		}
		else if (bindingName == "votecount")
		{
			if (flag)
			{
				if (!this.isWinner)
				{
					float num2 = 0f;
					if (this.vote.VoteCount > 0)
					{
						num2 = (float)this.vote.VoteCount / (float)this.vote.Owner.VoteCount;
					}
					value = this.voteCountFormatterInt.Format((int)(num2 * 100f));
				}
				else
				{
					value = "";
				}
			}
			else
			{
				value = "";
			}
			return true;
		}
		return base.GetBindingValueInternal(ref value, bindingName);
	}

	public override bool ParseAttribute(string name, string value)
	{
		if (name == "positive_color")
		{
			this.positiveColor = value;
			return true;
		}
		if (name == "negative_color")
		{
			this.negativeColor = value;
			return true;
		}
		if (name == "disabled_color")
		{
			this.disabledColor = value;
			return true;
		}
		if (name == "selected_color")
		{
			this.selectedTextColor = value;
			return true;
		}
		if (name == "bad_color")
		{
			this.textBadColor = value;
			return true;
		}
		if (!(name == "good_color"))
		{
			return base.ParseAttribute(name, value);
		}
		this.textGoodColor = value;
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleOnCountChanged(XUiController _sender, OnCountChangedEventArgs _e)
	{
		base.RefreshBindings();
	}

	public override void Update(float _dt)
	{
		if (this.Vote != null && this.Vote.UIDirty)
		{
			this.IsDirty = true;
			this.Vote.UIDirty = false;
		}
		if (this.IsDirty)
		{
			base.RefreshBindings();
			this.IsDirty = false;
		}
		base.Update(_dt);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public TwitchVoteEntry vote;

	public bool isWinner;

	[PublicizedFrom(EAccessModifier.Private)]
	public string positiveColor = "0,0,255";

	[PublicizedFrom(EAccessModifier.Private)]
	public string negativeColor = "255,0,0";

	[PublicizedFrom(EAccessModifier.Private)]
	public string textBadColor = "255,175,175";

	[PublicizedFrom(EAccessModifier.Private)]
	public string textGoodColor = "175,175,255";

	[PublicizedFrom(EAccessModifier.Private)]
	public string selectedTextColor = "222,206,163";

	[PublicizedFrom(EAccessModifier.Private)]
	public string disabledColor = "80,80,80";

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isReady;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<int> voteCountFormatterInt = new CachedStringFormatter<int>((int _i) => _i.ToString() + "%");
}
