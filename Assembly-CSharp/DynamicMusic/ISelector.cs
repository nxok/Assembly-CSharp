using System;

namespace DynamicMusic
{
	public interface ISelector<T>
	{
		T Select();
	}
}
