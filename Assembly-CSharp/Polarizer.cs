using System;
using UnityEngine;

public class Polarizer : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public void Start()
	{
		Shader shader = GlobalAssets.FindShader("Custom/DebugView");
		if (shader != null)
		{
			this.material = new Material(shader);
		}
	}

	public static void SetDebugView(Polarizer.ViewEnums view)
	{
		Polarizer.currDebugView = view;
	}

	public static Polarizer.ViewEnums GetDebugView()
	{
		return Polarizer.currDebugView;
	}

	public void OnPreRender()
	{
	}

	public void OnPostRender()
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Material material;

	public static Polarizer.ViewEnums currDebugView;

	public enum ViewEnums
	{
		None,
		Normals,
		Albedo,
		Specular
	}
}
