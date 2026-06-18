using System;
using UnityEngine;

public class NGuiHUDRoot : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public void Awake()
	{
		NGuiHUDRoot.go = base.gameObject;
	}

	public static GameObject go;
}
