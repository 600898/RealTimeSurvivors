namespace RealTimeSurvivors.Components.Shared
{
  /// <summary>
  /// Component for representing a building entity's size.
  /// Used in the Grid system to cover the correct amount of tiles for a building.
  /// </summary>
  public struct Size
  {
    public int w;
    public int h;
    public Size(int w, int h)
    {
      this.w = w;
      this.h = h;
    }
  }
}
