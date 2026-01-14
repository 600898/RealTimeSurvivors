namespace RealTimeSurvivors.Data
{
  /// <summary>
  /// Stores game data to be read by other Systems/Managers.
  /// Resources, modifiers to components, etc...
  /// </summary>
  public class GameStorage
  {
    public float Resources = 5000f;

    // Friendly modifiers
    public float fDamage = 1.0f;
    public float fSpeed = 1.0f;
    public float buildTime = 1.0f;
    public float generateAmount = 1.0f; // Modifier for buildings generating resources

    // Enemy modifiers
    public float eDamage = 1.0f;
    public float eSpeed = 1.0f;
  }
}