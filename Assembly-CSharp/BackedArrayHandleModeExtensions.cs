using System;

public static class BackedArrayHandleModeExtensions
{
	public static bool CanRead(this BackedArrayHandleMode mode)
	{
		bool flag;
		if (mode != BackedArrayHandleMode.ReadOnly)
		{
			if (mode != BackedArrayHandleMode.ReadWrite)
			{
				throw new ArgumentOutOfRangeException("mode", mode, string.Format("Unknown mode: {0}", mode));
			}
			flag = true;
		}
		else
		{
			flag = true;
		}
		return flag;
	}

	public static bool CanWrite(this BackedArrayHandleMode mode)
	{
		bool flag;
		if (mode != BackedArrayHandleMode.ReadOnly)
		{
			if (mode != BackedArrayHandleMode.ReadWrite)
			{
				throw new ArgumentOutOfRangeException("mode", mode, string.Format("Unknown mode: {0}", mode));
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
