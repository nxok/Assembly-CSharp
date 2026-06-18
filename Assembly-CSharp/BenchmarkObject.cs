using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

public class BenchmarkObject
{
	[Conditional("PROFILEx")]
	public static void StartTimer(string _benchmarkName, object _watchObject)
	{
		BenchmarkObject.BenchmarkContainer benchmarkContainer = new BenchmarkObject.BenchmarkContainer(_benchmarkName);
		Dictionary<object, BenchmarkObject.BenchmarkContainer> dictionary = BenchmarkObject.benchmarks;
		lock (dictionary)
		{
			BenchmarkObject.benchmarks[_watchObject] = benchmarkContainer;
		}
		benchmarkContainer.startTick = DateTime.Now.Ticks;
	}

	[Conditional("PROFILEx")]
	public static void SwitchObject(object _old, object _new)
	{
		Dictionary<object, BenchmarkObject.BenchmarkContainer> dictionary = BenchmarkObject.benchmarks;
		lock (dictionary)
		{
			if (BenchmarkObject.benchmarks.ContainsKey(_old))
			{
				BenchmarkObject.BenchmarkContainer benchmarkContainer = BenchmarkObject.benchmarks[_old];
				BenchmarkObject.benchmarks.Remove(_old);
				BenchmarkObject.benchmarks[_new] = benchmarkContainer;
			}
			else
			{
				Log.Out("SWITCHOBJECT: Object not found");
			}
		}
	}

	[Conditional("PROFILEx")]
	public static void UpdateName(object _watchObject, string _nameAppend)
	{
		BenchmarkObject.BenchmarkContainer benchmarkContainer = null;
		Dictionary<object, BenchmarkObject.BenchmarkContainer> dictionary = BenchmarkObject.benchmarks;
		lock (dictionary)
		{
			if (BenchmarkObject.benchmarks.ContainsKey(_watchObject))
			{
				benchmarkContainer = BenchmarkObject.benchmarks[_watchObject];
			}
		}
		if (benchmarkContainer != null)
		{
			BenchmarkObject.BenchmarkContainer benchmarkContainer2 = benchmarkContainer;
			benchmarkContainer2.name += _nameAppend;
			return;
		}
		Log.Out("UPDATENAME: Object not found: " + _nameAppend);
	}

	[Conditional("PROFILEx")]
	public static void StopTimer(object _watchObject)
	{
		long ticks = DateTime.Now.Ticks;
		Dictionary<object, BenchmarkObject.BenchmarkContainer> dictionary = BenchmarkObject.benchmarks;
		lock (dictionary)
		{
			if (BenchmarkObject.benchmarks.ContainsKey(_watchObject))
			{
				BenchmarkObject.benchmarks[_watchObject].endTick = ticks;
			}
			else
			{
				Log.Out("STOPTIMER: Object not found");
			}
		}
	}

	[Conditional("PROFILEx")]
	public static void PrintAll()
	{
		if (BenchmarkObject.benchmarks.Count > 0)
		{
			StringBuilder stringBuilder = new StringBuilder();
			int num = 0;
			foreach (KeyValuePair<object, BenchmarkObject.BenchmarkContainer> keyValuePair in BenchmarkObject.benchmarks)
			{
				if (keyValuePair.Value.name.Length > num)
				{
					num = keyValuePair.Value.name.Length;
				}
			}
			string text = "{0} {1} {2} {3}" + Environment.NewLine;
			stringBuilder.Append(string.Format(text, new object[] { "Name", "Start", "End", "Duration" }));
			foreach (BenchmarkObject.BenchmarkContainer benchmarkContainer in from b in BenchmarkObject.benchmarks.Values.ToList<BenchmarkObject.BenchmarkContainer>()
				orderby b.startTick
				select b)
			{
				stringBuilder.Append(string.Format(text, new object[]
				{
					benchmarkContainer.name,
					benchmarkContainer.startTick / 10L,
					benchmarkContainer.endTick / 10L,
					benchmarkContainer.ticks / 10L
				}));
			}
			SdFile.WriteAllText(GameIO.GetGameDir("") + "durations.txt", stringBuilder.ToString());
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static Dictionary<object, BenchmarkObject.BenchmarkContainer> benchmarks = new Dictionary<object, BenchmarkObject.BenchmarkContainer>();

	public class BenchmarkContainer
	{
		public string name
		{
			get
			{
				return this.pName;
			}
			set
			{
				this.pName = value;
			}
		}

		public long ticks
		{
			get
			{
				return this.endTick - this.startTick;
			}
		}

		public BenchmarkContainer(string _name)
		{
			this.pName = _name;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public string pName;

		public long startTick;

		public long endTick;
	}
}
