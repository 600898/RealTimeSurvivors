namespace RealTimeSurvivors.Components.Buildings
{
  /// <summary>
  /// Component for buildings that generate resources.
  /// Example: BuildingTypes.Generator
  /// </summary>
  public struct Generator
  {
    public float TotalResources;
    public float ResourcePerGoal;
    public float DeltaGoal;
    public float CurrentDelta;
  }
}