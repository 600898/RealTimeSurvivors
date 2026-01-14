using RealTimeSurvivors.Components.Units;

namespace RealTimeSurvivors.Components.Buildings
{
  /// <summary>
  /// Data for buildings that produce units.
  /// Example: BuildingTypes.Forge
  /// </summary>
  public struct Production
  {
    public UnitType[] AvailableUnits;

    public UnitType[] Queue;
    public int QueueSize;

    public float BuildTime;
    public float Progress;

    public bool IsBuilding;

    public Production(UnitType[] available, int maxQueue = 5, float buildTime = 5f)
    {
      AvailableUnits = available;
      Queue = new UnitType[maxQueue];
      QueueSize = 0;
      BuildTime = buildTime;
      Progress = 0;
      IsBuilding = false;
    }
  }
}