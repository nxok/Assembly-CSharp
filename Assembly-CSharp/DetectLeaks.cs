using System;
using System.Collections.Generic;
using UnityEngine;

public class DetectLeaks : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Private)]
	public void OnGUI()
	{
		Object[] array = Object.FindObjectsByType<Object>(0);
		Dictionary<string, int> dictionary = new Dictionary<string, int>();
		Object[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			string text = array2[i].GetType().ToString();
			if (dictionary.ContainsKey(text))
			{
				Dictionary<string, int> dictionary2 = dictionary;
				string text2 = text;
				int num = dictionary2[text2];
				dictionary2[text2] = num + 1;
			}
			else
			{
				dictionary[text] = 1;
			}
		}
		List<KeyValuePair<string, int>> list = new List<KeyValuePair<string, int>>(dictionary);
		list.Sort((KeyValuePair<string, int> firstPair, KeyValuePair<string, int> nextPair) => nextPair.Value.CompareTo(firstPair.Value));
		foreach (KeyValuePair<string, int> keyValuePair in list)
		{
			GUILayout.Label(keyValuePair.Key + ": " + keyValuePair.Value.ToString(), Array.Empty<GUILayoutOption>());
		}
	}
}
