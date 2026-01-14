using Godot;
using RealTimeSurvivors.Components.Buildings;
using RealTimeSurvivors.Components.Shared;
using RealTimeSurvivors.Components.Units;
using RealTimeSurvivors.Data;
using RealTimeSurvivors.Data.Entities;
using RealTimeSurvivors.Systems.Shared;

namespace RealTimeSurvivors.Systems.Buildings
{
  /// <summary>
  /// System for Managing BuildMode, interacting with the GridManager and updating specific Buildings systems.
  /// </summary>
  public class BuildManager : Node
  {
    private readonly WorldManager _world;
    private readonly EntityManager _entities;

    /*
     * Build mode related
    */
    public ModeTypes _buildMode { get; private set; } = ModeTypes.Select; //Select == buildMode off.
    private BuildingType _currentBuilding = BuildingType.Forge;
    private readonly Node2D _previewBuilding = new Node2D() { Name = "PreviewBuilding" }; // "Ghost" sprite for placement visual
    private readonly Sprite _previewSprite = new Sprite() { Name = "PreviewSprite" };
    private readonly GridManager _grid;
    private readonly SpatialIndex _spatial;
    private PositionI PreviewBuildingCenter()
    {
      return GridManager.WorldToTile(_previewBuilding.Position.x, _previewBuilding.Position.y);
    }

    /*
     * Building Component Managers
     * Used for building component specific behaviours
    */
    private BuildingType _currentSelectedType;
    private int _currentSelectedId = -1;
    public void SetSelectedBuilding(int entity, EntityManager _entities) 
    {
      _currentSelectedId = entity;
      GD.Print("Building selected: " + entity);
      if (_currentSelectedId != -1)
      {
        _currentSelectedType = (BuildingType)_entities.Types[entity];
        GD.Print("Building selected: " + _currentSelectedType);
      }
    }
    private readonly GeneratorManager _resource;
    private readonly ProductionManager _production;


    public BuildManager(WorldManager _world, GridManager _grid, SpatialIndex _spatial, GameStorage _store)
    {
      this._world = _world;
      this._grid = _grid;
      this._spatial = _spatial;
      this._entities = _world._entities;
      this._resource = new GeneratorManager(_world, _entities, _store);
      this._production = new ProductionManager(_world, _entities, _store);
      Name = "BuildManager";
    }

    // Updates the component specific systems for buildings.
    public void Update(float delta)
    {
      _resource.Update(delta);
      _production.Update(delta);
    }

    // Functions run when the Node enters the Godot scene tree.
    public override void _Ready()
    {
      _previewBuilding.AddChild(_previewSprite);
      AddChild( _previewBuilding );
      _previewBuilding.Visible = false;
      var tex = GD.Load<Texture>($"{AssetPaths.BuildingPath}{_currentBuilding}.png");
      _previewSprite.Texture = tex;
    }

    // Sets the current BuildMode based on ModeTypes input.
    public void SetMode(ModeTypes mode)
    {
      if (_buildMode == mode)
        return;

      _buildMode = mode;

      switch (_buildMode)
      {
        case ModeTypes.Select:
          ExitBuildMode();
          break;

        case ModeTypes.Build:
          EnterBuildMode();
          break;

        case ModeTypes.Destroy:
          ExitBuildMode();
          break;
      }
    }

    private void EnterBuildMode()
    {
      _previewBuilding.Modulate = new Color(1, 1, 1, 0.5f);
      _previewBuilding.Visible = true;
    }

    private void ExitBuildMode()
    {
      _previewBuilding.Visible = false;
    }

    /// <summary>
    /// Sets the current placable building and "Ghost" sprite at the cursor.
    /// </summary>
    /// <param name="BuildingTypeInt">Buildings in the interface are in order of the BuildingTypes.</param>
    public void ChangeBuilding(int BuildingTypeInt)
    {
      if( 0 < BuildingTypeInt && BuildingTypeInt < 5)
      {
        _currentBuilding = (BuildingType)BuildingTypeInt;
        var tex = GD.Load<Texture>($"{AssetPaths.BuildingPath}{_currentBuilding}.png");
        GD.Print($"Loading Building: {AssetPaths.BuildingPath}{_currentBuilding}.png");
        _previewSprite.Texture = tex;
      }
    }

    /// <summary>
    /// Updates the position of the building "Ghost" sprite at the cursor.
    /// </summary>
    /// <param name="worldPos">Cursor Position</param>
    public void UpdatePlacement(Vector2 worldPos)
    {
      if (_buildMode != ModeTypes.Build) return;

      PositionI tile = GridManager.WorldToTile(worldPos.x, worldPos.y);
      Position worldCenter = GridManager.TileToWorld(tile);

      _previewBuilding.Position = (Vector2)worldCenter;
    }

    /// <summary>
    /// Creates the building entity
    /// </summary>
    public void ConfirmPlacement()
    {
      if (_buildMode != ModeTypes.Build) return;

      PositionI center = PreviewBuildingCenter();
      int id = _world.CreateBuildingEntity(_currentBuilding, center);
    }

    /// <summary>
    /// Tries to place a Building in the grid from the GridManager.
    /// Checks and removes the needed Resources from the GameStorage.
    /// </summary>
    public void TryPlaceBuilding()
    {
      PositionI center = PreviewBuildingCenter();
      Building b = EntityList.Buildings[_currentBuilding];

      if (b.cost > _world.store.Resources) { return; }

      int halfW = b.size.w / 2;
      int halfH = b.size.h / 2;

      int startX = center.X - halfW;
      int startY = center.Y - halfH;

      if (_grid.CanPlace(startX, startY, b.size.w, b.size.h))
      {
        ConfirmPlacement();
        _world.store.Resources -= b.cost;
      }
    }
    /// <summary>
    /// Tries to Destroy a building entity and free up tiles inside the Grid at the cursor position.
    /// Refunds cost of a building to GameStorage.
    /// </summary>
    /// <param name="clickPos">Cursor Position</param>
    public void TryDestroyBuilding(Vector2 clickPos)
    {
      PositionI gridPos = new PositionI(clickPos.x, clickPos.y);
      gridPos = GridManager.WorldToTile(gridPos);
      int entity = _grid.GetOccupant(gridPos.X, gridPos.Y);

      if(entity == -1 || _entities.Types[entity] is BuildingType.Heart) { return; }

      GD.Print($"Tried to delete building at grid position: {gridPos} id: {entity}");

      if(entity != 0) // Entity 0 is always the heart.
      {
        BuildingType bType = (BuildingType)_entities.Types[entity];
        Size bSize = EntityList.Buildings[bType].size;

        _entities.DestroyEntity(entity);
        BuildingFactory.DestroyComponent(bType, _entities, entity);
        int halfW = bSize.w / 2;
        int halfH = bSize.h / 2;

        _grid.FreeTiles(
          gridPos.X - halfW,
          gridPos.Y - halfH,
          bSize.w,
          bSize.h
        );

        Building b = EntityList.Buildings[bType];
        _world.store.Resources += b.cost;
        GD.Print($"Building deleted: {bType} id: {entity}");
      }
    }
    /// <summary>
    /// Currently only used for Production buildings.
    /// Takes a numeric input for buttons in order of the UnitTypes array connected in Production buildings.
    /// </summary>
    /// <param name="input">Number Input</param>
    public void TryUseBuilding(int input)
    {
      if (_currentSelectedId == -1 || !_entities.Productions.ContainsKey(_currentSelectedId)) 
      {
        return;
      }

      Production prod = _entities.Productions[_currentSelectedId];

      int idx = input-1;
      if (idx < 0 || idx >= prod.AvailableUnits.Length)
      {
        return;
      }

      UnitType unit = prod.AvailableUnits[idx];
      Unit e = EntityList.Units[unit];
      if(_world.store.Resources - e.cost >= 0)
      {
        if(_production.StartUnitCreation(_currentSelectedId, unit))
        {
          _world.store.Resources -= e.cost;
        }
      }
    }

    /// <summary>
    /// Gets the rectangle size for a building in World Space.
    /// Helper function for MovementManager.
    /// </summary>
    /// <param name="pos">Top Left position for buildings</param>
    /// <param name="bType">Specific Building Type for looking up size</param>
    /// <returns></returns>
    public static Rect2 GetBuildingWorldRect(Position center, BuildingType bType)
    {
      Building b = EntityList.Buildings[bType];
      float tile = GridManager.GetTileSize;

      Vector2 size = new Vector2(b.size.w * tile, b.size.h * tile);
      Vector2 topLeft = new Vector2(center.X, center.Y) - size / 2f;

      return new Rect2(topLeft, size);
    }

  }
}
