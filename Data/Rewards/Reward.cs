using System.Collections.Generic;

namespace RealTimeSurvivors.Data.Rewards
{
  /// <summary>
  /// A reward is a multiple predefined modifiers that can change the GameStorage values.
  /// </summary>
  public class Reward
  {
    public string Description; // Flavour text for the reward.
    public List<IMultiplier> Modifiers;

    public Reward(string description, List<IMultiplier> modifiers)
    {
      Description = description; 
      Modifiers = modifiers;
    }

    public void Apply(ref GameStorage storage)
    {
      foreach (var mod in Modifiers)
      {
        ApplyModifier(ref storage, mod);
      }
    }

    private void ApplyModifier(ref GameStorage storage, IMultiplier mod)
    {
      switch(mod.Type)
      {
        case ModifierType.Resources:
          storage.Resources = mod.Apply(storage.Resources);
          break;
        case ModifierType.FriendlyDamage:
          storage.fDamage = mod.Apply(storage.fDamage);
          break;
        case ModifierType.FriendlySpeed:
          storage.fSpeed = mod.Apply(storage.fSpeed);
          break;
        case ModifierType.BuildTime:
          storage.buildTime = mod.Apply(storage.buildTime);
          break;
        case ModifierType.GenerateAmount:
          storage.generateAmount = mod.Apply(storage.generateAmount);
          break;
        case ModifierType.EnemyDamage:
          storage.eDamage = mod.Apply(storage.eDamage);
          break;
        case ModifierType.EnemySpeed:
          storage.eSpeed = mod.Apply(storage.eSpeed);
          break;
      }
    }
  }
}
