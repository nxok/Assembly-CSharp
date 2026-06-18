using System;
using System.Collections.Generic;
using System.Text;

public class ObjectDuplicateChecker<T> where T : class, IEquatable<T>
{
	public void AddValue(T value)
	{
		int i = 0;
		while (i < this.Objects.Count)
		{
			if (this.Objects[i].Value == value)
			{
				if (this.Objects[i].Value == value)
				{
					this.Objects[i].SharedCount++;
					return;
				}
				this.Objects[i].DupeCount++;
				return;
			}
			else
			{
				i++;
			}
		}
		this.Objects.Add(new ObjectDuplicateChecker<T>.ObjectInfo
		{
			Value = value,
			DupeCount = 0,
			SharedCount = 1
		});
	}

	public string GenerateReport()
	{
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		StringBuilder stringBuilder = new StringBuilder();
		foreach (ObjectDuplicateChecker<T>.ObjectInfo objectInfo in this.Objects)
		{
			int sizeAuto = MemoryTracker.GetSizeAuto(objectInfo.Value);
			int num4 = sizeAuto + sizeAuto * objectInfo.DupeCount;
			num += num4;
			num2 += sizeAuto;
			num3 += sizeAuto * objectInfo.SharedCount;
		}
		stringBuilder.AppendLine(string.Format("    Size(in memory): {0:F2}kb, Size(est unduped): {1:F2}kb, Size(saved by sharing): {2:F2}kb", (double)num / 1024.0, (double)num2 / 1024.0, (double)num3 / 1024.0));
		return stringBuilder.ToString();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<ObjectDuplicateChecker<T>.ObjectInfo> Objects = new List<ObjectDuplicateChecker<T>.ObjectInfo>();

	[PublicizedFrom(EAccessModifier.Private)]
	public class ObjectInfo
	{
		public T Value;

		public int DupeCount;

		public int SharedCount;
	}
}
