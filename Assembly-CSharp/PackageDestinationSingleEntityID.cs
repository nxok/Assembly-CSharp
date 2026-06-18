using System;

public class PackageDestinationSingleEntityID : IPackageDestinationFilter
{
	public PackageDestinationSingleEntityID(int _entityId)
	{
		this.entityId = _entityId;
	}

	public bool Exclude(ClientInfo _cInfo)
	{
		return !_cInfo.bAttachedToEntity || _cInfo.entityId != this.entityId;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int entityId;
}
