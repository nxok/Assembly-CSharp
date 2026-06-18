using System;

[Flags]
public enum ChunkProtectionLevel
{
	None = 0,
	NearOfflinePlayer = 1,
	NearBedroll = 2,
	NearSupplyCrate = 4,
	NearQuestObjective = 8,
	NearDroppedBackpack = 16,
	NearVehicle = 32,
	NearLandClaim = 64,
	OfflinePlayer = 128,
	Bedroll = 256,
	SupplyCrate = 512,
	QuestObjective = 1024,
	DroppedBackpack = 2048,
	Trader = 4096,
	Drone = 8192,
	Vehicle = 16384,
	LandClaim = 32768,
	CurrentlySynced = 65536,
	All = -1
}
