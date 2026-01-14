using RealTimeSurvivors.Components.Shared;
using RealTimeSurvivors.Components.Units;
using RealTimeSurvivors.Systems.Buildings;
using RealTimeSurvivors.Systems.Shared;
using System.Collections.Generic;

namespace RealTimeSurvivors.Systems.Units
{
  public class CommandManager
  {
    private EntityManager _entities;
    private GridManager _grid;

    public CommandManager(EntityManager entityManager, GridManager gridManager)
    {
      _entities = entityManager;
      _grid = gridManager;
    }

    /// <summary>
    /// Assigns units that have the Selected component set to true a target position.
    /// Makes units in groups move to adjacent tiles to the targetPos given in the input.
    /// </summary>
    /// <param name="targetPos"></param>
    public void MoveCommand(Position targetPos)
    {
      var selectedUnitIds = new List<int>();
      targetPos = (Position)GridManager.WorldToTile(targetPos);

      foreach (var kvp in _entities.Selected)
      {
        if (kvp.Value.Value)
        {
          selectedUnitIds.Add(kvp.Key);
        }
      }

      if (selectedUnitIds.Count == 0)
      {
        return;
      }

      (int x, int y)[] cluster = _grid.GetNearestWalkables((int)targetPos.X, (int)targetPos.Y, selectedUnitIds.Count);
      int clusterIdx = 0;

      foreach (int entity in selectedUnitIds)
      {
        Position targetWorld = new Position();
        targetWorld = GridManager.TileToWorld(cluster[clusterIdx].x, cluster[clusterIdx].y);
        _entities.CurrentTarget[entity] = new Move(targetWorld, MoveType.Path);
        clusterIdx++;
        _entities.States[entity] = UnitState.Moving;
      }
    }
  }
}
