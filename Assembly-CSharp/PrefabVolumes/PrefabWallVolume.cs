using System;

namespace PrefabVolumes
{
	public class PrefabWallVolume : PrefabVolumeAbs<PrefabWallVolume>
	{
		public override PrefabVolumeAbs.EVolumeType VolumeType
		{
			get
			{
				return PrefabVolumeAbs.EVolumeType.Wall;
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
