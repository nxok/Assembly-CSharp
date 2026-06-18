using System;
using System.Collections.Generic;
using System.Globalization;
using Challenges;
using Platform;
using Twitch;
using UniLinq;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_OptionsTwitch : XUiC_OptionsDialogBase
{
	public static event Action OnSettingsChanged;

	public override void Init()
	{
		base.Init();
		this.tabs = base.GetChildByType<XUiC_TabSelector>();
		this.tabs.OnTabChanged += this.Tabs_OnTabChanged;
		this.comboAllowActionsDuringBloodmoon = base.GetChildById("AllowActionsDuringBloodmoon").GetChildByType<XUiC_ComboBoxEnum<XUiC_OptionsTwitch.TwitchBloodMoonOptions>>();
		this.comboAllowActionsDuringQuests = base.GetChildById("AllowActionsDuringQuests").GetChildByType<XUiC_ComboBoxEnum<XUiC_OptionsTwitch.TwitchBloodMoonOptions>>();
		this.comboPointsGeneration = base.GetChildById("PointGeneration").GetChildByType<XUiC_ComboBoxEnum<XUiC_OptionsTwitch.PointsGenerationOptions>>();
		this.comboCooldownPreset = base.GetChildById("CooldownPreset").GetChildByType<XUiC_ComboBoxList<string>>();
		this.comboUseProgression = base.GetChildById("UseProgression").GetChildByType<XUiC_ComboBoxBool>();
		this.comboOptOutTwitch = base.GetChildById("OptOutTwitch").GetChildByType<XUiC_ComboBoxBool>();
		this.comboAllowVisionEffects = base.GetChildById("AllowVisual").GetChildByType<XUiC_ComboBoxBool>();
		this.comboAllowCrateSharing = base.GetChildById("AllowCrateSharing").GetChildByType<XUiC_ComboBoxBool>();
		this.comboActionSpamDelay = base.GetChildById("ActionSpamDelay").GetChildByType<XUiC_ComboBoxList<string>>();
		this.comboActionTwitchIntegrationType = base.GetChildById("ActionTwitchIntegrationType").GetChildByType<XUiC_ComboBoxEnum<TwitchManager.IntegrationSettings>>();
		this.comboAllowActions = base.GetChildById("AllowActions").GetChildByType<XUiC_ComboBoxList<string>>();
		this.comboAllowVotes = base.GetChildById("AllowVotes").GetChildByType<XUiC_ComboBoxList<string>>();
		this.comboAllowEvents = base.GetChildById("AllowEvents").GetChildByType<XUiC_ComboBoxList<string>>();
		this.comboAllowVotesDuringBloodmoon = base.GetChildById("AllowVotesDuringBloodmoon").GetChildByType<XUiC_ComboBoxBool>();
		this.comboAllowVotesDuringQuests = base.GetChildById("AllowVotesDuringQuests").GetChildByType<XUiC_ComboBoxBool>();
		this.comboAllowVotesInSafeZone = base.GetChildById("AllowVotesInSafeZone").GetChildByType<XUiC_ComboBoxBool>();
		this.comboBitPrices = base.GetChildById("BitPrices").GetChildByType<XUiC_ComboBoxList<string>>();
		this.comboBitPotPercent = base.GetChildById("BitPotPercent").GetChildByType<XUiC_ComboBoxList<string>>();
		this.comboViewerStartingPoints = base.GetChildById("ViewerStartingPoints").GetChildByType<XUiC_ComboBoxList<string>>();
		this.comboBitPointsAdd = base.GetChildById("BitPointsAdd").GetChildByType<XUiC_ComboBoxEnum<XUiC_OptionsTwitch.BitAddOptions>>();
		this.comboSubPointsAdd = base.GetChildById("SubPointsAdd").GetChildByType<XUiC_ComboBoxEnum<XUiC_OptionsTwitch.PointsGenerationOptions>>();
		this.comboGiftSubPointsAdd = base.GetChildById("GiftSubPointsAdd").GetChildByType<XUiC_ComboBoxEnum<XUiC_OptionsTwitch.PointsGenerationOptions>>();
		this.comboRaidPointsAdd = base.GetChildById("RaidPointsAdd").GetChildByType<XUiC_ComboBoxList<string>>();
		this.comboRaidViewerMinimum = base.GetChildById("RaidViewerMinimum").GetChildByType<XUiC_ComboBoxList<string>>();
		this.comboHelperRewardAmount = base.GetChildById("HelperRewardAmount").GetChildByType<XUiC_ComboBoxList<string>>();
		this.comboHelperRewardTime = base.GetChildById("HelperRewardTime").GetChildByType<XUiC_ComboBoxList<string>>();
		this.comboMaxDailyVotes = base.GetChildById("MaxDailyVotes").GetChildByType<XUiC_ComboBoxList<string>>();
		this.comboVoteTime = base.GetChildById("VoteTime").GetChildByType<XUiC_ComboBoxList<string>>();
		this.comboVoteDayTimeRange = base.GetChildById("VoteDayTimeRange").GetChildByType<XUiC_ComboBoxList<string>>();
		this.comboVoteViewerDefeatReward = base.GetChildById("ViewerDefeatReward").GetChildByType<XUiC_ComboBoxList<string>>();
		this.comboAllowBitEvents = base.GetChildById("AllowBitEvents").GetChildByType<XUiC_ComboBoxBool>();
		this.comboAllowSubEvents = base.GetChildById("AllowSubEvents").GetChildByType<XUiC_ComboBoxBool>();
		this.comboAllowGiftSubEvents = base.GetChildById("AllowGiftSubEvents").GetChildByType<XUiC_ComboBoxBool>();
		this.comboAllowCharityEvents = base.GetChildById("AllowCharityEvents").GetChildByType<XUiC_ComboBoxBool>();
		this.comboAllowRaidEvents = base.GetChildById("AllowRaidEvents").GetChildByType<XUiC_ComboBoxBool>();
		this.comboAllowHypeTrainEvents = base.GetChildById("AllowHypeTrainEvents").GetChildByType<XUiC_ComboBoxBool>();
		this.comboAllowCreatorGoalEvents = base.GetChildById("AllowCreatorGoalEvents").GetChildByType<XUiC_ComboBoxBool>();
		this.comboAllowChannelPointRedeemEvents = base.GetChildById("AllowChannelPointRedeemEvents").GetChildByType<XUiC_ComboBoxBool>();
		this.comboAllowActionsDuringBloodmoon.OnValueChangedGeneric += this.Combo_OnValueChangedGeneric;
		this.comboAllowActionsDuringQuests.OnValueChangedGeneric += this.Combo_OnValueChangedGeneric;
		this.comboPointsGeneration.OnValueChangedGeneric += this.Combo_OnValueChangedGeneric;
		this.comboUseProgression.OnValueChangedGeneric += this.Combo_OnValueChangedGeneric;
		this.comboOptOutTwitch.OnValueChangedGeneric += this.Combo_OnValueChangedGeneric;
		this.comboAllowVisionEffects.OnValueChangedGeneric += this.Combo_OnValueChangedGeneric;
		this.comboAllowCrateSharing.OnValueChangedGeneric += this.Combo_OnValueChangedGeneric;
		this.comboActionTwitchIntegrationType.OnValueChangedGeneric += this.Combo_OnValueChangedGeneric;
		this.comboAllowActions.OnValueChanged += this.ComboPreset_OnValueChanged;
		this.comboAllowVotes.OnValueChanged += this.ComboPreset_OnValueChanged;
		this.comboAllowEvents.OnValueChanged += this.ComboPreset_OnValueChanged;
		this.comboAllowVotesDuringBloodmoon.OnValueChangedGeneric += this.Combo_OnValueChangedGeneric;
		this.comboAllowVotesDuringQuests.OnValueChangedGeneric += this.Combo_OnValueChangedGeneric;
		this.comboAllowVotesInSafeZone.OnValueChangedGeneric += this.Combo_OnValueChangedGeneric;
		this.comboBitPointsAdd.OnValueChangedGeneric += this.Combo_OnValueChangedGeneric;
		this.comboSubPointsAdd.OnValueChanged += this.SubPoints_OnValueChanged;
		this.comboGiftSubPointsAdd.OnValueChanged += this.GiftSubPoints_OnValueChanged;
		this.comboAllowBitEvents.OnValueChangedGeneric += this.Combo_OnValueChangedGeneric;
		this.comboAllowSubEvents.OnValueChangedGeneric += this.Combo_OnValueChangedGeneric;
		this.comboAllowGiftSubEvents.OnValueChangedGeneric += this.Combo_OnValueChangedGeneric;
		this.comboAllowCharityEvents.OnValueChangedGeneric += this.Combo_OnValueChangedGeneric;
		this.comboAllowRaidEvents.OnValueChangedGeneric += this.Combo_OnValueChangedGeneric;
		this.comboAllowHypeTrainEvents.OnValueChangedGeneric += this.Combo_OnValueChangedGeneric;
		this.comboAllowCreatorGoalEvents.OnValueChangedGeneric += this.Combo_OnValueChangedGeneric;
		this.comboAllowChannelPointRedeemEvents.OnValueChangedGeneric += this.Combo_OnValueChangedGeneric;
		this.comboBitPrices.OnValueChangedGeneric += this.Combo_OnValueChangedGeneric;
		this.comboBitPotPercent.OnValueChangedGeneric += this.Combo_OnValueChangedGeneric;
		XUiC_Button xuiC_Button = base.GetChildById("btnLoginTwitch") as XUiC_Button;
		if (xuiC_Button != null)
		{
			xuiC_Button.OnPress += this.BtnLoginTwitch_OnPressed;
		}
		XUiC_Button xuiC_Button2 = base.GetChildById("btnShowExtras") as XUiC_Button;
		if (xuiC_Button2 != null)
		{
			xuiC_Button2.OnPress += this.BtnShowExtras_OnPressed;
		}
		XUiC_Button xuiC_Button3 = base.GetChildById("btnEnableAllExtras") as XUiC_Button;
		if (xuiC_Button3 != null)
		{
			xuiC_Button3.OnPress += this.BtnEnableAllExtras_OnPressed;
		}
		XUiC_Button xuiC_Button4 = base.GetChildById("btnDisableAllExtras") as XUiC_Button;
		if (xuiC_Button4 != null)
		{
			xuiC_Button4.OnPress += this.BtnDisableAllExtras_OnPressed;
		}
		XUiC_Button xuiC_Button5 = base.GetChildById("btnResetPrices") as XUiC_Button;
		if (xuiC_Button5 != null)
		{
			xuiC_Button5.OnPress += this.BtnResetPrices_OnPressed;
		}
		XUiC_Button xuiC_Button6 = base.GetChildById("btnShowDeviceCode") as XUiC_Button;
		if (xuiC_Button6 != null)
		{
			xuiC_Button6.OnPress += this.BtnShowDeviceCode_OnPressed;
		}
		this.qrCodeTexControl = base.GetChildById("qrCodeTex").ViewComponent as XUiV_Texture;
		TwitchAuthentication.QRCodeGenerated += this.TwitchAuthentication_QRCodeGenerated;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnShowDeviceCode_OnPressed(XUiController _sender, int _mouseButton)
	{
		this.showDeviceCode = !this.showDeviceCode;
		base.RefreshBindings();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Combo_OnValueChangedGeneric(XUiController _sender)
	{
		base.SetChanged();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Tabs_OnTabChanged(int _i, XUiC_TabSelectorTab _tab)
	{
		if (_i != 0 && this.twitchManager.InitState != TwitchManager.InitStates.Ready)
		{
			this.tabs.SelectedTabIndex = 0;
		}
		this.IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ComboBoxListString_OnValueChanged(XUiController _sender, string _oldValue, string _newValue)
	{
		base.SetChanged();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ComboPreset_OnValueChanged(XUiController _sender, string _oldValue, string _newValue)
	{
		base.SetChanged();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ComboVoteDayTimeRange_OnValueChanged(XUiController _sender, string _oldValue, string _newValue)
	{
		base.SetChanged();
		this.tempVoteDayTimeRange = this.comboVoteDayTimeRange.SelectedIndex;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SubPoints_OnValueChanged(XUiController _sender, XUiC_OptionsTwitch.PointsGenerationOptions _oldValue, XUiC_OptionsTwitch.PointsGenerationOptions _newValue)
	{
		base.SetChanged();
		this.tempSubModifier = (int)_newValue;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void GiftSubPoints_OnValueChanged(XUiController _sender, XUiC_OptionsTwitch.PointsGenerationOptions _oldValue, XUiC_OptionsTwitch.PointsGenerationOptions _newValue)
	{
		base.SetChanged();
		this.IsDirty = true;
		this.tempGiftSubModifier = (int)_newValue;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnLoginTwitch_OnPressed(XUiController _sender, int _mouseButton)
	{
		switch (this.twitchManager.InitState)
		{
		case TwitchManager.InitStates.None:
		case TwitchManager.InitStates.PermissionDenied:
		case TwitchManager.InitStates.WaitingForOAuth:
		case TwitchManager.InitStates.ExtensionNotInstalled:
		case TwitchManager.InitStates.Failed:
			this.startedSinceOpened = true;
			this.showDeviceCode = false;
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				this.twitchManager.StartTwitchIntegration();
			}
			else
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageTwitchAccess>().Setup(), false);
				this.twitchManager.WaitForPermission();
			}
			if (!this.localPlayer.PlayerUI.windowManager.IsWindowOpen("twitch"))
			{
				this.localPlayer.PlayerUI.windowManager.Open("twitch", false);
			}
			this.origAllowTwitchOptions = true;
			this.localPlayer.TwitchActionsEnabled = EntityPlayer.TwitchActionsStates.Enabled;
			return;
		case TwitchManager.InitStates.WaitingForPermission:
		case TwitchManager.InitStates.Authenticating:
		case TwitchManager.InitStates.Authenticated:
		case TwitchManager.InitStates.CheckingForExtension:
			break;
		case TwitchManager.InitStates.Ready:
			this.twitchManager.StopTwitchIntegration(TwitchManager.InitStates.None);
			break;
		default:
			return;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnShowExtras_OnPressed(XUiController _sender, int _mouseButton)
	{
		this.applyChanges();
		this.xui.playerUI.windowManager.Close(this.windowGroup, false);
		if (GameManager.Instance.IsPaused() && GameStats.GetInt(EnumGameStats.GameState) == 2)
		{
			GameManager.Instance.Pause(false);
		}
		XUiC_TwitchWindowSelector.OpenSelectorAndWindow(GameManager.Instance.World.GetPrimaryPlayer(), "Actions", true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnEnableAllExtras_OnPressed(XUiController _sender, int _mouseButton)
	{
		List<TwitchAction> list = (from entry in TwitchActionManager.TwitchActions.Values.ToList<TwitchAction>()
			where entry.DisplayCategory.Name == "Extras"
			orderby entry.Title
			select entry).ToList<TwitchAction>();
		TwitchActionPreset currentActionPreset = TwitchManager.Current.CurrentActionPreset;
		foreach (TwitchAction twitchAction in list)
		{
			if (!currentActionPreset.AddedActions.Contains(twitchAction.Name))
			{
				currentActionPreset.AddedActions.Add(twitchAction.Name);
				QuestEventManager.Current.TwitchEventReceived(TwitchObjectiveTypes.EnableExtras, twitchAction.DisplayCategory.Name);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnDisableAllExtras_OnPressed(XUiController _sender, int _mouseButton)
	{
		List<TwitchAction> list = (from entry in TwitchActionManager.TwitchActions.Values.ToList<TwitchAction>()
			where entry.DisplayCategory.Name == "Extras"
			orderby entry.Title
			select entry).ToList<TwitchAction>();
		TwitchActionPreset currentActionPreset = TwitchManager.Current.CurrentActionPreset;
		foreach (TwitchAction twitchAction in list)
		{
			if (currentActionPreset.AddedActions.Contains(twitchAction.Name))
			{
				currentActionPreset.AddedActions.Remove(twitchAction.Name);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnResetPrices_OnPressed(XUiController _sender, int _mouseButton)
	{
		this.twitchManager.ResetPricesToDefault();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void doSaveChangesInternal()
	{
		base.doSaveChangesInternal();
		this.applyChanges();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void doResetToDefaultsInternal()
	{
		base.doResetToDefaultsInternal();
		this.twitchManager.UseActionsDuringBloodmoon = 2;
		this.twitchManager.UseActionsDuringQuests = 2;
		this.twitchManager.ViewerData.PointRate = 1f;
		this.twitchManager.SetToDefaultCooldown();
		this.twitchManager.SetUseProgression(true);
		this.twitchManager.AllowCrateSharing = false;
		this.twitchManager.ViewerData.ActionSpamDelay = 3f;
		TwitchVotingManager votingManager = this.twitchManager.VotingManager;
		this.twitchManager.SetToDefaultActionPreset();
		this.twitchManager.SetToDefaultVotePreset();
		this.twitchManager.SetToDefaultEventPreset();
		this.twitchManager.ViewerData.StartingPoints = 100;
		this.twitchManager.BitPointModifier = 1;
		this.twitchManager.SubPointModifier = 1;
		this.twitchManager.GiftSubPointModifier = 2;
		this.twitchManager.RaidPointAdd = 1000;
		this.twitchManager.RaidViewerMinimum = 10;
		this.twitchManager.BitPotPercentage = 0.25f;
		TwitchManager.LeaderboardStats.GoodRewardAmount = 1000;
		TwitchManager.LeaderboardStats.GoodRewardTime = 15;
		this.localPlayer.TwitchActionsEnabled = EntityPlayer.TwitchActionsStates.Enabled;
		votingManager.MaxDailyVotes = 4;
		votingManager.VoteTime = 60f;
		votingManager.CurrentVoteDayTimeRange = 2;
		votingManager.ViewerDefeatReward = 250;
		votingManager.AllowVotesDuringBloodmoon = false;
		votingManager.AllowVotesDuringQuests = false;
		votingManager.AllowVotesInSafeZone = false;
		this.updateOptions();
		this.applyChanges();
		base.SetChanged();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TwitchAuthentication_QRCodeGenerated(Texture2D _qrCodeTex, string _userCode, string _verificationUrl)
	{
		this.qrCodeTexControl.Texture = _qrCodeTex;
		this.authCode = _userCode;
		this.authVerificationUrl = _verificationUrl;
		base.RefreshBindings();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateOptions()
	{
		if (GameStats.GetBool(EnumGameStats.TwitchBloodMoonAllowed))
		{
			this.comboAllowActionsDuringBloodmoon.Value = (this.origAllowActionsDuringBloodmoon = (XUiC_OptionsTwitch.TwitchBloodMoonOptions)TwitchManager.Current.UseActionsDuringBloodmoon);
			this.comboAllowActionsDuringBloodmoon.Enabled = true;
		}
		else
		{
			this.comboAllowActionsDuringBloodmoon.Value = XUiC_OptionsTwitch.TwitchBloodMoonOptions.Disabled;
			this.comboAllowActionsDuringBloodmoon.Enabled = false;
			this.twitchManager.UseActionsDuringBloodmoon = 0;
		}
		this.comboAllowActionsDuringQuests.Value = (this.origAllowActionsDuringQuests = (XUiC_OptionsTwitch.TwitchBloodMoonOptions)TwitchManager.Current.UseActionsDuringQuests);
		this.comboPointsGeneration.Value = (this.origPointsGeneration = (XUiC_OptionsTwitch.PointsGenerationOptions)this.twitchManager.ViewerData.PointRate);
		this.comboCooldownPreset.SelectedIndex = (this.origCooldownPresetIndex = this.twitchManager.CooldownPresetIndex);
		this.comboUseProgression.Value = (this.origUseProgression = this.twitchManager.UseProgression);
		this.comboAllowVisionEffects.Value = (this.origAllowVisionEffects = !this.localPlayer.TwitchVisionDisabled);
		this.comboOptOutTwitch.Value = (this.origAllowTwitchOptions = this.localPlayer.TwitchActionsEnabled > EntityPlayer.TwitchActionsStates.Disabled);
		this.comboAllowCrateSharing.Value = (this.origAllowCrateSharing = this.twitchManager.AllowCrateSharing);
		this.comboActionTwitchIntegrationType.Value = (this.origIntegrationSetting = this.twitchManager.IntegrationSetting);
		TwitchVotingManager votingManager = this.twitchManager.VotingManager;
		this.comboAllowActions.SelectedIndex = (this.origActionPresetIndex = this.twitchManager.ActionPresetIndex);
		this.comboAllowVotes.SelectedIndex = (this.origVotePresetIndex = this.twitchManager.VotePresetIndex);
		this.comboAllowEvents.SelectedIndex = (this.origEventPresetIndex = this.twitchManager.EventPresetIndex);
		this.comboBitPrices.SelectedIndex = (this.origBitPricesIndex = this.getBitPriceModifier(this.twitchManager.BitPriceMultiplier));
		this.comboBitPotPercent.SelectedIndex = (this.origBitPotPercentIndex = Mathf.FloorToInt((1f - this.twitchManager.BitPotPercentage) / 0.05f));
		this.comboViewerStartingPoints.SelectedIndex = (this.origViewerStartingPoints = this.getIndexFromCombobox(this.twitchManager.ViewerData.StartingPoints, this.comboViewerStartingPoints));
		this.comboBitPointsAdd.Value = (this.origBitPointModifier = (XUiC_OptionsTwitch.BitAddOptions)this.twitchManager.BitPointModifier);
		this.comboSubPointsAdd.Value = (this.origSubPointModifier = (XUiC_OptionsTwitch.PointsGenerationOptions)this.twitchManager.SubPointModifier);
		this.comboGiftSubPointsAdd.Value = (this.origGiftSubPointModifier = (XUiC_OptionsTwitch.PointsGenerationOptions)this.twitchManager.GiftSubPointModifier);
		this.comboRaidPointsAdd.SelectedIndex = (this.origRaidPointAmountIndex = this.getIndexFromCombobox(this.twitchManager.RaidPointAdd, this.comboRaidPointsAdd));
		this.comboRaidViewerMinimum.SelectedIndex = (this.origRaidViewerMinimumIndex = this.getIndexFromCombobox(this.twitchManager.RaidViewerMinimum, this.comboRaidViewerMinimum));
		this.comboHelperRewardAmount.SelectedIndex = (this.origHelperRewardAmountIndex = this.getIndexFromCombobox(TwitchManager.LeaderboardStats.GoodRewardAmount, this.comboHelperRewardAmount));
		this.comboHelperRewardTime.SelectedIndex = (this.origHelperRewardTimeIndex = this.getIndexFromCombobox(TwitchManager.LeaderboardStats.GoodRewardTime, this.comboHelperRewardTime));
		this.comboMaxDailyVotes.SelectedIndex = (this.origMaxDailyVotes = this.getIndexFromCombobox(votingManager.MaxDailyVotes, this.comboMaxDailyVotes));
		this.comboVoteTime.SelectedIndex = (this.origVoteTime = this.getIndexFromCombobox((int)votingManager.VoteTime, this.comboVoteTime));
		this.comboVoteDayTimeRange.SelectedIndex = (this.origVoteDayTimeRange = votingManager.CurrentVoteDayTimeRange);
		this.comboVoteViewerDefeatReward.SelectedIndex = (this.origVoteViewerDefeatReward = this.getIndexFromCombobox(votingManager.ViewerDefeatReward, this.comboVoteViewerDefeatReward));
		this.comboActionSpamDelay.SelectedIndex = (this.origActionSpamDelay = this.getActionSpamDelay(this.twitchManager.ViewerData.ActionSpamDelay));
		this.comboAllowVotesDuringBloodmoon.Value = (this.origAllowVotesDuringBloodmoon = votingManager.AllowVotesDuringBloodmoon);
		this.comboAllowVotesDuringQuests.Value = (this.origAllowVotesDuringQuests = votingManager.AllowVotesDuringQuests);
		this.comboAllowVotesInSafeZone.Value = (this.origAllowVotesInSafeZone = votingManager.AllowVotesInSafeZone);
		this.comboAllowBitEvents.Value = (this.origAllowBitEvents = this.twitchManager.AllowBitEvents);
		this.comboAllowSubEvents.Value = (this.origAllowSubEvents = this.twitchManager.AllowSubEvents);
		this.comboAllowGiftSubEvents.Value = (this.origAllowGiftSubEvents = this.twitchManager.AllowGiftSubEvents);
		this.comboAllowCharityEvents.Value = (this.origAllowCharityEvents = this.twitchManager.AllowCharityEvents);
		this.comboAllowRaidEvents.Value = (this.origAllowRaidEvents = this.twitchManager.AllowRaidEvents);
		this.comboAllowHypeTrainEvents.Value = (this.origAllowHypeTrainEvents = this.twitchManager.AllowHypeTrainEvents);
		this.comboAllowCreatorGoalEvents.Value = (this.origAllowCreatorGoalEvents = this.twitchManager.AllowCreatorGoalEvents);
		this.comboAllowChannelPointRedeemEvents.Value = (this.origAllowChannelPointRedemptions = this.twitchManager.AllowChannelPointRedemptions);
		this.tempSubModifier = (int)this.origSubPointModifier;
		this.tempGiftSubModifier = (int)this.origGiftSubPointModifier;
		this.tempVoteDayTimeRange = this.origVoteDayTimeRange;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int getIndexFromCombobox(int _value, XUiC_ComboBoxList<string> _cboList)
	{
		string text = _value.ToString();
		for (int i = 0; i < _cboList.Elements.Count; i++)
		{
			if (_cboList.Elements[i] == text)
			{
				return i;
			}
		}
		return -1;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int getActionSpamDelay(float _actionSpamDelay)
	{
		string text = ((int)_actionSpamDelay).ToString();
		for (int i = 0; i < this.comboActionSpamDelay.Elements.Count; i++)
		{
			if (i == 0)
			{
				if (text == "0")
				{
					return i;
				}
			}
			else if (this.comboActionSpamDelay.Elements[i] == text)
			{
				return i;
			}
		}
		return -1;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int getBitPriceModifier(float _bitPriceModifier)
	{
		if (_bitPriceModifier <= 0.5f)
		{
			if (_bitPriceModifier == 0.25f)
			{
				return 3;
			}
			if (_bitPriceModifier == 0.5f)
			{
				return 2;
			}
		}
		else
		{
			if (_bitPriceModifier == 0.75f)
			{
				return 1;
			}
			if (_bitPriceModifier == 1f)
			{
				return 0;
			}
		}
		return 0;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public float setBitPriceModifier(int _index)
	{
		float num;
		switch (_index)
		{
		case 0:
			num = 1f;
			break;
		case 1:
			num = 0.75f;
			break;
		case 2:
			num = 0.5f;
			break;
		case 3:
			num = 0.25f;
			break;
		default:
			num = 1f;
			break;
		}
		return num;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void applyChanges()
	{
		this.origAllowActionsDuringBloodmoon = this.comboAllowActionsDuringBloodmoon.Value;
		this.origAllowActionsDuringQuests = this.comboAllowActionsDuringQuests.Value;
		this.origPointsGeneration = this.comboPointsGeneration.Value;
		this.origCooldownPresetIndex = this.comboCooldownPreset.SelectedIndex;
		this.origUseProgression = this.comboUseProgression.Value;
		this.origAllowVisionEffects = this.comboAllowVisionEffects.Value;
		this.origAllowTwitchOptions = this.comboOptOutTwitch.Value;
		this.origAllowCrateSharing = this.comboAllowCrateSharing.Value;
		this.origActionSpamDelay = this.comboActionSpamDelay.SelectedIndex;
		this.origIntegrationSetting = this.comboActionTwitchIntegrationType.Value;
		this.origActionPresetIndex = this.comboAllowActions.SelectedIndex;
		this.origVotePresetIndex = this.comboAllowVotes.SelectedIndex;
		this.origEventPresetIndex = this.comboAllowEvents.SelectedIndex;
		this.origViewerStartingPoints = this.comboViewerStartingPoints.SelectedIndex;
		this.origBitPointModifier = this.comboBitPointsAdd.Value;
		this.origSubPointModifier = this.comboSubPointsAdd.Value;
		this.origGiftSubPointModifier = this.comboGiftSubPointsAdd.Value;
		this.origRaidPointAmountIndex = this.comboRaidPointsAdd.SelectedIndex;
		this.origRaidViewerMinimumIndex = this.comboRaidViewerMinimum.SelectedIndex;
		this.origHelperRewardAmountIndex = this.comboHelperRewardAmount.SelectedIndex;
		this.origHelperRewardTimeIndex = this.comboHelperRewardTime.SelectedIndex;
		this.origBitPricesIndex = this.comboBitPrices.SelectedIndex;
		this.origBitPotPercentIndex = this.comboBitPotPercent.SelectedIndex;
		this.origMaxDailyVotes = this.comboMaxDailyVotes.SelectedIndex;
		this.origVoteTime = this.comboVoteTime.SelectedIndex;
		this.origVoteDayTimeRange = this.comboVoteDayTimeRange.SelectedIndex;
		this.origVoteViewerDefeatReward = this.comboVoteViewerDefeatReward.SelectedIndex;
		this.origAllowVotesDuringBloodmoon = this.comboAllowVotesDuringBloodmoon.Value;
		this.origAllowVotesDuringQuests = this.comboAllowVotesDuringQuests.Value;
		this.origAllowVotesInSafeZone = this.comboAllowVotesInSafeZone.Value;
		this.origAllowBitEvents = this.comboAllowBitEvents.Value;
		this.origAllowSubEvents = this.comboAllowSubEvents.Value;
		this.origAllowGiftSubEvents = this.comboAllowGiftSubEvents.Value;
		this.origAllowCharityEvents = this.comboAllowCharityEvents.Value;
		this.origAllowRaidEvents = this.comboAllowRaidEvents.Value;
		this.origAllowHypeTrainEvents = this.comboAllowHypeTrainEvents.Value;
		this.origAllowCreatorGoalEvents = this.comboAllowCreatorGoalEvents.Value;
		this.origAllowChannelPointRedemptions = this.comboAllowChannelPointRedeemEvents.Value;
		Action onSettingsChanged = XUiC_OptionsTwitch.OnSettingsChanged;
		if (onSettingsChanged == null)
		{
			return;
		}
		onSettingsChanged();
	}

	public override void OnOpen()
	{
		this.twitchManager = TwitchManager.Current;
		this.twitchManager.ConnectionStateChanged -= this.TwitchManager_ConnectionStateChanged;
		this.twitchManager.ConnectionStateChanged += this.TwitchManager_ConnectionStateChanged;
		this.comboCooldownPreset.OnValueChanged -= this.ComboBoxListString_OnValueChanged;
		this.comboCooldownPreset.Elements.Clear();
		for (int i = 0; i < this.twitchManager.CooldownPresets.Count; i++)
		{
			this.comboCooldownPreset.Elements.Add(this.twitchManager.CooldownPresets[i].Title);
		}
		if (this.twitchManager.CooldownPresetIndex >= this.comboCooldownPreset.Elements.Count)
		{
			this.twitchManager.SetToDefaultCooldown();
		}
		this.comboCooldownPreset.Value = this.comboCooldownPreset.Elements[this.twitchManager.CooldownPresetIndex];
		this.comboCooldownPreset.OnValueChanged += this.ComboBoxListString_OnValueChanged;
		this.localPlayer = GameManager.Instance.World.GetPrimaryPlayer();
		this.comboAllowActions.OnValueChanged -= this.ComboPreset_OnValueChanged;
		this.comboAllowActions.Elements.Clear();
		for (int j = 0; j < this.twitchManager.ActionPresets.Count; j++)
		{
			TwitchActionPreset twitchActionPreset = this.twitchManager.ActionPresets[j];
			if (twitchActionPreset.IsEnabled)
			{
				this.comboAllowActions.Elements.Add(twitchActionPreset.Title);
			}
		}
		if (this.twitchManager.ActionPresetIndex >= this.comboAllowActions.Elements.Count)
		{
			this.twitchManager.SetToDefaultActionPreset();
		}
		this.comboAllowActions.Value = this.comboAllowActions.Elements[this.twitchManager.ActionPresetIndex];
		this.comboAllowActions.OnValueChanged += this.ComboPreset_OnValueChanged;
		this.comboAllowVotes.OnValueChanged -= this.ComboPreset_OnValueChanged;
		this.comboAllowVotes.Elements.Clear();
		for (int k = 0; k < this.twitchManager.VotePresets.Count; k++)
		{
			this.comboAllowVotes.Elements.Add(this.twitchManager.VotePresets[k].Title);
		}
		if (this.twitchManager.VotePresetIndex >= this.comboAllowVotes.Elements.Count)
		{
			this.twitchManager.SetToDefaultVotePreset();
		}
		this.comboAllowVotes.Value = this.comboAllowVotes.Elements[this.twitchManager.VotePresetIndex];
		this.comboAllowVotes.OnValueChanged += this.ComboPreset_OnValueChanged;
		this.comboAllowEvents.OnValueChanged -= this.ComboPreset_OnValueChanged;
		this.comboAllowEvents.Elements.Clear();
		for (int l = 0; l < this.twitchManager.EventPresets.Count; l++)
		{
			this.comboAllowEvents.Elements.Add(this.twitchManager.EventPresets[l].Title);
		}
		if (this.twitchManager.EventPresetIndex >= this.comboAllowEvents.Elements.Count)
		{
			this.twitchManager.SetToDefaultEventPreset();
		}
		this.comboAllowEvents.Value = this.comboAllowEvents.Elements[this.twitchManager.EventPresetIndex];
		this.comboAllowEvents.OnValueChanged += this.ComboPreset_OnValueChanged;
		this.setupComboBoxListString(this.comboViewerStartingPoints, new string[] { "0", "50", "100", "150", "200", "250", "500" }, this.twitchManager.ViewerData.StartingPoints, null);
		this.setupComboBoxListString(this.comboRaidPointsAdd, new string[] { "0", "500", "1000", "1500", "2000", "2500" }, this.twitchManager.RaidPointAdd, null);
		this.setupComboBoxListString(this.comboRaidViewerMinimum, new string[] { "1", "3", "5", "10", "15", "20", "25", "50", "100" }, this.twitchManager.RaidViewerMinimum, null);
		this.setupComboBoxListString(this.comboMaxDailyVotes, new string[]
		{
			"1", "2", "3", "4", "5", "6", "7", "8", "9", "10",
			"11", "12"
		}, this.twitchManager.VotingManager.MaxDailyVotes, null);
		this.setupComboBoxListString(this.comboVoteTime, new string[] { "30", "60", "90", "120" }, (int)this.twitchManager.VotingManager.VoteTime, null);
		this.setupComboBoxListString(this.comboHelperRewardAmount, new string[] { "100", "250", "500", "1000", "1250", "1500", "2000" }, TwitchManager.LeaderboardStats.GoodRewardAmount, null);
		this.setupComboBoxListString(this.comboHelperRewardTime, new string[] { "15", "30", "45", "60" }, TwitchManager.LeaderboardStats.GoodRewardTime, null);
		this.comboVoteDayTimeRange.OnValueChanged -= this.ComboVoteDayTimeRange_OnValueChanged;
		this.comboVoteDayTimeRange.Elements.Clear();
		for (int m = 0; m < this.twitchManager.VotingManager.VoteDayTimeRanges.Count; m++)
		{
			this.comboVoteDayTimeRange.Elements.Add(this.twitchManager.VotingManager.VoteDayTimeRanges[m].Name);
		}
		this.comboVoteDayTimeRange.SelectedIndex = this.twitchManager.VotingManager.CurrentVoteDayTimeRange;
		this.comboVoteDayTimeRange.OnValueChanged += this.ComboVoteDayTimeRange_OnValueChanged;
		this.setupComboBoxListString(this.comboVoteViewerDefeatReward, new string[] { "0", "100", "200", "250", "500", "1000", "2500", "5000" }, this.twitchManager.VotingManager.ViewerDefeatReward, null);
		this.comboActionSpamDelay.OnValueChanged -= this.ComboBoxListString_OnValueChanged;
		this.comboActionSpamDelay.Elements.Clear();
		this.comboActionSpamDelay.Elements.AddRange(new string[]
		{
			Localization.Get("xuiLightPropShadowsNone", false, null),
			"1",
			"2",
			"3",
			"4",
			"5"
		});
		this.comboActionSpamDelay.Value = this.comboActionSpamDelay.Elements[this.getActionSpamDelay(this.twitchManager.ViewerData.ActionSpamDelay)];
		this.comboActionSpamDelay.OnValueChanged += this.ComboBoxListString_OnValueChanged;
		this.comboBitPrices.OnValueChanged -= this.ComboBoxListString_OnValueChanged;
		this.comboBitPrices.Elements.Clear();
		this.comboBitPrices.Elements.AddRange(new string[]
		{
			Localization.Get("xuiDefault", false, null),
			"75%",
			"50%",
			"25%"
		});
		this.comboBitPrices.Value = this.comboBitPrices.Elements[this.getBitPriceModifier(this.twitchManager.BitPriceMultiplier)];
		this.comboBitPrices.OnValueChanged += this.ComboBoxListString_OnValueChanged;
		this.comboBitPotPercent.OnValueChanged -= this.ComboBoxListString_OnValueChanged;
		this.comboBitPotPercent.Elements.Clear();
		this.comboBitPotPercent.Elements.AddRange(new string[]
		{
			"100%", "95%", "90%", "85%", "80%", "75%", "70%", "65%", "60%", "55%",
			"50%", "45%", "40%", "35%", "30%", "25%", "20%", "15%", "10%", "0%"
		});
		this.comboBitPotPercent.Value = this.comboBitPotPercent.Elements[Mathf.FloorToInt((1f - this.twitchManager.BitPotPercentage) / 0.05f)];
		this.comboBitPotPercent.OnValueChanged += this.ComboBoxListString_OnValueChanged;
		if (this.tabs != null)
		{
			this.tabs.SelectedTabIndex = 0;
		}
		this.updateOptions();
		base.OnOpen();
		this.startedSinceOpened = false;
		XUi.InGameMenuOpen = true;
		this.IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setupComboBoxListString(XUiC_ComboBoxList<string> _comboBox, string[] _values, int _currentValue, XUiC_ComboBox<string>.XUiEvent_ValueChanged _valueChanged = null)
	{
		if (_valueChanged == null)
		{
			_valueChanged = new XUiC_ComboBox<string>.XUiEvent_ValueChanged(this.ComboBoxListString_OnValueChanged);
		}
		_comboBox.OnValueChanged -= _valueChanged;
		_comboBox.Elements.Clear();
		_comboBox.Elements.AddRange(_values);
		_comboBox.Value = _comboBox.Elements[this.getIndexFromCombobox(_currentValue, _comboBox)];
		_comboBox.OnValueChanged += _valueChanged;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TwitchManager_ConnectionStateChanged(TwitchManager.InitStates _oldState, TwitchManager.InitStates _newState)
	{
		base.RefreshBindings();
		if (_oldState != TwitchManager.InitStates.Ready && _newState == TwitchManager.InitStates.Ready)
		{
			this.localPlayer.TwitchActionsEnabled = EntityPlayer.TwitchActionsStates.Enabled;
			this.comboOptOutTwitch.Value = true;
			this.origAllowTwitchOptions = true;
		}
	}

	public override void OnClose()
	{
		base.OnClose();
		this.twitchManager.UseActionsDuringBloodmoon = (int)this.origAllowActionsDuringBloodmoon;
		this.twitchManager.UseActionsDuringQuests = (int)this.origAllowActionsDuringQuests;
		this.twitchManager.ViewerData.PointRate = (float)this.origPointsGeneration;
		this.twitchManager.BitPriceMultiplier = this.setBitPriceModifier(this.origBitPricesIndex);
		this.twitchManager.BitPotPercentage = 1f - (float)this.origBitPotPercentIndex * 0.05f;
		this.twitchManager.SetCooldownPreset(this.origCooldownPresetIndex);
		this.twitchManager.SetUseProgression(this.origUseProgression);
		this.twitchManager.AllowCrateSharing = this.origAllowCrateSharing;
		this.twitchManager.ViewerData.ActionSpamDelay = (float)this.origActionSpamDelay;
		this.twitchManager.IntegrationSetting = this.origIntegrationSetting;
		TwitchVotingManager votingManager = this.twitchManager.VotingManager;
		bool allowChannelPointRedemptions = this.twitchManager.AllowChannelPointRedemptions;
		this.twitchManager.AllowBitEvents = this.origAllowBitEvents;
		this.twitchManager.AllowSubEvents = this.origAllowSubEvents;
		this.twitchManager.AllowGiftSubEvents = this.origAllowGiftSubEvents;
		this.twitchManager.AllowCharityEvents = this.origAllowCharityEvents;
		this.twitchManager.AllowRaidEvents = this.origAllowRaidEvents;
		this.twitchManager.AllowHypeTrainEvents = this.origAllowHypeTrainEvents;
		this.twitchManager.AllowCreatorGoalEvents = this.origAllowCreatorGoalEvents;
		this.twitchManager.AllowChannelPointRedemptions = this.origAllowChannelPointRedemptions;
		this.twitchManager.SetTwitchActionPreset(this.origActionPresetIndex);
		this.twitchManager.SetTwitchVotePreset(this.origVotePresetIndex);
		this.twitchManager.SetTwitchEventPreset(this.origEventPresetIndex, allowChannelPointRedemptions);
		this.twitchManager.ViewerData.StartingPoints = StringParsers.ParseSInt32(this.comboViewerStartingPoints.Elements[this.origViewerStartingPoints], 0, -1, NumberStyles.Integer);
		this.twitchManager.BitPointModifier = (int)this.origBitPointModifier;
		this.twitchManager.SubPointModifier = (int)this.origSubPointModifier;
		this.twitchManager.GiftSubPointModifier = (int)this.origGiftSubPointModifier;
		this.twitchManager.RaidPointAdd = StringParsers.ParseSInt32(this.comboRaidPointsAdd.Elements[this.origRaidPointAmountIndex], 0, -1, NumberStyles.Integer);
		this.twitchManager.RaidViewerMinimum = StringParsers.ParseSInt32(this.comboRaidViewerMinimum.Elements[this.origRaidViewerMinimumIndex], 0, -1, NumberStyles.Integer);
		TwitchManager.LeaderboardStats.GoodRewardAmount = StringParsers.ParseSInt32(this.comboHelperRewardAmount.Elements[this.origHelperRewardAmountIndex], 0, -1, NumberStyles.Integer);
		TwitchManager.LeaderboardStats.GoodRewardTime = StringParsers.ParseSInt32(this.comboHelperRewardTime.Elements[this.origHelperRewardTimeIndex], 0, -1, NumberStyles.Integer);
		votingManager.MaxDailyVotes = StringParsers.ParseSInt32(this.comboMaxDailyVotes.Elements[this.origMaxDailyVotes], 0, -1, NumberStyles.Integer);
		votingManager.VoteTime = StringParsers.ParseFloat(this.comboVoteTime.Elements[this.origVoteTime], 0, -1, NumberStyles.Any);
		votingManager.CurrentVoteDayTimeRange = this.origVoteDayTimeRange;
		votingManager.ViewerDefeatReward = StringParsers.ParseSInt32(this.comboVoteViewerDefeatReward.Elements[this.origVoteViewerDefeatReward], 0, -1, NumberStyles.Integer);
		votingManager.AllowVotesDuringBloodmoon = this.origAllowVotesDuringBloodmoon;
		votingManager.AllowVotesDuringQuests = this.origAllowVotesDuringQuests;
		votingManager.AllowVotesInSafeZone = this.origAllowVotesInSafeZone;
		this.twitchManager.HasDataChanges = true;
		if (this.origAllowTwitchOptions)
		{
			if (this.localPlayer.TwitchActionsEnabled == EntityPlayer.TwitchActionsStates.Disabled)
			{
				this.localPlayer.TwitchActionsEnabled = EntityPlayer.TwitchActionsStates.Enabled;
			}
		}
		else if (this.localPlayer.TwitchActionsEnabled != EntityPlayer.TwitchActionsStates.Disabled)
		{
			this.localPlayer.TwitchActionsEnabled = EntityPlayer.TwitchActionsStates.Disabled;
		}
		this.localPlayer.TwitchVisionDisabled = !this.origAllowVisionEffects;
		this.twitchManager.ConnectionStateChanged -= this.TwitchManager_ConnectionStateChanged;
		if (this.twitchManager.CurrentCooldownPreset == null)
		{
			this.twitchManager.GetCooldownMax();
		}
		if (this.startedSinceOpened)
		{
			if (this.twitchManager.CooldownType == TwitchManager.CooldownTypes.Startup && this.twitchManager.CurrentCooldownPreset.StartCooldownTime > 0)
			{
				this.twitchManager.SetCooldown((float)this.twitchManager.CurrentCooldownPreset.StartCooldownTime, TwitchManager.CooldownTypes.Startup, false, false);
			}
			this.twitchManager.InitialCooldownSet = true;
		}
		if (this.twitchManager.InitState == TwitchManager.InitStates.Ready && this.twitchManager.CurrentCooldownPreset.CooldownType != CooldownPreset.CooldownTypes.Fill)
		{
			this.twitchManager.SetCooldown(0f, TwitchManager.CooldownTypes.None, false, false);
		}
		this.twitchManager.ExtensionCheckTime = 0f;
		this.twitchManager = null;
		this.localPlayer = null;
		XUi.InGameMenuOpen = false;
	}

	public TwitchActionPreset ActionPreset
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this.twitchManager.ActionPresets[this.comboAllowActions.SelectedIndex];
		}
	}

	public TwitchVotePreset VotePreset
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this.twitchManager.VotePresets[this.comboAllowVotes.SelectedIndex];
		}
	}

	public TwitchEventPreset EventPreset
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this.twitchManager.EventPresets[this.comboAllowEvents.SelectedIndex];
		}
	}

	[XuiXmlBinding("twitchstatus")]
	public string TwitchStatus
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			TwitchManager twitchManager = this.twitchManager;
			return ((twitchManager != null) ? twitchManager.StateText : null) ?? "";
		}
	}

	[XuiXmlBinding("twitchbuttontext")]
	public string TwitchButtonText
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (this.twitchManager == null)
			{
				return "";
			}
			if (this.twitchManager.InitState != TwitchManager.InitStates.Ready)
			{
				return Localization.Get("xuiOptionsTwitchLoginTwitch", false, null);
			}
			return Localization.Get("xuiOptionsTwitchDisconnect", false, null);
		}
	}

	[XuiXmlBinding("onlyconnected")]
	public bool OnlyConnected
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			TwitchManager twitchManager = this.twitchManager;
			return twitchManager != null && twitchManager.InitState == TwitchManager.InitStates.Ready;
		}
	}

	[XuiXmlBinding("notconnected")]
	public bool NotConnected
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			TwitchManager twitchManager = this.twitchManager;
			return twitchManager == null || twitchManager.InitState != TwitchManager.InitStates.Ready;
		}
	}

	[XuiXmlBinding("notconnecting_console")]
	public bool NotConnectingConsole
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			TwitchManager twitchManager = this.twitchManager;
			return twitchManager == null || twitchManager.InitState != TwitchManager.InitStates.WaitingForOAuth || (DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX).IsCurrent();
		}
	}

	[XuiXmlBinding("connecting_console")]
	public bool ConnectingConsole
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			TwitchManager twitchManager = this.twitchManager;
			return twitchManager != null && twitchManager.InitState == TwitchManager.InitStates.WaitingForOAuth && !(DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX).IsCurrent();
		}
	}

	[XuiXmlBinding("connecting_standalone")]
	public bool ConnectingStandalone
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			TwitchManager twitchManager = this.twitchManager;
			return twitchManager != null && twitchManager.InitState == TwitchManager.InitStates.WaitingForOAuth && (DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX).IsCurrent();
		}
	}

	[XuiXmlBinding("auth_devicecode")]
	public string AuthDeviceCode
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this.authCode ?? "";
		}
	}

	[XuiXmlBinding("auth_verificationUrl")]
	public string AuthVerificationUrl
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this.authVerificationUrl ?? "";
		}
	}

	[XuiXmlBinding("show_devicecode")]
	public string ShowDeviceCode
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (!this.showDeviceCode)
			{
				return Localization.Get("xuiOptionsTwitchDeviceCodeShow", false, null);
			}
			return this.authCode;
		}
	}

	[XuiXmlBinding("hascustomevents")]
	public bool HasCustomEvents
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			TwitchManager twitchManager = this.twitchManager;
			return twitchManager != null && twitchManager.InitState == TwitchManager.InitStates.Ready && this.twitchManager.HasCustomEvents;
		}
	}

	[XuiXmlBinding("allowactions")]
	public bool AllowActions
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			TwitchManager twitchManager = this.twitchManager;
			return twitchManager != null && twitchManager.InitState == TwitchManager.InitStates.Ready && !this.ActionPreset.IsEmpty;
		}
	}

	[XuiXmlBinding("allowvotes")]
	public bool AllowVotes
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			TwitchManager twitchManager = this.twitchManager;
			return twitchManager != null && twitchManager.InitState == TwitchManager.InitStates.Ready && !this.VotePreset.IsEmpty;
		}
	}

	[XuiXmlBinding("allowevents")]
	public bool AllowEvents
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			TwitchManager twitchManager = this.twitchManager;
			return twitchManager != null && twitchManager.InitState == TwitchManager.InitStates.Ready && !this.EventPreset.IsEmpty;
		}
	}

	[XuiXmlBinding("subvalues")]
	public string SubValues
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (this.twitchManager != null)
			{
				return this.twitchManager.GetSubTierRewards(this.tempSubModifier);
			}
			return "";
		}
	}

	[XuiXmlBinding("giftsubvalues")]
	public string GiftSubValues
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (this.twitchManager != null)
			{
				return this.twitchManager.GetGiftSubTierRewards(this.tempGiftSubModifier);
			}
			return "";
		}
	}

	[XuiXmlBinding("votedaytimerange")]
	public string VoteDayTimeRange
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (this.twitchManager != null)
			{
				return this.twitchManager.VotingManager.GetDayTimeRange(this.tempVoteDayTimeRange);
			}
			return "";
		}
	}

	[XuiXmlBinding("action_description")]
	public string ActionDescription
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (this.twitchManager != null)
			{
				return "[DECEA3]" + this.ActionPreset.Title + "[-]\n" + this.ActionPreset.Description;
			}
			return "";
		}
	}

	[XuiXmlBinding("vote_description")]
	public string VoteDescription
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (this.twitchManager != null)
			{
				return "[DECEA3]" + this.VotePreset.Title + "[-]\n" + this.VotePreset.Description;
			}
			return "";
		}
	}

	[XuiXmlBinding("event_description")]
	public string EventDescription
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (this.twitchManager != null)
			{
				return "[DECEA3]" + this.EventPreset.Title + "[-]\n" + this.EventPreset.Description;
			}
			return "";
		}
	}

	[XuiXmlBinding("hasbitevents")]
	public bool HasBitEvents
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			TwitchManager twitchManager = this.twitchManager;
			return twitchManager != null && twitchManager.InitState == TwitchManager.InitStates.Ready && this.EventPreset.HasBitEvents;
		}
	}

	[XuiXmlBinding("hassubevents")]
	public bool HasSubEvents
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			TwitchManager twitchManager = this.twitchManager;
			return twitchManager != null && twitchManager.InitState == TwitchManager.InitStates.Ready && this.EventPreset.HasSubEvents;
		}
	}

	[XuiXmlBinding("hasgiftsubevents")]
	public bool HasGiftSubEvents
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			TwitchManager twitchManager = this.twitchManager;
			return twitchManager != null && twitchManager.InitState == TwitchManager.InitStates.Ready && this.EventPreset.HasGiftSubEvents;
		}
	}

	[XuiXmlBinding("hascharityevents")]
	public bool HasCharityEvents
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			TwitchManager twitchManager = this.twitchManager;
			return twitchManager != null && twitchManager.InitState == TwitchManager.InitStates.Ready && this.EventPreset.HasCharityEvents;
		}
	}

	[XuiXmlBinding("hasraidevents")]
	public bool HasRaidEvents
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			TwitchManager twitchManager = this.twitchManager;
			return twitchManager != null && twitchManager.InitState == TwitchManager.InitStates.Ready && this.EventPreset.HasRaidEvents;
		}
	}

	[XuiXmlBinding("hashypetrainevents")]
	public bool HasHypeTrainEvents
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			TwitchManager twitchManager = this.twitchManager;
			return twitchManager != null && twitchManager.InitState == TwitchManager.InitStates.Ready && this.EventPreset.HasHypeTrainEvents;
		}
	}

	[XuiXmlBinding("hascreatorgoalevents")]
	public bool HasCreatorGoalEvents
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			TwitchManager twitchManager = this.twitchManager;
			return twitchManager != null && twitchManager.InitState == TwitchManager.InitStates.Ready && this.EventPreset.HasCreatorGoalEvents;
		}
	}

	[XuiXmlBinding("haschannelpointevents")]
	public bool HasChannelPointEvents
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			TwitchManager twitchManager = this.twitchManager;
			return twitchManager != null && twitchManager.InitState == TwitchManager.InitStates.Ready && this.EventPreset.HasChannelPointEvents;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityPlayerLocal localPlayer;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TabSelector tabs;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxBool comboOptOutTwitch;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxBool comboUseProgression;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxBool comboAllowCrateSharing;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxEnum<XUiC_OptionsTwitch.TwitchBloodMoonOptions> comboAllowActionsDuringBloodmoon;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxEnum<XUiC_OptionsTwitch.TwitchBloodMoonOptions> comboAllowActionsDuringQuests;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxBool comboAllowVotesDuringBloodmoon;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxBool comboAllowVotesDuringQuests;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxBool comboAllowVotesInSafeZone;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxEnum<XUiC_OptionsTwitch.PointsGenerationOptions> comboPointsGeneration;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<string> comboCooldownPreset;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxBool comboAllowVisionEffects;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<string> comboActionSpamDelay;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxEnum<TwitchManager.IntegrationSettings> comboActionTwitchIntegrationType;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<string> comboAllowActions;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<string> comboAllowVotes;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<string> comboAllowEvents;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<string> comboViewerStartingPoints;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxEnum<XUiC_OptionsTwitch.BitAddOptions> comboBitPointsAdd;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxEnum<XUiC_OptionsTwitch.PointsGenerationOptions> comboSubPointsAdd;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxEnum<XUiC_OptionsTwitch.PointsGenerationOptions> comboGiftSubPointsAdd;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<string> comboRaidPointsAdd;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<string> comboRaidViewerMinimum;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<string> comboMaxDailyVotes;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<string> comboVoteTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<string> comboVoteViewerDefeatReward;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<string> comboVoteDayTimeRange;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<string> comboHelperRewardAmount;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<string> comboHelperRewardTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxBool comboAllowBitEvents;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxBool comboAllowSubEvents;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxBool comboAllowGiftSubEvents;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxBool comboAllowCharityEvents;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxBool comboAllowRaidEvents;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxBool comboAllowHypeTrainEvents;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxBool comboAllowCreatorGoalEvents;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxBool comboAllowChannelPointRedeemEvents;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<string> comboBitPrices;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<string> comboBitPotPercent;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_OptionsTwitch.TwitchBloodMoonOptions origAllowActionsDuringBloodmoon;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_OptionsTwitch.TwitchBloodMoonOptions origAllowActionsDuringQuests;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_OptionsTwitch.PointsGenerationOptions origPointsGeneration;

	[PublicizedFrom(EAccessModifier.Private)]
	public int origCooldownPresetIndex;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool origUseProgression;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool origAllowTwitchOptions;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool origAllowVisionEffects = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool origAllowCrateSharing;

	[PublicizedFrom(EAccessModifier.Private)]
	public int origActionSpamDelay = 1;

	[PublicizedFrom(EAccessModifier.Private)]
	public TwitchManager.IntegrationSettings origIntegrationSetting;

	[PublicizedFrom(EAccessModifier.Private)]
	public int origActionPresetIndex;

	[PublicizedFrom(EAccessModifier.Private)]
	public int origVotePresetIndex;

	[PublicizedFrom(EAccessModifier.Private)]
	public int origEventPresetIndex;

	[PublicizedFrom(EAccessModifier.Private)]
	public int origBitPricesIndex;

	[PublicizedFrom(EAccessModifier.Private)]
	public int origBitPotPercentIndex;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_OptionsTwitch.BitAddOptions origBitPointModifier;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_OptionsTwitch.PointsGenerationOptions origSubPointModifier;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_OptionsTwitch.PointsGenerationOptions origGiftSubPointModifier;

	[PublicizedFrom(EAccessModifier.Private)]
	public int origRaidPointAmountIndex;

	[PublicizedFrom(EAccessModifier.Private)]
	public int origRaidViewerMinimumIndex;

	[PublicizedFrom(EAccessModifier.Private)]
	public int origHelperRewardAmountIndex;

	[PublicizedFrom(EAccessModifier.Private)]
	public int origHelperRewardTimeIndex;

	[PublicizedFrom(EAccessModifier.Private)]
	public int origViewerStartingPoints;

	[PublicizedFrom(EAccessModifier.Private)]
	public int origMaxDailyVotes;

	[PublicizedFrom(EAccessModifier.Private)]
	public int origVoteTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public int origVoteDayTimeRange;

	[PublicizedFrom(EAccessModifier.Private)]
	public int origVoteViewerDefeatReward;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool origAllowVotesDuringBloodmoon;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool origAllowVotesDuringQuests;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool origAllowVotesInSafeZone;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool startedSinceOpened;

	[PublicizedFrom(EAccessModifier.Private)]
	public TwitchManager twitchManager;

	[PublicizedFrom(EAccessModifier.Private)]
	public int tempSubModifier = 1;

	[PublicizedFrom(EAccessModifier.Private)]
	public int tempGiftSubModifier = 1;

	[PublicizedFrom(EAccessModifier.Private)]
	public int tempVoteDayTimeRange = 2;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool origAllowBitEvents = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool origAllowSubEvents = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool origAllowGiftSubEvents = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool origAllowCharityEvents = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool origAllowRaidEvents = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool origAllowHypeTrainEvents = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool origAllowCreatorGoalEvents = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool origAllowChannelPointRedemptions = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool showDeviceCode;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Texture qrCodeTexControl;

	[PublicizedFrom(EAccessModifier.Private)]
	public string authCode = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public string authVerificationUrl = "";

	public enum TwitchBloodMoonOptions
	{
		Disabled,
		Standard,
		CooldownOnly
	}

	public enum PointsGenerationOptions
	{
		Disabled,
		Standard,
		Double,
		Triple
	}

	public enum BitAddOptions
	{
		Standard = 1,
		Double,
		Triple
	}
}
