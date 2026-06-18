using System;

namespace SandboxOptions
{
	public class BaseSandboxOption
	{
		public virtual BaseSandboxOption.OptionTypes OptionType
		{
			get
			{
				return BaseSandboxOption.OptionTypes.Invalid;
			}
		}

		public string OptionNameText
		{
			get
			{
				if (this.optionNameText == null)
				{
					if (this.OverrideOptionName == null)
					{
						string text = "go" + this.Option.ToString();
						this.optionNameText = (Localization.Exists(text, false) ? Localization.Get(text, false, null) : ("*" + this.OptionName));
					}
					else
					{
						this.optionNameText = (Localization.Exists(this.OverrideOptionName, false) ? Localization.Get(this.OverrideOptionName, false, null) : "");
					}
				}
				return this.optionNameText;
			}
		}

		public string DescriptionText
		{
			get
			{
				if (this.descriptionText == null)
				{
					if (this.OverrideDescriptionName == null)
					{
						string text = "go" + this.Option.ToString() + "Desc";
						this.descriptionText = (Localization.Exists(text, false) ? Localization.Get(text, false, null) : "");
					}
					else
					{
						this.descriptionText = (Localization.Exists(this.OverrideDescriptionName, false) ? Localization.Get(this.OverrideDescriptionName, false, null) : "");
					}
				}
				return this.descriptionText;
			}
		}

		public SandboxOptionValueSet ValueOptions
		{
			get; [PublicizedFrom(EAccessModifier.Private)]
			set;
		}

		public virtual string ValueText
		{
			get
			{
				return "";
			}
		}

		public BaseSandboxOption(SandboxOptions option, string name, string categoryName, string valueSetName, bool newUISection)
		{
			this.Option = option;
			this.OptionName = name;
			this.CategoryName = categoryName;
			this.ValueSetName = valueSetName;
			this.NewUISection = newUISection;
		}

		public virtual bool GetBoolValue()
		{
			return false;
		}

		public virtual float GetFloatValue()
		{
			return 0f;
		}

		public virtual int GetIntValue()
		{
			return 0;
		}

		public SandboxOptionValueSet GetValueSet()
		{
			if (this.ValueOptions == null)
			{
				if (!SandboxOptionManager.Current.ValueSets.ContainsKey(this.ValueSetName))
				{
					throw new Exception("BaseSandboxOption: ValueOption " + this.ValueSetName + " does not exist.");
				}
				this.ValueOptions = SandboxOptionManager.Current.ValueSets[this.ValueSetName];
			}
			return this.ValueOptions;
		}

		public virtual void SetValueFromIndex(int index)
		{
		}

		public virtual void SetValue(string value)
		{
		}

		public virtual void SetFloat(float value)
		{
		}

		public virtual void SetInt(int value)
		{
		}

		public virtual void SetBool(bool value)
		{
		}

		public virtual bool IsChanged()
		{
			return false;
		}

		public virtual object GetDefaultValue()
		{
			return null;
		}

		public virtual float GetDefaultFloatValue()
		{
			return -1f;
		}

		public virtual int GetDefaultIntValue()
		{
			return -1;
		}

		public virtual bool GetDefaultBoolValue()
		{
			return false;
		}

		public virtual int GetDefaultIndex()
		{
			return -1;
		}

		public virtual void SetToDefault()
		{
		}

		public virtual string GetDefaultValueText()
		{
			return this.GetValueSet().GetDisplayAtIndex(this.GetDefaultIndex());
		}

		public virtual string GetValueTextFromIndex(int index)
		{
			return this.GetValueSet().GetDisplayAtIndex(index);
		}

		public virtual string GetValueText()
		{
			return "";
		}

		public virtual int GetValueIndex()
		{
			return 0;
		}

		public virtual int GetValueIndex(string value)
		{
			return 0;
		}

		public string OptionName;

		[PublicizedFrom(EAccessModifier.Private)]
		public string optionNameText;

		public string OverrideOptionName;

		public string DisabledByText = "";

		public bool IsEnabled = true;

		[PublicizedFrom(EAccessModifier.Private)]
		public string descriptionText;

		public string OverrideDescriptionName;

		[PublicizedFrom(EAccessModifier.Private)]
		public string ValueSetName = "";

		public SandboxOptions Option;

		public string CategoryName = "";

		public bool NewUISection;

		public enum OptionTypes
		{
			Invalid,
			Int,
			Float,
			String,
			Bool
		}
	}
}
