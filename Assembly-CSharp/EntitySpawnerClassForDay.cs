using System;
using System.Collections.Generic;

public class EntitySpawnerClassForDay
{
	public void AddForDay(int _day, EntitySpawnerClass _class)
	{
		if (_day != 0 && this.days.Count == 0)
		{
			this.days.Add(_class);
		}
		while (this.days.Count <= _day)
		{
			this.days.Add(null);
		}
		this.days[_day] = _class;
	}

	public EntitySpawnerClass Day(int _day)
	{
		if (this.days.Count == 0)
		{
			return null;
		}
		if (this.bWrapDays && _day > 0 && _day >= this.days.Count)
		{
			if (this.days.Count > 1)
			{
				_day %= this.days.Count - 1;
				if (_day == 0)
				{
					_day = this.days.Count - 1;
				}
			}
			else
			{
				_day = 1;
			}
			if (_day == 0)
			{
				_day++;
			}
		}
		else if (this.bClampDays && _day >= this.days.Count && this.days.Count > 0)
		{
			_day = this.days.Count - 1;
		}
		if (_day >= this.days.Count || this.days[_day] == null)
		{
			return this.days[0];
		}
		return this.days[_day];
	}

	public int Count()
	{
		return this.days.Count;
	}

	public bool bDynamicSpawner;

	public bool bWrapDays;

	public bool bClampDays;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<EntitySpawnerClass> days = new List<EntitySpawnerClass>();
}
