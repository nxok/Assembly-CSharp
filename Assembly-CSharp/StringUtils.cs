using System;
using System.Globalization;

public static class StringUtils
{
	[PublicizedFrom(EAccessModifier.Private)]
	static StringUtils()
	{
		Localization.LanguageSelected += StringUtils.OnLanguageSelected;
		StringUtils.OnLanguageSelected(Localization.RequestedLanguage);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void OnLanguageSelected(string _lang)
	{
		string text = Localization.Get("cultureInfoName", false, null);
		if (string.IsNullOrEmpty(text))
		{
			Log.Warning("No culture info name given for selected language: " + _lang);
			return;
		}
		TextInfo textInfo;
		try
		{
			textInfo = CultureInfo.GetCultureInfo(text).TextInfo;
		}
		catch (Exception)
		{
			Log.Warning(string.Concat(new string[] { "No culture info found for given name: ", text, " (language: ", _lang, ")" }));
			return;
		}
		if (textInfo.CultureName != StringUtils.textInfo.CultureName)
		{
			StringUtils.textInfo = textInfo;
			Log.Out("Updated culture for display texts");
		}
	}

	public static string ToLowerWithUserLocale(this string _value)
	{
		return StringUtils.textInfo.ToLower(_value);
	}

	public static string ToUpperWithUserLocale(this string _value)
	{
		return StringUtils.textInfo.ToUpper(_value);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static TextInfo textInfo = Utils.StandardCulture.TextInfo;
}
