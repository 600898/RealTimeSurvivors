using Godot;
using RealTimeSurvivors.Data;
using RealTimeSurvivors.Systems.Godot;
using RealTimeSurvivors.Systems.Shared;

/// <summary>
/// Script connected to the Main Godot Node containing the game's Interface, World, etc...
/// </summary>
namespace RealTimeSurvivors.GDScenes.Scripts
{

  public class Main : Node2D
  {
    private Camera2D _camera;
    private NinePatchRect _selectionBox;
    private WorldManager _world;
    private InputManager _input;
    private Node2D WorldNode = new Node2D() { Name = "World" };
    private CanvasLayer Interface;
    private Panel MainMenu;
    private Control WorldUI;

    public override void _Ready()
    {
      Interface = GetNode<CanvasLayer>("Interface");
      _camera = GetNode<Camera2D>("Camera2D");
      _selectionBox = Interface.GetNode<NinePatchRect>("SlctRect");
      MainMenu = Interface.GetNode<Panel>("START_MENU");
      WorldUI = Interface.GetNode<Control>("WorldUI");

      Interface.GetNode<Button>("START_MENU/CenterContainer/VBoxContainer/Button").Connect("pressed", this, nameof(StartGame));
    }

    public override void _Process(float delta)
    {
      if (_world != null)
      {
        _world.Update(delta);
      }
    }

    private void StartGame()
    {
      WorldNode = new Node2D() { Name = "World" };
      _world = new WorldManager(WorldNode, Interface);
      _world.OnGameEnded += OnGameEndedCallback;
      _camera.Position = new Vector2(1600, 1600);

      _input = new InputManager(_camera, _selectionBox, _world);

      AddChild(WorldNode);
      AddChild(_world._building);
      AddChild(_input);

      MainMenu.Visible = false;
      WorldUI.Visible = true;
    }

    private async void OnGameEndedCallback(GameCondition condition)
    {
      GD.Print($"Game ended: {condition}");

      _world._interface.ToggleGameEndHud(condition);

      GetTree().Paused = true;
      await ToSignal(GetTree().CreateTimer(7.0f), "timeout");

      GetTree().Paused = false;

      _world._interface.ToggleGameEndHud(GameCondition.Running);

      RemoveChild(_input);
      RemoveChild(_world._building);
      RemoveChild(WorldNode);

      _world.OnGameEnded -= OnGameEndedCallback;
      _world = null;
      _input = null;

      if (condition == GameCondition.Victory)
      {
        MainMenu.Visible = true;
        WorldUI.Visible = false;
      }

      if (condition == GameCondition.Defeat)
      {
        MainMenu.Visible = true;
        WorldUI.Visible = false;
      }
    }
  }
}
