using RealTimeSurvivors.Components.Shared;
using RealTimeSurvivors.Systems.Buildings;
using System.Collections.Generic;

namespace RealTimeSurvivors.Systems.Shared
{
  /// <summary>
  /// Implementation for storing and searching for entities
  /// </summary>
  public class SpatialIndex
  {
    private readonly Dictionary<PositionI, HashSet<int>> _buckets = new Dictionary<PositionI, HashSet<int>>();

    private readonly Dictionary<int, PositionI> _entityTile = new Dictionary<int, PositionI>();

    /// <summary>
    /// Inserts an entity into the index.
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="worldPos"></param>
    public void Insert(int entity, Position worldPos)
    {
      var tile = GridManager.WorldToTile(worldPos);

      if (!_buckets.TryGetValue(tile, out var set))
      {
        set = new HashSet<int>();
        _buckets[tile] = set;
      }

      set.Add(entity);
      _entityTile[entity] = tile;
    }

    /// <summary>
    /// Updates the Position and and location of an entity inside the index.
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="worldPos"></param>
    public void Update(int entity, Position worldPos)
    {
      PositionI newTile = GridManager.WorldToTile(worldPos);

      if (_entityTile.TryGetValue(entity, out var oldTile) && oldTile == newTile)
      {
        return;
      }

      Remove(entity);
      Insert(entity, worldPos);
    }

    /// <summary>
    /// Removes an entity from the index.
    /// </summary>
    /// <param name="entity"></param>
    public void Remove(int entity)
    {
      if (_entityTile.TryGetValue(entity, out var tile))
      {
        if (_buckets.TryGetValue(tile, out var set))
        {
          set.Remove(entity);
          if (set.Count == 0)
          {
            _buckets.Remove(tile);
          }
        }

        _entityTile.Remove(entity);
      }
    }

    /// <summary>
    /// Try to find the closest entity ID inside the index.
    /// Returns -1 if nothing exists -- which is the default none-target id.
    /// </summary>
    /// <param name="self"></param>
    /// <param name="worldPos"></param>
    /// <param name="rangeInTiles"></param>
    /// <param name="entities"></param>
    /// <returns></returns>
    public int FindClosest(int self, Position worldPos, int rangeInTiles, EntityManager entities)
    {
      PositionI center = GridManager.WorldToTile(worldPos);

      int best = -1;
      float bestDist = float.MaxValue;

      for (int dx = -rangeInTiles; dx <= rangeInTiles; dx++)
      {
        for (int dy = -rangeInTiles; dy <= rangeInTiles; dy++)
        {
          PositionI tile = new PositionI(center.X + dx, center.Y + dy);

          if (!_buckets.TryGetValue(tile, out var set)) { continue; }

          foreach (var other in set)
          {
            if (other == self) { continue; }

            if (!entities.hasPosition[other]) { continue; }

            if (entities.Friendly[self] == entities.Friendly[other]) { continue; }

            float d = worldPos.DistanceTo(entities.Positions[other]);

            if (d < bestDist)
            {
              bestDist = d;
              best = other;
            }
          }
        }
      }

      return best;
    }

  }
}