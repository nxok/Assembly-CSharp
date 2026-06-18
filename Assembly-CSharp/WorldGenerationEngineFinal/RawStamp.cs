using System;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;

namespace WorldGenerationEngineFinal
{
	[BurstCompile(CompileSynchronously = true)]
	public class RawStamp
	{
		[BurstCompile(CompileSynchronously = true)]
		public static void SmoothAlpha(ref RawStamp.Data d, int _boxSize)
		{
			RawStamp.SmoothAlpha_0000B63C$BurstDirectCall.Invoke(ref d, _boxSize);
		}

		public void BoxAlpha()
		{
			for (int i = 0; i < this.height; i += 4)
			{
				for (int j = 0; j < this.width; j += 4)
				{
					int num = j + i * this.width;
					double num2 = 0.0;
					for (int k = 0; k < 4; k++)
					{
						for (int l = 0; l < 4; l++)
						{
							num2 += (double)this.data.alphaPixels[num + l + k * this.width];
						}
					}
					num2 /= 16.0;
					for (int m = 0; m < 4; m++)
					{
						for (int n = 0; n < 4; n++)
						{
							this.data.alphaPixels[num + n + m * this.width] = (float)num2;
						}
					}
				}
			}
		}

		public void Clear()
		{
			this.data.heightPixels.Dispose();
			this.data.alphaPixels.Dispose();
			this.data.waterPixels.Dispose();
		}

		public int width
		{
			get
			{
				return this.data.width;
			}
		}

		public int height
		{
			get
			{
				return this.data.height;
			}
		}

		[BurstCompile(CompileSynchronously = true)]
		[PublicizedFrom(EAccessModifier.Internal)]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void SmoothAlpha$BurstManaged(ref RawStamp.Data d, int _boxSize)
		{
			NativeArray<float> nativeArray = new NativeArray<float>(d.alphaPixels.Length, 4, 0);
			for (int i = 0; i < d.height; i++)
			{
				for (int j = 0; j < d.width; j++)
				{
					float num = 0f;
					int num2 = 0;
					for (int k = -1; k < _boxSize; k++)
					{
						int num3 = i + k;
						if (num3 < d.height)
						{
							for (int l = -1; l < _boxSize; l++)
							{
								int num4 = j + l;
								if (num4 < d.width)
								{
									num += d.alphaPixels[num4 + num3 * d.width];
									num2++;
								}
							}
						}
					}
					num /= (float)num2;
					nativeArray[j + i * d.width] = num;
				}
			}
			d.alphaPixels.Dispose();
			d.alphaPixels = nativeArray;
		}

		public string name;

		public RawStamp.Data data;

		public struct Data
		{
			public float heightConst;

			public NativeArray<float> heightPixels;

			public float alphaConst;

			public NativeArray<float> alphaPixels;

			public NativeArray<float> waterPixels;

			public int width;

			public int height;
		}

		[PublicizedFrom(EAccessModifier.Internal)]
		public delegate void SmoothAlpha_0000B63C$PostfixBurstDelegate(ref RawStamp.Data d, int _boxSize);

		[PublicizedFrom(EAccessModifier.Internal)]
		public static class SmoothAlpha_0000B63C$BurstDirectCall
		{
			[BurstDiscard]
			[PublicizedFrom(EAccessModifier.Private)]
			public static void GetFunctionPointerDiscard(ref IntPtr A_0)
			{
				if (RawStamp.SmoothAlpha_0000B63C$BurstDirectCall.Pointer == 0)
				{
					RawStamp.SmoothAlpha_0000B63C$BurstDirectCall.Pointer = BurstCompiler.GetILPPMethodFunctionPointer2(RawStamp.SmoothAlpha_0000B63C$BurstDirectCall.DeferredCompilation, methodof(RawStamp.SmoothAlpha$BurstManaged(ref RawStamp.Data, int)).MethodHandle, typeof(RawStamp.SmoothAlpha_0000B63C$PostfixBurstDelegate).TypeHandle);
				}
				A_0 = RawStamp.SmoothAlpha_0000B63C$BurstDirectCall.Pointer;
			}

			[PublicizedFrom(EAccessModifier.Private)]
			public static IntPtr GetFunctionPointer()
			{
				IntPtr intPtr = (IntPtr)0;
				RawStamp.SmoothAlpha_0000B63C$BurstDirectCall.GetFunctionPointerDiscard(ref intPtr);
				return intPtr;
			}

			public static void Constructor()
			{
				RawStamp.SmoothAlpha_0000B63C$BurstDirectCall.DeferredCompilation = BurstCompiler.CompileILPPMethod2(methodof(RawStamp.SmoothAlpha(ref RawStamp.Data, int)).MethodHandle);
			}

			public static void Initialize()
			{
			}

			// Note: this type is marked as 'beforefieldinit'.
			[PublicizedFrom(EAccessModifier.Private)]
			static SmoothAlpha_0000B63C$BurstDirectCall()
			{
				RawStamp.SmoothAlpha_0000B63C$BurstDirectCall.Constructor();
			}

			public static void Invoke(ref RawStamp.Data d, int _boxSize)
			{
				if (BurstCompiler.IsEnabled)
				{
					IntPtr functionPointer = RawStamp.SmoothAlpha_0000B63C$BurstDirectCall.GetFunctionPointer();
					if (functionPointer != 0)
					{
						calli(System.Void(WorldGenerationEngineFinal.RawStamp/Data&,System.Int32), ref d, _boxSize, functionPointer);
						return;
					}
				}
				RawStamp.SmoothAlpha$BurstManaged(ref d, _boxSize);
			}

			[PublicizedFrom(EAccessModifier.Private)]
			public static IntPtr Pointer;

			[PublicizedFrom(EAccessModifier.Private)]
			public static IntPtr DeferredCompilation;
		}
	}
}
