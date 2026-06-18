using System;
using System.Collections.Generic;
using MusicUtils.Enums;
using UniLinq;
using UnityEngine.Scripting;

namespace DynamicMusic
{
	[Preserve]
	public class ContentQueue
	{
		[Preserve]
		public bool IsReady
		{
			get
			{
				return true;
			}
		}

		public ContentQueue(SectionType _section, LayerType _layer)
		{
			this.section = _section;
			this.layer = _layer;
			this.count = (from e in Content.AllContent.OfType<LayeredContent>()
				where e.Section == this.section && e.Layer == this.layer
				select e).Count<LayeredContent>();
		}

		public LayeredContent Next()
		{
			if (this.queue.Count < this.count / 2)
			{
				(from e in Content.AllContent.OfType<LayeredContent>()
					where e.Section == this.section && e.Layer == this.layer && !this.queue.Contains(e)
					orderby ContentQueue.rng.RandomInt
					select e).ToList<LayeredContent>().ForEach(new Action<LayeredContent>(this.queue.Enqueue));
			}
			return this.queue.Dequeue();
		}

		public void Clear()
		{
			this.queue.Clear();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static GameRandom rng = GameRandomManager.Instance.CreateGameRandom();

		[PublicizedFrom(EAccessModifier.Private)]
		public SectionType section;

		[PublicizedFrom(EAccessModifier.Private)]
		public LayerType layer;

		[PublicizedFrom(EAccessModifier.Private)]
		public int count;

		[PublicizedFrom(EAccessModifier.Private)]
		public Queue<LayeredContent> queue = new Queue<LayeredContent>();
	}
}
