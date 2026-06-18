using System;
using UnityEngine;

public interface IAIDirectorMarker
{
	void Reference();

	bool Release();

	void Tick(double dt);

	EntityPlayer Player { get; }

	double IntensityForPosition(Vector3 position);

	Vector3 Position { get; }

	Vector3 TargetPosition { get; }

	bool Valid { get; }

	float MaxRadius { get; }

	float Radius { get; }

	float TimeToLive { get; }

	float ValidTime { get; }

	float Speed { get; }

	int Priority { get; }

	bool InterruptsNonPlayerAttack { get; }

	bool IsDistraction { get; }
}
