using System;
using UnityEngine.Scripting;

[Preserve]
public class ObjectiveFetchKeep : ObjectiveFetch
{
	public ObjectiveFetchKeep()
	{
		this.KeepItems = true;
	}

	public override BaseObjective Clone()
	{
		ObjectiveFetchKeep objectiveFetchKeep = new ObjectiveFetchKeep();
		this.CopyValues(objectiveFetchKeep);
		objectiveFetchKeep.KeepItems = this.KeepItems;
		return objectiveFetchKeep;
	}
}
