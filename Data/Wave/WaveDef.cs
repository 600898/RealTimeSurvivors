namespace RealTimeSurvivors.Data.Wave
{
  /// <summary>
  /// Defines a single full wave, using multiple UnitGroups.
  /// </summary>
  public struct WaveDef
  {
    public UnitGroup[] groups;

    public WaveDef(UnitGroup[] groups)
    {
      this.groups = groups;
    }
  }
}
