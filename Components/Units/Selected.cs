namespace RealTimeSurvivors.Components.Units
{
  /// <summary>
  /// Component for tracking if a Unit is selectable, but also if a Unit is Selected during "Click-To-Move" input.
  /// </summary>
  public struct Selected
  {
    public bool Value;

    public Selected(bool selected = false)
    {
      Value = selected;
    }
  }
}
