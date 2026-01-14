namespace RealTimeSurvivors.Data.Rewards
{
  /// <summary>
  /// Adds the modifier value to the given GameStorage value.
  /// </summary>
  public struct AdditionMultiplier : IMultiplier
  {
    public ModifierType Type;
    public float Value;

    ModifierType IMultiplier.Type => Type;

    public AdditionMultiplier(ModifierType type, float value)
    {
      Type = type;
      Value = value;
    }

    public float Apply(float value)
    {
      return value + Value;
    }
  }
}
