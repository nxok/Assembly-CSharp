using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.XGamingRuntime;
using Unity.XGamingRuntime.Interop;

namespace Platform.XBL
{
	public static class XblHelpers
	{
		public static event XblHelpers.ErrorDelegate OnError;

		public static bool Succeeded(int _hresult, string _operationFriendlyName, bool _logToConsole = true, bool _printSuccess = false)
		{
			if (HR.SUCCEEDED(_hresult))
			{
				if (_printSuccess && _logToConsole)
				{
					Log.Out("[XBL] Success: " + _operationFriendlyName);
				}
				return true;
			}
			string text;
			if (!XblHelpers.hresultToFriendlyErrorLookup.TryGetValue(_hresult, out text))
			{
				text = _operationFriendlyName + " failed. Error code: " + XblHelpers.GetHRName(_hresult);
			}
			if (_logToConsole)
			{
				Log.Error(string.Format("[XBL] Error: 0x{0:X8} - {1}", _hresult, text));
			}
			XblHelpers.ErrorDelegate onError = XblHelpers.OnError;
			if (onError != null)
			{
				onError(_hresult, _operationFriendlyName, text);
			}
			return false;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		static XblHelpers()
		{
			XblHelpers.hresultToFriendlyErrorLookup[-2143330041] = "IAP_UNEXPECTED: Does the player you are signed in as have a license for the game? You can get one by downloading your game from the store and purchasing it first. If you can't find your game in the store, have you published it in Partner Center?";
			XblHelpers.hresultToFriendlyErrorLookup[-2015035361] = "Missing Game Config";
		}

		public static string GetHRName(int hr)
		{
			string text;
			if (!XblHelpers.s_hrToName.TryGetValue(hr, out text))
			{
				return "UNKNOWN";
			}
			return text;
		}

		public static void LogHR(int hr, string identifier, bool failWarn = false)
		{
			bool flag = HR.SUCCEEDED(hr);
			string text = (flag ? "SUCCEEDED" : "FAILED");
			string text2 = string.Format("[HResult] {0} (0x{1:X8} = {2}) {3}", new object[]
			{
				text,
				hr,
				XblHelpers.GetHRName(hr),
				identifier
			});
			if (flag)
			{
				Log.Out(text2);
				return;
			}
			if (failWarn)
			{
				Log.Warning(text2);
				return;
			}
			Log.Error(text2);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly Dictionary<int, string> hresultToFriendlyErrorLookup = new Dictionary<int, string>();

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly IReadOnlyDictionary<int, string> s_hrToName = (from f in new Type[]
			{
				typeof(HR),
				typeof(HR),
				typeof(HREx)
			}.SelectMany((Type t) => t.GetFields(BindingFlags.Static | BindingFlags.Public))
			where f.IsLiteral && !f.IsInitOnly && f.FieldType == typeof(int)
			select f).ToDictionary((FieldInfo f) => (int)f.GetValue(null), (FieldInfo f) => f.Name);

		public delegate void ErrorDelegate(int _hresult, string _operationFriendlyName, string _errorMessage);
	}
}
