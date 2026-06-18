using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdVisitMap : ConsoleCmdAbstract
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Visit an given area of the map. Optionally run the density check on each visited chunk.";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "\n\t\t\t|Usage:\n\t\t\t|  1. visitmap <x1> <z1> <x2> <z2> [check]\n\t\t\t|  2. visitmap full [check]\n\t\t\t|  3. visitmap stop\n\t\t\t|1. Start visiting the map in the rectangle specified with the two edges defined by\n\t\t\t|   coordinate pairs x1/z1 and x2/z2. If the parameter \"check\" is added each visited\n\t\t\t|   chunk will be checked for density issues.\n\t\t\t|2. Start visiting the full map. If the parameter \"check\" is added each visited\n\t\t\t|   chunk will be checked for density issues.\n\t\t\t|3. Stop the current visitmap run.\n\t\t\t".Unindent(true);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[] { "visitmap" };
	}

	public bool IsRunning
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this.mapVisitor != null && this.mapVisitor.IsRunning();
		}
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (_params.Count != 1 && _params.Count != 2 && _params.Count != 4 && _params.Count != 5)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Wrong number of arguments, expected 1, 2, 4 or 5, found " + _params.Count.ToString() + ".");
			return;
		}
		bool flag = _params[_params.Count - 1].EqualsCaseInsensitive("check");
		if (_params.Count == 1)
		{
			if (_params[0].EqualsCaseInsensitive("stop"))
			{
				this.stop();
				return;
			}
			if (_params[0].EqualsCaseInsensitive("full"))
			{
				Vector3i vector3i;
				Vector3i vector3i2;
				GameManager.Instance.World.GetWorldExtent(out vector3i, out vector3i2);
				this.visit(vector3i, vector3i2, false);
				return;
			}
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Illegal arguments");
			return;
		}
		else
		{
			if (_params.Count != 2)
			{
				if (_params.Count == 4 || _params.Count == 5)
				{
					int num;
					if (!int.TryParse(_params[0], out num))
					{
						SingletonMonoBehaviour<SdtdConsole>.Instance.Output("The given x1 coordinate is not a valid integer");
						return;
					}
					int num2;
					if (!int.TryParse(_params[1], out num2))
					{
						SingletonMonoBehaviour<SdtdConsole>.Instance.Output("The given z1 coordinate is not a valid integer");
						return;
					}
					int num3;
					if (!int.TryParse(_params[2], out num3))
					{
						SingletonMonoBehaviour<SdtdConsole>.Instance.Output("The given x2 coordinate is not a valid integer");
						return;
					}
					int num4;
					if (!int.TryParse(_params[3], out num4))
					{
						SingletonMonoBehaviour<SdtdConsole>.Instance.Output("The given z2 coordinate is not a valid integer");
						return;
					}
					this.visit(new Vector3i(num, 0, num2), new Vector3i(num3, 0, num4), flag);
				}
				return;
			}
			if (_params[0].EqualsCaseInsensitive("full"))
			{
				Vector3i vector3i3;
				Vector3i vector3i4;
				GameManager.Instance.World.GetWorldExtent(out vector3i3, out vector3i4);
				this.visit(vector3i3, vector3i4, flag);
				return;
			}
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Illegal arguments");
			return;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void visit(Vector3i _start, Vector3i _end, bool _checkDensities)
	{
		if (this.mapVisitor != null && this.mapVisitor.IsRunning())
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("VisitMap already running. You can stop it with \"visitmap stop\".");
			return;
		}
		this.mapVisitor = new MapVisitor(_start, _end);
		this.mapVisitor.OnVisitChunk += this.LogChunk;
		this.mapVisitor.OnVisitChunk += this.RenderMinimap;
		if (_checkDensities)
		{
			this.densityMismatches = new List<Chunk.DensityMismatchInformation>();
			this.mapVisitor.OnVisitChunk += this.CheckDensities;
		}
		this.mapVisitor.OnVisitMapDone += this.OnDone;
		this.mapVisitor.Start();
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("Started visiting the map between {0} and {1}", this.mapVisitor.WorldPosStart, this.mapVisitor.WorldPosEnd));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void stop()
	{
		if (!this.IsRunning)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("VisitMap not running.");
			return;
		}
		this.mapVisitor.Stop();
		this.mapVisitor = null;
		this.densityMismatches = null;
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("VisitMap stopped.");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CheckDensities(Chunk _chunk, int _done, int _total, float _elapsed)
	{
		this.densityMismatches.AddRange(_chunk.CheckDensities(false));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void LogChunk(Chunk _chunk, int _done, int _total, float _elapsed)
	{
		if (_done % 200 != 0)
		{
			return;
		}
		float num = (float)(_total - _done) * (_elapsed / (float)_done);
		Log.Out("VisitMap ({3:00}%): {0} / {1} chunks done (estimated time left {2} seconds)", new object[]
		{
			_done,
			_total,
			num.ToCultureInvariantString("0.00"),
			Mathf.RoundToInt(100f * ((float)_done / (float)_total))
		});
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RenderMinimap(Chunk _chunk, int _done, int _total, float _elapsed)
	{
		_chunk.GetMapColors();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDone(int _chunks, float _duration)
	{
		Log.Out("VisitMap done, visited {0} chunks in {1} seconds (average {2} chunks/sec).", new object[]
		{
			_chunks,
			_duration.ToCultureInvariantString("0.00"),
			((float)_chunks / _duration).ToCultureInvariantString("0.00")
		});
		this.writeDensityMismatchFile();
		this.mapVisitor = null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void writeDensityMismatchFile()
	{
		if (this.densityMismatches == null)
		{
			return;
		}
		StringBuilder stringBuilder = new StringBuilder("[\n");
		for (int i = 0; i < this.densityMismatches.Count; i++)
		{
			if (i > 0)
			{
				stringBuilder.Append(", ");
			}
			stringBuilder.Append(this.densityMismatches[i].ToJsonString());
			stringBuilder.Append('\n');
		}
		stringBuilder.Append("]");
		SdFile.WriteAllText(((Application.platform == 1 || Application.platform == 45) ? (Application.dataPath + "/../../") : (Application.dataPath + "/../")) + "densitymismatch.json", stringBuilder.ToString());
		StringBuilder stringBuilder2 = new StringBuilder();
		stringBuilder2.AppendLine("x;y;z;Density;IsTerrain;BvType");
		for (int j = 0; j < this.densityMismatches.Count; j++)
		{
			stringBuilder2.AppendLine(string.Format("{0};{1};{2};{3};{4};{5}", new object[]
			{
				this.densityMismatches[j].x,
				this.densityMismatches[j].y,
				this.densityMismatches[j].z,
				this.densityMismatches[j].density,
				this.densityMismatches[j].isTerrain,
				this.densityMismatches[j].bvType
			}));
		}
		SdFile.WriteAllText(((Application.platform == 1 || Application.platform == 45) ? (Application.dataPath + "/../../") : (Application.dataPath + "/../")) + "densitymismatch.csv", stringBuilder2.ToString());
		this.densityMismatches = null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public MapVisitor mapVisitor;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<Chunk.DensityMismatchInformation> densityMismatches;
}
