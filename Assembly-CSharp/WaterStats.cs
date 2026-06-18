using System;
using Unity.Collections;

public struct WaterStats
{
	public static WaterStats Sum(NativeArray<WaterStats> array)
	{
		WaterStats waterStats = default(WaterStats);
		for (int i = 0; i < array.Length; i++)
		{
			waterStats += array[i];
		}
		return waterStats;
	}

	public static WaterStats operator +(WaterStats a, WaterStats b)
	{
		return new WaterStats
		{
			NumChunksProcessed = a.NumChunksProcessed + b.NumChunksProcessed,
			NumChunksActive = a.NumChunksActive + b.NumChunksActive,
			NumFlowEvents = a.NumFlowEvents + b.NumFlowEvents,
			NumVoxelsProcessed = a.NumVoxelsProcessed + b.NumVoxelsProcessed,
			NumVoxelsPutToSleep = a.NumVoxelsPutToSleep + b.NumVoxelsPutToSleep,
			NumVoxelsWokeUp = a.NumVoxelsWokeUp + b.NumVoxelsWokeUp
		};
	}

	public void ResetFrame()
	{
		this.NumChunksProcessed = 0;
		this.NumChunksActive = 0;
		this.NumFlowEvents = 0;
		this.NumVoxelsProcessed = 0;
		this.NumVoxelsPutToSleep = 0;
		this.NumVoxelsWokeUp = 0;
	}

	public int NumChunksProcessed;

	public int NumChunksActive;

	public int NumFlowEvents;

	public int NumVoxelsProcessed;

	public int NumVoxelsPutToSleep;

	public int NumVoxelsWokeUp;
}
