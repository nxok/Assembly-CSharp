using System;
using System.Collections.Generic;
using System.Linq;

namespace DynamicMusic.Legacy.ObjectModel
{
	public class Layer : Dictionary<int, InstrumentID>
	{
		public Layer()
		{
			if (Layer.Random == null)
			{
				Layer.Random = GameRandomManager.Instance.CreateGameRandom();
			}
		}

		public InstrumentID GetInstrumentID()
		{
			if (this.idQ == null || this.idQ.Count < 3)
			{
				this.PopulateQueue();
			}
			LayerReserve.AddLoad(this.idQ.ElementAt(1));
			return this.idQ.Dequeue();
		}

		public void PopulateQueue()
		{
			if (this.idQ == null)
			{
				this.idQ = new Queue<InstrumentID>(base.Values.OrderBy((InstrumentID e) => Layer.Random.RandomRange(int.MaxValue)));
				LayerReserve.AddLoad(this.idQ.Peek());
				return;
			}
			this.RefillQueue();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void RefillQueue()
		{
			Array.ForEach<InstrumentID>(base.Values.OrderBy(delegate(InstrumentID e)
			{
				if (e.Name.Equals(this.idQ.Peek().Name) || e.Name.Equals(this.idQ.ElementAt(1).Name))
				{
					return int.MaxValue;
				}
				return Layer.Random.RandomRange(int.MaxValue);
			}).ToArray<InstrumentID>(), new Action<InstrumentID>(this.idQ.Enqueue));
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public Queue<InstrumentID> idQ;

		[PublicizedFrom(EAccessModifier.Private)]
		public static GameRandom Random;
	}
}
