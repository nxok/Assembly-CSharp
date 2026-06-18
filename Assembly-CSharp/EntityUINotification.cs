using System;
using UnityEngine;

public interface EntityUINotification
{
	BuffValue Buff { get; }

	string Icon { get; }

	Color GetColor();

	float CurrentValue { get; }

	string Units { get; }

	EnumEntityUINotificationDisplayMode DisplayMode { get; }

	EnumEntityUINotificationSubject Subject { get; }
}
