using System;
using System.Reflection;
using UniLinq;

public static class EnumExtensions
{
	public static string GetDocumentation(this Enum enumValue)
	{
		MemberInfo memberInfo = enumValue.GetType().GetMember(enumValue.ToString()).FirstOrDefault<MemberInfo>();
		string text;
		if (memberInfo == null)
		{
			text = null;
		}
		else
		{
			Documentation customAttribute = memberInfo.GetCustomAttribute<Documentation>();
			text = ((customAttribute != null) ? customAttribute.Text : null);
		}
		return text ?? enumValue.ToString();
	}
}
