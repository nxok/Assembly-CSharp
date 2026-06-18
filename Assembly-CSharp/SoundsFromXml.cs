using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;
using Audio;
using UniLinq;

public class SoundsFromXml
{
	public static IEnumerator CreateSounds(XmlFile xmlFile)
	{
		XElement root = xmlFile.XmlDoc.Root;
		if (!root.HasElements)
		{
			throw new Exception("No element <mix> found!");
		}
		Manager.Reset();
		SoundsFromXml.ParseNode(null, root);
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ParseNode(string master, XElement root)
	{
		SoundsFromXml.controllerVibrationAudioSourceExclusions = new List<string>();
		foreach (XElement xelement in root.Elements("ControllerVibrationAudioSourceExclusions"))
		{
			foreach (XElement xelement2 in xelement.Elements())
			{
				if (xelement2.Name == "Source")
				{
					SoundsFromXml.controllerVibrationAudioSourceExclusions.Add(xelement2.GetAttribute("name"));
				}
			}
		}
		foreach (XElement xelement3 in root.Elements("SoundDataNode"))
		{
			SoundsFromXml.Parse(xelement3);
		}
		SoundsFromXml.controllerVibrationAudioSourceExclusions = null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void Parse(XElement node)
	{
		string value = node.Attributes().First<XAttribute>().Value;
		XmlData xmlData = new XmlData();
		xmlData.soundGroupName = value;
		xmlData.playImmediate = false;
		string text = null;
		foreach (XElement xelement in node.Elements())
		{
			string localName = xelement.Name.LocalName;
			if (localName.EqualsCaseInsensitive("audiosource"))
			{
				text = xelement.Attributes().First<XAttribute>().Value;
			}
			else if (localName.EqualsCaseInsensitive("noise"))
			{
				XElement xelement2 = xelement;
				string text2 = xelement2.GetAttribute("noise");
				xmlData.noiseData.volume = (string.IsNullOrEmpty(text2) ? 0f : StringParsers.ParseFloat(text2, 0, -1, NumberStyles.Any));
				text2 = xelement2.GetAttribute("time");
				xmlData.noiseData.time = (string.IsNullOrEmpty(text2) ? 0f : StringParsers.ParseFloat(text2, 0, -1, NumberStyles.Any));
				text2 = xelement2.GetAttribute("heat_map_strength");
				xmlData.noiseData.heatMapStrength = (string.IsNullOrEmpty(text2) ? 0f : StringParsers.ParseFloat(text2, 0, -1, NumberStyles.Any));
				text2 = xelement2.GetAttribute("heat_map_time");
				xmlData.noiseData.heatMapTime = (string.IsNullOrEmpty(text2) ? 100UL : ulong.Parse(text2));
				text2 = xelement2.GetAttribute("muffled_when_crouched");
				xmlData.noiseData.crouchMuffle = (string.IsNullOrEmpty(text2) ? 1f : StringParsers.ParseFloat(text2, 0, -1, NumberStyles.Any));
			}
			else if (localName.EqualsCaseInsensitive("audioclip"))
			{
				ClipSourceMap clipSourceMap = new ClipSourceMap();
				XElement xelement3 = xelement;
				clipSourceMap.clipName = xelement3.GetAttribute("ClipName");
				clipSourceMap.audioSourceName = xelement3.GetAttribute("AudioSourceName");
				if (xelement3.HasAttribute("Loop"))
				{
					clipSourceMap.forceLoop = StringParsers.ParseBool(xelement3.GetAttribute("Loop"), 0, -1, true);
				}
				clipSourceMap.clipName_distant = xelement3.GetAttribute("DistantClip");
				clipSourceMap.audioSourceName_distant = xelement3.GetAttribute("DistantSource");
				if (string.IsNullOrEmpty(clipSourceMap.audioSourceName))
				{
					clipSourceMap.audioSourceName = text;
				}
				bool flag;
				StringParsers.TryParseBool(xelement3.GetAttribute("AltSound"), out flag);
				if (flag)
				{
					xmlData.AddAltClipSourceMap(clipSourceMap);
				}
				else
				{
					xmlData.audioClipMap.Add(clipSourceMap);
				}
				if (text == null)
				{
					text = clipSourceMap.audioSourceName;
				}
				if (string.IsNullOrEmpty(clipSourceMap.audioSourceName))
				{
					Log.Error("ParseSoundDataNode() - missing audio source for " + clipSourceMap.clipName + ".");
				}
				clipSourceMap.subtitleID = xelement3.GetAttribute("Subtitle");
				clipSourceMap.hasSubtitle = !string.IsNullOrEmpty(clipSourceMap.subtitleID);
				if (xelement3.HasAttribute("profanity"))
				{
					clipSourceMap.profanity = StringParsers.ParseBool(xelement3.GetAttribute("profanity"), 0, -1, true);
				}
				if (clipSourceMap.profanity)
				{
					xmlData.hasProfanity = true;
				}
				DataLoader.PreloadBundle(clipSourceMap.audioSourceName);
				DataLoader.PreloadBundle(clipSourceMap.clipName);
				DataLoader.PreloadBundle(clipSourceMap.audioSourceName_distant);
				DataLoader.PreloadBundle(clipSourceMap.clipName_distant);
			}
			else if (localName.EqualsCaseInsensitive("localcrouchvolumescale"))
			{
				xmlData.localCrouchVolumeScale = StringParsers.ParseFloat(xelement.Attributes().First<XAttribute>().Value, 0, -1, NumberStyles.Any);
			}
			else if (localName.EqualsCaseInsensitive("crouchnoisescale"))
			{
				xmlData.crouchNoiseScale = StringParsers.ParseFloat(xelement.Attributes().First<XAttribute>().Value, 0, -1, NumberStyles.Any);
			}
			else if (localName.EqualsCaseInsensitive("noisescale"))
			{
				xmlData.noiseScale = StringParsers.ParseFloat(xelement.Attributes().First<XAttribute>().Value, 0, -1, NumberStyles.Any);
			}
			else if (localName.EqualsCaseInsensitive("maxvoices"))
			{
				xmlData.maxVoices = int.Parse(xelement.Attributes().First<XAttribute>().Value);
			}
			else if (localName.EqualsCaseInsensitive("maxVoicesPerEntity"))
			{
				xmlData.maxVoicesPerEntity = int.Parse(xelement.Attributes().First<XAttribute>().Value);
			}
			else if (localName.EqualsCaseInsensitive("prioritizeNewNodes"))
			{
				xmlData.prioritizeNewNodes = StringParsers.ParseBool(xelement.Attributes().First<XAttribute>().Value, 0, -1, true);
			}
			else if (localName.EqualsCaseInsensitive("maxrepeatrate"))
			{
				xmlData.maxRepeatRate = StringParsers.ParseFloat(xelement.Attributes().First<XAttribute>().Value, 0, -1, NumberStyles.Any);
			}
			else if (localName.EqualsCaseInsensitive("ignoredistancecheck"))
			{
				Manager.AddSoundToIgnoreDistanceCheckList(value);
			}
			else if (localName.EqualsCaseInsensitive("immediate"))
			{
				xmlData.playImmediate = true;
			}
			else if (localName.EqualsCaseInsensitive("sequence"))
			{
				xmlData.sequence = true;
			}
			else if (localName.EqualsCaseInsensitive("runningvolumescale"))
			{
				xmlData.runningVolumeScale = StringParsers.ParseFloat(xelement.Attributes().First<XAttribute>().Value, 0, -1, NumberStyles.Any);
			}
			else if (localName.EqualsCaseInsensitive("lowestpitch"))
			{
				xmlData.lowestPitch = StringParsers.ParseFloat(xelement.Attributes().First<XAttribute>().Value, 0, -1, NumberStyles.Any);
			}
			else if (localName.EqualsCaseInsensitive("highestpitch"))
			{
				xmlData.highestPitch = StringParsers.ParseFloat(xelement.Attributes().First<XAttribute>().Value, 0, -1, NumberStyles.Any);
			}
			else if (localName.EqualsCaseInsensitive("distantfadestart"))
			{
				xmlData.distantFadeStart = StringParsers.ParseFloat(xelement.Attributes().First<XAttribute>().Value, 0, -1, NumberStyles.Any);
			}
			else if (localName.EqualsCaseInsensitive("distantfadeend"))
			{
				xmlData.distantFadeEnd = StringParsers.ParseFloat(xelement.Attributes().First<XAttribute>().Value, 0, -1, NumberStyles.Any);
			}
			else if (localName.EqualsCaseInsensitive("channel"))
			{
				if (xelement.Attributes().First<XAttribute>().Value.EqualsCaseInsensitive("mouth"))
				{
					xmlData.channel = XmlData.Channel.Mouth;
				}
				else
				{
					xmlData.channel = XmlData.Channel.Environment;
				}
			}
			else if (localName.EqualsCaseInsensitive("priority"))
			{
				xmlData.priority = int.Parse(xelement.Attributes().First<XAttribute>().Value);
			}
			else if (localName.EqualsCaseInsensitive("vibratecontroller"))
			{
				xmlData.vibratesController = StringParsers.ParseBool(xelement.Attributes().First<XAttribute>().Value, 0, -1, true);
			}
			else if (localName.EqualsCaseInsensitive("vibrationstrengthmultiply"))
			{
				xmlData.vibrationStrengthMultiplier = StringParsers.ParseFloat(xelement.Attributes().First<XAttribute>().Value, 0, -1, NumberStyles.Any);
			}
		}
		xmlData.vibratesController = xmlData.vibratesController && !SoundsFromXml.controllerVibrationAudioSourceExclusions.Contains(text);
		Manager.AddAudioData(xmlData);
	}

	public static IEnumerator LoadSubtitleXML(XmlFile xmlFile)
	{
		XElement root = xmlFile.XmlDoc.Root;
		if (!root.HasElements)
		{
			throw new Exception("No element <mix> found!");
		}
		SoundsFromXml.ParseSubtitleNode(null, root);
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ParseSubtitleNode(string master, XElement root)
	{
		List<SubtitleData> list = new List<SubtitleData>();
		List<SubtitleSpeakerColor> list2 = new List<SubtitleSpeakerColor>();
		foreach (XElement xelement in root.Elements())
		{
			if (xelement.Name == "Subtitle")
			{
				SubtitleData subtitleData = new SubtitleData();
				xelement.Elements();
				subtitleData.name = xelement.Attributes().First<XAttribute>().Value;
				subtitleData.contentLocId = xelement.GetAttribute("contentLocId");
				subtitleData.speakerColorId = xelement.GetAttribute("speakerColor");
				subtitleData.speakerLocId = xelement.GetAttribute("speakerLocId");
				list.Add(subtitleData);
			}
			else if (xelement.Name == "SpeakerColors")
			{
				foreach (XElement xelement2 in xelement.Elements())
				{
					list2.Add(new SubtitleSpeakerColor
					{
						name = xelement2.Attributes().First<XAttribute>().Value,
						color = xelement2.Attributes().Last<XAttribute>().Value
					});
				}
			}
		}
		Manager.AddSubtitleData(list, list2);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static List<string> controllerVibrationAudioSourceExclusions;
}
