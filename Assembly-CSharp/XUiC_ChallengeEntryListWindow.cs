using System;
using System.Collections.Generic;
using Challenges;
using UniLinq;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_ChallengeEntryListWindow : XUiController
{
	public override void Init()
	{
		base.Init();
		this.categoryList = base.GetChildByType<XUiC_CategoryList>();
		XUiC_ChallengeGroupList[] childrenByType = base.GetChildrenByType<XUiC_ChallengeGroupList>(null);
		if (childrenByType != null)
		{
			foreach (XUiC_ChallengeGroupList xuiC_ChallengeGroupList in childrenByType)
			{
				xuiC_ChallengeGroupList.ChallengeEntryListWindow = this;
				xuiC_ChallengeGroupList.CategoryList = this.categoryList;
				this.challengeGroupList.Add(xuiC_ChallengeGroupList);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ChallengeGroupList GetListFromKey(string key)
	{
		XUiC_ChallengeGroupList xuiC_ChallengeGroupList = null;
		for (int i = 0; i < this.challengeGroupList.Count; i++)
		{
			XUiC_ChallengeGroupList xuiC_ChallengeGroupList2 = this.challengeGroupList[i];
			if (xuiC_ChallengeGroupList2.DisplayKey.EqualsCaseInsensitive(key))
			{
				xuiC_ChallengeGroupList = xuiC_ChallengeGroupList2;
				xuiC_ChallengeGroupList2.ViewComponent.IsVisible = true;
			}
			else
			{
				xuiC_ChallengeGroupList2.ViewComponent.IsVisible = false;
			}
		}
		return xuiC_ChallengeGroupList;
	}

	public override void Update(float _dt)
	{
		if (this.IsDirty)
		{
			this.challengeList = this.GetListFromKey(this.displayKey);
			this.challengeList.SetChallengeGroupEntryList(this.player.challengeJournal.ChallengeGroups, this.categoryChange);
			this.IsDirty = false;
			this.categoryChange = false;
		}
		base.Update(_dt);
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.player = this.xui.playerUI.entityPlayer;
		if (this.categoryList != null)
		{
			this.categoryList.SetupCategoriesBasedOnChallengeCategories(ChallengeCategory.s_ChallengeCategories.Values.ToList<ChallengeCategory>());
		}
		if (this.categoryList != null)
		{
			this.categoryList.CategoryChanged += this.CategoryList_CategoryChanged;
		}
		if (this.categoryList != null && (this.categoryList.CurrentCategory == null || this.categoryList.CurrentCategory.SpriteName == ""))
		{
			this.categoryList.SetCategoryToFirst();
		}
		this.IsDirty = true;
	}

	public override void OnClose()
	{
		if (this.categoryList != null)
		{
			this.categoryList.CategoryChanged -= this.CategoryList_CategoryChanged;
		}
		base.OnClose();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CategoryList_CategoryChanged(XUiC_CategoryEntry _categoryEntry)
	{
		this.displayKey = ChallengeCategory.s_ChallengeCategories[_categoryEntry.CategoryName].DisplayKey;
		this.IsDirty = true;
		this.categoryChange = true;
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public void SetSelectedByUnlockData(RecipeUnlockData unlockData)
	{
		string text = "";
		RecipeUnlockData.UnlockTypes unlockType = unlockData.UnlockType;
		if (unlockType != RecipeUnlockData.UnlockTypes.ChallengeGroup)
		{
			if (unlockType == RecipeUnlockData.UnlockTypes.Challenge)
			{
				this.displayKey = ChallengeCategory.s_ChallengeCategories[unlockData.Challenge.ChallengeGroup.Category].DisplayKey;
				text = unlockData.Challenge.ChallengeGroup.Category;
			}
		}
		else
		{
			this.displayKey = ChallengeCategory.s_ChallengeCategories[unlockData.ChallengeGroup.Category].DisplayKey;
			text = unlockData.ChallengeGroup.Category;
		}
		if (this.categoryList != null)
		{
			this.categoryList.CategoryChanged -= this.CategoryList_CategoryChanged;
			this.categoryList.SetCategory(text);
			this.categoryList.CategoryChanged += this.CategoryList_CategoryChanged;
		}
		this.IsDirty = true;
		this.categoryChange = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityPlayerLocal player;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ChallengeGroupList challengeList;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput txtInput;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_CategoryList categoryList;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool categoryChange;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<XUiC_ChallengeGroupList> challengeGroupList = new List<XUiC_ChallengeGroupList>();

	[PublicizedFrom(EAccessModifier.Private)]
	public string displayKey = "";
}
