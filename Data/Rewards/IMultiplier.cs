namespace RealTimeSurvivors.Data.Rewards
{
  /// <summary>
  /// "Generic" interface for having different multiplier varients.
  /// </summary>
  public interface IMultiplier
  {
    ModifierType Type { get; }
    float Apply(float value);
  }
}
