using System;
using Audio;
using Challenges;
using Platform;
using Twitch;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_TwitchEntryDescriptionWindow : XUiController
{
	public TwitchAction CurrentTwitchActionEntry
	{
		get
		{
			return this.currentTwitchActionEntry;
		}
		set
		{
			this.currentTwitchActionEntry = value;
			base.RefreshBindings();
		}
	}

	public TwitchVote CurrentTwitchVoteEntry
	{
		get
		{
			return this.currentTwitchVoteEntry;
		}
		set
		{
			this.currentTwitchVoteEntry = value;
			base.RefreshBindings();
		}
	}

	public TwitchActionHistoryEntry CurrentTwitchActionHistoryEntry
	{
		get
		{
			return this.currentTwitchActionHistoryEntry;
		}
		set
		{
			this.currentTwitchActionHistoryEntry = value;
			base.RefreshBindings();
		}
	}

	public override void Init()
	{
		base.Init();
		XUiController childById = ((XUiC_TwitchInfoWindowGroup)this.windowGroup.Controller).GetChildByType<XUiC_TwitchHowToWindow>().GetChildById("leftButton");
		XUiController childById2 = this.windowGroup.Controller.GetChildById("windowTwitchInfoDescription");
		XUiController xuiController = childById2.GetChildById("btnEnable");
		XUiController childById3 = childById2.GetChildById("statClick");
		this.btnEnable = (XUiV_Button)xuiController.GetChildById("clickable").ViewComponent;
		this.btnEnable.Controller.OnPress += this.btnEnable_OnPress;
		this.btnEnable.NavDownTarget = childById.ViewComponent;
		xuiController = childById2.GetChildById("btnRefund");
		this.btnRefund = (XUiV_Button)xuiController.GetChildById("clickable").ViewComponent;
		this.btnRefund.Controller.OnPress += this.btnRefund_OnPress;
		this.btnRetry = childById2.GetChildById("btnRetry").GetChildByType<XUiC_SimpleButton>();
		this.btnRetry.OnPressed += this.btnRetry_OnPress;
		this.btnRetry.ViewComponent.NavDownTarget = childById.ViewComponent;
		childById2.GetChildById("btnIncrease").GetChildByType<XUiC_SimpleButton>().OnPressed += this.BtnIncrease_OnPressed;
		childById2.GetChildById("btnDecrease").GetChildByType<XUiC_SimpleButton>().OnPressed += this.BtnDecrease_OnPressed;
		childById3.OnPress += this.RectStat_OnPress;
		this.lblStartGamestage = Localization.Get("TwitchInfo_ActionStartGamestage", false, null);
		this.lblEndGamestage = Localization.Get("TwitchInfo_ActionEndGamestage", false, null);
		this.lblPointCost = Localization.Get("TwitchInfo_ActionPointCost", false, null);
		this.lblDiscountCost = Localization.Get("TwitchInfo_ActionDiscountCost", false, null);
		this.lblCooldown = Localization.Get("TwitchInfo_ActionCooldown", false, null);
		this.lblRandomDaily = Localization.Get("TwitchInfo_ActionRandomDaily", false, null);
		this.lblIsPositive = Localization.Get("TwitchInfo_ActionIsPositive", false, null);
		this.lblPointType = Localization.Get("TwitchInfo_ActionPointType", false, null);
		this.lblEnableAction = Localization.Get("TwitchInfo_ActionEnableAction", false, null) + " ([action:gui:GUI D-Pad Up])";
		this.lblDisableAction = Localization.Get("TwitchInfo_ActionDisableAction", false, null) + " ([action:gui:GUI D-Pad Up])";
		this.lblIncreasePrice = Localization.Get("TwitchInfo_IncreasePriceButton", false, null) + " ([action:gui:GUI D-Pad Right])";
		this.lblDecreasePrice = Localization.Get("TwitchInfo_DecreasePriceButton", false, null) + " ([action:gui:GUI D-Pad Left])";
		this.lblEnableVote = Localization.Get("TwitchInfo_ActionEnableVote", false, null) + " ([action:gui:GUI D-Pad Up])";
		this.lblDisableVote = Localization.Get("TwitchInfo_ActionDisableVote", false, null) + " ([action:gui:GUI D-Pad Up])";
		this.lblEnableAction_Controller = Localization.Get("TwitchInfo_ActionEnableAction", false, null) + " [action:gui:GUI HalfStack]";
		this.lblDisableAction_Controller = Localization.Get("TwitchInfo_ActionDisableAction", false, null) + " [action:gui:GUI HalfStack]";
		this.lblIncreasePrice_Controller = Localization.Get("TwitchInfo_IncreasePriceButton", false, null) + " [action:gui:GUI Inspect] + [action:gui:GUI D-Pad Right]";
		this.lblDecreasePrice_Controller = Localization.Get("TwitchInfo_DecreasePriceButton", false, null) + " [action:gui:GUI Inspect] + [action:gui:GUI D-Pad Left]";
		this.lblEnableVote_Controller = Localization.Get("TwitchInfo_ActionEnableVote", false, null) + " [action:gui:GUI HalfStack]";
		this.lblDisableVote_Controller = Localization.Get("TwitchInfo_ActionDisableVote", false, null) + " [action:gui:GUI HalfStack]";
		this.lblActionEmpty = Localization.Get("TwitchInfo_ActionEmpty", false, null);
		this.lblVoteEmpty = Localization.Get("TwitchInfo_VoteEmpty", false, null);
		this.lblActionHistoryEmpty = Localization.Get("TwitchInfo_ActionHistoryEmpty", false, null);
		this.lblLeaderboardEmpty = Localization.Get("TwitchInfo_LeaderboardEmpty", false, null);
		this.lblHistoryTargetTitle = Localization.Get("TwitchInfo_ActionHistoryTarget", false, null);
		this.lblHistoryStateTitle = Localization.Get("xuiLightPropState", false, null);
		this.lblHistoryTimeStampTitle = Localization.Get("ObjectiveTime_keyword", false, null);
		this.lblRefund = Localization.Get("TwitchInfo_ActionHistoryRefund", false, null);
		this.lblNoRefund = Localization.Get("TwitchInfo_ActionHistoryRefundNotAvailable", false, null);
		this.lblRetry = Localization.Get("TwitchInfo_ActionHistoryRetry", false, null);
		this.lblNoRetry = Localization.Get("TwitchInfo_ActionHistoryRetryNotAvailable", false, null);
		this.lblRetryActionUnavailable = Localization.Get("TwitchInfo_ActionHistoryRetryActionUnavailable", false, null);
		this.lblLeaderboardStats = Localization.Get("TwitchInfo_LeaderboardStats", false, null);
		this.lblShowBitTotal = Localization.Get("TwitchInfo_LeaderboardShowBitTotal", false, null);
		this.lblTopKiller = Localization.Get("TwitchInfo_TopKiller", false, null);
		this.lblTopGood = Localization.Get("TwitchInfo_TopGood", false, null);
		this.lblTopEvil = Localization.Get("TwitchInfo_TopEvil", false, null);
		this.lblCurrentGood = Localization.Get("TwitchInfo_CurrentGood", false, null);
		this.lblMostBits = Localization.Get("TwitchInfo_MostBits", false, null);
		this.lblTotalBits = Localization.Get("TwitchInfo_TotalBits", false, null);
		this.lblTotalBad = Localization.Get("TwitchInfo_TotalBad", false, null);
		this.lblTotalGood = Localization.Get("TwitchInfo_TotalGood", false, null);
		this.lblTotalActions = Localization.Get("TwitchInfo_TotalActions", false, null);
		this.lblLargestPimpPot = Localization.Get("TwitchInfo_LargestPimpPot", false, null);
		this.lblTrue = Localization.Get("statTrue", false, null);
		this.lblFalse = Localization.Get("statFalse", false, null);
		this.lblPointsPP = Localization.Get("TwitchPoints_PP", false, null);
		this.lblPointsSP = Localization.Get("TwitchPoints_SP", false, null);
		this.lblPointsBits = Localization.Get("TwitchPoints_Bits", false, null);
		base.registerForInputStyleChanges();
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (base.ViewComponent.IsActiveInHierarchy && (this.actionEntry != null || this.voteEntry != null))
		{
			PlayerActionsGUI guiactions = this.xui.playerUI.playerInput.GUIActions;
			if (PlatformManager.NativePlatform.Input.CurrentInputStyle == PlayerInputManager.InputStyle.Keyboard && !this.xui.playerUI.windowManager.IsInputActive())
			{
				if (this.actionEntry != null)
				{
					if (guiactions.DPad_Up.WasPressed)
					{
						this.btnEnable_OnPress(this.btnEnable.Controller, -1);
					}
					if (guiactions.DPad_Left.WasPressed)
					{
						this.BtnDecrease_OnPressed(null, -1);
					}
					if (guiactions.DPad_Right.WasPressed)
					{
						this.BtnIncrease_OnPressed(null, -1);
						return;
					}
				}
				else if (this.voteEntry != null && guiactions.DPad_Up.WasPressed)
				{
					this.btnEnable_OnPress(this.btnEnable.Controller, -1);
					return;
				}
			}
			else if (PlatformManager.NativePlatform.Input.CurrentInputStyle != PlayerInputManager.InputStyle.Keyboard)
			{
				if (this.actionEntry != null)
				{
					if (guiactions.HalfStack.WasPressed)
					{
						this.btnEnable_OnPress(this.btnEnable.Controller, -1);
					}
					if (guiactions.Inspect.IsPressed)
					{
						if (guiactions.DPad_Left.WasPressed)
						{
							this.BtnDecrease_OnPressed(null, -1);
						}
						if (guiactions.DPad_Right.WasPressed)
						{
							this.BtnIncrease_OnPressed(null, -1);
							return;
						}
					}
				}
				else if (this.voteEntry != null && guiactions.HalfStack.WasPressed)
				{
					this.btnEnable_OnPress(this.btnEnable.Controller, -1);
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnDecrease_OnPressed(XUiController _sender, int _mouseButton)
	{
		if (this.currentTwitchActionEntry != null)
		{
			this.currentTwitchActionEntry.DecreaseCost();
			base.RefreshBindings();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnIncrease_OnPressed(XUiController _sender, int _mouseButton)
	{
		if (this.currentTwitchActionEntry != null)
		{
			this.currentTwitchActionEntry.IncreaseCost();
			base.RefreshBindings();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RectStat_OnPress(XUiController _sender, int _mouseButton)
	{
		if (!this.showBitTotal)
		{
			this.showBitTotal = true;
			base.RefreshBindings();
			_sender.ViewComponent.EventOnPress = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void btnRefund_OnPress(XUiController _sender, int _mouseButton)
	{
		if (this.currentTwitchActionHistoryEntry != null)
		{
			this.currentTwitchActionHistoryEntry.Refund();
			base.RefreshBindings();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void btnRetry_OnPress(XUiController _sender, int _mouseButton)
	{
		if (this.currentTwitchActionHistoryEntry != null)
		{
			this.currentTwitchActionHistoryEntry.Retry();
			base.RefreshBindings();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void btnEnable_OnPress(XUiController _sender, int _mouseButton)
	{
		if (this.currentTwitchActionEntry != null)
		{
			TwitchManager twitchManager = TwitchManager.Current;
			TwitchActionPreset currentActionPreset = twitchManager.CurrentActionPreset;
			if (this.currentTwitchActionEntry.IsInPresetDefault(currentActionPreset))
			{
				if (currentActionPreset.RemovedActions.Contains(this.currentTwitchActionEntry.Name))
				{
					currentActionPreset.RemovedActions.Remove(this.currentTwitchActionEntry.Name);
				}
				else
				{
					currentActionPreset.RemovedActions.Add(this.currentTwitchActionEntry.Name);
				}
			}
			else if (currentActionPreset.AddedActions.Contains(this.currentTwitchActionEntry.Name))
			{
				currentActionPreset.AddedActions.Remove(this.currentTwitchActionEntry.Name);
			}
			else
			{
				currentActionPreset.AddedActions.Add(this.currentTwitchActionEntry.Name);
				if (this.currentTwitchActionEntry.DisplayCategory.Name == "Extras")
				{
					QuestEventManager.Current.TwitchEventReceived(TwitchObjectiveTypes.EnableExtras, this.currentTwitchActionEntry.DisplayCategory.Name);
				}
			}
			twitchManager.HandleChangedPropertyList();
			Manager.PlayInsidePlayerHead("craft_click_craft", -1, 0f, false, false);
			twitchManager.SetupAvailableCommands();
			twitchManager.HandleCooldownActionLocking();
			base.RefreshBindings();
			this.actionEntry.RefreshBindings();
			return;
		}
		if (this.currentTwitchVoteEntry != null)
		{
			this.currentTwitchVoteEntry.Enabled = !this.currentTwitchVoteEntry.Enabled;
			TwitchManager.Current.HandleChangedPropertyList();
			Manager.PlayInsidePlayerHead("craft_click_craft", -1, 0f, false, false);
			base.RefreshBindings();
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.stats = TwitchManager.LeaderboardStats;
		this.stats.StatsChanged += this.Stats_StatsChanged;
		base.RefreshBindings();
	}

	public override void OnClose()
	{
		base.OnClose();
		this.stats.StatsChanged -= this.Stats_StatsChanged;
		TwitchManager.Current.HandleChangedPropertyList();
		TwitchManager.Current.ResetPrices();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Stats_StatsChanged()
	{
		base.RefreshBindings();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string value, string bindingName)
	{
		uint num = <PrivateImplementationDetails>.ComputeStringHash(bindingName);
		if (num <= 2236179277U)
		{
			if (num <= 1143702022U)
			{
				if (num <= 750543317U)
				{
					if (num <= 337301254U)
					{
						if (num <= 118022627U)
						{
							if (num != 59652435U)
							{
								if (num == 118022627U)
								{
									if (bindingName == "showhistory_event")
									{
										value = (this.currentTwitchActionHistoryEntry != null && this.currentTwitchActionHistoryEntry.EventEntry != null).ToString();
										return true;
									}
								}
							}
							else if (bindingName == "actionrandomgroup")
							{
								value = ((this.currentTwitchActionEntry != null) ? ((this.currentTwitchActionEntry.RandomGroup != "") ? this.lblTrue : this.lblFalse) : "");
								return true;
							}
						}
						else if (num != 140690669U)
						{
							if (num == 337301254U)
							{
								if (bindingName == "voteendgamestage")
								{
									if (this.currentTwitchVoteEntry != null && this.currentTwitchVoteEntry.EndGameStage > 0)
									{
										value = this.currentTwitchVoteEntry.EndGameStage.ToString();
									}
									else
									{
										value = "";
									}
									return true;
								}
							}
						}
						else if (bindingName == "actiondefaultcost")
						{
							if (this.currentTwitchActionEntry != null)
							{
								value = this.currentTwitchActionEntry.ModifiedCost.ToString();
							}
							else if (this.currentTwitchActionHistoryEntry != null)
							{
								value = this.currentTwitchActionHistoryEntry.PointsSpent.ToString();
							}
							else
							{
								value = "";
							}
							return true;
						}
					}
					else if (num <= 422696738U)
					{
						if (num != 362072451U)
						{
							if (num == 422696738U)
							{
								if (bindingName == "currentgood_title")
								{
									value = ((this.stats != null) ? string.Format(this.lblCurrentGood, this.stats.GoodRewardTime) : "");
									return true;
								}
							}
						}
						else if (bindingName == "sessiontotalgood_title")
						{
							value = this.lblTotalGood;
							return true;
						}
					}
					else if (num != 718761959U)
					{
						if (num == 750543317U)
						{
							if (bindingName == "decreasepricetext")
							{
								value = "";
								if (this.currentTwitchActionEntry != null)
								{
									value = ((PlatformManager.NativePlatform.Input.CurrentInputStyle == PlayerInputManager.InputStyle.Keyboard) ? this.lblDecreasePrice : this.lblDecreasePrice_Controller);
								}
								return true;
							}
						}
					}
					else if (bindingName == "entrydescription")
					{
						if (this.currentTwitchActionEntry != null)
						{
							value = this.currentTwitchActionEntry.Description;
						}
						else if (this.currentTwitchVoteEntry != null)
						{
							value = this.currentTwitchVoteEntry.Description;
						}
						else if (this.currentTwitchActionHistoryEntry != null)
						{
							value = this.currentTwitchActionHistoryEntry.Description;
						}
						else
						{
							value = "";
						}
						return true;
					}
				}
				else if (num <= 853102448U)
				{
					if (num <= 825770997U)
					{
						if (num != 782028412U)
						{
							if (num == 825770997U)
							{
								if (bindingName == "sessionkiller_title")
								{
									value = this.lblTopKiller;
									return true;
								}
							}
						}
						else if (bindingName == "actioncommand")
						{
							if (this.currentTwitchActionEntry != null)
							{
								value = this.currentTwitchActionEntry.Command;
							}
							else if (this.currentTwitchActionHistoryEntry != null)
							{
								value = this.currentTwitchActionHistoryEntry.UserName;
							}
							else
							{
								value = "";
							}
							return true;
						}
					}
					else if (num != 831944002U)
					{
						if (num == 853102448U)
						{
							if (bindingName == "historystate")
							{
								value = ((this.currentTwitchActionHistoryEntry != null) ? this.currentTwitchActionHistoryEntry.EntryState.ToString() : "");
								return true;
							}
						}
					}
					else if (bindingName == "historytarget")
					{
						if (this.currentTwitchActionHistoryEntry != null)
						{
							value = this.currentTwitchActionHistoryEntry.Target;
						}
						else
						{
							value = "";
						}
						return true;
					}
				}
				else if (num <= 900437321U)
				{
					if (num != 884373603U)
					{
						if (num == 900437321U)
						{
							if (bindingName == "leaderboard_goodperson")
							{
								value = ((this.stats != null && this.stats.TopGoodViewer != null) ? string.Format("[{0}]{1}[-] ({2})", this.stats.TopGoodViewer.UserColor, this.stats.TopGoodViewer.Name, this.stats.TopGoodViewer.GoodActions) : "--");
								return true;
							}
						}
					}
					else if (bindingName == "showleaderboard")
					{
						value = (this.OwnerList != null && this.OwnerList.CurrentType == XUiC_TwitchEntryListWindow.ListTypes.Leaderboard).ToString();
						return true;
					}
				}
				else if (num != 930150597U)
				{
					if (num != 1079842190U)
					{
						if (num == 1143702022U)
						{
							if (bindingName == "historystatetitle")
							{
								value = this.lblHistoryStateTitle;
								return true;
							}
						}
					}
					else if (bindingName == "leaderboard_mostbits")
					{
						value = ((this.stats != null && this.stats.MostBitsSpentViewer != null) ? string.Format("[{0}]{1}[-] ({2})", this.stats.MostBitsSpentViewer.UserColor, this.stats.MostBitsSpentViewer.Name, this.stats.MostBitsSpentViewer.BitsUsed) : "--");
						return true;
					}
				}
				else if (bindingName == "leaderboard_totalbits")
				{
					if (this.showBitTotal)
					{
						value = ((this.stats != null) ? this.stats.TotalBits.ToString() : "0");
					}
					else
					{
						value = string.Format("<{0}>", this.lblShowBitTotal);
					}
					return true;
				}
			}
			else if (num <= 1548958396U)
			{
				if (num <= 1300396129U)
				{
					if (num <= 1182727743U)
					{
						if (num != 1152134642U)
						{
							if (num == 1182727743U)
							{
								if (bindingName == "leaderboard_currentgood")
								{
									value = ((this.stats != null && this.stats.CurrentGoodViewer != null) ? string.Format("[{0}]{1}[-] ({2})", this.stats.CurrentGoodViewer.UserColor, this.stats.CurrentGoodViewer.Name, this.stats.CurrentGoodViewer.CurrentGoodActions) : "--");
									return true;
								}
							}
						}
						else if (bindingName == "entryicon")
						{
							value = "";
							if (this.currentTwitchActionEntry != null && this.currentTwitchActionEntry.MainCategory != null)
							{
								value = this.currentTwitchActionEntry.MainCategory.Icon;
							}
							else if (this.currentTwitchVoteEntry != null && this.currentTwitchVoteEntry.MainVoteType != null)
							{
								value = this.currentTwitchVoteEntry.MainVoteType.Icon;
							}
							else if (this.currentTwitchActionHistoryEntry != null)
							{
								if (this.currentTwitchActionHistoryEntry.Action != null)
								{
									TwitchAction action = this.currentTwitchActionHistoryEntry.Action;
									if (action.MainCategory != null)
									{
										value = action.MainCategory.Icon;
									}
								}
								else if (this.currentTwitchActionHistoryEntry.Vote != null)
								{
									TwitchVoteType mainVoteType = this.currentTwitchActionHistoryEntry.Vote.MainVoteType;
									if (mainVoteType != null)
									{
										value = mainVoteType.Icon;
									}
								}
							}
							return true;
						}
					}
					else if (num != 1187804961U)
					{
						if (num == 1300396129U)
						{
							if (bindingName == "votestartgamestagetitle")
							{
								value = this.lblStartGamestage;
								return true;
							}
						}
					}
					else if (bindingName == "historytimestamptitle")
					{
						value = this.lblHistoryTimeStampTitle;
						return true;
					}
				}
				else if (num <= 1413159236U)
				{
					if (num != 1383999083U)
					{
						if (num == 1413159236U)
						{
							if (bindingName == "actionispositive")
							{
								value = ((this.currentTwitchActionEntry != null) ? (this.currentTwitchActionEntry.IsPositive ? this.lblTrue : this.lblFalse) : "");
								return true;
							}
						}
					}
					else if (bindingName == "showhistory_action")
					{
						value = (this.currentTwitchActionHistoryEntry != null && this.currentTwitchActionHistoryEntry.Action != null).ToString();
						return true;
					}
				}
				else if (num != 1453562025U)
				{
					if (num == 1548958396U)
					{
						if (bindingName == "actioncooldowntitle")
						{
							value = this.lblCooldown;
							return true;
						}
					}
				}
				else if (bindingName == "actioncostcolor")
				{
					value = "222,206,163,255";
					if (this.currentTwitchActionEntry != null)
					{
						int num2 = this.currentTwitchActionEntry.ModifiedCost - this.currentTwitchActionEntry.DefaultCost;
						if (num2 != 0)
						{
							if (num2 > 0)
							{
								value = "255,0,0,255";
							}
							else if (num2 < 0)
							{
								value = "0,255,0,255";
							}
						}
					}
					return true;
				}
			}
			else if (num <= 1835511105U)
			{
				if (num <= 1715807981U)
				{
					if (num != 1589536157U)
					{
						if (num == 1715807981U)
						{
							if (bindingName == "actiondiscountcost")
							{
								if (this.currentTwitchActionEntry != null && TwitchManager.Current.BitPriceMultiplier != 1f && this.currentTwitchActionEntry.PointType == TwitchAction.PointTypes.Bits && !this.currentTwitchActionEntry.IgnoreDiscount)
								{
									value = string.Format("{0} {1}", this.currentTwitchActionEntry.GetModifiedDiscountCost(), this.lblPointsBits);
								}
								else
								{
									value = "";
								}
								return true;
							}
						}
					}
					else if (bindingName == "actiongamestage")
					{
						value = ((this.currentTwitchActionEntry != null) ? this.currentTwitchActionEntry.StartGameStage.ToString() : "");
						return true;
					}
				}
				else if (num != 1780150248U)
				{
					if (num == 1835511105U)
					{
						if (bindingName == "actiondefaultcosttitle")
						{
							value = this.lblPointCost;
							return true;
						}
					}
				}
				else if (bindingName == "sessionevil_title")
				{
					value = this.lblTopEvil;
					return true;
				}
			}
			else if (num <= 1882578559U)
			{
				if (num != 1841054916U)
				{
					if (num == 1882578559U)
					{
						if (bindingName == "sessiontotalactions_title")
						{
							value = this.lblTotalActions;
							return true;
						}
					}
				}
				else if (bindingName == "historytargettitle")
				{
					value = this.lblHistoryTargetTitle;
					return true;
				}
			}
			else if (num != 2003818719U)
			{
				if (num != 2170302190U)
				{
					if (num == 2236179277U)
					{
						if (bindingName == "historytimestamp")
						{
							value = ((this.currentTwitchActionHistoryEntry != null) ? this.currentTwitchActionHistoryEntry.ActionTime : "");
							return true;
						}
					}
				}
				else if (bindingName == "showaction")
				{
					value = (this.currentTwitchActionEntry != null).ToString();
					return true;
				}
			}
			else if (bindingName == "sessiontotalbad_title")
			{
				value = this.lblTotalBad;
				return true;
			}
		}
		else if (num <= 3257770903U)
		{
			if (num <= 2818986817U)
			{
				if (num <= 2493832296U)
				{
					if (num <= 2415022578U)
					{
						if (num != 2373030536U)
						{
							if (num == 2415022578U)
							{
								if (bindingName == "actionispositivetitle")
								{
									value = this.lblIsPositive;
									return true;
								}
							}
						}
						else if (bindingName == "leaderboard_sessionkiller")
						{
							value = ((this.stats != null && this.stats.TopKillerViewer != null) ? string.Format("[{0}]{1}[-] ({2})", this.stats.TopKillerViewer.UserColor, this.stats.TopKillerViewer.Name, this.stats.TopKillerViewer.Kills) : "--");
							return true;
						}
					}
					else if (num != 2476815079U)
					{
						if (num == 2493832296U)
						{
							if (bindingName == "refundtext")
							{
								if (this.currentTwitchActionHistoryEntry != null && this.currentTwitchActionHistoryEntry.CanRefund())
								{
									value = string.Format(this.lblRefund, this.GetHistoryPointCost());
								}
								else
								{
									value = this.lblNoRefund;
								}
								return true;
							}
						}
					}
					else if (bindingName == "actionpointtypetitle")
					{
						value = this.lblPointType;
						return true;
					}
				}
				else if (num <= 2646313357U)
				{
					if (num != 2493865531U)
					{
						if (num == 2646313357U)
						{
							if (bindingName == "votestartgamestage")
							{
								if (this.currentTwitchVoteEntry != null && this.currentTwitchVoteEntry.StartGameStage > 0)
								{
									value = this.currentTwitchVoteEntry.StartGameStage.ToString();
								}
								else
								{
									value = "";
								}
								return true;
							}
						}
					}
					else if (bindingName == "sessiongood_title")
					{
						value = this.lblTopGood;
						return true;
					}
				}
				else if (num != 2658197062U)
				{
					if (num == 2818986817U)
					{
						if (bindingName == "actiondiscountcosttitle")
						{
							if (this.currentTwitchActionEntry != null && TwitchManager.Current.BitPriceMultiplier != 1f && this.currentTwitchActionEntry.PointType == TwitchAction.PointTypes.Bits && !this.currentTwitchActionEntry.IgnoreDiscount)
							{
								value = this.lblDiscountCost;
							}
							else
							{
								value = "";
							}
							return true;
						}
					}
				}
				else if (bindingName == "leaderboard_largestpot")
				{
					string text = ((TwitchManager.Current.PimpPotType == TwitchManager.PimpPotSettings.EnabledSP) ? Localization.Get("TwitchPoints_SP", false, null) : Localization.Get("TwitchPoints_PP", false, null));
					value = string.Format("{0} {1}", (this.stats != null) ? this.stats.LargestPimpPot : 0, text);
					return true;
				}
			}
			else if (num <= 3051633273U)
			{
				if (num <= 2958318179U)
				{
					if (num != 2846886419U)
					{
						if (num == 2958318179U)
						{
							if (bindingName == "entrytitle")
							{
								if (this.currentTwitchActionEntry != null)
								{
									value = this.currentTwitchActionEntry.Title;
								}
								else if (this.currentTwitchVoteEntry != null)
								{
									value = this.currentTwitchVoteEntry.VoteDescription;
								}
								else if (this.currentTwitchActionHistoryEntry != null)
								{
									value = this.currentTwitchActionHistoryEntry.Title;
								}
								else if (this.OwnerList != null && this.OwnerList.CurrentType == XUiC_TwitchEntryListWindow.ListTypes.Leaderboard)
								{
									value = this.lblLeaderboardStats;
								}
								else
								{
									value = "";
								}
								return true;
							}
						}
					}
					else if (bindingName == "sessionlargestpimppot_title")
					{
						value = this.lblLargestPimpPot;
						return true;
					}
				}
				else if (num != 3051255413U)
				{
					if (num == 3051633273U)
					{
						if (bindingName == "increasepricetext")
						{
							value = "";
							if (this.currentTwitchActionEntry != null)
							{
								value = ((PlatformManager.NativePlatform.Input.CurrentInputStyle == PlayerInputManager.InputStyle.Keyboard) ? this.lblIncreasePrice : this.lblIncreasePrice_Controller);
							}
							return true;
						}
					}
				}
				else if (bindingName == "sessionmostbits_title")
				{
					value = this.lblMostBits;
					return true;
				}
			}
			else if (num <= 3184965712U)
			{
				if (num != 3144329307U)
				{
					if (num == 3184965712U)
					{
						if (bindingName == "showvote")
						{
							value = (this.currentTwitchVoteEntry != null).ToString();
							return true;
						}
					}
				}
				else if (bindingName == "showenable")
				{
					value = (this.currentTwitchActionEntry != null || this.currentTwitchVoteEntry != null).ToString();
					return true;
				}
			}
			else if (num != 3213443644U)
			{
				if (num != 3214603061U)
				{
					if (num == 3257770903U)
					{
						if (bindingName == "showstats")
						{
							value = (this.currentTwitchActionEntry != null || this.currentTwitchVoteEntry != null || this.currentTwitchActionHistoryEntry != null || (this.OwnerList != null && this.OwnerList.CurrentType == XUiC_TwitchEntryListWindow.ListTypes.Leaderboard)).ToString();
							return true;
						}
					}
				}
				else if (bindingName == "showhistory_retry")
				{
					value = (this.currentTwitchActionHistoryEntry != null && (this.currentTwitchActionHistoryEntry.Action != null || this.currentTwitchActionHistoryEntry.EventEntry != null)).ToString();
					return true;
				}
			}
			else if (bindingName == "enablerefund")
			{
				value = (this.currentTwitchActionHistoryEntry != null && this.currentTwitchActionHistoryEntry.CanRefund()).ToString();
				return true;
			}
		}
		else if (num <= 3639271085U)
		{
			if (num <= 3487000492U)
			{
				if (num <= 3314030970U)
				{
					if (num != 3275561864U)
					{
						if (num == 3314030970U)
						{
							if (bindingName == "retrytext")
							{
								if (this.currentTwitchActionHistoryEntry != null)
								{
									if (this.currentTwitchActionHistoryEntry.CanRetry())
									{
										value = this.lblRetry;
									}
									else
									{
										value = (this.currentTwitchActionHistoryEntry.HasRetried ? this.lblNoRetry : this.lblRetryActionUnavailable);
									}
								}
								else
								{
									value = "";
								}
								return true;
							}
						}
					}
					else if (bindingName == "leaderboard_totalactions")
					{
						value = ((this.stats != null) ? this.stats.TotalActions.ToString() : "0");
						return true;
					}
				}
				else if (num != 3387479438U)
				{
					if (num == 3487000492U)
					{
						if (bindingName == "showhistory")
						{
							value = (this.currentTwitchActionHistoryEntry != null).ToString();
							return true;
						}
					}
				}
				else if (bindingName == "actionpointcost")
				{
					if (this.currentTwitchActionEntry != null)
					{
						switch (this.currentTwitchActionEntry.PointType)
						{
						case TwitchAction.PointTypes.PP:
							value = string.Format("{0} {1}", this.currentTwitchActionEntry.ModifiedCost, this.lblPointsPP);
							break;
						case TwitchAction.PointTypes.SP:
							value = string.Format("{0} {1}", this.currentTwitchActionEntry.ModifiedCost, this.lblPointsSP);
							break;
						case TwitchAction.PointTypes.Bits:
							value = string.Format("{0} {1}", this.currentTwitchActionEntry.ModifiedCost, this.lblPointsBits);
							break;
						}
					}
					else if (this.currentTwitchActionHistoryEntry != null && this.currentTwitchActionHistoryEntry.Action != null)
					{
						value = this.GetHistoryPointCost();
					}
					else
					{
						value = "";
					}
					return true;
				}
			}
			else if (num <= 3526138964U)
			{
				if (num != 3500355537U)
				{
					if (num == 3526138964U)
					{
						if (bindingName == "enableretry")
						{
							value = (this.currentTwitchActionHistoryEntry != null && this.currentTwitchActionHistoryEntry.CanRetry()).ToString();
							return true;
						}
					}
				}
				else if (bindingName == "actiongamestagetitle")
				{
					value = this.lblStartGamestage;
					return true;
				}
			}
			else if (num != 3616278858U)
			{
				if (num == 3639271085U)
				{
					if (bindingName == "enablebuttontext")
					{
						value = "";
						if (this.currentTwitchActionEntry != null)
						{
							if (this.currentTwitchActionEntry.IsInPreset(TwitchManager.Current.CurrentActionPreset))
							{
								value = ((PlatformManager.NativePlatform.Input.CurrentInputStyle == PlayerInputManager.InputStyle.Keyboard) ? this.lblDisableAction : this.lblDisableAction_Controller);
							}
							else
							{
								value = ((PlatformManager.NativePlatform.Input.CurrentInputStyle == PlayerInputManager.InputStyle.Keyboard) ? this.lblEnableAction : this.lblEnableAction_Controller);
							}
						}
						else if (this.currentTwitchVoteEntry != null)
						{
							if (this.currentTwitchVoteEntry.Enabled)
							{
								value = ((PlatformManager.NativePlatform.Input.CurrentInputStyle == PlayerInputManager.InputStyle.Keyboard) ? this.lblDisableVote : this.lblDisableVote_Controller);
							}
							else
							{
								value = ((PlatformManager.NativePlatform.Input.CurrentInputStyle == PlayerInputManager.InputStyle.Keyboard) ? this.lblEnableVote : this.lblEnableVote_Controller);
							}
						}
						return true;
					}
				}
			}
			else if (bindingName == "actioncooldown")
			{
				value = ((this.currentTwitchActionEntry != null) ? XUiM_PlayerBuffs.ConvertToTimeString(this.currentTwitchActionEntry.Cooldown) : "");
				return true;
			}
		}
		else if (num <= 4060322893U)
		{
			if (num <= 3745003420U)
			{
				if (num != 3649566128U)
				{
					if (num == 3745003420U)
					{
						if (bindingName == "leaderboard_totalgood")
						{
							value = string.Format("[AFAFFF]{0}[-]", (this.stats != null) ? this.stats.TotalGood.ToString() : "0");
							return true;
						}
					}
				}
				else if (bindingName == "voteendgamestagetitle")
				{
					value = this.lblEndGamestage;
					return true;
				}
			}
			else if (num != 3755168009U)
			{
				if (num == 4060322893U)
				{
					if (bindingName == "showempty")
					{
						value = (this.currentTwitchActionEntry == null && this.currentTwitchVoteEntry == null && this.currentTwitchActionHistoryEntry == null && this.OwnerList != null && this.OwnerList.CurrentType != XUiC_TwitchEntryListWindow.ListTypes.Leaderboard).ToString();
						return true;
					}
				}
			}
			else if (bindingName == "showhistory_vote")
			{
				value = (this.currentTwitchActionHistoryEntry != null && this.currentTwitchActionHistoryEntry.Vote != null).ToString();
				return true;
			}
		}
		else if (num <= 4142793257U)
		{
			if (num != 4101101692U)
			{
				if (num == 4142793257U)
				{
					if (bindingName == "leaderboard_badperson")
					{
						value = ((this.stats != null && this.stats.TopBadViewer != null) ? string.Format("[{0}]{1}[-] ({2})", this.stats.TopBadViewer.UserColor, this.stats.TopBadViewer.Name, this.stats.TopBadViewer.BadActions) : "--");
						return true;
					}
				}
			}
			else if (bindingName == "leaderboard_totalbad")
			{
				value = string.Format("[FFAFAF]{0}[-]", (this.stats != null) ? this.stats.TotalBad.ToString() : "0");
				return true;
			}
		}
		else if (num != 4227361571U)
		{
			if (num != 4259958381U)
			{
				if (num == 4292913866U)
				{
					if (bindingName == "sessiontotalbits_title")
					{
						value = this.lblTotalBits;
						return true;
					}
				}
			}
			else if (bindingName == "emptytext")
			{
				value = "";
				if (this.OwnerList != null)
				{
					switch (this.OwnerList.CurrentType)
					{
					case XUiC_TwitchEntryListWindow.ListTypes.Actions:
						value = this.lblActionEmpty;
						break;
					case XUiC_TwitchEntryListWindow.ListTypes.Votes:
						value = this.lblVoteEmpty;
						break;
					case XUiC_TwitchEntryListWindow.ListTypes.ActionHistory:
						value = this.lblActionHistoryEmpty;
						break;
					case XUiC_TwitchEntryListWindow.ListTypes.Leaderboard:
						value = this.lblLeaderboardEmpty;
						break;
					}
				}
				return true;
			}
		}
		else if (bindingName == "actionrandomgrouptitle")
		{
			value = this.lblRandomDaily;
			return true;
		}
		return base.GetBindingValueInternal(ref value, bindingName);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string GetHistoryPointCost()
	{
		if (this.currentTwitchActionHistoryEntry == null || this.currentTwitchActionHistoryEntry.Action == null)
		{
			return "";
		}
		switch (this.currentTwitchActionHistoryEntry.Action.PointType)
		{
		case TwitchAction.PointTypes.PP:
			return string.Format("{0} {1}", this.currentTwitchActionHistoryEntry.PointsSpent, this.lblPointsPP);
		case TwitchAction.PointTypes.SP:
			return string.Format("{0} {1}", this.currentTwitchActionHistoryEntry.PointsSpent, this.lblPointsSP);
		case TwitchAction.PointTypes.Bits:
			return string.Format("{0} {1}", this.currentTwitchActionHistoryEntry.PointsSpent, this.lblPointsBits);
		default:
			return "";
		}
	}

	public void SetTwitchAction(XUiC_TwitchActionEntry twitchInfoEntry)
	{
		this.actionEntry = twitchInfoEntry;
		this.voteEntry = null;
		this.CurrentTwitchVoteEntry = null;
		this.historyEntry = null;
		this.CurrentTwitchActionHistoryEntry = null;
		if (this.actionEntry != null)
		{
			this.CurrentTwitchActionEntry = this.actionEntry.Action;
			return;
		}
		this.CurrentTwitchActionEntry = null;
	}

	public void SetTwitchVote(XUiC_TwitchVoteInfoEntry twitchInfoEntry)
	{
		this.voteEntry = twitchInfoEntry;
		this.actionEntry = null;
		this.CurrentTwitchActionEntry = null;
		this.historyEntry = null;
		this.CurrentTwitchActionHistoryEntry = null;
		if (this.voteEntry != null)
		{
			this.CurrentTwitchVoteEntry = this.voteEntry.Vote;
			return;
		}
		this.CurrentTwitchVoteEntry = null;
	}

	public void SetTwitchHistory(XUiC_TwitchActionHistoryEntry twitchInfoEntry)
	{
		this.historyEntry = twitchInfoEntry;
		this.actionEntry = null;
		this.CurrentTwitchActionEntry = null;
		this.voteEntry = null;
		this.CurrentTwitchVoteEntry = null;
		if (this.historyEntry != null)
		{
			this.CurrentTwitchActionHistoryEntry = this.historyEntry.HistoryItem;
			return;
		}
		this.CurrentTwitchActionHistoryEntry = null;
	}

	public void ClearEntries()
	{
		this.actionEntry = null;
		this.CurrentTwitchActionEntry = null;
		this.voteEntry = null;
		this.CurrentTwitchVoteEntry = null;
		this.historyEntry = null;
		this.CurrentTwitchActionHistoryEntry = null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TwitchActionEntry actionEntry;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TwitchVoteInfoEntry voteEntry;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TwitchActionHistoryEntry historyEntry;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Button btnEnable;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Button btnRefund;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnRetry;

	public XUiC_TwitchEntryListWindow OwnerList;

	[PublicizedFrom(EAccessModifier.Private)]
	public TwitchAction currentTwitchActionEntry;

	[PublicizedFrom(EAccessModifier.Private)]
	public TwitchVote currentTwitchVoteEntry;

	[PublicizedFrom(EAccessModifier.Private)]
	public TwitchActionHistoryEntry currentTwitchActionHistoryEntry;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblStartGamestage;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblEndGamestage;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblPointCost;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblCooldown;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblRandomDaily;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblIsPositive;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblPointType;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblEnableAction;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblDisableAction;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblEnableVote;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblDisableVote;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblActionEmpty;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblVoteEmpty;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblActionHistoryEmpty;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblLeaderboardEmpty;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblHistoryTargetTitle;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblHistoryStateTitle;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblHistoryTimeStampTitle;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblRefund;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblNoRefund;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblRetry;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblNoRetry;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblRetryActionUnavailable;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblLeaderboardStats;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblShowBitTotal;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblDiscountCost;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblIncreasePrice;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblDecreasePrice;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblEnableAction_Controller;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblDisableAction_Controller;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblEnableVote_Controller;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblDisableVote_Controller;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblIncreasePrice_Controller;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblDecreasePrice_Controller;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblTopKiller;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblTopGood;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblTopEvil;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblCurrentGood;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblMostBits;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblTotalBits;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblTotalBad;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblTotalGood;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblTotalActions;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblLargestPimpPot;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblTrue;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblFalse;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblPointsPP;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblPointsSP;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblPointsBits;

	[PublicizedFrom(EAccessModifier.Private)]
	public TwitchLeaderboardStats stats;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool showBitTotal;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly string DayTimeFormatString = Localization.Get("xuiDay", false, null) + "{0}, {1:00}:{2:00}";

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<ulong> daytimeFormatter = new CachedStringFormatter<ulong>((ulong _worldTime) => ValueDisplayFormatters.WorldTime(_worldTime, XUiC_TwitchEntryDescriptionWindow.DayTimeFormatString));
}
