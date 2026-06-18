using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using MusicUtils;
using MusicUtils.Enums;
using UniLinq;
using UnityEngine.Scripting;

namespace DynamicMusic
{
	[Preserve]
	public abstract class LayeredContent : Content
	{
		public LayerType Layer
		{
			get; [PublicizedFrom(EAccessModifier.Private)]
			set;
		}

		public LayeredContent()
		{
			this.clips = new EnumDictionary<PlacementType, IClipAdapter>();
		}

		public abstract float GetSample(PlacementType _placement, int _idx, params float[] _params);

		public override bool IsLoaded
		{
			get
			{
				using (Dictionary<PlacementType, IClipAdapter>.ValueCollection.Enumerator enumerator = this.clips.Values.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						if (!enumerator.Current.IsLoaded)
						{
							return false;
						}
					}
				}
				return true;
			}
		}

		public override IEnumerator Load()
		{
			foreach (IClipAdapter clipAdapter in this.clips.Values)
			{
				yield return clipAdapter.Load();
			}
			Dictionary<PlacementType, IClipAdapter>.ValueCollection.Enumerator enumerator = default(Dictionary<PlacementType, IClipAdapter>.ValueCollection.Enumerator);
			yield break;
			yield break;
		}

		public void LoadImmediate()
		{
			foreach (IClipAdapter clipAdapter in this.clips.Values)
			{
				clipAdapter.LoadImmediate();
			}
		}

		public override void Unload()
		{
			foreach (IClipAdapter clipAdapter in this.clips.Values)
			{
				clipAdapter.Unload();
			}
		}

		public override void ParseFromXml(XElement _xmlNode)
		{
			base.ParseFromXml(_xmlNode);
			base.Section = EnumUtils.Parse<SectionType>(_xmlNode.Parent.Parent.GetAttribute("name"), false);
			this.Layer = EnumUtils.Parse<LayerType>(_xmlNode.Parent.GetAttribute("name"), false);
		}

		public void SetData(string _clipAdapterType, int _num, SectionType _section, LayerType _layer, bool loopOnly = false)
		{
			base.Name = _num.ToString("000") + DMSConstants.SectionAbbrvs[_section] + DMSConstants.LayerAbbrvs[_layer];
			base.Section = _section;
			this.Layer = _layer;
			this.AddClipAdapter(_clipAdapterType, _num, _section, _layer, PlacementType.Loop);
			if (!loopOnly)
			{
				this.AddClipAdapter(_clipAdapterType, _num, _section, _layer, PlacementType.Begin);
				this.AddClipAdapter(_clipAdapterType, _num, _section, _layer, PlacementType.End);
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void AddClipAdapter(string _clipAdapterType, int _num, SectionType _section, LayerType _layer, PlacementType _placement)
		{
			IClipAdapter clipAdapter = LayeredContent.CreateClipAdapter(_clipAdapterType);
			clipAdapter.SetPaths(_num, _placement, _section, _layer, "");
			this.clips.Add(_placement, clipAdapter);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public static IClipAdapter CreateClipAdapter(string _type)
		{
			return (IClipAdapter)Activator.CreateInstance(ReflectionHelpers.GetTypeWithPrefix("DynamicMusic.", _type));
		}

		public static T Get<T>(SectionType _section, LayerType _layer) where T : LayeredContent
		{
			(from e in Content.AllContent.OfType<T>()
				where e.Section == _section && e.Layer == _layer
				select e).ToList<T>();
			Tuple<SectionType, LayerType> tuple = new Tuple<SectionType, LayerType>(_section, _layer);
			ContentQueue contentQueue;
			if (LayeredContent.queueFor.TryGetValue(tuple, out contentQueue))
			{
				return (T)((object)contentQueue.Next());
			}
			Log.Warning(string.Format("there is no Content for {0}", tuple));
			return default(T);
		}

		public static void ReadyQueuesImmediate()
		{
			LayeredContent.queueFor.Clear();
			using (List<SectionType>.Enumerator enumerator = DMSConstants.LayeredSections.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					SectionType section = enumerator.Current;
					using (IEnumerator enumerator2 = Enum.GetValues(typeof(LayerType)).GetEnumerator())
					{
						while (enumerator2.MoveNext())
						{
							LayerType layer = (LayerType)enumerator2.Current;
							if (((ICollection<LayeredContent>)(from c in Content.AllContent.OfType<LayeredContent>()
								where c.Section == section && c.Layer == layer
								select c).ToList<LayeredContent>()).Count > 0)
							{
								ContentQueue contentQueue = new ContentQueue(section, layer);
								LayeredContent.queueFor.Add(new Tuple<SectionType, LayerType>(section, layer), contentQueue);
							}
						}
					}
				}
			}
		}

		public static void ClearQueues()
		{
			foreach (ContentQueue contentQueue in LayeredContent.queueFor.Values)
			{
				contentQueue.Clear();
			}
			LayeredContent.queueFor.Clear();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static Dictionary<Tuple<SectionType, LayerType>, ContentQueue> queueFor = new Dictionary<Tuple<SectionType, LayerType>, ContentQueue>();

		[PublicizedFrom(EAccessModifier.Protected)]
		public EnumDictionary<PlacementType, IClipAdapter> clips;
	}
}
