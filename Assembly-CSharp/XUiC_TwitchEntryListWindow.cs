using System;
using System.Collections.Generic;
using Twitch;
using UniLinq;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_TwitchEntryListWindow : XUiController
{
	public string ActionCategory
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public string VoteCategory
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public string ActionHistoryCategory
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public string LeaderboardCategory
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public override void Init()
	{
		base.Init();
		this.twitchActionEntryList = base.GetChildByType<XUiC_TwitchActionEntryList>();
		this.twitchActionEntryList.TwitchEntryListWindow = this;
		this.twitchVoteInfoEntryList = base.GetChildByType<XUiC_TwitchVoteInfoEntryList>();
		this.twitchVoteInfoEntryList.TwitchEntryListWindow = this;
		this.twitchActionHistoryEntryList = base.GetChildByType<XUiC_TwitchActionHistoryEntryList>();
		this.twitchActionHistoryEntryList.TwitchEntryListWindow = this;
		this.twitchLeaderboardEntryList = base.GetChildByType<XUiC_TwitchLeaderboardEntryList>();
		this.twitchLeaderboardEntryList.TwitchEntryListWindow = this;
		this.categoryList = (XUiC_CategoryList)this.windowGroup.Controller.GetChildById("actioncategories");
		this.categoryList.CategoryChanged += this.HandleCategoryChanged;
		this.voteCategoryList = (XUiC_CategoryList)this.windowGroup.Controller.GetChildById("votecategories");
		this.voteCategoryList.CategoryChanged += this.HandleVoteCategoryChanged;
		this.actionHistoryCategoryList = (XUiC_CategoryList)this.windowGroup.Controller.GetChildById("actionHistoryCategories");
		this.actionHistoryCategoryList.CategoryChanged += this.HandleActionHistoryCategoryChanged;
		this.leaderboardCategoryList = (XUiC_CategoryList)this.windowGroup.Controller.GetChildById("leaderboardCategories");
		this.leaderboardCategoryList.CategoryChanged += this.HandleLeaderboardCategoryChanged;
		this.txtInput = (XUiC_TextInput)this.windowGroup.Controller.GetChildById("searchInput");
		XUiController childById = base.GetChildById("allactions");
		if (childById != null)
		{
			this.allActionsButton = childById.ViewComponent as XUiV_Button;
			if (this.allActionsButton != null)
			{
				childById.OnPress += this.AllActionsButtonCtrl_OnPress;
			}
		}
		if (this.txtInput != null)
		{
			this.txtInput.OnChangeHandler += this.HandleOnChangedHandler;
			this.txtInput.Text = "";
		}
		this.ActionCategory = "";
		this.lblActions = Localization.Get("TwitchInfo_Actions", false, null);
		this.lblVotes = Localization.Get("TwitchInfo_Votes", false, null);
		this.lblActionHistory = Localization.Get("TwitchInfo_ActionHistory", false, null);
		this.lblLeaderboard = Localization.Get("TwitchInfo_Leaderboard", false, null);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void AllActionsButtonCtrl_OnPress(XUiController _sender, int _mouseButton)
	{
		this.showAllActions = !this.showAllActions;
		this.allActionsButton.Selected = this.showAllActions;
		this.IsDirty = true;
	}

	public void SetOpenToActions(bool openExtras = false)
	{
		this.CurrentType = XUiC_TwitchEntryListWindow.ListTypes.Actions;
		this.IsDirty = true;
		this.forceExtras = openExtras;
	}

	public void SetOpenToLeaderboard()
	{
		this.CurrentType = XUiC_TwitchEntryListWindow.ListTypes.Leaderboard;
		this.IsDirty = true;
	}

	public void SetOpenToVotes()
	{
		this.CurrentType = XUiC_TwitchEntryListWindow.ListTypes.Votes;
		this.IsDirty = true;
	}

	public void SetOpenToHistory()
	{
		this.CurrentType = XUiC_TwitchEntryListWindow.ListTypes.ActionHistory;
		this.IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleCategoryChanged(XUiC_CategoryEntry _categoryEntry)
	{
		if (this.txtInput != null)
		{
			this.txtInput.Text = "";
		}
		this.ActionCategory = _categoryEntry.CategoryName;
		this.HandleFilterActions();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleFilterActions()
	{
		if (this.currentActions != null && this.CurrentType == XUiC_TwitchEntryListWindow.ListTypes.Actions)
		{
			TwitchActionPreset currentPreset = this.tm.CurrentActionPreset;
			List<TwitchAction> list = (from entry in this.currentActions
				where (this.filterText == "" || entry.Title.ContainsCaseInsensitive(this.filterText) || entry.Command.ContainsCaseInsensitive(this.filterText)) && ((this.ActionCategory == "" && entry.DisplayCategory == entry.MainCategory) || entry.DisplayCategory.Name == this.ActionCategory) && entry.ShowInActionList && (currentPreset.AllowPointGeneration || entry.PointType == TwitchAction.PointTypes.Bits)
				orderby entry.Title
				select entry).ToList<TwitchAction>();
			this.twitchActionEntryList.SetTwitchActionList(list, this.tm.CurrentActionPreset);
			base.RefreshBindings();
			return;
		}
		this.IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleVoteCategoryChanged(XUiC_CategoryEntry _categoryEntry)
	{
		if (this.txtInput != null)
		{
			this.txtInput.Text = "";
		}
		this.VoteCategory = _categoryEntry.CategoryName;
		this.IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleActionHistoryCategoryChanged(XUiC_CategoryEntry _categoryEntry)
	{
		if (this.txtInput != null)
		{
			this.txtInput.Text = "";
		}
		if (_categoryEntry.CategoryName != this.ActionHistoryCategory)
		{
			this.ActionHistoryCategory = _categoryEntry.CategoryName;
			this.twitchActionHistoryEntryList.SelectedEntry = null;
			this.twitchActionHistoryEntryList.setFirstEntry = true;
		}
		this.IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleLeaderboardCategoryChanged(XUiC_CategoryEntry _categoryEntry)
	{
		if (this.txtInput != null)
		{
			this.txtInput.Text = "";
		}
		this.LeaderboardCategory = _categoryEntry.CategoryName;
		this.IsDirty = true;
		this.UpdateDelay = 0f;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleOnChangedHandler(XUiController _sender, string _text, bool _changeFromCode)
	{
		this.filterText = _text;
		if (this.CurrentType == XUiC_TwitchEntryListWindow.ListTypes.Actions)
		{
			this.HandleFilterActions();
			return;
		}
		this.IsDirty = true;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (this.UpdateDelay > 0f)
		{
			this.UpdateDelay -= _dt;
			return;
		}
		if (this.IsDirty)
		{
			if (this.tm == null)
			{
				this.tm = TwitchManager.Current;
			}
			switch (this.CurrentType)
			{
			case XUiC_TwitchEntryListWindow.ListTypes.Actions:
			{
				this.currentActions = (from entry in TwitchActionManager.TwitchActions.Values.ToList<TwitchAction>()
					where entry != null && entry.ShowInActionList && (entry.IsInPresetForList(this.tm.CurrentActionPreset) || (this.showAllActions && !entry.OnlyShowInPreset) || (entry.DisplayCategory != null && entry.DisplayCategory.AlwaysShowInMenu))
					orderby entry.Title
					select entry).ToList<TwitchAction>();
				this.twitchActionEntryList.SetTwitchActionList(this.currentActions, this.tm.CurrentActionPreset);
				string text = ((this.categoryList.CurrentCategory != null) ? this.categoryList.CurrentCategory.CategoryName : "");
				this.categoryList.SetupCategoriesBasedOnTwitchActions(this.currentActions);
				if (this.forceExtras)
				{
					text = "Extras";
					this.forceExtras = false;
				}
				if (text == "")
				{
					this.categoryList.SetCategoryToFirst();
				}
				else
				{
					this.categoryList.SetCategory(text);
					if (this.categoryList.CurrentCategory == null || this.categoryList.CurrentCategory.CategoryName == "")
					{
						this.categoryList.SetCategoryToFirst();
					}
				}
				break;
			}
			case XUiC_TwitchEntryListWindow.ListTypes.Votes:
				this.currentVotes = (from entry in TwitchActionManager.TwitchVotes.Values.ToList<TwitchVote>()
					where (this.filterText == "" || entry.Display.ContainsCaseInsensitive(this.filterText)) && (this.VoteCategory == "" || (entry.MainVoteType != null && entry.MainVoteType.Name == this.VoteCategory && entry.MainVoteType.IsInPreset(this.tm.CurrentVotePreset.Name))) && entry.IsInPreset(this.tm.CurrentVotePreset)
					orderby entry.VoteDescription
					select entry).ToList<TwitchVote>();
				this.twitchVoteInfoEntryList.SetTwitchVoteList(this.currentVotes);
				break;
			case XUiC_TwitchEntryListWindow.ListTypes.ActionHistory:
			{
				List<TwitchActionHistoryEntry> list = null;
				string actionHistoryCategory = this.ActionHistoryCategory;
				if (!(actionHistoryCategory == "action"))
				{
					if (!(actionHistoryCategory == "vote"))
					{
						if (actionHistoryCategory == "event")
						{
							list = this.tm.EventHistory;
						}
					}
					else
					{
						list = this.tm.VoteHistory;
					}
				}
				else
				{
					list = this.tm.ActionHistory;
				}
				if (list != null)
				{
					this.currentRedemptions = list.Where((TwitchActionHistoryEntry entry) => entry != null && entry.IsValid() && (this.filterText == "" || entry.UserName.ContainsCaseInsensitive(this.filterText) || entry.Action.Command.ContainsCaseInsensitive(this.filterText)) && (this.ActionHistoryCategory == "" || this.ActionHistoryCategory == entry.HistoryType)).ToList<TwitchActionHistoryEntry>();
					this.twitchActionHistoryEntryList.SetTwitchActionHistoryList(this.currentRedemptions);
				}
				break;
			}
			case XUiC_TwitchEntryListWindow.ListTypes.Leaderboard:
			{
				string text2 = ((this.leaderboardCategoryList == null) ? "" : this.leaderboardCategoryList.CurrentCategory.CategoryName);
				if (!(text2 == "global_kills"))
				{
					if (!(text2 == "session_kills"))
					{
						if (!(text2 == "session_good"))
						{
							if (!(text2 == "session_bad"))
							{
								if (!(text2 == "session_bits"))
								{
									if (text2 == "current_good")
									{
										Dictionary<string, TwitchLeaderboardStats.StatEntry> statEntries = TwitchManager.LeaderboardStats.StatEntries;
										this.currentLeaderboard = (from viewer in statEntries.Values
											where (this.filterText == "" || viewer.Name.ContainsCaseInsensitive(this.filterText)) && viewer.CurrentActions > 0
											orderby viewer.CurrentGoodActions descending
											select new TwitchLeaderboardEntry(viewer.Name, viewer.UserColor, viewer.CurrentGoodActions)).ToList<TwitchLeaderboardEntry>();
										this.twitchLeaderboardEntryList.SetTwitchLeaderboardList(this.currentLeaderboard);
									}
								}
								else
								{
									Dictionary<string, TwitchLeaderboardStats.StatEntry> statEntries2 = TwitchManager.LeaderboardStats.StatEntries;
									this.currentLeaderboard = (from viewer in statEntries2.Values
										where (this.filterText == "" || viewer.Name.ContainsCaseInsensitive(this.filterText)) && viewer.BitsUsed > 0
										orderby viewer.BitsUsed descending
										select new TwitchLeaderboardEntry(viewer.Name, viewer.UserColor, viewer.BitsUsed)).ToList<TwitchLeaderboardEntry>();
									this.twitchLeaderboardEntryList.SetTwitchLeaderboardList(this.currentLeaderboard);
								}
							}
							else
							{
								Dictionary<string, TwitchLeaderboardStats.StatEntry> statEntries3 = TwitchManager.LeaderboardStats.StatEntries;
								this.currentLeaderboard = (from viewer in statEntries3.Values
									where (this.filterText == "" || viewer.Name.ContainsCaseInsensitive(this.filterText)) && viewer.BadActions > 0
									orderby viewer.BadActions descending
									select new TwitchLeaderboardEntry(viewer.Name, viewer.UserColor, viewer.BadActions)).ToList<TwitchLeaderboardEntry>();
								this.twitchLeaderboardEntryList.SetTwitchLeaderboardList(this.currentLeaderboard);
							}
						}
						else
						{
							Dictionary<string, TwitchLeaderboardStats.StatEntry> statEntries4 = TwitchManager.LeaderboardStats.StatEntries;
							this.currentLeaderboard = (from viewer in statEntries4.Values
								where (this.filterText == "" || viewer.Name.ContainsCaseInsensitive(this.filterText)) && viewer.GoodActions > 0
								orderby viewer.GoodActions descending
								select new TwitchLeaderboardEntry(viewer.Name, viewer.UserColor, viewer.GoodActions)).ToList<TwitchLeaderboardEntry>();
							this.twitchLeaderboardEntryList.SetTwitchLeaderboardList(this.currentLeaderboard);
						}
					}
					else
					{
						Dictionary<string, TwitchLeaderboardStats.StatEntry> statEntries5 = TwitchManager.LeaderboardStats.StatEntries;
						this.currentLeaderboard = (from viewer in statEntries5.Values
							where (this.filterText == "" || viewer.Name.ContainsCaseInsensitive(this.filterText)) && viewer.Kills > 0
							orderby viewer.Kills descending
							select new TwitchLeaderboardEntry(viewer.Name, viewer.UserColor, viewer.Kills)).ToList<TwitchLeaderboardEntry>();
						this.twitchLeaderboardEntryList.SetTwitchLeaderboardList(this.currentLeaderboard);
					}
				}
				else
				{
					this.currentLeaderboard = (from entry in this.tm.Leaderboard
						where this.filterText == "" || entry.UserName.ContainsCaseInsensitive(this.filterText)
						orderby entry.Kills descending
						select entry).ToList<TwitchLeaderboardEntry>();
					this.twitchLeaderboardEntryList.SetTwitchLeaderboardList(this.currentLeaderboard);
				}
				this.UpdateDelay = 1f;
				break;
			}
			}
			base.RefreshBindings();
			this.IsDirty = false;
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		if (!this.isInitialized)
		{
			this.player = this.xui.playerUI.entityPlayer;
			this.categoryList.SetupCategoriesBasedOnTwitchCategories(TwitchActionManager.Current.CategoryList);
			this.categoryList.SetCategoryToFirst();
			this.voteCategoryList.SetupCategoriesBasedOnTwitchVoteCategories(TwitchManager.Current.VotingManager.VoteTypes.Values.Where((TwitchVoteType v) => v.IsInPreset(TwitchManager.Current.CurrentVotePreset.Name)).ToList<TwitchVoteType>());
			this.voteCategoryList.SetCategoryToFirst();
			this.actionHistoryCategoryList.SetCategoryEntry(0, "action", "ui_game_symbol_twitch_actions", Localization.Get("TwitchInfo_Actions", false, null));
			this.actionHistoryCategoryList.SetCategoryEntry(1, "vote", "ui_game_symbol_twitch_vote", Localization.Get("TwitchInfo_Votes", false, null));
			this.actionHistoryCategoryList.SetCategoryEntry(2, "event", "ui_game_symbol_twitch_custom_actions", Localization.Get("xuiOptionsTwitchEvents", false, null));
			this.actionHistoryCategoryList.SetCategoryToFirst();
			this.leaderboardCategoryList.SetCategoryEntry(0, "global_kills", "ui_game_symbol_twitch_top_killer", Localization.Get("TwitchInfo_LeaderboardGlobalKills", false, null));
			this.leaderboardCategoryList.SetCategoryEntry(1, "session_kills", "ui_game_symbol_skull", Localization.Get("TwitchInfo_LeaderboardSessionKills", false, null));
			this.leaderboardCategoryList.SetCategoryEntry(2, "session_good", "ui_game_symbol_twitch_top_good", Localization.Get("TwitchInfo_LeaderboardSessionGood", false, null));
			this.leaderboardCategoryList.SetCategoryEntry(3, "session_bad", "ui_game_symbol_twitch_top_bad", Localization.Get("TwitchInfo_LeaderboardSessionBad", false, null));
			this.leaderboardCategoryList.SetCategoryEntry(4, "session_bits", "ui_game_symbol_twitch_bits", Localization.Get("TwitchInfo_LeaderboardSessionBits", false, null));
			this.leaderboardCategoryList.SetCategoryEntry(5, "current_good", "ui_game_symbol_twitch_best_helper", Localization.Get("TwitchInfo_LeaderboardCurrentGood", false, null));
			this.leaderboardCategoryList.SetCategoryToFirst();
			this.IsDirty = true;
			this.isInitialized = true;
		}
		this.tm = TwitchManager.Current;
		this.tm.ActionHistoryAdded += this.Tm_ActionHistoryAdded;
		this.tm.VoteHistoryAdded += this.Tm_VoteHistoryAdded;
		this.tm.EventHistoryAdded += this.Tm_EventHistoryAdded;
		TwitchManager.LeaderboardStats.LeaderboardChanged += this.LeaderboardStats_LeaderboardChanged;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void LeaderboardStats_LeaderboardChanged()
	{
		if (this.CurrentType == XUiC_TwitchEntryListWindow.ListTypes.Leaderboard)
		{
			this.IsDirty = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Tm_ActionHistoryAdded()
	{
		if (this.CurrentType == XUiC_TwitchEntryListWindow.ListTypes.ActionHistory && this.ActionHistoryCategory == "action")
		{
			this.IsDirty = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Tm_VoteHistoryAdded()
	{
		if (this.CurrentType == XUiC_TwitchEntryListWindow.ListTypes.ActionHistory && this.ActionHistoryCategory == "vote")
		{
			this.IsDirty = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Tm_EventHistoryAdded()
	{
		if (this.CurrentType == XUiC_TwitchEntryListWindow.ListTypes.ActionHistory && this.ActionHistoryCategory == "event")
		{
			this.IsDirty = true;
		}
	}

	public override void OnClose()
	{
		base.OnClose();
		TwitchManager twitchManager = TwitchManager.Current;
		twitchManager.ActionHistoryAdded -= this.Tm_ActionHistoryAdded;
		twitchManager.VoteHistoryAdded -= this.Tm_VoteHistoryAdded;
		twitchManager.EventHistoryAdded -= this.Tm_EventHistoryAdded;
		TwitchManager.LeaderboardStats.LeaderboardChanged -= this.LeaderboardStats_LeaderboardChanged;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string value, string bindingName)
	{
		uint num = <PrivateImplementationDetails>.ComputeStringHash(bindingName);
		if (num <= 2855073766U)
		{
			if (num <= 956754208U)
			{
				if (num != 884373603U)
				{
					if (num == 956754208U)
					{
						if (bindingName == "showcategories")
						{
							value = (this.CurrentType == XUiC_TwitchEntryListWindow.ListTypes.Actions).ToString();
							return true;
						}
					}
				}
				else if (bindingName == "showleaderboard")
				{
					value = (this.CurrentType == XUiC_TwitchEntryListWindow.ListTypes.Leaderboard).ToString();
					return true;
				}
			}
			else if (num != 1092444455U)
			{
				if (num == 2855073766U)
				{
					if (bindingName == "leaderboard_header_value")
					{
						if (this.CurrentType == XUiC_TwitchEntryListWindow.ListTypes.Leaderboard)
						{
							string text;
							if (this.leaderboardCategoryList != null)
							{
								XUiC_CategoryEntry currentCategory = this.leaderboardCategoryList.CurrentCategory;
								text = ((currentCategory != null) ? currentCategory.CategoryName : null) ?? "";
							}
							else
							{
								text = "";
							}
							string text2 = text;
							if (!(text2 == "global_kills") && !(text2 == "session_kills"))
							{
								if (!(text2 == "session_good") && !(text2 == "session_bad") && !(text2 == "current_good"))
								{
									if (text2 == "session_bits")
									{
										value = Localization.Get("TwitchPoints_Bits", false, null);
									}
								}
								else
								{
									value = Localization.Get("TwitchInfo_Actions", false, null);
								}
							}
							else
							{
								value = Localization.Get("TwitchInfo_KillsHeader", false, null);
							}
						}
						return true;
					}
				}
			}
			else if (bindingName == "showactions")
			{
				value = (this.CurrentType == XUiC_TwitchEntryListWindow.ListTypes.Actions).ToString();
				return true;
			}
		}
		else if (num <= 3272249238U)
		{
			if (num != 3131143201U)
			{
				if (num == 3272249238U)
				{
					if (bindingName == "headertitle")
					{
						switch (this.CurrentType)
						{
						case XUiC_TwitchEntryListWindow.ListTypes.Actions:
						{
							string text3;
							if (this.categoryList != null)
							{
								XUiC_CategoryEntry currentCategory2 = this.categoryList.CurrentCategory;
								text3 = ((currentCategory2 != null) ? currentCategory2.CategoryDisplayName : null) ?? "";
							}
							else
							{
								text3 = "";
							}
							string text4 = text3;
							value = ((text4 != "") ? text4 : Localization.Get("lblAll", false, null));
							break;
						}
						case XUiC_TwitchEntryListWindow.ListTypes.Votes:
						{
							string text5;
							if (this.voteCategoryList != null)
							{
								XUiC_CategoryEntry currentCategory3 = this.voteCategoryList.CurrentCategory;
								text5 = ((currentCategory3 != null) ? currentCategory3.CategoryDisplayName : null) ?? "";
							}
							else
							{
								text5 = "";
							}
							string text6 = text5;
							value = ((text6 != "") ? text6 : Localization.Get("lblAll", false, null));
							break;
						}
						case XUiC_TwitchEntryListWindow.ListTypes.ActionHistory:
						{
							string text7;
							if (this.actionHistoryCategoryList != null)
							{
								XUiC_CategoryEntry currentCategory4 = this.actionHistoryCategoryList.CurrentCategory;
								text7 = ((currentCategory4 != null) ? currentCategory4.CategoryDisplayName : null) ?? "";
							}
							else
							{
								text7 = "";
							}
							string text8 = text7;
							value = ((text8 != "") ? text8 : Localization.Get("lblAll", false, null));
							break;
						}
						case XUiC_TwitchEntryListWindow.ListTypes.Leaderboard:
						{
							string text9;
							if (this.leaderboardCategoryList != null)
							{
								XUiC_CategoryEntry currentCategory5 = this.leaderboardCategoryList.CurrentCategory;
								text9 = ((currentCategory5 != null) ? currentCategory5.CategoryDisplayName : null) ?? "";
							}
							else
							{
								text9 = "";
							}
							string text10 = text9;
							value = ((text10 != "") ? text10 : Localization.Get("lblAll", false, null));
							break;
						}
						}
						return true;
					}
				}
			}
			else if (bindingName == "headericon")
			{
				switch (this.CurrentType)
				{
				case XUiC_TwitchEntryListWindow.ListTypes.Actions:
					value = "ui_game_symbol_twitch_actions";
					break;
				case XUiC_TwitchEntryListWindow.ListTypes.Votes:
					value = "ui_game_symbol_twitch_vote";
					break;
				case XUiC_TwitchEntryListWindow.ListTypes.ActionHistory:
					value = "ui_game_symbol_twitch_history";
					break;
				case XUiC_TwitchEntryListWindow.ListTypes.Leaderboard:
					value = "ui_game_symbol_twitch_leaderboard_1";
					break;
				}
				return true;
			}
		}
		else if (num != 4008283398U)
		{
			if (num == 4228112153U)
			{
				if (bindingName == "showvotes")
				{
					value = (this.CurrentType == XUiC_TwitchEntryListWindow.ListTypes.Votes).ToString();
					return true;
				}
			}
		}
		else if (bindingName == "showactionhistory")
		{
			value = (this.CurrentType == XUiC_TwitchEntryListWindow.ListTypes.ActionHistory).ToString();
			return true;
		}
		return base.GetBindingValueInternal(ref value, bindingName);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityPlayerLocal player;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TwitchActionEntryList twitchActionEntryList;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TwitchVoteInfoEntryList twitchVoteInfoEntryList;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TwitchActionHistoryEntryList twitchActionHistoryEntryList;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TwitchLeaderboardEntryList twitchLeaderboardEntryList;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput txtInput;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<TwitchAction> currentActions;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<TwitchVote> currentVotes;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<TwitchActionHistoryEntry> currentRedemptions;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<TwitchLeaderboardEntry> currentLeaderboard;

	[PublicizedFrom(EAccessModifier.Private)]
	public string filterText = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_CategoryList categoryList;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_CategoryList voteCategoryList;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_CategoryList actionHistoryCategoryList;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_CategoryList leaderboardCategoryList;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isInitialized;

	public XUiC_TwitchEntryListWindow.ListTypes CurrentType;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblActions;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblVotes;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblActionHistory;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblLeaderboard;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool showAllActions;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Button allActionsButton;

	[PublicizedFrom(EAccessModifier.Private)]
	public float UpdateDelay;

	[PublicizedFrom(EAccessModifier.Private)]
	public TwitchManager tm;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool forceExtras;

	public enum ListTypes
	{
		Actions,
		Votes,
		ActionHistory,
		Leaderboard
	}
}
