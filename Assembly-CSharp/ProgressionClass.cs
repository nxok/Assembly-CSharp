using System;
using System.Collections.Generic;
using UnityEngine;

public class ProgressionClass
{
	public bool IsBookGroup
	{
		get
		{
			return this.Type == ProgressionType.BookGroup;
		}
	}

	public bool IsBook
	{
		get
		{
			return this.Type == ProgressionType.Book;
		}
	}

	public bool ValidDisplay(ProgressionClass.DisplayTypes displayType)
	{
		switch (displayType)
		{
		case ProgressionClass.DisplayTypes.Standard:
			return this.Type != ProgressionType.BookGroup && this.Type != ProgressionType.Crafting;
		case ProgressionClass.DisplayTypes.Book:
			return this.Type == ProgressionType.BookGroup;
		case ProgressionClass.DisplayTypes.Crafting:
			return this.Type == ProgressionType.Crafting;
		default:
			return false;
		}
	}

	public ProgressionCurrencyType CurrencyType
	{
		get
		{
			switch (this.Type)
			{
			case ProgressionType.Attribute:
				return ProgressionCurrencyType.SP;
			case ProgressionType.Skill:
				return ProgressionCurrencyType.XP;
			case ProgressionType.Perk:
				return ProgressionCurrencyType.SP;
			default:
				return ProgressionCurrencyType.None;
			}
		}
	}

	public ProgressionClass Parent
	{
		get
		{
			if (this.ParentName == null)
			{
				return this;
			}
			ProgressionClass progressionClass;
			if (Progression.ProgressionClasses.TryGetValue(this.ParentName, out progressionClass))
			{
				return progressionClass;
			}
			return null;
		}
	}

	public bool IsPerk
	{
		get
		{
			return this.Type == ProgressionType.Perk;
		}
	}

	public bool IsSkill
	{
		get
		{
			return this.Type == ProgressionType.Skill;
		}
	}

	public bool IsAttribute
	{
		get
		{
			return this.Type == ProgressionType.Attribute;
		}
	}

	public bool IsCrafting
	{
		get
		{
			return this.Type == ProgressionType.Crafting;
		}
	}

	public float ListSortOrder
	{
		get
		{
			if (this.IsPerk)
			{
				return this.Parent.ListSortOrder + this.listSortOrder * 0.001f;
			}
			if (this.IsSkill)
			{
				return this.Parent.ListSortOrder + this.listSortOrder;
			}
			return this.listSortOrder * 100f;
		}
		set
		{
			this.listSortOrder = value;
		}
	}

	public ProgressionClass.DisplayData AddDisplayData(string _item, int[] _qualityStarts, string[] _customIcon, string[] _customIconTint, string[] _customName, bool _customHasQuality)
	{
		if (this.DisplayDataList == null)
		{
			this.DisplayDataList = new List<ProgressionClass.DisplayData>();
		}
		ProgressionClass.DisplayData displayData = new ProgressionClass.DisplayData
		{
			ItemName = _item,
			QualityStarts = _qualityStarts,
			Owner = this,
			CustomIcon = _customIcon,
			CustomIconTint = _customIconTint,
			CustomName = _customName,
			CustomHasQuality = _customHasQuality
		};
		this.DisplayDataList.Add(displayData);
		return displayData;
	}

	public ProgressionClass(string _name)
	{
		this.Name = _name;
		this.NameKey = this.Name;
		this.NameTag = FastTags<TagGroup.Global>.GetTag(_name);
		this.DescKey = "";
		this.ListSortOrder = float.MaxValue;
		this.ParentName = null;
		this.Type = ProgressionType.None;
	}

	public void ModifyValue(EntityAlive _ea, ProgressionValue _pv, PassiveEffects _effect, ref float _base_value, ref float _perc_value, FastTags<TagGroup.Global> _tags = default(FastTags<TagGroup.Global>))
	{
		if (this.Effects == null)
		{
			return;
		}
		this.Effects.ModifyValue(_ea, _effect, ref _base_value, ref _perc_value, _pv.GetCalculatedLevel(_ea), _tags, 1);
	}

	public void GetModifiedValueData(List<EffectManager.ModifierValuesAndSources> _modValueSources, EffectManager.ModifierValuesAndSources.ValueSourceType _sourceType, EntityAlive _ea, ProgressionValue _pv, PassiveEffects _effect, ref float _base_value, ref float _perc_value, FastTags<TagGroup.Global> _tags = default(FastTags<TagGroup.Global>))
	{
		if (this.Effects == null)
		{
			return;
		}
		this.Effects.GetModifiedValueData(_modValueSources, _sourceType, _ea, _effect, ref _base_value, ref _perc_value, _pv.GetCalculatedLevel(_ea), _tags, 1);
	}

	public bool HasEvents()
	{
		return this.Effects != null && this.Effects.HasEvents();
	}

	public void FireEvent(MinEventTypes _eventType, MinEventParams _params)
	{
		if (this.Effects != null)
		{
			this.Effects.FireEvent(_eventType, _params);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool canRun(RequirementGroup Requirements, MinEventParams _params)
	{
		return Requirements == null || Requirements.IsValid(_params);
	}

	public LevelRequirement GetRequirementsForLevel(int _level)
	{
		if (this.LevelRequirements.Count <= 0)
		{
			return new LevelRequirement(_level);
		}
		LevelRequirement levelRequirement;
		if (this.LevelRequirements.dict.TryGetValue(_level, out levelRequirement))
		{
			return levelRequirement;
		}
		return new LevelRequirement(_level);
	}

	public void AddLevelRequirement(LevelRequirement _lr)
	{
		this.LevelRequirements.Add(_lr.Level, _lr);
	}

	public void PostInit()
	{
		this.LevelRequirements.list.Sort((LevelRequirement a, LevelRequirement b) => a.Level - b.Level);
	}

	public static int GetCalculatedMaxLevel(EntityAlive _ea, ProgressionValue _pv)
	{
		ProgressionClass progressionClass = _pv.ProgressionClass;
		int num = 0;
		if (progressionClass.LevelRequirements.Count > 0)
		{
			List<LevelRequirement> list = progressionClass.LevelRequirements.list;
			int i = 0;
			int num2 = list.Count;
			while (i < num2)
			{
				int num3 = (num2 + i) / 2;
				LevelRequirement levelRequirement = list[num3];
				if (ProgressionClass.canRun(levelRequirement.Requirements, _ea.MinEventContext))
				{
					num = levelRequirement.Level;
					i = num3 + 1;
				}
				else
				{
					num2 = num3;
				}
			}
			if (!progressionClass.IsAttribute)
			{
				if (num < progressionClass.MinLevel)
				{
					num = progressionClass.MinLevel;
				}
				if (num > progressionClass.MaxLevel)
				{
					num = progressionClass.MaxLevel;
				}
			}
		}
		else if (progressionClass.IsAttribute)
		{
			num = 20;
		}
		else
		{
			num = progressionClass.MaxLevel;
		}
		return num;
	}

	public int CalculatedCostForLevel(int _level)
	{
		if (this.OverrideCost == null)
		{
			return (int)(Mathf.Pow(this.CostMultiplier, (float)_level) * (float)this.BaseCostToLevel);
		}
		if (_level - 1 < this.OverrideCost.Length)
		{
			return this.OverrideCost[_level - 1];
		}
		return 0;
	}

	public float GetPercentThisLevel(ProgressionValue _pv)
	{
		if (this.Type != ProgressionType.Skill)
		{
			return 0f;
		}
		if (_pv.Level == this.MaxLevel)
		{
			return 0f;
		}
		float num = (float)((int)(Mathf.Pow(this.CostMultiplier, (float)_pv.Level) * (float)this.BaseCostToLevel) - _pv.CostForNextLevel) / (Mathf.Pow(this.CostMultiplier, (float)_pv.Level) * (float)this.BaseCostToLevel);
		if (!float.IsNaN(num))
		{
			return num;
		}
		return 0f;
	}

	public void HandleCheckCrafting(EntityPlayerLocal _player, int _oldLevel, int _newLevel)
	{
		if (this.DisplayDataList != null)
		{
			for (int i = 0; i < this.DisplayDataList.Count; i++)
			{
				ProgressionClass.DisplayData displayData = this.DisplayDataList[i];
				if (displayData.Enabled)
				{
					displayData.HandleCheckCrafting(_player, _oldLevel, _newLevel);
				}
			}
		}
	}

	public void UpdateForSandbox()
	{
		if (this.DisplayDataList != null)
		{
			ProgressionClass.DisplayData displayData = null;
			this.Enabled = false;
			for (int i = this.DisplayDataList.Count - 1; i >= 0; i--)
			{
				ProgressionClass.DisplayData displayData2 = this.DisplayDataList[i];
				if (displayData2.HandleCheckEnabled())
				{
					this.Enabled = true;
					if (displayData != null)
					{
						this.MaxLevel = displayData2.QualityStarts[displayData2.QualityStarts.Length - 1];
						displayData = null;
					}
				}
				else
				{
					displayData = displayData2;
				}
			}
		}
	}

	public override string ToString()
	{
		return string.Format("{0}, {1}, lvl {2} to {3}", new object[]
		{
			base.ToString(),
			this.Name,
			this.MinLevel,
			this.MaxLevel
		});
	}

	public readonly string Name;

	public readonly FastTags<TagGroup.Global> NameTag;

	public float ParentMaxLevelRatio = 1f;

	public string NameKey;

	public string DescKey;

	public string LongDescKey;

	public string Icon;

	public int MinLevel;

	public int MaxLevel;

	public int BaseCostToLevel;

	public float CostMultiplier;

	public bool Hidden;

	public int[] OverrideCost;

	public bool Enabled = true;

	public ProgressionClass.DisplayTypes DisplayType;

	public MinEffectController Effects;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly DictionaryList<int, LevelRequirement> LevelRequirements = new DictionaryList<int, LevelRequirement>();

	public readonly List<ProgressionClass> Children = new List<ProgressionClass>();

	public string ParentName;

	public ProgressionType Type;

	[PublicizedFrom(EAccessModifier.Private)]
	public float listSortOrder;

	public List<ProgressionClass.DisplayData> DisplayDataList;

	public enum DisplayTypes
	{
		Standard,
		Book,
		Crafting
	}

	public class ListSortOrderComparer : IComparer<ProgressionValue>
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public ListSortOrderComparer()
		{
		}

		public int Compare(ProgressionValue _x, ProgressionValue _y)
		{
			return _x.ProgressionClass.ListSortOrder.CompareTo(_y.ProgressionClass.ListSortOrder);
		}

		public static ProgressionClass.ListSortOrderComparer Instance = new ProgressionClass.ListSortOrderComparer();
	}

	public class DisplayData
	{
		public ItemClass Item
		{
			get
			{
				if (this.item == null)
				{
					this.item = ItemClass.GetItemClass(this.ItemName, false);
				}
				return this.item;
			}
		}

		public bool HasQuality
		{
			get
			{
				if (this.ItemName != "")
				{
					return this.Item.HasQuality;
				}
				return this.CustomHasQuality;
			}
		}

		public string GetName(int level)
		{
			if (this.ItemName != "")
			{
				return this.Item.GetLocalizedItemName();
			}
			if (this.CustomName == null)
			{
				return "";
			}
			int num = this.GetQualityLevel(level);
			if (num >= this.CustomName.Length)
			{
				num = 0;
			}
			return this.CustomName[num];
		}

		public string GetIcon(int level)
		{
			if (this.ItemName != "")
			{
				return this.Item.GetIconName();
			}
			if (this.CustomIcon == null)
			{
				return "";
			}
			int num = this.GetQualityLevel(level);
			if (num >= this.CustomIcon.Length)
			{
				num = 0;
			}
			return this.CustomIcon[num];
		}

		public string GetIconTint(int level)
		{
			if (this.ItemName != "")
			{
				return Utils.ColorToHex(this.Item.GetIconTint(null));
			}
			if (this.CustomIconTint == null)
			{
				return "FFFFFF";
			}
			int num = this.GetQualityLevel(level);
			if (num >= this.CustomName.Length)
			{
				num = 0;
			}
			return this.CustomIconTint[num];
		}

		public int GetQualityLevel(int level)
		{
			for (int i = 0; i < this.QualityStarts.Length; i++)
			{
				if (this.QualityStarts[i] > level)
				{
					return i;
				}
			}
			return this.QualityStarts.Length;
		}

		public int GetNextPoints(int level)
		{
			for (int i = 0; i < this.QualityStarts.Length; i++)
			{
				if (this.QualityStarts[i] > level)
				{
					return this.QualityStarts[i];
				}
			}
			return 0;
		}

		public bool IsComplete(int level)
		{
			for (int i = 0; i < this.QualityStarts.Length; i++)
			{
				if (this.QualityStarts[i] > level)
				{
					return false;
				}
			}
			return true;
		}

		public void AddUnlockData(string itemName, int unlockTier, string[] recipeList)
		{
			if (this.UnlockDataList == null)
			{
				this.UnlockDataList = new List<ProgressionClass.DisplayData.UnlockData>();
			}
			this.UnlockDataList.Add(new ProgressionClass.DisplayData.UnlockData
			{
				ItemName = itemName,
				UnlockTier = unlockTier,
				RecipeList = recipeList
			});
		}

		public ItemClass GetUnlockItem(int index)
		{
			if (this.UnlockDataList == null)
			{
				return null;
			}
			if (index >= this.UnlockDataList.Count)
			{
				return null;
			}
			ProgressionClass.DisplayData.UnlockData unlockData = this.UnlockDataList[index];
			if (!unlockData.Enabled)
			{
				return null;
			}
			return unlockData.Item;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public ProgressionClass.DisplayData.UnlockData GetUnlockData(int index)
		{
			if (this.UnlockDataList == null)
			{
				return null;
			}
			if (index >= this.UnlockDataList.Count)
			{
				return null;
			}
			ProgressionClass.DisplayData.UnlockData unlockData = this.UnlockDataList[index];
			if (!unlockData.Enabled)
			{
				return null;
			}
			return unlockData;
		}

		public string GetUnlockItemIconName(int index)
		{
			ItemClass unlockItem = this.GetUnlockItem(index);
			if (unlockItem != null)
			{
				return unlockItem.GetIconName();
			}
			return "";
		}

		public string GetUnlockItemName(int index)
		{
			ItemClass unlockItem = this.GetUnlockItem(index);
			if (unlockItem != null)
			{
				return unlockItem.GetLocalizedItemName();
			}
			return "";
		}

		public List<int> GetUnlockItemRecipes(int index)
		{
			if (this.UnlockDataList == null)
			{
				return null;
			}
			if (index >= this.UnlockDataList.Count)
			{
				return null;
			}
			List<int> list = new List<int>();
			if (this.Item != null)
			{
				list.Add(this.Item.Id);
			}
			else
			{
				ProgressionClass.DisplayData.UnlockData unlockData = this.UnlockDataList[index];
				if (unlockData != null)
				{
					if (unlockData.RecipeList != null)
					{
						for (int i = 0; i < unlockData.RecipeList.Length; i++)
						{
							list.Add(ItemClass.GetItemClass(unlockData.RecipeList[i], true).Id);
						}
					}
					else if (unlockData.Item != null)
					{
						list.Add(unlockData.Item.Id);
					}
				}
			}
			return list;
		}

		public string GetUnlockItemIconAtlas(EntityPlayerLocal player, int index)
		{
			ProgressionClass.DisplayData.UnlockData unlockData = this.GetUnlockData(index);
			if (unlockData == null)
			{
				return "ItemIconAtlas";
			}
			if (this.GetQualityLevel(player.Progression.GetProgressionValue(this.Owner.Name).Level) <= unlockData.UnlockTier)
			{
				return "ItemIconAtlasGreyscale";
			}
			return "ItemIconAtlas";
		}

		public bool GetUnlockItemLocked(EntityPlayerLocal player, int index)
		{
			ProgressionClass.DisplayData.UnlockData unlockData = this.GetUnlockData(index);
			return unlockData != null && this.GetQualityLevel(player.Progression.GetProgressionValue(this.Owner.Name).Level) <= unlockData.UnlockTier;
		}

		public void HandleCheckCrafting(EntityPlayerLocal _player, int _oldLevel, int _newLevel)
		{
			if (this.UnlockDataList == null)
			{
				return;
			}
			for (int i = 0; i < this.QualityStarts.Length; i++)
			{
				int num = this.QualityStarts[i];
				if (_oldLevel < num && _newLevel >= num)
				{
					if (this.HasQuality)
					{
						GameManager.ShowTooltip(_player, Localization.Get("ttCraftingSkillUnlockQuality", false, null), new string[]
						{
							Localization.Get(this.Owner.NameKey, false, null),
							this.GetName(_newLevel),
							(i + 1).ToString()
						}, ProgressionClass.DisplayData.CompletionSound, null, false, false, 0f);
					}
					else
					{
						GameManager.ShowTooltip(_player, Localization.Get("ttCraftingSkillUnlock", false, null), new string[]
						{
							Localization.Get(this.Owner.NameKey, false, null),
							this.GetName(_oldLevel)
						}, ProgressionClass.DisplayData.CompletionSound, null, false, false, 0f);
					}
				}
			}
		}

		public bool HandleCheckEnabled()
		{
			if (this.UnlockDataList == null)
			{
				this.Enabled = true;
				return true;
			}
			bool flag = false;
			for (int i = 0; i < this.UnlockDataList.Count; i++)
			{
				ProgressionClass.DisplayData.UnlockData unlockData = this.UnlockDataList[i];
				unlockData.Enabled = unlockData.Item == null || unlockData.Item.ItemTechType <= ItemClass.MaxTechType;
				if (unlockData.Enabled)
				{
					flag = true;
				}
			}
			this.Enabled = flag;
			return this.Enabled;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public ItemClass item;

		public string ItemName = "";

		public string[] CustomIcon;

		public string[] CustomIconTint;

		public string[] CustomName;

		public bool CustomHasQuality;

		public int[] QualityStarts;

		public bool Enabled = true;

		public List<ProgressionClass.DisplayData.UnlockData> UnlockDataList = new List<ProgressionClass.DisplayData.UnlockData>();

		public static string CompletionSound = "";

		public ProgressionClass Owner;

		public class UnlockData
		{
			public ItemClass Item
			{
				get
				{
					if (this.item == null)
					{
						this.item = ItemClass.GetItemClass(this.ItemName, false);
					}
					return this.item;
				}
			}

			[PublicizedFrom(EAccessModifier.Private)]
			public ItemClass item;

			public string[] RecipeList;

			public string ItemName = "";

			public bool Enabled = true;

			public int UnlockTier;
		}
	}
}
