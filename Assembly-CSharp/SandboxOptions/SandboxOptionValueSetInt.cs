using System;

namespace SandboxOptions
{
	public class SandboxOptionValueSetInt : SandboxOptionValueSet
	{
		public override void Init()
		{
			string[] displayValues = this.DisplayValues;
			this.DisplayValues = new string[this.IntValues.Length];
			string text = ((this.DisplayFormat != null) ? Localization.Get(this.DisplayFormat, false, null) : "{0}");
			for (int i = 0; i < this.IntValues.Length; i++)
			{
				string text2 = "";
				if (this.AlternateDisplayValues != null && this.AlternateDisplayValues.Length > i)
				{
					text2 = this.AlternateDisplayValues[i];
				}
				if (displayValues != null && displayValues.Length > i)
				{
					this.DisplayValues[i] = string.Format(Localization.Get(displayValues[i], false, null), (text2 != "") ? text2 : this.IntValues[i]);
				}
				else
				{
					this.DisplayValues[i] = string.Format(text, (text2 != "") ? text2 : this.IntValues[i]);
				}
			}
		}

		public override bool GetIntValue(int index, out int val)
		{
			if (index >= 0 && index < this.IntValues.Length)
			{
				val = this.IntValues[index];
				return true;
			}
			val = 0;
			return false;
		}

		public override int GetIntIndex(int val)
		{
			for (int i = 0; i < this.IntValues.Length; i++)
			{
				if (val == this.IntValues[i])
				{
					return i;
				}
			}
			return -1;
		}

		public override bool IsValidIndex(int index)
		{
			return index >= 0 && index < this.IntValues.Length;
		}

		public override string GetValueListString()
		{
			return string.Join<int>(", ", this.IntValues);
		}

		public int[] IntValues;
	}
}
