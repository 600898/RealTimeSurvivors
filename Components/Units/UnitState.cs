namespace RealTimeSurvivors.Components.Units
{
  /// <summary>
  /// State for how a Unit/Entity should act at the current process cycle inside UnitStatSystem.
  /// </summary>
  public enum UnitState
  {
    None,
    Idle,
    Chasing,
    Moving,
    Attacking
  }
}