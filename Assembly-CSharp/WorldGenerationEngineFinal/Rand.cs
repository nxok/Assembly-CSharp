using System;
using UnityEngine;

namespace WorldGenerationEngineFinal
{
	public class Rand
	{
		public static Rand Instance
		{
			get
			{
				if (Rand.instance == null)
				{
					Rand.instance = new Rand();
				}
				return Rand.instance;
			}
		}

		public Rand()
		{
			this.gameRandom = GameRandomManager.Instance.CreateGameRandom();
		}

		public Rand(int seed)
		{
			this.gameRandom = GameRandomManager.Instance.CreateGameRandom();
			this.SetSeed(seed);
		}

		public void Cleanup()
		{
			GameRandomManager.Instance.FreeGameRandom(this.gameRandom);
			Rand.instance = null;
		}

		public void Free()
		{
			GameRandomManager.Instance.FreeGameRandom(this.gameRandom);
		}

		public void SetSeed(int seed)
		{
			this.gameRandom.SetSeed(seed);
		}

		public float Float()
		{
			return this.gameRandom.RandomFloat;
		}

		public int Int()
		{
			return this.gameRandom.RandomInt;
		}

		public int Range(int min, int max)
		{
			return this.gameRandom.RandomRange(min, max);
		}

		public int Range(int max)
		{
			return this.gameRandom.RandomRange(max);
		}

		public float Range(float min, float max)
		{
			return this.gameRandom.RandomRange(min, max);
		}

		public int Angle()
		{
			return this.gameRandom.RandomRange(360);
		}

		public Vector2 RandomOnUnitCircle()
		{
			return this.gameRandom.RandomOnUnitCircle;
		}

		public int PeekSample()
		{
			return this.gameRandom.PeekSample();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static Rand instance;

		public GameRandom gameRandom;
	}
}
