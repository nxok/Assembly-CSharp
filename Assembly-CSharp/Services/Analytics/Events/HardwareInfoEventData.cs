using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace Services.Analytics.Events
{
	public class HardwareInfoEventData : BaseEventData
	{
		public override string EventType
		{
			get
			{
				return "hardware_info";
			}
		}

		[JsonProperty(PropertyName = "os")]
		public string OperatingSystem
		{
			get; [PublicizedFrom(EAccessModifier.Private)]
			set;
		} = SystemInfo.operatingSystem;

		[JsonProperty(PropertyName = "cpu_data")]
		public Dictionary<string, string> CpuData
		{
			get; [PublicizedFrom(EAccessModifier.Private)]
			set;
		} = new Dictionary<string, string>
		{
			{
				"type",
				SystemInfo.processorType
			},
			{
				"count",
				SystemInfo.processorCount.ToString()
			}
		};

		[JsonProperty(PropertyName = "gpu_data")]
		public Dictionary<string, string> GpuData
		{
			get; [PublicizedFrom(EAccessModifier.Private)]
			set;
		} = new Dictionary<string, string>
		{
			{
				"name",
				SystemInfo.graphicsDeviceName
			},
			{
				"type",
				SystemInfo.graphicsDeviceType.ToString()
			},
			{
				"version",
				SystemInfo.graphicsDeviceVersion
			},
			{
				"memory",
				SystemInfo.graphicsMemorySize.ToString()
			},
			{
				"vendor",
				SystemInfo.graphicsDeviceVendor
			}
		};

		[JsonProperty(PropertyName = "memory_ram")]
		public int MemoryRam
		{
			get; [PublicizedFrom(EAccessModifier.Private)]
			set;
		} = SystemInfo.systemMemorySize;
	}
}
