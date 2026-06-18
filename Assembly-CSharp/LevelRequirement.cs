using System;

public struct LevelRequirement
{
	public LevelRequirement(int _level)
	{
		this.Level = _level;
		this.Requirements = null;
	}

	public readonly int Level;

	public RequirementGroup Requirements;
}
