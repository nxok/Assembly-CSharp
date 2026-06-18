using System;
using System.IO;
using UnityEngine;

public static class TGALoader
{
	public static Texture2D LoadTGA(string fileName, bool mipMaps = true)
	{
		Texture2D texture2D;
		using (Stream stream = SdFile.OpenRead(fileName))
		{
			texture2D = TGALoader.LoadTGA(stream, mipMaps);
		}
		return texture2D;
	}

	public static Texture2D LoadTGA(Stream TGAStream, bool mipMaps = true)
	{
		Texture2D texture2D2;
		using (BinaryReader binaryReader = new BinaryReader(TGAStream))
		{
			binaryReader.BaseStream.Seek(0L, SeekOrigin.Begin);
			byte[] array = binaryReader.ReadBytes(12);
			if (array[0] != 0)
			{
				throw new Exception("TGA ID length !0");
			}
			if (array[1] != 0)
			{
				throw new Exception("TGA has color map");
			}
			byte b = array[2];
			if (b != 2 && b != 3)
			{
				throw new Exception("TGA unsupported image type " + b.ToString());
			}
			short num = binaryReader.ReadInt16();
			short num2 = binaryReader.ReadInt16();
			int num3 = (int)binaryReader.ReadByte();
			binaryReader.BaseStream.Seek(1L, SeekOrigin.Current);
			Texture2D texture2D = new Texture2D((int)num, (int)num2, 4, mipMaps);
			Color32[] array2 = new Color32[(int)(num * num2)];
			if (num3 == 8)
			{
				Color32 color;
				color..ctor(0, 0, 0, byte.MaxValue);
				for (int i = 0; i < (int)(num * num2); i++)
				{
					byte b2 = binaryReader.ReadByte();
					color.r = b2;
					color.g = b2;
					color.b = b2;
					array2[i] = color;
				}
			}
			else if (num3 == 32)
			{
				for (int j = 0; j < (int)(num * num2); j++)
				{
					byte b3 = binaryReader.ReadByte();
					byte b4 = binaryReader.ReadByte();
					byte b5 = binaryReader.ReadByte();
					byte b6 = binaryReader.ReadByte();
					array2[j] = new Color32(b5, b4, b3, b6);
				}
			}
			else
			{
				if (num3 != 24)
				{
					throw new Exception("TGA texture had non 8/32/24 bit depth.");
				}
				for (int k = 0; k < (int)(num * num2); k++)
				{
					byte b7 = binaryReader.ReadByte();
					byte b8 = binaryReader.ReadByte();
					byte b9 = binaryReader.ReadByte();
					array2[k] = new Color32(b9, b8, b7, byte.MaxValue);
				}
			}
			texture2D.SetPixels32(array2);
			texture2D.Apply();
			texture2D2 = texture2D;
		}
		return texture2D2;
	}

	public static Color32[] LoadTGAAsArray(string fileName, out int width, out int height, byte[] tempBuf = null)
	{
		Color32[] array2;
		using (Stream stream = SdFile.OpenRead(fileName))
		{
			using (BinaryReader binaryReader = new BinaryReader(stream))
			{
				binaryReader.BaseStream.Seek(12L, SeekOrigin.Begin);
				width = (int)binaryReader.ReadInt16();
				height = (int)binaryReader.ReadInt16();
				int num = (int)binaryReader.ReadByte();
				binaryReader.BaseStream.Seek(1L, SeekOrigin.Current);
				Color32[] array = new Color32[width * height];
				if (tempBuf == null)
				{
					tempBuf = new byte[1024];
				}
				int num2 = 0;
				if (num == 32)
				{
					for (;;)
					{
						int num3 = binaryReader.Read(tempBuf, 0, tempBuf.Length);
						if (num3 == 0)
						{
							break;
						}
						int i = 0;
						while (i < num3)
						{
							if (num2 >= array.Length)
							{
								break;
							}
							byte b = tempBuf[i++];
							byte b2 = tempBuf[i++];
							byte b3 = tempBuf[i++];
							byte b4 = tempBuf[i++];
							array[num2++] = new Color32(b3, b2, b, b4);
						}
					}
				}
				else
				{
					if (num != 24)
					{
						throw new Exception("TGA texture had non 32/24 bit depth.");
					}
					for (;;)
					{
						int num4 = binaryReader.Read(tempBuf, 0, tempBuf.Length);
						if (num4 == 0)
						{
							break;
						}
						int j = 0;
						while (j < num4)
						{
							if (num2 >= array.Length)
							{
								break;
							}
							byte b5 = tempBuf[j++];
							byte b6 = tempBuf[j++];
							byte b7 = tempBuf[j++];
							array[num2++] = new Color32(b7, b6, b5, byte.MaxValue);
						}
					}
				}
				array2 = array;
			}
		}
		return array2;
	}

	public static Color32[] LoadTGAAsArrayThreaded(string fileName, out int w, out int h)
	{
		Color32[] pulledColors2;
		using (Stream stream = SdFile.OpenRead(fileName))
		{
			using (BinaryReader binaryReader = new BinaryReader(stream))
			{
				binaryReader.BaseStream.Seek(12L, SeekOrigin.Begin);
				int width = (int)binaryReader.ReadInt16();
				int height = (int)binaryReader.ReadInt16();
				int bitDepth = (int)binaryReader.ReadByte();
				binaryReader.BaseStream.Seek(1L, SeekOrigin.Current);
				Color32[] pulledColors = new Color32[width * height];
				ThreadManager.TaskInfo taskInfo = ThreadManager.AddSingleTask(delegate(ThreadManager.TaskInfo _taskInfo)
				{
					using (Stream stream3 = SdFile.OpenRead(fileName))
					{
						using (BinaryReader binaryReader3 = new BinaryReader(stream3))
						{
							int width2 = width;
							int height2 = height;
							int num2 = 0 / 4;
							binaryReader3.BaseStream.Seek(18L, SeekOrigin.Begin);
							TGALoader.loadPart(bitDepth, binaryReader3, pulledColors, num2, width * height / 4);
						}
					}
				}, null, null, true);
				ThreadManager.TaskInfo taskInfo2 = ThreadManager.AddSingleTask(delegate(ThreadManager.TaskInfo _taskInfo)
				{
					using (Stream stream4 = SdFile.OpenRead(fileName))
					{
						using (BinaryReader binaryReader4 = new BinaryReader(stream4))
						{
							int num3 = width * height / 4;
							binaryReader4.BaseStream.Seek(18L, SeekOrigin.Begin);
							TGALoader.loadPart(bitDepth, binaryReader4, pulledColors, num3, width * height / 4);
						}
					}
				}, null, null, true);
				ThreadManager.TaskInfo taskInfo3 = ThreadManager.AddSingleTask(delegate(ThreadManager.TaskInfo _taskInfo)
				{
					using (Stream stream5 = SdFile.OpenRead(fileName))
					{
						using (BinaryReader binaryReader5 = new BinaryReader(stream5))
						{
							int num4 = 2 * (width * height) / 4;
							binaryReader5.BaseStream.Seek(18L, SeekOrigin.Begin);
							TGALoader.loadPart(bitDepth, binaryReader5, pulledColors, num4, width * height / 4);
						}
					}
				}, null, null, true);
				using (Stream stream2 = SdFile.OpenRead(fileName))
				{
					using (BinaryReader binaryReader2 = new BinaryReader(stream2))
					{
						int num = 3 * (width * height) / 4;
						binaryReader2.BaseStream.Seek(18L, SeekOrigin.Begin);
						TGALoader.loadPart(bitDepth, binaryReader2, pulledColors, num, width * height / 4);
					}
				}
				taskInfo.WaitForEnd();
				taskInfo2.WaitForEnd();
				taskInfo3.WaitForEnd();
				w = width;
				h = height;
				pulledColors2 = pulledColors;
			}
		}
		return pulledColors2;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void loadPart(int bitDepth, BinaryReader r, Color32[] pulledColors, int offs, int length)
	{
		byte[] array = new byte[1024];
		int num = 0;
		if (bitDepth == 32)
		{
			r.BaseStream.Seek((long)(offs * 4), SeekOrigin.Current);
			for (;;)
			{
				int num2 = r.Read(array, 0, array.Length);
				if (num2 == 0)
				{
					break;
				}
				int i = 0;
				while (i < num2)
				{
					if (num >= length)
					{
						break;
					}
					byte b = array[i++];
					byte b2 = array[i++];
					byte b3 = array[i++];
					byte b4 = array[i++];
					pulledColors[offs + num++] = new Color32(b3, b2, b, b4);
				}
			}
		}
		else
		{
			if (bitDepth != 24)
			{
				throw new Exception("TGA texture had non 32/24 bit depth.");
			}
			r.BaseStream.Seek((long)(offs * 3), SeekOrigin.Current);
			for (;;)
			{
				int num3 = r.Read(array, 0, array.Length);
				if (num3 == 0)
				{
					break;
				}
				int j = 0;
				while (j < num3)
				{
					if (num >= length)
					{
						break;
					}
					byte b5 = array[j++];
					byte b6 = array[j++];
					byte b7 = array[j++];
					pulledColors[offs + num++] = new Color32(b7, b6, b5, byte.MaxValue);
				}
			}
		}
	}
}
