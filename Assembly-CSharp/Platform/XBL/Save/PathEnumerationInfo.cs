using System;
using System.Collections.Generic;
using System.Linq;

namespace Platform.XBL.Save
{
	public sealed class PathEnumerationInfo
	{
		public PathEnumerationInfo(string relativePath, bool isDirectory, bool isFile)
		{
			this.RelativePath = relativePath;
			this.IsDirectory = isDirectory;
			this.IsFile = isFile;
		}

		public override string ToString()
		{
			return string.Format("{0}[{1}=\"{2}\", {3}={4}, {5}={6}]", new object[] { "PathEnumerationInfo", "RelativePath", this.RelativePath, "IsDirectory", this.IsDirectory, "IsFile", this.IsFile });
		}

		public void UsedOnlyForAOTCodeGeneration()
		{
			IEnumerable<SaveDataManagedPath> enumerable = from _ in Enumerable.Empty<PathEnumerationInfo>()
				select null;
			enumerable.Select((SaveDataManagedPath _) => null);
			enumerable.Select((SaveDataManagedPath _) => null);
			enumerable.Select((SaveDataManagedPath _) => null);
			throw new InvalidOperationException("This method is used for AOT code generation only. Do not call it at runtime.");
		}

		public readonly string RelativePath;

		public readonly bool IsDirectory;

		public readonly bool IsFile;
	}
}
