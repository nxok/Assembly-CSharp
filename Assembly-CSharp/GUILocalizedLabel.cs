using System;
using UnityEngine;
using UnityEngine.UI;

public class GUILocalizedLabel : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Private)]
	public void Awake()
	{
		Text component = base.GetComponent<Text>();
		if (component)
		{
			component.text = Localization.Get(this.localizationKey, false, null);
		}
	}

	[SerializeField]
	[PublicizedFrom(EAccessModifier.Private)]
	public string localizationKey;
}
