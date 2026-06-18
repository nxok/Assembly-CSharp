using System;
using System.Collections.Generic;
using Challenges;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_ChallengeGroupList : XUiController
{
	public XUiC_ChallengeGroupEntry SelectedGroup
	{
		get
		{
			return this.selectedGroup;
		}
		set
		{
			if (this.selectedGroup != null)
			{
				this.selectedGroup.UnSelect();
			}
			this.selectedGroup = value;
		}
	}

	public override void Init()
	{
		base.Init();
		XUiC_ChallengeWindowGroup xuiC_ChallengeWindowGroup = (XUiC_ChallengeWindowGroup)base.WindowGroup.Controller;
		for (int i = 0; i < this.children.Count; i++)
		{
			if (this.children[i] is XUiC_ChallengeGroupEntry)
			{
				XUiC_ChallengeGroupEntry xuiC_ChallengeGroupEntry = (XUiC_ChallengeGroupEntry)this.children[i];
				xuiC_ChallengeGroupEntry.Owner = this;
				this.entryList.Add(xuiC_ChallengeGroupEntry);
			}
		}
	}

	public override void Update(float _dt)
	{
		if (this.CategoryList.CurrentCategory != null && this.IsDirty)
		{
			this.updateChallengeEntries();
			this.IsDirty = false;
			this.categoryChange = false;
		}
		base.Update(_dt);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateChallengeEntries()
	{
		if (this.challengeGroupList == null)
		{
			return;
		}
		string categoryName = this.CategoryList.CurrentCategory.CategoryName;
		int num = 0;
		for (int i = 0; i < this.challengeGroupList.Count; i++)
		{
			XUiC_ChallengeGroupEntry xuiC_ChallengeGroupEntry = this.entryList[num];
			if (xuiC_ChallengeGroupEntry != null && this.challengeGroupList[i].ChallengeGroup.Category.EqualsCaseInsensitive(categoryName))
			{
				xuiC_ChallengeGroupEntry.Entry = this.challengeGroupList[i];
				xuiC_ChallengeGroupEntry.ViewComponent.SoundPlayOnClick = true;
				if (this.categoryChange)
				{
					XUiC_ChallengeEntryList.SelectedEntry = null;
				}
				num++;
			}
			if (num >= this.entryList.Count)
			{
				break;
			}
		}
		for (int j = num; j < this.entryList.Count; j++)
		{
			XUiC_ChallengeGroupEntry xuiC_ChallengeGroupEntry2 = this.entryList[j];
			xuiC_ChallengeGroupEntry2.Entry = null;
			xuiC_ChallengeGroupEntry2.ViewComponent.SoundPlayOnClick = false;
		}
	}

	public void SetChallengeGroupEntryList(List<ChallengeGroupEntry> newChallengeGroupList, bool newCategoryChange)
	{
		this.challengeGroupList = newChallengeGroupList;
		if (this.CategoryList != null && this.CategoryList.CurrentCategory == null)
		{
			this.CategoryList.SetCategoryToFirst();
		}
		this.categoryChange = newCategoryChange;
		if (this.xui.QuestTracker.TrackedChallenge != null)
		{
			ChallengeGroup challengeGroup = this.xui.QuestTracker.TrackedChallenge.ChallengeGroup;
			int num = 0;
			while (num < this.challengeGroupList.Count && this.challengeGroupList[num].ChallengeGroup != challengeGroup)
			{
				num++;
			}
		}
		this.IsDirty = true;
	}

	public override bool ParseAttribute(string name, string value)
	{
		if (name == "display_key")
		{
			this.DisplayKey = value;
			return true;
		}
		return base.ParseAttribute(name, value);
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.player = this.xui.playerUI.entityPlayer;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityPlayerLocal player;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<XUiC_ChallengeGroupEntry> entryList = new List<XUiC_ChallengeGroupEntry>();

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ChallengeGroupEntry selectedGroup;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool categoryChange;

	public string DisplayKey = "";

	public XUiC_CategoryList CategoryList;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<ChallengeGroupEntry> challengeGroupList;

	public XUiC_ChallengeEntryListWindow ChallengeEntryListWindow;
}
