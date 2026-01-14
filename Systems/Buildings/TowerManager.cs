using Godot;
using RealTimeSurvivors.Components.Buildings;
using RealTimeSurvivors.Components.Shared;
using RealTimeSurvivors.Components.Units;
using RealTimeSurvivors.Systems.Shared;

namespace RealTimeSurvivors.Systems.Buildings
{
  public class TowerManager
  {
    private readonly EntityManager _entities;
    private readonly SpatialIndex _spatial;
    private readonly CombatSystem _combat;

    private readonly int TileSearchRange = 32;

    public TowerManager(EntityManager _entities, SpatialIndex _spatial, CombatSystem _combat)
    {
      this._entities = _entities;
      this._spatial = _spatial;
      this._combat = _combat;
    }

    public void HandleTower(int entity, float delta)
    {
      switch(_entities.States[entity])
      {
        case UnitState.Idle:
          UpdateIdle(entity);
          break;
        case UnitState.Attacking:
          UpdateAttack(entity, delta);
          break;
      }
    }

    /// <summary>
    /// Updates the Idle state for Towers.
    /// Checks for surrounding enemies before switching to attack state.
    /// </summary>
    /// <param name="e">Entity id</param>
    private void UpdateIdle(int e)
    {
      int target = _spatial.FindClosest(
        e,
        _entities.Positions[e],
        TileSearchRange,
        _entities
      );

      if(target != -1)
      {
        Attack a = _entities.Attacks[e];
        a.TargetId = target;
        _entities.Attacks[e] = a;
        _entities.States[e] = UnitState.Attacking;
      }
    }

    /// <summary>
    /// Updates the Attack state for Towers.
    /// Tries to attack the current Target or switch to Idle state if dead or out of range.
    /// </summary>
    /// <param name="e">Entity id</param>
    /// <param name="delta">Delta of time between frames</param>
    private void UpdateAttack(int e, float delta)
    {
      if (!_entities.hasAttack[e])
      {
        _entities.States[e] = UnitState.Idle;
        return;
      }

      Attack a = _entities.Attacks[e];
      int target = a.TargetId;

      if (!_entities.hasPosition[e]) // No position == functionally dead
      {
        a.TargetId = -1;
        _entities.Attacks[e] = a;
        _entities.States[e] = UnitState.Idle;
        return;
      }

      if (target < 0 || !_entities.hasPosition[target])
      {
        a.TargetId = -1;
        _entities.Attacks[e] = a;
        _entities.States[e] = UnitState.Idle;
        return;
      }

      Position self = _entities.Positions[e];
      Position enemy = _entities.Positions[target];

      if (_entities.Types[target] is BuildingType bType)
      {
        Rect2 bounds = BuildManager.GetBuildingWorldRect(enemy, bType);
        if (self.DistanceToRect(bounds) > a.Range)
        {
          _entities.States[e] = UnitState.Idle;
          return;
        }
      }
      else
      {
        if (self.DistanceTo(enemy) > a.Range)
        {
          _entities.States[e] = UnitState.Idle;
          return;
        }
      }

      if ((a.CurrentCooldown -= delta) <= 0f)
      {
        _combat.Attack(e, target, a);
        a.CurrentCooldown = a.Cooldown;
      }
      _entities.Attacks[e] = a;
    }

  }
}
