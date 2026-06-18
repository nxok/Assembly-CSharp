using System;
using UnityEngine;

public class ScreenshotData : MonoBehaviour
{
	public void Start()
	{
	}

	public void Update()
	{
	}

	public void OnGUI()
	{
		if (GameManager.Instance == null || GameManager.Instance.World == null || GameManager.Instance.World.GetPrimaryPlayer() == null)
		{
			return;
		}
		LocalPlayerUI uiforPrimaryPlayer = LocalPlayerUI.GetUIForPrimaryPlayer();
		if (uiforPrimaryPlayer == null || !uiforPrimaryPlayer.windowManager.IsHUDEnabled() || GameManager.Instance.ShowBackground())
		{
			return;
		}
		GUI.Label(new Rect(10f, 10f, 200f, 20f), GameManager.Instance.backgroundColor.ToCultureInvariantString());
	}
}
