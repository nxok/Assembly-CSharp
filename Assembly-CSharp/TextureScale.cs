using System;
using System.Threading;
using UnityEngine;

public class TextureScale
{
	public static void Point(Texture2D tex, int newWidth, int newHeight)
	{
		TextureScale.ThreadedScale(tex, newWidth, newHeight, false);
	}

	public static void Bilinear(Texture2D tex, int newWidth, int newHeight)
	{
		TextureScale.ThreadedScale(tex, newWidth, newHeight, true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ThreadedScale(Texture2D tex, int newWidth, int newHeight, bool useBilinear)
	{
		TextureScale.texColors = tex.GetPixels();
		TextureScale.newColors = new Color[newWidth * newHeight];
		if (useBilinear)
		{
			TextureScale.ratioX = 1f / ((float)newWidth / (float)(tex.width - 1));
			TextureScale.ratioY = 1f / ((float)newHeight / (float)(tex.height - 1));
		}
		else
		{
			TextureScale.ratioX = (float)tex.width / (float)newWidth;
			TextureScale.ratioY = (float)tex.height / (float)newHeight;
		}
		TextureScale.w = tex.width;
		TextureScale.w2 = newWidth;
		int num = Mathf.Min(SystemInfo.processorCount, newHeight);
		int num2 = newHeight / num;
		TextureScale.finishCount = 0;
		if (TextureScale.mutex == null)
		{
			TextureScale.mutex = new Mutex(false);
		}
		if (num > 1)
		{
			int i;
			TextureScale.ThreadData threadData;
			for (i = 0; i < num - 1; i++)
			{
				threadData = new TextureScale.ThreadData(num2 * i, num2 * (i + 1));
				new Thread(useBilinear ? new ParameterizedThreadStart(TextureScale.BilinearScale) : new ParameterizedThreadStart(TextureScale.PointScale))
				{
					Name = "TextureScale_" + i.ToString()
				}.Start(threadData);
			}
			threadData = new TextureScale.ThreadData(num2 * i, newHeight);
			if (useBilinear)
			{
				TextureScale.BilinearScale(threadData);
			}
			else
			{
				TextureScale.PointScale(threadData);
			}
			while (TextureScale.finishCount < num)
			{
				Thread.Sleep(1);
			}
		}
		else
		{
			TextureScale.ThreadData threadData2 = new TextureScale.ThreadData(0, newHeight);
			if (useBilinear)
			{
				TextureScale.BilinearScale(threadData2);
			}
			else
			{
				TextureScale.PointScale(threadData2);
			}
		}
		if (!tex.Reinitialize(newWidth, newHeight))
		{
			Log.Warning(string.Concat(new string[]
			{
				"Resized image format: ",
				tex.format.ToString(),
				" (",
				tex.name,
				")"
			}));
		}
		tex.SetPixels(TextureScale.newColors);
		tex.Apply();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void BilinearScale(object obj)
	{
		TextureScale.ThreadData threadData = (TextureScale.ThreadData)obj;
		for (int i = threadData.start; i < threadData.end; i++)
		{
			int num = (int)Mathf.Floor((float)i * TextureScale.ratioY);
			int num2 = num * TextureScale.w;
			int num3 = (num + 1) * TextureScale.w;
			int num4 = i * TextureScale.w2;
			for (int j = 0; j < TextureScale.w2; j++)
			{
				int num5 = (int)Mathf.Floor((float)j * TextureScale.ratioX);
				float num6 = (float)j * TextureScale.ratioX - (float)num5;
				TextureScale.newColors[num4 + j] = TextureScale.ColorLerpUnclamped(TextureScale.ColorLerpUnclamped(TextureScale.texColors[num2 + num5], TextureScale.texColors[num2 + num5 + 1], num6), TextureScale.ColorLerpUnclamped(TextureScale.texColors[num3 + num5], TextureScale.texColors[num3 + num5 + 1], num6), (float)i * TextureScale.ratioY - (float)num);
			}
		}
		TextureScale.mutex.WaitOne();
		TextureScale.finishCount++;
		TextureScale.mutex.ReleaseMutex();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void PointScale(object obj)
	{
		TextureScale.ThreadData threadData = (TextureScale.ThreadData)obj;
		for (int i = threadData.start; i < threadData.end; i++)
		{
			int num = (int)(TextureScale.ratioY * (float)i) * TextureScale.w;
			int num2 = i * TextureScale.w2;
			for (int j = 0; j < TextureScale.w2; j++)
			{
				TextureScale.newColors[num2 + j] = TextureScale.texColors[(int)((float)num + TextureScale.ratioX * (float)j)];
			}
		}
		TextureScale.mutex.WaitOne();
		TextureScale.finishCount++;
		TextureScale.mutex.ReleaseMutex();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static Color ColorLerpUnclamped(Color c1, Color c2, float value)
	{
		return new Color(c1.r + (c2.r - c1.r) * value, c1.g + (c2.g - c1.g) * value, c1.b + (c2.b - c1.b) * value, c1.a + (c2.a - c1.a) * value);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static Color[] texColors;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Color[] newColors;

	[PublicizedFrom(EAccessModifier.Private)]
	public static int w;

	[PublicizedFrom(EAccessModifier.Private)]
	public static float ratioX;

	[PublicizedFrom(EAccessModifier.Private)]
	public static float ratioY;

	[PublicizedFrom(EAccessModifier.Private)]
	public static int w2;

	[PublicizedFrom(EAccessModifier.Private)]
	public static int finishCount;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Mutex mutex;

	public class ThreadData
	{
		public ThreadData(int s, int e)
		{
			this.start = s;
			this.end = e;
		}

		public int start;

		public int end;
	}
}
