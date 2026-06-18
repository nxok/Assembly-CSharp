using System;

[Flags]
public enum EntityFlags : uint
{
	None = 0U,
	Player = 1U,
	Zombie = 2U,
	Animal = 4U,
	Bandit = 8U,
	Edible = 16U,
	All = 4294967295U,
	AIHearing = 14U,
	AISmelling = 6U
}
