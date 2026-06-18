using System;
using System.Collections.Generic;
using System.Text;
using GamePath;
using UnityEngine;

public class MemoryPools
{
	public static void InitStatic(bool _usePools)
	{
		_usePools = true;
		MemoryPools.PoolChunks = new MemoryPooledObject<Chunk>(_usePools ? 1000 : 0);
		MemoryPools.poolCBL = new MemoryPooledObject<ChunkBlockLayer>(_usePools ? 50000 : 0);
		MemoryPools.poolVML = new MemoryPooledObject<VoxelMeshLayer>(_usePools ? 1000 : 0);
		MemoryPools.poolCGOL = new MemoryPooledObject<ChunkGameObjectLayer>(_usePools ? 1000 : 0);
		MemoryPools.poolMS = new MemoryPooledObject<PooledMemoryStream>(_usePools ? 40 : 0);
		MemoryPools.poolCBC = new MemoryPooledObject<CBCLayer>(_usePools ? 50000 : 0);
	}

	public static void Cleanup()
	{
		MemoryPools.PoolChunks.Cleanup();
		MemoryPools.poolCBL.Cleanup();
		MemoryPools.poolVML.Cleanup();
		MemoryPools.poolCGOL.Cleanup();
		MemoryPools.poolMS.Cleanup();
		MemoryPools.poolCBC.Cleanup();
		MemoryPools.poolVector3.FreeAll();
		MemoryPools.poolVector4.FreeAll();
		MemoryPools.poolVector2.FreeAll();
		MemoryPools.poolInt.FreeAll();
		MemoryPools.poolColor.FreeAll();
		MemoryPools.poolByte.FreeAll();
		List<byte[]> list = MemoryPools.poolCBLUpper24BitArrCache;
		lock (list)
		{
			MemoryPools.poolCBLUpper24BitArrCache.Clear();
		}
		list = MemoryPools.poolCBLLower8BitArrCache;
		lock (list)
		{
			MemoryPools.poolCBLLower8BitArrCache.Clear();
		}
	}

	public static string GetDebugInfo()
	{
		return string.Format("Chunks:{0}/{1} CBL:{2}/{3} CBC:{4}/{5} CGL:{6}/{7} VML:{8}/{9} MS:{10}/{11}", new object[]
		{
			Chunk.InstanceCount,
			MemoryPools.PoolChunks.GetPoolSize(),
			ChunkBlockLayer.InstanceCount,
			MemoryPools.poolCBL.GetPoolSize(),
			CBCLayer.InstanceCount,
			MemoryPools.poolCBC.GetPoolSize(),
			ChunkGameObjectLayer.InstanceCount,
			MemoryPools.poolCGOL.GetPoolSize(),
			VoxelMeshLayer.InstanceCount,
			MemoryPools.poolVML.GetPoolSize(),
			PooledMemoryStream.InstanceCount,
			MemoryPools.poolMS.GetPoolSize()
		});
	}

	public static string GetDebugInfoEx()
	{
		int count = MemoryPools.poolVector2.GetCount();
		int count2 = MemoryPools.poolVector3.GetCount();
		int count3 = MemoryPools.poolVector4.GetCount();
		int count4 = MemoryPools.poolInt.GetCount();
		int count5 = MemoryPools.poolUInt16.GetCount();
		int count6 = MemoryPools.poolFloat.GetCount();
		int count7 = MemoryPools.poolColor.GetCount();
		int count8 = MemoryPools.poolByte.GetCount();
		int count9 = MemoryPools.poolCBLUpper24BitArrCache.Count;
		int count10 = MemoryPools.poolCBLLower8BitArrCache.Count;
		long num = MemoryPools.poolVector2.GetElementsCount() * (long)MemoryTracker.GetSize<Vector2>() + MemoryPools.poolVector3.GetElementsCount() * (long)MemoryTracker.GetSize<Vector3>() + MemoryPools.poolVector4.GetElementsCount() * (long)MemoryTracker.GetSize<Vector4>() + MemoryPools.poolInt.GetElementsCount() * (long)MemoryTracker.GetSize<int>() + MemoryPools.poolUInt16.GetElementsCount() * (long)MemoryTracker.GetSize<ushort>() + MemoryPools.poolFloat.GetElementsCount() * (long)MemoryTracker.GetSize<float>() + MemoryPools.poolColor.GetElementsCount() * (long)MemoryTracker.GetSize<Color>() + MemoryPools.poolByte.GetElementsCount() + (long)(count9 * 1024 * 3) + (long)(count10 * 1024);
		int num2 = MemoryPools.PoolChunks.GetPoolSize() + MemoryPools.poolCBL.GetPoolSize() + MemoryPools.poolCBC.GetPoolSize() + MemoryPools.poolCGOL.GetPoolSize() + MemoryPools.poolVML.GetPoolSize() + MemoryPools.poolMS.GetPoolSize();
		int num3 = Chunk.InstanceCount + ChunkBlockLayer.InstanceCount + CBCLayer.InstanceCount + ChunkGameObjectLayer.InstanceCount + VoxelMeshLayer.InstanceCount + PooledMemoryStream.InstanceCount;
		return string.Format("V2/V3/V4/C:{0}/{1}/{2}/{3} I/UI/F/B:{4}/{5}/{6}/{7} 24/8:{8}/{9} pools={10} inst={11} pooled arrays mem={12}", new object[]
		{
			count,
			count2,
			count3,
			count7,
			count4,
			count5,
			count6,
			count8,
			count9,
			count10,
			num2,
			num3,
			MetricConversion.ToShortestBytesString(num)
		});
	}

	public static string GetDebugInfoArrays()
	{
		StringBuilder stringBuilder = new StringBuilder();
		MemoryPools.AppendDebugInfoArray<Vector2>(stringBuilder, MemoryPools.poolVector2);
		stringBuilder.AppendLine();
		MemoryPools.AppendDebugInfoArray<Vector3>(stringBuilder, MemoryPools.poolVector3);
		stringBuilder.AppendLine();
		MemoryPools.AppendDebugInfoArray<Vector4>(stringBuilder, MemoryPools.poolVector4);
		stringBuilder.AppendLine();
		MemoryPools.AppendDebugInfoArray<Color>(stringBuilder, MemoryPools.poolColor);
		stringBuilder.AppendLine();
		MemoryPools.AppendDebugInfoArray<int>(stringBuilder, MemoryPools.poolInt);
		stringBuilder.AppendLine();
		MemoryPools.AppendDebugInfoArray<byte>(stringBuilder, MemoryPools.poolByte);
		stringBuilder.AppendLine();
		MemoryPools.AppendDebugInfoArray<ushort>(stringBuilder, MemoryPools.poolUInt16);
		stringBuilder.AppendLine();
		MemoryPools.AppendDebugInfoArray<float>(stringBuilder, MemoryPools.poolFloat);
		stringBuilder.AppendLine();
		return stringBuilder.ToString();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void AppendDebugInfoArray<T>(StringBuilder _builder, MemoryPooledArray<T> _pool) where T : new()
	{
		_builder.AppendLine(typeof(T).Name);
		long num = 0L;
		long num2 = (long)MemoryTracker.GetSize<T>();
		for (int i = 0; i < MemoryPooledArraySizes.poolElements.Length; i++)
		{
			int count = _pool.GetCount(i);
			_builder.Append(MetricConversion.ToShortestBytesString((long)MemoryPooledArraySizes.poolElements[i] * num2));
			_builder.Append(": ");
			_builder.Append(count);
			if (i < MemoryPooledArraySizes.poolElements.Length - 1)
			{
				_builder.Append(", ");
			}
			num += (long)(count * MemoryPooledArraySizes.poolElements[i]);
		}
		_builder.AppendLine();
		_builder.Append("Total: ");
		_builder.AppendLine(MetricConversion.ToShortestBytesString(num * num2));
	}

	public const int VMLPoolSize = 1000;

	public static MemoryPooledObject<Chunk> PoolChunks = new MemoryPooledObject<Chunk>(0);

	public static MemoryPooledObject<ChunkBlockLayer> poolCBL = new MemoryPooledObject<ChunkBlockLayer>(0);

	public static MemoryPooledObject<VoxelMeshLayer> poolVML = new MemoryPooledObject<VoxelMeshLayer>(0);

	public static MemoryPooledObject<ChunkGameObjectLayer> poolCGOL = new MemoryPooledObject<ChunkGameObjectLayer>(0);

	public static MemoryPooledObject<PooledMemoryStream> poolMS = new MemoryPooledObject<PooledMemoryStream>(40);

	public static MemoryPooledObject<CBCLayer> poolCBC = new MemoryPooledObject<CBCLayer>(0);

	public static MemoryPooledArray<Vector3> poolVector3 = new MemoryPooledArray<Vector3>();

	public static MemoryPooledArray<Vector4> poolVector4 = new MemoryPooledArray<Vector4>();

	public static MemoryPooledArray<Vector2> poolVector2 = new MemoryPooledArray<Vector2>();

	public static MemoryPooledArray<int> poolInt = new MemoryPooledArray<int>();

	public static MemoryPooledArray<ushort> poolUInt16 = new MemoryPooledArray<ushort>();

	public static MemoryPooledArray<float> poolFloat = new MemoryPooledArray<float>();

	public static MemoryPooledArray<Color> poolColor = new MemoryPooledArray<Color>();

	public static MemoryPooledArray<byte> poolByte = new MemoryPooledArray<byte>();

	public static MemoryPooledObject<PooledExpandableMemoryStream> poolMemoryStream = new MemoryPooledObject<PooledExpandableMemoryStream>(100);

	public static MemoryPooledObject<PooledBinaryReader> poolBinaryReader = new MemoryPooledObject<PooledBinaryReader>(100);

	public static MemoryPooledObject<PooledBinaryWriter> poolBinaryWriter = new MemoryPooledObject<PooledBinaryWriter>(100);

	public static MemoryPooledObject<NameIdMapping> poolNameIdMapping = new MemoryPooledObject<NameIdMapping>(10);

	public static List<byte[]> poolCBLUpper24BitArrCache = new List<byte[]>();

	public static List<byte[]> poolCBLLower8BitArrCache = new List<byte[]>();

	public static DynamicObjectPool<PathPoint> s_pool = new DynamicObjectPool<PathPoint>(64);
}
