namespace RealTimeSurvivors.Components.Shared
{
  /// <summary>
  /// Component that keeps track of an entities health.
  /// All entities have a Health component.
  /// </summary>
  public struct Health
  {
    public float Value;
    public float Current;

    public Health(int health)
    {
      Value = health;
      Current = health;
    }
  }
}
