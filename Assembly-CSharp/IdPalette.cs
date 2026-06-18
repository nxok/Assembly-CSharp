using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewIdPalette", menuName = "ID Maps/ID Palette", order = 0)]
public class IdPalette : ScriptableObject
{
	public static IdPalette Instance
	{
		get
		{
			if (IdPalette._instance)
			{
				return IdPalette._instance;
			}
			IdPalette._instance = Resources.Load<IdPalette>("IdPalette");
			if (IdPalette._instance == null)
			{
				Debug.LogError("IdPalette.Instance: Couldn't find Resources/IdPalette.asset. Create one IdPalette and place it under a Resources folder.");
			}
			return IdPalette._instance;
		}
	}

	[RuntimeInitializeOnLoadMethod(4)]
	[PublicizedFrom(EAccessModifier.Private)]
	public static void ResetStatic()
	{
		IdPalette._instance = null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnEnable()
	{
		if (IdPalette._instance != null && IdPalette._instance != this)
		{
			Debug.LogWarning("Multiple IdPalette instances loaded. Using the first one: " + IdPalette._instance.name);
			return;
		}
		IdPalette._instance = this;
	}

	public List<PaletteEntry> entries = new List<PaletteEntry>();

	public const string ResourcesPath = "IdPalette";

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static IdPalette _instance;
}
