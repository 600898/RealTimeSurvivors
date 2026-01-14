using Godot;
using RealTimeSurvivors.Components.Buildings;
using RealTimeSurvivors.Components.Units;
using RealTimeSurvivors.Data;
using RealTimeSurvivors.Data.Entities;
using RealTimeSurvivors.Data.Rewards;
using RealTimeSurvivors.Systems.Buildings;
using RealTimeSurvivors.Systems.Shared;
using System;
namespace RealTimeSurvivors.Systems.Godot
{
  /// <summary>
  /// Handles all Godot Control Interface Nodes and their interaction.
  /// </summary>
  public class InterfaceManager : Node
  {
    private CanvasLayer _interface;
    private TextureButton _generatorBuildBtn;
    private TextureButton _towerBuildBtn;
    private TextureButton _wallBuildBtn;
    private TextureButton _forgeBuildBtn;
    private TextureButton[] _unitBtns;
    private TextureButton[] _cardBtns;
    private Label _selectedType; 
    private Label _resourceAmount;
    private Label _mode;
    private Label _waveCounter;
    private Label _waveCountdown;

    private GameStorage _storage;
    private Reward[] rewards;

    public InterfaceManager(CanvasLayer _interface, WorldManager _world, GameStorage _storage)
    {
      this._interface = _interface;
      this._storage = _storage;
      _generatorBuildBtn = _interface.GetNode<TextureButton>("WorldUI/BottomLeft/HBoxContainer/Key1");
      _towerBuildBtn = _interface.GetNode<TextureButton>("WorldUI/BottomLeft/HBoxContainer/Key2");
      _wallBuildBtn = _interface.GetNode<TextureButton>("WorldUI/BottomLeft/HBoxContainer/Key3");
      _forgeBuildBtn = _interface.GetNode<TextureButton>("WorldUI/BottomLeft/HBoxContainer/Key4");
      _unitBtns = new[] 
      {
        _interface.GetNode<TextureButton>("WorldUI/BottomRight/GridContainer/Unit_Key1"),
        _interface.GetNode<TextureButton>("WorldUI/BottomRight/GridContainer/Unit_Key2"),
        _interface.GetNode<TextureButton>("WorldUI/BottomRight/GridContainer/Unit_Key3"),
        _interface.GetNode<TextureButton>("WorldUI/BottomRight/GridContainer/Unit_Key4"),
        _interface.GetNode<TextureButton>("WorldUI/BottomRight/GridContainer/Unit_Key5"),
        _interface.GetNode<TextureButton>("WorldUI/BottomRight/GridContainer/Unit_Key6"),
        _interface.GetNode<TextureButton>("WorldUI/BottomRight/GridContainer/Unit_Key7"),
        _interface.GetNode<TextureButton>("WorldUI/BottomRight/GridContainer/Unit_Key8"),
        _interface.GetNode<TextureButton>("WorldUI/BottomRight/GridContainer/Unit_Key9")
      };
      _cardBtns = new[] 
      { 
        _interface.GetNode<TextureButton>("WorldUI/CardRewards/CenterContainer/HBoxContainer/Card1"), 
        _interface.GetNode<TextureButton>("WorldUI/CardRewards/CenterContainer/HBoxContainer/Card2"), 
        _interface.GetNode<TextureButton>("WorldUI/CardRewards/CenterContainer/HBoxContainer/Card3") 
      };
      _selectedType = _interface.GetNode<Label>("WorldUI/BottomRight/Label_Selected");
      _resourceAmount = _interface.GetNode<Label>("WorldUI/BottomRight/Label_Resources");
      _mode = _interface.GetNode<Label>("WorldUI/BottomLeft/Label_Mode");
      _waveCounter = _interface.GetNode<Label>("WorldUI/TopLeft/Label_WaveCounter");
      _waveCountdown = _interface.GetNode<Label>("WorldUI/TopLeft/Label_WaveCountdown");

      _generatorBuildBtn.HintTooltip ="Generator: Gathers resources. \n -,200r";
      _towerBuildBtn.HintTooltip = "Tower: Acts as a stationary ranged weapon. \n -,750r";
      _wallBuildBtn.HintTooltip = "Wall: High health structure for blocking enemies. \n -,100r";
      _forgeBuildBtn.HintTooltip = "Forge: Allows you to create units. \n -,500r";

      _generatorBuildBtn.Connect("pressed", this, nameof(OnPressKey1));
      _towerBuildBtn.Connect("pressed", this, nameof(OnPressKey2));
      _wallBuildBtn.Connect("pressed", this, nameof(OnPressKey3));
      _forgeBuildBtn.Connect("pressed", this, nameof(OnPressKey4));
      for(int i =0; i < Math.Min(_unitBtns.Length, 9); i++)
      {
        _unitBtns[i].Connect("pressed", this, $"OnPressKey{i+1}");
      }
      for(int i = 0; i < 3; i++)
      {
        _cardBtns[i].Connect("pressed", this, $"OnPressCard{i+1}");
      }
    }

    // Godot 3.6 does not have lambda expressions for assigning functions to buttons.
    private void OnPressKey1() { SendKeyPress(KeyList.Key1); }
    private void OnPressKey2() { SendKeyPress(KeyList.Key2); }
    private void OnPressKey3() { SendKeyPress(KeyList.Key3); }
    private void OnPressKey4() { SendKeyPress(KeyList.Key4); }
    private void OnPressKey5() { SendKeyPress(KeyList.Key5); }
    private void OnPressKey6() { SendKeyPress(KeyList.Key6); }
    private void OnPressKey7() { SendKeyPress(KeyList.Key7); }
    private void OnPressKey8() { SendKeyPress(KeyList.Key8); }
    private void OnPressKey9() { SendKeyPress(KeyList.Key9); }
    private void OnPressKey0() { SendKeyPress(KeyList.Key0); }

    private void OnPressCard1() { rewards[0].Apply(ref _storage); ToggleRewardHud(); }
    private void OnPressCard2() { rewards[1].Apply(ref _storage); ToggleRewardHud(); }
    private void OnPressCard3() { rewards[2].Apply(ref _storage); ToggleRewardHud(); }

    /// <summary>
    /// Toggles the BuildingTypes buttons on the hud to choose a current building.
    /// </summary>
    /// <param name="visible">Show = true, Hide = false</param>
    public void ToggleBuildHudElement(bool visible)
    {
      HBoxContainer buttonsLeft = _interface.GetNode<HBoxContainer>("WorldUI/BottomLeft/HBoxContainer");
      buttonsLeft.Visible = visible;

      GridContainer buttonsRight = _interface.GetNode<GridContainer>("WorldUI/BottomRight/GridContainer");
      buttonsRight.Visible = !visible;

    }

    /// <summary>
    /// Toggles the Unit button elements on the interface, depending on the size of the passed UnitTypes array.
    /// No Input, or null array, hides all unit buttons.
    /// </summary>
    /// <param name="availableUnits"></param>
    public void ToggleUnitHudElement(UnitType[] availableUnits = null)
    {
      if(availableUnits == null)
      {
        for(int i = 0; i < 9; i++)
        {
          _unitBtns[i].Visible = false;
        }

        return;
      }

      for (int i = 0; i < 9; i++)
      {
        if (i < availableUnits.Length)
        {
          UnitType type = availableUnits[i];
          Unit unit = EntityList.Units[type];
          _unitBtns[i].TextureNormal = GD.Load<Texture>($"{AssetPaths.UnitPath}{type}.png");
          _unitBtns[i].Visible = true;
          _unitBtns[i].HintTooltip = type + 
          "\n Damage: " + unit.attack.Damage +
          "\n Range: " + unit.attack.Range +
          "\n -," + unit.cost;
        }
        else
        {
          _unitBtns[i].Visible = false;
        }
      }
    }

    /// <summary>
    /// Toggles the three Reward cards a player can choose on the interface.
    /// </summary>
    public void ToggleRewardHud()
    {
      Control rewardHud = _interface.GetNode<Control>("WorldUI/CardRewards");
      rewardHud.Visible = !rewardHud.Visible;

      if (rewardHud.Visible)
      {
        rewards = RewardSystem.GetRandomRewards(_cardBtns.Length);
        for (int i = 0; i < _cardBtns.Length; i++)
        {
          _cardBtns[i].GetNode<Label>("Label").Text = rewards[i].Description;

        }
      }
    }

    // Fire off Key press input by interface.
    private void SendKeyPress(KeyList key)
    {
      InputEventKey ev = new InputEventKey
      {
        Pressed = true,
        Scancode = (uint)key,
      };
      Input.ParseInputEvent(ev);
      ev = new InputEventKey
      {
        Pressed = false,
        Scancode = (uint)key
      };
      Input.ParseInputEvent(ev);
    }

    /// <summary>
    /// Sets the current selected building on the hud.
    /// </summary>
    /// <param name="entity">Entity id</param>
    /// <param name="_entities">EntityManager</param>
    public void SetSelectedBuilding(int entity, EntityManager _entities)
    {
      if(entity == -1)
      {
        _selectedType.Text = "Selected Building:";
        ToggleUnitHudElement();
        return;
      }

      BuildingType type = (BuildingType)_entities.Types[entity];
      _selectedType.Text = "Selected Building: " + type;

      if(type == BuildingType.Forge)
      {
        Production fp = _entities.Productions[entity];
        ToggleUnitHudElement(fp.AvailableUnits);
        return;
      }

      ToggleUnitHudElement();
    }

    /// <summary>
    /// Sets the current resource amount from the GameStorage on the hud.
    /// </summary>
    /// <param name="amount"></param>
    public void SetResourceAmount(int amount)
    {
      string amountTxt = amount.ToString();
      string result = "";
      int count = 0;

      for(int i = amountTxt.Length-1;  i >= 0; i--)
      {
        result = amountTxt[i] + result;
        count++;

        if(count == 3 && i != 0)
        {
          result = "'" + result;
          count = 0;
        }
      }
      _resourceAmount.Text = "Resources: " + result;
    }

    /// <summary>
    /// Sets a visual indicator on the hud for what BuildMode you are in.
    /// </summary>
    /// <param name="type"></param>
    public void SetMode(ModeTypes type)
    {
      _mode.Text = "Mode: " + type;
    }

    /// <summary>
    /// Set the current Wave and next Wave timer on the hud.
    /// </summary>
    /// <param name="waveCounter"></param>
    /// <param name="waveCountdown"></param>
    public void SetWave(int waveCounter, int waveCountdown)
    {
      _waveCounter.Text = $"Current Wave {waveCounter.ToString()}";
      _waveCountdown.Text = $"Next wave in: {TimeSpan.FromSeconds(waveCountdown).ToString()}";
    }

    /// <summary>
    /// Shows the Game end screen depending on the games end condition (Victory or Defeat).
    /// </summary>
    /// <param name="condition"></param>
    public void ToggleGameEndHud(GameCondition condition)
    {
      if(condition == GameCondition.Running)
      {
        _interface.GetNode<Control>("WorldUI/GameEnd").Visible = false;
        return;
      }

      Label endText = _interface.GetNode<Label>("WorldUI/GameEnd/CenterContainer/Label");
      Panel endBG = _interface.GetNode<Panel>("WorldUI/GameEnd/Panel");

      if(condition == GameCondition.Victory)
      {
        endText.Text = $"VICTORY \nAll Waves have been defeated.";
        endText.AddColorOverride("Font Color", new Color("005eff"));
        endBG.Modulate = new Color("2eebff");
      }
      else
      {
        endText.Text = $"DEFEAT \nThe heart was destroyed.";
        endText.AddColorOverride("Font Color", new Color("ff194f"));
        endBG.Modulate = new Color("ff2e7c");
      }

      _interface.GetNode<Control>("WorldUI/GameEnd").Visible = true;
    }

  }
}
