using System;

public interface ILaunchPref
{
	string Name { get; }

	bool TrySet(string stringRepresentation);
}
