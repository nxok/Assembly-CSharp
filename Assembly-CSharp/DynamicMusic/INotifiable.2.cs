using System;

namespace DynamicMusic
{
	public interface INotifiable<T>
	{
		void Notify(T _state);
	}
}
