using System;

public abstract class DynamicMeshServerData : NetPackage
{
	public abstract bool Prechecks();

	[PublicizedFrom(EAccessModifier.Protected)]
	public DynamicMeshServerData()
	{
	}

	public int X;

	public int Z;
}
