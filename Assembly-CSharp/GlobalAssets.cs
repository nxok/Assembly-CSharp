using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.AddressableAssets;

public static class GlobalAssets
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static Dictionary<string, string> LoadShaderMappings()
	{
		return JsonUtility.FromJson<AssetMappings>(File.ReadAllText(Path.Combine(Addressables.RuntimePath, "shaders.json"))).ToDictionary();
	}

	public static Shader FindShader(string name)
	{
		if (GlobalAssets.shaders == null)
		{
			GlobalAssets.shaders = GlobalAssets.LoadShaderMappings();
		}
		string text;
		if (GlobalAssets.shaders.TryGetValue(name, out text))
		{
			return LoadManager.LoadAssetFromAddressables<Shader>(text, null, null, false, true, false).Asset;
		}
		return Shader.Find(name);
	}

	public const string ShaderMappingFile = "shaders.json";

	[PublicizedFrom(EAccessModifier.Private)]
	public static Dictionary<string, string> shaders;
}
