using System;

namespace PrefabVolumes
{
	public class PrefabInfoVolume : PrefabVolumeAbs<PrefabInfoVolume>
	{
		public override PrefabVolumeAbs.EVolumeType VolumeType
		{
			get
			{
				return PrefabVolumeAbs.EVolumeType.Info;
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
