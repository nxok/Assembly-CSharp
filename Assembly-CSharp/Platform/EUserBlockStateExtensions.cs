using System;

namespace Platform
{
	public static class EUserBlockStateExtensions
	{
		public static bool IsBlocked(this EUserBlockState blockState)
		{
			bool flag;
			if (blockState != EUserBlockState.NotBlocked)
			{
				if (blockState - EUserBlockState.InGame > 1)
				{
					throw new ArgumentOutOfRangeException("blockState", blockState, string.Format("{0} not implemented for {1}.{2}", "IsBlocked", "EUserBlockState", blockState));
				}
				flag = true;
			}
			else
			{
				flag = false;
			}
			return flag;
		}
	}
}
