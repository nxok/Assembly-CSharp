using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GUIWindowConsoleComponents : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Private)]
	public void Awake()
	{
		this.buttonPrompts = new List<GUIButtonPrompt>(base.GetComponentsInChildren<GUIButtonPrompt>());
	}

	public void RefreshButtonPrompts()
	{
		foreach (GUIButtonPrompt guibuttonPrompt in this.buttonPrompts)
		{
			guibuttonPrompt.RefreshIcon();
		}
	}

	public ScrollRect scrollRect;

	public Transform contentRect;

	public InputField commandField;

	public Button closeButton;

	public Button openLogsButton;

	public GameObject controllerPrompts;

	public GameObject consoleLinePrefab;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public List<GUIButtonPrompt> buttonPrompts;
}
