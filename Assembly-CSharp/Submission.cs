using System;

public static class Submission
{
	public static bool Enabled
	{
		get
		{
			if (!Submission.isSubmissionChecked)
			{
				string[] commandLineArgs = GameStartupHelper.GetCommandLineArgs();
				for (int i = 0; i < commandLineArgs.Length; i++)
				{
					if (commandLineArgs[i].EqualsCaseInsensitive(Constants.cArgSubmissionBuild))
					{
						Log.Out("Submission Enabled by argument");
						Submission.isSubmission = true;
					}
				}
				Submission.isSubmissionChecked = true;
			}
			return Submission.isSubmission;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool isSubmissionChecked;

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool isSubmission;
}
