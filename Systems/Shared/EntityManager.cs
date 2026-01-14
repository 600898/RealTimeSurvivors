using Godot;
using RealTimeSurvivors.Components.Buildings;
using RealTimeSurvivors.Components.Shared;
using RealTimeSurvivors.Components.Units;
using System;
using System.Collections.Generic;
using Path = RealTimeSurvivors.Components.Units.Path;
namespace RealTimeSurvivors.Systems.Shared
{
  /// <summary>
  /// Stores, Destroys and Manages all Entity Components.
  /// </summary>
  public class EntityManager
  {
    private static int _entityId = 0; /// Entity ID used for all entities
    public Stack<int> _pendingDestroy = new Stack<int>();
    private static Stack<int> _freeEntityId = new Stack<int>();
    public static int GetEntityID => _entityId;
    private int _maxEntities;

    /*
     * Dense Components are stored inside arrays.
     * "has-" arrays are components checked by systems before acting on them.
    */
    public readonly Position[] Positions;
    public readonly bool[] hasPosition;
    public readonly float[] Velocities;
    public readonly bool[] hasVelocity;
    public readonly Health[] Healths;
    public readonly Sprite[] Renders;
    public readonly bool[] Friendly;
    public readonly Attack[] Attacks;
    public readonly bool[] hasAttack;
    public readonly Enum[] Types;
    public readonly UnitState[] States;

    public EntityManager(int _maxEntities = 1024)
    {
      this._maxEntities = _maxEntities;
      Positions = new Position[_maxEntities];
      hasPosition = new bool[_maxEntities];
      Velocities = new float[_maxEntities];
      hasVelocity = new bool[_maxEntities];
      Healths = new Health[_maxEntities];
      Renders = new Sprite[_maxEntities];
      Friendly = new bool[_maxEntities];
      Attacks = new Attack[_maxEntities];
      hasAttack = new bool[_maxEntities];
      Types = new Enum[_maxEntities];
      States = new UnitState[_maxEntities];
    }

    /*
     * Non frequent/sparse components are inside dictionaries. 
    */
    public readonly Dictionary<int, Move> CurrentTarget = new Dictionary<int, Move>();
    public readonly Dictionary<int, Path> Paths = new Dictionary<int, Path>();
    public readonly Dictionary<int, Selected> Selected = new Dictionary<int, Selected>();
    public readonly Dictionary<int, Generator> ResourceGenerators = new Dictionary<int, Generator>();
    public readonly Dictionary<int, Production> Productions = new Dictionary<int, Production>();

    /// <summary>
    /// Returns an available or new Entity ID.
    /// </summary>
    /// <returns></returns>
    public int CreateEntity()
    {
      if(_freeEntityId.Count > 0)
      {
        return _freeEntityId.Pop();
      }

      if(_entityId == _maxEntities)
      {
        return -1;
      }

      return _entityId++;
    }
    
    /// <summary>
    /// Sets up an entity to be destroyed at the end of a update cycle.
    /// </summary>
    /// <param name="id">Entity id</param>
    public void DestroyEntity(int id)
    {
      _pendingDestroy.Push(id);
    }

    /// <summary>
    /// Removes all possible Components from the entity's inside the pending destroy list.
    /// </summary>
    public void CleanupDestroyed()
    {
      foreach(int id in _pendingDestroy)
      {
        Positions[id] = default;
        Attacks[id] = default;
        Healths[id] = default;
        hasPosition[id] = false;
        hasAttack[id] = false;
        hasVelocity[id] = false;
        hasPosition[id] = false;
        Renders[id] = null;
        Types[id] = null;
        Selected.Remove(id);
        ResourceGenerators.Remove(id);
        States[id] = UnitState.None;

        CurrentTarget.Remove(id);
        Paths.Remove(id);
        Selected.Remove(id);

        _freeEntityId.Push(id);
      }

      _pendingDestroy.Clear();
    }
  }
}
