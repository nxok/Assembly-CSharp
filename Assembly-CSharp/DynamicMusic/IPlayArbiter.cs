using System;

namespace DynamicMusic
{
	public interface IPlayArbiter
	{
		bool WillAllowPlay { get; }
	}
}
