using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;
using UnityEngine;

public static class MiscFromXml
{
	public static IEnumerator Create(XmlFile _xmlFile)
	{
		XElement root = _xmlFile.XmlDoc.Root;
		if (root == null)
		{
			yield break;
		}
		foreach (XElement xelement in root.Elements("animation"))
		{
			if (xelement.Name == "animation")
			{
				foreach (XElement xelement2 in xelement.Elements("hold"))
				{
					string text = xelement2.GetAttribute("id");
					if (text.Length == 0)
					{
						throw new Exception("Attribute 'id' missing in hold");
					}
					int num;
					if (!int.TryParse(text, out num))
					{
						throw new Exception("Invalid hold id");
					}
					int num2 = 0;
					text = xelement2.GetAttribute("carry");
					if (text.Length > 0 && !int.TryParse(text, out num2))
					{
						throw new Exception("Invalid hold carry");
					}
					float num3 = 0f;
					if (xelement2.HasAttribute("ray_cast"))
					{
						num3 = StringParsers.ParseFloat(xelement2.GetAttribute("ray_cast"), 0, -1, NumberStyles.Any);
					}
					float num4 = num3;
					if (xelement2.HasAttribute("ray_cast_moving"))
					{
						num4 = StringParsers.ParseFloat(xelement2.GetAttribute("ray_cast_moving"), 0, -1, NumberStyles.Any);
					}
					float num5 = Constants.cMinHolsterTime;
					if (xelement2.HasAttribute("holster"))
					{
						num5 = Utils.FastMax(StringParsers.ParseFloat(xelement2.GetAttribute("holster"), 0, -1, NumberStyles.Any), num5);
					}
					float num6 = Constants.cMinUnHolsterTime;
					if (xelement2.HasAttribute("unholster"))
					{
						num6 = Utils.FastMax(StringParsers.ParseFloat(xelement2.GetAttribute("unholster"), 0, -1, NumberStyles.Any), num6);
					}
					Vector3 vector = Vector3.zero;
					if (xelement2.HasAttribute("third_person_position"))
					{
						vector = StringParsers.ParseVector3(xelement2.GetAttribute("third_person_position"), 0, -1);
					}
					Vector3 vector2 = Vector3.zero;
					if (xelement2.HasAttribute("third_person_rotation"))
					{
						vector2 = StringParsers.ParseVector3(xelement2.GetAttribute("third_person_rotation"), 0, -1);
					}
					bool flag = false;
					if (xelement2.HasAttribute("two_handed"))
					{
						flag = StringParsers.ParseBool(xelement2.GetAttribute("two_handed"), 0, -1, true);
					}
					AnimationDelayData.AnimationDelay[num] = new AnimationDelayData.AnimationDelays(num2, num3, num4, num5, num6, flag);
					AnimationGunjointOffsetData.AnimationGunjointOffset[num] = new AnimationGunjointOffsetData.AnimationGunjointOffsets(vector, vector2);
				}
			}
		}
		int num7 = 0;
		int num8 = 0;
		foreach (XElement xelement3 in root.Elements("trigger_effects"))
		{
			foreach (XElement xelement4 in xelement3.Elements("trigger_effect"))
			{
				if (xelement4.HasAttribute("type_ds"))
				{
					num7++;
				}
				else if (xelement4.HasAttribute("type_xb"))
				{
					num8++;
				}
			}
		}
		TriggerEffectManager.ControllerTriggerEffectsDS.Clear();
		TriggerEffectManager.ControllerTriggerEffectsXb.Clear();
		TriggerEffectManager.ControllerTriggerEffectsDS.EnsureCapacity(num7);
		TriggerEffectManager.ControllerTriggerEffectsXb.EnsureCapacity(num8);
		using (IEnumerator<XElement> enumerator = root.Elements("trigger_effects").GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				XElement xelement5 = enumerator.Current;
				foreach (XElement xelement6 in xelement5.Elements("trigger_effect"))
				{
					if (!xelement6.HasAttribute("name"))
					{
						Debug.LogError("Every Trigger effect requires a name attribute set to a unique value");
					}
					else
					{
						string attribute = xelement6.GetAttribute("name");
						string text2;
						string text3;
						if (xelement6.TryGetAttribute("type_ds", out text2))
						{
							byte[] array;
							TriggerEffectManager.EffectDualsense effectDualsense;
							byte b;
							byte b2;
							byte b3;
							byte b4;
							byte b5;
							if (text2.ContainsCaseInsensitive("Weapon"))
							{
								if (!TriggerEffectDualsenseParsers.ParseWeaponEffects(text2, xelement6, attribute, out array, out effectDualsense, out b, out b2, out b3, out b4, out b5))
								{
									continue;
								}
							}
							else if (text2.ContainsCaseInsensitive("Feedback"))
							{
								if (text2.ContainsCaseInsensitive("MultipointFeedback") || text2.ContainsCaseInsensitive("FeedbackMultipoint"))
								{
									b2 = 0;
									b3 = 0;
									b4 = 0;
									b5 = 0;
									b = 0;
									effectDualsense = TriggerEffectManager.EffectDualsense.FeedbackMultipoint;
									if (!TriggerEffectDualsenseParsers.ParseEffectStrengths(text2, xelement6, attribute, out array))
									{
										continue;
									}
								}
								else if (text2.ContainsCaseInsensitive("SlopeFeedback") || text2.ContainsCaseInsensitive("FeedbackSlope"))
								{
									array = null;
									b = 0;
									effectDualsense = TriggerEffectManager.EffectDualsense.FeedbackSlope;
									if (!TriggerEffectDualsenseParsers.ParseStartEndPosition(text2, xelement6, attribute, out b3, out b4))
									{
										continue;
									}
									if (!TriggerEffectDualsenseParsers.ParseStartEndStrengths(text2, xelement6, attribute, out b2, out b5))
									{
										continue;
									}
								}
								else
								{
									b2 = 0;
									array = null;
									b4 = 0;
									b5 = 0;
									b = 0;
									effectDualsense = TriggerEffectManager.EffectDualsense.FeedbackSingle;
									if (!TriggerEffectDualsenseParsers.ParsePosition(text2, xelement6, attribute, out b3))
									{
										continue;
									}
									if (!TriggerEffectDualsenseParsers.ParseStrength(text2, xelement6, attribute, out b2))
									{
										continue;
									}
								}
							}
							else if (text2.ContainsCaseInsensitive("Vibration"))
							{
								if (text2.ContainsCaseInsensitive("Multipoint"))
								{
									b2 = 0;
									b5 = 0;
									b = 0;
									b3 = 0;
									b4 = 0;
									effectDualsense = TriggerEffectManager.EffectDualsense.VibrationMultipoint;
									if (!TriggerEffectDualsenseParsers.ParseEffectStrengths(text2, xelement6, attribute, out array))
									{
										continue;
									}
								}
								else
								{
									if (text2.ContainsCaseInsensitive("Slope"))
									{
										Debug.LogWarning("Trigger effectType: " + text2 + "(type_ds) is not implemented");
										continue;
									}
									b4 = 0;
									b2 = 0;
									array = null;
									effectDualsense = TriggerEffectManager.EffectDualsense.VibrationSingle;
									if (!TriggerEffectDualsenseParsers.ParsePosition(text2, xelement6, attribute, out b3) || !TriggerEffectDualsenseParsers.ParseAmplitude(text2, xelement6, attribute, out b5))
									{
										continue;
									}
									if (!TriggerEffectDualsenseParsers.ParseFrequency(text2, xelement6, attribute, out b))
									{
										continue;
									}
								}
							}
							else
							{
								if (text2.ContainsCaseInsensitive("NoEffect"))
								{
									Debug.LogError("Trigger effectType cannot be redefined: " + attribute + ":NoEffect");
									continue;
								}
								Debug.LogError("Trigger effectType Not supported: " + attribute + ":" + text2);
								continue;
							}
							if (!TriggerEffectManager.ControllerTriggerEffectsDS.TryAdd(attribute, new TriggerEffectManager.TriggerEffectDS
							{
								Effect = effectDualsense,
								Frequency = b,
								Position = b3,
								EndPosition = b4,
								Strength = b2,
								AmplitudeEndStrength = b5,
								Strengths = array
							}))
							{
								Debug.LogError("Trigger effect defined multiply in misc.xml: " + attribute);
							}
						}
						else if (xelement6.TryGetAttribute("type_xb", out text3))
						{
							TriggerEffectManager.EffectXbox effectXbox;
							float num9;
							float num10;
							float num11;
							float num12;
							if (text3.ContainsCaseInsensitive("Feedback"))
							{
								if (text3.ContainsCaseInsensitive("SlopeFeedback") || text3.ContainsCaseInsensitive("FeedbackSlope"))
								{
									effectXbox = TriggerEffectManager.EffectXbox.FeedbackSlope;
									if (!TriggerEffectXboxParsers.ParseStartEndPosition(text3, xelement6, attribute, out num9, out num10))
									{
										continue;
									}
									if (!TriggerEffectXboxParsers.ParseStartEndStrength(text3, xelement6, attribute, out num11, out num12))
									{
										continue;
									}
								}
								else
								{
									num9 = 0f;
									num10 = 0f;
									num12 = 0f;
									effectXbox = TriggerEffectManager.EffectXbox.FeedbackSingle;
									if (!TriggerEffectXboxParsers.ParseStrength(text3, xelement6, attribute, out num11))
									{
										continue;
									}
								}
							}
							else if (text3.ContainsCaseInsensitive("Vibration"))
							{
								if (text3.ContainsCaseInsensitive("SlopeVibration") || text3.ContainsCaseInsensitive("VibrationSlope"))
								{
									effectXbox = TriggerEffectManager.EffectXbox.VibrationSlope;
									if (!TriggerEffectXboxParsers.ParseStartEndPosition(text3, xelement6, attribute, out num9, out num10))
									{
										continue;
									}
									if (!TriggerEffectXboxParsers.ParseStartEndStrength(text3, xelement6, attribute, out num11, out num12))
									{
										continue;
									}
								}
								else
								{
									num9 = 0f;
									num10 = 0f;
									num12 = 0f;
									effectXbox = TriggerEffectManager.EffectXbox.VibrationSingle;
									if (!TriggerEffectXboxParsers.ParseStrength(text3, xelement6, attribute, out num11))
									{
										continue;
									}
								}
							}
							else
							{
								if (text3.ContainsCaseInsensitive("NoEffect"))
								{
									Debug.LogError("Trigger effectType cannot be redefined: " + text3);
									continue;
								}
								Debug.LogError("Trigger effectType Not supported: " + text3);
								continue;
							}
							if (!TriggerEffectManager.ControllerTriggerEffectsXb.TryAdd(attribute, new TriggerEffectManager.TriggerEffectXB
							{
								Effect = effectXbox,
								StartPosition = num9,
								EndPosition = num10,
								Strength = num11,
								EndStrength = num12
							}))
							{
								Debug.LogError("Trigger effect defined multiply in misc.xml: " + attribute);
							}
						}
						else
						{
							Debug.LogError("Trigger effect needs an xb_type or a ds_type: " + attribute);
						}
					}
				}
			}
			yield break;
		}
		yield break;
	}
}
