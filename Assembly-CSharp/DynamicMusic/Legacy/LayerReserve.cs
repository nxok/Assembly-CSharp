using System;
using System.Collections.Generic;
using DynamicMusic.Legacy.ObjectModel;

namespace DynamicMusic.Legacy
{
	public static class LayerReserve
	{
		public static void Tick()
		{
			LayerReserve.Load();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static void Load()
		{
			if (LayerReserve.CurrentLoading == null)
			{
				if (LayerReserve.toLoad.Count > 0)
				{
					LayerReserve.CurrentLoading = LayerReserve.toLoad.Dequeue();
					LayerReserve.CurrentLoading.Load();
				}
				return;
			}
			if (!LayerReserve.CurrentLoading.IsLoaded)
			{
				LayerReserve.CurrentLoading.Load();
				return;
			}
			if (LayerReserve.toLoad.Count > 0)
			{
				LayerReserve.CurrentLoading = LayerReserve.toLoad.Dequeue();
				LayerReserve.CurrentLoading.Load();
				return;
			}
			LayerReserve.CurrentLoading = null;
		}

		public static void AddLoad(InstrumentID _id)
		{
			if (LayerReserve.toLoad == null)
			{
				LayerReserve.toLoad = new Queue<InstrumentID>();
			}
			LayerReserve.toLoad.Enqueue(_id);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static Queue<InstrumentID> toLoad;

		[PublicizedFrom(EAccessModifier.Private)]
		public static InstrumentID CurrentLoading;
	}
}
