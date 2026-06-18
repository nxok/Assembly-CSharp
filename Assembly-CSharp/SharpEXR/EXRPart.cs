using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SharpEXR.AttributeTypes;
using SharpEXR.ColorSpace;

namespace SharpEXR
{
	public class EXRPart
	{
		public Dictionary<string, float[]> FloatChannels
		{
			get
			{
				return this.floatChannels;
			}
			[PublicizedFrom(EAccessModifier.Protected)]
			set
			{
				this.floatChannels = value;
			}
		}

		public Dictionary<string, Half[]> HalfChannels
		{
			get
			{
				return this.halfChannels;
			}
			[PublicizedFrom(EAccessModifier.Protected)]
			set
			{
				this.halfChannels = value;
			}
		}

		public EXRPart(EXRVersion version, EXRHeader header, OffsetTable offsets)
		{
			this.Version = version;
			this.Header = header;
			this.Offsets = offsets;
			if (this.Version.IsMultiPart)
			{
				this.Type = header.Type;
			}
			else
			{
				this.Type = (version.IsSinglePartTiled ? PartType.Tiled : PartType.ScanLine);
			}
			this.DataWindow = this.Header.DataWindow;
			this.FloatChannels = new Dictionary<string, float[]>();
			this.HalfChannels = new Dictionary<string, Half[]>();
			foreach (Channel channel in header.Channels)
			{
				if (channel.Type == PixelType.Float)
				{
					this.FloatChannels[channel.Name] = new float[this.DataWindow.Width * this.DataWindow.Height];
				}
				else
				{
					if (channel.Type != PixelType.Half)
					{
						throw new NotImplementedException("Only 16 and 32 bit floating point EXR images are supported.");
					}
					this.HalfChannels[channel.Name] = new Half[this.DataWindow.Width * this.DataWindow.Height];
				}
			}
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public virtual void CheckHasData()
		{
			if (!this.hasData)
			{
				throw new InvalidOperationException("Call EXRPart.Open before performing image operations.");
			}
		}

		public Half[] GetHalfs(ChannelConfiguration channels, bool premultiplied, GammaEncoding gamma)
		{
			return this.GetHalfs(channels, premultiplied, gamma, this.HasAlpha);
		}

		public Half[] GetHalfs(ChannelConfiguration channels, bool premultiplied, GammaEncoding gamma, bool includeAlpha)
		{
			ImageSourceFormat imageSourceFormat;
			if (this.HalfChannels.ContainsKey("R") && this.HalfChannels.ContainsKey("G") && this.HalfChannels.ContainsKey("B"))
			{
				imageSourceFormat = (includeAlpha ? ImageSourceFormat.HalfRGBA : ImageSourceFormat.HalfRGB);
			}
			else
			{
				if (!this.FloatChannels.ContainsKey("R") || !this.FloatChannels.ContainsKey("G") || !this.FloatChannels.ContainsKey("B"))
				{
					throw new EXRFormatException("Unrecognized EXR image format, did not contain half/single RGB color channels");
				}
				imageSourceFormat = (includeAlpha ? ImageSourceFormat.SingleRGBA : ImageSourceFormat.SingleRGB);
			}
			return this.GetHalfs(imageSourceFormat, channels, premultiplied, gamma);
		}

		public Half[] GetHalfs(ImageSourceFormat srcFormat, ChannelConfiguration channels, bool premultiplied, GammaEncoding gamma)
		{
			ImageDestFormat imageDestFormat;
			if (srcFormat == ImageSourceFormat.HalfRGBA || srcFormat == ImageSourceFormat.SingleRGBA)
			{
				if (premultiplied)
				{
					imageDestFormat = ((channels == ChannelConfiguration.BGR) ? ImageDestFormat.PremultipliedBGRA16 : ImageDestFormat.PremultipliedRGBA16);
				}
				else
				{
					imageDestFormat = ((channels == ChannelConfiguration.BGR) ? ImageDestFormat.BGRA16 : ImageDestFormat.RGBA16);
				}
			}
			else
			{
				imageDestFormat = ((channels == ChannelConfiguration.BGR) ? ImageDestFormat.BGR16 : ImageDestFormat.RGB16);
			}
			int bytesPerPixel = EXRFile.GetBytesPerPixel(imageDestFormat);
			if (srcFormat != ImageSourceFormat.SingleRGB)
			{
			}
			byte[] bytes = this.GetBytes(srcFormat, imageDestFormat, gamma, this.DataWindow.Width * bytesPerPixel);
			Half[] array = new Half[bytes.Length / 2];
			Buffer.BlockCopy(bytes, 0, array, 0, bytes.Length);
			return array;
		}

		public float[] GetFloats(ChannelConfiguration channels, bool premultiplied, GammaEncoding gamma)
		{
			return this.GetFloats(channels, premultiplied, gamma, this.HasAlpha);
		}

		public float[] GetFloats(ChannelConfiguration channels, bool premultiplied, GammaEncoding gamma, bool includeAlpha)
		{
			ImageSourceFormat imageSourceFormat;
			if (this.HalfChannels.ContainsKey("R") && this.HalfChannels.ContainsKey("G") && this.HalfChannels.ContainsKey("B"))
			{
				imageSourceFormat = (includeAlpha ? ImageSourceFormat.HalfRGBA : ImageSourceFormat.HalfRGB);
			}
			else
			{
				if (!this.FloatChannels.ContainsKey("R") || !this.FloatChannels.ContainsKey("G") || !this.FloatChannels.ContainsKey("B"))
				{
					throw new EXRFormatException("Unrecognized EXR image format, did not contain half/single RGB color channels");
				}
				imageSourceFormat = (includeAlpha ? ImageSourceFormat.SingleRGBA : ImageSourceFormat.SingleRGB);
			}
			return this.GetFloats(imageSourceFormat, channels, premultiplied, gamma);
		}

		public float[] GetFloats(ImageSourceFormat srcFormat, ChannelConfiguration channels, bool premultiplied, GammaEncoding gamma)
		{
			ImageDestFormat imageDestFormat;
			if (srcFormat == ImageSourceFormat.HalfRGBA || srcFormat == ImageSourceFormat.SingleRGBA)
			{
				if (premultiplied)
				{
					imageDestFormat = ((channels == ChannelConfiguration.BGR) ? ImageDestFormat.PremultipliedBGRA32 : ImageDestFormat.PremultipliedRGBA32);
				}
				else
				{
					imageDestFormat = ((channels == ChannelConfiguration.BGR) ? ImageDestFormat.BGRA32 : ImageDestFormat.RGBA32);
				}
			}
			else
			{
				imageDestFormat = ((channels == ChannelConfiguration.BGR) ? ImageDestFormat.BGR32 : ImageDestFormat.RGB32);
			}
			int bytesPerPixel = EXRFile.GetBytesPerPixel(imageDestFormat);
			if (srcFormat != ImageSourceFormat.SingleRGB)
			{
			}
			byte[] bytes = this.GetBytes(srcFormat, imageDestFormat, gamma, this.DataWindow.Width * bytesPerPixel);
			float[] array = new float[bytes.Length / 4];
			Buffer.BlockCopy(bytes, 0, array, 0, bytes.Length);
			return array;
		}

		public byte[] GetBytes(ImageDestFormat destFormat, GammaEncoding gamma)
		{
			return this.GetBytes(destFormat, gamma, this.DataWindow.Width * EXRFile.GetBytesPerPixel(destFormat));
		}

		public byte[] GetBytes(ImageDestFormat destFormat, GammaEncoding gamma, int stride)
		{
			ImageSourceFormat imageSourceFormat;
			if (this.HalfChannels.ContainsKey("R") && this.HalfChannels.ContainsKey("G") && this.HalfChannels.ContainsKey("B"))
			{
				imageSourceFormat = (this.HalfChannels.ContainsKey("A") ? ImageSourceFormat.HalfRGBA : ImageSourceFormat.HalfRGB);
			}
			else
			{
				if (!this.FloatChannels.ContainsKey("R") || !this.FloatChannels.ContainsKey("G") || !this.FloatChannels.ContainsKey("B"))
				{
					throw new EXRFormatException("Unrecognized EXR image format, did not contain half/single RGB color channels");
				}
				imageSourceFormat = (this.FloatChannels.ContainsKey("A") ? ImageSourceFormat.SingleRGBA : ImageSourceFormat.SingleRGB);
			}
			return this.GetBytes(imageSourceFormat, destFormat, gamma, stride);
		}

		public byte[] GetBytes(ImageSourceFormat srcFormat, ImageDestFormat destFormat, GammaEncoding gamma)
		{
			return this.GetBytes(srcFormat, destFormat, gamma, this.DataWindow.Width * EXRFile.GetBytesPerPixel(destFormat));
		}

		public byte[] GetBytes(ImageSourceFormat srcFormat, ImageDestFormat destFormat, GammaEncoding gamma, int stride)
		{
			this.CheckHasData();
			int bytesPerPixel = EXRFile.GetBytesPerPixel(destFormat);
			int bitsPerPixel = EXRFile.GetBitsPerPixel(destFormat);
			if (stride < bytesPerPixel * this.DataWindow.Width)
			{
				throw new ArgumentException("Stride was lower than minimum", "stride");
			}
			byte[] array = new byte[stride * this.DataWindow.Height];
			int num = stride - bytesPerPixel * this.DataWindow.Width;
			bool flag = srcFormat == ImageSourceFormat.HalfRGB || srcFormat == ImageSourceFormat.HalfRGBA;
			bool flag2 = false;
			bool flag3 = destFormat == ImageDestFormat.BGRA16 || destFormat == ImageDestFormat.BGRA32 || destFormat == ImageDestFormat.BGRA8 || destFormat == ImageDestFormat.PremultipliedBGRA16 || destFormat == ImageDestFormat.PremultipliedBGRA32 || destFormat == ImageDestFormat.PremultipliedBGRA8 || destFormat == ImageDestFormat.PremultipliedRGBA16 || destFormat == ImageDestFormat.PremultipliedRGBA32 || destFormat == ImageDestFormat.PremultipliedRGBA8 || destFormat == ImageDestFormat.RGBA16 || destFormat == ImageDestFormat.RGBA32 || destFormat == ImageDestFormat.RGBA8;
			bool flag4 = destFormat == ImageDestFormat.PremultipliedBGRA16 || destFormat == ImageDestFormat.PremultipliedBGRA32 || destFormat == ImageDestFormat.PremultipliedBGRA8 || destFormat == ImageDestFormat.PremultipliedRGBA16 || destFormat == ImageDestFormat.PremultipliedRGBA32 || destFormat == ImageDestFormat.PremultipliedRGBA8;
			bool flag5 = destFormat == ImageDestFormat.BGR16 || destFormat == ImageDestFormat.BGR32 || destFormat == ImageDestFormat.BGR8 || destFormat == ImageDestFormat.BGRA16 || destFormat == ImageDestFormat.BGRA32 || destFormat == ImageDestFormat.BGRA8 || destFormat == ImageDestFormat.PremultipliedBGRA16 || destFormat == ImageDestFormat.PremultipliedBGRA32 || destFormat == ImageDestFormat.PremultipliedBGRA8;
			Half[] array5;
			Half[] array4;
			Half[] array3;
			Half[] array2 = (array3 = (array4 = (array5 = null)));
			float[] array9;
			float[] array8;
			float[] array7;
			float[] array6 = (array7 = (array8 = (array9 = null)));
			if (flag)
			{
				if (!this.HalfChannels.ContainsKey("R"))
				{
					throw new ArgumentException("Half type channel R not found", "srcFormat");
				}
				if (!this.HalfChannels.ContainsKey("G"))
				{
					throw new ArgumentException("Half type channel G not found", "srcFormat");
				}
				if (!this.HalfChannels.ContainsKey("B"))
				{
					throw new ArgumentException("Half type channel B not found", "srcFormat");
				}
				array3 = this.HalfChannels["R"];
				array2 = this.HalfChannels["G"];
				array4 = this.HalfChannels["B"];
				if (srcFormat == ImageSourceFormat.HalfRGBA)
				{
					if (!this.HalfChannels.ContainsKey("A"))
					{
						throw new ArgumentException("Half type channel A not found", "srcFormat");
					}
					array5 = this.HalfChannels["A"];
					flag2 = true;
				}
			}
			else
			{
				if (!this.FloatChannels.ContainsKey("R"))
				{
					throw new ArgumentException("Single type channel R not found", "srcFormat");
				}
				if (!this.FloatChannels.ContainsKey("G"))
				{
					throw new ArgumentException("Single type channel G not found", "srcFormat");
				}
				if (!this.FloatChannels.ContainsKey("B"))
				{
					throw new ArgumentException("Single type channel B not found", "srcFormat");
				}
				array7 = this.FloatChannels["R"];
				array6 = this.FloatChannels["G"];
				array8 = this.FloatChannels["B"];
				if (srcFormat == ImageSourceFormat.HalfRGBA)
				{
					if (!this.FloatChannels.ContainsKey("A"))
					{
						throw new ArgumentException("Single type channel A not found", "srcFormat");
					}
					array9 = this.FloatChannels["A"];
					flag2 = true;
				}
			}
			int num2 = 0;
			int num3 = 0;
			BinaryWriter binaryWriter = new BinaryWriter(new MemoryStream(array));
			int i = 0;
			while (i < this.DataWindow.Height)
			{
				this.GetScanlineBytes(bytesPerPixel, num3, num2, flag, flag3, flag2, array3, array2, array4, array5, array7, array6, array8, array9, bitsPerPixel, gamma, flag4, flag5, array, binaryWriter);
				num3 += this.DataWindow.Width * bytesPerPixel;
				num2 += this.DataWindow.Width;
				i++;
				num3 += num;
			}
			binaryWriter.Dispose();
			binaryWriter.BaseStream.Dispose();
			return array;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void GetScanlineBytes(int bytesPerPixel, int destIndex, int srcIndex, bool isHalf, bool destinationAlpha, bool sourceAlpha, Half[] hr, Half[] hg, Half[] hb, Half[] ha, float[] fr, float[] fg, float[] fb, float[] fa, int bitsPerPixel, GammaEncoding gamma, bool premultiplied, bool bgra, byte[] buffer, BinaryWriter writer)
		{
			writer.Seek(destIndex, SeekOrigin.Begin);
			int i = 0;
			while (i < this.DataWindow.Width)
			{
				float num;
				float num2;
				float num3;
				float num4;
				if (isHalf)
				{
					num = hr[srcIndex];
					num2 = hg[srcIndex];
					num3 = hb[srcIndex];
					if (destinationAlpha)
					{
						num4 = (sourceAlpha ? ha[srcIndex] : 1f);
					}
					else
					{
						num4 = 1f;
					}
				}
				else
				{
					num = fr[srcIndex];
					num2 = fg[srcIndex];
					num3 = fb[srcIndex];
					if (destinationAlpha)
					{
						num4 = (sourceAlpha ? fa[srcIndex] : 1f);
					}
					else
					{
						num4 = 1f;
					}
				}
				if (bitsPerPixel == 8)
				{
					byte b = byte.MaxValue;
					byte b2;
					byte b3;
					byte b4;
					if (gamma == GammaEncoding.Linear)
					{
						if (premultiplied)
						{
							b2 = (byte)Math.Min(255.0, Math.Max(0.0, Math.Floor((double)(num * num4 * 255f) + 0.5)));
							b3 = (byte)Math.Min(255.0, Math.Max(0.0, Math.Floor((double)(num2 * num4 * 255f) + 0.5)));
							b4 = (byte)Math.Min(255.0, Math.Max(0.0, Math.Floor((double)(num3 * num4 * 255f) + 0.5)));
							b = (byte)Math.Min(255.0, Math.Max(0.0, Math.Floor((double)(num4 * 255f) + 0.5)));
						}
						else
						{
							b2 = (byte)Math.Min(255.0, Math.Max(0.0, Math.Floor((double)(num * 255f) + 0.5)));
							b3 = (byte)Math.Min(255.0, Math.Max(0.0, Math.Floor((double)(num2 * 255f) + 0.5)));
							b4 = (byte)Math.Min(255.0, Math.Max(0.0, Math.Floor((double)(num3 * 255f) + 0.5)));
							if (destinationAlpha)
							{
								b = (byte)Math.Min(255.0, Math.Max(0.0, Math.Floor((double)(num4 * 255f) + 0.5)));
							}
						}
					}
					else if (gamma == GammaEncoding.Gamma)
					{
						if (premultiplied)
						{
							b2 = (byte)Math.Min(255.0, Math.Max(0.0, Math.Floor((double)(Gamma.Compress(num) * num4 * 255f) + 0.5)));
							b3 = (byte)Math.Min(255.0, Math.Max(0.0, Math.Floor((double)(Gamma.Compress(num2) * num4 * 255f) + 0.5)));
							b4 = (byte)Math.Min(255.0, Math.Max(0.0, Math.Floor((double)(Gamma.Compress(num3) * num4 * 255f) + 0.5)));
							b = (byte)Math.Min(255.0, Math.Max(0.0, Math.Floor((double)(num4 * 255f) + 0.5)));
						}
						else
						{
							b2 = (byte)Math.Min(255.0, Math.Max(0.0, Math.Floor((double)(Gamma.Compress(num) * 255f) + 0.5)));
							b3 = (byte)Math.Min(255.0, Math.Max(0.0, Math.Floor((double)(Gamma.Compress(num2) * 255f) + 0.5)));
							b4 = (byte)Math.Min(255.0, Math.Max(0.0, Math.Floor((double)(Gamma.Compress(num3) * 255f) + 0.5)));
							if (destinationAlpha)
							{
								b = (byte)Math.Min(255.0, Math.Max(0.0, Math.Floor((double)(num4 * 255f) + 0.5)));
							}
						}
					}
					else if (premultiplied)
					{
						b2 = (byte)Math.Min(255.0, Math.Max(0.0, Math.Floor((double)(Gamma.Compress_sRGB(num) * num4 * 255f) + 0.5)));
						b3 = (byte)Math.Min(255.0, Math.Max(0.0, Math.Floor((double)(Gamma.Compress_sRGB(num2) * num4 * 255f) + 0.5)));
						b4 = (byte)Math.Min(255.0, Math.Max(0.0, Math.Floor((double)(Gamma.Compress_sRGB(num3) * num4 * 255f) + 0.5)));
						b = (byte)Math.Min(255.0, Math.Max(0.0, Math.Floor((double)(num4 * 255f) + 0.5)));
					}
					else
					{
						b2 = (byte)Math.Min(255.0, Math.Max(0.0, Math.Floor((double)(Gamma.Compress_sRGB(num) * 255f) + 0.5)));
						b3 = (byte)Math.Min(255.0, Math.Max(0.0, Math.Floor((double)(Gamma.Compress_sRGB(num2) * 255f) + 0.5)));
						b4 = (byte)Math.Min(255.0, Math.Max(0.0, Math.Floor((double)(Gamma.Compress_sRGB(num3) * 255f) + 0.5)));
						if (destinationAlpha)
						{
							b = (byte)Math.Min(255.0, Math.Max(0.0, Math.Floor((double)(num4 * 255f) + 0.5)));
						}
					}
					if (bgra)
					{
						buffer[destIndex] = b4;
						buffer[destIndex + 1] = b3;
						buffer[destIndex + 2] = b2;
					}
					else
					{
						buffer[destIndex] = b2;
						buffer[destIndex + 1] = b3;
						buffer[destIndex + 2] = b4;
					}
					if (destinationAlpha)
					{
						buffer[destIndex + 3] = b;
					}
				}
				else if (bitsPerPixel == 32)
				{
					float num5 = 1f;
					float num6;
					float num7;
					float num8;
					if (gamma == GammaEncoding.Linear)
					{
						if (premultiplied)
						{
							num6 = num * num4;
							num7 = num2 * num4;
							num8 = num3 * num4;
							num5 = num4;
						}
						else
						{
							num6 = num;
							num7 = num2;
							num8 = num3;
							if (destinationAlpha)
							{
								num5 = num4;
							}
						}
					}
					else if (gamma == GammaEncoding.Gamma)
					{
						if (premultiplied)
						{
							num6 = Gamma.Compress(num) * num4;
							num7 = Gamma.Compress(num2) * num4;
							num8 = Gamma.Compress(num3) * num4;
							num5 = num4;
						}
						else
						{
							num6 = Gamma.Compress(num);
							num7 = Gamma.Compress(num2);
							num8 = Gamma.Compress(num3);
							if (destinationAlpha)
							{
								num5 = num4;
							}
						}
					}
					else if (premultiplied)
					{
						num6 = Gamma.Compress_sRGB(num) * num4;
						num7 = Gamma.Compress_sRGB(num2) * num4;
						num8 = Gamma.Compress_sRGB(num3) * num4;
						num5 = num4;
					}
					else
					{
						num6 = Gamma.Compress_sRGB(num);
						num7 = Gamma.Compress_sRGB(num2);
						num8 = Gamma.Compress_sRGB(num3);
						if (destinationAlpha)
						{
							num5 = num4;
						}
					}
					if (bgra)
					{
						writer.Write(num8);
						writer.Write(num7);
						writer.Write(num6);
					}
					else
					{
						writer.Write(num6);
						writer.Write(num7);
						writer.Write(num8);
					}
					if (destinationAlpha)
					{
						writer.Write(num5);
					}
				}
				else
				{
					Half half = new Half(1f);
					Half half2;
					Half half3;
					Half half4;
					if (gamma == GammaEncoding.Linear)
					{
						if (premultiplied)
						{
							half2 = (Half)(num * num4);
							half3 = (Half)(num2 * num4);
							half4 = (Half)(num3 * num4);
							half = (Half)num4;
						}
						else
						{
							half2 = (Half)num;
							half3 = (Half)num2;
							half4 = (Half)num3;
							if (destinationAlpha)
							{
								half = (Half)num4;
							}
						}
					}
					else if (gamma == GammaEncoding.Gamma)
					{
						if (premultiplied)
						{
							half2 = (Half)(Gamma.Compress(num) * num4);
							half3 = (Half)(Gamma.Compress(num2) * num4);
							half4 = (Half)(Gamma.Compress(num3) * num4);
							half = (Half)num4;
						}
						else
						{
							half2 = (Half)Gamma.Compress(num);
							half3 = (Half)Gamma.Compress(num2);
							half4 = (Half)Gamma.Compress(num3);
							if (destinationAlpha)
							{
								half = (Half)num4;
							}
						}
					}
					else if (premultiplied)
					{
						half2 = (Half)(Gamma.Compress_sRGB(num) * num4);
						half3 = (Half)(Gamma.Compress_sRGB(num2) * num4);
						half4 = (Half)(Gamma.Compress_sRGB(num3) * num4);
						half = (Half)num4;
					}
					else
					{
						half2 = (Half)Gamma.Compress_sRGB(num);
						half3 = (Half)Gamma.Compress_sRGB(num2);
						half4 = (Half)Gamma.Compress_sRGB(num3);
						if (destinationAlpha)
						{
							half = (Half)num4;
						}
					}
					if (bgra)
					{
						writer.Write(half4.value);
						writer.Write(half3.value);
						writer.Write(half2.value);
					}
					else
					{
						writer.Write(half2.value);
						writer.Write(half3.value);
						writer.Write(half4.value);
					}
					if (destinationAlpha)
					{
						writer.Write(half.value);
					}
				}
				i++;
				destIndex += bytesPerPixel;
				srcIndex++;
			}
		}

		public void Open(string file)
		{
			EXRReader exrreader = new EXRReader(new FileStream(file, FileMode.Open, FileAccess.Read), false);
			this.Open(exrreader);
			exrreader.Dispose();
		}

		public void Open(Stream stream)
		{
			EXRReader exrreader = new EXRReader(new BinaryReader(stream));
			this.Open(exrreader);
			exrreader.Dispose();
		}

		public void Close()
		{
			this.hasData = false;
			this.HalfChannels.Clear();
			this.FloatChannels.Clear();
		}

		public void Open(IEXRReader reader)
		{
			this.hasData = true;
			this.ReadPixelData(reader);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void ReadPixelBlock(IEXRReader reader, uint offset, int linesPerBlock, List<Channel> sortedChannels)
		{
			reader.Position = (int)offset;
			if (this.Version.IsMultiPart)
			{
				reader.ReadUInt32();
				reader.ReadUInt32();
			}
			int num = reader.ReadInt32();
			int num2 = Math.Min(this.DataWindow.Height, num + linesPerBlock);
			int num3 = num * this.DataWindow.Width;
			reader.ReadInt32();
			if (this.Header.Compression != EXRCompression.None)
			{
				throw new NotImplementedException("Compressed images are currently not supported");
			}
			foreach (Channel channel in sortedChannels)
			{
				float[] array = null;
				Half[] array2 = null;
				if (channel.Type == PixelType.Float)
				{
					array = this.FloatChannels[channel.Name];
				}
				else
				{
					if (channel.Type != PixelType.Half)
					{
						throw new NotImplementedException();
					}
					array2 = this.HalfChannels[channel.Name];
				}
				int num4 = num3;
				for (int i = num; i < num2; i++)
				{
					int j = 0;
					while (j < this.DataWindow.Width)
					{
						if (channel.Type == PixelType.Float)
						{
							array[num4] = reader.ReadSingle();
						}
						else
						{
							if (channel.Type != PixelType.Half)
							{
								throw new NotImplementedException();
							}
							array2[num4] = reader.ReadHalf();
						}
						j++;
						num4++;
					}
				}
			}
		}

		public void OpenParallel(string file)
		{
			this.Open(file);
		}

		public void OpenParallel(ParallelReaderCreationDelegate createReader)
		{
			IEXRReader iexrreader = createReader();
			this.Open(iexrreader);
			iexrreader.Dispose();
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public void ReadPixelData(IEXRReader reader)
		{
			int scanLinesPerBlock = EXRFile.GetScanLinesPerBlock(this.Header.Compression);
			List<Channel> list = this.Header.Channels.OrderBy((Channel c) => c.Name).ToList<Channel>();
			foreach (uint num in this.Offsets)
			{
				this.ReadPixelBlock(reader, num, scanLinesPerBlock, list);
			}
		}

		public bool IsRGB
		{
			get
			{
				return (this.HalfChannels.ContainsKey("R") || this.FloatChannels.ContainsKey("R")) && (this.HalfChannels.ContainsKey("G") || this.FloatChannels.ContainsKey("G")) && (this.HalfChannels.ContainsKey("B") || this.FloatChannels.ContainsKey("B"));
			}
		}

		public bool HasAlpha
		{
			get
			{
				return this.HalfChannels.ContainsKey("A") || this.FloatChannels.ContainsKey("A");
			}
		}

		public readonly EXRVersion Version;

		public readonly EXRHeader Header;

		public readonly OffsetTable Offsets;

		public readonly PartType Type;

		public readonly Box2I DataWindow;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool hasData;

		[PublicizedFrom(EAccessModifier.Private)]
		public Dictionary<string, float[]> floatChannels;

		[PublicizedFrom(EAccessModifier.Private)]
		public Dictionary<string, Half[]> halfChannels;
	}
}
