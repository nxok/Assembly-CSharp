using System;
using System.Collections.Generic;

namespace GameEvent.GameEventHelpers
{
	public class HomerunManager
	{
		public void Cleanup()
		{
			for (int i = 0; i < this.HomerunDataList.Count; i++)
			{
				this.HomerunDataList.list[i].Cleanup();
			}
			this.HomerunDataList.Clear();
		}

		public void Update(float deltaTime)
		{
			for (int i = this.HomerunDataList.Count - 1; i >= 0; i--)
			{
				if (!this.HomerunDataList.list[i].Update(deltaTime))
				{
					HomerunData homerunData = this.HomerunDataList.list[i];
					homerunData.CompleteCallback();
					this.HomerunDataList.Remove(homerunData.Player);
					homerunData.Cleanup();
				}
			}
		}

		public void AddPlayerToHomerun(EntityPlayer player, List<int> rewardLevels, List<string> rewardEvents, float gameTime, Action completeCallback)
		{
			if (!this.HomerunDataList.dict.ContainsKey(player))
			{
				this.HomerunDataList.Add(player, new HomerunData(player, gameTime, "twitch_homerungoal_red,twitch_homerungoal_blue,twitch_homerungoal_green", rewardLevels, rewardEvents, this, completeCallback));
			}
		}

		public bool HasHomerunActive(EntityPlayer player)
		{
			return this.HomerunDataList.dict.ContainsKey(player);
		}

		public DictionaryList<EntityPlayer, HomerunData> HomerunDataList = new DictionaryList<EntityPlayer, HomerunData>();
	}
}
