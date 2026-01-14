using Godot;
using RealTimeSurvivors.Components.Shared;
using RealTimeSurvivors.Components.Units;
using RealTimeSurvivors.Systems.Shared;

namespace RealTimeSurvivors.Systems.Units
{
  /// <summary>
  /// Handles what happens with the input from the Selection Box from the interface.
  /// </summary>
  public class SelectionSystem
  {
    private EntityManager _entities;

    private readonly int MaxUnitSelection = 30;

    public SelectionSystem(EntityManager _entities)
    {
      this._entities = _entities;
    }

    /// <summary>
    /// Selects units based on their intersection with the Selection Box.
    /// </summary>
    /// <param name="dragStart">Selection Box start in World Space</param>
    /// <param name="dragEnd">Selection Box end in World Space</param>
    /// <param name="camera">Camera</param>
    public void HandleSelection(Vector2 dragStart, Vector2 dragEnd, Camera2D camera)
    {
      Vector2 worldStart = camera.GetGlobalMousePosition() - (dragEnd - dragStart);
      Vector2 worldEnd = camera.GetGlobalMousePosition();

      Rect2 box = new Rect2(
          new Vector2(Mathf.Min(worldStart.x, worldEnd.x), Mathf.Min(worldStart.y, worldEnd.y)),
          new Vector2(Mathf.Abs(worldEnd.x - worldStart.x), Mathf.Abs(worldEnd.y - worldStart.y))
      );

      int selectedIdx = 0;

      for (int entity = 0; entity < EntityManager.GetEntityID; entity++)
      {
        Position pos = _entities.Positions[entity];

        if (!_entities.Selected.ContainsKey(entity))
          continue;

        Selected sel = _entities.Selected[entity];
        sel.Value = box.HasPoint((Vector2)pos);
        _entities.Selected[entity] = sel;

        if (sel.Value)
        {
          selectedIdx++;
          if (selectedIdx == MaxUnitSelection)
          {
            break;
          }
        }
      }
    }
  }
}
