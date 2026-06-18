using System;
using MusicUtils.Enums;

namespace DynamicMusic
{
	public class LayerState : ICountable
	{
		public int Count
		{
			get
			{
				return 1;
			}
		}

		public LayerState(Func<float, LayerStateType> _getFunc)
		{
			this.Get = _getFunc;
		}

		public readonly Func<float, LayerStateType> Get;
	}
}
