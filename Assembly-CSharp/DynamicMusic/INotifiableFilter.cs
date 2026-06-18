using System;

namespace DynamicMusic
{
	public interface INotifiableFilter<T> : INotifiable, IFilter<T>
	{
	}
}
