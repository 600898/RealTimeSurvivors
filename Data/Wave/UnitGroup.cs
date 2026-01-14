using RealTimeSurvivors.Components.Units;
namespace RealTimeSurvivors.Data.Wave
{
  /// <summary>
  /// Represents a single unit type and the amount to be spawned during a wave.
  /// </summary>
  public struct UnitGroup
  {
    public UnitType type;
    public int amount;

    public UnitGroup(UnitType type, int amount)
    {
      this.type = type;
      this.amount = amount;
    }
  }
}
