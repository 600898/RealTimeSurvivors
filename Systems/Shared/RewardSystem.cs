using RealTimeSurvivors.Data.Rewards;
using System;
using System.Linq;

namespace RealTimeSurvivors.Systems.Shared
{
  /// <summary>
  /// Used to randomize given Rewards/Cards from the RewardList.
  /// </summary>
  public static class RewardSystem
  {

    public static Reward[] GetRandomRewards(int count)
    {
      Random r = new Random();
      Reward[] rewards = new Reward[count];
      for(int i = 0; i < count; i++)
      {
        rewards[i] = RewardList.Cards.ElementAt(r.Next(0, count));
      }
      return rewards;
    }
  }
}
