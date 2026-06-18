using System;
using UnityEngine;

public static class ServerHelper
{
	public static void SetupForServer(GameObject obj)
	{
		Component[] array = obj.GetComponentsInChildren<Renderer>();
		array = array;
		for (int i = 0; i < array.Length; i++)
		{
			((Renderer)array[i]).enabled = false;
		}
	}
}
