using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using UnityEngine;

[Serializable]
public class AutomationScript
{
	public string ResolveSessionDir()
	{
		if (!string.IsNullOrEmpty(this.defaultSessionDir))
		{
			return this.defaultSessionDir;
		}
		return string.Concat(new string[]
		{
			Constants.cVersionInformation.ShortString,
			"_",
			GamePrefs.GetString(EnumGamePrefs.GameWorld),
			"_",
			GamePrefs.GetString(EnumGamePrefs.GameName),
			DateTime.UtcNow.ToString("yyyy-MM-dd_HH-mm-ss.fff")
		});
	}

	public int CountStepsOfType(AutomationStep.StepType stepType)
	{
		return this.steps.Count((AutomationStep s) => s.type == stepType);
	}

	public string Validate()
	{
		if (this.steps.Count == 0)
		{
			return "Script '" + this.name + "' has no steps.";
		}
		int num = 0;
		for (int i = 0; i < this.steps.Count; i++)
		{
			AutomationStep automationStep = this.steps[i];
			if (automationStep.type == AutomationStep.StepType.LoadGame)
			{
				if (string.IsNullOrEmpty(automationStep.world))
				{
					return string.Format("Script '{0}' step [{1}] LoadGame: 'world' is empty.", this.name, i);
				}
				if (string.IsNullOrEmpty(automationStep.gameName))
				{
					return string.Format("Script '{0}' step [{1}] LoadGame: 'gameName' is empty.", this.name, i);
				}
			}
			if (automationStep.type == AutomationStep.StepType.MovePingPong)
			{
				if (automationStep.duration <= 0f)
				{
					return string.Format("Script '{0}' step [{1}] MovePingPong: duration must be > 0.", this.name, i);
				}
				if (automationStep.pingPongCount < 1)
				{
					return string.Format("Script '{0}' step [{1}] MovePingPong: pingPongCount must be >= 1.", this.name, i);
				}
				if (automationStep.position == automationStep.positionB)
				{
					return string.Format("Script '{0}' step [{1}] MovePingPong: point A and point B are identical.", this.name, i);
				}
			}
			if (automationStep.type == AutomationStep.StepType.StartPerfSession)
			{
				if (automationStep.runCount < 1)
				{
					return string.Format("Script '{0}' step [{1}] StartPerfSession: runCount must be >= 1.", this.name, i);
				}
				if (num > 0)
				{
					return string.Format("Script '{0}' step [{1}] StartPerfSession: nested sessions are not supported.", this.name, i);
				}
				num++;
			}
			if (automationStep.type == AutomationStep.StepType.StopPerfSession)
			{
				if (num == 0)
				{
					return string.Format("Script '{0}' step [{1}] StopPerfSession: no matching StartPerfSession.", this.name, i);
				}
				num--;
			}
		}
		if (num > 0)
		{
			return "Script '" + this.name + "': StartPerfSession without matching StopPerfSession.";
		}
		return null;
	}

	public string Describe()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine(string.Format("Script '{0}'  steps={1}", this.name, this.steps.Count));
		for (int i = 0; i < this.steps.Count; i++)
		{
			stringBuilder.AppendLine(this.steps[i].Describe(i));
		}
		return stringBuilder.ToString().TrimEnd();
	}

	public static string GetScriptsDirectory()
	{
		string text = AutomationRunner.GetAutomationDataPath() + "Scripts";
		if (!Directory.Exists(text))
		{
			Directory.CreateDirectory(text);
		}
		return text;
	}

	public void SaveToFile(string scriptName = null)
	{
		Log.Error("[AutomationScript] Disabled for this build type.");
	}

	public static AutomationScript LoadFromFile(string scriptName)
	{
		Log.Error("[AutomationRunner] Disabled for this build type.");
		return null;
	}

	public static List<string> ListSavedScripts()
	{
		return (from n in Directory.GetFiles(AutomationScript.GetScriptsDirectory(), "*.json").Select(new Func<string, string>(Path.GetFileNameWithoutExtension))
			orderby n
			select n).ToList<string>();
	}

	public string name = "unnamed";

	public string defaultSessionDir = string.Empty;

	public List<AutomationStep> steps = new List<AutomationStep>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static readonly JsonSerializerSettings s_jsonSettings = new JsonSerializerSettings
	{
		Formatting = 1,
		Converters = 
		{
			new StringEnumConverter(),
			new AutomationScript.Vector3Converter()
		}
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public class Vector3Converter : JsonConverter<Vector3>
	{
		public override void WriteJson(JsonWriter writer, Vector3 value, JsonSerializer serializer)
		{
			writer.WriteStartObject();
			writer.WritePropertyName("x");
			writer.WriteValue(value.x);
			writer.WritePropertyName("y");
			writer.WriteValue(value.y);
			writer.WritePropertyName("z");
			writer.WriteValue(value.z);
			writer.WriteEndObject();
		}

		public override Vector3 ReadJson(JsonReader reader, Type objectType, Vector3 existingValue, bool hasExistingValue, JsonSerializer serializer)
		{
			JObject jobject = JObject.Load(reader);
			return new Vector3(jobject.Value<float>("x"), jobject.Value<float>("y"), jobject.Value<float>("z"));
		}
	}
}
