using System;
using UnityEngine;

[ExecuteInEditMode]
public class ShaderGlobalsHelper : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Private)]
	public void Start()
	{
		Shader.SetGlobalFloat("_ElectricShockIntensity", this.electricShockIntensity);
		Shader.SetGlobalColor("_ElectricShockColor", this.electricShockColor);
		Shader.SetGlobalFloat("_ElectricShockSpeed", this.electricShockSpeed);
		Shader.SetGlobalFloat("_ElectricShockScale", this.electricShockScale);
		Shader.SetGlobalTexture("_ElectricShockTexture", this.electricShockTexture);
		Shader.SetGlobalVector("_ElectricShockTexture_ST", this.electricShockTexture_ST);
		Shader.SetGlobalFloat("_ElectricShockTexturePanSpeed", this.electricShockTexturePanSpeed);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
	}

	[Header("Electric Shock Shader Properties")]
	[SerializeField]
	[PublicizedFrom(EAccessModifier.Private)]
	public float electricShockIntensity = 1f;

	[SerializeField]
	[PublicizedFrom(EAccessModifier.Private)]
	public Color electricShockColor = new Color(0.5f, 0.8f, 1f, 1f);

	[SerializeField]
	[PublicizedFrom(EAccessModifier.Private)]
	public float electricShockSpeed = 5f;

	[SerializeField]
	[PublicizedFrom(EAccessModifier.Private)]
	public float electricShockScale = 2.5f;

	[SerializeField]
	[PublicizedFrom(EAccessModifier.Private)]
	public Texture electricShockTexture;

	[SerializeField]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector4 electricShockTexture_ST;

	[SerializeField]
	[PublicizedFrom(EAccessModifier.Private)]
	public float electricShockTexturePanSpeed = 10f;

	[SerializeField]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool updateEveryFrame;
}
