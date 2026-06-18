using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdSystemInfo : ConsoleCmdAbstract
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[] { "SystemInfo" };
	}

	public override bool AllowedInMainMenu
	{
		get
		{
			return true;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "List SystemInfo";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Device Model                   :" + SystemInfo.deviceModel);
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("deviceModel                    :" + SystemInfo.deviceModel);
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("deviceName                     :" + SystemInfo.deviceName);
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("deviceType                     :" + SystemInfo.deviceType.ToStringCached<DeviceType>());
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("deviceUniqueIdentifier         :" + SystemInfo.deviceUniqueIdentifier);
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("graphicsDeviceID               :" + SystemInfo.graphicsDeviceID.ToString());
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("graphicsDeviceName             :" + SystemInfo.graphicsDeviceName);
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("graphicsDeviceType             :" + SystemInfo.graphicsDeviceType.ToStringCached<GraphicsDeviceType>());
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("graphicsDeviceVendor           :" + SystemInfo.graphicsDeviceVendor);
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("graphicsDeviceVendorID         :" + SystemInfo.graphicsDeviceVendorID.ToString());
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("graphicsDeviceVersion          :" + SystemInfo.graphicsDeviceVersion);
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("graphicsMemorySize             :" + SystemInfo.graphicsMemorySize.ToString());
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("graphicsMultiThreaded          :" + SystemInfo.graphicsMultiThreaded.ToString());
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("graphicsShaderLevel            :" + SystemInfo.graphicsShaderLevel.ToString());
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("maxTextureSize                 :" + SystemInfo.maxTextureSize.ToString());
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("npotSupport                    :" + SystemInfo.npotSupport.ToStringCached<NPOTSupport>());
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("operatingSystem                :" + SystemInfo.operatingSystem);
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("processorCount                 :" + SystemInfo.processorCount.ToString());
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("processorType                  :" + SystemInfo.processorType);
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("supportedRenderTargetCount     :" + SystemInfo.supportedRenderTargetCount.ToString());
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("supports3DTextures             :" + SystemInfo.supports3DTextures.ToString());
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("supportsAccelerometer          :" + SystemInfo.supportsAccelerometer.ToString());
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("supportsComputeShaders         :" + SystemInfo.supportsComputeShaders.ToString());
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("supportsGyroscope              :" + SystemInfo.supportsGyroscope.ToString());
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("supportsImageEffects           : true (always)");
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("supportsInstancing             :" + SystemInfo.supportsInstancing.ToString());
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("supportsLocationService        :" + SystemInfo.supportsLocationService.ToString());
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("supportsRenderToCubemap        : true (always)");
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("supportsShadows                :" + SystemInfo.supportsShadows.ToString());
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("supportsSparseTextures         :" + SystemInfo.supportsSparseTextures.ToString());
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("supportsVibration              :" + SystemInfo.supportsVibration.ToString());
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("systemMemorySize               :" + SystemInfo.systemMemorySize.ToString());
	}
}
