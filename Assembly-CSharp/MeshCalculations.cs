using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

public static class MeshCalculations
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static List<MeshCalculations.VertexEntry> GetCacheBySize(int capacity)
	{
		int num = 1024;
		int num2 = capacity / num + num;
		ConcurrentQueue<List<MeshCalculations.VertexEntry>> concurrentQueue;
		if (!MeshCalculations.Cache.TryGetValue(num2, out concurrentQueue))
		{
			concurrentQueue = new ConcurrentQueue<List<MeshCalculations.VertexEntry>>();
			MeshCalculations.Cache.TryAdd(num2, concurrentQueue);
		}
		List<MeshCalculations.VertexEntry> list;
		if (!concurrentQueue.TryDequeue(out list))
		{
			list = new List<MeshCalculations.VertexEntry>(num);
		}
		return list;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void AddToCacheBySize(List<MeshCalculations.VertexEntry> list)
	{
		int num = 1024;
		int num2 = list.Capacity / num + num;
		if (list.Capacity % num != 0)
		{
			Log.Warning("list not divisible by keysize: " + num.ToString() + " vs " + list.Capacity.ToString());
		}
		ConcurrentQueue<List<MeshCalculations.VertexEntry>> concurrentQueue;
		if (!MeshCalculations.Cache.TryGetValue(num2, out concurrentQueue))
		{
			concurrentQueue = new ConcurrentQueue<List<MeshCalculations.VertexEntry>>();
			MeshCalculations.Cache.TryAdd(num2, concurrentQueue);
		}
		concurrentQueue.Enqueue(list);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static List<MeshCalculations.VertexEntry> GetVertexEntryList()
	{
		List<MeshCalculations.VertexEntry> list;
		if (!MeshCalculations.VertexEntries.TryDequeue(out list))
		{
			list = new List<MeshCalculations.VertexEntry>(4);
		}
		return list;
	}

	public static void CalculateMeshTangents(ArrayListMP<Vector3> _vertices, ArrayListMP<int> _indices, ArrayListMP<Vector3> _normals, ArrayListMP<Vector2> _uvs, ArrayListMP<Vector4> _tangents, bool _bIgnoreUvs = false)
	{
		object @lock = MeshCalculations._lock;
		lock (@lock)
		{
			int count = _indices.Count;
			int count2 = _vertices.Count;
			_tangents.Grow(count2);
			_tangents.Count = count2;
			Vector3[] array = null;
			Vector3[] array2 = null;
			if (array == null || count2 >= array.Length)
			{
				Log.Out("Increasing tangent cache: " + count2.ToString());
				array = new Vector3[count2 + 1];
				array2 = new Vector3[count2 + 1];
			}
			else
			{
				for (int i = 0; i < count2; i++)
				{
					array[i].Set(0f, 0f, 0f);
					array2[i].Set(0f, 0f, 0f);
				}
			}
			for (int j = 0; j < count; j += 3)
			{
				int num = _indices[j];
				int num2 = _indices[j + 1];
				int num3 = _indices[j + 2];
				Vector3 vector = _vertices[num];
				Vector3 vector2 = _vertices[num2];
				Vector3 vector3 = _vertices[num3];
				Vector2 vector4;
				Vector2 vector5;
				Vector2 vector6;
				if (_bIgnoreUvs)
				{
					vector4 = Vector2.zero;
					vector5 = Vector2.zero;
					vector6 = Vector2.zero;
				}
				else
				{
					vector4 = _uvs[num];
					vector5 = _uvs[num2];
					vector6 = _uvs[num3];
				}
				float num4 = vector2.x - vector.x;
				float num5 = vector3.x - vector.x;
				float num6 = vector2.y - vector.y;
				float num7 = vector3.y - vector.y;
				float num8 = vector2.z - vector.z;
				float num9 = vector3.z - vector.z;
				float num10 = vector5.x - vector4.x;
				float num11 = vector6.x - vector4.x;
				float num12 = vector5.y - vector4.y;
				float num13 = vector6.y - vector4.y;
				float num14 = num10 * num13 - num11 * num12;
				float num15 = ((num14 == 0f) ? 0f : (1f / num14));
				float num16 = (num13 * num4 - num12 * num5) * num15;
				float num17 = (num13 * num6 - num12 * num7) * num15;
				float num18 = (num13 * num8 - num12 * num9) * num15;
				float num19 = (num10 * num5 - num11 * num4) * num15;
				float num20 = (num10 * num7 - num11 * num6) * num15;
				float num21 = (num10 * num9 - num11 * num8) * num15;
				Vector3[] array3 = array;
				int num22 = num;
				array3[num22].x = array3[num22].x + num16;
				Vector3[] array4 = array;
				int num23 = num;
				array4[num23].y = array4[num23].y + num17;
				Vector3[] array5 = array;
				int num24 = num;
				array5[num24].z = array5[num24].z + num18;
				Vector3[] array6 = array;
				int num25 = num2;
				array6[num25].x = array6[num25].x + num16;
				Vector3[] array7 = array;
				int num26 = num2;
				array7[num26].y = array7[num26].y + num17;
				Vector3[] array8 = array;
				int num27 = num2;
				array8[num27].z = array8[num27].z + num18;
				Vector3[] array9 = array;
				int num28 = num3;
				array9[num28].x = array9[num28].x + num16;
				Vector3[] array10 = array;
				int num29 = num3;
				array10[num29].y = array10[num29].y + num17;
				Vector3[] array11 = array;
				int num30 = num3;
				array11[num30].z = array11[num30].z + num18;
				Vector3[] array12 = array2;
				int num31 = num;
				array12[num31].x = array12[num31].x + num19;
				Vector3[] array13 = array2;
				int num32 = num;
				array13[num32].y = array13[num32].y + num20;
				Vector3[] array14 = array2;
				int num33 = num;
				array14[num33].z = array14[num33].z + num21;
				Vector3[] array15 = array2;
				int num34 = num2;
				array15[num34].x = array15[num34].x + num19;
				Vector3[] array16 = array2;
				int num35 = num2;
				array16[num35].y = array16[num35].y + num20;
				Vector3[] array17 = array2;
				int num36 = num2;
				array17[num36].z = array17[num36].z + num21;
				Vector3[] array18 = array2;
				int num37 = num3;
				array18[num37].x = array18[num37].x + num19;
				Vector3[] array19 = array2;
				int num38 = num3;
				array19[num38].y = array19[num38].y + num20;
				Vector3[] array20 = array2;
				int num39 = num3;
				array20[num39].z = array20[num39].z + num21;
			}
			for (int k = 0; k < count2; k++)
			{
				Vector3 vector7 = _normals[k];
				Vector3 vector8 = array[k];
				Vector3.OrthoNormalize(ref vector7, ref vector8);
				Vector4 vector9;
				vector9..ctor(vector8.x, vector8.y, vector8.z, (Vector3.Dot(Vector3.Cross(vector7, vector8), array2[k]) < 0f) ? (-1f) : 1f);
				_tangents.Add(vector9);
			}
		}
	}

	public static void CalculateMeshTangents(List<Vector3> _vertices, List<int> _indices, List<Vector3> _normals, List<Vector2> _uvs, List<Vector4> _tangents, bool _bIgnoreUvs = false)
	{
		object @lock = MeshCalculations._lock;
		lock (@lock)
		{
			int count = _indices.Count;
			int count2 = _vertices.Count;
			_tangents.Capacity = Math.Max(_tangents.Capacity, count2);
			_tangents.Clear();
			Vector3[] array = null;
			Vector3[] array2 = null;
			if (array == null || count2 >= array.Length)
			{
				Log.Out("Increasing tangent cache: " + count2.ToString());
				array = new Vector3[count2 + 1];
				array2 = new Vector3[count2 + 1];
			}
			else
			{
				for (int i = 0; i < count2; i++)
				{
					array[i].Set(0f, 0f, 0f);
					array2[i].Set(0f, 0f, 0f);
				}
			}
			for (int j = 0; j < count; j += 3)
			{
				int num = _indices[j];
				int num2 = _indices[j + 1];
				int num3 = _indices[j + 2];
				Vector3 vector = _vertices[num];
				Vector3 vector2 = _vertices[num2];
				Vector3 vector3 = _vertices[num3];
				Vector2 vector4;
				Vector2 vector5;
				Vector2 vector6;
				if (_bIgnoreUvs)
				{
					vector4 = Vector2.zero;
					vector5 = Vector2.zero;
					vector6 = Vector2.zero;
				}
				else
				{
					vector4 = _uvs[num];
					vector5 = _uvs[num2];
					vector6 = _uvs[num3];
				}
				float num4 = vector2.x - vector.x;
				float num5 = vector3.x - vector.x;
				float num6 = vector2.y - vector.y;
				float num7 = vector3.y - vector.y;
				float num8 = vector2.z - vector.z;
				float num9 = vector3.z - vector.z;
				float num10 = vector5.x - vector4.x;
				float num11 = vector6.x - vector4.x;
				float num12 = vector5.y - vector4.y;
				float num13 = vector6.y - vector4.y;
				float num14 = num10 * num13 - num11 * num12;
				float num15 = ((num14 == 0f) ? 0f : (1f / num14));
				float num16 = (num13 * num4 - num12 * num5) * num15;
				float num17 = (num13 * num6 - num12 * num7) * num15;
				float num18 = (num13 * num8 - num12 * num9) * num15;
				float num19 = (num10 * num5 - num11 * num4) * num15;
				float num20 = (num10 * num7 - num11 * num6) * num15;
				float num21 = (num10 * num9 - num11 * num8) * num15;
				Vector3[] array3 = array;
				int num22 = num;
				array3[num22].x = array3[num22].x + num16;
				Vector3[] array4 = array;
				int num23 = num;
				array4[num23].y = array4[num23].y + num17;
				Vector3[] array5 = array;
				int num24 = num;
				array5[num24].z = array5[num24].z + num18;
				Vector3[] array6 = array;
				int num25 = num2;
				array6[num25].x = array6[num25].x + num16;
				Vector3[] array7 = array;
				int num26 = num2;
				array7[num26].y = array7[num26].y + num17;
				Vector3[] array8 = array;
				int num27 = num2;
				array8[num27].z = array8[num27].z + num18;
				Vector3[] array9 = array;
				int num28 = num3;
				array9[num28].x = array9[num28].x + num16;
				Vector3[] array10 = array;
				int num29 = num3;
				array10[num29].y = array10[num29].y + num17;
				Vector3[] array11 = array;
				int num30 = num3;
				array11[num30].z = array11[num30].z + num18;
				Vector3[] array12 = array2;
				int num31 = num;
				array12[num31].x = array12[num31].x + num19;
				Vector3[] array13 = array2;
				int num32 = num;
				array13[num32].y = array13[num32].y + num20;
				Vector3[] array14 = array2;
				int num33 = num;
				array14[num33].z = array14[num33].z + num21;
				Vector3[] array15 = array2;
				int num34 = num2;
				array15[num34].x = array15[num34].x + num19;
				Vector3[] array16 = array2;
				int num35 = num2;
				array16[num35].y = array16[num35].y + num20;
				Vector3[] array17 = array2;
				int num36 = num2;
				array17[num36].z = array17[num36].z + num21;
				Vector3[] array18 = array2;
				int num37 = num3;
				array18[num37].x = array18[num37].x + num19;
				Vector3[] array19 = array2;
				int num38 = num3;
				array19[num38].y = array19[num38].y + num20;
				Vector3[] array20 = array2;
				int num39 = num3;
				array20[num39].z = array20[num39].z + num21;
			}
			for (int k = 0; k < count2; k++)
			{
				Vector3 vector7 = _normals[k];
				Vector3 vector8 = array[k];
				Vector3.OrthoNormalize(ref vector7, ref vector8);
				Vector4 vector9;
				vector9..ctor(vector8.x, vector8.y, vector8.z, (Vector3.Dot(Vector3.Cross(vector7, vector8), array2[k]) < 0f) ? (-1f) : 1f);
				_tangents.Add(vector9);
			}
		}
	}

	public static List<Vector4> CalculateMeshTangents(Vector3[] _vertices, int[] _indices, Vector3[] _normals, Vector2[] _uvs, bool _bIgnoreUvs = false)
	{
		object @lock = MeshCalculations._lock;
		List<Vector4> list2;
		lock (@lock)
		{
			int num = _indices.Length;
			int num2 = _vertices.Length;
			List<Vector4> list = new List<Vector4>(_vertices.Length);
			Vector3[] array = null;
			Vector3[] array2 = null;
			if (array == null || num2 >= array.Length)
			{
				Log.Out("Increasing tangent cache: " + num2.ToString());
				array = new Vector3[num2 + 1];
				array2 = new Vector3[num2 + 1];
			}
			else
			{
				for (int i = 0; i < num2; i++)
				{
					array[i].Set(0f, 0f, 0f);
					array2[i].Set(0f, 0f, 0f);
				}
			}
			for (int j = 0; j < num; j += 3)
			{
				int num3 = _indices[j];
				int num4 = _indices[j + 1];
				int num5 = _indices[j + 2];
				Vector3 vector = _vertices[num3];
				Vector3 vector2 = _vertices[num4];
				Vector3 vector3 = _vertices[num5];
				Vector2 vector4;
				Vector2 vector5;
				Vector2 vector6;
				if (_bIgnoreUvs)
				{
					vector4 = Vector2.zero;
					vector5 = Vector2.zero;
					vector6 = Vector2.zero;
				}
				else
				{
					vector4 = _uvs[num3];
					vector5 = _uvs[num4];
					vector6 = _uvs[num5];
				}
				float num6 = vector2.x - vector.x;
				float num7 = vector3.x - vector.x;
				float num8 = vector2.y - vector.y;
				float num9 = vector3.y - vector.y;
				float num10 = vector2.z - vector.z;
				float num11 = vector3.z - vector.z;
				float num12 = vector5.x - vector4.x;
				float num13 = vector6.x - vector4.x;
				float num14 = vector5.y - vector4.y;
				float num15 = vector6.y - vector4.y;
				float num16 = num12 * num15 - num13 * num14;
				float num17 = ((num16 == 0f) ? 0f : (1f / num16));
				float num18 = (num15 * num6 - num14 * num7) * num17;
				float num19 = (num15 * num8 - num14 * num9) * num17;
				float num20 = (num15 * num10 - num14 * num11) * num17;
				float num21 = (num12 * num7 - num13 * num6) * num17;
				float num22 = (num12 * num9 - num13 * num8) * num17;
				float num23 = (num12 * num11 - num13 * num10) * num17;
				Vector3[] array3 = array;
				int num24 = num3;
				array3[num24].x = array3[num24].x + num18;
				Vector3[] array4 = array;
				int num25 = num3;
				array4[num25].y = array4[num25].y + num19;
				Vector3[] array5 = array;
				int num26 = num3;
				array5[num26].z = array5[num26].z + num20;
				Vector3[] array6 = array;
				int num27 = num4;
				array6[num27].x = array6[num27].x + num18;
				Vector3[] array7 = array;
				int num28 = num4;
				array7[num28].y = array7[num28].y + num19;
				Vector3[] array8 = array;
				int num29 = num4;
				array8[num29].z = array8[num29].z + num20;
				Vector3[] array9 = array;
				int num30 = num5;
				array9[num30].x = array9[num30].x + num18;
				Vector3[] array10 = array;
				int num31 = num5;
				array10[num31].y = array10[num31].y + num19;
				Vector3[] array11 = array;
				int num32 = num5;
				array11[num32].z = array11[num32].z + num20;
				Vector3[] array12 = array2;
				int num33 = num3;
				array12[num33].x = array12[num33].x + num21;
				Vector3[] array13 = array2;
				int num34 = num3;
				array13[num34].y = array13[num34].y + num22;
				Vector3[] array14 = array2;
				int num35 = num3;
				array14[num35].z = array14[num35].z + num23;
				Vector3[] array15 = array2;
				int num36 = num4;
				array15[num36].x = array15[num36].x + num21;
				Vector3[] array16 = array2;
				int num37 = num4;
				array16[num37].y = array16[num37].y + num22;
				Vector3[] array17 = array2;
				int num38 = num4;
				array17[num38].z = array17[num38].z + num23;
				Vector3[] array18 = array2;
				int num39 = num5;
				array18[num39].x = array18[num39].x + num21;
				Vector3[] array19 = array2;
				int num40 = num5;
				array19[num40].y = array19[num40].y + num22;
				Vector3[] array20 = array2;
				int num41 = num5;
				array20[num41].z = array20[num41].z + num23;
			}
			for (int k = 0; k < num2; k++)
			{
				Vector3 vector7 = _normals[k];
				Vector3 vector8 = array[k];
				Vector3.OrthoNormalize(ref vector7, ref vector8);
				Vector4 vector9;
				vector9..ctor(vector8.x, vector8.y, vector8.z, (Vector3.Dot(Vector3.Cross(vector7, vector8), array2[k]) < 0f) ? (-1f) : 1f);
				list.Add(vector9);
			}
			list2 = list;
		}
		return list2;
	}

	public static void RecalculateNormals(ArrayListMP<Vector3> _vertices, ArrayListMP<int> _indices, ArrayListMP<Vector3> normals, float angle = 60f)
	{
		object @lock = MeshCalculations._lock;
		lock (@lock)
		{
			float num = Mathf.Cos(angle * 0.017453292f);
			normals.Grow(_vertices.Count);
			normals.Count = _vertices.Count;
			int num2 = 1;
			if (MeshCalculations.dictionary == null)
			{
				MeshCalculations.dictionary = new Dictionary<MeshCalculations.VertexKey, List<MeshCalculations.VertexEntry>>(_vertices.Count);
			}
			List<List<Vector3>> list = null;
			Dictionary<MeshCalculations.VertexKey, List<MeshCalculations.VertexEntry>> dictionary = MeshCalculations.dictionary;
			if (list == null)
			{
				list = new List<List<Vector3>>();
			}
			for (int i = list.Count; i < num2; i++)
			{
				list.Add(new List<Vector3>());
			}
			for (int j = 0; j < num2; j++)
			{
				list[j].Capacity = Math.Max(_indices.Count / 3, list[j].Capacity);
				for (int k = list[j].Count; k < list[j].Capacity; k++)
				{
					list[j].Add(default(Vector3));
				}
				for (int l = 0; l < _indices.Count; l += 3)
				{
					int num3 = _indices[l];
					int num4 = _indices[l + 1];
					int num5 = _indices[l + 2];
					Vector3 vector = _vertices[num4] - _vertices[num3];
					Vector3 vector2 = _vertices[num5] - _vertices[num3];
					Vector3 normalized = Vector3.Cross(vector, vector2).normalized;
					int num6 = l / 3;
					list[j][num6] = normalized;
					Dictionary<MeshCalculations.VertexKey, List<MeshCalculations.VertexEntry>> dictionary2 = dictionary;
					MeshCalculations.VertexKey vertexKey = new MeshCalculations.VertexKey(_vertices[num3]);
					List<MeshCalculations.VertexEntry> list2;
					if (!dictionary2.TryGetValue(vertexKey, out list2))
					{
						list2 = MeshCalculations.GetVertexEntryList();
						dictionary.Add(vertexKey, list2);
					}
					list2.Add(new MeshCalculations.VertexEntry(j, num6, num3));
					Dictionary<MeshCalculations.VertexKey, List<MeshCalculations.VertexEntry>> dictionary3 = dictionary;
					vertexKey = new MeshCalculations.VertexKey(_vertices[num4]);
					if (!dictionary3.TryGetValue(vertexKey, out list2))
					{
						list2 = MeshCalculations.GetVertexEntryList();
						dictionary.Add(vertexKey, list2);
					}
					list2.Add(new MeshCalculations.VertexEntry(j, num6, num4));
					Dictionary<MeshCalculations.VertexKey, List<MeshCalculations.VertexEntry>> dictionary4 = dictionary;
					vertexKey = new MeshCalculations.VertexKey(_vertices[num5]);
					if (!dictionary4.TryGetValue(vertexKey, out list2))
					{
						list2 = MeshCalculations.GetVertexEntryList();
						dictionary.Add(vertexKey, list2);
					}
					list2.Add(new MeshCalculations.VertexEntry(j, num6, num5));
				}
			}
			foreach (List<MeshCalculations.VertexEntry> list3 in dictionary.Values)
			{
				for (int m = 0; m < list3.Count; m++)
				{
					Vector3 vector3 = default(Vector3);
					MeshCalculations.VertexEntry vertexEntry = list3[m];
					for (int n = 0; n < list3.Count; n++)
					{
						MeshCalculations.VertexEntry vertexEntry2 = list3[n];
						if (vertexEntry.VertexIndex == vertexEntry2.VertexIndex)
						{
							vector3 += list[vertexEntry2.MeshIndex][vertexEntry2.TriangleIndex];
						}
						else if (Vector3.Dot(list[vertexEntry.MeshIndex][vertexEntry.TriangleIndex], list[vertexEntry2.MeshIndex][vertexEntry2.TriangleIndex]) >= num)
						{
							vector3 += list[vertexEntry2.MeshIndex][vertexEntry2.TriangleIndex];
						}
					}
					normals[vertexEntry.VertexIndex] = vector3.normalized;
				}
			}
			dictionary.Clear();
		}
	}

	public static List<Vector3> RecalculateNormals(Vector3[] _vertices, int[] _indices, float angle = 60f)
	{
		object @lock = MeshCalculations._lock;
		List<Vector3> list5;
		lock (@lock)
		{
			float num = Mathf.Cos(angle * 0.017453292f);
			List<Vector3> list = new List<Vector3>(_vertices);
			int num2 = 1;
			if (MeshCalculations.dictionary == null)
			{
				MeshCalculations.dictionary = new Dictionary<MeshCalculations.VertexKey, List<MeshCalculations.VertexEntry>>(_vertices.Length);
			}
			Dictionary<MeshCalculations.VertexKey, List<MeshCalculations.VertexEntry>> dictionary = MeshCalculations.dictionary;
			List<List<Vector3>> list2 = null;
			if (list2 == null)
			{
				list2 = new List<List<Vector3>>();
			}
			for (int i = list2.Count; i < num2; i++)
			{
				list2.Add(new List<Vector3>());
			}
			for (int j = 0; j < num2; j++)
			{
				list2[j].Capacity = Math.Max(_indices.Length / 3, list2[j].Capacity);
				for (int k = list2[j].Count; k < list2[j].Capacity; k++)
				{
					list2[j].Add(default(Vector3));
				}
				for (int l = 0; l < _indices.Length; l += 3)
				{
					int num3 = _indices[l];
					int num4 = _indices[l + 1];
					int num5 = _indices[l + 2];
					Vector3 vector = _vertices[num4] - _vertices[num3];
					Vector3 vector2 = _vertices[num5] - _vertices[num3];
					Vector3 normalized = Vector3.Cross(vector, vector2).normalized;
					int num6 = l / 3;
					list2[j][num6] = normalized;
					Dictionary<MeshCalculations.VertexKey, List<MeshCalculations.VertexEntry>> dictionary2 = dictionary;
					MeshCalculations.VertexKey vertexKey = new MeshCalculations.VertexKey(_vertices[num3]);
					List<MeshCalculations.VertexEntry> list3;
					if (!dictionary2.TryGetValue(vertexKey, out list3))
					{
						list3 = MeshCalculations.GetVertexEntryList();
						dictionary.Add(vertexKey, list3);
					}
					list3.Add(new MeshCalculations.VertexEntry(j, num6, num3));
					Dictionary<MeshCalculations.VertexKey, List<MeshCalculations.VertexEntry>> dictionary3 = dictionary;
					vertexKey = new MeshCalculations.VertexKey(_vertices[num4]);
					if (!dictionary3.TryGetValue(vertexKey, out list3))
					{
						list3 = MeshCalculations.GetVertexEntryList();
						dictionary.Add(vertexKey, list3);
					}
					list3.Add(new MeshCalculations.VertexEntry(j, num6, num4));
					Dictionary<MeshCalculations.VertexKey, List<MeshCalculations.VertexEntry>> dictionary4 = dictionary;
					vertexKey = new MeshCalculations.VertexKey(_vertices[num5]);
					if (!dictionary4.TryGetValue(vertexKey, out list3))
					{
						list3 = MeshCalculations.GetVertexEntryList();
						dictionary.Add(vertexKey, list3);
					}
					list3.Add(new MeshCalculations.VertexEntry(j, num6, num5));
				}
			}
			foreach (List<MeshCalculations.VertexEntry> list4 in dictionary.Values)
			{
				for (int m = 0; m < list4.Count; m++)
				{
					Vector3 vector3 = default(Vector3);
					MeshCalculations.VertexEntry vertexEntry = list4[m];
					for (int n = 0; n < list4.Count; n++)
					{
						MeshCalculations.VertexEntry vertexEntry2 = list4[n];
						if (vertexEntry.VertexIndex == vertexEntry2.VertexIndex)
						{
							vector3 += list2[vertexEntry2.MeshIndex][vertexEntry2.TriangleIndex];
						}
						else if (Vector3.Dot(list2[vertexEntry.MeshIndex][vertexEntry.TriangleIndex], list2[vertexEntry2.MeshIndex][vertexEntry2.TriangleIndex]) >= num)
						{
							vector3 += list2[vertexEntry2.MeshIndex][vertexEntry2.TriangleIndex];
						}
					}
					list[vertexEntry.VertexIndex] = vector3.normalized;
				}
			}
			dictionary.Clear();
			list5 = list;
		}
		return list5;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static object _lock = new object();

	[PublicizedFrom(EAccessModifier.Private)]
	public static Dictionary<MeshCalculations.VertexKey, List<MeshCalculations.VertexEntry>> dictionary;

	[PublicizedFrom(EAccessModifier.Private)]
	public static ConcurrentQueue<List<MeshCalculations.VertexEntry>> VertexEntries = new ConcurrentQueue<List<MeshCalculations.VertexEntry>>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static ConcurrentDictionary<int, ConcurrentQueue<List<MeshCalculations.VertexEntry>>> Cache = new ConcurrentDictionary<int, ConcurrentQueue<List<MeshCalculations.VertexEntry>>>();

	[PublicizedFrom(EAccessModifier.Private)]
	public struct VertexKey
	{
		public VertexKey(Vector3 position)
		{
			this._x = (long)Mathf.Round(position.x * 100000f);
			this._y = (long)Mathf.Round(position.y * 100000f);
			this._z = (long)Mathf.Round(position.z * 100000f);
		}

		public override bool Equals(object obj)
		{
			MeshCalculations.VertexKey vertexKey = (MeshCalculations.VertexKey)obj;
			return this._x == vertexKey._x && this._y == vertexKey._y && this._z == vertexKey._z;
		}

		public override int GetHashCode()
		{
			long num = (long)((ulong)(-2128831035));
			num ^= this._x;
			num *= 16777619L;
			num ^= this._y;
			num *= 16777619L;
			num ^= this._z;
			return (num * 16777619L).GetHashCode();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly long _x;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly long _y;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly long _z;

		[PublicizedFrom(EAccessModifier.Private)]
		public const int Tolerance = 100000;

		[PublicizedFrom(EAccessModifier.Private)]
		public const long FNV32Init = 2166136261L;

		[PublicizedFrom(EAccessModifier.Private)]
		public const long FNV32Prime = 16777619L;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public struct VertexEntry
	{
		public VertexEntry(int meshIndex, int triIndex, int vertIndex)
		{
			this.MeshIndex = meshIndex;
			this.TriangleIndex = triIndex;
			this.VertexIndex = vertIndex;
		}

		public int MeshIndex;

		public int TriangleIndex;

		public int VertexIndex;
	}
}
