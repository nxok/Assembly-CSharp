using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

public static class ListExtensions
{
	public static T[] GetInternalArray<T>(this List<T> list)
	{
		return ListExtensions.ArrayAccessor<T>.Getter(list);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static class ArrayAccessor<T>
	{
		[PublicizedFrom(EAccessModifier.Private)]
		static ArrayAccessor()
		{
			DynamicMethod dynamicMethod = new DynamicMethod("get", MethodAttributes.FamANDAssem | MethodAttributes.Family | MethodAttributes.Static, CallingConventions.Standard, typeof(T[]), new Type[] { typeof(List<T>) }, typeof(ListExtensions.ArrayAccessor<T>), true);
			ILGenerator ilgenerator = dynamicMethod.GetILGenerator();
			ilgenerator.Emit(OpCodes.Ldarg_0);
			ilgenerator.Emit(OpCodes.Ldfld, typeof(List<T>).GetField("_items", BindingFlags.Instance | BindingFlags.NonPublic));
			ilgenerator.Emit(OpCodes.Ret);
			ListExtensions.ArrayAccessor<T>.Getter = (Func<List<T>, T[]>)dynamicMethod.CreateDelegate(typeof(Func<List<T>, T[]>));
		}

		public static Func<List<T>, T[]> Getter;
	}
}
