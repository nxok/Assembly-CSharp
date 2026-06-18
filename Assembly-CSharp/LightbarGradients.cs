using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Gradients/Lightbar Gradient Data")]
[Serializable]
public class LightbarGradients : ScriptableObject
{
	public Gradient timeOfDayGradient;

	public Gradient dayGradient;

	public Gradient nightGradient;

	public Gradient cloudDayGradient;

	public Gradient cloudNightGradient;

	public Gradient bloodmoonGradient;

	public Color mainMenuColor;
}
