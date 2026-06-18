using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

public static class SignLibraryMigrations
{
	public static XElement Migrate(XElement root, int fromVersion, int toVersion)
	{
		if (toVersion != fromVersion + 1)
		{
			throw new ArgumentException(string.Format("Migrations must be sequential. Cannot migrate from v{0} to v{1}", fromVersion, toVersion));
		}
		SignLibraryMigrations.MigrationFunc migrationFunc;
		if (!SignLibraryMigrations.Migrations.TryGetValue(fromVersion, out migrationFunc))
		{
			throw new InvalidOperationException(string.Format("No migration registered for v{0} to v{1}", fromVersion, toVersion));
		}
		Log.Out(string.Format("  Applying migration: v{0} -> v{1}", fromVersion, toVersion));
		XElement xelement = migrationFunc(root);
		xelement.SetAttributeValue("version", toVersion);
		return xelement;
	}

	public static bool CanMigrate(int fromVersion, int toVersion)
	{
		for (int i = fromVersion; i < toVersion; i++)
		{
			if (!SignLibraryMigrations.Migrations.ContainsKey(i))
			{
				return false;
			}
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static XElement VisitLayers(XElement root, Action<XElement, string> visitor)
	{
		foreach (XElement xelement in root.Elements(XNames.sign))
		{
			SignLibraryMigrations.<VisitLayers>g__ProcessLayers|4_0(xelement.Elements("layer"), visitor);
		}
		return root;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static XElement MigrateV0ToV1(XElement root)
	{
		return root;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static XElement MigrateV1ToV2(XElement root)
	{
		return SignLibraryMigrations.VisitLayers(root, delegate(XElement layer, string layerType)
		{
			if (layerType == "TextSignLayer")
			{
				layer.SetAttributeValue("direction", 0);
				layer.SetAttributeValue("spacing", 1);
			}
		});
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Internal)]
	public static void <VisitLayers>g__ProcessLayers|4_0(IEnumerable<XElement> layers, Action<XElement, string> visitor)
	{
		foreach (XElement xelement in layers)
		{
			string attribute = xelement.GetAttribute("type");
			visitor(xelement, attribute);
			if (attribute == "GroupSignLayer")
			{
				SignLibraryMigrations.<VisitLayers>g__ProcessLayers|4_0(xelement.Elements("layer"), visitor);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Dictionary<int, SignLibraryMigrations.MigrationFunc> Migrations = new Dictionary<int, SignLibraryMigrations.MigrationFunc>
	{
		{
			0,
			new SignLibraryMigrations.MigrationFunc(SignLibraryMigrations.MigrateV0ToV1)
		},
		{
			1,
			new SignLibraryMigrations.MigrationFunc(SignLibraryMigrations.MigrateV1ToV2)
		}
	};

	public delegate XElement MigrationFunc(XElement root);
}
