using System;

namespace SandboxOptions
{
	public class SandboxOptionValueSetBool : SandboxOptionValueSet
	{
		public override void Init()
		{
			for (int i = 0; i < this.DisplayValues.Length; i++)
			{
				this.DisplayValues[i] = Localization.Get(this.DisplayValues[i], false, null);
			}
		}

		public override bool GetBoolValue(int index, out bool val)
		{
			if (index >= 0 && index < this.BoolValues.Length)
			{
				val = this.BoolValues[index];
				return true;
			}
			val = false;
			return false;
		}

		public override bool GetBoolValue(int index)
		{
			return this.BoolValues[index];
		}

		public override int GetBoolIndex(bool val)
		{
			for (int i = 0; i < this.BoolValues.Length; i++)
			{
				if (val == this.BoolValues[i])
				{
					return i;
				}
			}
			return -1;
		}

		public override bool IsValidIndex(int index)
		{
			return index >= 0 && index < this.BoolValues.Length;
		}

		public override string GetValueListString()
		{
			return string.Join<bool>(", ", this.BoolValues);
		}

		public bool[] BoolValues;
	}
}
