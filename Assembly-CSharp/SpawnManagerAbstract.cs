using System;

public abstract class SpawnManagerAbstract
{
	public SpawnManagerAbstract(World _world)
	{
		this.world = _world;
	}

	public abstract void Update(string _spawnerName, bool _bSpawnEnemyEntities, object _userData);

	[PublicizedFrom(EAccessModifier.Protected)]
	public World world;
}
