using System;
using System.Collections.Generic;
using UnityEngine;

public class MicroSplatKeywords : ScriptableObject
{
	public bool IsKeywordEnabled(string k)
	{
		return this.keywords.Contains(k);
	}

	public void EnableKeyword(string k)
	{
		if (!this.IsKeywordEnabled(k))
		{
			this.keywords.Add(k);
		}
	}

	public void DisableKeyword(string k)
	{
		if (this.IsKeywordEnabled(k))
		{
			this.keywords.Remove(k);
		}
	}

	public List<string> keywords = new List<string>();

	public int drawOrder = 100;
}
