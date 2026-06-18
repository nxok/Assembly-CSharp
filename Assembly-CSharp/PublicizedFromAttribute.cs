using System;
using System.Runtime.InteropServices;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Event | AttributeTargets.Interface | AttributeTargets.Delegate)]
[StructLayout(LayoutKind.Auto, CharSet = CharSet.Auto)]
public class PublicizedFromAttribute : Attribute
{
	public PublicizedFromAttribute(EAccessModifier _originalAccessModifier)
	{
		this.OriginalAccessModifier = _originalAccessModifier;
	}

	public readonly EAccessModifier OriginalAccessModifier;
}
