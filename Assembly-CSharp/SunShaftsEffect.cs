using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

[PostProcess(typeof(SunShaftsEffectRenderer), 3, "Custom/Sun Shafts", false)]
[Serializable]
public sealed class SunShaftsEffect : PostProcessEffectSettings
{
	public override bool IsEnabledAndSupported(PostProcessRenderContext context)
	{
		if (this.enabled.value && SystemInfo.SupportsRenderTextureFormat(1))
		{
			Shader value = this.sunShaftsShader.value;
			if (value != null && value.isSupported)
			{
				Shader value2 = this.simpleClearShader.value;
				return value2 != null && value2.isSupported;
			}
		}
		return false;
	}

	public SunShaftsEffect.SunSettings GetSunSettings()
	{
		return new SunShaftsEffect.SunSettings
		{
			sunColor = this.sunColor.value,
			sunThreshold = this.sunThreshold.value,
			sunPosition = this.sunPosition.value,
			sunShaftIntensity = this.sunShaftIntensity.value
		};
	}

	public SunShaftsEffect.SunShaftsResolutionParameter resolution = new SunShaftsEffect.SunShaftsResolutionParameter
	{
		value = SunShaftsEffect.SunShaftsResolution.Normal
	};

	public SunShaftsEffect.ShaftsScreenBlendModeParameter screenBlendMode = new SunShaftsEffect.ShaftsScreenBlendModeParameter
	{
		value = SunShaftsEffect.ShaftsScreenBlendMode.Screen
	};

	public BoolParameter autoUpdateSun = new BoolParameter
	{
		value = true
	};

	public Vector3Parameter sunPosition = new Vector3Parameter();

	[Range(1f, 4f)]
	public IntParameter radialBlurIterations = new IntParameter
	{
		value = 2
	};

	public ColorParameter sunColor = new ColorParameter
	{
		value = Color.white
	};

	public ColorParameter sunThreshold = new ColorParameter
	{
		value = new Color(0.87f, 0.74f, 0.65f)
	};

	public FloatParameter sunShaftBlurRadius = new FloatParameter
	{
		value = 2.5f
	};

	public FloatParameter sunShaftIntensity = new FloatParameter
	{
		value = 1.15f
	};

	public FloatParameter maxRadius = new FloatParameter
	{
		value = 0.75f
	};

	public SunShaftsEffect.ShaderParameter sunShaftsShader = new SunShaftsEffect.ShaderParameter();

	public SunShaftsEffect.ShaderParameter simpleClearShader = new SunShaftsEffect.ShaderParameter();

	public struct SunSettings
	{
		public Color sunColor;

		public Color sunThreshold;

		public Vector3 sunPosition;

		public float sunShaftIntensity;
	}

	public enum SunShaftsResolution
	{
		Low,
		Normal,
		High
	}

	public enum ShaftsScreenBlendMode
	{
		Screen,
		Add
	}

	[Serializable]
	public sealed class SunShaftsResolutionParameter : ParameterOverride<SunShaftsEffect.SunShaftsResolution>
	{
	}

	[Serializable]
	public sealed class ShaftsScreenBlendModeParameter : ParameterOverride<SunShaftsEffect.ShaftsScreenBlendMode>
	{
	}

	[Serializable]
	public sealed class ShaderParameter : ParameterOverride<Shader>
	{
	}
}
