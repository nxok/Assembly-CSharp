using System;
using UnityEngine;

public static class MetricHelpers
{
	public static CallbackMetric TextureStreamingCurrent = new CallbackMetric
	{
		Header = "Texture Streaming Current",
		callback = () => string.Format("{0:F2}", Texture.currentTextureMemory * 9.5367431640625E-07)
	};

	public static CallbackMetric TextureStreamingTarget = new CallbackMetric
	{
		Header = "Texture Streaming Target",
		callback = () => string.Format("{0:F2}", Texture.targetTextureMemory * 9.5367431640625E-07)
	};

	public static CallbackMetric TextureStreamingDesired = new CallbackMetric
	{
		Header = "Texture Streaming Desired",
		callback = () => string.Format("{0:F2}", Texture.desiredTextureMemory * 9.5367431640625E-07)
	};

	public static CallbackMetric TextureStreamingNonStreamed = new CallbackMetric
	{
		Header = "Texture Streaming Non-Streamed",
		callback = () => string.Format("{0:F2}", Texture.nonStreamingTextureMemory * 9.5367431640625E-07)
	};

	public static CallbackMetric TextureStreamingBudget = new CallbackMetric
	{
		Header = "Texture Streaming Budget",
		callback = () => string.Format("{0:F2}", QualitySettings.streamingMipmapsActive ? QualitySettings.streamingMipmapsMemoryBudget : (-1f))
	};
}
