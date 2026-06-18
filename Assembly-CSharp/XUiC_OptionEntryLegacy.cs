using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_OptionEntryLegacy : XUiC_OptionEntryAbs
{
	public override bool IsChanged
	{
		get
		{
			return false;
		}
	}

	public override bool IsDefault
	{
		get
		{
			return false;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void initCurrentValue()
	{
	}

	public override void DiscardCurrentChange()
	{
	}

	public override void ApplySelection()
	{
	}

	public override void ResetToDefault()
	{
	}
}
