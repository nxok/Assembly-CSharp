using System;
using System.Globalization;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_ServerBrowserGamePrefSelectorCombo : XUiController, IServerBrowserFilterControl
{
	[XuiXmlAttribute("allow_custom", false)]
	public bool AllowCustom
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	} = true;

	[XuiXmlAttribute("force_custom", false)]
	public bool ForceCustom
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[XuiXmlAttribute("default", false)]
	public string DefaultValue
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[XuiXmlAttribute("default_min", false)]
	public string DefaultMin
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[XuiXmlAttribute("default_max", false)]
	public string DefaultMax
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[XuiXmlAttribute("default_string", false)]
	public string DefaultString
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public bool IsCustomRange
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this.controlCombo != null && this.controlCombo.Value.Type == XUiC_ServerBrowserGamePrefSelectorCombo.EOptionValueType.Custom;
		}
	}

	public GameInfoBool GameInfoBool
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	} = (GameInfoBool)(-1);

	public GameInfoInt GameInfoInt
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	} = (GameInfoInt)(-1);

	public GameInfoString GameInfoString
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	} = (GameInfoString)(-1);

	public int? ValueRangeMin
	{
		set
		{
			int? num = this.valueRangeMin;
			int? num2 = value;
			if ((num.GetValueOrDefault() == num2.GetValueOrDefault()) & (num != null == (num2 != null)))
			{
				return;
			}
			this.valueRangeMin = value;
			this.setupOptions();
		}
	}

	public int? ValueRangeMax
	{
		set
		{
			int? num = this.valueRangeMax;
			int? num2 = value;
			if ((num.GetValueOrDefault() == num2.GetValueOrDefault()) & (num != null == (num2 != null)))
			{
				return;
			}
			this.valueRangeMax = value;
			this.setupOptions();
		}
	}

	public Func<int, int> CustomValuePreFilterModifierFunc
	{
		set
		{
			this.customValuePreFilterModifierFunc = value;
		}
	}

	public Func<int, int> ValuePreDisplayModifierFunc
	{
		set
		{
			this.valuePreDisplayModifierFunc = value;
			if (this.valuePreDisplayModifierFunc != null)
			{
				this.setupOptions();
			}
		}
	}

	public bool Enabled
	{
		get
		{
			return this.enabled;
		}
		set
		{
			if (this.enabled != value)
			{
				this.enabled = value;
				this.controlCombo.Enabled = value;
			}
		}
	}

	[XuiXmlBinding("iscustomrange")]
	public bool BindingIsCustomRange
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this.valueType != GamePrefs.EnumType.String && (this.ForceCustom || this.IsCustomRange);
		}
	}

	[XuiXmlBinding("iscustomstring")]
	public bool IsCustomString
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this.valueType == GamePrefs.EnumType.String && (this.ForceCustom || this.IsCustomRange);
		}
	}

	[XuiXmlBinding("usecombo")]
	public bool UseCombo
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return !this.ForceCustom;
		}
	}

	public override void Init()
	{
		base.Init();
		GameInfoBool gameInfoBool;
		if (EnumUtils.TryParse<GameInfoBool>(this.viewComponent.ID, out gameInfoBool, false))
		{
			this.GameInfoBool = gameInfoBool;
			this.valueType = GamePrefs.EnumType.Bool;
		}
		GameInfoInt gameInfoInt;
		if (EnumUtils.TryParse<GameInfoInt>(this.viewComponent.ID, out gameInfoInt, false))
		{
			this.GameInfoInt = gameInfoInt;
			this.valueType = GamePrefs.EnumType.Int;
		}
		GameInfoString gameInfoString;
		if (EnumUtils.TryParse<GameInfoString>(this.viewComponent.ID, out gameInfoString, false))
		{
			this.GameInfoString = gameInfoString;
			this.valueType = GamePrefs.EnumType.String;
		}
		this.controlCombo = base.GetChildById("ControlCombo").GetChildByType<XUiC_ComboBoxList<XUiC_ServerBrowserGamePrefSelectorCombo.GameOptionValue>>();
		this.controlCombo.OnValueChanged += this.ControlCombo_OnValueChanged;
		this.txtValueMin = base.GetChildById("valuemin").GetChildByType<XUiC_TextInput>();
		this.txtValueMin.OnChangeHandler += this.ControlValue_OnChangeHandler;
		this.txtValueMin.UIInputController.OnScroll += this.controlCombo.ScrollEvent;
		this.txtValueMax = base.GetChildById("valuemax").GetChildByType<XUiC_TextInput>();
		this.txtValueMax.OnChangeHandler += this.ControlValue_OnChangeHandler;
		this.txtValueMax.UIInputController.OnScroll += this.controlCombo.ScrollEvent;
		this.txtValueString = base.GetChildById("valuestring").GetChildByType<XUiC_TextInput>();
		this.txtValueString.OnChangeHandler += this.ControlText_OnChangeHandler;
		this.txtValueString.UIInputController.OnScroll += this.controlCombo.ScrollEvent;
		this.txtValueMin.SelectOnTab = this.txtValueMax;
		this.txtValueMax.SelectOnTab = this.txtValueMin;
		this.setupOptions();
	}

	public override void OnOpen()
	{
		base.OnOpen();
		if (this.ForceCustom)
		{
			this.controlCombo.ShowButtons = false;
		}
		this.IsDirty = true;
	}

	[XuiXmlAttribute("values", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public void attributeValues(string _value)
	{
		if (_value.Length <= 0)
		{
			return;
		}
		this.valuesFromXml = _value.Split(',', StringSplitOptions.None);
		for (int i = 0; i < this.valuesFromXml.Length; i++)
		{
			this.valuesFromXml[i] = this.valuesFromXml[i].Trim();
		}
	}

	[XuiXmlAttribute("display_names", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public void attributeDisplayNames(string _value)
	{
		if (_value.Length <= 0)
		{
			return;
		}
		this.namesFromXml = _value.Split(',', StringSplitOptions.None);
		for (int i = 0; i < this.namesFromXml.Length; i++)
		{
			this.namesFromXml[i] = this.namesFromXml[i].Trim();
		}
	}

	[XuiXmlAttribute("value_localization_prefix", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public void attributeValueLocalizationPrefix(string _value)
	{
		if (_value.Length > 0)
		{
			this.valueLocalizationPrefixFromXml = _value.Trim();
		}
	}

	[XuiXmlAttribute("allow_any", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public void setAllowAny(bool _value)
	{
		if (_value == this.allowAny)
		{
			return;
		}
		this.allowAny = _value;
		if (this.controlCombo == null || this.controlCombo.Elements.Count == 0)
		{
			return;
		}
		if (_value)
		{
			if (!this.controlCombo.Elements.Contains(new XUiC_ServerBrowserGamePrefSelectorCombo.GameOptionValue(XUiC_ServerBrowserGamePrefSelectorCombo.EOptionValueType.Any, "")))
			{
				this.controlCombo.Elements.Insert(1, new XUiC_ServerBrowserGamePrefSelectorCombo.GameOptionValue(XUiC_ServerBrowserGamePrefSelectorCombo.EOptionValueType.Any, Localization.Get("goAnyValue", false, null)));
				if (this.controlCombo.SelectedIndex > 0)
				{
					XUiC_ComboBoxList<XUiC_ServerBrowserGamePrefSelectorCombo.GameOptionValue> xuiC_ComboBoxList = this.controlCombo;
					int num = xuiC_ComboBoxList.SelectedIndex;
					xuiC_ComboBoxList.SelectedIndex = num + 1;
				}
			}
		}
		else if (this.controlCombo.Elements.Remove(new XUiC_ServerBrowserGamePrefSelectorCombo.GameOptionValue(XUiC_ServerBrowserGamePrefSelectorCombo.EOptionValueType.Any, "")) && this.controlCombo.SelectedIndex > 1)
		{
			XUiC_ComboBoxList<XUiC_ServerBrowserGamePrefSelectorCombo.GameOptionValue> xuiC_ComboBoxList2 = this.controlCombo;
			int num = xuiC_ComboBoxList2.SelectedIndex;
			xuiC_ComboBoxList2.SelectedIndex = num - 1;
		}
		this.controlCombo.SelectedIndex = this.controlCombo.SelectedIndex;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (this.IsDirty)
		{
			this.IsDirty = false;
			base.RefreshBindings();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string getGameInfoName()
	{
		switch (this.valueType)
		{
		case GamePrefs.EnumType.Int:
			return this.GameInfoInt.ToStringCached<GameInfoInt>();
		case GamePrefs.EnumType.String:
			return this.GameInfoString.ToStringCached<GameInfoString>();
		case GamePrefs.EnumType.Bool:
			return this.GameInfoBool.ToStringCached<GameInfoBool>();
		}
		return null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setupOptions()
	{
		this.controlCombo.Elements.Clear();
		this.controlCombo.Elements.Add(new XUiC_ServerBrowserGamePrefSelectorCombo.GameOptionValue(XUiC_ServerBrowserGamePrefSelectorCombo.EOptionValueType.Custom, ""));
		if (this.allowAny)
		{
			this.controlCombo.Elements.Add(new XUiC_ServerBrowserGamePrefSelectorCombo.GameOptionValue(XUiC_ServerBrowserGamePrefSelectorCombo.EOptionValueType.Any, Localization.Get("goAnyValue", false, null)));
		}
		if (!this.ForceCustom)
		{
			int[] array = null;
			string[] array2 = null;
			string[] array3;
			switch (this.valueType)
			{
			case GamePrefs.EnumType.Int:
			{
				if (!string.IsNullOrEmpty(this.DefaultValue))
				{
					this.defaultOptionValue = new XUiC_ServerBrowserGamePrefSelectorCombo.GameOptionValue(StringParsers.ParseSInt32(this.DefaultValue, 0, -1, NumberStyles.Integer), "");
				}
				if (this.valuesFromXml != null)
				{
					array = new int[this.valuesFromXml.Length];
					for (int i = 0; i < this.valuesFromXml.Length; i++)
					{
						array[i] = StringParsers.ParseSInt32(this.valuesFromXml[i], 0, -1, NumberStyles.Integer);
					}
				}
				if (array == null)
				{
					if (this.namesFromXml == null)
					{
						throw new Exception("Illegal option setup for " + this.getGameInfoName() + " (no values and no names specified)");
					}
					array = new int[this.namesFromXml.Length];
					for (int j = 0; j < array.Length; j++)
					{
						array[j] = j;
					}
				}
				array3 = new string[array.Length];
				if (this.namesFromXml == null || this.namesFromXml.Length != array.Length)
				{
					for (int k = 0; k < array.Length; k++)
					{
						if (this.namesFromXml != null && k < this.namesFromXml.Length)
						{
							array3[k] = Localization.Get(this.namesFromXml[k], false, null);
						}
						else
						{
							int num = array[k];
							if (this.valuePreDisplayModifierFunc != null)
							{
								num = this.valuePreDisplayModifierFunc(num);
							}
							if (string.IsNullOrEmpty(this.valueLocalizationPrefixFromXml))
							{
								array3[k] = num.ToString();
							}
							else
							{
								array3[k] = string.Format(Localization.Get(this.valueLocalizationPrefixFromXml + ((num == 1) ? "" : "s"), false, null), num);
							}
						}
					}
					goto IL_03AA;
				}
				for (int l = 0; l < this.namesFromXml.Length; l++)
				{
					array3[l] = Localization.Get(this.namesFromXml[l], false, null);
				}
				goto IL_03AA;
			}
			case GamePrefs.EnumType.String:
			{
				if (!string.IsNullOrEmpty(this.DefaultValue))
				{
					this.defaultOptionValue = new XUiC_ServerBrowserGamePrefSelectorCombo.GameOptionValue(this.DefaultValue, "");
				}
				array2 = this.valuesFromXml;
				array3 = new string[array2.Length];
				if (this.namesFromXml == null || this.namesFromXml.Length != array2.Length)
				{
					for (int m = 0; m < array2.Length; m++)
					{
						if (this.namesFromXml != null && m < this.namesFromXml.Length)
						{
							array3[m] = Localization.Get(this.namesFromXml[m], false, null);
						}
						else
						{
							string text = array2[m];
							if (string.IsNullOrEmpty(this.valueLocalizationPrefixFromXml))
							{
								array3[m] = text;
							}
							else
							{
								array3[m] = Localization.Get(this.valueLocalizationPrefixFromXml + text, false, null);
							}
						}
					}
					goto IL_03AA;
				}
				for (int n = 0; n < this.namesFromXml.Length; n++)
				{
					array3[n] = Localization.Get(this.namesFromXml[n], false, null);
				}
				goto IL_03AA;
			}
			case GamePrefs.EnumType.Bool:
				if (!string.IsNullOrEmpty(this.DefaultValue))
				{
					this.defaultOptionValue = new XUiC_ServerBrowserGamePrefSelectorCombo.GameOptionValue(StringParsers.ParseBool(this.DefaultValue, 0, -1, true) ? 1 : 0, "");
				}
				array = new int[] { 0, 1 };
				array3 = new string[]
				{
					Localization.Get("goOff", false, null),
					Localization.Get("goOn", false, null)
				};
				goto IL_03AA;
			}
			throw new NotSupportedException("Not a valid GameInfoX: " + this.viewComponent.ID);
			IL_03AA:
			if (array3 == null)
			{
				throw new Exception("Illegal option setup for " + this.getGameInfoName() + " (names null)");
			}
			if (this.valueType != GamePrefs.EnumType.String)
			{
				if (array == null)
				{
					throw new Exception("Illegal option setup for " + this.getGameInfoName() + " (values still null)");
				}
				for (int num2 = 0; num2 < array.Length; num2++)
				{
					if ((this.valueRangeMin == null || array[num2] >= this.valueRangeMin.Value) && (this.valueRangeMax == null || array[num2] <= this.valueRangeMax.Value))
					{
						this.controlCombo.Elements.Add(new XUiC_ServerBrowserGamePrefSelectorCombo.GameOptionValue(array[num2], array3[num2]));
					}
				}
			}
			if (this.valueType == GamePrefs.EnumType.String)
			{
				if (array2 == null)
				{
					throw new Exception("Illegal option setup for " + this.getGameInfoName() + " (values still null)");
				}
				for (int num3 = 0; num3 < array2.Length; num3++)
				{
					this.controlCombo.Elements.Add(new XUiC_ServerBrowserGamePrefSelectorCombo.GameOptionValue(array2[num3], array3[num3]));
				}
			}
		}
		this.controlCombo.MinIndex = (((this.valueType == GamePrefs.EnumType.Int || this.valueType == GamePrefs.EnumType.String) && (this.ForceCustom || this.AllowCustom)) ? 0 : 1);
		this.setDefaultValues();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ControlCombo_OnValueChanged(XUiController _sender, XUiC_ServerBrowserGamePrefSelectorCombo.GameOptionValue _oldValue, XUiC_ServerBrowserGamePrefSelectorCombo.GameOptionValue _newValue)
	{
		this.IsDirty = true;
		Action<IServerBrowserFilterControl> onValueChanged = this.OnValueChanged;
		if (onValueChanged == null)
		{
			return;
		}
		onValueChanged(this);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public unsafe void ControlValue_OnChangeHandler(XUiController _sender, string _text, bool _changeFromCode)
	{
		XUiC_TextInput xuiC_TextInput = _sender as XUiC_TextInput;
		if (xuiC_TextInput != null && !string.IsNullOrEmpty(_text))
		{
			ReadOnlySpan<char> readOnlySpan = _text.AsSpan().Trim();
			while (readOnlySpan.Length > 1 && *readOnlySpan[0] == 48)
			{
				ReadOnlySpan<char> readOnlySpan2 = readOnlySpan;
				readOnlySpan = readOnlySpan2.Slice(1, readOnlySpan2.Length - 1);
			}
			int num2;
			int? num = (StringParsers.TryParseSInt32(new string(readOnlySpan), out num2) ? new int?(num2) : null);
			if (num != null)
			{
				if (this.valueRangeMin != null && num.Value < this.valueRangeMin.Value)
				{
					num = this.valueRangeMin;
				}
				if (this.valueRangeMax != null && num.Value > this.valueRangeMax.Value)
				{
					num = this.valueRangeMax;
				}
			}
			string text = ((num != null) ? num.Value.ToString(CultureInfo.InvariantCulture) : "");
			if (text != _text)
			{
				xuiC_TextInput.Text = text;
			}
		}
		Action<IServerBrowserFilterControl> onValueChanged = this.OnValueChanged;
		if (onValueChanged == null)
		{
			return;
		}
		onValueChanged(this);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ControlText_OnChangeHandler(XUiController _sender, string _text, bool _changeFromCode)
	{
		Action<IServerBrowserFilterControl> onValueChanged = this.OnValueChanged;
		if (onValueChanged == null)
		{
			return;
		}
		onValueChanged(this);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setDefaultValues()
	{
		if (!this.ForceCustom && this.defaultOptionValue.Type != XUiC_ServerBrowserGamePrefSelectorCombo.EOptionValueType.Null)
		{
			this.controlCombo.SelectedIndex = this.controlCombo.Elements.IndexOf(this.defaultOptionValue);
		}
		else
		{
			this.controlCombo.SelectedIndex = (this.ForceCustom ? 0 : 1);
		}
		this.txtValueMin.Text = this.DefaultMin ?? "";
		this.txtValueMax.Text = this.DefaultMax ?? "";
		this.txtValueString.Text = this.DefaultString ?? "";
	}

	public void Reset()
	{
		this.setDefaultValues();
		this.IsDirty = true;
		Action<IServerBrowserFilterControl> onValueChanged = this.OnValueChanged;
		if (onValueChanged == null)
		{
			return;
		}
		onValueChanged(this);
	}

	public void SelectEntry(XUiC_ServerBrowserGamePrefSelectorCombo.GameOptionValue _value)
	{
		int num = this.controlCombo.Elements.IndexOf(_value);
		if (num >= 0)
		{
			this.controlCombo.SelectedIndex = num;
			Action<IServerBrowserFilterControl> onValueChanged = this.OnValueChanged;
			if (onValueChanged == null)
			{
				return;
			}
			onValueChanged(this);
		}
	}

	public XUiC_ServerBrowserGamePrefSelectorCombo.GameOptionValue GetSelection()
	{
		return this.controlCombo.Value;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ServersList.UiServerFilter getValueRangeFilter(string _filterName, int? _filterMin, int? _filterMax)
	{
		if (this.valueRangeMin != null)
		{
			if (_filterMin != null)
			{
				int? num = _filterMin;
				int num2 = this.valueRangeMin.Value;
				if (!((num.GetValueOrDefault() < num2) & (num != null)))
				{
					goto IL_0074;
				}
			}
			_filterMin = new int?(this.valueRangeMin.Value);
		}
		IL_0074:
		if (this.valueRangeMax != null)
		{
			if (_filterMax != null)
			{
				int? num = _filterMax;
				int num2 = this.valueRangeMax.Value;
				if (!((num.GetValueOrDefault() > num2) & (num != null)))
				{
					goto IL_00CD;
				}
			}
			_filterMax = new int?(this.valueRangeMax.Value);
		}
		IL_00CD:
		Func<XUiC_ServersList.ListEntry, bool> func;
		IServerListInterface.ServerFilter.EServerFilterType eserverFilterType;
		if (_filterMin != null)
		{
			if (_filterMax != null)
			{
				if (_filterMin.Value == _filterMax.Value)
				{
					func = delegate(XUiC_ServersList.ListEntry _entry)
					{
						int value = _entry.GameServerInfo.GetValue(this.GameInfoInt);
						int? filterMin = _filterMin;
						return (value == filterMin.GetValueOrDefault()) & (filterMin != null);
					};
					eserverFilterType = IServerListInterface.ServerFilter.EServerFilterType.IntValue;
				}
				else
				{
					func = delegate(XUiC_ServersList.ListEntry _entry)
					{
						int value2 = _entry.GameServerInfo.GetValue(this.GameInfoInt);
						int num3 = value2;
						int? num4 = _filterMin;
						if ((num3 >= num4.GetValueOrDefault()) & (num4 != null))
						{
							int num5 = value2;
							num4 = _filterMax;
							return (num5 <= num4.GetValueOrDefault()) & (num4 != null);
						}
						return false;
					};
					eserverFilterType = IServerListInterface.ServerFilter.EServerFilterType.IntRange;
				}
			}
			else
			{
				func = delegate(XUiC_ServersList.ListEntry _entry)
				{
					int value3 = _entry.GameServerInfo.GetValue(this.GameInfoInt);
					int? filterMin2 = _filterMin;
					return (value3 >= filterMin2.GetValueOrDefault()) & (filterMin2 != null);
				};
				eserverFilterType = IServerListInterface.ServerFilter.EServerFilterType.IntMin;
			}
		}
		else if (_filterMax != null)
		{
			func = delegate(XUiC_ServersList.ListEntry _entry)
			{
				int value4 = _entry.GameServerInfo.GetValue(this.GameInfoInt);
				int? filterMax = _filterMax;
				return (value4 <= filterMax.GetValueOrDefault()) & (filterMax != null);
			};
			eserverFilterType = IServerListInterface.ServerFilter.EServerFilterType.IntMax;
		}
		else
		{
			func = null;
			eserverFilterType = IServerListInterface.ServerFilter.EServerFilterType.Any;
		}
		return new XUiC_ServersList.UiServerFilter(_filterName, XUiC_ServersList.EnumServerLists.Regular, func, eserverFilterType, _filterMin.GetValueOrDefault(), _filterMax.GetValueOrDefault(), false, null);
	}

	public XUiC_ServersList.UiServerFilter GetFilter()
	{
		string gameInfoName = this.getGameInfoName();
		if (this.controlCombo.Value.Type == XUiC_ServerBrowserGamePrefSelectorCombo.EOptionValueType.Any)
		{
			if (this.valueType != GamePrefs.EnumType.String)
			{
				return this.getValueRangeFilter(gameInfoName, null, null);
			}
			return new XUiC_ServersList.UiServerFilter(gameInfoName, XUiC_ServersList.EnumServerLists.Regular, null, IServerListInterface.ServerFilter.EServerFilterType.Any, 0, 0, false, null);
		}
		else
		{
			if (!this.IsCustomRange)
			{
				switch (this.valueType)
				{
				case GamePrefs.EnumType.Int:
				{
					int intValue = this.controlCombo.Value.IntValue;
					return this.getValueRangeFilter(gameInfoName, new int?(intValue), new int?(intValue));
				}
				case GamePrefs.EnumType.String:
				{
					string sValue2 = this.controlCombo.Value.StringValue;
					Func<XUiC_ServersList.ListEntry, bool> func = (XUiC_ServersList.ListEntry _entry) => _entry.GameServerInfo.GetValue(this.GameInfoString).EqualsCaseInsensitive(sValue2);
					IServerListInterface.ServerFilter.EServerFilterType eserverFilterType = IServerListInterface.ServerFilter.EServerFilterType.StringValue;
					return new XUiC_ServersList.UiServerFilter(gameInfoName, XUiC_ServersList.EnumServerLists.Regular, func, eserverFilterType, 0, 0, false, sValue2);
				}
				case GamePrefs.EnumType.Bool:
				{
					bool bValue = this.controlCombo.Value.IntValue == 1;
					Func<XUiC_ServersList.ListEntry, bool> func = (XUiC_ServersList.ListEntry _entry) => _entry.GameServerInfo.GetValue(this.GameInfoBool) == bValue;
					IServerListInterface.ServerFilter.EServerFilterType eserverFilterType = IServerListInterface.ServerFilter.EServerFilterType.BoolValue;
					return new XUiC_ServersList.UiServerFilter(gameInfoName, XUiC_ServersList.EnumServerLists.Regular, func, eserverFilterType, 0, 0, bValue, null);
				}
				}
				throw new ArgumentOutOfRangeException();
			}
			if (this.valueType != GamePrefs.EnumType.String)
			{
				int num2;
				int? num = (StringParsers.TryParseSInt32(this.txtValueMin.Text, out num2) ? new int?(num2) : null);
				int num4;
				int? num3 = (StringParsers.TryParseSInt32(this.txtValueMax.Text, out num4) ? new int?(num4) : null);
				if (num != null && this.customValuePreFilterModifierFunc != null)
				{
					num = new int?(this.customValuePreFilterModifierFunc(num.Value));
				}
				if (num3 != null && this.customValuePreFilterModifierFunc != null)
				{
					num3 = new int?(this.customValuePreFilterModifierFunc(num3.Value));
				}
				return this.getValueRangeFilter(gameInfoName, num, num3);
			}
			string sValue = this.txtValueString.Text.Trim();
			if (sValue.Length == 0)
			{
				return new XUiC_ServersList.UiServerFilter(gameInfoName, XUiC_ServersList.EnumServerLists.Regular, null, IServerListInterface.ServerFilter.EServerFilterType.Any, 0, 0, false, null);
			}
			Func<XUiC_ServersList.ListEntry, bool> func2 = (XUiC_ServersList.ListEntry _entry) => _entry.GameServerInfo.GetValue(this.GameInfoString).ContainsCaseInsensitive(sValue);
			IServerListInterface.ServerFilter.EServerFilterType eserverFilterType2 = IServerListInterface.ServerFilter.EServerFilterType.StringContains;
			return new XUiC_ServersList.UiServerFilter(gameInfoName, XUiC_ServersList.EnumServerLists.Regular, func2, eserverFilterType2, 0, 0, false, this.txtValueString.Text);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<XUiC_ServerBrowserGamePrefSelectorCombo.GameOptionValue> controlCombo;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput txtValueMin;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput txtValueMax;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput txtValueString;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool allowAny = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ServerBrowserGamePrefSelectorCombo.GameOptionValue defaultOptionValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public string[] valuesFromXml;

	[PublicizedFrom(EAccessModifier.Private)]
	public string[] namesFromXml;

	[PublicizedFrom(EAccessModifier.Private)]
	public string valueLocalizationPrefixFromXml;

	[PublicizedFrom(EAccessModifier.Private)]
	public int? valueRangeMin;

	[PublicizedFrom(EAccessModifier.Private)]
	public int? valueRangeMax;

	[PublicizedFrom(EAccessModifier.Private)]
	public GamePrefs.EnumType valueType;

	[PublicizedFrom(EAccessModifier.Private)]
	public Func<int, int> customValuePreFilterModifierFunc;

	[PublicizedFrom(EAccessModifier.Private)]
	public Func<int, int> valuePreDisplayModifierFunc;

	public Action<IServerBrowserFilterControl> OnValueChanged;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool enabled = true;

	public enum EOptionValueType
	{
		Null,
		Custom,
		Any,
		Int,
		String
	}

	public readonly struct GameOptionValue : IEquatable<XUiC_ServerBrowserGamePrefSelectorCombo.GameOptionValue>
	{
		public GameOptionValue(XUiC_ServerBrowserGamePrefSelectorCombo.EOptionValueType _type, string _displayName)
		{
			this.Type = _type;
			this.IntValue = -1;
			this.StringValue = null;
			this.displayName = _displayName;
		}

		public GameOptionValue(int _intValue, string _displayName)
		{
			this.Type = XUiC_ServerBrowserGamePrefSelectorCombo.EOptionValueType.Int;
			this.IntValue = _intValue;
			this.StringValue = null;
			this.displayName = _displayName;
		}

		public GameOptionValue(string _stringValue, string _displayName)
		{
			this.Type = XUiC_ServerBrowserGamePrefSelectorCombo.EOptionValueType.String;
			this.IntValue = -1;
			this.StringValue = _stringValue;
			this.displayName = _displayName;
		}

		public override string ToString()
		{
			return this.displayName;
		}

		public bool Equals(XUiC_ServerBrowserGamePrefSelectorCombo.GameOptionValue _other)
		{
			if (this.Type != _other.Type)
			{
				return false;
			}
			switch (this.Type)
			{
			case XUiC_ServerBrowserGamePrefSelectorCombo.EOptionValueType.Custom:
			case XUiC_ServerBrowserGamePrefSelectorCombo.EOptionValueType.Any:
				return true;
			case XUiC_ServerBrowserGamePrefSelectorCombo.EOptionValueType.Int:
				return this.IntValue == _other.IntValue;
			case XUiC_ServerBrowserGamePrefSelectorCombo.EOptionValueType.String:
				return this.StringValue == _other.StringValue;
			default:
				throw new ArgumentOutOfRangeException();
			}
		}

		public override bool Equals(object _obj)
		{
			if (_obj is XUiC_ServerBrowserGamePrefSelectorCombo.GameOptionValue)
			{
				XUiC_ServerBrowserGamePrefSelectorCombo.GameOptionValue gameOptionValue = (XUiC_ServerBrowserGamePrefSelectorCombo.GameOptionValue)_obj;
				return this.Equals(gameOptionValue);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return (int)((((((this.Type * (XUiC_ServerBrowserGamePrefSelectorCombo.EOptionValueType)397) ^ (XUiC_ServerBrowserGamePrefSelectorCombo.EOptionValueType)this.IntValue) * (XUiC_ServerBrowserGamePrefSelectorCombo.EOptionValueType)397) ^ (XUiC_ServerBrowserGamePrefSelectorCombo.EOptionValueType)((this.StringValue != null) ? this.StringValue.GetHashCode() : 0)) * (XUiC_ServerBrowserGamePrefSelectorCombo.EOptionValueType)397) ^ (XUiC_ServerBrowserGamePrefSelectorCombo.EOptionValueType)((this.displayName != null) ? this.displayName.GetHashCode() : 0));
		}

		public readonly XUiC_ServerBrowserGamePrefSelectorCombo.EOptionValueType Type;

		public readonly int IntValue;

		public readonly string StringValue;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly string displayName;
	}
}
