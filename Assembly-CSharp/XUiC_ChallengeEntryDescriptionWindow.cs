using System;
using Audio;
using Challenges;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_ChallengeEntryDescriptionWindow : XUiController
{
	public Challenge CurrentChallengeEntry
	{
		get
		{
			return this.currentChallenge;
		}
		set
		{
			this.currentChallenge = value;
			this.challengeClass = ((this.currentChallenge != null) ? this.currentChallenge.ChallengeClass : null);
			base.RefreshBindings();
		}
	}

	public override void Init()
	{
		base.Init();
		this.btnTrack = base.GetChildById("btnTrack").GetChildByType<XUiC_SimpleButton>();
		this.btnTrack.OnPressed += this.BtnTrack_OnPressed;
		this.btnComplete = base.GetChildById("btnComplete").GetChildByType<XUiC_SimpleButton>();
		this.btnComplete.OnPressed += this.BtnComplete_OnPressed;
		this.gotoButton = base.GetChildById("gotoButton");
		if (this.gotoButton != null)
		{
			this.gotoButton.OnPress += this.GotoButton_OnPress;
		}
		base.registerForInputStyleChanges();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void GotoButton_OnPress(XUiController _sender, int _mouseButton)
	{
		if (this.currentChallenge != null)
		{
			BaseChallengeObjective navObjective = this.currentChallenge.GetNavObjective();
			ChallengeObjectiveCraft challengeObjectiveCraft = navObjective as ChallengeObjectiveCraft;
			if (challengeObjectiveCraft != null)
			{
				Recipe itemRecipe = challengeObjectiveCraft.itemRecipe;
				XUiC_WindowSelector.OpenSelectorAndWindow(this.xui.playerUI.entityPlayer, "crafting");
				XUiC_RecipeList childByType = this.xui.GetChildByType<XUiC_RecipeList>();
				if (childByType != null)
				{
					childByType.SetRecipeDataByItem(challengeObjectiveCraft.itemRecipe.itemValueType);
					return;
				}
			}
			else if (navObjective is ChallengeObjectiveTwitch)
			{
				XUiC_TwitchWindowSelector.OpenSelectorAndWindow(GameManager.Instance.World.GetPrimaryPlayer(), "Actions", true);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void inputStyleChanged(PlayerInputManager.InputStyle _oldStyle, PlayerInputManager.InputStyle _newStyle)
	{
		base.inputStyleChanged(_oldStyle, _newStyle);
		this.IsDirty = true;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (this.IsDirty || (this.currentChallenge != null && this.currentChallenge.NeedsUIUpdate))
		{
			base.RefreshBindings();
			this.IsDirty = false;
		}
		if (base.ViewComponent.IsActiveInHierarchy && this.currentChallenge != null)
		{
			PlayerActionsGUI guiactions = this.xui.playerUI.playerInput.GUIActions;
			if (PlatformManager.NativePlatform.Input.CurrentInputStyle == PlayerInputManager.InputStyle.Keyboard && !this.xui.playerUI.windowManager.IsInputActive())
			{
				if (guiactions.DPad_Up.WasPressed)
				{
					this.BtnComplete_OnPressed(this.btnComplete, -1);
				}
				if (guiactions.DPad_Left.WasPressed)
				{
					this.BtnTrack_OnPressed(this.btnTrack, -1);
				}
			}
		}
	}

	public void TrackCurrentChallenege()
	{
		if (this.currentChallenge != null && this.currentChallenge.IsActive)
		{
			if (this.xui.QuestTracker.TrackedChallenge == this.currentChallenge)
			{
				this.xui.QuestTracker.TrackedChallenge = null;
			}
			else
			{
				this.xui.QuestTracker.TrackedChallenge = this.currentChallenge;
				Manager.PlayInsidePlayerHead("ui_challenge_track", -1, 0f, false, false);
			}
			this.entry.Owner.MarkDirty();
		}
	}

	public void CompleteCurrentChallenege()
	{
		if (this.currentChallenge != null && this.currentChallenge.ReadyToComplete)
		{
			this.currentChallenge.ChallengeState = Challenge.ChallengeStates.Redeemed;
			this.currentChallenge.Redeem();
			QuestEventManager.Current.ChallengeCompleted(this.challengeClass, true);
			this.currentChallenge = this.currentChallenge.Owner.GetNextRedeemableChallenge(this.currentChallenge);
			XUiC_ChallengeEntry selectedEntry = XUiC_ChallengeEntryList.SelectedEntry;
			if (selectedEntry != null && selectedEntry.Entry != this.currentChallenge)
			{
				this.entry.Owner.SetEntryByChallenge(this.currentChallenge);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnTrack_OnPressed(XUiController _sender, int _mouseButton)
	{
		this.TrackCurrentChallenege();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnComplete_OnPressed(XUiController _sender, int _mouseButton)
	{
		this.CompleteCurrentChallenege();
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.RefreshButtonLabels(PlatformManager.NativePlatform.Input.CurrentInputStyle);
		QuestEventManager.Current.ChallengeComplete += this.Current_ChallengeComplete;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Current_ChallengeComplete(ChallengeClass challenge, bool isRedeemed)
	{
		base.RefreshBindings();
	}

	public override void OnClose()
	{
		base.OnClose();
		QuestEventManager.Current.ChallengeComplete -= this.Current_ChallengeComplete;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string value, string bindingName)
	{
		uint num = <PrivateImplementationDetails>.ComputeStringHash(bindingName);
		if (num <= 2281937273U)
		{
			if (num <= 908978632U)
			{
				if (num <= 227073911U)
				{
					if (num <= 193518673U)
					{
						if (num != 176741054U)
						{
							if (num == 193518673U)
							{
								if (bindingName == "hasobjective5")
								{
									if (this.currentChallenge != null)
									{
										value = (this.currentChallenge.ObjectiveList.Count > 4).ToString();
									}
									else
									{
										value = "false";
									}
									return true;
								}
							}
						}
						else if (bindingName == "hasobjective4")
						{
							if (this.currentChallenge != null)
							{
								value = (this.currentChallenge.ObjectiveList.Count > 3).ToString();
							}
							else
							{
								value = "false";
							}
							return true;
						}
					}
					else if (num != 210296292U)
					{
						if (num == 227073911U)
						{
							if (bindingName == "hasobjective3")
							{
								if (this.currentChallenge != null)
								{
									value = (this.currentChallenge.ObjectiveList.Count > 2).ToString();
								}
								else
								{
									value = "false";
								}
								return true;
							}
						}
					}
					else if (bindingName == "hasobjective2")
					{
						if (this.currentChallenge != null)
						{
							value = (this.currentChallenge.ObjectiveList.Count > 1).ToString();
						}
						else
						{
							value = "false";
						}
						return true;
					}
				}
				else if (num <= 338523197U)
				{
					if (num != 260629149U)
					{
						if (num == 338523197U)
						{
							if (bindingName == "enabletrack")
							{
								value = (this.currentChallenge != null && this.currentChallenge.ChallengeState == Challenge.ChallengeStates.Active).ToString();
								return true;
							}
						}
					}
					else if (bindingName == "hasobjective1")
					{
						if (this.currentChallenge != null)
						{
							value = (this.currentChallenge.ObjectiveList.Count > 0).ToString();
						}
						else
						{
							value = "false";
						}
						return true;
					}
				}
				else if (num != 718761959U)
				{
					if (num == 908978632U)
					{
						if (bindingName == "has4objective")
						{
							if (this.currentChallenge != null)
							{
								value = (this.currentChallenge.ObjectiveList.Count == 4).ToString();
							}
							else
							{
								value = "false";
							}
							return true;
						}
					}
				}
				else if (bindingName == "entrydescription")
				{
					value = ((this.currentChallenge != null) ? this.challengeClass.GetDescription() : "");
					return true;
				}
			}
			else if (num <= 1507958455U)
			{
				if (num <= 1152134642U)
				{
					if (num != 1003914668U)
					{
						if (num == 1152134642U)
						{
							if (bindingName == "entryicon")
							{
								value = ((this.currentChallenge != null) ? this.challengeClass.Icon : "");
								return true;
							}
						}
					}
					else if (bindingName == "rewardtitle")
					{
						value = Localization.Get("xuiRewards", false, null);
						return true;
					}
				}
				else if (num != 1498809836U)
				{
					if (num == 1507958455U)
					{
						if (bindingName == "entryshortdescription")
						{
							value = ((this.currentChallenge != null) ? this.challengeClass.ShortDescription : "");
							return true;
						}
					}
				}
				else if (bindingName == "haschallenge")
				{
					value = (this.currentChallenge != null).ToString();
					return true;
				}
			}
			else if (num <= 1833103823U)
			{
				if (num != 1518662865U)
				{
					if (num == 1833103823U)
					{
						if (bindingName == "showgoto")
						{
							value = (this.currentChallenge != null && this.currentChallenge.ChallengeClass.HasNavType).ToString();
							return true;
						}
					}
				}
				else if (bindingName == "rewardtext")
				{
					if (this.currentChallenge != null)
					{
						value = (this.currentChallenge.AutoCompleted ? "--" : this.challengeClass.RewardText);
					}
					else
					{
						value = "";
					}
					return true;
				}
			}
			else if (num != 1960932512U)
			{
				if (num != 2048631988U)
				{
					if (num == 2281937273U)
					{
						if (bindingName == "has1objective")
						{
							if (this.currentChallenge != null)
							{
								value = (this.currentChallenge.ObjectiveList.Count == 1).ToString();
							}
							else
							{
								value = "false";
							}
							return true;
						}
					}
				}
				else if (bindingName == "hasreward")
				{
					value = ((this.currentChallenge != null) ? (this.challengeClass.RewardEvent != "").ToString() : "false");
					return true;
				}
			}
			else if (bindingName == "adjustedheight")
			{
				if (this.currentChallenge != null)
				{
					switch (this.currentChallenge.ObjectiveList.Count)
					{
					case 1:
						value = "276";
						return true;
					case 2:
						value = "236";
						return true;
					case 3:
						value = "196";
						return true;
					case 4:
						value = "156";
						return true;
					case 5:
						value = "116";
						return true;
					}
				}
				value = "196";
				return true;
			}
		}
		else if (num <= 3256735460U)
		{
			if (num <= 2958318179U)
			{
				if (num <= 2675497066U)
				{
					if (num != 2650623958U)
					{
						if (num == 2675497066U)
						{
							if (bindingName == "entrygroup")
							{
								value = ((this.currentChallenge != null) ? this.currentChallenge.ChallengeGroup.Title : "");
								return true;
							}
						}
					}
					else if (bindingName == "enableredeem")
					{
						value = (this.currentChallenge != null && this.currentChallenge.ReadyToComplete).ToString();
						return true;
					}
				}
				else if (num != 2725401013U)
				{
					if (num == 2958318179U)
					{
						if (bindingName == "entrytitle")
						{
							value = ((this.currentChallenge != null) ? this.challengeClass.Title : "");
							return true;
						}
					}
				}
				else if (bindingName == "has5objective")
				{
					if (this.currentChallenge != null)
					{
						value = (this.currentChallenge.ObjectiveList.Count == 5).ToString();
					}
					else
					{
						value = "false";
					}
					return true;
				}
			}
			else if (num <= 3223180222U)
			{
				if (num != 3206402603U)
				{
					if (num == 3223180222U)
					{
						if (bindingName == "objective2")
						{
							if (this.currentChallenge != null)
							{
								value = ((this.currentChallenge.ObjectiveList.Count > 1) ? this.currentChallenge.ObjectiveList[1].ObjectiveText : "");
							}
							else
							{
								value = "";
							}
							return true;
						}
					}
				}
				else if (bindingName == "objective1")
				{
					if (this.currentChallenge != null)
					{
						value = ((this.currentChallenge.ObjectiveList.Count > 0) ? this.currentChallenge.ObjectiveList[0].ObjectiveText : "");
					}
					else
					{
						value = "";
					}
					return true;
				}
			}
			else if (num != 3239957841U)
			{
				if (num == 3256735460U)
				{
					if (bindingName == "objective4")
					{
						if (this.currentChallenge != null)
						{
							value = ((this.currentChallenge.ObjectiveList.Count > 3) ? this.currentChallenge.ObjectiveList[3].ObjectiveText : "");
						}
						else
						{
							value = "";
						}
						return true;
					}
				}
			}
			else if (bindingName == "objective3")
			{
				if (this.currentChallenge != null)
				{
					value = ((this.currentChallenge.ObjectiveList.Count > 2) ? this.currentChallenge.ObjectiveList[2].ObjectiveText : "");
				}
				else
				{
					value = "";
				}
				return true;
			}
		}
		else if (num <= 3746182248U)
		{
			if (num <= 3279637982U)
			{
				if (num != 3273513079U)
				{
					if (num == 3279637982U)
					{
						if (bindingName == "has2objective")
						{
							if (this.currentChallenge != null)
							{
								value = (this.currentChallenge.ObjectiveList.Count == 2).ToString();
							}
							else
							{
								value = "false";
							}
							return true;
						}
					}
				}
				else if (bindingName == "objective5")
				{
					if (this.currentChallenge != null)
					{
						value = ((this.currentChallenge.ObjectiveList.Count > 4) ? this.currentChallenge.ObjectiveList[4].ObjectiveText : "");
					}
					else
					{
						value = "";
					}
					return true;
				}
			}
			else if (num != 3479566019U)
			{
				if (num == 3746182248U)
				{
					if (bindingName == "objectivefill1")
					{
						if (this.currentChallenge != null)
						{
							value = ((this.currentChallenge.ObjectiveList.Count > 0) ? this.currentChallenge.ObjectiveList[0].FillAmount.ToString() : "0");
						}
						else
						{
							value = "0";
						}
						return true;
					}
				}
			}
			else if (bindingName == "has3objective")
			{
				if (this.currentChallenge != null)
				{
					value = (this.currentChallenge.ObjectiveList.Count == 3).ToString();
				}
				else
				{
					value = "false";
				}
				return true;
			}
		}
		else if (num <= 3796515105U)
		{
			if (num != 3779737486U)
			{
				if (num == 3796515105U)
				{
					if (bindingName == "objectivefill2")
					{
						if (this.currentChallenge != null)
						{
							value = ((this.currentChallenge.ObjectiveList.Count > 1) ? this.currentChallenge.ObjectiveList[1].FillAmount.ToString() : "0");
						}
						else
						{
							value = "0";
						}
						return true;
					}
				}
			}
			else if (bindingName == "objectivefill3")
			{
				if (this.currentChallenge != null)
				{
					value = ((this.currentChallenge.ObjectiveList.Count > 2) ? this.currentChallenge.ObjectiveList[2].FillAmount.ToString() : "0");
				}
				else
				{
					value = "0";
				}
				return true;
			}
		}
		else if (num != 3813292724U)
		{
			if (num != 3830070343U)
			{
				if (num == 4060322893U)
				{
					if (bindingName == "showempty")
					{
						value = (this.currentChallenge == null).ToString();
						return true;
					}
				}
			}
			else if (bindingName == "objectivefill4")
			{
				if (this.currentChallenge != null)
				{
					value = ((this.currentChallenge.ObjectiveList.Count > 3) ? this.currentChallenge.ObjectiveList[3].FillAmount.ToString() : "0");
				}
				else
				{
					value = "0";
				}
				return true;
			}
		}
		else if (bindingName == "objectivefill5")
		{
			if (this.currentChallenge != null)
			{
				value = ((this.currentChallenge.ObjectiveList.Count > 4) ? this.currentChallenge.ObjectiveList[4].FillAmount.ToString() : "0");
			}
			else
			{
				value = "0";
			}
			return true;
		}
		return base.GetBindingValueInternal(ref value, bindingName);
	}

	public void SetChallenge(XUiC_ChallengeEntry challengeEntry)
	{
		this.entry = challengeEntry;
		if (this.entry != null)
		{
			this.CurrentChallengeEntry = this.entry.Entry;
			return;
		}
		this.CurrentChallengeEntry = null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public new void OnLastInputStyleChanged(PlayerInputManager.InputStyle _style)
	{
		this.RefreshButtonLabels(_style);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RefreshButtonLabels(PlayerInputManager.InputStyle _style)
	{
		if (_style == PlayerInputManager.InputStyle.Keyboard)
		{
			(this.btnTrack.GetChildById("btnLabel").ViewComponent as XUiV_Label).Text = string.Format(Localization.Get("journalTrack", false, null), LocalPlayerUI.primaryUI.playerInput.GUIActions.DPad_Left.GetBindingString(false, PlayerInputManager.InputStyle.Undefined, XUiUtils.EmptyBindingStyle.LocalizedNone, XUiUtils.DisplayStyle.KeyboardWithAngleBrackets, false, null));
			(this.btnComplete.GetChildById("btnLabel").ViewComponent as XUiV_Label).Text = string.Format(Localization.Get("journalComplete", false, null), LocalPlayerUI.primaryUI.playerInput.GUIActions.DPad_Up.GetBindingString(false, PlayerInputManager.InputStyle.Undefined, XUiUtils.EmptyBindingStyle.LocalizedNone, XUiUtils.DisplayStyle.KeyboardWithAngleBrackets, false, null));
			return;
		}
		(this.btnTrack.GetChildById("btnLabel").ViewComponent as XUiV_Label).Text = string.Format(Localization.Get("journalTrack", false, null), LocalPlayerUI.primaryUI.playerInput.GUIActions.HalfStack.GetBindingString(true, PlatformManager.NativePlatform.Input.CurrentControllerInputStyle, XUiUtils.EmptyBindingStyle.LocalizedNone, XUiUtils.DisplayStyle.Plain, false, null));
		(this.btnComplete.GetChildById("btnLabel").ViewComponent as XUiV_Label).Text = string.Format(Localization.Get("journalComplete", false, null), LocalPlayerUI.primaryUI.playerInput.GUIActions.Inspect.GetBindingString(true, PlatformManager.NativePlatform.Input.CurrentControllerInputStyle, XUiUtils.EmptyBindingStyle.LocalizedNone, XUiUtils.DisplayStyle.Plain, false, null));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ChallengeEntry entry;

	[PublicizedFrom(EAccessModifier.Private)]
	public Challenge currentChallenge;

	[PublicizedFrom(EAccessModifier.Private)]
	public ChallengeClass challengeClass;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnTrack;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnComplete;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController gotoButton;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly string DayTimeFormatString = Localization.Get("xuiDay", false, null) + " {0}, {1:00}:{2:00}";

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<ulong> daytimeFormatter = new CachedStringFormatter<ulong>((ulong _worldTime) => ValueDisplayFormatters.WorldTime(_worldTime, XUiC_ChallengeEntryDescriptionWindow.DayTimeFormatString));
}
