using System;

namespace WorldGenerationEngineFinal
{
	public struct PathTile
	{
		public PathTile.PathTileStates TileState;

		public enum PathTileStates : byte
		{
			Free,
			Blocked,
			Highway,
			Country
		}
	}
}
