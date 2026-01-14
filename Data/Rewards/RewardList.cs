using System.Collections.Generic;

namespace RealTimeSurvivors.Data.Rewards
{
  /// <summary>
  /// Static Lists for getting Reward card definitions.
  /// </summary>
  public static class RewardList
  {
    public static readonly List<Reward> Cards = new List<Reward>()
    {
      new Reward(
          $"Heavy Bullets \n +10% Friendly Damage \n -10% Friendly Speed",
          new List<IMultiplier>()
          {
            new MultiplicationMultiplier(ModifierType.FriendlyDamage, 1.1f),
            new MultiplicationMultiplier(ModifierType.FriendlySpeed, 0.9f)
          }
        ),
      new Reward(
        $"Less Regulation \n -30% Build Time \n +20% Enemy Damage",
        new List<IMultiplier>()
        {
          new MultiplicationMultiplier(ModifierType.BuildTime, 0.7f),
          new MultiplicationMultiplier(ModifierType.EnemyDamage, 1.2f)
        }
      ),
      new Reward(
        $"Higher number good? \n +2 Build Time \n +5 Friendly Damage \n -5% Enemy Damage",
        new List<IMultiplier>()
        {
          new AdditionMultiplier(ModifierType.BuildTime, 2f),
          new AdditionMultiplier(ModifierType.FriendlyDamage, 5f),
          new MultiplicationMultiplier(ModifierType.EnemyDamage, 1.05f)
        }
      ),
      new Reward(
        $"Sabotage \n +40% Build Time \n +10'000r",
        new List<IMultiplier>()
        {
          new MultiplicationMultiplier(ModifierType.BuildTime, 1.4f),
          new AdditionMultiplier(ModifierType.Resources, 10000f)
        }
      ),
    };
  }
}
