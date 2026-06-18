using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[CreateAssetMenu]
public class SignFontConfiguration : ScriptableObject
{
	public List<SignFontConfiguration.FontInfo> fontInfos = new List<SignFontConfiguration.FontInfo>();

	[Serializable]
	public class FontInfo
	{
		public string name;

		public TMP_FontAsset fontAsset;
	}
}
