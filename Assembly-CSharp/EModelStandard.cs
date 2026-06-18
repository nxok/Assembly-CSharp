using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class EModelStandard : EModelBase
{
	public override void PostInit()
	{
		base.PostInit();
		Transform modelTransform = base.GetModelTransform();
		if (modelTransform)
		{
			base.SetColliderLayers(modelTransform, 0);
		}
	}
}
