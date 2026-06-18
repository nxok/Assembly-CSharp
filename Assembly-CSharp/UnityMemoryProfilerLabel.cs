using System;
using System.Text;
using Unity.Profiling;
using UnityEngine;

public class UnityMemoryProfilerLabel : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Private)]
	public void Awake()
	{
		if (this.label == null)
		{
			base.enabled = !base.gameObject.TryGetComponent<UILabel>(ref this.label);
			if (!base.enabled)
			{
				Object.Destroy(base.gameObject);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnEnable()
	{
		this.totalRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Total Used Memory", 1, 24);
		this.totalReservedRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Total Reserved Memory", 1, 24);
		this.systemRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "System Used Memory", 1, 24);
		this.gcReservedMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Reserved Memory", 1, 24);
		this.gcUsedMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Used Memory", 1, 24);
		this.gcAllocInFrameMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Allocated In Frame", 1, 24);
		this.gfxUsedMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Gfx Used Memory", 1, 24);
		this.mainThreadRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Internal, "Main Thread", 15, 24);
		this.meshBytesRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Mesh Memory", 1, 24);
		this.meshCountRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Mesh Count", 1, 24);
		this.textureBytesRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Used Textures Bytes", 1, 24);
		this.textureCountRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Used Textures Count", 1, 24);
		this.renderTextureBytesRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Render Textures Bytes", 1, 24);
		this.renderTextureCountRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Render Textures Count", 1, 24);
		this.setPassCallsRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "SetPass Calls Count", 1, 24);
		this.drawCallsRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Draw Calls Count", 1, 24);
		this.verticesRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Vertices Count", 1, 24);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		this.sb.Clear();
		this.sb.AppendLine("MEMORY");
		if (this.systemRecorder.Valid)
		{
			this.sb.AppendLine("System Used Memory " + UnityMemoryProfilerLabel.ToSize(this.systemRecorder.LastValue, UnityMemoryProfilerLabel.SiSizeUnits.GiB) + "GiB");
		}
		if (this.totalReservedRecorder.Valid)
		{
			this.sb.AppendLine("Total Reserved Memory " + UnityMemoryProfilerLabel.ToSize(this.totalReservedRecorder.LastValue, UnityMemoryProfilerLabel.SiSizeUnits.GiB) + "GiB");
		}
		if (this.totalRecorder.Valid)
		{
			this.sb.AppendLine("Total Used Memory " + UnityMemoryProfilerLabel.ToSize(this.totalRecorder.LastValue, UnityMemoryProfilerLabel.SiSizeUnits.GiB) + "GiB");
		}
		if (this.gcReservedMemoryRecorder.Valid)
		{
			this.sb.AppendLine("GC Reserved Memory " + UnityMemoryProfilerLabel.ToSize(this.gcReservedMemoryRecorder.LastValue, UnityMemoryProfilerLabel.SiSizeUnits.GiB) + "GiB");
		}
		if (this.gcUsedMemoryRecorder.Valid)
		{
			this.sb.AppendLine("GC Used Memory " + UnityMemoryProfilerLabel.ToSize(this.gcUsedMemoryRecorder.LastValue, UnityMemoryProfilerLabel.SiSizeUnits.GiB) + "GiB");
		}
		if (this.gcAllocInFrameMemoryRecorder.Valid)
		{
			this.sb.AppendLine("GC Allocated This Frame " + UnityMemoryProfilerLabel.ToSize(this.gcAllocInFrameMemoryRecorder.LastValue, UnityMemoryProfilerLabel.SiSizeUnits.MiB) + "MiB");
		}
		if (this.mainThreadRecorder.Valid)
		{
			this.sb.AppendLine("Main Thread Memory " + UnityMemoryProfilerLabel.ToSize(this.mainThreadRecorder.LastValue, UnityMemoryProfilerLabel.SiSizeUnits.MiB) + "MiB");
		}
		if (this.gfxUsedMemoryRecorder.Valid)
		{
			this.sb.AppendLine("GFX Used Memory " + UnityMemoryProfilerLabel.ToSize(this.gfxUsedMemoryRecorder.LastValue, UnityMemoryProfilerLabel.SiSizeUnits.GiB) + "GiB");
		}
		this.sb.AppendLine();
		this.sb.AppendLine("Rendering");
		if (this.meshCountRecorder.Valid)
		{
			this.sb.AppendLine(string.Format("Mesh Count {0}", this.meshCountRecorder.LastValue));
		}
		if (this.meshBytesRecorder.Valid)
		{
			if (this.meshBytesRecorder.LastValue > 1073741824L)
			{
				this.sb.AppendLine("Mesh Memory " + UnityMemoryProfilerLabel.ToSize(this.meshBytesRecorder.LastValue, UnityMemoryProfilerLabel.SiSizeUnits.GiB) + "GiB");
			}
			else
			{
				this.sb.AppendLine("Mesh Memory " + UnityMemoryProfilerLabel.ToSize(this.meshBytesRecorder.LastValue, UnityMemoryProfilerLabel.SiSizeUnits.MiB) + "MiB");
			}
		}
		if (this.textureCountRecorder.Valid && this.textureCountRecorder.LastValue > 0L)
		{
			this.sb.AppendLine(string.Format("Used Textures Count {0}", this.textureCountRecorder.LastValue));
		}
		if (this.textureBytesRecorder.Valid && this.textureBytesRecorder.LastValue > 0L)
		{
			if (this.meshBytesRecorder.LastValue > 1073741824L)
			{
				this.sb.AppendLine("Used Textures " + UnityMemoryProfilerLabel.ToSize(this.textureBytesRecorder.LastValue, UnityMemoryProfilerLabel.SiSizeUnits.GiB) + "GiB");
			}
			else
			{
				this.sb.AppendLine("Used Textures " + UnityMemoryProfilerLabel.ToSize(this.textureBytesRecorder.LastValue, UnityMemoryProfilerLabel.SiSizeUnits.MiB) + "MiB");
			}
		}
		if (this.renderTextureCountRecorder.Valid)
		{
			this.sb.AppendLine(string.Format("Render Textures {0}", this.renderTextureCountRecorder.LastValue));
		}
		if (this.renderTextureBytesRecorder.Valid)
		{
			this.sb.AppendLine("Render Textures " + UnityMemoryProfilerLabel.ToSize(this.renderTextureBytesRecorder.LastValue, UnityMemoryProfilerLabel.SiSizeUnits.MiB) + "MiB");
		}
		if (this.setPassCallsRecorder.Valid)
		{
			this.sb.AppendLine(string.Format("SetPass Calls: {0}", this.setPassCallsRecorder.LastValue));
		}
		if (this.drawCallsRecorder.Valid)
		{
			this.sb.AppendLine(string.Format("Draw Calls: {0}", this.drawCallsRecorder.LastValue));
		}
		if (this.verticesRecorder.Valid)
		{
			this.sb.AppendLine(string.Format("Vertices: {0}", this.verticesRecorder.LastValue));
		}
		this.label.text = this.sb.ToString();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDisable()
	{
		this.totalRecorder.Dispose();
		this.totalReservedRecorder.Dispose();
		this.systemRecorder.Dispose();
		this.gcReservedMemoryRecorder.Dispose();
		this.gcUsedMemoryRecorder.Dispose();
		this.gcAllocInFrameMemoryRecorder.Dispose();
		this.gfxUsedMemoryRecorder.Dispose();
		this.mainThreadRecorder.Dispose();
		this.textureBytesRecorder.Dispose();
		this.textureCountRecorder.Dispose();
		this.renderTextureBytesRecorder.Dispose();
		this.renderTextureCountRecorder.Dispose();
		this.setPassCallsRecorder.Dispose();
		this.drawCallsRecorder.Dispose();
		this.verticesRecorder.Dispose();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string ToSize(long _value, UnityMemoryProfilerLabel.SiSizeUnits _unit)
	{
		return ((double)_value / Math.Pow(1024.0, (double)((long)_unit))).ToString("0.0000");
	}

	[SerializeField]
	[PublicizedFrom(EAccessModifier.Private)]
	public UILabel label;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public ProfilerRecorder totalRecorder;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public ProfilerRecorder totalReservedRecorder;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public ProfilerRecorder systemRecorder;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public ProfilerRecorder gcReservedMemoryRecorder;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public ProfilerRecorder gcUsedMemoryRecorder;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public ProfilerRecorder gcAllocInFrameMemoryRecorder;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public ProfilerRecorder gfxUsedMemoryRecorder;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public ProfilerRecorder mainThreadRecorder;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public ProfilerRecorder meshBytesRecorder;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public ProfilerRecorder meshCountRecorder;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public ProfilerRecorder textureBytesRecorder;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public ProfilerRecorder textureCountRecorder;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public ProfilerRecorder renderTextureBytesRecorder;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public ProfilerRecorder renderTextureCountRecorder;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public ProfilerRecorder setPassCallsRecorder;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public ProfilerRecorder drawCallsRecorder;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public ProfilerRecorder verticesRecorder;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public StringBuilder sb = new StringBuilder(500);

	[PublicizedFrom(EAccessModifier.Private)]
	public enum SiSizeUnits
	{
		Byte,
		KiB,
		MiB,
		GiB,
		TiB,
		PiB,
		EiB,
		ZiB,
		YiB
	}
}
