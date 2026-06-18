using System;

public abstract class BaseObjectiveModifier
{
	public BaseObjective OwnerObjective { get; set; }

	public BaseObjectiveModifier()
	{
	}

	public void HandleAddHooks()
	{
		this.AddHooks();
	}

	public void HandleRemoveHooks()
	{
		this.RemoveHooks();
	}

	public virtual void AddHooks()
	{
	}

	public virtual void RemoveHooks()
	{
	}

	public virtual BaseObjectiveModifier Clone()
	{
		return null;
	}

	public virtual void ParseProperties(DynamicProperties properties)
	{
		this.Properties = properties;
		this.OwnerObjective.OwnerQuestClass.HandleVariablesForProperties(properties);
	}

	public DynamicProperties Properties;
}
