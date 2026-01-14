using System.Linq;
using Godot;
using RealTimeSurvivors.Components.Buildings;
using RealTimeSurvivors.Components.Shared;
using RealTimeSurvivors.Components.Units;
using RealTimeSurvivors.Data;
using RealTimeSurvivors.Data.Entities;
using RealTimeSurvivors.Systems.Shared;

namespace RealTimeSurvivors.Systems.Buildings
{
  public class ProductionManager
  {
    private WorldManager _world;
    private EntityManager _entities;
    private GameStorage _gameStorage;

    public ProductionManager(WorldManager _world, EntityManager _entities, GameStorage _store)
    {
      this._world = _world;
      this._entities = _entities;
      this._gameStorage = _store;
    }
    public void Update(float delta)
    {
      foreach (var kv in _entities.Productions.ToList())
      {
        int entity = kv.Key;
        Production prod = kv.Value;

        if (!prod.IsBuilding || prod.QueueSize == 0)
          continue;

        UnitType currentUnit = prod.Queue[0];
        Unit uDef = EntityList.Units[currentUnit];

        float requiredTime = prod.BuildTime * (1 + uDef.cost * 0.01f) * _gameStorage.buildTime;

        if ((prod.Progress+=delta) >= requiredTime)
        {
          SpawnUnit(entity, uDef);
          ShiftQueue(ref prod);

          GD.Print("UNIT SPAWNED: " + uDef.type);
        }

        _entities.Productions[entity] = prod;
      }
    }

    public bool StartUnitCreation(int entity, UnitType unit)
    {
      if (!_entities.Productions.TryGetValue(entity, out Production prod))
        return false;

      if (!prod.AvailableUnits.Contains(unit))
        return false;

      if (prod.QueueSize >= prod.Queue.Length)
        return false;

      prod.Queue[prod.QueueSize++] = unit;
      prod.IsBuilding = true;

      _entities.Productions[entity] = prod;
      return true;
    }

    private void SpawnUnit(int entity, Unit uDef)
    {
      Position pos = _entities.Positions[entity];
      _world.CreateUnitEntity(
        uDef.type,
        (int)uDef.health.Value,
        uDef.attack,
        uDef.speed,
        new Position(pos.X, pos.Y + 48),
        true
      );
    }

    private void ShiftQueue(ref Production prod)
    {
      for (int i = 1; i < prod.QueueSize; i++)
        prod.Queue[i - 1] = prod.Queue[i];

      prod.QueueSize--;
      prod.Progress = 0;
      prod.IsBuilding = prod.QueueSize > 0;
    }
  }
}