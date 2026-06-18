using System;
using UnityEngine;

public class ApplyExplosionForce : MonoBehaviour
{
	public static void Explode(Vector3 explosionPos, float power, float radius)
	{
		explosionPos -= Origin.position;
		power *= 20f;
		radius *= 1.75f;
		int num = Physics.OverlapSphereNonAlloc(explosionPos, radius, ApplyExplosionForce.colliderList);
		if (num > 1024)
		{
			num = 1024;
		}
		for (int i = 0; i < num; i++)
		{
			Rigidbody component = ApplyExplosionForce.colliderList[i].GetComponent<Rigidbody>();
			if (component)
			{
				component.AddExplosionForce(power, explosionPos, radius, 3f);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float cUpwards = 3f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const int cMaxColliders = 1024;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static Collider[] colliderList = new Collider[1024];
}
