using System;

namespace DynamicMusic
{
	public interface IPassArbiter
	{
		bool WillAllowPass { get; }
	}
}
