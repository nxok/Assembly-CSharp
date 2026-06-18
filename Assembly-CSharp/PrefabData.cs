using System;
using System.Collections.Generic;
using PrefabVolumes;

public class PrefabData
{
	public PrefabData(PathAbstractions.AbstractedLocation _location)
	{
		this.POIMarkers = new PrefabMarkerVolumeList(null);
		this.location = _location;
		this.Name = _location.Name.ToLower();
	}

	public bool Init(DynamicProperties properties)
	{
		this.size = new Vector3i(-1, -1, -1);
		properties.ParseVec("PrefabSize", ref this.size);
		if (this.size.x < 0 || this.size.y < 0 || this.size.z < 0)
		{
			Log.Error("PrefabData {0} PrefabSize error", new object[] { this.Name });
			return false;
		}
		this.POIMarkers.ReadFromProperties(properties);
		this.rotationToFaceNorth = (byte)properties.GetInt("RotationToFaceNorth");
		if (properties.Values.ContainsKey("Tags"))
		{
			this.Tags = FastTags<TagGroup.Poi>.Parse(properties.Values["Tags"].Replace(" ", ""));
		}
		if (properties.Values.ContainsKey("ThemeTags"))
		{
			this.ThemeTags = FastTags<TagGroup.Poi>.Parse(properties.Values["ThemeTags"].Replace(" ", ""));
		}
		properties.ParseInt("ThemeRepeatDistance", ref this.ThemeRepeatDistance);
		properties.ParseInt("DuplicateRepeatDistance", ref this.DuplicateRepeatDistance);
		if (properties.Classes.ContainsKey("Stats"))
		{
			WorldStats worldStats = WorldStats.FromProperties(properties.Classes["Stats"]);
			if (worldStats != null)
			{
				this.DensityScore = (float)((worldStats.TotalVertices + 50000) / 100000);
			}
		}
		this.yOffset = properties.GetInt("YOffset");
		properties.ParseInt("DifficultyTier", ref this.DifficultyTier);
		return true;
	}

	public List<Marker> RotatePOIMarkers(bool _bLeft, int _rotCount)
	{
		Vector3i vector3i = this.size;
		List<Marker> list = new List<Marker>(this.POIMarkers.Count);
		for (int i = 0; i < this.POIMarkers.Count; i++)
		{
			list.Add(this.POIMarkers[i].CloneGeneric());
		}
		for (int j = 0; j < _rotCount; j++)
		{
			for (int k = 0; k < list.Count; k++)
			{
				list[k].RotateY(_bLeft, vector3i);
			}
			MathUtils.Swap(ref vector3i.x, ref vector3i.z);
		}
		return list;
	}

	public static PrefabData LoadPrefabData(PathAbstractions.AbstractedLocation _location)
	{
		if (!SdFile.Exists(_location.FullPathNoExtension + ".xml"))
		{
			return null;
		}
		DynamicProperties dynamicProperties = new DynamicProperties();
		if (!dynamicProperties.Load(_location.Folder, _location.Name))
		{
			return null;
		}
		PrefabData prefabData = new PrefabData(_location);
		if (!prefabData.Init(dynamicProperties))
		{
			return null;
		}
		return prefabData;
	}

	public const int ThemeRepeatDistanceDefault = 300;

	public const int DuplicateRepeatDistanceDefault = 1000;

	public Vector3i size;

	public readonly string Name;

	public FastTags<TagGroup.Poi> Tags;

	public FastTags<TagGroup.Poi> ThemeTags;

	public int ThemeRepeatDistance = 300;

	public int DuplicateRepeatDistance = 1000;

	public byte rotationToFaceNorth;

	public float DensityScore;

	public int DifficultyTier;

	public int yOffset;

	public PathAbstractions.AbstractedLocation location;

	public readonly PrefabMarkerVolumeList POIMarkers;
}
