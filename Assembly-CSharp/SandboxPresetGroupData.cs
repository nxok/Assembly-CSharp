using System;

public struct SandboxPresetGroupData : IEquatable<SandboxPresetGroupData>
{
	public SandboxPresetGroupData(string _internalName, string _formattedName)
	{
		this.InternalName = _internalName;
		this.formattedName = _formattedName;
	}

	public override string ToString()
	{
		return this.formattedName;
	}

	public bool Equals(SandboxPresetGroupData _other)
	{
		return this.InternalName == _other.InternalName;
	}

	public override bool Equals(object _obj)
	{
		if (_obj is SandboxPresetGroupData)
		{
			SandboxPresetGroupData sandboxPresetGroupData = (SandboxPresetGroupData)_obj;
			return this.Equals(sandboxPresetGroupData);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return ((this.InternalName != null) ? this.InternalName.GetHashCode() : 0) * 397;
	}

	public readonly string InternalName;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly string formattedName;
}
