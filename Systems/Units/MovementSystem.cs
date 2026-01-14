using Godot;
using RealTimeSurvivors.Components.Buildings;
using RealTimeSurvivors.Components.Shared;
using RealTimeSurvivors.Components.Units;
using RealTimeSurvivors.Data;
using RealTimeSurvivors.Systems.Buildings;
using RealTimeSurvivors.Systems.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using Path = RealTimeSurvivors.Components.Units.Path;

namespace RealTimeSurvivors.Systems.Units
{
  /// <summary>
  /// Handles the moving interactions for Units and assigns paths based on a Direct or AStar approach.
  /// </summary>
  public class MovementSystem
  {
    private readonly EntityManager _entities;
    private readonly GridManager _grid;
    public readonly SpatialIndex _spatial;
    private readonly GameStorage _gameStorage;

    public MovementSystem(EntityManager entities, GridManager grid, SpatialIndex spatial, GameStorage store) 
    { 
      _entities = entities; 
      _grid = grid;
      _spatial = spatial;
      _gameStorage = store;
    }



  /// <summary>
  /// Move or Assign a Path to a Unit based on a current Target Position.
  /// </summary>
  /// <param name="entity">Entity id</param>
  /// <param name="delta">Delta of frames</param>
  /// <returns></returns>
  public bool MoveEntity(int entity, float delta)
  {

    if (!_entities.hasVelocity[entity]) { return true; }

    if (!_entities.CurrentTarget.TryGetValue(entity, out Move move)) { return true; }

    float speedModifier = _entities.Friendly[entity] ? _gameStorage.fSpeed : _gameStorage.eSpeed;
    float speed = _entities.Velocities[entity] * speedModifier;
    Position pos = _entities.Positions[entity];

    // Ensure a path exists or create a new one
    if (!_entities.Paths.TryGetValue(entity, out Path path))
    {
      Position[] result;

      if(move.type == MoveType.Path || !DirectPath(pos, move.position, out result))
      {
        result = AStar(pos, move.position);
      }

      if (result == null || result.Length == 0)
      {
        _entities.CurrentTarget.Remove(entity);
        return true;
      }

      path = new Path(result);
      _entities.Paths[entity] = path;
    }

    // Move along path
    Position next = path.Value[path.index];
    Vector2 toNext = new Vector2(next.X - pos.X, next.Y - pos.Y);

    if (toNext.Length() <= 4f) // Arrival tolerance -- Stops units from twitching/never reaching their target
    {
      pos = next;
      path.index++;

      // Reached final destination
      if (path.index >= path.Value.Length)
      {
        _entities.Paths.Remove(entity);

        if (move.consumeOnArrival)
        {
          _entities.CurrentTarget.Remove(entity);
        }

        _entities.Positions[entity] = pos;
        _spatial.Update(entity, pos);
        return true;
      }
    }
    else
    {
      Vector2 dir = toNext.Normalized();
      pos = new Position(
        pos.X + dir.x * speed * delta,
        pos.Y + dir.y * speed * delta
      );
    }

    // Commit movement
    _entities.Positions[entity] = pos;
    _entities.Paths[entity] = path;
    _spatial.Update(entity, pos);

    return false;
  }

    /*
     * Uses the "BresenhamLine" algorithm to check if a unit can walk in a straight line from the starting position to the target.
     * Less computational than AStar for pathfinding, and is limited to 6 steps/tiles.
     */
    private bool DirectPath(Position startWorld, Position targetWorld, out Position[] path, int stepCount = 6)
    {
      int tileSize = GridManager.GetTileSize;
      (int x, int y) start = ((int)startWorld.X / tileSize, (int)startWorld.Y / tileSize);
      (int x, int y) target = ((int)targetWorld.X / tileSize, (int)targetWorld.Y / tileSize);

      int dX = Math.Abs(target.x - start.x);
      int dY = Math.Abs(target.y - start.y);

      int sx = start.x < target.x ? 1 : -1;
      int sy = start.y < target.y ? 1 : -1;

      // -- dp: Decision Parameter
      int dp = dX - dY;

      path = new Position[stepCount];

      for(int i = 0; i < stepCount; i++)
      {
        int dp2 = 2 * dp;

        if(dp2 > -dY)
        {
          dp -= dY;
          start.x += sx;
        }
        if (dp2 < dX)
        {
          dp += dX;
          start.y += sy;
        }

        if(!_grid.IsWalkable(start.x, start.y))
        {
          return false;
        }

        path[i] = GridManager.TileToWorld(start.x, start.y);
      }

      return true;
    }

    /// <summary>
    /// Helper function to get appropriately close to a target.
    /// </summary>
    /// <param name="attacker">Entity id</param>
    /// <param name="target">Target entity id</param>
    /// <returns></returns>
    public Position GetApproachPosition(int attacker, int target)
    {
      Position self = _entities.Positions[attacker];
      float stopDist = _entities.Attacks[attacker].Range - 2f;

      if (_entities.Types[target] is BuildingType bType) // Target is a building
      {
        Rect2 rect = BuildManager.GetBuildingWorldRect(_entities.Positions[target], bType);

        // Find closest point on rect to the attacker
        float closestX = Mathf.Clamp(self.X, rect.Position.x, rect.Position.x + rect.Size.x);
        float closestY = Mathf.Clamp(self.Y, rect.Position.y, rect.Position.y + rect.Size.y);
        Position closest = new Position(closestX, closestY);

        // Direction from attacker to the closest point
        Vector2 dir = ((Vector2)(closest - self));
        if (dir.Length() > 0)
          dir = dir.Normalized();

        // Move towards that point but stop at stopDist
        return self + new Position(dir.x * stopDist, dir.y * stopDist);
      }
      else // Target is a unit
      {
        Position enemy = _entities.Positions[target];
        Vector2 dir = ((Vector2)(enemy - self));
        if (dir.Length() > 0)
          dir = dir.Normalized();

        return self + new Position(dir.x * stopDist, dir.y * stopDist);
      }
    }

    /*
     * Takes the start and end coordinates in world space and tries to find a path in the grid.
     * Based on A* search algorithm
     * Returns an array of the tile positions in world space.
     */
    private Position[] AStar(Position startWorld, Position targetWorld)
    {
      int tileSize = GridManager.GetTileSize;
      (int x, int y) start = ((int)startWorld.X / tileSize, (int)startWorld.Y / tileSize);
      (int x, int y) target = ((int)targetWorld.X / tileSize, (int)targetWorld.Y / tileSize);

      if (!_grid.InBounds(target.x, target.y) || !_grid.IsWalkable(target.x, target.y))
      {
        return null;
      }

      List<Node> open = new List<Node>();
      List<Node> close = new List<Node>();

      Node startNode = new Node(start.x, start.y);
      startNode.h = AStarHeuristic(start, target);
      open.Add(startNode);

      Node furthestNode = startNode;

      (int dx, int dy)[] neighbors = new[]
      {
        (-1, 0), (-1,-1), (-1,1), (0,-1), (0,1), (1,-1), (1,0), (1,1)
      };

      while (open.Count > 0)
      {
        Node current = open[0];
        foreach(var node in open)
        {
          if (node.f < current.f)
          {
            current = node;
          }
        }

        if (current.f < furthestNode.f)
        {
          furthestNode = current;
        }

        if ((current.x, current.y) == (target.x, target.y)) 
        { 
          return Reconstruct(current); 
        }

        open.Remove(current);
        close.Add(current);

        foreach(var (dx, dy) in neighbors)
        {
          int nx = current.x + dx;
          int ny = current.y + dy;

          if(!_grid.IsWalkable(nx, ny) || !_grid.InBounds(nx, ny))
          {
            continue;
          }

          if(close.Any(n => n.x == nx && n.y == ny))
          {
            continue;
          }

          float tentativeG = current.g + ((dx == 0 || dy == 0) ? 10 : 14);

          Node existing = open.FirstOrDefault(n => n.x == nx && n.y == ny);

          if(existing == null)
          {
            Node nn = new Node(nx, ny);
            nn.g = tentativeG;
            nn.h = AStarHeuristic((nx, ny), target);
            nn.parent = current;

            open.Add(nn);
          }
          else if (tentativeG < existing.g)
          {
            existing.g = tentativeG;
            existing.parent = current;
          }
        }
      }
      return Reconstruct(furthestNode);
    }

    // Reconstruct the path from A*
    private Position[] Reconstruct(Node current)
    {
      List<Position> path = new List<Position>();

      while (current != null)
      {
        // Convert tile indices to world position using TileToWorld
        path.Add(GridManager.TileToWorld(current.x, current.y));
        current = current.parent;
      }

      path.Reverse();
      return path.ToArray();
    }

    // A* Node
    private class Node
    {
      public int x, y;
      public float g, h;
      public float f => g + h;

      public Node parent;

      public Node(int x, int y)
      {
        this.x = x;
        this.y = y;
        g = 0;
        h = 0;
        parent = null;
      }
    }

    /*
     * Returns the heuristic needed for A*.
     * Input is the current and target Node/cell
     */
    private float AStarHeuristic((int x, int y) current, (int x, int y) target)
    {
      float dx = Mathf.Abs(current.x - target.x);
      float dy = Mathf.Abs(current.y - target.y);
      return 10 * (dx + dy) + 4 * Mathf.Min(dx, dy);
    }
  }
}
