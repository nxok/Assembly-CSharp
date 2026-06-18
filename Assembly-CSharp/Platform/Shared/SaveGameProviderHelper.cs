using System;
using System.Collections.Generic;

namespace Platform.Shared
{
	public static class SaveGameProviderHelper
	{
		public static IEnumerable<SaveDataManagedPath> GetManagedPathsFromBaseAndSubPaths(SaveDataManagedPath path, string basePath, IEnumerable<string> subPaths)
		{
			foreach (string text in subPaths)
			{
				yield return SaveGameProviderHelper.GetManagedPathFromBaseAndSubPath(path, basePath, text);
			}
			IEnumerator<string> enumerator = null;
			yield break;
			yield break;
		}

		public static SaveDataManagedPath GetManagedPathFromBaseAndSubPath(SaveDataManagedPath path, string basePath, string subPath)
		{
			ReadOnlySpan<char> readOnlySpan = subPath.AsSpan(basePath.Length).TrimStart("\\/");
			return path.GetChildPath(readOnlySpan);
		}
	}
}
