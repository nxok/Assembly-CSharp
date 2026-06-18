using System;

namespace WorldGenerationEngineFinal
{
	public class TownshipShared
	{
		public TownshipShared(WorldBuilder _worldBuilder)
		{
			this.worldBuilder = _worldBuilder;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly WorldBuilder worldBuilder;

		public int NextId;

		public readonly Vector2i[] dir4way = new Vector2i[]
		{
			new Vector2i(0, 1),
			new Vector2i(1, 0),
			new Vector2i(0, -1),
			new Vector2i(-1, 0)
		};
	}
}
