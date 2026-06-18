using System;

namespace SandboxOptions
{
	public class SandboxOptionValueSetFloat : SandboxOptionValueSet
	{
		public override void Init()
		{
			string[] displayValues = this.DisplayValues;
			this.DisplayValues = new string[this.FloatValues.Length];
			string text = ((this.DisplayFormat != null) ? Localization.Get(this.DisplayFormat, false, null) : "{0}");
			for (int i = 0; i < this.FloatValues.Length; i++)
			{
				string text2 = "";
				if (this.AlternateDisplayValues != null && this.AlternateDisplayValues.Length > i)
				{
					text2 = this.AlternateDisplayValues[i];
				}
				if (displayValues != null && displayValues.Length > i)
				{
					this.DisplayValues[i] = string.Format(Localization.Get(displayValues[i], false, null), (text2 != "") ? text2 : (this.FloatValues[i] * 100f));
				}
				else
				{
					this.DisplayValues[i] = string.Format(text, (text2 != "") ? text2 : (this.FloatValues[i] * 100f));
				}
			}
		}

		public override bool GetFloatValue(int index, out float val)
		{
			if (index >= 0 && index < this.FloatValues.Length)
			{
				val = this.FloatValues[index];
				return true;
			}
			val = 0f;
			return false;
		}

		public override int GetFloatIndex(float val)
		{
			for (int i = 0; i < this.FloatValues.Length; i++)
			{
				if (val == this.FloatValues[i])
				{
					return i;
				}
			}
			return -1;
		}

		public override bool IsValidIndex(int index)
		{
			return index >= 0 && index < this.FloatValues.Length;
		}

		public override string GetValueListString()
		{
			return string.Join<float>(", ", this.FloatValues);
		}

		public float[] FloatValues;
	}
}
