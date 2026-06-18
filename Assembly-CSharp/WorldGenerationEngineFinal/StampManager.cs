using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using UnityEngine;

namespace WorldGenerationEngineFinal
{
	[BurstCompile(CompileSynchronously = true)]
	public class StampManager
	{
		public StampManager(WorldBuilder _worldBuilder)
		{
			this.worldBuilder = _worldBuilder;
		}

		public void ClearStamps()
		{
			foreach (RawStamp rawStamp in this.AllStamps.Values)
			{
				rawStamp.Clear();
			}
			this.AllStamps.Clear();
		}

		public void DrawStampGroup(StampGroup _group, ref NativeArray<float> _dest, int size)
		{
			MicroStopwatch microStopwatch = new MicroStopwatch(true);
			for (int i = 0; i < _group.Stamps.Count; i++)
			{
				Stamp stamp = _group.Stamps[i];
				if (stamp != null)
				{
					this.DrawStamp(ref _dest, stamp);
					if (this.worldBuilder.IsCanceled)
					{
						break;
					}
				}
			}
			Log.Out("DrawStampGroup '{0}', count {1}, in {2}", new object[]
			{
				_group.Name,
				_group.Stamps.Count,
				(float)microStopwatch.ElapsedMilliseconds * 0.001f
			});
		}

		public void DrawStampGroup(StampGroup _group, Color32[] _image, int size, float _stampScale = 1f)
		{
			MicroStopwatch microStopwatch = new MicroStopwatch(true);
			for (int i = 0; i < _group.Stamps.Count; i++)
			{
				Stamp stamp = _group.Stamps[i];
				if (stamp != null)
				{
					int num = (int)((float)stamp.transform.x * _stampScale);
					int num2 = (int)((float)stamp.transform.y * _stampScale);
					StampManager.DrawStamp(_image, ref stamp.stamp.data, num, num2, size, size, stamp.imageWidth, stamp.imageHeight, stamp.alpha, stamp.scale * _stampScale, stamp.customColor, stamp.alphaCutoff, (float)stamp.transform.rotation, stamp.isWater);
					if (this.worldBuilder.IsCanceled)
					{
						break;
					}
				}
			}
			Log.Out("DrawStampGroup c32 '{0}', count {1}, in {2}", new object[]
			{
				_group.Name,
				_group.Stamps.Count,
				(float)microStopwatch.ElapsedMilliseconds * 0.001f
			});
		}

		public void DrawWaterStampGroup(StampGroup _group, ref NativeArray<float> _dest, int _destSize)
		{
			MicroStopwatch microStopwatch = new MicroStopwatch(true);
			for (int i = 0; i < _group.Stamps.Count; i++)
			{
				Stamp stamp = _group.Stamps[i];
				if (stamp != null)
				{
					StampManager.DrawWaterStamp(stamp, ref _dest, _destSize);
					if (this.worldBuilder.IsCanceled)
					{
						break;
					}
				}
			}
			Log.Out("DrawWaterStampGroup '{0}', count {1}, in {2}", new object[]
			{
				_group.Name,
				_group.Stamps.Count,
				(float)microStopwatch.ElapsedMilliseconds * 0.001f
			});
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static float CalcRotatedValue(float x1, float y1, ref NativeArray<float> src, float sine, float cosine, int width, int height, bool isWater = false)
		{
			int num = width >> 1;
			int num2 = height >> 1;
			float num3 = cosine * (x1 - (float)num) + sine * (y1 - (float)num2) + (float)num;
			int num4 = (int)num3;
			if ((ulong)num4 >= (ulong)((long)width))
			{
				return 0f;
			}
			float num5 = -sine * (x1 - (float)num) + cosine * (y1 - (float)num2) + (float)num2;
			int num6 = (int)num5;
			if ((ulong)num6 >= (ulong)((long)height))
			{
				return 0f;
			}
			int num7 = num4 + num6 * width;
			float num8 = src[num7];
			float num9;
			if (num4 + 1 < width)
			{
				num9 = src[num7 + 1];
			}
			else
			{
				num9 = num8;
			}
			float num10;
			if (num6 + 1 < height)
			{
				num10 = src[num7 + width];
			}
			else
			{
				num10 = num8;
			}
			float num11;
			if (num4 + 1 < width && num6 + 1 < height)
			{
				num11 = src[num7 + width + 1];
			}
			else
			{
				num11 = num8;
			}
			if (isWater && (num8 > 0f || num10 > 0f || num9 > 0f || num11 > 0f))
			{
				return num8;
			}
			num3 -= (float)num4;
			num5 -= (float)num6;
			num8 += (num9 - num8) * num3;
			float num12 = num10 + (num11 - num10) * num3;
			return num8 + (num12 - num8) * num5;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static float CalcRotatedValue(ref StampManager.RotateParams _p, float x1, float y1, ref NativeArray<float> src)
		{
			float num = _p.cosine * (x1 - _p.halfWidth) + _p.sine * (y1 - _p.halfHeight) + _p.halfWidth;
			int num2 = (int)num;
			if ((ulong)num2 >= (ulong)((long)_p.width))
			{
				return 0f;
			}
			float num3 = -_p.sine * (x1 - _p.halfWidth) + _p.cosine * (y1 - _p.halfHeight) + _p.halfHeight;
			int num4 = (int)num3;
			if ((ulong)num4 >= (ulong)((long)_p.height))
			{
				return 0f;
			}
			int num5 = num2 + num4 * _p.width;
			float num6 = src[num5];
			float num7 = num6;
			if (num2 + 1 < _p.width)
			{
				num7 = src[num5 + 1];
			}
			float num8 = num6;
			if (num4 + 1 < _p.height)
			{
				num8 = src[num5 + _p.width];
			}
			float num9 = num6;
			if (num2 + 1 < _p.width && num4 + 1 < _p.height)
			{
				num9 = src[num5 + _p.width + 1];
			}
			if (_p.isWater && (num6 > 0f || num8 > 0f || num7 > 0f || num9 > 0f))
			{
				return num6;
			}
			num -= (float)num2;
			num3 -= (float)num4;
			float num10 = num6 + (num7 - num6) * num;
			float num11 = num8 + (num9 - num8) * num;
			return num10 + (num11 - num10) * num3;
		}

		public void DrawStamp(ref NativeArray<float> _dest, Stamp stamp)
		{
			int x = stamp.transform.x;
			int y = stamp.transform.y;
			int worldSize = this.worldBuilder.WorldSize;
			float num = (float)stamp.transform.rotation;
			StampManager.DrawStamp(ref _dest, ref stamp.stamp.data, x, y, worldSize, worldSize, stamp.alpha, stamp.additive, stamp.scale, ref stamp.customColor, stamp.alphaCutoff, num);
		}

		[BurstCompile(CompileSynchronously = true)]
		[PublicizedFrom(EAccessModifier.Private)]
		public static void DrawStamp(ref NativeArray<float> _dest, ref RawStamp.Data _src, int _x, int _y, int _destWidth, int _destHeight, float _alphaScale, bool _additive, float _scale, ref Color32 _customColor, float _alphaCutoff, float _angle)
		{
			StampManager.DrawStamp_0000B65F$BurstDirectCall.Invoke(ref _dest, ref _src, _x, _y, _destWidth, _destHeight, _alphaScale, _additive, _scale, ref _customColor, _alphaCutoff, _angle);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static void DrawStamp(Color32[] _dest, ref RawStamp.Data _src, int _x, int _y, int _destWidth, int _destHeight, int _srcWidth, int _srcHeight, float _alpha, float _scale, Color32 _customColor, float _alphaCutoff = 0.1f, float _angle = 0f, bool isWater = false)
		{
			_x -= (int)((float)_srcWidth * _scale) / 2;
			_y -= (int)((float)_srcHeight * _scale) / 2;
			double num = (double)(0.017453292f * _angle);
			float num2 = (float)Math.Sin(num);
			float num3 = (float)Math.Cos(num);
			int num4 = Mathf.FloorToInt((float)(((int)Mathf.Sqrt((float)(_srcWidth * _srcWidth + _srcHeight * _srcHeight)) - _srcWidth) / 2) * _scale);
			int num5 = Mathf.FloorToInt((float)_srcWidth * _scale + (float)num4);
			num4 = -num4;
			int num6 = num4;
			int num7 = _x + num4;
			if (num7 < 0)
			{
				num6 -= num7;
			}
			int num8 = num5;
			num7 = _x + num5;
			if (num7 >= _destWidth)
			{
				num8 -= num7 - _destWidth;
			}
			int num9 = num4;
			int num10 = _y + num4;
			if (num10 < 0)
			{
				num9 -= num10;
			}
			int num11 = num5;
			num10 = _y + num5;
			if (num10 >= _destHeight)
			{
				num11 -= num10 - _destHeight;
			}
			for (int i = num9; i < num11; i++)
			{
				int num12 = (_y + i) * _destWidth;
				float num13 = (float)i / _scale;
				for (int j = num6; j < num8; j++)
				{
					float num14 = _src.alphaConst;
					if (_src.alphaPixels.IsCreated)
					{
						num14 = StampManager.CalcRotatedValue((float)j / _scale, num13, ref _src.alphaPixels, num2, num3, _srcWidth, _srcHeight, isWater);
					}
					if (num14 > _alphaCutoff)
					{
						int num15 = _x + j + num12;
						_dest[num15] = _customColor;
					}
				}
			}
		}

		public static void DrawWaterStamp(Stamp stamp, ref NativeArray<float> _dest, int _destSize)
		{
			if (!stamp.isWater)
			{
				throw new ArgumentException("DrawWaterStamp called with non-water stamp " + stamp.Name);
			}
			NativeArray<float> nativeArray = stamp.stamp.data.waterPixels;
			if (!nativeArray.IsCreated)
			{
				nativeArray = stamp.stamp.data.alphaPixels;
				if (!nativeArray.IsCreated)
				{
					return;
				}
			}
			int x = stamp.transform.x;
			int y = stamp.transform.y;
			float num = (float)stamp.transform.rotation;
			StampManager.DrawWaterStamp(ref _dest, ref nativeArray, x, y, _destSize, _destSize, stamp.imageWidth, stamp.imageHeight, stamp.alpha, stamp.scale, (float)stamp.customColor.b, num);
		}

		[BurstCompile(CompileSynchronously = true)]
		[PublicizedFrom(EAccessModifier.Private)]
		public static void DrawWaterStamp(ref NativeArray<float> _dest, ref NativeArray<float> _src, int _x, int _y, int _destWidth, int _destHeight, int _srcWidth, int _srcHeight, float _alpha, float _scale, float _customColor, float _angle)
		{
			StampManager.DrawWaterStamp_0000B662$BurstDirectCall.Invoke(ref _dest, ref _src, _x, _y, _destWidth, _destHeight, _srcWidth, _srcHeight, _alpha, _scale, _customColor, _angle);
		}

		public static void DrawBiomeStamp(Color32[] _dest, ref NativeArray<float> _src, int _x, int _y, int _destWidth, int _destHeight, int _srcWidth, int _srcHeight, float _scale, Color32 _destColor, float _alphaCutoff = 0.1f, float _angle = 0f)
		{
			_x -= (int)((float)_srcWidth * _scale) / 2;
			_y -= (int)((float)_srcHeight * _scale) / 2;
			double num = (double)(0.017453292f * _angle);
			float num2 = (float)Math.Sin(num);
			float num3 = (float)Math.Cos(num);
			int num4 = Mathf.FloorToInt((float)(((int)Mathf.Sqrt((float)(_srcWidth * _srcWidth + _srcHeight * _srcHeight)) - _srcWidth) / 2) * _scale);
			int num5 = Mathf.FloorToInt((float)_srcWidth * _scale + (float)num4);
			num4 = -num4;
			int num6 = num4;
			int num7 = _x + num4;
			if (num7 < 0)
			{
				num6 -= num7;
			}
			int num8 = num5;
			num7 = _x + num5;
			if (num7 >= _destWidth)
			{
				num8 -= num7 - _destWidth;
			}
			int num9 = num4;
			int num10 = _y + num4;
			if (num10 < 0)
			{
				num9 -= num10;
			}
			int num11 = num5;
			num10 = _y + num5;
			if (num10 >= _destHeight)
			{
				num11 -= num10 - _destHeight;
			}
			for (int i = num9; i < num11; i++)
			{
				int num12 = (_y + i) * _destWidth;
				float num13 = (float)i / _scale;
				for (int j = num6; j < num8; j++)
				{
					int num14 = _x + j + num12;
					if (StampManager.CalcRotatedValue((float)j / _scale, num13, ref _src, num2, num3, _srcWidth, _srcHeight, false) > _alphaCutoff)
					{
						_dest[num14] = _destColor;
					}
				}
			}
		}

		public bool TryGetStamp(string terrainTypeName, string comboTypeName, out RawStamp tmp)
		{
			return this.TryGetStamp(comboTypeName, out tmp) || this.TryGetStamp(terrainTypeName, out tmp);
		}

		public bool TryGetStamp(string _baseName, out RawStamp _output)
		{
			return this.TryGetStamp(_baseName, out _output, Rand.Instance);
		}

		public bool TryGetStamp(string _baseName, out RawStamp _output, Rand _rand)
		{
			List<RawStamp> list = this.tempGetStampList;
			bool flag2;
			lock (list)
			{
				foreach (KeyValuePair<string, RawStamp> keyValuePair in this.AllStamps)
				{
					if (keyValuePair.Key.StartsWith(_baseName))
					{
						this.tempGetStampList.Add(keyValuePair.Value);
					}
				}
				if (this.tempGetStampList.Count == 0)
				{
					_output = null;
					flag2 = false;
				}
				else
				{
					if (this.tempGetStampList.Count == 1)
					{
						_output = this.tempGetStampList[0];
					}
					else
					{
						_output = this.tempGetStampList[_rand.Range(this.tempGetStampList.Count)];
					}
					this.tempGetStampList.Clear();
					flag2 = true;
				}
			}
			return flag2;
		}

		public RawStamp GetStamp(string _baseName, Rand _rand = null)
		{
			if (_rand == null)
			{
				_rand = Rand.Instance;
			}
			RawStamp rawStamp;
			if (!this.TryGetStamp(_baseName, out rawStamp, _rand))
			{
				return null;
			}
			return rawStamp;
		}

		public IEnumerator LoadStamps()
		{
			if (this.AllStamps.Count > 0)
			{
				yield break;
			}
			MicroStopwatch ms = new MicroStopwatch(true);
			this.AllStamps.Clear();
			List<PathAbstractions.AbstractedLocation> stampMaps = PathAbstractions.RwgStampsSearchPaths.GetAvailablePathsList(null, true, null);
			MicroStopwatch msRestart = new MicroStopwatch(true);
			int num;
			for (int i = 0; i < stampMaps.Count; i = num + 1)
			{
				string extension = stampMaps[i].Extension;
				if (!(extension != ".exr") || !(extension != ".raw"))
				{
					this.LoadStamp(stampMaps[i]);
					if (msRestart.ElapsedMilliseconds > 500L)
					{
						yield return null;
						msRestart.ResetAndRestart();
					}
				}
				num = i;
			}
			for (int i = 0; i < stampMaps.Count; i = num + 1)
			{
				this.LoadStamp(stampMaps[i]);
				if (msRestart.ElapsedMilliseconds > 500L)
				{
					yield return null;
					msRestart.ResetAndRestart();
				}
				num = i;
			}
			Log.Out("LoadStamps in {0}", new object[] { (float)ms.ElapsedMilliseconds * 0.001f });
			yield break;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void LoadStamp(PathAbstractions.AbstractedLocation _path)
		{
			string fileNameNoExtension = _path.FileNameNoExtension;
			if (this.AllStamps.ContainsKey(fileNameNoExtension))
			{
				return;
			}
			MicroStopwatch microStopwatch = new MicroStopwatch(true);
			Color[] array;
			if (_path.Extension == ".raw")
			{
				array = Utils.LoadRawStampFileArray(_path.FullPath);
			}
			else
			{
				if (_path.Extension == ".exr")
				{
					return;
				}
				Texture2D texture2D = TextureUtils.LoadTexture(_path.FullPath, 0, false, false, null);
				array = texture2D.GetPixels();
				if (Application.isPlaying)
				{
					Object.Destroy(texture2D);
				}
			}
			if (array == null)
			{
				Log.Error("LoadStamp {0} failed", new object[] { _path.FullPath });
				return;
			}
			NativeArray<float> nativeArray = default(NativeArray<float>);
			float num = array[0].r;
			for (int i = 1; i < array.Length; i++)
			{
				if (num != array[i].r)
				{
					nativeArray = new NativeArray<float>(array.Length, 4, 0);
					for (int j = 0; j < array.Length; j++)
					{
						nativeArray[j] = array[j].r * 255f;
					}
					break;
				}
			}
			num *= 255f;
			NativeArray<float> nativeArray2 = default(NativeArray<float>);
			float a = array[0].a;
			for (int k = 1; k < array.Length; k++)
			{
				if (a != array[k].a)
				{
					nativeArray2 = new NativeArray<float>(array.Length, 4, 0);
					for (int l = 0; l < array.Length; l++)
					{
						nativeArray2[l] = array[l].a;
					}
					break;
				}
			}
			NativeArray<float> nativeArray3 = default(NativeArray<float>);
			if (!fileNameNoExtension.Contains("rwg_tile"))
			{
				for (int m = 0; m < array.Length; m++)
				{
					if (array[m].b != 0f)
					{
						nativeArray3 = new NativeArray<float>(array.Length, 4, 0);
						for (int n = 0; n < array.Length; n++)
						{
							nativeArray3[n] = array[n].b * 255f;
						}
						break;
					}
				}
			}
			int num2 = (int)Mathf.Sqrt((float)array.Length);
			RawStamp rawStamp = new RawStamp
			{
				name = fileNameNoExtension
			};
			rawStamp.data.heightConst = num;
			rawStamp.data.heightPixels = nativeArray;
			rawStamp.data.alphaConst = a;
			rawStamp.data.alphaPixels = nativeArray2;
			rawStamp.data.waterPixels = nativeArray3;
			rawStamp.data.height = num2;
			rawStamp.data.width = num2;
			this.AllStamps[fileNameNoExtension] = rawStamp;
			if (nativeArray2.IsCreated)
			{
				if (fileNameNoExtension.StartsWith("mountains_"))
				{
					RawStamp.SmoothAlpha(ref rawStamp.data, 5);
				}
				if (fileNameNoExtension.StartsWith("desert_mountains_"))
				{
					RawStamp.SmoothAlpha(ref rawStamp.data, 3);
				}
			}
			Log.Out("LoadStamp {0}, size {1}, height {2} ({3}), alpha {4} ({5}), water {6}, in {7}", new object[]
			{
				_path.FullPath,
				num2,
				nativeArray.IsCreated,
				num,
				nativeArray2.IsCreated,
				a,
				nativeArray3.IsCreated,
				(float)microStopwatch.ElapsedMilliseconds * 0.001f
			});
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static void SaveEXR(string cleanname, Color[] colors, bool _replace = true)
		{
			string text = GameIO.GetGameDir("Data/Stamps") + "/" + cleanname + ".exr";
			if (!_replace && File.Exists(text))
			{
				return;
			}
			int num = (int)Mathf.Sqrt((float)colors.Length);
			Texture2D texture2D = new Texture2D(num, num, 20, false);
			texture2D.SetPixels(colors);
			File.WriteAllBytes(text, ImageConversion.EncodeToEXR(texture2D, 0));
			Log.Out("SaveEXR {0}", new object[] { text });
		}

		[BurstCompile(CompileSynchronously = true)]
		[PublicizedFrom(EAccessModifier.Internal)]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void DrawStamp$BurstManaged(ref NativeArray<float> _dest, ref RawStamp.Data _src, int _x, int _y, int _destWidth, int _destHeight, float _alphaScale, bool _additive, float _scale, ref Color32 _customColor, float _alphaCutoff, float _angle)
		{
			_x -= (int)((float)_src.width * _scale) / 2;
			_y -= (int)((float)_src.height * _scale) / 2;
			double num = (double)(0.017453292f * _angle);
			float num2 = (float)Math.Sin(num);
			float num3 = (float)Math.Cos(num);
			int num4 = (int)Math.Floor((double)((float)(((int)Math.Sqrt((double)(_src.width * _src.width + _src.height * _src.height)) - _src.width) / 2) * _scale));
			int num5 = (int)Math.Floor((double)((float)_src.width * _scale + (float)num4));
			num4 = -num4;
			int num6 = num4;
			int num7 = _x + num4;
			if (num7 < 0)
			{
				num6 -= num7;
			}
			int num8 = num5;
			num7 = _x + num5;
			if (num7 >= _destWidth)
			{
				num8 -= num7 - _destWidth;
			}
			int num9 = num4;
			int num10 = _y + num4;
			if (num10 < 0)
			{
				num9 -= num10;
			}
			int num11 = num5;
			num10 = _y + num5;
			if (num10 >= _destHeight)
			{
				num11 -= num10 - _destHeight;
			}
			StampManager.RotateParams rotateParams;
			rotateParams.sine = num2;
			rotateParams.cosine = num3;
			rotateParams.width = _src.width;
			rotateParams.height = _src.height;
			rotateParams.halfWidth = (float)(_src.width / 2);
			rotateParams.halfHeight = (float)(_src.height / 2);
			rotateParams.isWater = false;
			for (int i = num9; i < num11; i++)
			{
				int num12 = (_y + i) * _destWidth;
				float num13 = (float)i / _scale;
				for (int j = num6; j < num8; j++)
				{
					float num14 = (float)j / _scale;
					float num15 = _src.alphaConst;
					if (_src.alphaPixels.IsCreated)
					{
						num15 = StampManager.CalcRotatedValue(ref rotateParams, num14, num13, ref _src.alphaPixels);
					}
					if (num15 >= 1E-05f)
					{
						int num16 = _x + j + num12;
						if (_customColor.a > 0)
						{
							if (num15 > _alphaCutoff)
							{
								_dest[num16] = (float)_customColor.r;
							}
						}
						else
						{
							float num17 = _src.heightConst;
							if (_src.heightPixels.IsCreated)
							{
								num17 = StampManager.CalcRotatedValue(ref rotateParams, num14, num13, ref _src.heightPixels);
							}
							float num18 = num15 * _alphaScale;
							float num19 = _dest[num16];
							if (_additive)
							{
								num19 += num17 * num18;
							}
							else
							{
								num19 += (num17 - num19) * num18;
							}
							_dest[num16] = num19;
						}
					}
				}
			}
		}

		[BurstCompile(CompileSynchronously = true)]
		[PublicizedFrom(EAccessModifier.Internal)]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void DrawWaterStamp$BurstManaged(ref NativeArray<float> _dest, ref NativeArray<float> _src, int _x, int _y, int _destWidth, int _destHeight, int _srcWidth, int _srcHeight, float _alpha, float _scale, float _customColor, float _angle)
		{
			_x -= (int)((float)_srcWidth * _scale) / 2;
			_y -= (int)((float)_srcHeight * _scale) / 2;
			double num = (double)(0.017453292f * _angle);
			float num2 = (float)Math.Sin(num);
			float num3 = (float)Math.Cos(num);
			int num4 = (int)Math.Floor((double)((float)(((int)Math.Sqrt((double)(_srcWidth * _srcWidth + _srcHeight * _srcHeight)) - _srcWidth) / 2) * _scale));
			int num5 = (int)Math.Floor((double)((float)_srcWidth * _scale + (float)num4));
			num4 = -num4;
			int num6 = num4;
			int num7 = _x + num4;
			if (num7 < 0)
			{
				num6 -= num7;
			}
			int num8 = num5;
			num7 = _x + num5;
			if (num7 >= _destWidth)
			{
				num8 -= num7 - _destWidth;
			}
			int num9 = num4;
			int num10 = _y + num4;
			if (num10 < 0)
			{
				num9 -= num10;
			}
			int num11 = num5;
			num10 = _y + num5;
			if (num10 >= _destHeight)
			{
				num11 -= num10 - _destHeight;
			}
			StampManager.RotateParams rotateParams;
			rotateParams.sine = num2;
			rotateParams.cosine = num3;
			rotateParams.width = _srcWidth;
			rotateParams.height = _srcHeight;
			rotateParams.halfWidth = (float)(_srcWidth / 2);
			rotateParams.halfHeight = (float)(_srcHeight / 2);
			rotateParams.isWater = true;
			for (int i = num9; i < num11; i++)
			{
				int num12 = (_y + i) * _destWidth;
				float num13 = (float)i / _scale;
				for (int j = num6; j < num8; j++)
				{
					int num14 = _x + j + num12;
					if (_dest[num14] <= 0f && StampManager.CalcRotatedValue(ref rotateParams, (float)j / _scale, num13, ref _src) >= 1E-05f)
					{
						_dest[num14] = _customColor;
					}
				}
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public const float cAlphaCutoff = 1E-05f;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly WorldBuilder worldBuilder;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly Dictionary<string, RawStamp> AllStamps = new Dictionary<string, RawStamp>();

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly List<RawStamp> tempGetStampList = new List<RawStamp>();

		[PublicizedFrom(EAccessModifier.Private)]
		public struct RotateParams
		{
			public float sine;

			public float cosine;

			public int width;

			public int height;

			public float halfWidth;

			public float halfHeight;

			public bool isWater;
		}

		[PublicizedFrom(EAccessModifier.Internal)]
		public delegate void DrawStamp_0000B65F$PostfixBurstDelegate(ref NativeArray<float> _dest, ref RawStamp.Data _src, int _x, int _y, int _destWidth, int _destHeight, float _alphaScale, bool _additive, float _scale, ref Color32 _customColor, float _alphaCutoff, float _angle);

		[PublicizedFrom(EAccessModifier.Internal)]
		public static class DrawStamp_0000B65F$BurstDirectCall
		{
			[BurstDiscard]
			[PublicizedFrom(EAccessModifier.Private)]
			public static void GetFunctionPointerDiscard(ref IntPtr A_0)
			{
				if (StampManager.DrawStamp_0000B65F$BurstDirectCall.Pointer == 0)
				{
					StampManager.DrawStamp_0000B65F$BurstDirectCall.Pointer = BurstCompiler.GetILPPMethodFunctionPointer2(StampManager.DrawStamp_0000B65F$BurstDirectCall.DeferredCompilation, methodof(StampManager.DrawStamp$BurstManaged(ref NativeArray<float>, ref RawStamp.Data, int, int, int, int, float, bool, float, ref Color32, float, float)).MethodHandle, typeof(StampManager.DrawStamp_0000B65F$PostfixBurstDelegate).TypeHandle);
				}
				A_0 = StampManager.DrawStamp_0000B65F$BurstDirectCall.Pointer;
			}

			[PublicizedFrom(EAccessModifier.Private)]
			public static IntPtr GetFunctionPointer()
			{
				IntPtr intPtr = (IntPtr)0;
				StampManager.DrawStamp_0000B65F$BurstDirectCall.GetFunctionPointerDiscard(ref intPtr);
				return intPtr;
			}

			public static void Constructor()
			{
				StampManager.DrawStamp_0000B65F$BurstDirectCall.DeferredCompilation = BurstCompiler.CompileILPPMethod2(methodof(StampManager.DrawStamp(ref NativeArray<float>, ref RawStamp.Data, int, int, int, int, float, bool, float, ref Color32, float, float)).MethodHandle);
			}

			public static void Initialize()
			{
			}

			// Note: this type is marked as 'beforefieldinit'.
			[PublicizedFrom(EAccessModifier.Private)]
			static DrawStamp_0000B65F$BurstDirectCall()
			{
				StampManager.DrawStamp_0000B65F$BurstDirectCall.Constructor();
			}

			public static void Invoke(ref NativeArray<float> _dest, ref RawStamp.Data _src, int _x, int _y, int _destWidth, int _destHeight, float _alphaScale, bool _additive, float _scale, ref Color32 _customColor, float _alphaCutoff, float _angle)
			{
				if (BurstCompiler.IsEnabled)
				{
					IntPtr functionPointer = StampManager.DrawStamp_0000B65F$BurstDirectCall.GetFunctionPointer();
					if (functionPointer != 0)
					{
						calli(System.Void(Unity.Collections.NativeArray`1<System.Single>&,WorldGenerationEngineFinal.RawStamp/Data&,System.Int32,System.Int32,System.Int32,System.Int32,System.Single,System.Boolean,System.Single,UnityEngine.Color32&,System.Single,System.Single), ref _dest, ref _src, _x, _y, _destWidth, _destHeight, _alphaScale, _additive, _scale, ref _customColor, _alphaCutoff, _angle, functionPointer);
						return;
					}
				}
				StampManager.DrawStamp$BurstManaged(ref _dest, ref _src, _x, _y, _destWidth, _destHeight, _alphaScale, _additive, _scale, ref _customColor, _alphaCutoff, _angle);
			}

			[PublicizedFrom(EAccessModifier.Private)]
			public static IntPtr Pointer;

			[PublicizedFrom(EAccessModifier.Private)]
			public static IntPtr DeferredCompilation;
		}

		[PublicizedFrom(EAccessModifier.Internal)]
		public delegate void DrawWaterStamp_0000B662$PostfixBurstDelegate(ref NativeArray<float> _dest, ref NativeArray<float> _src, int _x, int _y, int _destWidth, int _destHeight, int _srcWidth, int _srcHeight, float _alpha, float _scale, float _customColor, float _angle);

		[PublicizedFrom(EAccessModifier.Internal)]
		public static class DrawWaterStamp_0000B662$BurstDirectCall
		{
			[BurstDiscard]
			[PublicizedFrom(EAccessModifier.Private)]
			public static void GetFunctionPointerDiscard(ref IntPtr A_0)
			{
				if (StampManager.DrawWaterStamp_0000B662$BurstDirectCall.Pointer == 0)
				{
					StampManager.DrawWaterStamp_0000B662$BurstDirectCall.Pointer = BurstCompiler.GetILPPMethodFunctionPointer2(StampManager.DrawWaterStamp_0000B662$BurstDirectCall.DeferredCompilation, methodof(StampManager.DrawWaterStamp$BurstManaged(ref NativeArray<float>, ref NativeArray<float>, int, int, int, int, int, int, float, float, float, float)).MethodHandle, typeof(StampManager.DrawWaterStamp_0000B662$PostfixBurstDelegate).TypeHandle);
				}
				A_0 = StampManager.DrawWaterStamp_0000B662$BurstDirectCall.Pointer;
			}

			[PublicizedFrom(EAccessModifier.Private)]
			public static IntPtr GetFunctionPointer()
			{
				IntPtr intPtr = (IntPtr)0;
				StampManager.DrawWaterStamp_0000B662$BurstDirectCall.GetFunctionPointerDiscard(ref intPtr);
				return intPtr;
			}

			public static void Constructor()
			{
				StampManager.DrawWaterStamp_0000B662$BurstDirectCall.DeferredCompilation = BurstCompiler.CompileILPPMethod2(methodof(StampManager.DrawWaterStamp(ref NativeArray<float>, ref NativeArray<float>, int, int, int, int, int, int, float, float, float, float)).MethodHandle);
			}

			public static void Initialize()
			{
			}

			// Note: this type is marked as 'beforefieldinit'.
			[PublicizedFrom(EAccessModifier.Private)]
			static DrawWaterStamp_0000B662$BurstDirectCall()
			{
				StampManager.DrawWaterStamp_0000B662$BurstDirectCall.Constructor();
			}

			public static void Invoke(ref NativeArray<float> _dest, ref NativeArray<float> _src, int _x, int _y, int _destWidth, int _destHeight, int _srcWidth, int _srcHeight, float _alpha, float _scale, float _customColor, float _angle)
			{
				if (BurstCompiler.IsEnabled)
				{
					IntPtr functionPointer = StampManager.DrawWaterStamp_0000B662$BurstDirectCall.GetFunctionPointer();
					if (functionPointer != 0)
					{
						calli(System.Void(Unity.Collections.NativeArray`1<System.Single>&,Unity.Collections.NativeArray`1<System.Single>&,System.Int32,System.Int32,System.Int32,System.Int32,System.Int32,System.Int32,System.Single,System.Single,System.Single,System.Single), ref _dest, ref _src, _x, _y, _destWidth, _destHeight, _srcWidth, _srcHeight, _alpha, _scale, _customColor, _angle, functionPointer);
						return;
					}
				}
				StampManager.DrawWaterStamp$BurstManaged(ref _dest, ref _src, _x, _y, _destWidth, _destHeight, _srcWidth, _srcHeight, _alpha, _scale, _customColor, _angle);
			}

			[PublicizedFrom(EAccessModifier.Private)]
			public static IntPtr Pointer;

			[PublicizedFrom(EAccessModifier.Private)]
			public static IntPtr DeferredCompilation;
		}
	}
}
