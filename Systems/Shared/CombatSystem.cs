using RealTimeSurvivors.Components.Shared;
using RealTimeSurvivors.Data;

namespace RealTimeSurvivors.Systems.Shared
{
  /// <summary>
  /// Handles what happens when an Entity attacks another.
  /// </summary>
  public class CombatSystem
  {
    private EntityManager _entities;
    private GameStorage _gameStorage;

    public CombatSystem(EntityManager _entities, GameStorage _store) 
    {
      this._entities = _entities;
      this._gameStorage = _store;
    }

    public bool Attack(int attacker, int target, Attack attack)
    {
      Health h = _entities.Healths[target];
      float attackModifier = _entities.Friendly[attacker] ? _gameStorage.fDamage : _gameStorage.eDamage;
        
      h.Current -= attack.Damage * attackModifier;
      _entities.Healths[target] = h;

      attack.CurrentCooldown = 0;
      _entities.Attacks[attacker] = attack;

      if (h.Current <= 0)
      {
        _entities.DestroyEntity(target);

        attack.TargetId = -1;
        _entities.Attacks[attacker] = attack;

        return true;
      }

      return false;
    }

  }
  
}
