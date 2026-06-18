using System;
using System.Globalization;
using UnityEngine.Scripting;

[Preserve]
[PublicizedFrom(EAccessModifier.Internal)]
public class XUiC_CustomCharacterWindowGroup : XUiController
{
	[XuiXmlBinding("ismale")]
	public bool IsMale
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this.cbxGender != null && this.cbxGender.SelectedIndex == 1;
		}
	}

	public override void Init()
	{
		base.Init();
		this.gr = GameRandomManager.Instance.CreateGameRandom();
		SDCSDataUtils.SetupData();
	}

	public override void OnOpen()
	{
		this.windowGroup.openWindowOnEsc = XUiC_PlayerProfile.ID;
		base.OnOpen();
		this.PlayerProfile = null;
		this.Archetype = null;
		this.Archetype = Archetype.GetArchetype(ProfileSDF.CurrentProfileName());
		if (this.Archetype != null)
		{
			this.Archetype = this.Archetype.Clone();
		}
		else
		{
			string text = ProfileSDF.CurrentProfileName();
			this.PlayerProfile = PlayerProfile.LoadProfile(text).Clone();
		}
		this.setInitialOptions();
	}

	public override void OnClose()
	{
		base.OnClose();
		Action action = this.onClosed;
		if (action != null)
		{
			action();
		}
		this.onClosed = null;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		base.handleDirtyUpdateDefault();
		if (XUiUtils.HotkeysAllowedFor(this.viewComponent, false))
		{
			if (this.xui.playerUI.playerInput.GUIActions.Apply.WasReleased)
			{
				ThreadManager.RunTaskAfterFrames(delegate
				{
					this.BtnApply_OnPress(null, 0);
				}, 1);
			}
			if (this.xui.playerUI.playerInput.GUIActions.Cancel.WasReleased)
			{
				ThreadManager.RunTaskAfterFrames(delegate
				{
					this.BtnBack_OnPress(null, 0);
				}, 1);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setInitialOptions()
	{
		if (this.PlayerProfile != null)
		{
			this.setupGenders();
			this.cbxGender.SelectedIndex = (this.PlayerProfile.IsMale ? 1 : 0);
			this.setupRaces(this.PlayerProfile.IsMale);
			this.setupVariants(this.PlayerProfile.IsMale, this.PlayerProfile.RaceName);
			this.setupHairStyles(this.PlayerProfile.IsMale);
			this.setupMustaches(this.PlayerProfile.IsMale);
			this.setupChops(this.PlayerProfile.IsMale);
			this.setupBeards(this.PlayerProfile.IsMale);
			this.setupEyeColors();
			this.setupHairColors();
			this.setSelectedRace(this.PlayerProfile.RaceName, false);
			this.setSelectedVariant(this.PlayerProfile.VariantNumber, false);
			this.setSelectedGeneric(this.PlayerProfile.EyeColor, this.cbxEyeColor, new Action<string>(this.applyEyeColor), false);
			this.setSelectedGeneric(this.PlayerProfile.HairName, this.cbxHairStyle, new Action<string>(this.applyHairStyle), false);
			this.setSelectedGeneric(this.PlayerProfile.MustacheName, this.cbxMustaches, new Action<string>(this.applyMustache), false);
			this.setSelectedGeneric(this.PlayerProfile.ChopsName, this.cbxChops, new Action<string>(this.applyChops), false);
			this.setSelectedGeneric(this.PlayerProfile.BeardName, this.cbxBeards, new Action<string>(this.applyBeard), false);
			this.setSelectedGeneric(this.PlayerProfile.HairColor, this.cbxHairColor, new Action<string>(this.applyHairColor), false);
			this.resetLocks();
			return;
		}
		Log.Error("PlayerProfile is null");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void resetLocks()
	{
		this.btnLockRace.IsSelected = true;
		this.btnLockGender.IsSelected = true;
		this.btnLockFace.IsSelected = false;
		this.btnLockEyeColor.IsSelected = false;
		this.btnLockHairStyle.IsSelected = false;
		this.btnLockHairColor.IsSelected = false;
		this.btnLockMustaches.IsSelected = false;
		this.btnLockChops.IsSelected = false;
		this.btnLockBeards.IsSelected = false;
	}

	[XuiBindEvent("OnPress", "btnApply")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnApply_OnPress(XUiController _sender, int _mouseButton)
	{
		if (this.hasChanges)
		{
			if (this.PlayerProfile != null)
			{
				ProfileSDF.SaveProfile(ProfileSDF.CurrentProfileName(), this.PlayerProfile.ProfileArchetype, this.PlayerProfile.IsMale, this.PlayerProfile.RaceName, this.PlayerProfile.VariantNumber, this.PlayerProfile.EyeColor, this.PlayerProfile.HairName, this.PlayerProfile.HairColor, this.PlayerProfile.MustacheName, this.PlayerProfile.ChopsName, this.PlayerProfile.BeardName);
			}
			else if (this.Archetype != null)
			{
				Archetype.SetArchetype(this.Archetype);
				Archetype.SaveArchetypesToFile();
			}
			this.hasChanges = false;
		}
		this.close();
	}

	[XuiBindEvent("OnPress", "btnBack")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnBack_OnPress(XUiController _sender, int _mouseButton)
	{
		this.close();
	}

	[XuiBindEvent("OnPress", "btnRandomize")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnRandomize_OnPressed(XUiController _sender, int _mouseButton)
	{
		if (!this.btnLockGender.IsSelected)
		{
			this.cbxGender.SelectedIndex = this.gr.RandomRange(2);
			this.cbxGender.TriggerValueChangedEvent(this.cbxGender.Elements[0]);
		}
		if (!this.btnLockRace.IsSelected)
		{
			this.cbxRace.SelectedIndex = this.gr.RandomRange(this.cbxRace.Elements.Count);
			this.cbxRace.TriggerValueChangedEvent(this.cbxRace.Elements[0]);
		}
		if (!this.btnLockFace.IsSelected)
		{
			this.cbxVariant.SelectedIndex = this.gr.RandomRange(this.cbxVariant.Elements.Count);
			this.cbxVariant.TriggerValueChangedEvent(this.cbxVariant.Elements[0]);
		}
		if (!this.btnLockEyeColor.IsSelected)
		{
			this.cbxEyeColor.SelectedIndex = this.gr.RandomRange(this.cbxEyeColor.Elements.Count);
			this.cbxEyeColor.TriggerValueChangedEvent(this.cbxEyeColor.Elements[0]);
		}
		if (!this.btnLockHairStyle.IsSelected)
		{
			this.cbxHairStyle.SelectedIndex = this.gr.RandomRange(this.cbxHairStyle.Elements.Count);
			this.cbxHairStyle.TriggerValueChangedEvent(this.cbxHairStyle.Elements[0]);
		}
		if (!this.btnLockHairColor.IsSelected)
		{
			this.cbxHairColor.SelectedIndex = this.gr.RandomRange(this.cbxHairColor.Elements.Count);
			this.cbxHairColor.TriggerValueChangedEvent(this.cbxHairColor.Elements[0]);
		}
		if (!this.btnLockMustaches.IsSelected)
		{
			this.cbxMustaches.SelectedIndex = this.gr.RandomRange(this.cbxMustaches.Elements.Count);
			this.cbxMustaches.TriggerValueChangedEvent(this.cbxMustaches.Elements[0]);
		}
		if (!this.btnLockChops.IsSelected)
		{
			this.cbxChops.SelectedIndex = this.gr.RandomRange(this.cbxChops.Elements.Count);
			this.cbxChops.TriggerValueChangedEvent(this.cbxChops.Elements[0]);
		}
		if (!this.btnLockBeards.IsSelected)
		{
			this.cbxBeards.SelectedIndex = this.gr.RandomRange(this.cbxBeards.Elements.Count);
			this.cbxBeards.TriggerValueChangedEvent(this.cbxBeards.Elements[0]);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void close()
	{
		this.xui.playerUI.windowManager.Close(this.windowGroup, false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updatePreview(bool _zoomToHead = true)
	{
		this.previewWindow.MakePreview();
		if (_zoomToHead)
		{
			this.previewWindow.ZoomToHead();
		}
		this.hasChanges = true;
		this.IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setSelectedGeneric(string _name, XUiC_ComboBoxList<XUiC_CustomCharacterWindowGroup.NameData> _combo, Action<string> _applyFunc, bool _applyToPreview = false)
	{
		if (!string.IsNullOrEmpty(_name))
		{
			for (int i = 0; i < _combo.Elements.Count; i++)
			{
				if (_combo.Elements[i].InternalName.EqualsCaseInsensitive(_name))
				{
					_combo.SelectedIndex = i;
					return;
				}
			}
		}
		_combo.SelectedIndex = 0;
		if (_applyToPreview)
		{
			_applyFunc(_combo.Value.InternalName);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setupGenders()
	{
		this.cbxGender.Elements.Clear();
		this.cbxGender.Elements.Add(new XUiC_CustomCharacterWindowGroup.NameData("female", Localization.Get("xuiBoolMaleOff", false, null)));
		this.cbxGender.Elements.Add(new XUiC_CustomCharacterWindowGroup.NameData("male", Localization.Get("xuiBoolMaleOn", false, null)));
	}

	[XuiBindEvent("OnValueChanged", "cbxGender")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void cbxGenderOnValueChanged(XUiController _sender, XUiC_CustomCharacterWindowGroup.NameData _oldValue, XUiC_CustomCharacterWindowGroup.NameData _newValue)
	{
		string internalName = _newValue.InternalName;
		this.previewWindow.Archetype.Sex = internalName;
		bool flag = internalName == "male";
		if (this.PlayerProfile != null)
		{
			this.PlayerProfile.IsMale = flag;
		}
		string internalName2 = this.cbxRace.Value.InternalName;
		int num = StringParsers.ParseSInt32(this.cbxVariant.Value.InternalName, 0, -1, NumberStyles.Integer);
		this.setupRaces(flag);
		this.setSelectedRace(internalName2, true);
		this.setupVariants(flag, this.cbxRace.Value.InternalName);
		this.setSelectedVariant(num, true);
		this.setupHairStyles(flag);
		this.setupMustaches(flag);
		this.setupChops(flag);
		this.setupBeards(flag);
		this.setSelectedGeneric("", this.cbxMustaches, new Action<string>(this.applyMustache), true);
		this.setSelectedGeneric("", this.cbxChops, new Action<string>(this.applyChops), true);
		this.setSelectedGeneric("", this.cbxBeards, new Action<string>(this.applyBeard), true);
		this.updatePreview(false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setupRaces(bool _isMale)
	{
		this.cbxRace.Elements.Clear();
		foreach (string text in SDCSDataUtils.GetRaceList(_isMale))
		{
			this.cbxRace.Elements.Add(new XUiC_CustomCharacterWindowGroup.NameData(text, Localization.Get("xuiRace" + text.ToLower(), false, null)));
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setSelectedRace(string _raceName, bool _applyToPreview = false)
	{
		if (!string.IsNullOrEmpty(_raceName))
		{
			for (int i = 0; i < this.cbxRace.Elements.Count; i++)
			{
				if (this.cbxRace.Elements[i].InternalName.EqualsCaseInsensitive(_raceName))
				{
					this.cbxRace.SelectedIndex = i;
					return;
				}
			}
		}
		this.cbxRace.SelectedIndex = 0;
		if (_applyToPreview)
		{
			this.applyRace(this.cbxRace.Value.InternalName);
		}
	}

	[XuiBindEvent("OnValueChanged", "cbxRace")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void cbxRaceOnValueChanged(XUiController _sender, XUiC_CustomCharacterWindowGroup.NameData _oldValue, XUiC_CustomCharacterWindowGroup.NameData _newValue)
	{
		this.applyRace(_newValue.InternalName);
		int num = StringParsers.ParseSInt32(this.cbxVariant.Value.InternalName, 0, -1, NumberStyles.Integer);
		this.setupVariants(this.previewWindow.Archetype.IsMale, this.cbxRace.Value.InternalName);
		this.setSelectedVariant(num, true);
		this.updatePreview(false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void applyRace(string _name)
	{
		this.previewWindow.Archetype.Race = _name;
		if (this.PlayerProfile != null)
		{
			this.PlayerProfile.RaceName = _name;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setupVariants(bool _isMale, string _raceName)
	{
		this.cbxVariant.Elements.Clear();
		int num = 1;
		foreach (string text in SDCSDataUtils.GetVariantList(_isMale, _raceName))
		{
			this.cbxVariant.Elements.Add(new XUiC_CustomCharacterWindowGroup.NameData(text, string.Format("{0} {1:00}", Localization.Get("lblFace", false, null), num)));
			num++;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setSelectedVariant(int _variant, bool _applyToPreview = false)
	{
		if (_variant != -1)
		{
			for (int i = 0; i < this.cbxVariant.Elements.Count; i++)
			{
				if (StringParsers.ParseSInt32(this.cbxVariant.Elements[i].InternalName, 0, -1, NumberStyles.Integer) == _variant)
				{
					this.cbxVariant.SelectedIndex = i;
					return;
				}
			}
		}
		this.cbxVariant.SelectedIndex = 0;
		if (_applyToPreview)
		{
			this.applyVariant(StringParsers.ParseSInt32(this.cbxVariant.Value.InternalName, 0, -1, NumberStyles.Integer));
		}
	}

	[XuiBindEvent("OnValueChanged", "cbxVariant")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void cbxVariantOnValueChanged(XUiController _sender, XUiC_CustomCharacterWindowGroup.NameData _oldValue, XUiC_CustomCharacterWindowGroup.NameData _newValue)
	{
		this.applyVariant(StringParsers.ParseSInt32(_newValue.InternalName, 0, -1, NumberStyles.Integer));
		this.updatePreview(true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void applyVariant(int _variant)
	{
		this.previewWindow.Archetype.Variant = _variant;
		if (this.PlayerProfile != null)
		{
			this.PlayerProfile.VariantNumber = _variant;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setupEyeColors()
	{
		this.cbxEyeColor.Elements.Clear();
		int num = 1;
		foreach (string text in SDCSDataUtils.GetEyeColorNames())
		{
			this.cbxEyeColor.Elements.Add(new XUiC_CustomCharacterWindowGroup.NameData(text, string.Format("{0} {1:00}", Localization.Get("xuiCharacterColorSlotEyes", false, null), num)));
			num++;
		}
	}

	[XuiBindEvent("OnValueChanged", "cbxEyeColor")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void cbxEyeColorOnValueChanged(XUiController _sender, XUiC_CustomCharacterWindowGroup.NameData _oldValue, XUiC_CustomCharacterWindowGroup.NameData _newValue)
	{
		this.applyEyeColor(_newValue.InternalName);
		this.updatePreview(true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void applyEyeColor(string _name)
	{
		this.previewWindow.Archetype.EyeColorName = _name;
		if (this.PlayerProfile != null)
		{
			this.PlayerProfile.EyeColor = _name;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setupHairStyles(bool _isMale)
	{
		this.cbxHairStyle.Elements.Clear();
		this.cbxHairStyle.Elements.Add(new XUiC_CustomCharacterWindowGroup.NameData("", Localization.Get("xuiCharacterHairStyle", false, null) + " 00"));
		int num = 1;
		foreach (string text in SDCSDataUtils.GetHairNames(_isMale, SDCSDataUtils.HairTypes.Hair))
		{
			this.cbxHairStyle.Elements.Add(new XUiC_CustomCharacterWindowGroup.NameData(text, string.Format("{0} {1:00}", Localization.Get("xuiCharacterHairStyle", false, null), num)));
			num++;
		}
	}

	[XuiBindEvent("OnValueChanged", "cbxHairStyle")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void cbxHairsOnValueChanged(XUiController _sender, XUiC_CustomCharacterWindowGroup.NameData _oldValue, XUiC_CustomCharacterWindowGroup.NameData _newValue)
	{
		this.applyHairStyle(_newValue.InternalName);
		this.updatePreview(true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void applyHairStyle(string _name)
	{
		this.previewWindow.Archetype.Hair = _name;
		if (this.PlayerProfile != null)
		{
			this.PlayerProfile.HairName = _name;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setupHairColors()
	{
		this.cbxHairColor.Elements.Clear();
		int num = 1;
		foreach (SDCSDataUtils.HairColorData hairColorData in SDCSDataUtils.GetHairColorNames())
		{
			this.cbxHairColor.Elements.Add(new XUiC_CustomCharacterWindowGroup.NameData(hairColorData.PrefabName, string.Format("{0} {1:00}", Localization.Get("xuiCharacterHairColor", false, null), num)));
			num++;
		}
	}

	[XuiBindEvent("OnValueChanged", "cbxHairColor")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void cbxHairColorOnValueChanged(XUiController _sender, XUiC_CustomCharacterWindowGroup.NameData _oldValue, XUiC_CustomCharacterWindowGroup.NameData _newValue)
	{
		this.applyHairColor(_newValue.InternalName);
		this.updatePreview(true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void applyHairColor(string _name)
	{
		this.previewWindow.Archetype.HairColor = _name;
		if (this.PlayerProfile != null)
		{
			this.PlayerProfile.HairColor = _name;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setupMustaches(bool _isMale)
	{
		this.cbxMustaches.Elements.Clear();
		this.cbxMustaches.Elements.Add(new XUiC_CustomCharacterWindowGroup.NameData("", Localization.Get("xuiCharacterMustaches", false, null) + " 00"));
		foreach (string text in SDCSDataUtils.GetHairNames(_isMale, SDCSDataUtils.HairTypes.Mustache))
		{
			string text2 = text;
			if (text2.Length == 1)
			{
				text2 = text2.Insert(0, "0");
			}
			this.cbxMustaches.Elements.Add(new XUiC_CustomCharacterWindowGroup.NameData(text, Localization.Get("xuiCharacterMustaches", false, null) + " " + Localization.Get(text2, false, null)));
		}
	}

	[XuiBindEvent("OnValueChanged", "cbxMustaches")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void cbxMustachesOnValueChanged(XUiController _sender, XUiC_CustomCharacterWindowGroup.NameData _oldValue, XUiC_CustomCharacterWindowGroup.NameData _newValue)
	{
		this.applyMustache(_newValue.InternalName);
		this.updatePreview(true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void applyMustache(string _name)
	{
		this.previewWindow.Archetype.MustacheName = _name;
		if (this.PlayerProfile != null)
		{
			this.PlayerProfile.MustacheName = _name;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setupChops(bool _isMale)
	{
		this.cbxChops.Elements.Clear();
		this.cbxChops.Elements.Add(new XUiC_CustomCharacterWindowGroup.NameData("", Localization.Get("xuiCharacterChops", false, null) + " 00"));
		foreach (string text in SDCSDataUtils.GetHairNames(_isMale, SDCSDataUtils.HairTypes.Chops))
		{
			string text2 = text;
			if (text2.Length == 1)
			{
				text2 = text2.Insert(0, "0");
			}
			this.cbxChops.Elements.Add(new XUiC_CustomCharacterWindowGroup.NameData(text, Localization.Get("xuiCharacterChops", false, null) + " " + Localization.Get(text2, false, null)));
		}
	}

	[XuiBindEvent("OnValueChanged", "cbxChops")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void cbxChopsOnValueChanged(XUiController _sender, XUiC_CustomCharacterWindowGroup.NameData _oldValue, XUiC_CustomCharacterWindowGroup.NameData _newValue)
	{
		this.applyChops(_newValue.InternalName);
		this.updatePreview(true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void applyChops(string _name)
	{
		this.previewWindow.Archetype.ChopsName = _name;
		if (this.PlayerProfile != null)
		{
			this.PlayerProfile.ChopsName = _name;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setupBeards(bool _isMale)
	{
		this.cbxBeards.Elements.Clear();
		this.cbxBeards.Elements.Add(new XUiC_CustomCharacterWindowGroup.NameData("", Localization.Get("xuiCharacterBeards", false, null) + " 00"));
		foreach (string text in SDCSDataUtils.GetHairNames(_isMale, SDCSDataUtils.HairTypes.Beard))
		{
			string text2 = text;
			if (text2.Length == 1)
			{
				text2 = text2.Insert(0, "0");
			}
			this.cbxBeards.Elements.Add(new XUiC_CustomCharacterWindowGroup.NameData(text, Localization.Get("xuiCharacterBeards", false, null) + " " + Localization.Get(text2, false, null)));
		}
	}

	[XuiBindEvent("OnValueChanged", "cbxBeards")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void cbxBeardsOnValueChanged(XUiController _sender, XUiC_CustomCharacterWindowGroup.NameData _oldValue, XUiC_CustomCharacterWindowGroup.NameData _newValue)
	{
		this.applyBeard(_newValue.InternalName);
		this.updatePreview(true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void applyBeard(string _name)
	{
		this.previewWindow.Archetype.BeardName = _name;
		if (this.PlayerProfile != null)
		{
			this.PlayerProfile.BeardName = _name;
		}
	}

	public static void Open(XUi _xui, Action _onClosed)
	{
		XUiC_CustomCharacterWindowGroup childByType = _xui.GetChildByType<XUiC_CustomCharacterWindowGroup>();
		childByType.onClosed = _onClosed;
		_xui.playerUI.windowManager.Open(childByType.WindowGroup, false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool hasChanges;

	public PlayerProfile PlayerProfile;

	public Archetype Archetype;

	[XuiBindComponent("cbxRace", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ComboBoxList<XUiC_CustomCharacterWindowGroup.NameData> cbxRace;

	[XuiBindComponent("cbxGender", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ComboBoxList<XUiC_CustomCharacterWindowGroup.NameData> cbxGender;

	[XuiBindComponent("cbxEyeColor", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ComboBoxList<XUiC_CustomCharacterWindowGroup.NameData> cbxEyeColor;

	[XuiBindComponent("cbxHairStyle", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ComboBoxList<XUiC_CustomCharacterWindowGroup.NameData> cbxHairStyle;

	[XuiBindComponent("cbxHairColor", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ComboBoxList<XUiC_CustomCharacterWindowGroup.NameData> cbxHairColor;

	[XuiBindComponent("cbxMustaches", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ComboBoxList<XUiC_CustomCharacterWindowGroup.NameData> cbxMustaches;

	[XuiBindComponent("cbxChops", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ComboBoxList<XUiC_CustomCharacterWindowGroup.NameData> cbxChops;

	[XuiBindComponent("cbxBeards", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ComboBoxList<XUiC_CustomCharacterWindowGroup.NameData> cbxBeards;

	[XuiBindComponent("cbxFace", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ComboBoxList<XUiC_CustomCharacterWindowGroup.NameData> cbxVariant;

	[XuiBindComponent(true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_SDCSPreviewWindow previewWindow;

	[XuiBindComponent("btnBack", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnBack;

	[XuiBindComponent("btnApply", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnApply;

	[XuiBindComponent("btnRandomize", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnRandomize;

	[XuiBindComponent("btnLockRace", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ButtonSelectable btnLockRace;

	[XuiBindComponent("btnLockGender", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ButtonSelectable btnLockGender;

	[XuiBindComponent("btnLockFace", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ButtonSelectable btnLockFace;

	[XuiBindComponent("btnLockEyeColor", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ButtonSelectable btnLockEyeColor;

	[XuiBindComponent("btnLockHairStyle", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ButtonSelectable btnLockHairStyle;

	[XuiBindComponent("btnLockHairColor", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ButtonSelectable btnLockHairColor;

	[XuiBindComponent("btnLockMustaches", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ButtonSelectable btnLockMustaches;

	[XuiBindComponent("btnLockChops", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ButtonSelectable btnLockChops;

	[XuiBindComponent("btnLockBeards", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ButtonSelectable btnLockBeards;

	[PublicizedFrom(EAccessModifier.Private)]
	public GameRandom gr;

	[PublicizedFrom(EAccessModifier.Private)]
	public Action onClosed;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly struct NameData
	{
		public NameData(string _internalName, string _formattedName)
		{
			this.InternalName = _internalName;
			this.formattedName = _formattedName;
		}

		public override string ToString()
		{
			return this.formattedName;
		}

		public readonly string InternalName;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly string formattedName;
	}
}
