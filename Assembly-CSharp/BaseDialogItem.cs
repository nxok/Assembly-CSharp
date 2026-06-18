using System;

public class BaseDialogItem
{
	public virtual string HeaderName
	{
		get; [PublicizedFrom(EAccessModifier.Protected)]
		set;
	}

	public Dialog OwnerDialog { get; set; }

	public string ID;
}
