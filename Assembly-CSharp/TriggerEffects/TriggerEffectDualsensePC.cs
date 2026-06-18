using System;
using UnityEngine;

namespace TriggerEffects
{
	public static class TriggerEffectDualsensePC
	{
		public static TriggerEffectDualsensePC.APIState _apiState
		{
			[PublicizedFrom(EAccessModifier.Private)]
			get;
			[PublicizedFrom(EAccessModifier.Private)]
			set;
		}

		[PublicizedFrom(EAccessModifier.Internal)]
		public static void SetTriggerEffectVibration(int userID, TriggerEffectManager.GamepadTrigger trigger, byte position, byte amplitude, byte frequency)
		{
			if ((Application.platform == 7 || Application.platform == 2) && TriggerEffectDualsensePC._apiState == TriggerEffectDualsensePC.APIState.OK)
			{
				ControllerExt.ControllerExtSetTriggerEffectVibration(userID, (int)trigger, position, amplitude, frequency);
			}
		}

		[PublicizedFrom(EAccessModifier.Internal)]
		public static void ResetControllerIdentification()
		{
			if ((Application.platform == 7 || Application.platform == 2) && TriggerEffectDualsensePC._apiState == TriggerEffectDualsensePC.APIState.OK)
			{
				ControllerExt.ControllerExtResetLightbar(1);
				ControllerExt.ControllerExtResetLightbar(2);
				ControllerExt.ControllerExtResetLightbar(3);
				ControllerExt.ControllerExtResetLightbar(4);
			}
		}

		[PublicizedFrom(EAccessModifier.Internal)]
		public static void SetControllerIdentification()
		{
			if ((Application.platform == 7 || Application.platform == 2) && TriggerEffectDualsensePC._apiState == TriggerEffectDualsensePC.APIState.OK)
			{
				ControllerExt.ControllerExtSetLightbar(1, 180, 180, 0);
				ControllerExt.ControllerExtSetLightbar(2, 80, 80, 0);
				ControllerExt.ControllerExtSetLightbar(3, 0, 180, 80);
				ControllerExt.ControllerExtSetLightbar(4, 0, 80, 80);
			}
		}

		[PublicizedFrom(EAccessModifier.Internal)]
		public static void SetLightbar(int userId, byte colorR, byte colorG, byte colorB)
		{
			if ((Application.platform == 7 || Application.platform == 2) && TriggerEffectDualsensePC._apiState == TriggerEffectDualsensePC.APIState.OK)
			{
				ControllerExt.ControllerExtSetLightbar(userId, colorR, colorG, colorB);
			}
		}

		[PublicizedFrom(EAccessModifier.Internal)]
		public static void SetWeaponEffect(int userID, TriggerEffectManager.GamepadTrigger trigger, byte startPosition, byte endPosition, byte strength)
		{
			if ((Application.platform == 7 || Application.platform == 2) && TriggerEffectDualsensePC._apiState == TriggerEffectDualsensePC.APIState.OK)
			{
				ControllerExt.ControllerExtSetTriggerEffectWeapon(userID, (int)trigger, startPosition, endPosition, strength);
			}
		}

		[PublicizedFrom(EAccessModifier.Internal)]
		public static void SetTriggerEffectVibrationMultiplePosition(int userID, TriggerEffectManager.GamepadTrigger trigger, byte[] amplitudes, byte frequency)
		{
			if ((Application.platform == 7 || Application.platform == 2) && TriggerEffectDualsensePC._apiState == TriggerEffectDualsensePC.APIState.OK)
			{
				ControllerExt.ControllerExtSetTriggerEffectMultiVibration(userID, (int)trigger, amplitudes, amplitudes.Length, frequency);
			}
		}

		[PublicizedFrom(EAccessModifier.Internal)]
		public static void LibShutdown()
		{
			try
			{
				if ((Application.platform == 7 || Application.platform == 2) && TriggerEffectDualsensePC._apiState == TriggerEffectDualsensePC.APIState.OK)
				{
					for (int i = 1; i < 5; i++)
					{
						ControllerExt.ControllerExtResetLightbar(i);
						ControllerExt.ControllerExtSetTriggerEffectOff(i, 0);
						ControllerExt.ControllerExtSetTriggerEffectOff(i, 1);
						if (!ControllerExt.ControllerExtClosePad(i))
						{
							Log.Warning(string.Format("ControllerExtClosePad failed for user {0}", i));
						}
					}
					if (!ControllerExt.ControllerExtShutdown())
					{
						Log.Warning("ControllerExtShutdown failed");
					}
					Application.quitting -= TriggerEffectDualsensePC.LibShutdown;
					TriggerEffectDualsensePC._apiState = TriggerEffectDualsensePC.APIState.UnInit;
				}
			}
			catch (Exception ex)
			{
				Log.Warning(string.Format("[ControllerExt] Shutdown failed {0}", ex));
			}
		}

		[PublicizedFrom(EAccessModifier.Internal)]
		public static void InitTriggerEffectManager(ref bool[] controllersConnected)
		{
			if (Application.platform == 7 || (Application.platform == 2 && TriggerEffectDualsensePC._apiState == TriggerEffectDualsensePC.APIState.UnInit))
			{
				Application.quitting += TriggerEffectDualsensePC.LibShutdown;
				try
				{
					if (!ControllerExt.ControllerExtInit() && Application.platform == 7)
					{
						string text;
						if (ControllerExt.GetErrorString(ref text))
						{
							Log.Warning("TriggerEffectManager: ControllerExtInit failed: " + text);
						}
						ControllerExt.ControllerExtUpdate();
						for (int i = 1; i < 5; i++)
						{
							ControllerExt.ControllerExtClosePad(i);
							controllersConnected[i - 1] = false;
						}
					}
					else
					{
						ControllerExt.ControllerExtUpdate();
						for (int j = 1; j < 5; j++)
						{
							if (ControllerExt.ControllerExtOpenPad(j))
							{
								controllersConnected[j - 1] = true;
							}
						}
						ControllerExt.ControllerExtUpdate();
						TriggerEffectDualsensePC._apiState = TriggerEffectDualsensePC.APIState.OK;
					}
				}
				catch (Exception ex)
				{
					Log.Error(string.Format("[ControllerExt] Failed to load Library, disabiling: {0}", ex));
				}
			}
		}

		[PublicizedFrom(EAccessModifier.Internal)]
		public static void PCTriggerUpdate(bool stateChanged, TriggerEffectManager.TriggerEffectDS currentEffectLeft, TriggerEffectManager.TriggerEffectDS currentEffectRight)
		{
			if ((Application.platform == 7 || Application.platform == 2) && TriggerEffectDualsensePC._apiState == TriggerEffectDualsensePC.APIState.OK)
			{
				for (int i = 1; i < 5; i++)
				{
					if (GameManager.Instance.IsPaused())
					{
						TriggerEffectDualsensePC.ApplyEffectDualsenseOnPC(i, TriggerEffectManager.GamepadTrigger.LeftTrigger, TriggerEffectManager.NoneEffectDs);
						TriggerEffectDualsensePC.ApplyEffectDualsenseOnPC(i, TriggerEffectManager.GamepadTrigger.RightTrigger, TriggerEffectManager.NoneEffectDs);
					}
					else
					{
						string text;
						if (!TriggerEffectDualsensePC.ApplyEffectDualsenseOnPC(i, TriggerEffectManager.GamepadTrigger.LeftTrigger, currentEffectLeft) && stateChanged && ControllerExt.GetErrorString(ref text))
						{
							Log.Warning(string.Format("Controller {0} ControllerExtTriggerEffectApply returned false ({1})", i, text));
						}
						string text2;
						if (!TriggerEffectDualsensePC.ApplyEffectDualsenseOnPC(i, TriggerEffectManager.GamepadTrigger.RightTrigger, currentEffectRight) && stateChanged && ControllerExt.GetErrorString(ref text2))
						{
							Log.Warning(string.Format("Controller {0} ControllerExtTriggerEffectApply returned false ({1})", i, text2));
						}
					}
				}
			}
		}

		[PublicizedFrom(EAccessModifier.Internal)]
		public static void PCConnectedUpdate(ref bool[] controllersConnected, TriggerEffectManager.TriggerEffectDS currentEffectLeft, TriggerEffectManager.TriggerEffectDS currentEffectRight)
		{
			if ((Application.platform == 7 || Application.platform == 2) && TriggerEffectDualsensePC._apiState == TriggerEffectDualsensePC.APIState.OK)
			{
				ControllerExt.ControllerExtUpdate();
				for (int i = 1; i < 5; i++)
				{
					if (controllersConnected[i - 1] && !ControllerExt.ControllerExtIsConnected(i))
					{
						ControllerExt.ControllerExtClosePad(i);
						controllersConnected[i - 1] = false;
					}
					else if (!controllersConnected[i - 1] && ControllerExt.ControllerExtOpenPad(i))
					{
						controllersConnected[i - 1] = true;
						if (GameManager.Instance.IsPaused())
						{
							TriggerEffectDualsensePC.ApplyEffectDualsenseOnPC(i, TriggerEffectManager.GamepadTrigger.LeftTrigger, TriggerEffectManager.NoneEffectDs);
							TriggerEffectDualsensePC.ApplyEffectDualsenseOnPC(i, TriggerEffectManager.GamepadTrigger.RightTrigger, TriggerEffectManager.NoneEffectDs);
						}
						else
						{
							TriggerEffectDualsensePC.ApplyEffectDualsenseOnPC(i, TriggerEffectManager.GamepadTrigger.LeftTrigger, currentEffectLeft);
							TriggerEffectDualsensePC.ApplyEffectDualsenseOnPC(i, TriggerEffectManager.GamepadTrigger.RightTrigger, currentEffectRight);
						}
					}
				}
			}
		}

		[PublicizedFrom(EAccessModifier.Internal)]
		public static bool ApplyEffectDualsenseOnPC(int userId, TriggerEffectManager.GamepadTrigger trigger, TriggerEffectManager.TriggerEffectDS effect)
		{
			bool flag = true;
			if ((Application.platform == 7 || Application.platform == 2) && TriggerEffectDualsensePC._apiState == TriggerEffectDualsensePC.APIState.OK)
			{
				switch (effect.Effect)
				{
				case TriggerEffectManager.EffectDualsense.Off:
					flag = ControllerExt.ControllerExtSetTriggerEffectOff(userId, (int)trigger);
					break;
				case TriggerEffectManager.EffectDualsense.WeaponSingle:
					flag = ControllerExt.ControllerExtSetTriggerEffectWeapon(userId, (int)trigger, effect.Position, effect.EndPosition, effect.Strength);
					break;
				case TriggerEffectManager.EffectDualsense.WeaponMultipoint:
				case TriggerEffectManager.EffectDualsense.VibrationSlope:
					throw new NotSupportedException();
				case TriggerEffectManager.EffectDualsense.FeedbackSingle:
					flag = ControllerExt.ControllerExtSetTriggerEffectFeedback(userId, (int)trigger, effect.Position, effect.Strength);
					break;
				case TriggerEffectManager.EffectDualsense.VibrationSingle:
					flag = ControllerExt.ControllerExtSetTriggerEffectVibration(userId, (int)trigger, effect.Position, effect.AmplitudeEndStrength, effect.Frequency);
					break;
				case TriggerEffectManager.EffectDualsense.FeedbackSlope:
					flag = ControllerExt.ControllerExtSetTriggerEffectSlopeFeedback(userId, (int)trigger, effect.Position, effect.EndPosition, effect.Strength, effect.AmplitudeEndStrength);
					break;
				case TriggerEffectManager.EffectDualsense.FeedbackMultipoint:
					flag = ControllerExt.ControllerExtSetTriggerEffectMultiFeedback(userId, (int)trigger, effect.Strengths, effect.Strengths.Length);
					break;
				case TriggerEffectManager.EffectDualsense.VibrationMultipoint:
					flag = ControllerExt.ControllerExtSetTriggerEffectMultiVibration(userId, (int)trigger, effect.Strengths, effect.Strengths.Length, effect.Frequency);
					break;
				default:
					throw new ArgumentOutOfRangeException();
				}
			}
			return flag;
		}

		[PublicizedFrom(EAccessModifier.Internal)]
		public static void ApplyImmediate(TriggerEffectManager.GamepadTrigger trigger, TriggerEffectManager.ControllerTriggerEffect currentEffect)
		{
			if ((Application.platform == 7 || Application.platform == 2) && TriggerEffectDualsensePC._apiState == TriggerEffectDualsensePC.APIState.OK)
			{
				for (int i = 1; i < 5; i++)
				{
					TriggerEffectDualsensePC.ApplyEffectDualsenseOnPC(i, trigger, currentEffect.DualsenseEffect);
				}
			}
		}

		[PublicizedFrom(EAccessModifier.Internal)]
		public static void DualsensePCSetEffectToOff()
		{
			if ((Application.platform == 7 || Application.platform == 2) && TriggerEffectDualsensePC._apiState == TriggerEffectDualsensePC.APIState.OK)
			{
				for (int i = 1; i < 5; i++)
				{
					string text;
					if (!ControllerExt.ControllerExtSetTriggerEffectOff(i, 0) && ControllerExt.GetErrorString(ref text))
					{
						Log.Warning(string.Format("Controller {0} ControllerExtTriggerEffectApply returned false {1}", i, text));
					}
					if (!ControllerExt.ControllerExtSetTriggerEffectOff(i, 1) && ControllerExt.GetErrorString(ref text))
					{
						Log.Warning(string.Format("Controller {0} ControllerExtTriggerEffectApply returned false {1}", i, text));
					}
				}
			}
		}

		[PublicizedFrom(EAccessModifier.Internal)]
		public static void EnableVibration()
		{
			if ((Application.platform == 7 || Application.platform == 2) && TriggerEffectDualsensePC._apiState == TriggerEffectDualsensePC.APIState.OK)
			{
				for (int i = 1; i < 5; i++)
				{
					ControllerExt.ControllerExtEnableCompatibleVibration(i);
				}
			}
		}

		[PublicizedFrom(EAccessModifier.Internal)]
		public static void SetDualSenseVibration(byte _smallMotor, byte _largeMotor)
		{
			if ((Application.platform == 7 || Application.platform == 2) && TriggerEffectDualsensePC._apiState == TriggerEffectDualsensePC.APIState.OK)
			{
				for (int i = 1; i < 5; i++)
				{
					ControllerExt.ControllerExtSetMotorValue(i, _smallMotor, _largeMotor);
				}
			}
		}

		public enum APIState
		{
			UnInit,
			OK,
			Error
		}
	}
}
