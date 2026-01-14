using RealTimeSurvivors.Components.Shared;
using System.Collections.Generic;

namespace RealTimeSurvivors.Systems.Buildings
{
  /// <summary>
  /// Tile grid for building placement.
  /// - tileSize: the size of one tile in world pixels
  /// - width, height: grid size in x/y tiles
  /// - occupency: tiles occupied by entityId, where -1 is an empty tile
  /// </summary>
  public class GridManager
  {
    private static int _tileSize = 16;
    private static (int x, int y) _gridSize;
    public static int GetTileSize => _tileSize;
    public static (int gx, int gy) GetGridSize => _gridSize; 
    private readonly int _width;
    private readonly int _height;
    private readonly int[,] _occupancy;


    public GridManager(int _width, int _height)
    {
      this._width = _width;
      this._height = _height;
      _gridSize = (_width, _height);

      _occupancy = new int[_width, _height];
      for (int x = 0; x < _width; x++)
      {
        for (int y = 0; y < _height; y++)
        {
          _occupancy[x, y] = -1;
        }
      }
    }

    public static Position TileToWorld(int tx, int ty)
    {
      return new Position(tx * _tileSize + _tileSize / 2f, ty * _tileSize + _tileSize / 2f);
    }

    public static Position TileToWorld(Position pos)
    {
      return new Position(pos.X * _tileSize + _tileSize / 2f, pos.Y * _tileSize + _tileSize / 2f);
    }

    public static Position TileToWorld(PositionI pos)
    {
      return new Position(pos.X * _tileSize + _tileSize / 2f, pos.Y * _tileSize + _tileSize / 2f);
    }

    public static PositionI WorldToTile(float wx, float wy)
    {
      return (PositionI) new Position(wx / _tileSize, wy / _tileSize);
    }

    public static PositionI WorldToTile(Position pos)
    {
      return (PositionI) new Position(pos.X / _tileSize, pos.Y / _tileSize);
    }

    public static PositionI WorldToTile(PositionI pos)
    {
      return new PositionI(pos.X / _tileSize, pos.Y / _tileSize);
    }

    /// <summary>
    /// Checks if you are within the grid space bounds.
    /// Use Tile coordinates.
    /// </summary>
    /// <param name="tx"></param>
    /// <param name="ty"></param>
    /// <returns></returns>
    public bool InBounds(int tx, int ty)
    {
      return tx >= 0 && ty >= 0 && tx < _width && ty < _height;
    }

    /// <summary>
    /// Check if you can place a building with the size of (w*h) starting at top-left tile (tx,ty)
    /// </summary>
    public bool CanPlace(int tx, int ty, int w, int h)
    {
      for (int x = tx; x < tx + w; x++)
      {
        for (int y = ty; y < ty + h; y++)
        {
          if (!InBounds(x, y)) { return false; }
          if (_occupancy[x, y] != -1) { return false; }
        }
      }
      return true;
    }

    /// <summary>
    /// Occupy tiles with an entity's id.
    /// </summary>
    public void OccupyTiles(int entityId, int tx, int ty, int w, int h)
    {
      for (int x = tx; x < tx + w; x++)
      {
        for (int y = ty; y < ty + h; y++)
        {
          if (InBounds(x, y)) 
          {
            _occupancy[x, y] = entityId;
          }
        }
      }
    }

    ///<summary>
    /// Free up tiles for entities
    /// </summary>
    public void FreeTiles(int tx, int ty, int w, int h)
    {
      for (int x = tx; x < tx + w; x++)
      {
        for (int y = ty; y < ty + h; y++)
        {
          if (InBounds(x, y))
          {
            _occupancy[x, y] = -1;
          }
        }
      }
    }

    public int GetOccupant(int tx, int ty)
    {
      if(!InBounds(tx, ty)) {  return -1; }
      return _occupancy[tx, ty];
    }

    public bool IsWalkable(int tx, int ty)
    {
      if(!InBounds(tx, ty)) { return false; }
      return _occupancy[tx, ty] == -1;
    }

    /// <summary>
    /// Helper function to find nearest non-occupied tiles for better Unit formation.
    /// Tries to find tiles around the input coordinates, with a maximum ring of 3 (
    /// </summary>
    /// <param name="tx">X value in grid space</param>
    /// <param name="ty">Y value in grid space</param>
    /// <param name="size">How many neighbour coordinates returned</param>
    /// <returns>(int x, int y)[]</returns>
    public (int x, int y)[] GetNearestWalkables(int tx, int ty, int size)
    {
      Queue<(int, int)> queue = new Queue<(int, int)>();
      HashSet<(int, int)> visited = new HashSet<(int, int)>();
      (int x, int y)[] result = new (int x, int y)[size];
      int resultIdx = 0;

      queue.Enqueue((tx, ty));
      visited.Add((tx, ty));

      (int dx, int dy)[] neighbors = new[]
{
        (-1, 0), (-1,-1), (-1,1), (0,-1), (0,1), (1,-1), (1,0), (1,1)
      };

      while (queue.Count > 0 && resultIdx < size)
      {
        (int x, int y) current = queue.Dequeue();

        if(InBounds(current.x, current.y))
        {
          if(IsWalkable(current.x, current.y))
          {
            result[resultIdx] = current;
            resultIdx++;
          }
        }

        for(int i = 0; i < neighbors.Length; i++)
        {
          (int x, int y) next = (
            current.x + neighbors[i].dx, 
            current.y + neighbors[i].dy
            );

          if(!visited.Contains(next))
          {
            visited.Add(next);
            queue.Enqueue(next);
          }
        }
      }

      return result;
    }
  }
}
