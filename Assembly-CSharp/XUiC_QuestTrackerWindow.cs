using System;
using Challenges;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_QuestTrackerWindow : XUiController
{
	public Quest CurrentQuest
	{
		get
		{
			return this.currentQuest;
		}
		set
		{
			this.currentQuest = value;
			this.questClass = ((value != null) ? QuestClass.GetQuest(this.currentQuest.ID) : null);
			if (value != null)
			{
				if (this.currentChallenge != null)
				{
					this.currentChallenge.OnChallengeStateChanged -= this.CurrentChallenge_OnChallengeStateChanged;
				}
				this.currentChallenge = null;
				this.challengeClass = null;
			}
			base.RefreshBindings();
		}
	}

	public Challenge CurrentChallenge
	{
		get
		{
			return this.currentChallenge;
		}
		set
		{
			if (this.currentChallenge != null)
			{
				this.currentChallenge.OnChallengeStateChanged -= this.CurrentChallenge_OnChallengeStateChanged;
			}
			this.currentChallenge = value;
			this.challengeClass = ((value != null) ? this.currentChallenge.ChallengeClass : null);
			if (value != null)
			{
				this.currentQuest = null;
				this.questClass = null;
				this.currentChallenge.OnChallengeStateChanged += this.CurrentChallenge_OnChallengeStateChanged;
			}
			base.RefreshBindings();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CurrentChallenge_OnChallengeStateChanged(Challenge challenge)
	{
		base.RefreshBindings();
	}

	public override void Init()
	{
		base.Init();
		XUiC_QuestTrackerWindow.ID = base.WindowGroup.Id;
		this.objectiveList = base.GetChildByType<XUiC_QuestTrackerObjectiveList>();
		base.registerForInputStyleChanges();
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
		if (this.localPlayer == null)
		{
			this.localPlayer = this.xui.playerUI.entityPlayer;
		}
		GUIWindowManager windowManager = this.xui.playerUI.windowManager;
		if (windowManager.IsHUDEnabled() || (this.xui.DragAndDropWindow.InMenu && windowManager.IsHUDPartialHidden()))
		{
			if (base.ViewComponent.IsVisible && this.localPlayer.IsDead())
			{
				this.IsDirty = true;
			}
			else if (!base.ViewComponent.IsVisible && !this.localPlayer.IsDead())
			{
				this.IsDirty = true;
			}
			if (this.currentChallenge != null && this.currentChallenge.UIDirty)
			{
				this.IsDirty = true;
				this.currentChallenge.UIDirty = false;
			}
			if (this.IsDirty)
			{
				this.objectiveList.Quest = this.currentQuest;
				this.objectiveList.Challenge = this.currentChallenge;
				base.RefreshBindings();
				this.IsDirty = false;
			}
			return;
		}
		base.ViewComponent.IsVisible = false;
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.xui.QuestTracker.OnTrackedQuestChanged += this.QuestTracker_OnTrackedQuestChanged;
		this.xui.playerUI.entityPlayer.QuestChanged += this.QuestJournal_QuestChanged;
		this.xui.QuestTracker.OnTrackedChallengeChanged += this.QuestTracker_OnTrackedChallengeChanged;
		this.xui.playerUI.entityPlayer.QuestJournal.RefreshTracked();
		if (this.xui.QuestTracker.TrackedQuest != null)
		{
			this.CurrentQuest = this.xui.QuestTracker.TrackedQuest;
			return;
		}
		if (this.xui.QuestTracker.TrackedChallenge != null)
		{
			this.CurrentChallenge = this.xui.QuestTracker.TrackedChallenge;
		}
	}

	public override void OnClose()
	{
		base.OnClose();
		if (XUi.IsGameRunning())
		{
			this.xui.QuestTracker.OnTrackedQuestChanged -= this.QuestTracker_OnTrackedQuestChanged;
			this.xui.playerUI.entityPlayer.QuestChanged -= this.QuestJournal_QuestChanged;
			this.xui.QuestTracker.OnTrackedChallengeChanged -= this.QuestTracker_OnTrackedChallengeChanged;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void QuestTracker_OnTrackedQuestChanged()
	{
		this.CurrentQuest = this.xui.QuestTracker.TrackedQuest;
		this.IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void QuestTracker_OnTrackedChallengeChanged()
	{
		this.CurrentChallenge = this.xui.QuestTracker.TrackedChallenge;
		this.IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void QuestJournal_QuestChanged(Quest q)
	{
		if (this.CurrentQuest == q)
		{
			this.IsDirty = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string value, string bindingName)
	{
		uint num = <PrivateImplementationDetails>.ComputeStringHash(bindingName);
		if (num <= 2783733891U)
		{
			if (num <= 1901444352U)
			{
				if (num != 112224632U)
				{
					if (num == 1901444352U)
					{
						if (bindingName == "questhint")
						{
							if (this.currentQuest != null)
							{
								value = this.questClass.GetCurrentHint((int)this.currentQuest.CurrentPhase);
							}
							else if (this.currentChallenge != null)
							{
								value = this.challengeClass.GetHint(this.currentChallenge.NeedsPreRequisites);
							}
							else
							{
								value = "";
							}
							return true;
						}
					}
				}
				else if (bindingName == "questicon")
				{
					if (this.currentQuest != null)
					{
						value = this.questClass.Icon;
					}
					else if (this.currentChallenge != null)
					{
						value = this.challengeClass.Icon;
					}
					else
					{
						value = "";
					}
					return true;
				}
			}
			else if (num != 2730462270U)
			{
				if (num == 2783733891U)
				{
					if (bindingName == "questhintposition")
					{
						if (this.currentQuest != null)
						{
							value = string.Format("0,{0}", -50 + this.currentQuest.ActiveObjectives * -27);
						}
						else if (this.currentChallenge != null)
						{
							value = string.Format("0,{0}", -50 + this.currentChallenge.ActiveObjectives * -27);
						}
						else
						{
							value = "0,0";
						}
						return true;
					}
				}
			}
			else if (bindingName == "questname")
			{
				if (this.currentQuest != null)
				{
					value = this.questClass.Name;
				}
				else if (this.currentChallenge != null)
				{
					value = this.challengeClass.Title;
				}
				else
				{
					value = "";
				}
				return true;
			}
		}
		else if (num <= 3047389681U)
		{
			if (num != 2823605611U)
			{
				if (num == 3047389681U)
				{
					if (bindingName == "questtitle")
					{
						if (this.currentQuest != null)
						{
							value = this.questClass.SubTitle;
						}
						else if (this.currentChallenge != null)
						{
							value = this.challengeClass.Title;
						}
						else
						{
							value = "";
						}
						return true;
					}
				}
			}
			else if (bindingName == "questhintavailable")
			{
				if (this.currentQuest != null)
				{
					value = (this.questClass.GetCurrentHint((int)this.currentQuest.CurrentPhase) != "").ToString();
				}
				else if (this.currentChallenge != null)
				{
					value = (this.challengeClass.GetHint(this.currentChallenge.NeedsPreRequisites) != "").ToString();
				}
				else
				{
					value = "false";
				}
				return true;
			}
		}
		else if (num != 3231221182U)
		{
			if (num != 4060322893U)
			{
				if (num == 4116915492U)
				{
					if (bindingName == "trackerheight")
					{
						if (this.currentQuest != null)
						{
							value = this.trackerheightFormatter.Format(this.currentQuest.ActiveObjectives * 27);
						}
						else if (this.currentChallenge != null)
						{
							value = this.trackerheightFormatter.Format(this.currentChallenge.ActiveObjectives * 27);
						}
						else
						{
							value = "0";
						}
						return true;
					}
				}
			}
			else if (bindingName == "showempty")
			{
				value = (this.currentQuest == null && this.currentChallenge == null).ToString();
				return true;
			}
		}
		else if (bindingName == "showquest")
		{
			value = ((this.currentQuest != null || this.currentChallenge != null) && XUi.IsGameRunning() && this.localPlayer != null && !this.localPlayer.IsDead()).ToString();
			return true;
		}
		return base.GetBindingValueInternal(ref value, bindingName);
	}

	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_QuestTrackerObjectiveList objectiveList;

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityPlayerLocal localPlayer;

	[PublicizedFrom(EAccessModifier.Private)]
	public QuestClass questClass;

	[PublicizedFrom(EAccessModifier.Private)]
	public ChallengeClass challengeClass;

	[PublicizedFrom(EAccessModifier.Private)]
	public Quest currentQuest;

	[PublicizedFrom(EAccessModifier.Private)]
	public Challenge currentChallenge;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterInt trackerheightFormatter = new CachedStringFormatterInt();
}
