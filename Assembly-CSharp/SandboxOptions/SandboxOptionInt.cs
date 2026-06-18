using System;

namespace SandboxOptions
{
	public class SandboxOptionInt : BaseSandboxOption
	{
		public override BaseSandboxOption.OptionTypes OptionType
		{
			get
			{
				return BaseSandboxOption.OptionTypes.Int;
			}
		}

		public SandboxOptionInt(SandboxOptions option, string optionName, string categoryName, string valueSetName, int defaultValue, bool newUISection = false, SandboxOptionInt.DisabledOptionsOnValue disabledOptions = null)
			: base(option, optionName, categoryName, valueSetName, newUISection)
		{
			this.DefaultValue = defaultValue;
			this.DisabledOptions = disabledOptions;
		}

		public override int GetIntValue()
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

		public override float GetFloatValue()
		{
			return (float)this.CurrentValue / 100f;
		}

		public override void SetValue(string value)
		{
			int num;
			if (StringParsers.TryParseSInt32(value, out num) && base.ValueOptions.GetIntIndex(num) != -1)
			{
				this.CurrentValue = num;
			}
		}

		public override void SetInt(int value)
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

		public override int GetDefaultIntValue()
		{
			return this.DefaultValue;
		}

		public override int GetDefaultIndex()
		{
			return base.GetValueSet().GetIntIndex(this.DefaultValue);
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
			return base.GetValueSet().GetIntIndex(this.CurrentValue);
		}

		public override int GetValueIndex(string value)
		{
			int num;
			if (StringParsers.TryParseSInt32(value, out num))
			{
				return base.GetValueSet().GetIntIndex(num);
			}
			return base.GetValueSet().GetIntIndex(this.DefaultValue);
		}

		public override void SetValueFromIndex(int index)
		{
			if (!base.GetValueSet().GetIntValue(index, out this.CurrentValue))
			{
				this.CurrentValue = this.DefaultValue;
			}
		}

		public int DefaultValue;

		public int CurrentValue = 1;

		public SandboxOptionInt.DisabledOptionsOnValue DisabledOptions;

		public class DisabledOptionsOnValue
		{
			public DisabledOptionsOnValue(SandboxOptions[] options, int value, bool inverted = false)
			{
				this.DisabledOptions = options;
				this.Value = value;
				this.Inverted = inverted;
			}

			public int Value;

			public bool Inverted;

			public SandboxOptions[] DisabledOptions;
		}
	}
}
