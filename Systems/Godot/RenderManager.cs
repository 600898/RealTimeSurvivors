using Godot;
using RealTimeSurvivors.Components.Buildings;
using RealTimeSurvivors.Components.Shared;
using RealTimeSurvivors.Data.Entities;
using RealTimeSurvivors.Systems.Buildings;
using RealTimeSurvivors.Systems.Shared;
using System.Collections.Generic;
using System.Data;
namespace RealTimeSurvivors.Systems.Godot
{
  /// <summary>
  /// Ties the Entity Component System to the Godot Node system
  /// Updates the entity's sprite position.
  /// </summary>
  public class RenderManager
  {
    private readonly EntityManager _entities;
    private readonly PackedScene _hpBarScene = GD.Load<PackedScene>("res://GDScenes/HPBar.tscn");
    private readonly Dictionary<int, ProgressBar> _hpBars = new Dictionary<int, ProgressBar>();

    public RenderManager(EntityManager entities)
    {
      _entities = entities;
    }

    public void HandleEntity(int entity)
    {   
      Sprite render = _entities.Renders[entity];

      if(!UpdateHP(entity) && render != null)
      {
        _hpBars[entity] = CreateHpBar(entity, render);
      }

      if (render == null || _entities.Types[entity] is BuildingType) { return; }

      Vector2 newPos = (Vector2)_entities.Positions[entity];
      render.LookAt(newPos);
      _hpBars[entity].RectRotation = -render.RotationDegrees;
      render.Position = newPos;
    }

    private ProgressBar CreateHpBar(int entity, Sprite parent)
    {
      var bar = _hpBarScene.Instance<ProgressBar>();

      bar.MinValue = 0;
      bar.MaxValue = _entities.Healths[entity].Value;
      bar.Value = _entities.Healths[entity].Current;
      bar.PercentVisible = false;

      if (_entities.Types[entity] is BuildingType)
      {
        bar.AnchorLeft = -0.4f;
        bar.AnchorRight = 0.2f;
        bar.AnchorTop = -0.3f;
        bar.AnchorBottom = -0.3f;
      }
      bar.Visible = false;
      bar.ShowOnTop = true;

      parent.AddChild(bar);

      return bar;
    }

    private bool UpdateHP(int entity)
    {
      if (_hpBars.TryGetValue(entity, out ProgressBar hbar))
      {
        Health h = _entities.Healths[entity];
        if(h.Value != h.Current)
        {
          hbar.Value = h.Current;
          hbar.Show();
        }
        return true;
      }

      return false;
    }

  }

}
