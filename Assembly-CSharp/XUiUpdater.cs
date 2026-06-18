using System;
using System.Collections.Generic;
using UnityEngine;

[PublicizedFrom(EAccessModifier.Internal)]
public static class XUiUpdater
{
	public static void Add(XUi _ui)
	{
		if (!XUiUpdater.uiToUpdate.Contains(_ui))
		{
			XUiUpdater.uiToUpdate.Add(_ui);
		}
	}

	public static void Remove(XUi _ui)
	{
		XUiUpdater.uiToUpdate.Remove(_ui);
	}

	public static void Update()
	{
		for (int i = 0; i < XUiUpdater.uiToUpdate.Count; i++)
		{
			if (XUiUpdater.uiToUpdate[i] != null)
			{
				XUiUpdater.uiToUpdate[i].OnUpdateDeltaTime(Time.deltaTime);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly List<XUi> uiToUpdate = new List<XUi>();
}
