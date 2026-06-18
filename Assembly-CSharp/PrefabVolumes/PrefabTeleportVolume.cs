using System;

namespace PrefabVolumes
{
	public class PrefabTeleportVolume : PrefabVolumeAbs<PrefabTeleportVolume>
	{
		public override PrefabVolumeAbs.EVolumeType VolumeType
		{
			get
			{
				return PrefabVolumeAbs.EVolumeType.Teleport;
			}
		}

		public override int SerializedSize
		{
			get
			{
				return 25;
			}
		}
	}
}
