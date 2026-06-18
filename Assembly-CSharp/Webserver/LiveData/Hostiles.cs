using System;

namespace Webserver.LiveData
{
	public class Hostiles : EntityFilterList<EntityEnemy>
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		public override EntityEnemy predicate(Entity _e)
		{
			EntityEnemy entityEnemy = _e as EntityEnemy;
			if (entityEnemy != null && entityEnemy.IsAlive())
			{
				return entityEnemy;
			}
			return null;
		}

		public static readonly Hostiles Instance = new Hostiles();
	}
}
