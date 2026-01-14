using System.Linq;
using RealTimeSurvivors.Components.Buildings;
using RealTimeSurvivors.Data;
using RealTimeSurvivors.Systems.Shared;
namespace RealTimeSurvivors.Systems.Buildings
{
  /// <summary>
  /// Updates the Entities that have the GenerateResource building component.
  /// </summary>
  public class GeneratorManager 
  {
    private EntityManager _entities;
    private WorldManager _world;
    private GameStorage _gameStorage;

    public GeneratorManager(WorldManager _world, EntityManager _entities, GameStorage _store)
    {
      this._entities = _entities;
      this._world = _world;
      this._gameStorage = _store;
    }

    public void Update(float delta)
    {
      foreach(int entity in _entities.ResourceGenerators.Keys.ToList())
      {
        Generator gr = _entities.ResourceGenerators[entity];
        gr.CurrentDelta += delta;

        if(gr.CurrentDelta >= gr.DeltaGoal)
        {
          gr.TotalResources += gr.ResourcePerGoal;
          _world.store.Resources += gr.ResourcePerGoal * _gameStorage.generateAmount;
          gr.CurrentDelta = 0f;
        }
        _entities.ResourceGenerators[entity] = gr;
      }
      _world._interface.SetResourceAmount((int)_world.store.Resources);
    }
  }
}