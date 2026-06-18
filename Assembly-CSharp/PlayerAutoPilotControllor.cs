using System;
using UnityEngine;

public class PlayerAutoPilotControllor
{
	public PlayerAutoPilotControllor(GameManager _gm)
	{
	}

	public bool IsEnabled()
	{
		return false;
	}

	public void Update()
	{
	}

	public float GetForwardMovement()
	{
		return 0f;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int frameCnt;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 lastPosition = Vector3.zero;
}
