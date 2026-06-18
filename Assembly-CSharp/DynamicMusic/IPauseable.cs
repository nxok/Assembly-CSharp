using System;

namespace DynamicMusic
{
	public interface IPauseable
	{
		void OnPause();

		void OnUnPause();
	}
}
