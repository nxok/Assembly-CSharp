using System;
using System.Collections;
using UnityEngine;

[PublicizedFrom(EAccessModifier.Internal)]
public class CWeightList
{
	public CWeightList()
	{
		this.weights = new ArrayList();
	}

	public Transform transform;

	public ArrayList weights;
}
