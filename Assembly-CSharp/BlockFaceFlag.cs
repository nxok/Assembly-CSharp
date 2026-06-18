using System;

[Flags]
public enum BlockFaceFlag
{
	None = 0,
	Top = 1,
	Bottom = 2,
	North = 4,
	West = 8,
	South = 16,
	East = 32,
	All = 63,
	Solid = 63,
	Axials = 60
}
