using System;

namespace SandboxOptions
{
	public class SandboxOptionFloat : BaseSandboxOption
	{
		public override BaseSandboxOption.OptionTypes OptionType
		{
			get
			{
				return BaseSandboxOption.OptionTypes.Float;
			}
		}

		public SandboxOptionFloat(SandboxOptions option, string optionName, string categoryName, string valueSetName, float defaultValue, bool newUISection = false, SandboxOptionFloat.DisabledOptionsOnValue disabledOptions = null)
			: base(option, optionName, categoryName, valueSetName, newUISection)
		{
			this.DefaultValue = defaultValue;
			this.DisabledOptions = disabledOptions;
		}

		public override float GetFloatValue()
		{
			return this.CurrentValue;
		}

		public override int GetIntValue()
		{
			return (int)(this.CurrentValue * 100f);
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
			float num;
			if (StringParsers.TryParseFloat(value, out num) && base.ValueOptions.GetFloatIndex(num) != -1)
			{
				this.CurrentValue = num;
			}
		}

		public override void SetFloat(float value)
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

		public override float GetDefaultFloatValue()
		{
			return this.DefaultValue;
		}

		public override int GetDefaultIndex()
		{
			return base.GetValueSet().GetFloatIndex(this.DefaultValue);
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
			return base.GetValueSet().GetFloatIndex(this.CurrentValue);
		}

		public override int GetValueIndex(string value)
		{
			float num;
			if (StringParsers.TryParseFloat(value, out num))
			{
				return base.GetValueSet().GetFloatIndex(num);
			}
			return base.GetValueSet().GetFloatIndex(this.DefaultValue);
		}

		public override void SetValueFromIndex(int index)
		{
			if (!base.GetValueSet().GetFloatValue(index, out this.CurrentValue))
			{
				this.CurrentValue = this.DefaultValue;
			}
		}

		public float DefaultValue;

		public float CurrentValue = 1f;

		public bool IsPercent;

		public SandboxOptionFloat.DisabledOptionsOnValue DisabledOptions;

		public class DisabledOptionsOnValue
		{
			public DisabledOptionsOnValue(SandboxOptions[] options, float value, bool inverted = false)
			{
				this.DisabledOptions = options;
				this.Value = value;
				this.Inverted = inverted;
			}

			public float Value;

			public bool Inverted;

			public SandboxOptions[] DisabledOptions;
		}
	}
}
