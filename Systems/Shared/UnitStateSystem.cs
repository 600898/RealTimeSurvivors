using Godot;
using RealTimeSurvivors.Components.Buildings;
using RealTimeSurvivors.Components.Shared;
using RealTimeSurvivors.Components.Units;
using RealTimeSurvivors.Systems.Buildings;
using RealTimeSurvivors.Systems.Shared;
using RealTimeSurvivors.Systems.Units;

namespace RealTimeSurvivors.Systems.Shared
{
  /// <summary>
  /// Handles AI of all entities inside the game.
  /// </summary>
  public class UnitStateSystem
  {
    private readonly EntityManager _entities;
    private readonly SpatialIndex _spatial;
    private readonly MovementSystem _movement;
    private readonly CombatSystem _combat;
    private readonly TowerManager _tower;

    private int TileSearchRange = 24; // SpatialIndex search size for target ID.

    public UnitStateSystem(EntityManager _entities, SpatialIndex _spatial, MovementSystem _movement, CombatSystem _combat)
    {
      this._entities = _entities;
      this._spatial = _spatial;
      this._movement = _movement;
      this._combat = _combat;
      _tower = new TowerManager(_entities, _spatial, _combat);
    }

    /// <summary>
    /// Updates an entity based on its current UnitState.
    /// </summary>
    /// <param name="entity">Entity id</param>
    /// <param name="delta">Delta of frames</param>
    public void HandleEntity(int entity, float delta)
    {
      if (_entities.Types[entity] is BuildingType.Tower)
      {
        _tower.HandleTower(entity, delta);
        return;
      }
      switch (_entities.States[entity])
      {
        case UnitState.Idle:
          UpdateIdle(entity);
          break;

        case UnitState.Moving:
          UpdateMoving(entity, delta);
          break;

        case UnitState.Chasing:
          UpdateChasing(entity, delta);
          break;

        case UnitState.Attacking:
          UpdateAttacking(entity, delta);
          break;
      }
    }

    // Updates the Idle for entities.
    private void UpdateIdle(int e)
    {
      if (_entities.CurrentTarget.TryGetValue(e, out Move move))
      {
        _entities.States[e] = UnitState.Moving;
        return;
      }

      int target = _spatial.FindClosest(
        e,
        _entities.Positions[e],
        TileSearchRange,
        _entities
      );

      if (target != -1)
      {
        Attack a = _entities.Attacks[e];
        a.TargetId = target;
        _entities.Attacks[e] = a;
        _entities.States[e] = UnitState.Chasing;
      }
      else if (_entities.Friendly[e] == false && _entities.hasPosition[0]) // Enemy Unit logic
      {
        _entities.CurrentTarget[e] = new Move(_entities.Positions[0]); // Move towards player base --First entity is always the Heart
        _entities.States[e] = UnitState.Moving;
      }

    }

    // Updates the the moving behaviour of an entity.
    private void UpdateMoving(int e, float delta)
    {
      bool arrived = _movement.MoveEntity(e, delta);

      if (!arrived)
        return;

      _entities.CurrentTarget.Remove(e);

      if (_entities.hasAttack[e] && _entities.Attacks[e].TargetId >= 0)
      {
        _entities.States[e] = UnitState.Chasing;
      }
      else
      {
        _entities.States[e] = UnitState.Idle;
      }
    }

    // Updates the Chasing logic for an entity out of attack range.
    public void UpdateChasing(int e, float delta)
    {
      Attack a = _entities.Attacks[e];
      int target = a.TargetId;

      if (target < 0 || !_entities.hasPosition[target])
      {
        a.TargetId = -1;
        _entities.Attacks[e] = a;
        _entities.States[e] = UnitState.Idle;
        return;
      }

      Position self = _entities.Positions[e];
      Position targetPos = _entities.Positions[target];

      // Determine distance to target (building vs unit)
      float distance = _entities.Types[target] is BuildingType bType
          ? self.DistanceToRect(BuildManager.GetBuildingWorldRect(targetPos, bType))
          : self.DistanceTo(targetPos);

      if (distance <= a.Range)
      {
        _entities.States[e] = UnitState.Attacking;
        return;
      }

      // Move closer if not in range
      Position approach = _movement.GetApproachPosition(e, target);
      if (!_entities.CurrentTarget.TryGetValue(e, out Move current) || current.position.DistanceTo(approach) > 1f)
      {
        _entities.CurrentTarget[e] = new Move(approach);
      }

      _movement.MoveEntity(e, delta);
    }

    // Handles the attack state of entities
    private void UpdateAttacking(int e, float delta)
    {
      Attack a = _entities.Attacks[e];
      int target = a.TargetId;

      if (target < 0 || !_entities.hasPosition[target])
      {
        a.TargetId = -1;
        _entities.Attacks[e] = a;
        _entities.States[e] = UnitState.Idle;
        return;
      }

      Position self = _entities.Positions[e];
      Position targetPos = _entities.Positions[target];

      // Determine distance to target (building vs unit)
      float distance = _entities.Types[target] is BuildingType bType
          ? self.DistanceToRect(BuildManager.GetBuildingWorldRect(targetPos, bType))
          : self.DistanceTo(targetPos);

      // Go back to chasing if target moved out of range
      if (distance > a.Range)
      {
        _entities.States[e] = UnitState.Chasing;
        return;
      }

      // Attack if cooldown allows
      if ((a.CurrentCooldown -= delta) <= 0f)
      {
        _combat.Attack(e, target, a);
        a.CurrentCooldown = a.Cooldown;
      }

      _entities.Attacks[e] = a;
    }

  }
}