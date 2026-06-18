using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

public static class SdFile
{
	public static StreamReader OpenText(string path)
	{
		SaveDataManagedPath saveDataManagedPath;
		if (SaveDataUtils.TryGetManagedPath(path, out saveDataManagedPath))
		{
			return SdFile.ManagedOpenText(saveDataManagedPath);
		}
		return File.OpenText(path);
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public static StreamReader ManagedOpenText(SaveDataManagedPath path)
	{
		return new StreamReader(SdFile.ManagedOpen(path, FileMode.Open, FileAccess.Read, FileShare.Read));
	}

	public static StreamWriter CreateText(string path)
	{
		SaveDataManagedPath saveDataManagedPath;
		if (SaveDataUtils.TryGetManagedPath(path, out saveDataManagedPath))
		{
			return SdFile.ManagedCreateText(saveDataManagedPath);
		}
		return File.CreateText(path);
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public static StreamWriter ManagedCreateText(SaveDataManagedPath path)
	{
		return new StreamWriter(SdFile.ManagedOpen(path, FileMode.Create, FileAccess.Write, FileShare.Read));
	}

	public static StreamWriter AppendText(string path)
	{
		SaveDataManagedPath saveDataManagedPath;
		if (SaveDataUtils.TryGetManagedPath(path, out saveDataManagedPath))
		{
			return SdFile.ManagedAppendText(saveDataManagedPath);
		}
		return File.AppendText(path);
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public static StreamWriter ManagedAppendText(SaveDataManagedPath path)
	{
		return new StreamWriter(SdFile.ManagedOpen(path, FileMode.Append, FileAccess.Write, FileShare.Read));
	}

	public static void Copy(string sourceFileName, string destFileName)
	{
		SaveDataManagedPath saveDataManagedPath;
		bool flag = SaveDataUtils.TryGetManagedPath(sourceFileName, out saveDataManagedPath);
		SaveDataManagedPath saveDataManagedPath2;
		bool flag2 = SaveDataUtils.TryGetManagedPath(destFileName, out saveDataManagedPath2);
		if (flag && flag2)
		{
			SdFile.ManagedToManagedCopy(saveDataManagedPath, saveDataManagedPath2, false);
			return;
		}
		if (flag)
		{
			SdFile.ManagedToUnmanagedCopy(saveDataManagedPath, destFileName, false);
			return;
		}
		if (flag2)
		{
			SdFile.UnmanagedToManagedCopy(sourceFileName, saveDataManagedPath2, false);
			return;
		}
		File.Copy(sourceFileName, destFileName);
	}

	public static void Copy(string sourceFileName, string destFileName, bool overwrite)
	{
		SaveDataManagedPath saveDataManagedPath;
		bool flag = SaveDataUtils.TryGetManagedPath(sourceFileName, out saveDataManagedPath);
		SaveDataManagedPath saveDataManagedPath2;
		bool flag2 = SaveDataUtils.TryGetManagedPath(destFileName, out saveDataManagedPath2);
		if (flag && flag2)
		{
			SdFile.ManagedToManagedCopy(saveDataManagedPath, saveDataManagedPath2, overwrite);
			return;
		}
		if (flag)
		{
			SdFile.ManagedToUnmanagedCopy(saveDataManagedPath, destFileName, overwrite);
			return;
		}
		if (flag2)
		{
			SdFile.UnmanagedToManagedCopy(sourceFileName, saveDataManagedPath2, overwrite);
			return;
		}
		File.Copy(sourceFileName, destFileName, overwrite);
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public static void ManagedToManagedCopy(SaveDataManagedPath sourceFileName, SaveDataManagedPath destFileName, bool overwrite)
	{
		using (Stream stream = SdFile.ManagedOpen(sourceFileName, FileMode.Open, FileAccess.Read, FileShare.Read))
		{
			using (Stream stream2 = SdFile.ManagedOpen(destFileName, overwrite ? FileMode.Create : FileMode.CreateNew, FileAccess.Write, FileShare.Read))
			{
				StreamUtils.StreamCopy(stream, stream2, null, true);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public static void ManagedToUnmanagedCopy(SaveDataManagedPath sourceFileName, string destFileName, bool overwrite)
	{
		using (Stream stream = SdFile.ManagedOpen(sourceFileName, FileMode.Open, FileAccess.Read, FileShare.Read))
		{
			using (FileStream fileStream = File.Open(destFileName, overwrite ? FileMode.Create : FileMode.CreateNew, FileAccess.Write, FileShare.Read))
			{
				StreamUtils.StreamCopy(stream, fileStream, null, true);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public static void UnmanagedToManagedCopy(string sourceFileName, SaveDataManagedPath destFileName, bool overwrite)
	{
		using (FileStream fileStream = File.Open(sourceFileName, FileMode.Open, FileAccess.Read, FileShare.Read))
		{
			using (Stream stream = SdFile.ManagedOpen(destFileName, overwrite ? FileMode.Create : FileMode.CreateNew, FileAccess.Write, FileShare.Read))
			{
				StreamUtils.StreamCopy(fileStream, stream, null, true);
			}
		}
	}

	public static Stream Create(string path)
	{
		SaveDataManagedPath saveDataManagedPath;
		if (SaveDataUtils.TryGetManagedPath(path, out saveDataManagedPath))
		{
			return SdFile.ManagedCreate(saveDataManagedPath);
		}
		return File.Create(path);
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public static Stream ManagedCreate(SaveDataManagedPath path)
	{
		return SdFile.ManagedOpen(path, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
	}

	public static void Delete(string path)
	{
		SaveDataManagedPath saveDataManagedPath;
		if (SaveDataUtils.TryGetManagedPath(path, out saveDataManagedPath))
		{
			SdFile.ManagedDelete(saveDataManagedPath);
			return;
		}
		File.Delete(path);
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public static void ManagedDelete(SaveDataManagedPath path)
	{
		SaveDataUtils.SaveDataManager.ManagedFileDelete(path);
	}

	public static bool Exists(string path)
	{
		SaveDataManagedPath saveDataManagedPath;
		if (SaveDataUtils.TryGetManagedPath(path, out saveDataManagedPath))
		{
			return SdFile.ManagedExists(saveDataManagedPath);
		}
		return File.Exists(path);
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public static bool ManagedExists(SaveDataManagedPath path)
	{
		return SaveDataUtils.SaveDataManager.ManagedFileExists(path);
	}

	public static Stream Open(string path, FileMode mode)
	{
		SaveDataManagedPath saveDataManagedPath;
		if (SaveDataUtils.TryGetManagedPath(path, out saveDataManagedPath))
		{
			return SdFile.ManagedOpen(saveDataManagedPath, mode, (mode == FileMode.Append) ? FileAccess.Write : FileAccess.ReadWrite, FileShare.None);
		}
		return File.Open(path, mode);
	}

	public static Stream Open(string path, FileMode mode, FileAccess access)
	{
		SaveDataManagedPath saveDataManagedPath;
		if (SaveDataUtils.TryGetManagedPath(path, out saveDataManagedPath))
		{
			return SdFile.ManagedOpen(saveDataManagedPath, mode, access, FileShare.None);
		}
		return File.Open(path, mode, access);
	}

	public static Stream Open(string path, FileMode mode, FileAccess access, FileShare share)
	{
		SaveDataManagedPath saveDataManagedPath;
		if (SaveDataUtils.TryGetManagedPath(path, out saveDataManagedPath))
		{
			return SdFile.ManagedOpen(saveDataManagedPath, mode, access, share);
		}
		return File.Open(path, mode, access, share);
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public static Stream ManagedOpen(SaveDataManagedPath path, FileMode mode, FileAccess access, FileShare share)
	{
		return SaveDataUtils.SaveDataManager.ManagedFileOpen(path, mode, access, share);
	}

	public static DateTime GetLastWriteTime(string path)
	{
		SaveDataManagedPath saveDataManagedPath;
		if (SaveDataUtils.TryGetManagedPath(path, out saveDataManagedPath))
		{
			return SdFile.ManagedGetLastWriteTime(saveDataManagedPath);
		}
		return File.GetLastWriteTime(path);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static DateTime ManagedGetLastWriteTime(SaveDataManagedPath path)
	{
		return SdFile.ManagedGetLastWriteTimeUtc(path).ToLocalTime();
	}

	public static DateTime GetLastWriteTimeUtc(string path)
	{
		SaveDataManagedPath saveDataManagedPath;
		if (SaveDataUtils.TryGetManagedPath(path, out saveDataManagedPath))
		{
			return SdFile.ManagedGetLastWriteTimeUtc(saveDataManagedPath);
		}
		return File.GetLastWriteTimeUtc(path);
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public static DateTime ManagedGetLastWriteTimeUtc(SaveDataManagedPath path)
	{
		return SaveDataUtils.SaveDataManager.ManagedFileGetLastWriteTimeUtc(path);
	}

	public static Stream OpenRead(string path)
	{
		SaveDataManagedPath saveDataManagedPath;
		if (SaveDataUtils.TryGetManagedPath(path, out saveDataManagedPath))
		{
			return SdFile.ManagedOpen(saveDataManagedPath, FileMode.Open, FileAccess.Read, FileShare.Read);
		}
		return File.OpenRead(path);
	}

	public static Stream OpenWrite(string path)
	{
		SaveDataManagedPath saveDataManagedPath;
		if (SaveDataUtils.TryGetManagedPath(path, out saveDataManagedPath))
		{
			return SdFile.ManagedOpen(saveDataManagedPath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
		}
		return File.OpenWrite(path);
	}

	public static string ReadAllText(string path)
	{
		SaveDataManagedPath saveDataManagedPath;
		if (SaveDataUtils.TryGetManagedPath(path, out saveDataManagedPath))
		{
			return SdFile.ManagedReadAllText(saveDataManagedPath, Encoding.UTF8);
		}
		return File.ReadAllText(path);
	}

	public static string ReadAllText(string path, Encoding encoding)
	{
		SaveDataManagedPath saveDataManagedPath;
		if (SaveDataUtils.TryGetManagedPath(path, out saveDataManagedPath))
		{
			return SdFile.ManagedReadAllText(saveDataManagedPath, encoding);
		}
		return File.ReadAllText(path, encoding);
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public static string ManagedReadAllText(SaveDataManagedPath path, Encoding encoding)
	{
		string text;
		using (Stream stream = SdFile.ManagedOpen(path, FileMode.Open, FileAccess.Read, FileShare.Read))
		{
			using (StreamReader streamReader = new StreamReader(stream, encoding))
			{
				text = streamReader.ReadToEnd();
			}
		}
		return text;
	}

	public static void WriteAllText(string path, string contents)
	{
		SaveDataManagedPath saveDataManagedPath;
		if (SaveDataUtils.TryGetManagedPath(path, out saveDataManagedPath))
		{
			SdFile.ManagedWriteAllText(saveDataManagedPath, contents, SdFile.UTF8NoBom);
			return;
		}
		File.WriteAllText(path, contents);
	}

	public static void WriteAllText(string path, string contents, Encoding encoding)
	{
		SaveDataManagedPath saveDataManagedPath;
		if (SaveDataUtils.TryGetManagedPath(path, out saveDataManagedPath))
		{
			SdFile.ManagedWriteAllText(saveDataManagedPath, contents, encoding);
			return;
		}
		File.WriteAllText(path, contents, encoding);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ManagedWriteAllText(SaveDataManagedPath path, string contents, Encoding encoding)
	{
		using (Stream stream = SdFile.ManagedOpen(path, FileMode.Create, FileAccess.Write, FileShare.Read))
		{
			using (StreamWriter streamWriter = new StreamWriter(stream, encoding))
			{
				streamWriter.Write(contents);
			}
		}
	}

	public static byte[] ReadAllBytes(string path)
	{
		SaveDataManagedPath saveDataManagedPath;
		if (SaveDataUtils.TryGetManagedPath(path, out saveDataManagedPath))
		{
			return SdFile.ManagedReadAllBytes(saveDataManagedPath);
		}
		return File.ReadAllBytes(path);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static byte[] ManagedReadAllBytes(SaveDataManagedPath path)
	{
		byte[] array;
		using (PooledExpandableMemoryStream pooledExpandableMemoryStream = MemoryPools.poolMemoryStream.AllocSync(true))
		{
			using (Stream stream = SdFile.ManagedOpen(path, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				stream.CopyTo(pooledExpandableMemoryStream);
				array = pooledExpandableMemoryStream.ToArray();
			}
		}
		return array;
	}

	public static void WriteAllBytes(string path, byte[] bytes)
	{
		SaveDataManagedPath saveDataManagedPath;
		if (SaveDataUtils.TryGetManagedPath(path, out saveDataManagedPath))
		{
			SdFile.ManagedWriteAllBytes(saveDataManagedPath, bytes);
			return;
		}
		File.WriteAllBytes(path, bytes);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ManagedWriteAllBytes(SaveDataManagedPath path, byte[] bytes)
	{
		using (Stream stream = SdFile.ManagedOpen(path, FileMode.Create, FileAccess.Write, FileShare.Read))
		{
			stream.Write(bytes, 0, bytes.Length);
		}
	}

	public static string[] ReadAllLines(string path)
	{
		SaveDataManagedPath saveDataManagedPath;
		if (SaveDataUtils.TryGetManagedPath(path, out saveDataManagedPath))
		{
			return SdFile.ManagedReadAllLines(saveDataManagedPath, Encoding.UTF8);
		}
		return File.ReadAllLines(path);
	}

	public static string[] ReadAllLines(string path, Encoding encoding)
	{
		SaveDataManagedPath saveDataManagedPath;
		if (SaveDataUtils.TryGetManagedPath(path, out saveDataManagedPath))
		{
			return SdFile.ManagedReadAllLines(saveDataManagedPath, encoding);
		}
		return File.ReadAllLines(path, encoding);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string[] ManagedReadAllLines(SaveDataManagedPath path, Encoding encoding)
	{
		List<string> list = new List<string>();
		string[] array;
		using (Stream stream = SdFile.ManagedOpen(path, FileMode.Open, FileAccess.Read, FileShare.Read))
		{
			using (StreamReader streamReader = new StreamReader(stream, encoding))
			{
				for (;;)
				{
					string text = streamReader.ReadLine();
					if (text == null)
					{
						break;
					}
					list.Add(text);
				}
				array = list.ToArray();
			}
		}
		return array;
	}

	public static IEnumerable<string> ReadLines(string path)
	{
		SaveDataManagedPath saveDataManagedPath;
		if (SaveDataUtils.TryGetManagedPath(path, out saveDataManagedPath))
		{
			return SdFile.ManagedReadLines(saveDataManagedPath, Encoding.UTF8);
		}
		return File.ReadLines(path);
	}

	public static IEnumerable<string> ReadLines(string path, Encoding encoding)
	{
		SaveDataManagedPath saveDataManagedPath;
		if (SaveDataUtils.TryGetManagedPath(path, out saveDataManagedPath))
		{
			return SdFile.ManagedReadLines(saveDataManagedPath, encoding);
		}
		return File.ReadLines(path, encoding);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static IEnumerable<string> ManagedReadLines(SaveDataManagedPath path, Encoding encoding)
	{
		SdFile.<ManagedReadLines>d__43 <ManagedReadLines>d__ = new SdFile.<ManagedReadLines>d__43(-2);
		<ManagedReadLines>d__.<>3__path = path;
		<ManagedReadLines>d__.<>3__encoding = encoding;
		return <ManagedReadLines>d__;
	}

	public static void WriteAllLines(string path, string[] contents)
	{
		SaveDataManagedPath saveDataManagedPath;
		if (SaveDataUtils.TryGetManagedPath(path, out saveDataManagedPath))
		{
			SdFile.ManagedWriteAllLines(saveDataManagedPath, contents, SdFile.UTF8NoBom);
			return;
		}
		File.WriteAllLines(path, contents);
	}

	public static void WriteAllLines(string path, string[] contents, Encoding encoding)
	{
		SaveDataManagedPath saveDataManagedPath;
		if (SaveDataUtils.TryGetManagedPath(path, out saveDataManagedPath))
		{
			SdFile.ManagedWriteAllLines(saveDataManagedPath, contents, encoding);
			return;
		}
		File.WriteAllLines(path, contents, encoding);
	}

	public static void WriteAllLines(string path, IEnumerable<string> contents)
	{
		SaveDataManagedPath saveDataManagedPath;
		if (SaveDataUtils.TryGetManagedPath(path, out saveDataManagedPath))
		{
			SdFile.ManagedWriteAllLines(saveDataManagedPath, contents, SdFile.UTF8NoBom);
			return;
		}
		File.WriteAllLines(path, contents);
	}

	public static void WriteAllLines(string path, IEnumerable<string> contents, Encoding encoding)
	{
		SaveDataManagedPath saveDataManagedPath;
		if (SaveDataUtils.TryGetManagedPath(path, out saveDataManagedPath))
		{
			SdFile.ManagedWriteAllLines(saveDataManagedPath, contents, encoding);
			return;
		}
		File.WriteAllLines(path, contents, encoding);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ManagedWriteAllLines(SaveDataManagedPath path, IEnumerable<string> contents, Encoding encoding)
	{
		using (Stream stream = SdFile.ManagedOpen(path, FileMode.Create, FileAccess.Write, FileShare.Read))
		{
			using (StreamWriter streamWriter = new StreamWriter(stream, encoding))
			{
				SdFile.ManagedWriteAllLines(streamWriter, contents);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ManagedWriteAllLines(TextWriter writer, IEnumerable<string> contents)
	{
		try
		{
			foreach (string text in contents)
			{
				writer.WriteLine(text);
			}
		}
		finally
		{
			if (writer != null)
			{
				((IDisposable)writer).Dispose();
			}
		}
	}

	public static void AppendAllText(string path, string contents)
	{
		SaveDataManagedPath saveDataManagedPath;
		if (SaveDataUtils.TryGetManagedPath(path, out saveDataManagedPath))
		{
			SdFile.ManagedAppendAllText(saveDataManagedPath, contents, SdFile.UTF8NoBom);
			return;
		}
		File.AppendAllText(path, contents);
	}

	public static void AppendAllText(string path, string contents, Encoding encoding)
	{
		SaveDataManagedPath saveDataManagedPath;
		if (SaveDataUtils.TryGetManagedPath(path, out saveDataManagedPath))
		{
			SdFile.ManagedAppendAllText(saveDataManagedPath, contents, encoding);
			return;
		}
		File.AppendAllText(path, contents, encoding);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ManagedAppendAllText(SaveDataManagedPath path, string contents, Encoding encoding)
	{
		using (Stream stream = SdFile.ManagedOpen(path, FileMode.Append, FileAccess.Write, FileShare.Read))
		{
			using (StreamWriter streamWriter = new StreamWriter(stream, encoding))
			{
				streamWriter.Write(contents);
			}
		}
	}

	public static void AppendAllLines(string path, IEnumerable<string> contents)
	{
		SaveDataManagedPath saveDataManagedPath;
		if (SaveDataUtils.TryGetManagedPath(path, out saveDataManagedPath))
		{
			SdFile.ManagedAppendAllLines(saveDataManagedPath, contents, SdFile.UTF8NoBom);
			return;
		}
		File.AppendAllLines(path, contents);
	}

	public static void AppendAllLines(string path, IEnumerable<string> contents, Encoding encoding)
	{
		SaveDataManagedPath saveDataManagedPath;
		if (SaveDataUtils.TryGetManagedPath(path, out saveDataManagedPath))
		{
			SdFile.ManagedAppendAllLines(saveDataManagedPath, contents, encoding);
			return;
		}
		File.AppendAllLines(path, contents, encoding);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ManagedAppendAllLines(SaveDataManagedPath path, IEnumerable<string> contents, Encoding encoding)
	{
		using (Stream stream = SdFile.ManagedOpen(path, FileMode.Append, FileAccess.Write, FileShare.Read))
		{
			using (StreamWriter streamWriter = new StreamWriter(stream, encoding))
			{
				SdFile.ManagedWriteAllLines(streamWriter, contents);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Encoding UTF8NoBom = new UTF8Encoding(false);
}
