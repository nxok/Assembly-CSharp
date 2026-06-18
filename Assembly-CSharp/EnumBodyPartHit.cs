using System;

[Flags]
public enum EnumBodyPartHit
{
	None = 0,
	Torso = 1,
	Head = 2,
	LeftUpperArm = 4,
	RightUpperArm = 8,
	LeftUpperLeg = 16,
	RightUpperLeg = 32,
	LeftLowerArm = 64,
	RightLowerArm = 128,
	LeftLowerLeg = 256,
	RightLowerLeg = 512,
	Special = 1024,
	UpperArms = 12,
	LowerArms = 192,
	Arms = 204,
	UpperLegs = 48,
	LowerLegs = 768,
	Legs = 816,
	BitsUsed = 11
}
