using System;

namespace Webserver.LiveData
{
	public class Animals : EntityFilterList<EntityAnimal>
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		public override EntityAnimal predicate(Entity _e)
		{
			EntityAnimal entityAnimal = _e as EntityAnimal;
			if (entityAnimal != null && entityAnimal.IsAlive())
			{
				return entityAnimal;
			}
			return null;
		}

		public static readonly Animals Instance = new Animals();
	}
}
