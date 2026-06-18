using System;
using System.Collections.Generic;
using Challenges;
using UniLinq;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_ChallengeGroupEntry : XUiController
{
	public ChallengeGroupEntry Entry
	{
		get
		{
			return this.entry;
		}
		set
		{
			base.ViewComponent.Enabled = value != null;
			this.entry = value;
			this.group = ((this.entry != null) ? this.entry.ChallengeGroup : null);
			this.IsDirty = true;
		}
	}

	public override void Init()
	{
		base.Init();
		this.ChallengeList = base.GetChildByType<XUiC_ChallengeEntryList>();
		this.IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string value, string bindingName)
	{
		bool flag = this.group != null;
		uint num = <PrivateImplementationDetails>.ComputeStringHash(bindingName);
		if (num <= 1566407741U)
		{
			if (num != 541430242U)
			{
				if (num != 694121954U)
				{
					if (num == 1566407741U)
					{
						if (bindingName == "hasentry")
						{
							value = (flag ? "true" : "false");
							return true;
						}
					}
				}
				else if (bindingName == "hasreset")
				{
					value = (flag ? (this.group.DayReset != -1).ToString() : "false");
					return true;
				}
			}
			else if (bindingName == "intro_disabled")
			{
				value = (flag ? (!ChallengeJournal.IntroChallengesEnabled && this.group.IsIntro).ToString() : "false");
				return true;
			}
		}
		else if (num <= 2446454527U)
		{
			if (num != 2444835803U)
			{
				if (num == 2446454527U)
				{
					if (bindingName == "groupobjective")
					{
						value = (flag ? this.group.ObjectiveText : "");
						return true;
					}
				}
			}
			else if (bindingName == "groupreward")
			{
				value = (flag ? this.group.RewardText : "");
				return true;
			}
		}
		else if (num != 3504806855U)
		{
			if (num == 4037261514U)
			{
				if (bindingName == "resetday")
				{
					if (flag)
					{
						value = ((this.group.DayReset == -1) ? "" : this.entry.LastUpdateDay.ToString());
					}
					else
					{
						value = "";
					}
					return true;
				}
			}
		}
		else if (bindingName == "groupname")
		{
			value = "";
			if (flag)
			{
				value = this.group.Title;
			}
			return true;
		}
		return base.GetBindingValueInternal(ref value, bindingName);
	}

	public override void Update(float _dt)
	{
		if (this.group != null && this.group.UIDirty)
		{
			this.IsDirty = true;
			this.group.UIDirty = false;
		}
		if (this.IsDirty)
		{
			this.currentItems = this.player.challengeJournal.Challenges.Where((Challenge item) => item.ChallengeGroup == this.group).ToList<Challenge>();
			if (this.ChallengeList != null)
			{
				this.ChallengeList.Owner = this;
				this.ChallengeList.SetChallengeEntryList(this.currentItems);
			}
			base.RefreshBindings();
			this.IsDirty = false;
		}
		base.Update(_dt);
	}

	public void Select()
	{
		this.Owner.SelectedGroup = this;
	}

	public void UnSelect()
	{
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.player = this.xui.playerUI.entityPlayer;
		this.IsDirty = true;
	}

	public void Refresh()
	{
		this.IsDirty = true;
	}

	public bool IsHovered;

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityPlayerLocal player;

	public XUiC_ChallengeEntryList ChallengeList;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<Challenge> currentItems;

	[PublicizedFrom(EAccessModifier.Private)]
	public ChallengeGroup group;

	[PublicizedFrom(EAccessModifier.Private)]
	public ChallengeGroupEntry entry;

	public XUiC_ChallengeGroupList Owner;
}
