using System;
using System.Collections.Generic;
using System.Xml.Linq;
using MusicUtils.Enums;
using UniLinq;

namespace DynamicMusic
{
	public abstract class AbstractConfiguration : IConfiguration
	{
		public virtual IList<SectionType> Sections
		{
			get; [PublicizedFrom(EAccessModifier.Protected)]
			set;
		}

		public abstract int CountFor(LayerType _layer);

		public AbstractConfiguration()
		{
			this.Sections = new List<SectionType>();
			AbstractConfiguration.AllConfigurations.Add(this);
		}

		public static AbstractConfiguration CreateWrapper(string _type)
		{
			return (AbstractConfiguration)Activator.CreateInstance(ReflectionHelpers.GetTypeWithPrefix("DynamicMusic.", _type));
		}

		public virtual void ParseFromXml(XElement _xmlNode)
		{
			foreach (string text in _xmlNode.GetAttribute("sections").Split(',', StringSplitOptions.None))
			{
				this.Sections.Add(EnumUtils.Parse<SectionType>(text, false));
			}
		}

		public static T Get<T>(SectionType _sectionType) where T : IConfiguration
		{
			List<T> list = AbstractConfiguration.AllConfigurations.OfType<T>().ToList<T>().FindAll((T c) => c.Sections.Contains(_sectionType));
			if (list.Count <= 0)
			{
				return default(T);
			}
			return list[AbstractConfiguration.rng.RandomRange(list.Count)];
		}

		public static int GetBufferSize(SectionType _sectionType, LayerType _layerType)
		{
			IEnumerable<int> enumerable = from c in AbstractConfiguration.AllConfigurations.OfType<IConfiguration>()
				where c.Sections.Contains(_sectionType)
				select c.CountFor(_layerType);
			if (enumerable != null && enumerable.Count<int>() != 0)
			{
				return enumerable.Max();
			}
			return 0;
		}

		public static IList<AbstractConfiguration> AllConfigurations = new List<AbstractConfiguration>();

		[PublicizedFrom(EAccessModifier.Protected)]
		public static GameRandom rng = GameRandomManager.Instance.CreateGameRandom();
	}
}
