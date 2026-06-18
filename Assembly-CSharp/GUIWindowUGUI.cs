using System;
using UnityEngine;

public abstract class GUIWindowUGUI : GUIWindow
{
	public abstract string UIPrefabPath { get; }

	public GUIWindowUGUI(string _id)
		: base(_id)
	{
		this.uiPrefab = DataLoader.LoadAsset<GameObject>(this.UIPrefabPath, false);
		this.canvas = Object.Instantiate<GameObject>(this.uiPrefab).GetComponent<Canvas>();
		this.canvas.gameObject.SetActive(false);
	}

	public override void OnOpen()
	{
		base.OnOpen();
		if (ThreadManager.IsMainThread())
		{
			this.canvas.gameObject.SetActive(true);
			return;
		}
		this.shouldOpen = true;
	}

	public override void Update()
	{
		if (this.shouldOpen && !this.canvas.gameObject.activeSelf)
		{
			this.canvas.gameObject.SetActive(true);
			this.shouldOpen = false;
		}
	}

	public override void OnClose()
	{
		base.OnClose();
		this.canvas.gameObject.SetActive(false);
	}

	public override void Cleanup()
	{
		Object.Destroy(this.canvas.gameObject);
		this.uiPrefab = null;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public GameObject uiPrefab;

	[PublicizedFrom(EAccessModifier.Protected)]
	public Canvas canvas;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool shouldOpen;
}
