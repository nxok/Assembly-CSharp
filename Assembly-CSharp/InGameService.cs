using System;

public class InGameService
{
	public InGameService.InGameServiceTypes ServiceType { get; set; }

	public string Name { get; set; }

	public string Description { get; set; }

	public string Icon { get; set; }

	public int Price { get; set; }

	public Action<bool> VisibleChangedHandler;

	public enum InGameServiceTypes
	{
		VendingRent
	}
}
