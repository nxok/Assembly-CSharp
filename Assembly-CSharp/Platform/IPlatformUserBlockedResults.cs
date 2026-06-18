using System;

namespace Platform
{
	public interface IPlatformUserBlockedResults
	{
		IPlatformUser User { get; }

		void Block(EBlockType blockType);

		void BlockAll()
		{
			foreach (EBlockType eblockType in EnumUtils.Values<EBlockType>())
			{
				this.Block(eblockType);
			}
		}

		void Error();
	}
}
