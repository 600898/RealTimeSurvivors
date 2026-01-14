using System;
using Godot;
using RealTimeSurvivors.Components.Buildings;
using RealTimeSurvivors.Components.Shared;
using RealTimeSurvivors.Components.Units;
using RealTimeSurvivors.Data;
using RealTimeSurvivors.Data.Entities;
using RealTimeSurvivors.Systems.Buildings;
using RealTimeSurvivors.Systems.Godot;
using RealTimeSurvivors.Systems.Units;

namespace RealTimeSurvivors.Systems.Shared
{
  /// <summary>
  /// Initialises the full game world and manages its systems.
  /// </summary>
  public class WorldManager
  {
    public event Action<GameCondition> OnGameEnded;
    private bool _gameEnded;
    public EntityManager _entities {  get; set; }
    public BuildManager _building { get; set; }
    public InterfaceManager _interface { get; set; }
    private SpatialIndex _spatial {  get; set; }
    private MovementSystem _movement { get; set; }
    private CombatSystem _combat {  get; set; }
    private UnitStateSystem _unitState {get; set; }
    private SelectionSystem _selection { get; set; }
    private RenderManager _render { get; set; }
    private CommandManager _command { get; set; }
    private GridManager _tileGrid { get; set; }
    private WaveManager _wave { get; set; }
    public Node2D _spawnNode { get; set; }

    // Modifiers and resource track
    public GameStorage store = new GameStorage()
    {
      Resources = 10000
    };

    public WorldManager(Node2D SpawnNode, CanvasLayer InterfaceNode)
    {
      _entities = new EntityManager();
      _tileGrid = new GridManager(200, 200);
      _spatial = new SpatialIndex();
      _selection = new SelectionSystem(_entities);
      _render = new RenderManager(_entities);
      _command = new CommandManager(_entities, _tileGrid);
      _movement = new MovementSystem(_entities, _tileGrid, _spatial, store);
      _combat = new CombatSystem(_entities, store);
      _unitState = new UnitStateSystem(_entities, _spatial, _movement, _combat);
      _building = new BuildManager(this, _tileGrid, _spatial, store);
      _interface = new InterfaceManager(InterfaceNode, this, store);
      _spawnNode = SpawnNode;

      _wave = new WaveManager(this);
    }

    /// <summary>
    /// Updates all world logic, placed inside Godot's _process.
    /// </summary>
    /// <param name="delta">Time between frames</param>
    public void Update(float delta)
    {
      for(int entity = 0; entity < EntityManager.GetEntityID; entity++)
      {
        if (_entities.hasAttack[entity])
        {
          _unitState.HandleEntity(entity, delta);
        }
        _render.HandleEntity(entity);
      }
      
      _building.Update(delta);
      _wave.Update(delta);

      CleanupEntities();
      CheckEndCondition();
    }

    // Checks if an game ending condition has arrived.
    private void CheckEndCondition()
    {
      if (!_entities.hasPosition[0]) // Base is always first entity, and is dead with no position
      {
        EndGame(GameCondition.Defeat);
        return;
      }

      if(_wave.CheckVictoryCondition)
      {
        EndGame(GameCondition.Victory);
        return;
      }
    }

    // Fires  of the OnGameEnded event subscribed to by Main.cs
    private void EndGame(GameCondition condition)
    {
      if (_gameEnded) { return; }
      _gameEnded = true;

      OnGameEnded?.Invoke(condition);
    }

    // Adds a node to the "SpawnNode" in Godot.
    // Ex: Sprites are added for rendering.
    private bool AddNode<E>(E node)
    {
      try
      {
        if(node is Node n)
        {
          _spawnNode.AddChild(n); 
          return true;  
        }
        return false;  
      } 
      catch (Exception e)
      {
        GD.PrintErr("Could not add Node to the World Node: " + e); 
        return false;
      }
    }


    // Removes a node from the "SpawnNode" in Godot.
    private bool RemoveNode<E>(E node)
    {
      try
      {
        if(node is Node n)
        {
          _spawnNode.RemoveChild(n);
          return true;
        }
        return false;
      }
      catch(Exception e)
      {
        GD.PrintErr("Could not remove Node from the World Node: " + e);
        return false;
      }
    }

    // Free up data for entities needed to be destroyed.
    // Example: Tiles in the build Grid, Entity position int the SpatialIndex and specific build Components.
    private void CleanupEntities()
    {
      if (_entities._pendingDestroy.Count > 0)
      {
        foreach (int id in _entities._pendingDestroy)
        {
          RemoveNode(_spawnNode.GetNode<Node>(id.ToString()));
          _spatial.Remove(id);
          if (_entities.Types[id] is BuildingType)
          {
            Building b = EntityList.Buildings[(BuildingType)_entities.Types[id]];
            BuildingFactory.DestroyComponent(b.type, _entities, id);

            PositionI bPos = GridManager.WorldToTile(_entities.Positions[id]);
            _tileGrid.FreeTiles(bPos.X - b.size.w/2, bPos.Y - b.size.h/2, b.size.w, b.size.h);
          }
        }
        _entities.CleanupDestroyed();
      }
    }

    /// <summary>
    /// Handles what happens with the selection input from the InputManager.
    /// Selects units or a building, depending on the size of the selection box.
    /// </summary>
    /// <param name="dragStart"></param>
    /// <param name="dragEnd"></param>
    /// <param name="camera"></param>
    public void HandleSelection(Vector2 dragStart, Vector2 dragEnd, Camera2D camera)
    {          
      if (dragStart.DistanceTo(dragEnd) > 5f)
      {
        _selection.HandleSelection(dragStart, dragEnd, camera);
      }
      else
      {
        Vector2 clickPos = camera.GetGlobalMousePosition();
        PositionI clickPosI = GridManager.WorldToTile(clickPos.x, clickPos.y);
        int entity = _tileGrid.GetOccupant(clickPosI.X, clickPosI.Y);
        _interface.SetSelectedBuilding(entity, _entities); 
        _building.SetSelectedBuilding(entity, _entities);
      }
    }

    /// <summary>
    /// Sends the Move Command to move all selected units to a position.
    /// </summary>
    /// <param name="target"></param>
    public void IssueMoveCommand(Position target)
    {
      _command.MoveCommand(target);
    }

    /// <summary>
    /// Checks if a building can be placed on the Grid, by World Space position.
    /// </summary>
    /// <param name="pos">Top left World Space position</param>
    /// <param name="tileWidth">Width right from pos</param>
    /// <param name="tileHeight">Height down from pos</param>
    /// <param name="entityId">Entity id</param>
    /// <returns></returns>
    public bool TryPlaceBuildingAtWorldPos((int x, int y) pos, int tileWidth, int tileHeight, int entityId)
    {
      PositionI tile = GridManager.WorldToTile(pos.x, pos.y);
      if (!_tileGrid.CanPlace(tile.X, tile.Y, tileWidth, tileHeight)) { return false; }
      _tileGrid.OccupyTiles(entityId, tile.X, tile.Y, tileWidth, tileHeight);
      return true;
    }

    /// <summary>
    /// Creates a building entity and its nececarry components.
    /// </summary>
    /// <param name="type">Building Type</param>
    /// <param name="position">Grid Space Position</param>
    /// <param name="friendly">Building faction</param>
    /// <returns></returns>
    public int CreateBuildingEntity(BuildingType type, PositionI center, bool friendly = true)
    {
      var def = EntityList.Buildings[type];
      int id = _entities.CreateEntity();

      if (!_tileGrid.CanPlace((int)center.X - def.size.w / 2, (int)center.Y - def.size.h / 2, def.size.w, def.size.h) || id == -1)
        return -1;

      _entities.Renders[id] = new Sprite
      {
        Texture = GD.Load<Texture>(AssetPaths.BuildingPath + type + ".png"),
        Name = $"{id}",
        GlobalPosition = (Vector2)GridManager.TileToWorld(center),
        Modulate = friendly ? new Color("ffffff") : new Color("ff60fd")
      };

      if (AddNode(_entities.Renders[id]))
      {
        _entities.Positions[id] = GridManager.TileToWorld(center);
        _entities.hasPosition[id] = true;
        _entities.Friendly[id] = friendly;
        _entities.Healths[id] = new Health(def.health);
        _entities.Types[id] = type;
        _tileGrid.OccupyTiles(id, (int)center.X - def.size.w / 2, (int)center.Y - def.size.h / 2, def.size.w, def.size.h);
        _spatial.Insert(id, GridManager.TileToWorld(center));
        BuildingFactory.CreateComponent(type, _entities, id);

        return id;
      }

      _entities.DestroyEntity(id);
      return -1;
    }

    /// <summary>
    /// Creates a unit entity and its nececarry components.
    /// </summary>
    /// <param name="type">Unit Type</param>
    /// <param name="health">Health points</param>
    /// <param name="attack">Attack</param>
    /// <param name="speed">Speed</param>
    /// <param name="position">Spawn position</param>
    /// <param name="friendly">Unit Faction</param>
    /// <returns></returns>
    public int CreateUnitEntity(UnitType type, int health, Attack attack, float speed, Position position, bool friendly)
    {
      int id = _entities.CreateEntity();

      if(id == -1)
      {
        return -1;
      }

      _entities.Renders[id] = new Sprite
      {
        Texture = GD.Load<Texture>(AssetPaths.UnitPath + type + ".png"),
        Name = $"{id}",
        GlobalPosition = (Vector2)position,
        SelfModulate = friendly ? new Color("ffffff") : new Color("ff60fd") // Enemy entities have a purple shade
      };

      if(friendly)
      {
        _entities.Selected.Add(id, new Selected());
      }

      if (AddNode(_entities.Renders[id]))
      {
        _entities.Positions[id] = position;
        _entities.hasPosition[id] = true;
        _entities.Attacks[id] = attack;
        _entities.hasAttack[id] = true;
        _entities.Velocities[id] = speed;
        _entities.hasVelocity[id] = true;
        _entities.Friendly[id] = friendly;
        _entities.Healths[id] = new Health(health);
        _entities.Types[id] = type;
        _entities.States[id] = UnitState.Idle;
        _spatial.Insert(id, position);

        return id;
      }

      _entities.DestroyEntity(id);
      return -1;
    }

  }
  
}
