using System;

public static class WaterDebugPools
{
	public static void CreatePools()
	{
		WaterDebugPools.rendererPool = new MemoryPooledObject<WaterDebugRenderer>(250);
		WaterDebugPools.layerPool = new MemoryPooledObject<WaterDebugRendererLayer>(4000);
	}

	public static void Cleanup()
	{
		MemoryPooledObject<WaterDebugRenderer> memoryPooledObject = WaterDebugPools.rendererPool;
		if (memoryPooledObject != null)
		{
			memoryPooledObject.Cleanup();
		}
		WaterDebugPools.rendererPool = null;
		MemoryPooledObject<WaterDebugRendererLayer> memoryPooledObject2 = WaterDebugPools.layerPool;
		if (memoryPooledObject2 != null)
		{
			memoryPooledObject2.Cleanup();
		}
		WaterDebugPools.layerPool = null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const int maxActiveChunks = 250;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int numLayers = 16;

	public static MemoryPooledObject<WaterDebugRenderer> rendererPool;

	public static MemoryPooledObject<WaterDebugRendererLayer> layerPool;
}
