using System;
using Challenges;
using UnityEngine;

public class RecipeUnlockData
{
	public RecipeUnlockData.UnlockTypes UnlockType
	{
		get
		{
			if (this.unlockText != "")
			{
				this.Init();
			}
			return this.unlockType;
		}
		set
		{
			this.unlockType = value;
		}
	}

	public ItemClass Item
	{
		get
		{
			return this.item;
		}
		set
		{
			this.item = value;
			this.unlockType = RecipeUnlockData.UnlockTypes.Schematic;
		}
	}

	public ProgressionClass Perk
	{
		get
		{
			return this.perk;
		}
		set
		{
			this.perk = value;
			if (this.perk.IsBook)
			{
				this.unlockType = RecipeUnlockData.UnlockTypes.Book;
				return;
			}
			if (this.perk.IsCrafting)
			{
				this.unlockType = RecipeUnlockData.UnlockTypes.Skill;
				return;
			}
			this.unlockType = RecipeUnlockData.UnlockTypes.Perk;
		}
	}

	public ChallengeGroup ChallengeGroup
	{
		get
		{
			return this.challengeGroup;
		}
		set
		{
			this.challengeGroup = value;
			this.unlockType = RecipeUnlockData.UnlockTypes.ChallengeGroup;
		}
	}

	public ChallengeClass Challenge
	{
		get
		{
			return this.challenge;
		}
		set
		{
			this.challenge = value;
			this.unlockType = RecipeUnlockData.UnlockTypes.Challenge;
		}
	}

	public RecipeUnlockData(string unlock)
	{
		this.unlockText = unlock;
	}

	public void Init()
	{
		if (Progression.ProgressionClasses.ContainsKey(this.unlockText))
		{
			this.Perk = Progression.ProgressionClasses[this.unlockText];
			return;
		}
		ChallengeGroup group;
		if ((group = ChallengeGroup.GetGroup(this.unlockText)) != null)
		{
			this.ChallengeGroup = group;
			return;
		}
		ChallengeClass challengeClass;
		if ((challengeClass = ChallengeClass.GetChallenge(this.unlockText)) != null)
		{
			this.Challenge = challengeClass;
			return;
		}
		ItemClass itemClass = ItemClass.GetItemClass(this.unlockText, true);
		if (itemClass != null)
		{
			this.Item = itemClass;
			return;
		}
		this.UnlockType = (GameManager.Instance.IsEditMode() ? RecipeUnlockData.UnlockTypes.PrefabEditorInvalid : RecipeUnlockData.UnlockTypes.None);
	}

	public string GetName()
	{
		if (this.unlockText != "")
		{
			this.Init();
		}
		if (this.unlockType == RecipeUnlockData.UnlockTypes.Schematic)
		{
			return this.item.GetLocalizedItemName();
		}
		if (this.unlockType == RecipeUnlockData.UnlockTypes.ChallengeGroup)
		{
			return this.ChallengeGroup.Title;
		}
		if (this.unlockType == RecipeUnlockData.UnlockTypes.Challenge)
		{
			return this.Challenge.Title;
		}
		return Localization.Get(this.perk.NameKey, false, null);
	}

	public string GetIcon()
	{
		if (this.unlockText != "")
		{
			this.Init();
		}
		if (this.unlockType == RecipeUnlockData.UnlockTypes.Schematic)
		{
			return "ui_game_symbol_book";
		}
		if (this.unlockType == RecipeUnlockData.UnlockTypes.Perk)
		{
			return "ui_game_symbol_skills";
		}
		if (this.unlockType == RecipeUnlockData.UnlockTypes.Skill)
		{
			return "ui_game_symbol_hammer";
		}
		if (this.unlockType == RecipeUnlockData.UnlockTypes.ChallengeGroup || this.unlockType == RecipeUnlockData.UnlockTypes.Challenge)
		{
			return "ui_game_symbol_challenge";
		}
		return "ui_game_symbol_book";
	}

	public string GetLevel(EntityPlayerLocal player, string recipeName)
	{
		if (this.unlockText != "")
		{
			this.Init();
		}
		if (this.unlockType == RecipeUnlockData.UnlockTypes.Skill)
		{
			for (int i = 0; i < this.perk.DisplayDataList.Count; i++)
			{
				ProgressionClass.DisplayData displayData = this.perk.DisplayDataList[i];
				if (displayData.Enabled)
				{
					for (int j = 0; j < displayData.UnlockDataList.Count; j++)
					{
						ProgressionClass.DisplayData.UnlockData unlockData = displayData.UnlockDataList[j];
						if (unlockData.ItemName == recipeName || (unlockData.RecipeList != null && unlockData.RecipeList.ContainsCaseInsensitive(recipeName)))
						{
							int unlockTier = unlockData.UnlockTier;
							ProgressionValue progressionValue = player.Progression.GetProgressionValue(this.perk.Name);
							return string.Format("{0}/{1}", progressionValue.Level, displayData.QualityStarts[unlockTier].ToString());
						}
					}
				}
			}
			return "--";
		}
		return "--";
	}

	public string GetIconAtlas()
	{
		return "UIAtlas";
	}

	public Color GetItemTint()
	{
		if (this.unlockText != "")
		{
			this.Init();
		}
		if (this.unlockType == RecipeUnlockData.UnlockTypes.Schematic && this.item != null)
		{
			return this.item.GetIconTint(null);
		}
		return Color.white;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public RecipeUnlockData.UnlockTypes unlockType;

	[PublicizedFrom(EAccessModifier.Private)]
	public string unlockText = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemClass item;

	[PublicizedFrom(EAccessModifier.Private)]
	public ProgressionClass perk;

	[PublicizedFrom(EAccessModifier.Private)]
	public ChallengeGroup challengeGroup;

	[PublicizedFrom(EAccessModifier.Private)]
	public ChallengeClass challenge;

	public enum UnlockTypes
	{
		None,
		Perk,
		Book,
		Skill,
		Schematic,
		ChallengeGroup,
		Challenge,
		PrefabEditorInvalid
	}
}
