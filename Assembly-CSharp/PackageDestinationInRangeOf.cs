using System;

public class PackageDestinationInRangeOf : IPackageDestinationFilter
{
	public PackageDestinationInRangeOf(int _entityId, int _range)
	{
		this.entityId = _entityId;
		this.range = _range;
	}

	public bool Exclude(ClientInfo _cInfo)
	{
		return !GameManager.Instance.World.IsEntityInRange(this.entityId, _cInfo.entityId, this.range);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int entityId;

	[PublicizedFrom(EAccessModifier.Private)]
	public int range;
}
