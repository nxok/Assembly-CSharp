using System;
using Platform;
using UnityEngine;
using UnityEngine.UI;

public class GUIButtonPrompt : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Private)]
	public void Awake()
	{
		this.image = base.GetComponent<Image>();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnEnable()
	{
		this.RefreshIcon();
	}

	public void RefreshIcon()
	{
		PlayerInputManager.InputStyle inputStyle = PlayerInputManager.InputStyleFromSelectedIconStyle();
		this.image.sprite = ((inputStyle == PlayerInputManager.InputStyle.PS4) ? this.PSSprite : this.XBSprite);
	}

	[SerializeField]
	[PublicizedFrom(EAccessModifier.Private)]
	public Sprite XBSprite;

	[SerializeField]
	[PublicizedFrom(EAccessModifier.Private)]
	public Sprite PSSprite;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Image image;
}
