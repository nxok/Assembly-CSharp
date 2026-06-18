using System;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Rendering;

public static class MeshDataUtils
{
	public static void CopyInterleaved(int elementSize, int offset, int stride, ReadOnlySpan<byte> src, Span<byte> dest)
	{
		int num = 0;
		int num2 = offset;
		while (num + elementSize <= src.Length && num2 + stride - offset <= dest.Length)
		{
			src.Slice(num, elementSize).CopyTo(dest.Slice(num2, elementSize));
			num += elementSize;
			num2 += stride;
		}
	}

	public static MeshDataLayout SetAttributes(Mesh.MeshData meshData, ReadOnlySpan<Vector3> vertices, ReadOnlySpan<int> indices, ReadOnlySpan<Vector3> normals, ReadOnlySpan<Vector4> tangents, ReadOnlySpan<Color> colors, ReadOnlySpan<Vector2> texCoord0s, ReadOnlySpan<Vector2> texCoord1s, NativeList<VertexAttributeDescriptor> vertexAttributesOut)
	{
		int num = 0;
		MeshDataLayout meshDataLayout;
		meshDataLayout.PositionSize = 12;
		meshDataLayout.PositionOffset = num;
		num += 12;
		VertexAttributeDescriptor vertexAttributeDescriptor = new VertexAttributeDescriptor(0, 0, 3, 0);
		vertexAttributesOut.Add(ref vertexAttributeDescriptor);
		if (normals.Length > 0)
		{
			meshDataLayout.NormalSize = 12;
			meshDataLayout.NormalOffset = num;
			num += 12;
			vertexAttributeDescriptor = new VertexAttributeDescriptor(1, 0, 3, 0);
			vertexAttributesOut.Add(ref vertexAttributeDescriptor);
		}
		else
		{
			meshDataLayout.NormalSize = -1;
			meshDataLayout.NormalOffset = -1;
		}
		if (tangents.Length > 0)
		{
			meshDataLayout.TangentSize = 16;
			meshDataLayout.TangentOffset = num;
			num += 16;
			vertexAttributeDescriptor = new VertexAttributeDescriptor(2, 0, 4, 0);
			vertexAttributesOut.Add(ref vertexAttributeDescriptor);
		}
		else
		{
			meshDataLayout.TangentSize = -1;
			meshDataLayout.TangentOffset = -1;
		}
		if (colors.Length > 0)
		{
			meshDataLayout.ColorSize = 16;
			meshDataLayout.ColorOffset = num;
			num += 16;
			vertexAttributeDescriptor = new VertexAttributeDescriptor(3, 0, 4, 0);
			vertexAttributesOut.Add(ref vertexAttributeDescriptor);
		}
		else
		{
			meshDataLayout.ColorSize = -1;
			meshDataLayout.ColorOffset = -1;
		}
		if (texCoord0s.Length > 0)
		{
			meshDataLayout.TexCoord0Size = 8;
			meshDataLayout.TexCoord0Offset = num;
			num += 8;
			vertexAttributeDescriptor = new VertexAttributeDescriptor(4, 0, 2, 0);
			vertexAttributesOut.Add(ref vertexAttributeDescriptor);
		}
		else
		{
			meshDataLayout.TexCoord0Size = -1;
			meshDataLayout.TexCoord0Offset = -1;
		}
		if (texCoord1s.Length > 0)
		{
			meshDataLayout.TexCoord1Size = 8;
			meshDataLayout.TexCoord1Offset = num;
			num += 8;
			vertexAttributeDescriptor = new VertexAttributeDescriptor(5, 0, 2, 0);
			vertexAttributesOut.Add(ref vertexAttributeDescriptor);
		}
		else
		{
			meshDataLayout.TexCoord1Size = -1;
			meshDataLayout.TexCoord1Offset = -1;
		}
		meshData.SetVertexBufferParams(vertices.Length, vertexAttributesOut.AsArray());
		meshDataLayout.Stride = num;
		meshData.SetIndexBufferParams(indices.Length, 1);
		return meshDataLayout;
	}

	public static void SetSubmesh(Mesh.MeshData meshData, int vertexCount, int indexCount, Bounds bounds)
	{
		meshData.subMeshCount = 1;
		int num = 0;
		SubMeshDescriptor subMeshDescriptor;
		subMeshDescriptor..ctor(0, indexCount, 0);
		subMeshDescriptor.bounds = bounds;
		subMeshDescriptor.vertexCount = vertexCount;
		meshData.SetSubMesh(num, subMeshDescriptor, 15);
	}

	public unsafe static Bounds RecalculateBounds(ReadOnlySpan<Vector3> vertices)
	{
		Bounds bounds;
		bounds..ctor(*vertices[0], Vector3.zero);
		for (int i = 1; i < vertices.Length; i++)
		{
			bounds.Encapsulate(*vertices[i]);
		}
		return bounds;
	}

	public static void ApplyMeshDataCompression(Mesh.MeshData meshData, NativeArray<VertexAttributeDescriptor> vertexAttributes)
	{
		GameUtils.ApplyCompressedVertexAttributes(ref vertexAttributes, false);
		meshData.SetVertexBufferParams(meshData.vertexCount, vertexAttributes);
	}

	public unsafe static void CalculateNormals(ReadOnlySpan<Vector3> vertices, ReadOnlySpan<int> indices, Span<Vector3> normals)
	{
		int num = Math.Min(vertices.Length, normals.Length);
		for (int i = 0; i < normals.Length; i++)
		{
			*normals[i] = Vector3.zero;
		}
		Allocator allocator = (ThreadManager.IsMainThread() ? 2 : 4);
		using (NativeParallelMultiHashMap<Vector3, int> nativeParallelMultiHashMap = new NativeParallelMultiHashMap<Vector3, int>(num, allocator))
		{
			for (int j = 0; j < num; j++)
			{
				nativeParallelMultiHashMap.Add(*vertices[j], j);
			}
			using (NativeArray<int> nativeArray = new NativeArray<int>(num, allocator, 1))
			{
				int num2 = 0;
				while (num2 + 2 < indices.Length)
				{
					int num3 = *indices[num2];
					int num4 = *indices[num2 + 1];
					int num5 = *indices[num2 + 2];
					Vector3 vector = *vertices[num3];
					Vector3 vector2 = *vertices[num4];
					Vector3 vector3 = *vertices[num5];
					Vector3 vector4 = Vector3.Cross(vector2 - vector, vector3 - vector);
					vector4.Normalize();
					MeshDataUtils.AddTriangleNormal(ref normals, nativeParallelMultiHashMap, nativeArray, vector, vector4);
					MeshDataUtils.AddTriangleNormal(ref normals, nativeParallelMultiHashMap, nativeArray, vector2, vector4);
					MeshDataUtils.AddTriangleNormal(ref normals, nativeParallelMultiHashMap, nativeArray, vector3, vector4);
					num2 += 3;
				}
				for (int k = 0; k < normals.Length; k++)
				{
					int num6 = nativeArray[k];
					if (num6 > 0)
					{
						*normals[k] /= (float)num6;
					}
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe static void AddTriangleNormal(ref Span<Vector3> normals, NativeParallelMultiHashMap<Vector3, int> vertexToIndices, NativeArray<int> normalsAdded, Vector3 vertex, Vector3 normal)
	{
		foreach (int num in vertexToIndices.GetValuesForKey(vertex))
		{
			*normals[num] += normal;
			int num2 = num;
			int num3 = normalsAdded[num2];
			normalsAdded[num2] = num3 + 1;
		}
	}

	public unsafe static AtomicSafeHandleScope CreateNativeArray<[IsUnmanaged] T>(T* ptr, int length, out NativeArray<T> array) where T : struct, ValueType
	{
		array = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<T>((void*)ptr, length, 1);
		return default(AtomicSafeHandleScope);
	}

	public const MeshUpdateFlags DoNothing = 15;
}
