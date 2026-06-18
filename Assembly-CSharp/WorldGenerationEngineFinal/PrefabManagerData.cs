using System;
using System.Collections.Generic;

namespace WorldGenerationEngineFinal
{
	public class PrefabManagerData
	{
		public void LoadPrefabs()
		{
			if (this.AllPrefabDatas.Count != 0)
			{
				return;
			}
			MicroStopwatch microStopwatch = new MicroStopwatch(true);
			List<PathAbstractions.AbstractedLocation> availablePathsList = PathAbstractions.PrefabsSearchPaths.GetAvailablePathsList(null, true, null);
			FastTags<TagGroup.Poi> fastTags = FastTags<TagGroup.Poi>.Parse("navonly,devonly,testonly,biomeonly");
			for (int i = 0; i < availablePathsList.Count; i++)
			{
				PathAbstractions.AbstractedLocation abstractedLocation = availablePathsList[i];
				int num = abstractedLocation.Folder.LastIndexOf("/Prefabs/");
				if (num < 0 || !abstractedLocation.Folder.Substring(num + 8, 5).EqualsCaseInsensitive("/test"))
				{
					PrefabData prefabData = PrefabData.LoadPrefabData(abstractedLocation);
					try
					{
						if (prefabData != null && !prefabData.Tags.Test_AnySet(fastTags) && !prefabData.Tags.IsEmpty)
						{
							this.AllPrefabDatas[abstractedLocation.Name.ToLower()] = prefabData;
						}
					}
					catch (Exception)
					{
						Log.Error("Could not load prefab data for " + abstractedLocation.Name);
					}
				}
			}
			Log.Out("LoadPrefabs {0} of {1} in {2} s", new object[]
			{
				this.AllPrefabDatas.Count,
				availablePathsList.Count,
				(float)microStopwatch.ElapsedMilliseconds * 0.001f
			});
		}

		public void ShufflePrefabData(int _seed)
		{
			this.prefabDataList.Clear();
			this.AllPrefabDatas.CopyValuesTo(this.prefabDataList);
			this.prefabDataList.Shuffle(_seed);
		}

		public void Cleanup()
		{
			this.AllPrefabDatas.Clear();
			this.prefabDataList.Clear();
		}

		public Prefab GetPreviewPrefabWithAnyTags(FastTags<TagGroup.Poi> _tags, int _townshipId, Vector2i size = default(Vector2i), bool useAnySizeSmaller = false)
		{
			Vector2i minSize = (useAnySizeSmaller ? Vector2i.zero : size);
			List<PrefabData> list = this.prefabDataList.FindAll((PrefabData _pd) => !_pd.Tags.Test_AnySet(this.PartsAndTilesTags) && PrefabManager.isSizeValid(_pd, minSize, size) && _pd.Tags.Test_AnySet(_tags));
			if (list.Count == 0)
			{
				return null;
			}
			list.Shuffle(this.previewSeed);
			Prefab prefab = new Prefab();
			prefab.Load(list[0].location, true, true, false, false);
			this.previewSeed++;
			return prefab;
		}

		public readonly Dictionary<string, PrefabData> AllPrefabDatas = new Dictionary<string, PrefabData>();

		public readonly List<PrefabData> prefabDataList = new List<PrefabData>();

		public readonly FastTags<TagGroup.Poi> PartsAndTilesTags = FastTags<TagGroup.Poi>.Parse("streettile,part");

		public readonly FastTags<TagGroup.Poi> WildernessTags = FastTags<TagGroup.Poi>.Parse("wilderness");

		public readonly FastTags<TagGroup.Poi> TraderTags = FastTags<TagGroup.Poi>.Parse("trader");

		public readonly FastTags<TagGroup.Poi> HasTags = FastTags<TagGroup.Poi>.Parse("has");

		[PublicizedFrom(EAccessModifier.Private)]
		public int previewSeed;
	}
}
