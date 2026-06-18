using System;
using SandboxOptions;

public struct SandboxPresetInfo : IEquatable<SandboxPresetInfo>
{
	public SandboxPresetInfo(SandboxOptionPreset _preset)
	{
		this.InternalName = _preset.Name;
		this.FormattedName = _preset.DisplayName;
		this.Description = _preset.DisplayDescription;
		this.Icon = _preset.Icon;
		this.Difficulty = (int)_preset.DifficultyRating;
		this.IsUserPreset = _preset.IsUserPreset;
		this.SandboxCode = _preset.SandboxCode;
		this.IsCustomPreset = _preset.IsCustomPreset;
	}

	public override string ToString()
	{
		return this.FormattedName;
	}

	public bool Equals(SandboxPresetInfo _other)
	{
		return this.InternalName == _other.InternalName;
	}

	public override bool Equals(object _obj)
	{
		if (_obj is SandboxPresetInfo)
		{
			SandboxPresetInfo sandboxPresetInfo = (SandboxPresetInfo)_obj;
			return this.Equals(sandboxPresetInfo);
		}
		return false;
	}

	public override int GetHashCode()
	{
		if (this.InternalName == null)
		{
			return 0;
		}
		return this.InternalName.GetHashCode();
	}

	public readonly string InternalName;

	public readonly string FormattedName;

	public readonly string Description;

	public readonly string Icon;

	public readonly int Difficulty;

	public readonly bool IsUserPreset;

	public readonly bool IsCustomPreset;

	public string SandboxCode;
}
