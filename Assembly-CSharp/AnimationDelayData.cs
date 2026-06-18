using System;
using UnityEngine.Scripting;

[Preserve]
public class AnimationDelayData
{
	public static void InitStatic()
	{
		AnimationDelayData.AnimationDelay = new AnimationDelayData.AnimationDelays[100];
		for (int i = 0; i < AnimationDelayData.AnimationDelay.Length; i++)
		{
			AnimationDelayData.AnimationDelay[i] = new AnimationDelayData.AnimationDelays(0, 0f, 0f, Constants.cMinHolsterTime, Constants.cMinUnHolsterTime, false);
		}
	}

	public static void Cleanup()
	{
		AnimationDelayData.InitStatic();
	}

	public static AnimationDelayData.AnimationDelays[] AnimationDelay;

	public struct AnimationDelays
	{
		public AnimationDelays(int _carry, float _rayCast, float _rayCastMoving, float _holster, float _unholster, bool _twoHanded)
		{
			this.Carry = _carry;
			this.RayCast = _rayCast;
			this.RayCastMoving = _rayCastMoving;
			this.Holster = _holster;
			this.Unholster = _unholster;
			this.TwoHanded = _twoHanded;
		}

		public int Carry;

		public float RayCast;

		public float RayCastMoving;

		public float Holster;

		public float Unholster;

		public bool TwoHanded;
	}
}
