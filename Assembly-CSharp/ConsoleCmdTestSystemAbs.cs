using System;

public abstract class ConsoleCmdTestSystemAbs : ConsoleCmdAbstract
{
	public static void AssertAreApproximatelyEqual(long expected, long actual, long tolerance, string message)
	{
		Math.Abs(expected - actual);
	}

	public static void AssertArraysAreEqual<T>(T[] expected, T[] actual, string message)
	{
		for (int i = 0; i < expected.Length; i++)
		{
		}
	}

	public static bool AssertException(Action action)
	{
		bool flag;
		try
		{
			action();
			flag = false;
		}
		catch (Exception ex)
		{
			Log.Out(string.Format("Expected exception:\n{0}", ex));
			flag = true;
		}
		return flag;
	}

	public static bool ExecutesWithoutExceptions(params Action[] testCases)
	{
		bool flag = true;
		foreach (Action action in testCases)
		{
			flag &= ConsoleCmdTestSystemAbs.ExecutesWithoutException(action);
		}
		return flag;
	}

	public static bool ExecutesWithoutException(Action testCase)
	{
		bool flag = true;
		try
		{
			Log.Out(testCase.Method.Name ?? "");
			testCase();
		}
		catch (Exception ex)
		{
			Log.Exception(ex);
			flag = false;
		}
		finally
		{
			Log.Out(testCase.Method.Name + " result: " + (flag ? "<color=green>Success</color>" : "<color=red>Failure</color>") + ".");
		}
		return flag;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public ConsoleCmdTestSystemAbs()
	{
	}

	public const string SuccessString = "<color=green>Success</color>";

	public const string FailureString = "<color=red>Failure</color>";
}
