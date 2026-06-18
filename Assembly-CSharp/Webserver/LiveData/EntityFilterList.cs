using System;
using System.Collections.Generic;

namespace Webserver.LiveData
{
	public abstract class EntityFilterList<T> where T : Entity
	{
		public void Get(List<T> _list)
		{
			_list.Clear();
			try
			{
				List<Entity> list = GameManager.Instance.World.Entities.list;
				for (int i = 0; i < list.Count; i++)
				{
					Entity entity = list[i];
					T t = this.predicate(entity);
					if (t != null)
					{
						_list.Add(t);
					}
				}
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
			}
		}

		public int GetCount()
		{
			int num = 0;
			try
			{
				List<Entity> list = GameManager.Instance.World.Entities.list;
				for (int i = 0; i < list.Count; i++)
				{
					Entity entity = list[i];
					if (this.predicate(entity) != null)
					{
						num++;
					}
				}
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
			}
			return num;
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public abstract T predicate(Entity _e);

		[PublicizedFrom(EAccessModifier.Protected)]
		public EntityFilterList()
		{
		}
	}
}
