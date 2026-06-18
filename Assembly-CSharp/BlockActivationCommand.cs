using System;
using UnityEngine;

public struct BlockActivationCommand
{
	public BlockActivationCommand(string _text, string _icon, bool _enabled, bool _highlighted = false, string _eventName = null)
	{
		this.text = _text;
		this.icon = _icon;
		this.enabled = _enabled;
		this.highlighted = _highlighted;
		this.eventName = _eventName;
		this.iconColor = Color.white;
		this.activateTime = -1f;
	}

	public string text;

	public string icon;

	public Color iconColor;

	public bool enabled;

	public bool highlighted;

	public string eventName;

	public float activateTime;

	public static readonly BlockActivationCommand[] Empty = Array.Empty<BlockActivationCommand>();
}
