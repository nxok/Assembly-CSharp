using System;
using UnityEngine;

public class CrosshairDrawer : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public void OnGUI()
	{
		if (!this.draw)
		{
			return;
		}
		EntityPlayerLocal.DrawDynamicCrosshair(this.centerX, this.centerY, this.openAreaX, this.openAreaY, this.length, this.thickness, this.opacity, this.color);
	}

	public bool draw;

	public float centerX;

	public float centerY;

	public float openAreaX;

	public float openAreaY;

	public float length;

	public float thickness;

	public float opacity;

	public Color color = Color.white;
}
