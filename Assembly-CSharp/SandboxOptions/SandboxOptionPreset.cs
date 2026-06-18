using System;
using System.Collections.Generic;
using System.Text;

namespace SandboxOptions
{
	public class SandboxOptionPreset
	{
		public string DisplayName
		{
			get
			{
				if (string.IsNullOrEmpty(this.LocalizedName))
				{
					return this.Name;
				}
				return Localization.Get(this.LocalizedName, false, null);
			}
		}

		public string DisplayDescription
		{
			get
			{
				if (string.IsNullOrEmpty(this.DescriptionKey))
				{
					return this.Description;
				}
				return Localization.Get(this.DescriptionKey, false, null);
			}
		}

		public string SandboxCode
		{
			get
			{
				return this.saveOptionsToCode();
			}
		}

		public bool ForceShowOption(SandboxOptions option)
		{
			return this.AlwaysShow != null && this.AlwaysShow.Contains(option);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public string saveOptionsToCode()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(SandboxOptionManager.currentVersion);
			foreach (SandboxOptions sandboxOptions in this.PresetValues.Keys)
			{
				stringBuilder.Append(string.Format("{0}{1}", SandboxOptionManager.IndexToAlpha2((int)sandboxOptions), SandboxOptionManager.IndexToAlpha(this.PresetValues[sandboxOptions])));
			}
			return stringBuilder.ToString();
		}

		public string Name;

		public string LocalizedName;

		public string Description = "";

		public string DescriptionKey = "";

		public string Icon = "";

		public string Group = "";

		public List<SandboxOptions> AlwaysShow;

		public bool IsDefault;

		public bool IsUserPreset;

		public bool IsModded;

		public bool IsCustomPreset;

		public short DifficultyRating;

		public readonly Dictionary<SandboxOptions, int> PresetValues = new Dictionary<SandboxOptions, int>();
	}
}
