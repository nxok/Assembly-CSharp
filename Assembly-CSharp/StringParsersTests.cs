using System;
using System.Globalization;
using System.Text;
using UnityEngine;

public class StringParsersTests
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static bool P8S(string _in, out sbyte _out)
	{
		return StringParsers.TryParseSInt8(_in, out _out);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool P8Us(string _in, out byte _out)
	{
		return StringParsers.TryParseUInt8(_in, out _out);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool P16S(string _in, out short _out)
	{
		return StringParsers.TryParseSInt16(_in, out _out);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool P16Us(string _in, out ushort _out)
	{
		return StringParsers.TryParseUInt16(_in, out _out);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool P32S(string _in, out int _out)
	{
		return StringParsers.TryParseSInt32(_in, out _out);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool P32Us(string _in, out uint _out)
	{
		return StringParsers.TryParseUInt32(_in, out _out);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool P64S(string _in, out long _out)
	{
		return StringParsers.TryParseSInt64(_in, out _out);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool P64Us(string _in, out ulong _out)
	{
		return StringParsers.TryParseUInt64(_in, out _out);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool PB(string _in, out bool _out)
	{
		return StringParsers.TryParseBool(_in, out _out);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static Vector3i PV3i(string _in)
	{
		return StringParsers.ParseVector3i(_in, 0, -1, false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static Vector2 PV2old(string _s)
	{
		string[] array = _s.Split(',', StringSplitOptions.None);
		if (array.Length != 2)
		{
			return Vector2.zero;
		}
		return new Vector2(float.Parse(array[0], NumberStyles.Any, CultureInfo.InvariantCulture), float.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static Vector3 PV3old(string _s)
	{
		string[] array = _s.Split(',', StringSplitOptions.None);
		if (array.Length != 3)
		{
			return Vector3.zero;
		}
		return new Vector3(float.Parse(array[0], NumberStyles.Any, CultureInfo.InvariantCulture), float.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture), float.Parse(array[2], NumberStyles.Any, CultureInfo.InvariantCulture));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static Vector2d PV2dOld(string _s)
	{
		string[] array = _s.Split(',', StringSplitOptions.None);
		if (array.Length != 2)
		{
			return Vector2d.Zero;
		}
		return new Vector2d(double.Parse(array[0], NumberStyles.Any, CultureInfo.InvariantCulture), double.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static Vector3d PV3dOld(string _s)
	{
		string[] array = _s.Split(',', StringSplitOptions.None);
		if (array.Length != 3)
		{
			return Vector3d.Zero;
		}
		return new Vector3d(double.Parse(array[0], NumberStyles.Any, CultureInfo.InvariantCulture), double.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture), double.Parse(array[2], NumberStyles.Any, CultureInfo.InvariantCulture));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static Vector2i PV2iOld(string _s)
	{
		string[] array = _s.Split(',', StringSplitOptions.None);
		if (array.Length != 2)
		{
			return Vector2i.zero;
		}
		return new Vector2i(int.Parse(array[0]), int.Parse(array[1]));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static Vector3i PV3iOld(string _s)
	{
		string[] array = _s.Split(',', StringSplitOptions.None);
		if (array.Length != 3)
		{
			return Vector3i.zero;
		}
		return new Vector3i(int.Parse(array[0]), int.Parse(array[1]), int.Parse(array[2]));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static TEnum PEnumOld<TEnum>(string _s) where TEnum : struct, IConvertible
	{
		return (TEnum)((object)Enum.Parse(typeof(TEnum), _s));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static TEnum PEnumNew<TEnum>(string _s) where TEnum : struct, IConvertible
	{
		return EnumUtils.Parse<TEnum>(_s, false);
	}

	public static void RunTests()
	{
		int num = 10000;
		Random random = new Random();
		string[] array = new string[100];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = ((random.Next() % 2 == 1) ? "-" : "") + (random.Next() & 255).ToString();
		}
		array[0] = "-256";
		array[1] = "-257";
		array[2] = "255";
		array[3] = "256";
		array[4] = "-128";
		array[5] = "-129";
		array[6] = "127";
		array[7] = "128";
		array[8] = "-0";
		StringParsersTests.TestClassTryParse<sbyte> testClassTryParse = new StringParsersTests.TestClassTryParse<sbyte>("SInt8", new StringParsersTests.TryParseFunc<sbyte>(sbyte.TryParse), new StringParsersTests.TryParseFunc<sbyte>(StringParsersTests.P8S));
		testClassTryParse.RunTests(array, num);
		StringParsersTests.TestClassTryParse<byte> testClassTryParse2 = new StringParsersTests.TestClassTryParse<byte>("UInt8", new StringParsersTests.TryParseFunc<byte>(byte.TryParse), new StringParsersTests.TryParseFunc<byte>(StringParsersTests.P8Us));
		testClassTryParse2.RunTests(array, num);
		array = new string[100];
		for (int j = 0; j < array.Length; j++)
		{
			array[j] = ((random.Next() % 2 == 1) ? "-" : "") + (random.Next() & 65535).ToString();
		}
		array[0] = "-65536";
		array[1] = "-65537";
		array[2] = "65535";
		array[3] = "65536";
		array[4] = "-32768";
		array[5] = "-32769";
		array[6] = "32767";
		array[7] = "32768";
		StringParsersTests.TestClassTryParse<short> testClassTryParse3 = new StringParsersTests.TestClassTryParse<short>("SInt16", new StringParsersTests.TryParseFunc<short>(short.TryParse), new StringParsersTests.TryParseFunc<short>(StringParsersTests.P16S));
		testClassTryParse3.RunTests(array, num);
		StringParsersTests.TestClassTryParse<ushort> testClassTryParse4 = new StringParsersTests.TestClassTryParse<ushort>("UInt16", new StringParsersTests.TryParseFunc<ushort>(ushort.TryParse), new StringParsersTests.TryParseFunc<ushort>(StringParsersTests.P16Us));
		testClassTryParse4.RunTests(array, num);
		array = new string[100];
		for (int k = 0; k < array.Length; k++)
		{
			array[k] = ((random.Next() % 2 == 1) ? "-" : "") + random.Next().ToString();
		}
		StringParsersTests.TestClassTryParse<int> testClassTryParse5 = new StringParsersTests.TestClassTryParse<int>("SInt32", new StringParsersTests.TryParseFunc<int>(int.TryParse), new StringParsersTests.TryParseFunc<int>(StringParsersTests.P32S));
		testClassTryParse5.RunTests(array, num);
		StringParsersTests.TestClassTryParse<uint> testClassTryParse6 = new StringParsersTests.TestClassTryParse<uint>("UInt32", new StringParsersTests.TryParseFunc<uint>(uint.TryParse), new StringParsersTests.TryParseFunc<uint>(StringParsersTests.P32Us));
		testClassTryParse6.RunTests(array, num);
		array = new string[100];
		for (int l = 0; l < array.Length; l++)
		{
			array[l] = ((random.Next() % 2 == 1) ? "-" : "") + (random.Next() * random.Next()).ToString();
		}
		array[0] = "-9223372036854775808";
		array[1] = "9223372036854775807";
		StringParsersTests.TestClassTryParse<long> testClassTryParse7 = new StringParsersTests.TestClassTryParse<long>("SInt64", new StringParsersTests.TryParseFunc<long>(long.TryParse), new StringParsersTests.TryParseFunc<long>(StringParsersTests.P64S));
		testClassTryParse7.RunTests(array, num);
		StringParsersTests.TestClassTryParse<ulong> testClassTryParse8 = new StringParsersTests.TestClassTryParse<ulong>("UInt64", new StringParsersTests.TryParseFunc<ulong>(ulong.TryParse), new StringParsersTests.TryParseFunc<ulong>(StringParsersTests.P64Us));
		testClassTryParse8.RunTests(array, num);
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("SInt8");
		stringBuilder.Append(testClassTryParse.GetResults());
		stringBuilder.AppendLine("UInt8");
		stringBuilder.Append(testClassTryParse2.GetResults());
		stringBuilder.AppendLine("SInt16");
		stringBuilder.Append(testClassTryParse3.GetResults());
		stringBuilder.AppendLine("UInt16");
		stringBuilder.Append(testClassTryParse4.GetResults());
		stringBuilder.AppendLine("SInt32");
		stringBuilder.Append(testClassTryParse5.GetResults());
		stringBuilder.AppendLine("UInt32");
		stringBuilder.Append(testClassTryParse6.GetResults());
		stringBuilder.AppendLine("SInt64");
		stringBuilder.Append(testClassTryParse7.GetResults());
		stringBuilder.AppendLine("UInt64");
		stringBuilder.Append(testClassTryParse8.GetResults());
		SdFile.WriteAllText("E:\\parsing_int.txt", stringBuilder.ToString());
		array = new string[] { "true", "false" };
		StringParsersTests.TestClassTryParse<bool> testClassTryParse9 = new StringParsersTests.TestClassTryParse<bool>("Bool NoWS", new StringParsersTests.TryParseFunc<bool>(bool.TryParse), new StringParsersTests.TryParseFunc<bool>(StringParsersTests.PB));
		testClassTryParse9.RunTests(array, num);
		array = new string[] { " true  ", "   false " };
		StringParsersTests.TestClassTryParse<bool> testClassTryParse10 = new StringParsersTests.TestClassTryParse<bool>("Bool WS", new StringParsersTests.TryParseFunc<bool>(bool.TryParse), new StringParsersTests.TryParseFunc<bool>(StringParsersTests.PB));
		testClassTryParse10.RunTests(array, num);
		StringBuilder stringBuilder2 = new StringBuilder();
		stringBuilder2.AppendLine("Bool NoWs");
		stringBuilder2.Append(testClassTryParse9.GetResults());
		stringBuilder2.AppendLine("Bool Ws");
		stringBuilder2.Append(testClassTryParse10.GetResults());
		SdFile.WriteAllText("E:\\parsing_bool.txt", stringBuilder2.ToString());
		array = new string[100];
		for (int m = 0; m < array.Length; m++)
		{
			string text = ((random.Next() % 2 == 1) ? "-" : "") + (random.NextDouble() * (double)random.Next() * (double)random.Next()).ToCultureInvariantString();
			string text2 = ((random.Next() % 2 == 1) ? "-" : "") + (random.NextDouble() * (double)random.Next() * (double)random.Next()).ToCultureInvariantString();
			array[m] = text + "," + text2;
		}
		StringParsersTests.TestClassParse<Vector2d> testClassParse = new StringParsersTests.TestClassParse<Vector2d>("Vector2d", new StringParsersTests.ParseFunc<Vector2d>(StringParsersTests.PV2dOld), new StringParsersTests.ParseFunc<Vector2d>(StringParsers.ParseVector2d));
		testClassParse.RunTests(array, num);
		array = new string[100];
		for (int n = 0; n < array.Length; n++)
		{
			string text3 = ((random.Next() % 2 == 1) ? "-" : "") + (random.NextDouble() * (double)random.Next() * (double)random.Next()).ToCultureInvariantString();
			string text4 = ((random.Next() % 2 == 1) ? "-" : "") + (random.NextDouble() * (double)random.Next() * (double)random.Next()).ToCultureInvariantString();
			string text5 = ((random.Next() % 2 == 1) ? "-" : "") + (random.NextDouble() * (double)random.Next() * (double)random.Next()).ToCultureInvariantString();
			array[n] = string.Concat(new string[] { text3, ",", text4, ",", text5 });
		}
		StringParsersTests.TestClassParse<Vector3d> testClassParse2 = new StringParsersTests.TestClassParse<Vector3d>("Vector3d", new StringParsersTests.ParseFunc<Vector3d>(StringParsersTests.PV3dOld), new StringParsersTests.ParseFunc<Vector3d>(StringParsers.ParseVector3d));
		testClassParse2.RunTests(array, num);
		StringBuilder stringBuilder3 = new StringBuilder();
		stringBuilder3.AppendLine("Vector2d");
		stringBuilder3.Append(testClassParse.GetResults());
		stringBuilder3.AppendLine("Vector3d");
		stringBuilder3.Append(testClassParse2.GetResults());
		SdFile.WriteAllText("E:\\parsing_vectord.txt", stringBuilder3.ToString());
		array = new string[100];
		int count = EnumUtils.Names<GameInfoInt>().Count;
		for (int num2 = 0; num2 < array.Length; num2++)
		{
			int num3 = random.Next() % count;
			array[num2] = EnumUtils.Names<GameInfoInt>()[num3];
		}
		StringParsersTests.TestClassParse<GameInfoInt> testClassParse3 = new StringParsersTests.TestClassParse<GameInfoInt>("Enum NoWs", new StringParsersTests.ParseFunc<GameInfoInt>(StringParsersTests.PEnumOld<GameInfoInt>), new StringParsersTests.ParseFunc<GameInfoInt>(StringParsersTests.PEnumNew<GameInfoInt>));
		testClassParse3.RunTests(array, num);
		for (int num4 = 0; num4 < array.Length; num4++)
		{
			array[num4] = " " + array[num4] + "  ";
		}
		StringParsersTests.TestClassParse<GameInfoInt> testClassParse4 = new StringParsersTests.TestClassParse<GameInfoInt>("Enum Ws", new StringParsersTests.ParseFunc<GameInfoInt>(StringParsersTests.PEnumOld<GameInfoInt>), new StringParsersTests.ParseFunc<GameInfoInt>(StringParsersTests.PEnumNew<GameInfoInt>));
		testClassParse4.RunTests(array, num);
		StringBuilder stringBuilder4 = new StringBuilder();
		stringBuilder4.AppendLine("Enums NoWs");
		stringBuilder4.Append(testClassParse3.GetResults());
		stringBuilder4.AppendLine("Enums Ws");
		stringBuilder4.Append(testClassParse4.GetResults());
		SdFile.WriteAllText("E:\\parsing_enums.txt", stringBuilder4.ToString());
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public delegate T ParseFunc<T>(string _in);

	[PublicizedFrom(EAccessModifier.Private)]
	public delegate bool TryParseFunc<T>(string _in, out T _out);

	[PublicizedFrom(EAccessModifier.Private)]
	public abstract class TestClassBase<TOut> where TOut : struct
	{
		public TestClassBase(string _testName)
		{
			this.testName = _testName;
		}

		public abstract void RunTests(string[] _testValues, int _runCount);

		public StringBuilder GetResults()
		{
			int num = 0;
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("Test#;Input Value;Same;SameEx;ValM;ValC;OkM;OkC;");
			for (int i = 0; i < this.inputValues.Length; i++)
			{
				stringBuilder.AppendLine(string.Format("{0};\"{1}\";{2};{3};{4};{5};{6};{7};", new object[]
				{
					i,
					this.inputValues[i],
					this.monoValue[i].Equals(this.customValue[i]),
					this.monoOk[i] == this.customOk[i],
					this.monoValue[i],
					this.customValue[i],
					this.monoOk[i],
					this.customOk[i]
				}));
				if (!this.monoValue[i].Equals(this.customValue[i]) || this.monoOk[i] != this.customOk[i])
				{
					num++;
				}
			}
			if (num > 0)
			{
				Log.Error(this.testName + " - failed: " + num.ToString());
			}
			return stringBuilder;
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public readonly string testName;

		[PublicizedFrom(EAccessModifier.Protected)]
		public string[] inputValues;

		[PublicizedFrom(EAccessModifier.Protected)]
		public bool[] monoOk;

		[PublicizedFrom(EAccessModifier.Protected)]
		public bool[] customOk;

		[PublicizedFrom(EAccessModifier.Protected)]
		public TOut[] monoValue;

		[PublicizedFrom(EAccessModifier.Protected)]
		public TOut[] customValue;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public class TestClassParse<TOut> : StringParsersTests.TestClassBase<TOut> where TOut : struct
	{
		public TestClassParse(string _testName, StringParsersTests.ParseFunc<TOut> _monoFunc, StringParsersTests.ParseFunc<TOut> _customFunc)
			: base(_testName)
		{
			this.monoFunc = _monoFunc;
			this.customFunc = _customFunc;
		}

		public override void RunTests(string[] _testValues, int _runCount)
		{
			this.inputValues = _testValues;
			this.monoOk = new bool[_testValues.Length];
			this.customOk = new bool[_testValues.Length];
			this.monoValue = new TOut[_testValues.Length];
			this.customValue = new TOut[_testValues.Length];
			for (int i = 0; i < _testValues.Length; i++)
			{
				this.monoOk[i] = true;
				this.customOk[i] = true;
			}
			foreach (string text in _testValues)
			{
				this.monoFunc(text);
				this.customFunc(text);
			}
			for (int k = 0; k < _runCount; k++)
			{
				int num = k % _testValues.Length;
				try
				{
					this.monoValue[num] = this.monoFunc(_testValues[num]);
				}
				catch (Exception)
				{
					this.monoOk[num] = false;
				}
			}
			for (int l = 0; l < _runCount; l++)
			{
				int num2 = l % _testValues.Length;
				try
				{
					this.customValue[num2] = this.customFunc(_testValues[num2]);
				}
				catch (Exception)
				{
					this.customOk[num2] = false;
				}
			}
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public readonly StringParsersTests.ParseFunc<TOut> monoFunc;

		[PublicizedFrom(EAccessModifier.Protected)]
		public readonly StringParsersTests.ParseFunc<TOut> customFunc;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public class TestClassTryParse<TOut> : StringParsersTests.TestClassBase<TOut> where TOut : struct
	{
		public TestClassTryParse(string _testName, StringParsersTests.TryParseFunc<TOut> _monoFunc, StringParsersTests.TryParseFunc<TOut> _customFunc)
			: base(_testName)
		{
			this.monoFunc = _monoFunc;
			this.customFunc = _customFunc;
		}

		public override void RunTests(string[] _testValues, int _runCount)
		{
			this.inputValues = _testValues;
			this.monoOk = new bool[_testValues.Length];
			this.customOk = new bool[_testValues.Length];
			this.monoValue = new TOut[_testValues.Length];
			this.customValue = new TOut[_testValues.Length];
			foreach (string text in _testValues)
			{
				this.monoFunc(text, out this.monoValue[0]);
				this.customFunc(text, out this.customValue[0]);
			}
			for (int j = 0; j < _runCount; j++)
			{
				int num = j % _testValues.Length;
				this.monoOk[num] = this.monoFunc(_testValues[num], out this.monoValue[num]);
			}
			for (int k = 0; k < _runCount; k++)
			{
				int num2 = k % _testValues.Length;
				this.customOk[num2] = this.customFunc(_testValues[num2], out this.customValue[num2]);
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly StringParsersTests.TryParseFunc<TOut> monoFunc;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly StringParsersTests.TryParseFunc<TOut> customFunc;
	}
}
