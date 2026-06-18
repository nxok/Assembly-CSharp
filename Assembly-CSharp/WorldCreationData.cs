using System;
using System.Xml.Linq;

public class WorldCreationData
{
	public WorldCreationData(string _levelDir)
	{
		try
		{
			XDocument xdocument = SdXDocument.Load(_levelDir + "/world.xml");
			if (xdocument.Root == null)
			{
				throw new Exception("No root node in world.xml!");
			}
			foreach (XElement xelement in xdocument.Root.Elements("property"))
			{
				this.Properties.Add(xelement, false);
			}
		}
		catch (Exception)
		{
		}
	}

	public void Apply(World _world, WorldState _worldState)
	{
		if (this.Properties.Values.ContainsKey("ProviderId"))
		{
			_worldState.providerId = (EnumChunkProviderId)int.Parse(this.Properties.Values["ProviderId"]);
		}
	}

	public const string PropProviderId = "ProviderId";

	public const string PropWorldEnvironment = "WorldEnvironment";

	public const string PropWorldEnvironment_Prefab = "Prefab";

	public const string PropWorldEnvironment_Class = "Class";

	public DynamicProperties Properties = new DynamicProperties();
}
