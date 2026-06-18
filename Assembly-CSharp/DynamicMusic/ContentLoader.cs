using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DynamicMusic
{
	public class ContentLoader
	{
		public static ContentLoader Instance
		{
			get
			{
				if (ContentLoader.instance == null)
				{
					ContentLoader.instance = new ContentLoader();
				}
				return ContentLoader.instance;
			}
		}

		public void Start()
		{
			this.LoadQueue = new Queue<IEnumerator>();
			this.Loader = GameManager.Instance.StartCoroutine(this.Load());
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public IEnumerator Load()
		{
			for (;;)
			{
				yield return new WaitUntil(() => this.LoadQueue.Count > 0);
				IEnumerator enumerator = this.LoadQueue.Dequeue();
				yield return enumerator;
			}
			yield break;
		}

		public void Cleanup()
		{
			if (this.Loader != null)
			{
				GameManager.Instance.StopCoroutine(this.Loader);
			}
			if (this.LoadQueue != null)
			{
				this.LoadQueue.Clear();
			}
			this.Loader = null;
			this.LoadQueue = null;
		}

		public static ContentLoader instance;

		[PublicizedFrom(EAccessModifier.Private)]
		public Coroutine Loader;

		public Queue<IEnumerator> LoadQueue;
	}
}
