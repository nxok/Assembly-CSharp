using System;

namespace Platform
{
	public interface IPlatformMemory
	{
		IPlatformMemorySampler CreateSampler();
	}
}
