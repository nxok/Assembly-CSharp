using System;
using System.Globalization;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_ServerBrowserGamePrefSelector : XUiController
{
	public GamePrefs.EnumType ValueType
	{
		get
		{
			return this.valueType;
		}
	}

	public GameInfoBool GameInfoBool
	{
		get
		{
			return this.gameInfoBool;
		}
	}

	public GameInfoInt GameInfoInt
	{
		get
		{
			return this.gameInfoInt;
		}
	}

	public Func<int, int> ValuePreDisplayModifierFunc
	{
		get
		{
			return this.valuePreDisplayModifierFunc;
		}
		set
		{
			this.valuePreDisplayModifierFunc = value;
			if (this.valuePreDisplayModifierFunc != null)
			{
				this.setupOptions();
			}
		}
	}

	public override void Init()
	{
		base.Init();
		if (EnumUtils.TryParse<GameInfoBool>(this.viewComponent.ID, out this.gameInfoBool, false))
		{
			this.valueType = GamePrefs.EnumType.Bool;
		}
		else
		{
			this.gameInfoBool = (GameInfoBool)(-1);
		}
		if (EnumUtils.TryParse<GameInfoInt>(this.viewComponent.ID, out this.gameInfoInt, false))
		{
			this.valueType = GamePrefs.EnumType.Int;
		}
		else
		{
			this.gameInfoInt = (GameInfoInt)(-1);
		}
		this.controlCombo = base.GetChildById("ControlCombo").GetChildByType<XUiC_ComboBoxList<XUiC_ServerBrowserGamePrefSelector.GameOptionValue>>();
		this.controlCombo.OnValueChanged += this.ControlCombo_OnValueChanged;
		this.setupOptions();
	}

	[XuiXmlAttribute("values", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public void attributeValues(string _value)
	{
		if (_value.Length <= 0)
		{
			return;
		}
		string[] array = _value.Split(',', StringSplitOptions.None);
		this.valuesFromXml = new int[array.Length];
		for (int i = 0; i < array.Length; i++)
		{
			this.valuesFromXml[i] = StringParsers.ParseSInt32(array[i], 0, -1, NumberStyles.Integer);
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

	[PublicizedFrom(EAccessModifier.Private)]
	public string getGameInfoName()
	{
		GamePrefs.EnumType enumType = this.valueType;
		string text;
		if (enumType != GamePrefs.EnumType.Int)
		{
			if (enumType == GamePrefs.EnumType.Bool)
			{
				text = this.gameInfoBool.ToStringCached<GameInfoBool>();
			}
			else
			{
				text = null;
			}
		}
		else
		{
			text = this.gameInfoInt.ToStringCached<GameInfoInt>();
		}
		return text;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setupOptions()
	{
		string[] array = null;
		if (this.valueType == GamePrefs.EnumType.Int)
		{
			if (this.valuesFromXml == null)
			{
				if (this.namesFromXml == null)
				{
					throw new Exception("Illegal option setup for " + this.getGameInfoName() + " (no values and no names specified)");
				}
				this.valuesFromXml = new int[this.namesFromXml.Length];
				for (int i = 0; i < this.valuesFromXml.Length; i++)
				{
					this.valuesFromXml[i] = i;
				}
			}
			array = new string[this.valuesFromXml.Length];
			if (this.namesFromXml == null || this.namesFromXml.Length != this.valuesFromXml.Length)
			{
				for (int j = 0; j < this.valuesFromXml.Length; j++)
				{
					if (this.namesFromXml != null && j < this.namesFromXml.Length)
					{
						array[j] = Localization.Get(this.namesFromXml[j], false, null);
					}
					else
					{
						int num = this.valuesFromXml[j];
						if (this.valuePreDisplayModifierFunc != null)
						{
							num = this.valuePreDisplayModifierFunc(num);
						}
						array[j] = string.Format(Localization.Get(this.valueLocalizationPrefixFromXml + ((num == 1) ? "" : "s"), false, null), num);
					}
				}
			}
			else
			{
				for (int k = 0; k < this.namesFromXml.Length; k++)
				{
					array[k] = Localization.Get(this.namesFromXml[k], false, null);
				}
			}
		}
		else if (this.valueType == GamePrefs.EnumType.Bool)
		{
			this.valuesFromXml = new int[] { 0, 1 };
			array = new string[]
			{
				Localization.Get("goOff", false, null),
				Localization.Get("goOn", false, null)
			};
		}
		this.controlCombo.Elements.Clear();
		XUiC_ServerBrowserGamePrefSelector.GameOptionValue gameOptionValue = new XUiC_ServerBrowserGamePrefSelector.GameOptionValue(int.MinValue, Localization.Get("goAnyValue", false, null));
		this.controlCombo.Elements.Add(gameOptionValue);
		if (this.valuesFromXml == null)
		{
			throw new Exception("Illegal option setup for " + this.getGameInfoName() + " (values still null)");
		}
		if (array == null)
		{
			throw new Exception("Illegal option setup for " + this.getGameInfoName() + " (names null)");
		}
		for (int l = 0; l < this.valuesFromXml.Length; l++)
		{
			gameOptionValue = new XUiC_ServerBrowserGamePrefSelector.GameOptionValue(this.valuesFromXml[l], array[l]);
			this.controlCombo.Elements.Add(gameOptionValue);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ControlCombo_OnValueChanged(XUiController _sender, XUiC_ServerBrowserGamePrefSelector.GameOptionValue _oldValue, XUiC_ServerBrowserGamePrefSelector.GameOptionValue _newValue)
	{
		Action<XUiC_ServerBrowserGamePrefSelector> onValueChanged = this.OnValueChanged;
		if (onValueChanged == null)
		{
			return;
		}
		onValueChanged(this);
	}

	public void SetCurrentValue(object _value)
	{
		try
		{
			if (this.valueType == GamePrefs.EnumType.Int)
			{
				int num = (int)_value;
				bool flag = false;
				for (int i = 1; i < this.controlCombo.Elements.Count; i++)
				{
					if (this.controlCombo.Elements[i].Value == num)
					{
						this.controlCombo.SelectedIndex = i;
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					this.controlCombo.SelectedIndex = this.controlCombo.MinIndex;
				}
			}
			else if (this.valueType == GamePrefs.EnumType.Bool)
			{
				this.controlCombo.SelectedIndex = (((bool)_value) ? 2 : 1);
			}
		}
		catch (Exception ex)
		{
			Log.Exception(ex);
		}
	}

	public XUiC_ServersList.UiServerFilter GetFilter()
	{
		int value = this.controlCombo.Value.Value;
		bool flag = value == int.MinValue;
		GamePrefs.EnumType enumType = this.valueType;
		string text;
		if (enumType != GamePrefs.EnumType.Int)
		{
			if (enumType != GamePrefs.EnumType.Bool)
			{
				throw new ArgumentOutOfRangeException();
			}
			text = this.gameInfoBool.ToStringCached<GameInfoBool>();
		}
		else
		{
			text = this.gameInfoInt.ToStringCached<GameInfoInt>();
		}
		string text2 = text;
		if (flag)
		{
			return new XUiC_ServersList.UiServerFilter(text2, XUiC_ServersList.EnumServerLists.Regular, null, IServerListInterface.ServerFilter.EServerFilterType.Any, 0, 0, false, null);
		}
		int num = 0;
		bool flag2 = false;
		enumType = this.valueType;
		Func<XUiC_ServersList.ListEntry, bool> func;
		IServerListInterface.ServerFilter.EServerFilterType eserverFilterType;
		if (enumType != GamePrefs.EnumType.Int)
		{
			if (enumType != GamePrefs.EnumType.Bool)
			{
				throw new ArgumentOutOfRangeException();
			}
			bool bValue = value == 1;
			func = (XUiC_ServersList.ListEntry _entry) => _entry.GameServerInfo.GetValue(this.gameInfoBool) == bValue;
			eserverFilterType = IServerListInterface.ServerFilter.EServerFilterType.BoolValue;
			flag2 = bValue;
		}
		else
		{
			int iValue = value;
			func = (XUiC_ServersList.ListEntry _entry) => _entry.GameServerInfo.GetValue(this.gameInfoInt) == iValue;
			eserverFilterType = IServerListInterface.ServerFilter.EServerFilterType.IntValue;
			num = iValue;
		}
		return new XUiC_ServersList.UiServerFilter(text2, XUiC_ServersList.EnumServerLists.Regular, func, eserverFilterType, num, 0, flag2, null);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<XUiC_ServerBrowserGamePrefSelector.GameOptionValue> controlCombo;

	[PublicizedFrom(EAccessModifier.Private)]
	public int[] valuesFromXml;

	[PublicizedFrom(EAccessModifier.Private)]
	public string[] namesFromXml;

	[PublicizedFrom(EAccessModifier.Private)]
	public string valueLocalizationPrefixFromXml;

	[PublicizedFrom(EAccessModifier.Private)]
	public GamePrefs.EnumType valueType;

	[PublicizedFrom(EAccessModifier.Private)]
	public GameInfoBool gameInfoBool;

	[PublicizedFrom(EAccessModifier.Private)]
	public GameInfoInt gameInfoInt;

	[PublicizedFrom(EAccessModifier.Private)]
	public Func<int, int> valuePreDisplayModifierFunc;

	public Action<XUiC_ServerBrowserGamePrefSelector> OnValueChanged;

	public struct GameOptionValue
	{
		public GameOptionValue(int _value, string _displayName)
		{
			this.Value = _value;
			this.displayName = _displayName;
		}

		public override string ToString()
		{
			return this.displayName;
		}

		public readonly int Value;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly string displayName;
	}
}
