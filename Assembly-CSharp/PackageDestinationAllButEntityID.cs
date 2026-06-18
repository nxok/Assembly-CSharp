using System;

public class PackageDestinationAllButEntityID : IPackageDestinationFilter
{
	public PackageDestinationAllButEntityID(int _excludedEntityId)
	{
		this.entityId = _excludedEntityId;
	}

	public bool Exclude(ClientInfo _cInfo)
	{
		return !_cInfo.bAttachedToEntity || _cInfo.entityId == this.entityId;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int entityId;
}
