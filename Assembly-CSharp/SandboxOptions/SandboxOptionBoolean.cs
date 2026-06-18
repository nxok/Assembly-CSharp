using System;

namespace SandboxOptions
{
	public class SandboxOptionBoolean : BaseSandboxOption
	{
		public override BaseSandboxOption.OptionTypes OptionType
		{
			get
			{
				return BaseSandboxOption.OptionTypes.Bool;
			}
		}

		public SandboxOptionBoolean(SandboxOptions option, string optionName, string categoryName, string valueSetName, bool defaultValue, bool newUISection = false, SandboxOptionBoolean.DisabledOptionsOnValue disabledOptions = null)
			: base(option, optionName, categoryName, valueSetName, newUISection)
		{
			this.DefaultValue = defaultValue;
			this.CurrentValue = defaultValue;
			this.DisabledOptions = disabledOptions;
		}

		public override bool GetBoolValue()
		{
			return this.CurrentValue;
		}

		public override string ValueText
		{
			get
			{
				return this.CurrentValue.ToString();
			}
		}

		public override void SetValue(string value)
		{
			bool flag;
			if (StringParsers.TryParseBool(value, out flag))
			{
				this.CurrentValue = flag;
			}
		}

		public override void SetBool(bool value)
		{
			this.CurrentValue = value;
		}

		public override bool IsChanged()
		{
			return this.CurrentValue != this.DefaultValue;
		}

		public override object GetDefaultValue()
		{
			return this.DefaultValue;
		}

		public override bool GetDefaultBoolValue()
		{
			return this.DefaultValue;
		}

		public override int GetDefaultIndex()
		{
			return base.GetValueSet().GetBoolIndex(this.DefaultValue);
		}

		public override void SetToDefault()
		{
			this.CurrentValue = this.DefaultValue;
		}

		public override string GetValueText()
		{
			return string.Format("{0} | default:{1}", this.CurrentValue, this.DefaultValue);
		}

		public override int GetValueIndex()
		{
			return base.GetValueSet().GetBoolIndex(this.CurrentValue);
		}

		public override int GetValueIndex(string value)
		{
			bool flag;
			if (StringParsers.TryParseBool(value, out flag))
			{
				return base.GetValueSet().GetBoolIndex(flag);
			}
			return base.GetValueSet().GetBoolIndex(this.DefaultValue);
		}

		public override void SetValueFromIndex(int index)
		{
			if (!base.GetValueSet().GetBoolValue(index, out this.CurrentValue))
			{
				this.CurrentValue = this.DefaultValue;
			}
		}

		public bool DefaultValue;

		public bool CurrentValue;

		public SandboxOptionBoolean.DisabledOptionsOnValue DisabledOptions;

		public class DisabledOptionsOnValue
		{
			public DisabledOptionsOnValue(SandboxOptions[] options, bool value, bool inverted = false)
			{
				this.DisabledOptions = options;
				this.Value = value;
				this.Inverted = inverted;
			}

			public bool Value;

			public bool Inverted;

			public SandboxOptions[] DisabledOptions;
		}
	}
}
