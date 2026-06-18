using System;
using UnityEngine;

[Serializable]
public class vp_ItemType : ScriptableObject
{
	public string DisplayNameFull
	{
		get
		{
			return this.IndefiniteArticle + " " + this.DisplayName;
		}
	}

	public string IndefiniteArticle = "a";

	public string DisplayName;

	public string Description;

	public Texture2D Icon;

	public float Space;
}
