using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class ChunkGroupVisualiser : SingletonMonoBehaviour<ChunkGroupVisualiser>
{
	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDrawGizmos()
	{
		Vector3 vector = Vector3.one * 16f;
		vector.y = 128f;
		Vector3 vector2 = -Origin.Instance.OriginPos;
		vector2.y = 0f;
		Gizmos.matrix = Matrix4x4.Translate(vector2);
		foreach (ValueTuple<Color, List<Vector3>> valueTuple in this.groupInfos)
		{
			Gizmos.color = valueTuple.Item1;
			foreach (Vector3 vector3 in valueTuple.Item2)
			{
				Gizmos.DrawCube(vector3, vector);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public void SetGroups(IEnumerable<LongSetGroups.Group> chunkGroups)
	{
		this.groupInfos.Clear();
		Random.InitState(42);
		foreach (LongSetGroups.Group group in chunkGroups)
		{
			ValueTuple<Color, List<Vector3>> valueTuple = new ValueTuple<Color, List<Vector3>>(Random.ColorHSV(0f, 1f, 0.8f, 1f, 0.8f, 1f, 0.25f, 0.25f), new List<Vector3>(group.Count));
			foreach (long num in group.Keys)
			{
				int num2 = WorldChunkCache.extractX(num) * 16 + 8;
				int num3 = WorldChunkCache.extractZ(num) * 16 + 8;
				valueTuple.Item2.Add(new Vector3((float)num2, 8f, (float)num3));
			}
			this.groupInfos.Add(valueTuple);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float alpha = 0.25f;

	[TupleElementNames(new string[] { "color", "positions" })]
	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public List<ValueTuple<Color, List<Vector3>>> groupInfos = new List<ValueTuple<Color, List<Vector3>>>();
}
