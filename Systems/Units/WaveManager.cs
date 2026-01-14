using Godot;
using RealTimeSurvivors.Components.Buildings;
using RealTimeSurvivors.Components.Shared;
using RealTimeSurvivors.Components.Units;
using RealTimeSurvivors.Data.Entities;
using RealTimeSurvivors.Data.Wave;
using RealTimeSurvivors.Systems.Buildings;
using RealTimeSurvivors.Systems.Shared;
using System;
using System.Linq;

namespace RealTimeSurvivors.Systems.Units
{
  /// <summary>
  /// Manages the spawning of enemy units on the grid.
  /// Requires the WorldManager and GridManager to be constructed.
  /// </summary>
  public class WaveManager
  {
    WorldManager _world;
    private float _difficulty;
    private float _waveDuration;
    private const float _normalWaveTime = 180f; // Wave time is in seconds
    private const float _firstWaveStartTime = 60f;
    private int _currentWave = 0;
    public WaveDef[] _waves = WaveList.Waves;
    public bool CheckVictoryCondition => _currentWave > _waves.Length;
    private Random _rng = new Random();

    public WaveManager(WorldManager _world, float _difficulty = 1.0f, float _waveDuration = _firstWaveStartTime ) 
    {
      this._world = _world;
      this._difficulty = _difficulty;
      this._waveDuration = _waveDuration;
      InitiateGame();
    }

    // Update wave timer and spawn waves
    public void Update(float delta)
    {
      if((_waveDuration-=delta) <= 0 && _currentWave != _waves.Count())
      {
        InitiateWave();
        _waveDuration = _normalWaveTime;
        _currentWave++;
        if(_currentWave > 1)
        {
          _world._interface.ToggleRewardHud();
        }
      }
      _world._interface.SetWave(_currentWave, (int)_waveDuration);
    }

    // Start the game by spawning in the Heart in the middle of the map.
    private bool InitiateGame()
    {
      PositionI gridMiddle = new PositionI((GridManager.GetGridSize.gx * 8 - 80), (GridManager.GetGridSize.gy * 8 - 80));

      _world.CreateBuildingEntity(BuildingType.Heart, GridManager.WorldToTile(gridMiddle));

      return true;
    }

    // Initiate the next wave of enemies inside _wave, by _currentWave.
    private bool InitiateWave()
    {
      var wave = _waves[_currentWave];

      foreach(var g in wave.groups)
      {
        for(int i = 0; i < g.amount; i++)
        {
          SpawnUnit(g.type, GetRandomEdgeTile());
        }
      }
      return true;
    }

    // Spawns an enemy unit at the input spawnPos
    private void SpawnUnit(UnitType type, Position spawnPos)
    {
      Unit baseUnit = EntityList.Units[type];

      float statMultiplier = _difficulty + (_currentWave * 0.05f);

      baseUnit.health.Value *= statMultiplier;
      baseUnit.attack.Damage *= statMultiplier;
      baseUnit.speed *= (1f + _currentWave * 0.02f);

      var unitEntity = _world.CreateUnitEntity(type, (int)baseUnit.health.Value, baseUnit.attack, baseUnit.speed, spawnPos, false);
    }

    /// Gets a random world space position at the edge of the grid.
    /// Used for placing enemy units at the start of a wave.
    private Position GetRandomEdgeTile()
    {
      (int x, int y) grid = GridManager.GetGridSize;
      int edge = _rng.Next(4);

      switch (edge)
      {
        case 0:
          return GridManager.TileToWorld( new Position(_rng.Next(0, grid.x), 1) );

        case 1:
          return GridManager.TileToWorld( new Position(_rng.Next(0, grid.x), grid.y - 1) );

        case 2:
          return GridManager.TileToWorld( new Position(1, _rng.Next(0, grid.y)) );

        case 3:
          return GridManager.TileToWorld( new Position(grid.x - 1, _rng.Next(0, grid.y)) );

        default:
          return new Position(1, 1);
      }
    }
  }
}
