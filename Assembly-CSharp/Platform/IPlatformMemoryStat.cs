using System;
using System.Text;

namespace Platform
{
	public interface IPlatformMemoryStat
	{
		string Name { get; }

		void RenderColumn(StringBuilder builder, MemoryStatColumn column, bool delta);

		void UpdateLast();
	}
}
