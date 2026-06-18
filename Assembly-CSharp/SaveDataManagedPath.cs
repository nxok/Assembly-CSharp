using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;

public class SaveDataManagedPath : IEquatable<SaveDataManagedPath>, IComparable<SaveDataManagedPath>, IComparable
{
	public SaveDataManagedPath(StringSpan pathRelativeToRoot)
	{
		string text;
		this..ctor(SaveDataManagedPath.TryFormatPath(pathRelativeToRoot.AsSpan(), out text) ? text : pathRelativeToRoot.ToString(), true);
	}

	public SaveDataManagedPath(string pathRelativeToRoot)
		: this(pathRelativeToRoot, false)
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public SaveDataManagedPath(string pathRelativeToRoot, bool alreadyFormatted)
	{
		if (pathRelativeToRoot == null)
		{
			throw new ArgumentNullException("pathRelativeToRoot");
		}
		if (alreadyFormatted)
		{
			this.PathRelativeToRoot = pathRelativeToRoot;
		}
		else
		{
			string text;
			this.PathRelativeToRoot = (SaveDataManagedPath.TryFormatPath(pathRelativeToRoot, out text) ? text : pathRelativeToRoot);
		}
		bool flag;
		try
		{
			flag = Path.IsPathRooted(this.PathRelativeToRoot);
		}
		catch (ArgumentException ex)
		{
			throw new ArgumentException("Failed to check if path was rooted. " + this.PathRelativeToRoot, "pathRelativeToRoot", ex);
		}
		if (flag)
		{
			throw new ArgumentException("Path should not be rooted. " + this.PathRelativeToRoot, "pathRelativeToRoot");
		}
		this.Type = this.GetSaveDataType();
		this.SlotPathRange = this.GetSlotPathRange();
		this.PathRelativeToSlotRange = this.GetPathRelativeToSlotRange();
		this.Slot = new SaveDataSlot(this);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public unsafe static bool TryFormatPath(ReadOnlySpan<char> unformattedPath, out string formattedPath)
	{
		ReadOnlySpan<char> readOnlySpan = unformattedPath.Trim(" \\/");
		if (readOnlySpan.Length <= 0)
		{
			formattedPath = string.Empty;
			return true;
		}
		bool flag = false;
		int num = 0;
		bool flag2 = false;
		ReadOnlySpan<char> readOnlySpan2 = readOnlySpan;
		int i = 0;
		while (i < readOnlySpan2.Length)
		{
			char c = (char)(*readOnlySpan2[i]);
			if (c == '/')
			{
				goto IL_004C;
			}
			if (c == '\\')
			{
				flag = true;
				goto IL_004C;
			}
			flag2 = false;
			IL_005B:
			i++;
			continue;
			IL_004C:
			if (!flag2)
			{
				flag2 = true;
				goto IL_005B;
			}
			num++;
			goto IL_005B;
		}
		if (readOnlySpan.Length == unformattedPath.Length && !flag && num == 0)
		{
			formattedPath = null;
			return false;
		}
		fixed (char* pinnableReference = readOnlySpan.GetPinnableReference())
		{
			void* ptr = (void*)pinnableReference;
			ValueTuple<IntPtr, int> valueTuple = new ValueTuple<IntPtr, int>((IntPtr)ptr, readOnlySpan.Length);
			formattedPath = string.Create<ValueTuple<IntPtr, int>>(readOnlySpan.Length - num, valueTuple, delegate(Span<char> span, [TupleElementNames(new string[] { null, "Length" })] ValueTuple<IntPtr, int> data)
			{
				IntPtr item = data.Item1;
				int item2 = data.Item2;
				ReadOnlySpan<char> readOnlySpan3 = new ReadOnlySpan<char>(item.ToPointer(), item2);
				int num2 = 0;
				bool flag3 = false;
				ReadOnlySpan<char> readOnlySpan4 = readOnlySpan3;
				for (int j = 0; j < readOnlySpan4.Length; j++)
				{
					char c2 = (char)(*readOnlySpan4[j]);
					if (c2 == '\\' || c2 == '/')
					{
						if (!flag3)
						{
							flag3 = true;
							*span[num2++] = '/';
						}
					}
					else
					{
						flag3 = false;
						*span[num2++] = c2;
					}
				}
			});
		}
		return true;
	}

	public SaveDataType Type { get; }

	[PublicizedFrom(EAccessModifier.Private)]
	public SaveDataType GetSaveDataType()
	{
		foreach (SaveDataType saveDataType in EnumUtils.Values<SaveDataType>())
		{
			if (!saveDataType.IsRoot())
			{
				string pathRaw = saveDataType.GetPathRaw();
				if (this.PathRelativeToRoot.IndexOf(pathRaw, StringComparison.Ordinal) == 0 && this.PathRelativeToRoot.Length >= pathRaw.Length + 2 && this.PathRelativeToRoot[pathRaw.Length] == '/')
				{
					return saveDataType;
				}
			}
		}
		return SaveDataType.User;
	}

	public Range SlotPathRange
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
	}

	public StringSpan SlotPath
	{
		get
		{
			string pathRelativeToRoot = this.PathRelativeToRoot;
			Range slotPathRange = this.SlotPathRange;
			int length = pathRelativeToRoot.Length;
			int offset = slotPathRange.Start.GetOffset(length);
			int num = slotPathRange.End.GetOffset(length) - offset;
			return pathRelativeToRoot.Substring(offset, num);
		}
	}

	public SaveDataSlot Slot { get; }

	[PublicizedFrom(EAccessModifier.Private)]
	public Range GetSlotPathRange()
	{
		int num = this.Type.GetSlotPathDepth();
		string pathRaw = this.Type.GetPathRaw();
		if (num <= 0 || pathRaw.Length == 0 || this.PathRelativeToRoot.Length < pathRaw.Length + 2)
		{
			return new Range(pathRaw.Length, pathRaw.Length);
		}
		for (int i = pathRaw.Length + 1; i < this.PathRelativeToRoot.Length; i++)
		{
			if (this.PathRelativeToRoot[i] == '/')
			{
				num--;
				if (num <= 0)
				{
					int num2 = pathRaw.Length + 1;
					int num3 = i;
					return new Range(num2, num3);
				}
			}
		}
		return new Range(pathRaw.Length, pathRaw.Length);
	}

	public Range PathRelativeToSlotRange
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
	}

	public StringSpan PathRelativeToSlot
	{
		get
		{
			string pathRelativeToRoot = this.PathRelativeToRoot;
			Range pathRelativeToSlotRange = this.PathRelativeToSlotRange;
			int length = pathRelativeToRoot.Length;
			int offset = pathRelativeToSlotRange.Start.GetOffset(length);
			int num = pathRelativeToSlotRange.End.GetOffset(length) - offset;
			return pathRelativeToRoot.Substring(offset, num);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Range GetPathRelativeToSlotRange()
	{
		int offset = this.SlotPathRange.End.GetOffset(this.PathRelativeToRoot.Length);
		if (offset >= this.PathRelativeToRoot.Length)
		{
			return new Range(offset, offset);
		}
		int num = ((this.PathRelativeToRoot[offset] == '/') ? (offset + 1) : offset);
		if (num >= this.PathRelativeToRoot.Length)
		{
			return new Range(offset, offset);
		}
		return new Range(num, this.PathRelativeToRoot.Length);
	}

	public string GetOriginalPath()
	{
		return GameIO.GetNormalizedPath(Path.Combine(SaveDataUtils.s_saveDataRootPathPrefix, this.PathRelativeToRoot));
	}

	public SaveDataManagedPath GetChildPath(StringSpan childPath)
	{
		return new SaveDataManagedPath(SpanUtils.Concat(this.PathRelativeToRoot, "/", childPath));
	}

	public bool TryGetParentPath(out SaveDataManagedPath parentPath)
	{
		if (this.PathRelativeToRoot.Length <= 0)
		{
			parentPath = null;
			return false;
		}
		int num = this.PathRelativeToRoot.LastIndexOf('/');
		if (num < 0)
		{
			parentPath = SaveDataManagedPath.RootPath;
			return true;
		}
		parentPath = new SaveDataManagedPath(this.PathRelativeToRoot.Substring(0, num));
		return true;
	}

	public bool IsParentOf(SaveDataManagedPath childPath)
	{
		return SaveDataManagedPath.<IsParentOf>g__IsParentOfInternal|28_0(this.PathRelativeToRoot, childPath.PathRelativeToRoot);
	}

	public override string ToString()
	{
		return this.PathRelativeToRoot;
	}

	public bool Equals(SaveDataManagedPath other)
	{
		return other != null && (this == other || this.PathRelativeToRoot == other.PathRelativeToRoot);
	}

	public override bool Equals(object obj)
	{
		return obj != null && (this == obj || (!(obj.GetType() != base.GetType()) && this.Equals((SaveDataManagedPath)obj)));
	}

	public override int GetHashCode()
	{
		return this.PathRelativeToRoot.GetHashCode();
	}

	public static bool operator ==(SaveDataManagedPath left, SaveDataManagedPath right)
	{
		return object.Equals(left, right);
	}

	public static bool operator !=(SaveDataManagedPath left, SaveDataManagedPath right)
	{
		return !object.Equals(left, right);
	}

	public int CompareTo(SaveDataManagedPath other)
	{
		if (other == null)
		{
			return 1;
		}
		if (this == other)
		{
			return 0;
		}
		return string.Compare(this.PathRelativeToRoot, other.PathRelativeToRoot, StringComparison.Ordinal);
	}

	public int CompareTo(object obj)
	{
		if (obj == null)
		{
			return 1;
		}
		if (this == obj)
		{
			return 0;
		}
		SaveDataManagedPath saveDataManagedPath = obj as SaveDataManagedPath;
		if (saveDataManagedPath == null)
		{
			throw new ArgumentException("Object must be of type SaveDataManagedPath");
		}
		return this.CompareTo(saveDataManagedPath);
	}

	public static bool operator <(SaveDataManagedPath left, SaveDataManagedPath right)
	{
		return Comparer<SaveDataManagedPath>.Default.Compare(left, right) < 0;
	}

	public static bool operator >(SaveDataManagedPath left, SaveDataManagedPath right)
	{
		return Comparer<SaveDataManagedPath>.Default.Compare(left, right) > 0;
	}

	public static bool operator <=(SaveDataManagedPath left, SaveDataManagedPath right)
	{
		return Comparer<SaveDataManagedPath>.Default.Compare(left, right) <= 0;
	}

	public static bool operator >=(SaveDataManagedPath left, SaveDataManagedPath right)
	{
		return Comparer<SaveDataManagedPath>.Default.Compare(left, right) >= 0;
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Internal)]
	public static bool <IsParentOf>g__IsParentOfInternal|28_0(string parent, string child)
	{
		if (parent.Length >= child.Length)
		{
			return false;
		}
		if (parent.Length == 0)
		{
			return true;
		}
		if (child[parent.Length] != '/')
		{
			return false;
		}
		for (int i = 0; i < parent.Length; i++)
		{
			if (parent[i] != child[i])
			{
				return false;
			}
		}
		return true;
	}

	public static readonly SaveDataManagedPath RootPath = new SaveDataManagedPath(string.Empty);

	public readonly string PathRelativeToRoot;
}
