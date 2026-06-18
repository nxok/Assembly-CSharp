using System;
using MusicUtils.Enums;
using UnityEngine;

namespace DynamicMusic
{
	public struct ThreatLevel : IThreatLevel
	{
		public ThreatLevelType Category
		{
			readonly get; [PublicizedFrom(EAccessModifier.Private)]
			set;
		}

		public float Numeric
		{
			get
			{
				return this.numeric;
			}
			set
			{
				if (value < 0.3f && Time.time - this.suspenseTimer <= 30f)
				{
					this.numeric = 0.3f;
					this.Category = ThreatLevelType.Spooked;
					return;
				}
				this.numeric = value;
				this.Category = ((value < 0.3f) ? ThreatLevelType.Safe : ((value < 0.7f) ? ThreatLevelType.Spooked : ThreatLevelType.Panicked));
				if (this.Category == ThreatLevelType.Spooked)
				{
					this.suspenseTimer = Time.time;
					return;
				}
				if (this.Category == ThreatLevelType.Panicked)
				{
					this.suspenseTimer = 0f;
				}
			}
		}

		public const float SPOOKED_THRESHOLD = 0.3f;

		public const float PANICKED_THRESHOLD = 0.7f;

		[PublicizedFrom(EAccessModifier.Private)]
		public const float SPOOKED_DECAY_TIME = 30f;

		[PublicizedFrom(EAccessModifier.Private)]
		public float suspenseTimer;

		[PublicizedFrom(EAccessModifier.Private)]
		public float numeric;
	}
}
