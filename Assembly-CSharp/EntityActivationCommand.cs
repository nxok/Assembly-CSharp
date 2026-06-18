using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public struct EntityActivationCommand
{
	public EntityActivationCommand(string _commandId, string _icon, string _eventName = null, string _customCommandText = null)
	{
		this.commandId = _commandId;
		this.icon = _icon;
		this.eventName = _eventName;
		this.iconColor = Color.white;
		this.activateTime = -1f;
		this.enabled = true;
		this.commandText = this.commandId;
		if (!string.IsNullOrEmpty(_customCommandText))
		{
			this.commandText = _customCommandText;
		}
		this.commandText = Localization.Get("entitycommand_" + this.commandText, false, null);
	}

	public string commandId;

	public string icon;

	public Color iconColor;

	public bool enabled;

	public string eventName;

	public float activateTime;

	public string commandText;
}
