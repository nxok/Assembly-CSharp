using System;

public enum TileEntityType : byte
{
	None,
	Collector = 3,
	VendingMachine = 7,
	Forge,
	Campfire,
	Workstation = 12,
	Powered = 15,
	PowerSource,
	PowerRangeTrap,
	Light,
	Trigger,
	Sleeper,
	PowerMeleeTrap,
	Composite = 25,
	Taskboard = 27,
	LandClaim = 4,
	Loot,
	Trader,
	SecureLoot = 10,
	SecureDoor,
	Sign = 13,
	GoreBlock,
	SecureLootSigned = 22
}
