using Godot;
using RealTimeSurvivors.Components.Shared;
using RealTimeSurvivors.Systems.Buildings;
using RealTimeSurvivors.Systems.Shared;
namespace RealTimeSurvivors.Systems.Godot
{
  /// <summary>
  /// Handles recieved input from Godot, and process them for other systems.
  /// Example: Camera movement, Selection cursor box, Building Mode and Interface buttons.
  /// </summary>
  public class InputManager : Node
  {
    private readonly Camera2D _camera;
    private readonly NinePatchRect _selectionBox;
    private readonly WorldManager _world;
    private readonly BuildManager _build;
    private readonly InterfaceManager _interface;

    private bool _isDragging = false;
    private Vector2 _dragStart;
    private Vector2 _dragEnd;

    public InputManager(Camera2D _camera, NinePatchRect _selectionBox, WorldManager _world)
    {
      this._camera = _camera;
      this._selectionBox = _selectionBox;
      this._world = _world;
      _build = _world._building;
      _interface = _world._interface;
      Name = "InputManager";
    }

    // Unhandled input gives priority to Control node clicks, before being handled here.
    public override void _UnhandledInput(InputEvent @event)
    {
      if (@event is InputEventMouseButton mouseEvent)
      {
        HandleMouseButton(mouseEvent);
      }

      if (_build._buildMode == ModeTypes.Select && @event is InputEventMouseMotion motion && _isDragging)
      {
        HandleDrag(motion);
      }

      HandleBuildInput(@event);
    }

    public override void _Process(float delta)
    {
      HandleCameraInput(delta);
      HandleBuildProcess();
    }

    // Handles all mouse input
    private void HandleMouseButton(InputEventMouseButton mouse)
    {
      switch (_build._buildMode)
      {
        case ModeTypes.Build:
          if (mouse.ButtonIndex == (int)ButtonList.Left && mouse.IsReleased())
          {
            _build.TryPlaceBuilding();
          }

          if (mouse.ButtonIndex == (int)ButtonList.Right)
          {
            _build.SetMode(ModeTypes.Select);
            _interface.SetMode(ModeTypes.Select);
            _interface.ToggleBuildHudElement(_build._buildMode == ModeTypes.Build ? true : false);
          }

          return;

        case ModeTypes.Destroy:
          if (mouse.ButtonIndex == (int)ButtonList.Left && mouse.IsReleased())
          {
            _build.TryDestroyBuilding(_camera.GetGlobalMousePosition());
          }

          if (mouse.ButtonIndex == (int)ButtonList.Right && mouse.IsPressed())
          {
            _build.SetMode(ModeTypes.Select);
            _interface.SetMode(ModeTypes.Select);
            _interface.ToggleBuildHudElement(_build._buildMode == ModeTypes.Build ? true : false);
          }

          return;
      }

      HandleSelectionAndMovement(mouse);
    }

    // Handles the Selection and Click-To-Move function for Unit Entity's
    private void HandleSelectionAndMovement(InputEventMouseButton mouse)
    {
      if (mouse.ButtonIndex == (int)ButtonList.Left)
      {
        if (mouse.Pressed)
        {
          _dragStart = mouse.Position;
          _dragEnd = _dragStart;
          _isDragging = true;
          _selectionBox.Visible = true;
          _selectionBox.RectSize = Vector2.Zero;
          _selectionBox.RectPosition = _dragStart;
        }
        else
        {
          _isDragging = false;
          _dragEnd = mouse.Position;
          _selectionBox.Visible = false;
          _world.HandleSelection(_dragStart, _dragEnd, _camera);
        }
      }

      if (mouse.ButtonIndex == (int)ButtonList.Right && mouse.Pressed)
      {
        Vector2 worldPos = _camera.GetGlobalMousePosition();
        _world.IssueMoveCommand(new Position(worldPos.x, worldPos.y));
      }
    }

    // Handles the full Selection box input.
    private void HandleDrag(InputEventMouseMotion motion)
    {
      _dragEnd = motion.Position;
      UpdateSelectionBox();
    }

    // Handles the Camera movement
    private void HandleCameraInput(float delta)
    {
      Vector2 input = Vector2.Zero;
      float cameraSpeed = 400f;

      if(Input.IsKeyPressed((int)KeyList.Shift))
      {
        cameraSpeed = 800f;
      }

      if (Input.IsActionPressed("Cam_UP") && _camera.Position.y >= 0) { input.y -= 1; }
      if (Input.IsActionPressed("Cam_DOWN") && _camera.Position.y <= 3200) { input.y += 1; }
      if (Input.IsActionPressed("Cam_RIGHT") && _camera.Position.x <= 3200) { input.x += 1; }
      if (Input.IsActionPressed("Cam_LEFT") && _camera.Position.x >= 0) { input.x -= 1; }

      if (input != Vector2.Zero)
      {
        _camera.Position += input.Normalized() * cameraSpeed * delta;
      }
    }

    // Handles the Selection box visual in the interface.
    private void UpdateSelectionBox()
    {
      _selectionBox.Visible = true;

      _selectionBox.RectPosition = new Vector2(
          Mathf.Min(_dragStart.x, _dragEnd.x),
          Mathf.Min(_dragStart.y, _dragEnd.y)
      );

      _selectionBox.RectSize = new Vector2(
          Mathf.Abs(_dragEnd.x - _dragStart.x),
          Mathf.Abs(_dragEnd.y - _dragStart.y)
      );
    }

    // Handles buttons that affect the BuildManager.
    private void HandleBuildProcess()
    {
      if(Input.IsActionJustPressed("BuildMode_Toggle")) 
      {
        _build.SetMode(
            _build._buildMode == ModeTypes.Build
                ? ModeTypes.Select
                : ModeTypes.Build
        );

        _isDragging = false;
        _selectionBox.Visible = false;

        _interface.SetMode(_build._buildMode);
        _interface.ToggleBuildHudElement(_build._buildMode == ModeTypes.Build ? true : false);
      }
      else if (Input.IsActionJustPressed("DestroyMode"))
      {
        _build.SetMode(
          _build._buildMode == ModeTypes.Destroy
              ? ModeTypes.Select
              : ModeTypes.Destroy
        );

        _interface.SetMode(_build._buildMode);
        _interface.ToggleBuildHudElement(_build._buildMode == ModeTypes.Build ? true : false);
      }
      _build.UpdatePlacement(_camera.GetGlobalMousePosition());
    }

    // Handles what happens when you press dedicated build buttons on the interface and keys (number 1-9,0).
    private void HandleBuildInput(InputEvent e)
    {
      int numberPressed = -1;

      if (e is InputEventKey ke)
      {
        if(ke.Pressed)
        {
          switch (ke.Scancode)
          {
            case (uint)KeyList.Key1:
              numberPressed = 1;
            break;
            case (uint)KeyList.Key2:
              numberPressed = 2;
            break;
            case (uint)KeyList.Key3:
              numberPressed = 3;
            break;
            case (uint)KeyList.Key4:
              numberPressed = 4;
            break;
            case (uint)KeyList.Key5:
              numberPressed = 5;
            break;
            case (uint)KeyList.Key6:
              numberPressed = 6;
            break;
            case (uint)KeyList.Key7:
              numberPressed = 7;
            break;
            case (uint)KeyList.Key8:
              numberPressed = 8;
            break;
            case (uint)KeyList.Key9:
              numberPressed = 9;
            break;
            case (uint)KeyList.Key0:
              numberPressed = 0;
            break;
          }
        }
      }

      if (numberPressed == -1) { return; }
      if (_build._buildMode == ModeTypes.Build) {_build.ChangeBuilding(numberPressed); }
      else { _build.TryUseBuilding(numberPressed); }
    }
  }

}
