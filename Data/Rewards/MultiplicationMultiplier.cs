namespace RealTimeSurvivors.Data.Rewards
{
  /// <summary>
  /// Multiplies the modifier value with the given GameStorage value.
  /// </summary>
  public struct MultiplicationMultiplier : IMultiplier
  {
    public ModifierType Type;
    public float Value;

    ModifierType IMultiplier.Type => Type;

    public MultiplicationMultiplier(ModifierType type, float value)
    {
      Type = type;
      Value = value;
    }

    public float Apply(float value)
    {
      return value * Value;
    }

  }
}
