using System;
using UnityEngine;

public class AnimationGunjointOffsetData
{
	public static void InitStatic()
	{
		AnimationGunjointOffsetData.AnimationGunjointOffset = new AnimationGunjointOffsetData.AnimationGunjointOffsets[100];
		for (int i = 0; i < AnimationGunjointOffsetData.AnimationGunjointOffset.Length; i++)
		{
			AnimationGunjointOffsetData.AnimationGunjointOffset[i] = new AnimationGunjointOffsetData.AnimationGunjointOffsets(Vector3.zero, Vector3.zero);
		}
	}

	public static void Cleanup()
	{
		AnimationGunjointOffsetData.InitStatic();
	}

	public static AnimationGunjointOffsetData.AnimationGunjointOffsets[] AnimationGunjointOffset;

	public struct AnimationGunjointOffsets
	{
		public AnimationGunjointOffsets(Vector3 _position, Vector3 _rotation)
		{
			this.position = _position;
			this.rotation = _rotation;
		}

		public Vector3 position;

		public Vector3 rotation;
	}
}
