using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_SignLayerType : XUiC_SignGridEntry
{
	public override void Init()
	{
		base.Init();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string value, string bindingName)
	{
		return base.GetBindingValueInternal(ref value, bindingName);
	}

	public override bool ParseAttribute(string name, string value)
	{
		return base.ParseAttribute(name, value);
	}
}
