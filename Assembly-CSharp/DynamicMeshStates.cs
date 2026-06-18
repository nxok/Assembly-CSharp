using System;

[Flags]
public enum DynamicMeshStates : short
{
	None = 0,
	ThreadUpdating = 1,
	SaveRequired = 2,
	LoadRequired = 4,
	UnloadMark1 = 8,
	UnloadMark2 = 16,
	UnloadMark3 = 32,
	MarkedForDelete = 64,
	LoadBoosted = 128,
	MainThreadLoadRequest = 256,
	FileMissing = 512,
	Generating = 1024
}
